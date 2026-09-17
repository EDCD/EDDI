using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class BuildSecretsBuildTests
    {
        [TestMethod]
        public async Task BuildGeneratesLiteralKeyAndRemovesStaleSigningSource ()
        {
            var directory = Path.Combine( Path.GetTempPath(), "eddi-signing-build-" + Guid.NewGuid().ToString( "N" ) );
            Directory.CreateDirectory( directory );
            try
            {
                foreach ( var file in Directory.GetFiles( Path.Combine( AppContext.BaseDirectory, "BuildSecretsFixture" ) ) )
                {
                    File.Copy( file, Path.Combine( directory, Path.GetFileName( file ) ) );
                }
                File.WriteAllText( Path.Combine( directory, "Directory.Build.props" ),
                    "<Project><PropertyGroup><TargetFramework>net8.0</TargetFramework><DebugType>embedded</DebugType><DebugSymbols>true</DebugSymbols></PropertyGroup></Project>" );
                File.WriteAllText( Path.Combine( directory, "NuGet.Config" ),
                    "<configuration><packageSources><clear /></packageSources></configuration>" );
                File.WriteAllText( Path.Combine( directory, ".gitignore" ), "BuildInjectedSecrets.local.cs\nbin/\nobj/\n" );
                await Run( directory, "git", "init", "--quiet" );
                await Run( directory, "git", "add", "." );
                await Run( directory, "git", "-c", "user.name=EDDI Tests", "-c", "user.email=tests@example.invalid",
                    "-c", "commit.gpgsign=false", "commit", "--quiet", "-m", "Disposable build fixture" );
                await Build( directory );
                VerifySignature( directory, null );

                var keyFile = Path.Combine( directory, "BuildInjectedSecrets.local.cs" );
                // Looks like Base64, but must be treated as literal text.
                const string firstKey = "ZGlzcG9zYWJsZS10ZXN0LWtleQ==";
                File.WriteAllText( keyFile, LocalSource( firstKey ), new UTF8Encoding( true ) );
                await Build( directory );
                VerifySignature( directory, firstKey );
                var generated = Directory.GetFiles( Path.Combine( directory, "obj" ), "EddnSigning.g.cs", SearchOption.AllDirectories ).Single();
                Assert.DoesNotContain( firstKey, File.ReadAllText( generated ));

                const string secondKey = " clé-\"disposable\"-星 ";
                File.WriteAllText( keyFile, LocalSource( secondKey ) );
                await Build( directory );
                VerifySignature( directory, secondKey );

                await Build( directory, "BuildSecrets must use", "-p:DebugType=embedded" );
                await Build( directory, "BuildSecrets must use", "-p:DebugSymbols=true" );

                foreach ( var invalid in new[]
                {
                    (LocalSource( "" ), "EddnSigningKeyInput is empty"),
                    (LocalSource( firstKey ) + LocalSource( firstKey ), "Invalid EDDN build input"),
                    (LocalSource( firstKey ).Replace( "#if false", "#if true" ), "Invalid EDDN build input"),
                    (LocalSource( firstKey ).Replace( "#if false", "" ).Replace( "#endif", "" ), "Invalid EDDN build input"),
                    (LocalSource( firstKey ).Replace( "@\"", "\"" ), "Invalid EDDN build input"),
                    ("// EDDN_SIGNING_KEY: " + firstKey, "Migrate the obsolete EDDN key comment")
                } )
                {
                    File.WriteAllText( keyFile, invalid.Item1 );
                    await Build( directory, invalid.Item2 );
                }
                File.WriteAllText( keyFile, "// Other local credentials can remain without EDDN signing." );
                await Build( directory );
                VerifySignature( directory, null );
                Assert.IsFalse( File.Exists( generated ), "Removing the key must remove stale generated source." );
                File.Delete( keyFile );
                await Build( directory );
                VerifySignature( directory, null );
            }
            finally
            {
                // Git object files are read-only on Windows. Only touch this disposable fixture.
                foreach ( var file in Directory.GetFiles( directory, "*", SearchOption.AllDirectories ) )
                {
                    File.SetAttributes( file, FileAttributes.Normal );
                }
                Directory.Delete( directory, recursive: true );
            }
        }

        private static string LocalSource ( string key ) =>
            "namespace BuildSecrets { public static partial class BuildInjectedSecrets {\n#if false\n" +
            "private const string EddnSigningKeyInput = @\"" + key.Replace( "\"", "\"\"" ) + "\";\n#endif\n} }\n";

        private static async Task Build ( string directory, string expectedError = null, params string[] overrides )
        {
            var arguments = new[] { "build", "BuildSecrets.csproj", "--configuration", "Release", "--verbosity", "quiet",
                "-p:RestoreConfigFile=" + Path.Combine( directory, "NuGet.Config" ) }.Concat( overrides ).ToArray();
            var (exitCode, output) = await Run( directory, "dotnet", arguments );
            Assert.AreEqual( expectedError == null, exitCode == 0, output );
            if ( expectedError != null ) { Assert.Contains( expectedError , output); }
            else
            {
                using var stream = File.OpenRead( Path.Combine( directory, "bin", "Release", "net8.0", "BuildSecrets.dll" ) );
                using var reader = new PEReader( stream );
                Assert.IsFalse( reader.ReadDebugDirectory().Any( entry => entry.Type is
                    DebugDirectoryEntryType.EmbeddedPortablePdb or DebugDirectoryEntryType.CodeView ) );
                Assert.IsEmpty( Directory.GetFiles( directory, "*.pdb", SearchOption.AllDirectories ));
            }
        }

        private static async Task<(int ExitCode, string Output)> Run ( string directory, string command, params string[] arguments )
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo( command )
                {
                    WorkingDirectory = directory,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            foreach ( var argument in arguments )
            {
                process.StartInfo.ArgumentList.Add( argument );
            }
            process.Start();
            var stdout = process.StandardOutput.ReadToEndAsync();
            var stderr = process.StandardError.ReadToEndAsync();
            using var timeout = new System.Threading.CancellationTokenSource( TimeSpan.FromMinutes( 2 ) );
            try { await process.WaitForExitAsync( timeout.Token ); }
            catch ( OperationCanceledException ) { process.Kill( entireProcessTree: true ); throw; }
            var output = await stdout + await stderr;
            if ( command == "git" ) { Assert.AreEqual( 0, process.ExitCode, output ); }
            return (process.ExitCode, output);
        }

        private static void VerifySignature ( string directory, string key )
        {
            var context = new AssemblyLoadContext( "Disposable BuildSecrets", isCollectible: true );
            try
            {
                using var stream = File.OpenRead( Path.Combine( directory, "bin", "Release", "net8.0", "BuildSecrets.dll" ) );
                var assembly = context.LoadFromStream( stream );
                var method = assembly.GetType( "BuildSecrets.BuildInjectedSecrets" )!
                    .GetMethod( "TrySignEddnPayload", BindingFlags.Public | BindingFlags.Static );
                var payload = Encoding.UTF8.GetBytes( "Étoile 星" );
                object[] arguments = [payload, null];
                Assert.AreEqual( key != null, (bool)method!.Invoke( null, arguments )! );
                var expected = key == null ? string.Empty : Convert.ToHexString(
                    HMACSHA256.HashData( Encoding.UTF8.GetBytes( key ), payload ) ).ToLowerInvariant();
                Assert.AreEqual( expected, arguments[1] );
            }
            finally { context.Unload(); }
        }
    }
}

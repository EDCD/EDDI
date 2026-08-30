#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public sealed class InstallerScriptTests
    {
        private const string UninstallSubkey = "Software\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{830C0324-30D8-423C-B5B4-D7EE8D007A79}_is1";

        [TestMethod]
        public void InstallerCleanup_ChecksPreviousInstallLocationsInBothRegistryHives ()
        {
            var script = ReadInstallerScript();

            Assert.Contains( "TryGetPreviousEddiInstallDirFromRoot(HKLM" , script);
            Assert.Contains( "TryGetPreviousEddiInstallDirFromRoot(HKCU" , script);
            Assert.Contains( "RemoveDirIfOldEddiInstall(PreviousDir)" , script);
        }

        [TestMethod]
        public void InstallerCleanup_RemovesOnlyMatchingStaleUninstallEntriesAfterLegacyDirectoryRemoval ()
        {
            var script = ReadInstallerScript();

            Assert.Contains( UninstallSubkey , script);
            Assert.Contains( "RegQueryStringValue(RootKey, UninstallSubkey, 'InstallLocation', ExistingInstallLocation)" , script);
            Assert.Contains( "SamePath(ExistingInstallLocation, RemovedDir)" , script);
            Assert.Contains( "Result := RegDeleteKeyIncludingSubkeys(RootKey, UninstallSubkey)" , script);
            AssertTextOccursAfter(
                script,
                "Result := DelTree(CleanDir, True, True, True);",
                "Result := RemoveStaleUninstallEntriesIfMatches(CleanDir);" );
        }

        [TestMethod]
        public void InstallerCleanup_PropagatesStaleUninstallEntryRemovalFailure ()
        {
            var script = ReadInstallerScript();

            Assert.Contains( "function RemoveStaleUninstallEntryIfMatches(RootKey: Integer; const RemovedDir: string): Boolean" , script);
            Assert.Contains( "function RemoveStaleUninstallEntriesIfMatches(const RemovedDir: string): Boolean" , script);
            Assert.Contains( "if not RemoveStaleUninstallEntryIfMatches(HKLM, RemovedDir) then" , script);
            Assert.Contains( "if not RemoveStaleUninstallEntryIfMatches(HKCU, RemovedDir) then" , script);
        }

        [TestMethod]
        public void InstallerCleanup_PreservesCurrentApplicationDirectory ()
        {
            var script = ReadInstallerScript();
            var cleanupFunction = NormalizeLineEndings( GetFunctionBlock( script, "function RemoveDirIfOldEddiInstall" ) );

            Assert.Contains(
                "if SamePath(CleanDir, ExpandConstant('{app}')) then\n    exit;" ,
                cleanupFunction);
            AssertTextOccursAfter(
                cleanupFunction,
                "if SamePath(CleanDir, ExpandConstant('{app}')) then\n    exit;",
                "Result := DelTree(CleanDir, True, True, True);" );
        }

        [TestMethod]
        public void CurrentUninstall_DoesNotDeleteCompanionApiCredentials ()
        {
            var script = ReadInstallerScript();
            var uninstallDeleteIndex = script.IndexOf( "[UninstallDelete]", StringComparison.Ordinal );
            var nextSectionIndex = script.IndexOf( "[Icons]", uninstallDeleteIndex, StringComparison.Ordinal );
            var uninstallDeleteSection = script.Substring( uninstallDeleteIndex, nextSectionIndex - uninstallDeleteIndex );

            Assert.IsFalse( uninstallDeleteSection.Contains( "CompanionAPI.json", StringComparison.Ordinal ) );
        }

        [TestMethod]
        public void InstallerPrerequisite_TargetsDesktopRuntimeMajorFromBuildTarget ()
        {
            var script = ReadInstallerScript();
            var props = ReadRepositoryFile( "Directory.Build.props" );
            var targetFramework = MatchRequired( props, @"<TargetFramework>net(?<major>\d+)\.0-windows" ).Groups["major"].Value;
            var installerRuntime = MatchRequired( script, @"#define DotNetDesktopRuntimeMajor ""(?<major>\d+)""" ).Groups["major"].Value;

            Assert.AreEqual( targetFramework, installerRuntime );
            Assert.Contains(
                $"#define DotNetDesktopRuntimeInstallerUrl \"https://aka.ms/dotnet/{targetFramework}.0/windowsdesktop-runtime-win-x64.exe\"" ,
                script);
        }

        [TestMethod]
        public void InstallerPrerequisite_InstallsDesktopRuntimeFromOfficialSourcesBeforeContinuing ()
        {
            var script = ReadInstallerScript();
            var detectorFunction = NormalizeLineEndings( GetFunctionBlock( script, "function IsRequiredDotNetDesktopRuntimeInstalled" ) );
            var registryFunction = NormalizeLineEndings( GetFunctionBlock( script, "function IsRequiredDotNetDesktopRuntimeRegisteredAt" ) );
            var directoryFunction = NormalizeLineEndings( GetFunctionBlock( script, "function IsRequiredDotNetDesktopRuntimeInDirectory" ) );
            var prerequisiteFunction = NormalizeLineEndings( GetFunctionBlock( script, "function EnsureRequiredDotNetDesktopRuntimeInstalled" ) );
            var installFunction = NormalizeLineEndings( GetFunctionBlock( script, "function WriteDotNetDesktopRuntimeInstallerScript" ) );
            var launcherFunction = NormalizeLineEndings( GetFunctionBlock( script, "function InstallRequiredDotNetDesktopRuntime" ) );
            var runAfterUpgradeFunction = NormalizeLineEndings( GetFunctionBlock( script, "function ShouldRunAfterInAppUpgrade" ) );
            var prepareFunction = NormalizeLineEndings( GetFunctionBlock( script, "function PrepareToInstall" ) );

            Assert.Contains( "RegGetValueNames(RootKey, RuntimeKey, ValueNames)", registryFunction );
            Assert.Contains( "RegGetSubkeyNames(RootKey, RuntimeKey, SubkeyNames)", registryFunction );
            Assert.Contains( "IsRequiredDotNetDesktopRuntimeVersionName(ValueNames[I])", registryFunction );
            Assert.Contains( "IsRequiredDotNetDesktopRuntimeVersionName(SubkeyNames[I])", registryFunction );
            Assert.Contains( "FindFirst(AddBackslash(RuntimeDir) + '{#DotNetDesktopRuntimeMajor}.*', FindRec)", directoryFunction );
            Assert.Contains( "HKLM64", detectorFunction );
            Assert.Contains( "HKLM32", detectorFunction );
            Assert.Contains( @"{commonpf64}\dotnet\shared\Microsoft.WindowsDesktop.App", detectorFunction );
            Assert.Contains( "DOTNET_ROOT_X64", detectorFunction );
            Assert.Contains( "DOTNET_ROOT", detectorFunction );
            Assert.Contains( "if IsRequiredDotNetDesktopRuntimeInstalled then\n    exit;" , prerequisiteFunction);
            Assert.Contains( "Invoke-WebRequest -Uri \"{#DotNetDesktopRuntimeInstallerUrl}\"" , installFunction);
            Assert.Contains( "function Test-RuntimeKey($key)", installFunction );
            Assert.Contains( "$item = Get-ItemProperty $key", installFunction );
            Assert.Contains( "Get-Member -MemberType NoteProperty", installFunction );
            Assert.Contains( "Get-ChildItem $key", installFunction );
            Assert.Contains( "$runtimeDirs = @(", installFunction );
            Assert.Contains( "Get-ChildItem $runtimeDir -Directory", installFunction );
            Assert.Contains( "Get-AuthenticodeSignature -FilePath $installer" , installFunction);
            Assert.Contains( "Microsoft Corporation" , installFunction);
            Assert.Contains( "Start-Process -FilePath $installer -ArgumentList \"/install\", \"/quiet\", \"/norestart\" -Wait -PassThru" , installFunction);
            Assert.Contains( "ShellExec(\n        'runas'," , launcherFunction);
            Assert.Contains( "DotNetDesktopRuntimeNeedsRestart := ResultCode = 3010;" , prerequisiteFunction);
            Assert.Contains( "NeedsRestart := True;" , prerequisiteFunction);
            Assert.Contains( "Result := (ResultCode = 0) or ((ResultCode = 3010) and IsRequiredDotNetDesktopRuntimeInstalled);" , prerequisiteFunction);
            Assert.Contains( "and not DotNetDesktopRuntimeNeedsRestart" , runAfterUpgradeFunction);
            Assert.Contains( "if not EnsureRequiredDotNetDesktopRuntimeInstalled(NeedsRestart) then" , prepareFunction);
            Assert.Contains( "Setup could not install the required .NET Desktop Runtime {#DotNetDesktopRuntimeMajor}." , prepareFunction);
            Assert.IsFalse( installFunction.Contains( "winget", StringComparison.OrdinalIgnoreCase ) );
        }

        private static string ReadInstallerScript ()
        {
            return ReadRepositoryFile( "Installer.iss" );
        }

        private static string ReadRepositoryFile ( string fileName )
        {
            var directory = new DirectoryInfo( AppContext.BaseDirectory );
            while ( directory != null )
            {
                var path = Path.Combine( directory.FullName, fileName );
                if ( File.Exists( path ) )
                {
                    return File.ReadAllText( path );
                }

                directory = directory.Parent;
            }

            Assert.Fail( $"Could not locate {fileName} from the test output directory." );
            return string.Empty;
        }

        private static string NormalizeLineEndings ( string text ) =>
            text.Replace( "\r\n", "\n" );

        private static Match MatchRequired ( string text, string pattern )
        {
            var match = Regex.Match( text, pattern );
            Assert.IsTrue( match.Success, $"Could not find expected pattern: {pattern}" );
            return match;
        }

        private static string GetFunctionBlock ( string script, string functionStart )
        {
            var startIndex = script.IndexOf( functionStart, StringComparison.Ordinal );
            Assert.IsGreaterThanOrEqualTo( 0, startIndex, $"Could not find expected function: {functionStart}" );

            var nextFunctionIndex = FindNextTopLevelDeclaration( script, startIndex + functionStart.Length, "function " );
            var nextProcedureIndex = FindNextTopLevelDeclaration( script, startIndex + functionStart.Length, "procedure " );
            var endIndex = MinPositive( nextFunctionIndex, nextProcedureIndex );

            return endIndex >= 0
                ? script.Substring( startIndex, endIndex - startIndex )
                : script.Substring( startIndex );
        }

        private static int FindNextTopLevelDeclaration ( string script, int startIndex, string declarationStart )
        {
            var match = Regex.Match(
                script.Substring( startIndex ),
                "^" + Regex.Escape( declarationStart ),
                RegexOptions.Multiline );

            return match.Success ? startIndex + match.Index : -1;
        }

        private static int MinPositive ( int first, int second )
        {
            if ( first < 0 )
            {
                return second;
            }

            if ( second < 0 )
            {
                return first;
            }

            return Math.Min( first, second );
        }
        private static void AssertTextOccursAfter ( string text, string earlier, string later )
        {
            var earlierIndex = text.IndexOf( earlier, StringComparison.Ordinal );
            var laterIndex = earlierIndex >= 0
                ? text.IndexOf( later, earlierIndex, StringComparison.Ordinal )
                : -1;

            Assert.IsGreaterThanOrEqualTo( 0, earlierIndex, $"Could not find expected text: {earlier}" );
            Assert.IsGreaterThanOrEqualTo( 0, laterIndex, $"Could not find expected text: {later}" );
            Assert.IsGreaterThan( earlierIndex, laterIndex, $"Expected '{later}' to occur after '{earlier}'." );
        }
    }
}

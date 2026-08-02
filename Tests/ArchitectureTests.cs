using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class ArchitectureTests
    {
        [TestMethod]
        public void RespondersAndMonitors_DoNotReferenceEddiEddiUiOrEachOther ()
        {
            var repoRoot = FindRepositoryRoot();
            var projectFiles = Directory.GetFiles( repoRoot, "*.csproj", SearchOption.AllDirectories )
                .Where( path => !path.Contains( $"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase ) )
                .Where( path => !path.Contains( $"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase ) )
                .ToList();

            var projectNamesByFullPath = projectFiles.ToDictionary(
                path => Path.GetFullPath( path ),
                path => Path.GetFileNameWithoutExtension( path ),
                StringComparer.OrdinalIgnoreCase );

            var violations = new List<string>();
            foreach ( var sourceProjectPath in projectFiles.Where( IsResponderOrMonitorProject ) )
            {
                var sourceProjectName = Path.GetFileNameWithoutExtension( sourceProjectPath );
                foreach ( var referencePath in ReadProjectReferences( sourceProjectPath ) )
                {
                    if ( referencePath.IndexOfAny( [ '*', '?' ] ) >= 0 )
                    {
                        violations.Add( $"{sourceProjectName} -> unresolved wildcard reference '{referencePath}'" );
                        continue;
                    }

                    var referencedProjectPath = Path.GetFullPath(
                        Path.Combine( Path.GetDirectoryName( sourceProjectPath ) ?? repoRoot, referencePath ) );

                    if ( !projectNamesByFullPath.TryGetValue( referencedProjectPath, out var referencedProjectName ) )
                    {
                        violations.Add( $"{sourceProjectName} -> unresolved reference '{referencePath}'" );
                        continue;
                    }

                    if ( IsForbiddenReference( referencedProjectName ) )
                    {
                        violations.Add( $"{sourceProjectName} -> {referencedProjectName}" );
                    }
                }
            }

            Assert.IsEmpty( violations, string.Join( Environment.NewLine, violations.OrderBy( v => v ) ) );
        }

        private static IEnumerable<string> ReadProjectReferences ( string projectPath )
        {
            var document = XDocument.Load( projectPath );
            return document
                .Descendants()
                .Where( element => element.Name.LocalName == "ProjectReference" )
                .Select( element => element.Attribute( "Include" )?.Value )
                .Where( include => !string.IsNullOrWhiteSpace( include ) );
        }

        private static bool IsForbiddenReference ( string projectName )
        {
            return projectName is "Eddi" or "EddiUI" || IsResponderOrMonitorProjectName( projectName );
        }

        private static bool IsResponderOrMonitorProject ( string projectPath )
        {
            return IsResponderOrMonitorProjectName( Path.GetFileNameWithoutExtension( projectPath ) );
        }

        private static bool IsResponderOrMonitorProjectName ( string projectName )
        {
            return projectName.EndsWith( "Responder", StringComparison.Ordinal )
                || projectName.EndsWith( "Monitor", StringComparison.Ordinal );
        }

        private static string FindRepositoryRoot ()
        {
            var directory = new DirectoryInfo( AppContext.BaseDirectory );
            while ( directory != null )
            {
                if ( File.Exists( Path.Combine( directory.FullName, "EDDI.sln" ) ) )
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            Assert.Fail( $"Unable to find EDDI.sln from {AppContext.BaseDirectory}." );
            throw new InvalidOperationException();
        }
    }
}

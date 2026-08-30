#nullable enable

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

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

            StringAssert.Contains( script, "TryGetPreviousEddiInstallDirFromRoot(HKLM" );
            StringAssert.Contains( script, "TryGetPreviousEddiInstallDirFromRoot(HKCU" );
            StringAssert.Contains( script, "RemoveDirIfOldEddiInstall(PreviousDir)" );
        }

        [TestMethod]
        public void InstallerCleanup_RemovesOnlyMatchingStaleUninstallEntriesAfterLegacyDirectoryRemoval ()
        {
            var script = ReadInstallerScript();

            StringAssert.Contains( script, UninstallSubkey );
            StringAssert.Contains( script, "RegQueryStringValue(RootKey, UninstallSubkey, 'InstallLocation', ExistingInstallLocation)" );
            StringAssert.Contains( script, "SamePath(ExistingInstallLocation, RemovedDir)" );
            StringAssert.Contains( script, "Result := RegDeleteKeyIncludingSubkeys(RootKey, UninstallSubkey)" );
            AssertTextOccursAfter(
                script,
                "Result := DelTree(CleanDir, True, True, True);",
                "Result := RemoveStaleUninstallEntriesIfMatches(CleanDir);" );
        }

        [TestMethod]
        public void InstallerCleanup_PropagatesStaleUninstallEntryRemovalFailure ()
        {
            var script = ReadInstallerScript();

            StringAssert.Contains( script, "function RemoveStaleUninstallEntryIfMatches(RootKey: Integer; const RemovedDir: string): Boolean" );
            StringAssert.Contains( script, "function RemoveStaleUninstallEntriesIfMatches(const RemovedDir: string): Boolean" );
            StringAssert.Contains( script, "if not RemoveStaleUninstallEntryIfMatches(HKLM, RemovedDir) then" );
            StringAssert.Contains( script, "if not RemoveStaleUninstallEntryIfMatches(HKCU, RemovedDir) then" );
        }

        [TestMethod]
        public void InstallerCleanup_PreservesCurrentApplicationDirectory ()
        {
            var script = ReadInstallerScript();
            var cleanupFunction = NormalizeLineEndings( GetFunctionBlock( script, "function RemoveDirIfOldEddiInstall" ) );

            StringAssert.Contains(
                cleanupFunction,
                "if SamePath(CleanDir, ExpandConstant('{app}')) then\n    exit;" );
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

        private static string ReadInstallerScript ()
        {
            var directory = new DirectoryInfo( AppContext.BaseDirectory );
            while ( directory != null )
            {
                var path = Path.Combine( directory.FullName, "Installer.iss" );
                if ( File.Exists( path ) )
                {
                    return File.ReadAllText( path );
                }

                directory = directory.Parent;
            }

            Assert.Fail( "Could not locate Installer.iss from the test output directory." );
            return string.Empty;
        }

        private static string NormalizeLineEndings ( string text ) =>
            text.Replace( "\r\n", "\n" );
        private static string GetFunctionBlock ( string script, string functionStart )
        {
            var startIndex = script.IndexOf( functionStart, StringComparison.Ordinal );
            Assert.IsTrue( startIndex >= 0, $"Could not find expected function: {functionStart}" );

            var nextFunctionIndex = script.IndexOf( "function ", startIndex + functionStart.Length, StringComparison.Ordinal );
            var nextProcedureIndex = script.IndexOf( "procedure ", startIndex + functionStart.Length, StringComparison.Ordinal );
            var endIndex = MinPositive( nextFunctionIndex, nextProcedureIndex );

            return endIndex >= 0
                ? script.Substring( startIndex, endIndex - startIndex )
                : script.Substring( startIndex );
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

            Assert.IsTrue( earlierIndex >= 0, $"Could not find expected text: {earlier}" );
            Assert.IsTrue( laterIndex >= 0, $"Could not find expected text: {later}" );
            Assert.IsTrue( laterIndex > earlierIndex, $"Expected '{later}' to occur after '{earlier}'." );
        }
    }
}
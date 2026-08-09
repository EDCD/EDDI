#nullable enable

using Eddi;
using EddiVoiceAttackAdapter;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public sealed class EddiInstallLocatorTests
    {
        [TestMethod]
        public void ResolveExecutablePath_CurrentUserRegistryWinsOverLocalMachine ()
        {
            using var paths = new LocatorTestPaths();
            var currentUserExe = paths.CreateExternalExe( "current-user" );
            var localMachineExe = paths.CreateExternalExe( "local-machine" );
            var store = new FakeEddiInstallLocatorStore();
            store.Set( EddiInstallLocatorHive.CurrentUser, EddiInstallLocator.ExecutablePathValueName, currentUserExe );
            store.Set( EddiInstallLocatorHive.LocalMachine, EddiInstallLocator.ExecutablePathValueName, localMachineExe );

            var resolved = EddiInstallLocator.ResolveExecutablePath(
                paths.ShimDirectory,
                store,
                markerFilePath: null,
                baseDirectory: paths.EmptyBaseDirectory );

            Assert.AreEqual( currentUserExe, resolved );
        }

        [TestMethod]
        public void ResolveExecutablePath_MarkerWinsOverStaleRegistryValues ()
        {
            using var paths = new LocatorTestPaths();
            var markerExe = paths.CreateExternalExe( "marker" );
            var currentUserExe = paths.CreateExternalExe( "stale-current-user" );
            var localMachineExe = paths.CreateExternalExe( "stale-local-machine" );
            var markerPath = paths.WriteMarker( markerExe );
            var store = new FakeEddiInstallLocatorStore();
            store.Set( EddiInstallLocatorHive.CurrentUser, EddiInstallLocator.ExecutablePathValueName, currentUserExe );
            store.Set( EddiInstallLocatorHive.LocalMachine, EddiInstallLocator.ExecutablePathValueName, localMachineExe );

            var resolved = EddiInstallLocator.ResolveExecutablePath(
                paths.ShimDirectory,
                store,
                markerPath,
                paths.EmptyBaseDirectory );

            Assert.AreEqual( markerExe, resolved );
        }

        [TestMethod]
        public void ResolveExecutablePath_LocalMachineRegistryWorksWhenCurrentUserMissing ()
        {
            using var paths = new LocatorTestPaths();
            var localMachineExe = paths.CreateExternalExe( "local-machine" );
            var store = new FakeEddiInstallLocatorStore();
            store.Set( EddiInstallLocatorHive.LocalMachine, EddiInstallLocator.ExecutablePathValueName, localMachineExe );

            var resolved = EddiInstallLocator.ResolveExecutablePath(
                paths.ShimDirectory,
                store,
                markerFilePath: null,
                baseDirectory: paths.EmptyBaseDirectory );

            Assert.AreEqual( localMachineExe, resolved );
        }

        [TestMethod]
        public void ResolveVersion_CurrentUserRegistryWinsOverLocalMachine ()
        {
            var store = new FakeEddiInstallLocatorStore();
            store.Set( EddiInstallLocatorHive.CurrentUser, EddiInstallLocator.VersionValueName, "5.0.3" );
            store.Set( EddiInstallLocatorHive.LocalMachine, EddiInstallLocator.VersionValueName, "5.0.2" );

            var version = EddiInstallLocator.ResolveVersion( store );

            Assert.AreEqual( "5.0.3", version );
        }

        [TestMethod]
        public void AdapterVersionProvider_UsesAdapterVisibleVersionBeforeRegistryFallback ()
        {
            using var paths = new LocatorTestPaths();
            var store = new FakeEddiInstallLocatorStore();
            store.Set( EddiInstallLocatorHive.LocalMachine, EddiInstallLocator.VersionValueName, "5.0.2" );

            var version = AdapterVersionProvider.GetDisplayVersion(
                store,
                paths.EmptyBaseDirectory,
                markerFilePath: null,
                baseDirectory: paths.EmptyBaseDirectory );

            Assert.AreEqual( Constants.EDDI_VERSION.ToString(), version );
        }

        [TestMethod]
        public void AdapterVersionProvider_FallsBackToAdapterVisibleVersionWhenLocatorIsUnavailable ()
        {
            using var paths = new LocatorTestPaths();

            var version = AdapterVersionProvider.GetDisplayVersion(
                new FakeEddiInstallLocatorStore(),
                paths.EmptyBaseDirectory,
                markerFilePath: null,
                baseDirectory: paths.EmptyBaseDirectory );

            Assert.AreEqual( Constants.EDDI_VERSION.ToString(), version );
        }

        [TestMethod]
        public void ResolveExecutablePath_MarkerWorksWhenRegistryMissing ()
        {
            using var paths = new LocatorTestPaths();
            var markerExe = paths.CreateExternalExe( "marker" );
            var markerPath = paths.WriteMarker( markerExe );

            var resolved = EddiInstallLocator.ResolveExecutablePath(
                paths.ShimDirectory,
                new FakeEddiInstallLocatorStore(),
                markerPath,
                paths.EmptyBaseDirectory );

            Assert.AreEqual( markerExe, resolved );
        }

        [TestMethod]
        public void ResolveExecutablePath_MarkerWorksWhenRegistryValuesAreInvalid ()
        {
            using var paths = new LocatorTestPaths();
            var markerExe = paths.CreateExternalExe( "marker-invalid-registry" );
            var markerPath = paths.WriteMarker( markerExe );
            var store = new FakeEddiInstallLocatorStore();
            store.Set( EddiInstallLocatorHive.CurrentUser, EddiInstallLocator.ExecutablePathValueName, paths.GetMissingExternalExe( "missing-current-user" ) );
            store.Set( EddiInstallLocatorHive.LocalMachine, EddiInstallLocator.ExecutablePathValueName, paths.CreateExternalNonEddiExe( "not-eddi-local-machine" ) );

            var resolved = EddiInstallLocator.ResolveExecutablePath(
                paths.ShimDirectory,
                store,
                markerPath,
                paths.EmptyBaseDirectory );

            Assert.AreEqual( markerExe, resolved );
        }

        [TestMethod]
        public void ResolveExecutablePath_RejectsRegistryAndMarkerCandidatesUnderShimDirectory ()
        {
            using var paths = new LocatorTestPaths();
            var shimExe = paths.CreateNestedShimExe();
            var externalFallback = paths.CreateBaseDirectoryExe();
            var markerPath = paths.WriteMarker( shimExe );
            var store = new FakeEddiInstallLocatorStore();
            store.Set( EddiInstallLocatorHive.CurrentUser, EddiInstallLocator.ExecutablePathValueName, shimExe );
            store.Set( EddiInstallLocatorHive.LocalMachine, EddiInstallLocator.ExecutablePathValueName, shimExe );

            var resolved = EddiInstallLocator.ResolveExecutablePath(
                paths.ShimDirectory,
                store,
                markerPath,
                paths.BaseDirectory );

            Assert.AreEqual( externalFallback, resolved );
        }

        [TestMethod]
        public void ResolveExecutablePath_SameDirectoryFallbackSupportsDevLayouts ()
        {
            using var paths = new LocatorTestPaths();
            var shimExe = paths.CreateShimExe();

            var resolved = EddiInstallLocator.ResolveExecutablePath(
                paths.ShimDirectory,
                new FakeEddiInstallLocatorStore(),
                markerFilePath: null,
                baseDirectory: paths.EmptyBaseDirectory );

            Assert.AreEqual( shimExe, resolved );
        }

        [TestMethod]
        public void TryWriteInstallLocation_WritesCurrentUserLocatorValues ()
        {
            using var paths = new LocatorTestPaths();
            var exePath = paths.CreateExternalExe( "self-heal" );
            var store = new FakeEddiInstallLocatorStore();

            var written = EddiInstallLocatorWriter.TryWriteCurrentUserInstallLocation(
                exePath,
                "5.0.2",
                store );

            Assert.IsTrue( written );
            Assert.AreEqual( exePath, store.Get( EddiInstallLocatorWriterHive.CurrentUser, EddiInstallLocatorWriter.ExecutablePathValueName ) );
            Assert.AreEqual( Path.GetDirectoryName( exePath ), store.Get( EddiInstallLocatorWriterHive.CurrentUser, EddiInstallLocatorWriter.InstallDirectoryValueName ) );
            Assert.AreEqual( "5.0.2", store.Get( EddiInstallLocatorWriterHive.CurrentUser, EddiInstallLocatorWriter.VersionValueName ) );
        }

        [TestMethod]
        public void AppRefreshInstallLocator_WritesCurrentUserLocatorValues ()
        {
            using var paths = new LocatorTestPaths();
            var exePath = paths.CreateExternalExe( "app-startup" );
            var store = new FakeEddiInstallLocatorStore();

            App.RefreshInstallLocator( store, exePath );

            Assert.AreEqual( exePath, store.Get( EddiInstallLocatorWriterHive.CurrentUser, EddiInstallLocatorWriter.ExecutablePathValueName ) );
            Assert.AreEqual( Path.GetDirectoryName( exePath ), store.Get( EddiInstallLocatorWriterHive.CurrentUser, EddiInstallLocatorWriter.InstallDirectoryValueName ) );
            Assert.AreEqual( Constants.EDDI_VERSION.ToString(), store.Get( EddiInstallLocatorWriterHive.CurrentUser, EddiInstallLocatorWriter.VersionValueName ) );
        }

        [TestMethod]
        public void AdapterAndAppLocatorContracts_Match ()
        {
            CollectionAssert.AreEqual( GetAdapterLocatorContract(), GetAppLocatorContract() );
        }

        private static string[] GetAdapterLocatorContract () =>
        [
            EddiInstallLocator.RegistrySubKey,
            EddiInstallLocator.ExecutablePathValueName,
            EddiInstallLocator.InstallDirectoryValueName,
            EddiInstallLocator.VersionValueName,
            EddiInstallLocator.MarkerFileName
        ];

        private static string[] GetAppLocatorContract () =>
        [
            EddiInstallLocatorWriter.RegistrySubKey,
            EddiInstallLocatorWriter.ExecutablePathValueName,
            EddiInstallLocatorWriter.InstallDirectoryValueName,
            EddiInstallLocatorWriter.VersionValueName,
            EddiInstallLocatorWriter.MarkerFileName
        ];
    }

    internal sealed class FakeEddiInstallLocatorStore : IEddiInstallLocatorStore, IEddiInstallLocatorWriterStore
    {
        private readonly Dictionary<(string Hive, string Name), string> _values = [];

        public string? ReadValue ( EddiInstallLocatorHive hive, string valueName )
        {
            return _values.TryGetValue( ( hive.ToString(), valueName ), out var value )
                ? value
                : null;
        }

        public bool TryWriteValues (
            EddiInstallLocatorWriterHive hive,
            string executablePath,
            string installDirectory,
            string version )
        {
            Set( hive, EddiInstallLocatorWriter.ExecutablePathValueName, executablePath );
            Set( hive, EddiInstallLocatorWriter.InstallDirectoryValueName, installDirectory );
            Set( hive, EddiInstallLocatorWriter.VersionValueName, version );
            return true;
        }

        public void Set ( EddiInstallLocatorHive hive, string valueName, string value )
        {
            _values[ ( hive.ToString(), valueName ) ] = value;
        }

        public void Set ( EddiInstallLocatorWriterHive hive, string valueName, string value )
        {
            _values[ ( hive.ToString(), valueName ) ] = value;
        }

        public string? Get ( EddiInstallLocatorWriterHive hive, string valueName )
        {
            return _values.TryGetValue( ( hive.ToString(), valueName ), out var value )
                ? value
                : null;
        }
    }

    internal sealed class LocatorTestPaths : IDisposable
    {
        private readonly string _root = Path.Combine( Path.GetTempPath(), $"eddi_locator_{Guid.NewGuid():N}" );

        public LocatorTestPaths ()
        {
            ShimDirectory = Path.Combine( _root, "VoiceAttack", "Apps", "EDDI" );
            BaseDirectory = Path.Combine( _root, "base" );
            EmptyBaseDirectory = Path.Combine( _root, "empty-base" );
            Directory.CreateDirectory( ShimDirectory );
            Directory.CreateDirectory( BaseDirectory );
            Directory.CreateDirectory( EmptyBaseDirectory );
        }

        public string ShimDirectory { get; }
        public string BaseDirectory { get; }
        public string EmptyBaseDirectory { get; }

        public string CreateExternalExe ( string folderName )
        {
            var directory = Path.Combine( _root, "external", folderName );
            Directory.CreateDirectory( directory );
            return CreateExe( directory );
        }

        public string GetMissingExternalExe ( string folderName )
        {
            return Path.Combine( _root, "external", folderName, "EDDI.exe" );
        }

        public string CreateExternalNonEddiExe ( string folderName )
        {
            var directory = Path.Combine( _root, "external", folderName );
            Directory.CreateDirectory( directory );
            var path = Path.Combine( directory, "NotEDDI.exe" );
            File.WriteAllText( path, "test" );
            return path;
        }

        public string CreateShimExe ()
        {
            return CreateExe( ShimDirectory );
        }

        public string CreateNestedShimExe ()
        {
            return CreateExe( Path.Combine( ShimDirectory, "app" ) );
        }

        public string CreateBaseDirectoryExe ()
        {
            return CreateExe( BaseDirectory );
        }

        public string WriteMarker ( string executablePath )
        {
            var markerPath = Path.Combine( ShimDirectory, EddiInstallLocator.MarkerFileName );
            File.WriteAllText( markerPath, executablePath );
            return markerPath;
        }

        private static string CreateExe ( string directory )
        {
            Directory.CreateDirectory( directory );
            var path = Path.Combine( directory, "EDDI.exe" );
            File.WriteAllText( path, "test" );
            return path;
        }

        public void Dispose ()
        {
            if ( Directory.Exists( _root ) )
            {
                Directory.Delete( _root, recursive: true );
            }
        }
    }
}

using EddiConfigService.Configurations;
using EddiDataDefinitions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Utilities;

namespace EddiConfigService
{
    public sealed class ConfigService : INotifyPropertyChanged, IDisposable
    {
        #region Configurations

        // The configurations managed by the configuration service
        public CargoMonitorConfiguration cargoMonitorConfiguration
        {
            get => GetConfig<CargoMonitorConfiguration>( nameof( CargoMonitorConfiguration ) );
            set => SetConfig( nameof(CargoMonitorConfiguration), value );
        }

        public CommanderConfiguration commanderConfiguration
        {
            get => GetConfig<CommanderConfiguration>( nameof( CommanderConfiguration ) );
            set => SetConfig( nameof( CommanderConfiguration ), value );
        }

        public CodexDiscoveryConfiguration codexDiscoveryConfiguration
        {
            get => GetConfig<CodexDiscoveryConfiguration>( nameof( CodexDiscoveryConfiguration ) );
            set => SetConfig( nameof( CodexDiscoveryConfiguration ), value );
        }

        public CrimeMonitorConfiguration crimeMonitorConfiguration
        {
            get => GetConfig<CrimeMonitorConfiguration>( nameof( CrimeMonitorConfiguration ) );
            set => SetConfig( nameof(CrimeMonitorConfiguration), value );
        }

        public EDDIConfiguration eddiConfiguration
        {
            get => GetConfig<EDDIConfiguration>( nameof( EDDIConfiguration ) );
            set => SetConfig( nameof( EDDIConfiguration ), value );
        }

        public EddpConfiguration eddpConfiguration
        {
            get => GetConfig<EddpConfiguration>( nameof( EddpConfiguration ) );
            set => SetConfig( nameof( EddpConfiguration ), value );
        }

        public FleetCarrierConfiguration fleetCarrierConfiguration
        {
            get => GetConfig<FleetCarrierConfiguration>( nameof( FleetCarrierConfiguration ) );
            set => SetConfig( nameof( FleetCarrierConfiguration ), value );
        }

        public GalnetConfiguration galnetConfiguration
        {
            get => GetConfig<GalnetConfiguration>( nameof( GalnetConfiguration ) );
            set => SetConfig( nameof( GalnetConfiguration ), value );
        }

        public InaraConfiguration inaraConfiguration
        {
            get => GetConfig<InaraConfiguration>( nameof( InaraConfiguration ) );
            set => SetConfig( nameof( InaraConfiguration ), value );
        }

        public MaterialMonitorConfiguration materialMonitorConfiguration
        {
            get => GetConfig<MaterialMonitorConfiguration>( nameof( MaterialMonitorConfiguration ) );
            set => SetConfig( nameof( MaterialMonitorConfiguration ), value );
        }

        public MissionMonitorConfiguration missionMonitorConfiguration
        {
            get => GetConfig<MissionMonitorConfiguration>( nameof( MissionMonitorConfiguration ) );
            set => SetConfig( nameof( MissionMonitorConfiguration ), value );
        }

        public NavigationMonitorConfiguration navigationMonitorConfiguration
        {
            get => GetConfig<NavigationMonitorConfiguration>( nameof( NavigationMonitorConfiguration ) );
            set => SetConfig( nameof( NavigationMonitorConfiguration ), value );
        }

        public ShipMonitorConfiguration shipMonitorConfiguration
        {
            get => GetConfig<ShipMonitorConfiguration>( nameof( ShipMonitorConfiguration ) );
            set => SetConfig( nameof( ShipMonitorConfiguration ), value );
        }

        public SpeechServiceConfiguration speechServiceConfiguration
        {
            get => GetConfig<SpeechServiceConfiguration>( nameof( SpeechServiceConfiguration ) );
            set => SetConfig( nameof( SpeechServiceConfiguration ), value );
        }

        public SpeechResponderConfiguration speechResponderConfiguration
        {
            get => GetConfig<SpeechResponderConfiguration>( nameof( SpeechResponderConfiguration ) );
            set => SetConfig( nameof( SpeechResponderConfiguration ), value );
        }

        public StarMapConfiguration edsmConfiguration
        {
            get => GetConfig<StarMapConfiguration>( nameof( StarMapConfiguration ) );
            set => SetConfig( nameof( StarMapConfiguration ), value );
        }

        #endregion

        #region Fields

        private string _dataDirectory;
        private string _commanderFID; // The current commander FID
        private ImmutableDictionary<string, Config> _currentConfigs;
        private readonly Timer _saveTimer;
        private readonly HashSet<string> _dirtyConfigs;
        private readonly object _dirtyLock = new();
        private volatile bool _isDisposed;
        private const int SAVE_DELAY_MS = 1000; // Debounce saves to 1 second

        public static bool unitTesting { get; set; }

        #endregion

        private ConfigService ()
        {
            _dirtyConfigs = [ ];

            // Initialize debounced save timer
            _saveTimer = new Timer(
                _ => SaveDirtyConfigurations(),
                null,
                Timeout.Infinite,
                Timeout.Infinite );

            // Initialize with default (legacy) location
            SetCommander();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged ( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        /// <summary>
        /// Raises PropertyChanged for both the configuration key (type-name) and any public
        /// property whose PropertyType matches the configuration type.
        /// This ensures subscribers that expect either the type-name key or the
        /// public property name (e.g., "commanderConfiguration", "edsmConfiguration") receive updates.
        /// </summary>
        private void RaiseConfigPropertyChangedForKey( string key )
        {
            if ( string.IsNullOrEmpty( key ) ) { return; }

            // Always raise for the type-name key to retain backward compatibility
            OnPropertyChanged( key );

            // Also raise for any public property whose PropertyType equals the config type
            if ( _currentConfigs != null && _currentConfigs.TryGetValue( key, out var config ) && config != null )
            {
                var configType = config.GetType();
                var props = this.GetType()
                    .GetProperties( BindingFlags.Public | BindingFlags.Instance )
                    .Where( p => p.PropertyType == configType )
                    .Select( p => p.Name );

                foreach ( var propName in props )
                {
                    // Avoid duplicate raises for the same name (if it equals the key)
                    if ( !string.Equals( propName, key, StringComparison.Ordinal ) )
                    {
                        OnPropertyChanged( propName );
                    }
                }
            }
        }

        #endregion

        #region Configuration Management

        private T GetConfig<T> ( string key ) where T : Config
        {
            lock ( _dirtyLock )
            {
                return _currentConfigs.TryGetValue( key, out var config )
                    ? config as T
                    : null;
            }
        }

        private void SetConfig<T> ( string key, T value ) where T : Config
        {
            if ( value == null )
            {
                return;
            }

            var changed = false;
            lock ( _dirtyLock )
            {
                var current = _currentConfigs.TryGetValue(key, out var existing) ? existing as T : null;

                if ( current == null || !ConfigEquals( current, value ) )
                {
                    changed = true;
                    _dirtyConfigs.Add( key );

                    // Unsubscribe from old config's PropertyChanged
                    if ( current != null )
                    {
                        current.PropertyChanged -= OnConfigPropertyChanged;
                    }

                    // Subscribe to new config's PropertyChanged
                    value.PropertyChanged -= OnConfigPropertyChanged;
                    value.PropertyChanged += OnConfigPropertyChanged;
                    
                    // Update the current config before notifying to avoid re-entrant handlers seeing stale state
                    _currentConfigs = _currentConfigs.SetItem( key, value );
                }
            }

            if ( changed )
            {
                RaiseConfigPropertyChangedForKey( key );
                // Schedule debounced save (avoid saving to disk on every property change)
                _saveTimer.Change( SAVE_DELAY_MS, Timeout.Infinite );
            }
        }

        private void OnConfigPropertyChanged ( object sender, PropertyChangedEventArgs e )
        {
            if ( sender is Config config )
            {
                var key = config.GetType().Name;
                lock ( _dirtyLock )
                {
                    _dirtyConfigs.Add( key );
                }
                _saveTimer.Change( SAVE_DELAY_MS, Timeout.Infinite );
                RaiseConfigPropertyChangedForKey( key );
            }
        }

        #endregion

        #region Configuration Equality Comparisons

        /// <summary>Compares two configuration objects for equality using reflection</summary>
        private static bool ConfigEquals ( Config current, Config proposed )
        {
            if ( current.GetType() != proposed.GetType() )
            {
                return false;
            }

            var properties = current.GetType().GetProperties(
                BindingFlags.Public | BindingFlags.Instance);

            foreach ( var prop in properties )
            {
                var currentValue = prop.GetValue(current);
                var proposedValue = prop.GetValue(proposed);

                // Handle nullable types
                if ( currentValue == null && proposedValue == null )
                {
                    continue;
                }

                if ( currentValue == null || proposedValue == null )
                {
                    return false;
                }

                // Use proper collection comparison
                if ( !CompareValues( currentValue, proposedValue ) )
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CompareValues ( object left, object right )
        {
            if ( ReferenceEquals( left, right ) )
            {
                return true;
            }

            if ( left == null || right == null )
            {
                return false;
            }

            var type = left.GetType();
            if ( type != right.GetType() )
            {
                return false;
            }

            // Handle IEquatable
            if ( typeof( IEquatable<> ).MakeGenericType( type ).IsAssignableFrom( type ) )
            {
                return left.Equals( right );
            }

            // Handle common collections
            if ( typeof( System.Collections.IEnumerable ).IsAssignableFrom( type ) )
            {
                var leftEnum = (System.Collections.IEnumerable)left;
                var rightEnum = (System.Collections.IEnumerable)right;
                return AreEnumerablesEqual( leftEnum, rightEnum );
            }

            return left.Equals( right );
        }

        private static bool AreEnumerablesEqual (
            System.Collections.IEnumerable left,
            System.Collections.IEnumerable right )
        {
            var leftEnumerator = left.GetEnumerator();
            var rightEnumerator = right.GetEnumerator();

            try
            {
                while ( leftEnumerator.MoveNext() )
                {
                    if ( !rightEnumerator.MoveNext() )
                    {
                        return false;
                    }

                    if ( !CompareValues( leftEnumerator.Current, rightEnumerator.Current ) )
                    {
                        return false;
                    }
                }

                return !rightEnumerator.MoveNext();
            }
            finally
            {
                ( leftEnumerator as IDisposable )?.Dispose();
                ( rightEnumerator as IDisposable )?.Dispose();
            }
        }

        #endregion

        #region Configuration I/O Operations

        private void CopyConfigurations ( string fromDirectory, string toDirectory )
        {
            ArgumentNullException.ThrowIfNull( fromDirectory );
            ArgumentNullException.ThrowIfNull( toDirectory );
            
            try
            {
                if ( !Directory.Exists( toDirectory ) )
                {
                    Directory.CreateDirectory( toDirectory );
                }

                foreach ( var config in _currentConfigs.Values )
                {
                    var relativePath = config.GetType()
                        .GetCustomAttribute<RelativePathAttribute>()
                        ?.relativePath;

                    if ( relativePath == null )
                    {
                        continue;
                    }

                    var fromFile = Path.Combine( fromDirectory, relativePath.TrimStart('\\') );
                    var toFile = Path.Combine( toDirectory, relativePath.TrimStart('\\') );

                    if ( File.Exists( fromFile ) )
                    {
                        var toFileDir = Path.GetDirectoryName( toFile );
                        if ( !string.IsNullOrEmpty( toFileDir ) && !Directory.Exists( toFileDir ) )
                        {
                            Directory.CreateDirectory( toFileDir );
                        }

                        File.Copy( fromFile, toFile, overwrite: true );
                    }
                }
            }
            catch ( Exception ex )
            {
                Logging.Warn( $"Failed to copy configurations from {fromDirectory} to {toDirectory}", ex );
            }
        }

        private void DeleteConfigurations ( string fromDirectory )
        {
            try
            {
                foreach ( var config in _currentConfigs.Values )
                {
                    var relativePath = config.GetType()
                        .GetCustomAttribute<RelativePathAttribute>()
                        ?.relativePath;

                    if ( relativePath == null )
                    {
                        continue;
                    }

                    var filename = Path.Combine( fromDirectory, relativePath.TrimStart('\\') );
                    if ( File.Exists( filename ) )
                    {
                        File.Delete( filename );
                    }
                }
            }
            catch ( IOException ioe )
            {
                Logging.Warn( $"Failed to delete configuration files from {fromDirectory}", ioe );
            }
        }

        /// <summary>Deserialize a configuration object from a JSON string (primarily for unit testing)</summary>
        public static T FromJson<T> ( string json ) where T : new()
        {
            T configuration = default;
            if ( json != null )
            {
                try
                {
                    configuration = JsonConvert.DeserializeObject<T>( json );
                }
                catch ( Exception ex )
                {
                    Logging.Warn( $"Failed to read {typeof( T ).Name}", ex );
                }
            }
            return configuration != null ? configuration : new T();
        }

        private ImmutableDictionary<string, Config> LoadAllConfigurations ()
        {
            var configs = new Dictionary<string, Config>();

            // Load each configuration type from its JSON file
            var configTypes = GetConfigTypes();
            foreach ( var configType in configTypes )
            {
                try
                {
                    var relativePath = configType.GetCustomAttribute<RelativePathAttribute>()?.relativePath;
                    if ( relativePath != null )
                    {
                        var filename = Path.Combine(_dataDirectory, relativePath.TrimStart('\\'));
                        var config = LoadConfiguration(configType, filename);

                        // Subscribe to nested property changes
                        config.PropertyChanged -= OnConfigPropertyChanged;
                        config.PropertyChanged += OnConfigPropertyChanged;

                        configs[ configType.Name ] = config;
                    }
                }
                catch ( Exception ex )
                {
                    Logging.Error( $"Failed to load {configType.Name}", ex );
                    var config = (Config)Activator.CreateInstance( configType );
                    if ( config != null )
                    {
                        config.PropertyChanged -= OnConfigPropertyChanged;
                        config.PropertyChanged += OnConfigPropertyChanged;
                        configs[ configType.Name ] = config;
                    }
                }
            }

            return configs.ToImmutableDictionary();
        }

        private static Config LoadConfiguration ( Type configType, string filename )
        {
            if ( !File.Exists( filename ) && unitTesting )
            {
                return (Config)Activator.CreateInstance( configType );
            }

            try
            {
                if ( File.Exists( filename ) )
                {
                    var json = File.ReadAllText(filename);
                    return (Config)JsonConvert.DeserializeObject( json, configType );
                }
            }
            catch ( Exception ex )
            {
                Logging.Warn( $"Failed to load {configType.Name} from {filename}", ex );
            }

            return (Config)Activator.CreateInstance( configType );
        }

        /// <summary>
        /// Saves only the configurations that have been modified since the last save.
        /// Called by debounced timer to avoid excessive disk I/O.
        /// </summary>
        private void SaveDirtyConfigurations ()
        {
            if ( unitTesting )
            {
                return;
            }

            List<string> configsToSave;
            lock ( _dirtyLock )
            {
                configsToSave = _dirtyConfigs.ToList();
                _dirtyConfigs.Clear();
            }

            if ( configsToSave.Count == 0 )
            {
                return;
            }

            // Create directory if needed
            if ( !Directory.Exists( _dataDirectory ) )
            {
                Directory.CreateDirectory( _dataDirectory );
            }

            // Save each dirty configuration synchronously (matching original behavior)
            foreach ( var key in configsToSave )
            {
                if ( _currentConfigs.TryGetValue( key, out var config ) )
                {
                    SaveConfiguration( config );
                }
            }
        }

        /// <summary>Saves a single configuration to disk synchronously (maintains compatibility with ConfigWrite.cs pattern)</summary>
        public void SaveConfiguration ( Config config )
        {
            try
            {
                var relativePath = config.GetType()
                    .GetCustomAttribute<RelativePathAttribute>()
                    ?.relativePath;

                if ( relativePath == null )
                {
                    return;
                }

                var filename = Path.Combine(_dataDirectory, relativePath.TrimStart('\\'));
                var directory = Path.GetDirectoryName(filename);

                // Ensure directory exists
                if ( !string.IsNullOrEmpty( directory ) && !Directory.Exists( directory ) )
                {
                    Directory.CreateDirectory( directory );
                }

                var json = JsonConvert.SerializeObject( config, Formatting.Indented );

                Logging.Debug( $"Writing configuration {config.GetType().Name} to file.", json );

                // Use atomic write pattern: write to temp file first, then move
                var tempFile = filename + ".tmp";
                try
                {
                    File.WriteAllText( tempFile, json );
                    File.Replace( tempFile, filename, null, ignoreMetadataErrors: false );
                }
                catch ( IOException )
                {
                    // Fallback to direct write if atomic operation fails
                    File.WriteAllText( filename, json );
                    if ( File.Exists( tempFile ) )
                    {
                        try
                        {
                            File.Delete( tempFile );
                        }
                        catch
                        {
                            // ignored
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                Logging.Error( $"Failed to save configuration {config.GetType().Name}", ex );
            }
        }

        #endregion

        #region Legacy Configuration Migration

        /// <summary>Converts legacy configuration data from EDDI configuration to their proper configuration classes</summary>
        private void ConvertLegacyConfigData ( ImmutableDictionary<string, Config> configs )
        {
            // Convert legacy data saved in the EDDI configuration to the Commander configuration
            if ( configs.TryGetValue( nameof( CommanderConfiguration ), out var commanderConfigVal ) &&
                 commanderConfigVal is CommanderConfiguration commanderConfig )
            {
                if ( configs.TryGetValue( nameof( EDDIConfiguration ), out var eddiConfigVal ) &&
                     eddiConfigVal is EDDIConfiguration configuration &&
                     configuration._additionalData is IDictionary<string, JToken> eddiConfigAdditionalData )
                {
                    if ( eddiConfigAdditionalData.TryGetValue( "CommanderName", out var commanderName ) )
                    {
                        commanderConfig.commanderName = commanderName.ToString();
                    }
                    if ( eddiConfigAdditionalData.TryGetValue( "Gender", out var gender ) )
                    {
                        commanderConfig.gender = gender.ToString();
                    }
                    if ( eddiConfigAdditionalData.TryGetValue( "homeSystemAddress", out var homeSystemAddress ) )
                    {
                        commanderConfig.homeSystemAddress = homeSystemAddress.ToObject<ulong?>();
                    }
                    if ( eddiConfigAdditionalData.TryGetValue( "homeStationMarketID", out var homeStationMarketID ) )
                    {
                        commanderConfig.homeStationMarketID = homeStationMarketID.ToObject<long?>();
                    }
                    if ( eddiConfigAdditionalData.TryGetValue( "PhoneticName", out var phoneticName ) )
                    {
                        commanderConfig.phoneticName = phoneticName.ToString();
                    }
                    if ( eddiConfigAdditionalData.TryGetValue( "powerMerits", out var powerMerits ) )
                    {
                        commanderConfig.powerMerits = powerMerits.ToObject<int?>();
                    }
                    if ( eddiConfigAdditionalData.TryGetValue( "squadronName", out var squadronName ) )
                    {
                        commanderConfig.squadronName = squadronName.ToString();
                    }
                    if ( eddiConfigAdditionalData.TryGetValue( "squadronID", out var squadronID ) )
                    {
                        commanderConfig.squadronTag = squadronID.ToString();
                    }
                    if ( eddiConfigAdditionalData.TryGetValue( "squadronAllegiance", out var squadronAllegiance ) )
                    {
                        commanderConfig.squadronAllegiance = squadronAllegiance.ToString();
                    }
                    if ( eddiConfigAdditionalData.TryGetValue( "squadronPower", out var squadronPower ) )
                    {
                        commanderConfig.squadronPower = squadronPower.ToString();
                    }
                    if ( eddiConfigAdditionalData.TryGetValue( "squadronSystemAddress", out var squadronSystemAddress ) )
                    {
                        commanderConfig.squadronSystemAddress = squadronSystemAddress.ToObject<ulong?>();
                    }
                    if ( eddiConfigAdditionalData.TryGetValue( "squadronFaction", out var squadronFaction ) )
                    {
                        commanderConfig.squadronFaction = squadronFaction.ToString();
                    }
                }
            }

            // Convert legacy data saved in the EDDI configuration to the Fleet Carrier configuration
            if ( configs.TryGetValue( nameof( FleetCarrierConfiguration ), out var fleetCarrierConfigVal ) &&
                 fleetCarrierConfigVal is FleetCarrierConfiguration fleetCarrierConfig &&
                 configs.TryGetValue( nameof( EDDIConfiguration ), out var eddiConfigVal2 ) &&
                 eddiConfigVal2._additionalData is IDictionary<string, JToken> eddiConfigAdditionalData2 )
            {
                if ( eddiConfigAdditionalData2.TryGetValue( "fleetCarrier", out var fleetCarrier ) )
                {
                    fleetCarrierConfig.fleetCarrier = fleetCarrier.ToObject<FleetCarrier>();
                }
            }

            // Clear legacy data after migration
            foreach ( var config in configs )
            {
                config.Value._additionalData = null;
            }
        }

        #endregion

        #region Helper Methods

        private static IEnumerable<Type> GetConfigTypes ()
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where( t =>
                    t.IsSubclassOf( typeof( Config ) ) &&
                    t.GetCustomAttribute<RelativePathAttribute>() != null &&
                    !t.IsAbstract );
        }

        /// <summary>Gets the data directory for the specified commander FID</summary>
        private static string GetDataDirectory ( string commanderFID = null )
        {
            return $@"{Constants.DATA_DIR}{( !string.IsNullOrEmpty( commanderFID ) ? @"\" + commanderFID : null )}";
        }

        /// <summary>Sets the current commander FID and corresponding data directory</summary>
        public void SetCommander ( string newCommanderFID = null )
        {
            List<string> keysToNotify;
            lock ( _dirtyLock )
            {
                // Save existing configurations before switching commander
                SaveDirtyConfigurations();

                var newDataDirectory = GetDataDirectory(newCommanderFID);

                // On first transition from legacy to commander-specific structure
                if ( string.IsNullOrEmpty( _commanderFID ) && !string.IsNullOrEmpty( newCommanderFID ) )
                {
                    CopyConfigurations( _dataDirectory, newDataDirectory );
                    DeleteConfigurations( _dataDirectory );
                }

                _commanderFID = newCommanderFID;
                _dataDirectory = newDataDirectory;

                // Load configurations for the new commander
                _currentConfigs = LoadAllConfigurations();

                // Apply legacy data migrations after loading
                ConvertLegacyConfigData( _currentConfigs );

                // Mark all configs as loaded (not dirty)
                _dirtyConfigs.Clear();

                // capture keys to notify outside of lock
                keysToNotify = _currentConfigs?.Keys.ToList() ?? [ ];
            }

            // Notify subscribers outside the lock to avoid re-entrancy/deadlocks
            foreach ( var key in keysToNotify )
            {
                RaiseConfigPropertyChangedForKey( key );
            }
        }

        #endregion

        #region Singleton

        private static readonly Lazy<ConfigService> _instance =
            new(() =>
            {
                Logging.Debug("Creating ConfigService instance");
                return new ConfigService();
            });

        public static ConfigService Instance => _instance.Value;

        #endregion

        public void Dispose ()
        {
            if ( _isDisposed )
            {
                return;
            }
            _isDisposed = true;

            _saveTimer?.Dispose();
            SaveDirtyConfigurations(); // Final save on shutdown
        }
    }
}

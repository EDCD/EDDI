using EddiConfigService.Configurations;
using EddiDataDefinitions;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Utilities;

namespace EddiConfigService
{
    public sealed partial class ConfigService : INotifyPropertyChanged
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

        private T GetConfig<T> ( string key, bool clone = true ) where T : Config
        {
            lock ( configurationsLock )
            {
                // Return a clone of the configuration object to keep the original immutable
                return clone ? currentConfigs[ key ].Clone<T>() : currentConfigs[ key ] as T;
            }
        }

        private void SetConfig<T> ( string key, T value ) where T : Config
        {
            lock ( configurationsLock )
            {
                // Check for equality and only raise a change event if the value has changed
                if ( !GetConfig<T>( key, false ).DeepEquals( value ) )
                {
                    currentConfigs[ key ] = value;
                    OnPropertyChanged( key );
                }
            }
        }

        /// <summary>Reads configurations from the specified data directory</summary>
        private ConcurrentDictionary<string, Config> ReadConfigurations ( string directory = null )
        {
            if ( string.IsNullOrEmpty( directory ) )
            {
                directory = GetDataDirectory( commanderFID );
            }
            var configs = new ConcurrentDictionary<string, Config>(
                GetConfigTypes().ToDictionary(
                    t => t.Name,
                    t => (Config)typeof(ConfigService)
                        .GetMethod("FromFile", BindingFlags.NonPublic | BindingFlags.Static)
                        ?.MakeGenericMethod(t)
                        .Invoke(null, new object[] { directory })
                )
            );
            ConvertLegacyConfigData( configs );
            return configs;

            IEnumerable<Type> GetConfigTypes ()
            {
                return Assembly.GetExecutingAssembly()
                    .GetTypes()
                    .Where( t => t.IsSubclassOf( typeof( Config ) ) && t.GetCustomAttribute<RelativePathAttribute>() != null );
            }
        }

        private void ConvertLegacyConfigData(ConcurrentDictionary<string, Config> configs)
        {
            // Convert legacy data saved in the EDDI configuration to the Commander configuration
            if ( configs[ nameof( CommanderConfiguration ) ] is CommanderConfiguration commanderConfig )
            {
                if (configs[ nameof( EDDIConfiguration ) ]._additionalData is IDictionary<string, JToken> eddiConfigAdditionalData)
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
                        commanderConfig.squadronID = squadronID.ToString();
                    }
                    if ( eddiConfigAdditionalData.TryGetValue( "squadronRank", out var squadronRank ) )
                    {
                        commanderConfig.squadronRank = squadronRank.ToString();
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
            if ( configs[ nameof(FleetCarrierConfiguration) ] is FleetCarrierConfiguration fleetCarrierConfig )
            {
                if (configs[ nameof(EDDIConfiguration) ]._additionalData is IDictionary<string, JToken>
                    eddiConfigAdditionalData)
                {
                    if ( eddiConfigAdditionalData.TryGetValue( "fleetCarrier", out var fleetCarrier ) )
                    {
                        fleetCarrierConfig.fleetCarrier = fleetCarrier.ToObject<FleetCarrier>();
                    }
                }
            }

            foreach ( var config in configs )
            {
                config.Value._additionalData = null;
            }
        }

        #endregion

        // The current commander FID
        private string commanderFID { get; set; }

        // The directory to use for reading and saving configuration files
        private string dataDirectory { get; set; }

        private ConcurrentDictionary<string, Config> currentConfigs = new ConcurrentDictionary<string, Config>();

        private static readonly object configurationsLock = new object();

        public static bool unitTesting { get; set; }

        private ConfigService()
        {
            SetCommander();
            PropertyChanged += ConfigChanged;
        }

        private void ConfigChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (var config in currentConfigs)
            {
                if (config.Key == e.PropertyName && !unitTesting)
                {
                    ToFile(config.Value, dataDirectory);
                }
            }
        }

        private static readonly Lazy<ConfigService> instance = new Lazy<ConfigService>( () =>
        {
            Logging.Debug( "No configuration service instance: creating one" );
            return new ConfigService();
        } );
        public static ConfigService Instance => instance.Value;

        /// <summary>Sets the current commander FID and corresponding data directory (if null, we'll default to the legacy directory location)</summary>
        public void SetCommander(string newCommanderFID = null)
        {
            lock (configurationsLock)
            {
                if (currentConfigs.Any())
                {
                    SaveConfigurations(dataDirectory, currentConfigs);
                }

                var newDataDirectory = GetDataDirectory(newCommanderFID);
                if (string.IsNullOrEmpty(commanderFID) && !string.IsNullOrEmpty(newCommanderFID))
                {
                    // When we first transition from the legacy file structure (i.e. all configurations in the root data directory)
                    // to the new file structure, move a copy of our current configuration with us.
                    CopyConfigurations(dataDirectory, newDataDirectory);
                    DeleteConfigurations(dataDirectory, currentConfigs);
                }
                commanderFID = newCommanderFID;
                currentConfigs = ReadConfigurations(newDataDirectory);
                dataDirectory = newDataDirectory;
            }
        }

        /// <summary>Gets the data directory for the specified commander FID</summary>
        private string GetDataDirectory(string _commanderFID = null)
        {
            return $@"{Constants.DATA_DIR}{(!string.IsNullOrEmpty(_commanderFID) ? @"\" + _commanderFID : null)}";
        }

        /// <summary>Saves configurations to the specified data directory</summary>
        private void SaveConfigurations(string directory, ConcurrentDictionary<string, Config> configurations)
        {
            if (configurations is null || unitTesting) { return; }

            if (!string.IsNullOrEmpty(directory))
            {
                var directoryInfo = new DirectoryInfo(directory);
                if (!directoryInfo.Exists)
                {
                    directoryInfo.Create();
                }
            }

            configurations.AsParallel().ForAll(c => ToFile(c.Value, directory));
        }

        /// <summary>Copies configurations from one directory to another</summary>
        private void CopyConfigurations(string fromDirectory, string toDirectory)
        {
            SaveConfigurations(toDirectory, ReadConfigurations(fromDirectory));
        }

        private void DeleteConfigurations(string fromDirectory, ConcurrentDictionary<string, Config> configurations)
        {
            if (configurations is null || unitTesting) { return; }

            foreach ( var config in configurations.Values )
            {
                var relativePath = ( config.GetType().GetCustomAttribute( typeof(RelativePathAttribute) ) as RelativePathAttribute )?.relativePath;
                var filename = fromDirectory + relativePath;
                try
                {
                    if ( File.Exists( filename ) )
                    {
                        File.Delete( filename );
                    }
                }
                catch ( IOException ioe )
                {
                    Logging.Warn( $"Failed to delete configuration file {filename}", ioe );
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

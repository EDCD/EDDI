using JetBrains.Annotations;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Utilities;

namespace EddiConfigService
{
    public partial class ConfigService : INotifyPropertyChanged
    {
        #region Configurations

        // The configurations managed by the configuration service
        public CargoMonitorConfiguration cargoMonitorConfiguration
        {
            get => currentConfigs[nameof(cargoMonitorConfiguration)] as CargoMonitorConfiguration;
            set
            {
                currentConfigs[nameof(cargoMonitorConfiguration)] = value;
                OnPropertyChanged();
            }
        }

        public CrimeMonitorConfiguration crimeMonitorConfiguration
        {
            get => currentConfigs[nameof(crimeMonitorConfiguration)] as CrimeMonitorConfiguration;
            set
            {
                currentConfigs[nameof(crimeMonitorConfiguration)] = value;
                OnPropertyChanged();
            }
        }

        public EDDIConfiguration eddiConfiguration
        {
            get => currentConfigs[nameof(eddiConfiguration)] as EDDIConfiguration;
            set
            {
                currentConfigs[nameof(eddiConfiguration)] = value;
                OnPropertyChanged();
            }
        }

        public EddpConfiguration eddpConfiguration
        {
            get => currentConfigs[nameof(eddpConfiguration)] as EddpConfiguration;
            set
            {
                currentConfigs[nameof(eddpConfiguration)] = value;
                OnPropertyChanged();
            }
        }

        public GalnetConfiguration galnetConfiguration
        {
            get => currentConfigs[nameof(galnetConfiguration)] as GalnetConfiguration;
            set
            {
                currentConfigs[nameof(galnetConfiguration)] = value;
                OnPropertyChanged();
            }
        }

        public InaraConfiguration inaraConfiguration
        {
            get => currentConfigs[nameof(inaraConfiguration)] as InaraConfiguration;
            set
            {
                currentConfigs[nameof(inaraConfiguration)] = value;
                OnPropertyChanged();
            }
        }

        public MaterialMonitorConfiguration materialMonitorConfiguration
        {
            get => currentConfigs[nameof(materialMonitorConfiguration)] as MaterialMonitorConfiguration;
            set
            {
                currentConfigs[nameof(materialMonitorConfiguration)] = value;
                OnPropertyChanged();
            }
        }

        public MissionMonitorConfiguration missionMonitorConfiguration
        {
            get => currentConfigs[nameof(missionMonitorConfiguration)] as MissionMonitorConfiguration;
            set
            {
                currentConfigs[nameof(missionMonitorConfiguration)] = value;
                OnPropertyChanged();
            }
        }

        public NavigationMonitorConfiguration navigationMonitorConfiguration
        {
            get => currentConfigs[nameof(navigationMonitorConfiguration)] as NavigationMonitorConfiguration;
            set
            {
                currentConfigs[nameof(navigationMonitorConfiguration)] = value;
                OnPropertyChanged();
            }
        }

        public SpeechResponderConfiguration speechResponderConfiguration
        {
            get => currentConfigs[nameof(speechResponderConfiguration)] as SpeechResponderConfiguration;
            set
            {
                currentConfigs[nameof(speechResponderConfiguration)] = value;
                OnPropertyChanged();
            }
        }

        public StarMapConfiguration edsmConfiguration
        {
            get => currentConfigs[nameof(edsmConfiguration)] as StarMapConfiguration;
            set
            {
                currentConfigs[nameof(edsmConfiguration)] = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Saves configurations from the specified data directory</summary>
        public ConcurrentDictionary<string, Config> ReadConfigurations(string directory)
        {
            return new ConcurrentDictionary<string, Config> (new Dictionary<string, Config>
            {
                {nameof(cargoMonitorConfiguration), FromFile<CargoMonitorConfiguration>(directory)},
                {nameof(crimeMonitorConfiguration), FromFile<CrimeMonitorConfiguration>(directory)},
                {nameof(eddiConfiguration), FromFile<EDDIConfiguration>(directory)},
                {nameof(edsmConfiguration), FromFile<StarMapConfiguration>(directory)},
                {nameof(eddpConfiguration), FromFile<EddpConfiguration>(directory)},
                {nameof(galnetConfiguration), FromFile<GalnetConfiguration>(directory)},
                {nameof(inaraConfiguration), FromFile<InaraConfiguration>(directory)},
                {nameof(materialMonitorConfiguration), FromFile<MaterialMonitorConfiguration>(directory)},
                {nameof(missionMonitorConfiguration), FromFile<MissionMonitorConfiguration>(directory)},
                {nameof(navigationMonitorConfiguration), FromFile<NavigationMonitorConfiguration>(directory)},
                {nameof(speechResponderConfiguration), FromFile<SpeechResponderConfiguration>(directory)}
            });
        }

        #endregion

        // The directory to use for reading and saving configuration files
        private string dataDirectory { get; set; }

        private ConcurrentDictionary<string, Config> currentConfigs = new ConcurrentDictionary<string, Config>();

        private static readonly object configurationsLock = new object();

        public ConfigService(string commanderFID = null)
        {
            SetCommander(commanderFID);
            PropertyChanged += ConfigChanged;
        }

        private void ConfigChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (var config in currentConfigs)
            {
                if (config.Key == e.PropertyName)
                {
                    ToFile(config.Value, dataDirectory);
                }
            }
        }

        private static ConfigService instance;
        private static readonly object instanceLock = new object();

        public static ConfigService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            Logging.Debug("No configuration service instance: creating one");
                            instance = new ConfigService();
                        }
                    }
                }

                return instance;
            }
        }

        /// <summary>Sets the current commander FID and corresponding data directory (if null, we'll default to the legacy directory location)</summary>
        public void SetCommander(string commanderFID)
        {
            lock (configurationsLock)
            {
                if (currentConfigs.Any())
                {
                    SaveConfigurations(dataDirectory, currentConfigs);
                }

                dataDirectory = GetDataDirectory(commanderFID);
                currentConfigs = ReadConfigurations(dataDirectory);
            }
        }

        /// <summary>Gets the data directory for the specified commander FID</summary>
        private string GetDataDirectory(string commanderFID)
        {
            return $@"{Constants.DATA_DIR}{(!string.IsNullOrEmpty(commanderFID) ? @"\" + commanderFID : null)}";
        }

        /// <summary>Saves configurations to the specified data directory</summary>
        public void SaveConfigurations(string directory, ConcurrentDictionary<string, Config> configurations)
        {
            if (configurations is null)
            {
                return;
            }

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
        public void CopyConfigurations(string fromDirectory, string toDirectory)
        {
            SaveConfigurations(toDirectory, ReadConfigurations(fromDirectory));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

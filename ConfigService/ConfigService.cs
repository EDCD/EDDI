using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
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

        /// <summary>Saves configurations from the specified data directory</summary>
        public Dictionary<string, Config> ReadConfigurations(string directory)
        {
            return new Dictionary<string, Config>
            {
                {nameof(cargoMonitorConfiguration), FromFile<CargoMonitorConfiguration>(directory)},
                {nameof(crimeMonitorConfiguration), FromFile<CrimeMonitorConfiguration>(directory)},
                {nameof(missionMonitorConfiguration), FromFile<MissionMonitorConfiguration>(directory)},
                {nameof(navigationMonitorConfiguration), FromFile<NavigationMonitorConfiguration>(directory)}
            };
        }

        #endregion

        // The directory to use for reading and saving configuration files
        private string dataDirectory { get; set; }

        private Dictionary<string, Config> currentConfigs = new Dictionary<string, Config>();

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
        public void SaveConfigurations(string directory, Dictionary<string, Config> configurations)
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

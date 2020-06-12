using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiConfigService
{
    public class ConfigService
    {
        private static ConfigService instance;
        private static readonly object instanceLock = new object();
        private static readonly object configLock = new object();
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

        private ConfigService()
        {

        }

        private CargoMonitorConfiguration _cargoMonitorConfiguration;
        public CargoMonitorConfiguration cargoMonitorConfiguration
        {
            get
            {
                lock (configLock)
                {
                    return _cargoMonitorConfiguration ?? CargoMonitorConfiguration.FromFile();
                }
            }
            set
            {
                _cargoMonitorConfiguration = value;
                lock (configLock)
                {
                    // Write configuration with current missions data
                    value.ToFile();
                }
            }
        }

        private MissionMonitorConfiguration _missionMonitorConfiguration;
        public MissionMonitorConfiguration missionMonitorConfiguration
        {
            get
            {
                lock (configLock)
                {
                    return _missionMonitorConfiguration ?? MissionMonitorConfiguration.FromFile();
                }
            }
            set
            { _missionMonitorConfiguration = value;
                lock (configLock)
                {
                    // Write configuration with current missions data
                    value.ToFile();
                }
            }
        }

        private NavigationMonitorConfiguration _navigationMonitorConfiguration;
        public NavigationMonitorConfiguration navigationMonitorConfiguration
        {
            get
            {
                lock (configLock)
                {
                    return _navigationMonitorConfiguration ?? NavigationMonitorConfiguration.FromFile();
                }
            }
            set
            {
                _navigationMonitorConfiguration = value;
                lock (configLock)
                {
                    // Write configuration with current missions data
                    value.ToFile();
                }
            }
        }
    }
}

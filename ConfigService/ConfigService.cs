using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static readonly object cargoLock = new object();
        private static readonly object missionLock = new object();
        private static readonly object navigationLock = new object();

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
                return _cargoMonitorConfiguration ?? CargoMonitorConfiguration.FromFile();
            }
            set
            {
                var stackTrace = new StackTrace();
                var Namespace = stackTrace.GetFrame(1).GetMethod().DeclaringType.Namespace;
                if (Namespace == "EddiCargoMonitor")
                {
                    lock (cargoLock)
                    {
                        _cargoMonitorConfiguration = value;
                    }

                    // Write configuration with current cargo data
                    value.ToFile();
                }
                else
                { }
            }
        }

        private MissionMonitorConfiguration _missionMonitorConfiguration;
        public MissionMonitorConfiguration missionMonitorConfiguration
        {
            get
            {
                return _missionMonitorConfiguration ?? MissionMonitorConfiguration.FromFile();
            }
            set
            {
                var stackTrace = new StackTrace();
                var Namespace = stackTrace.GetFrame(1).GetMethod().DeclaringType.Namespace;
                if (Namespace == "EddiMissionMonitor")
                {
                    lock (missionLock)
                    {
                        _missionMonitorConfiguration = value;
                    }

                    // Write configuration with current missions data
                    value.ToFile();
                }
                else { }
            }
        }

        private NavigationMonitorConfiguration _navigationMonitorConfiguration;
        public NavigationMonitorConfiguration navigationMonitorConfiguration
        {
            get
            {
                return _navigationMonitorConfiguration ?? NavigationMonitorConfiguration.FromFile();
            }
            set
            {
                var stackTrace = new StackTrace();
                var Namespace = stackTrace.GetFrame(1).GetMethod().DeclaringType.Namespace;
                if (Namespace == "EddiNavigationMonitor" || Namespace == "EddiNavigationService")
                {
                    lock (navigationLock)
                    {
                        _navigationMonitorConfiguration = value;
                    }

                    // Write configuration with current navigation data
                    value.ToFile();
                }
                else { }
            }
        }
    }
}

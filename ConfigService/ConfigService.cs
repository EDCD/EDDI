using Utilities;

namespace EddiConfigService
{
    public class ConfigService
    {
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

        private CargoMonitorConfiguration _cargoMonitorConfiguration;
        public CargoMonitorConfiguration cargoMonitorConfiguration
        {
            get
            {
                CargoMonitorConfiguration result = null;
                LockManager.GetLock(nameof(CargoMonitorConfiguration), () =>
                {
                    result = _cargoMonitorConfiguration;
                });
                return result ?? CargoMonitorConfiguration.FromFile();
            }
            set
            {
                LockManager.GetLock(nameof(CargoMonitorConfiguration), () =>
                {
                    _cargoMonitorConfiguration = value;
                });
            }
        }

        private MissionMonitorConfiguration _missionMonitorConfiguration;
        public MissionMonitorConfiguration missionMonitorConfiguration
        {
            get
            {
                MissionMonitorConfiguration result = null;
                LockManager.GetLock(nameof(MissionMonitorConfiguration), () =>
                {
                    result = _missionMonitorConfiguration;
                });
                return result ?? MissionMonitorConfiguration.FromFile();
            }
            set
            {
                LockManager.GetLock(nameof(MissionMonitorConfiguration), () =>
                {
                    _missionMonitorConfiguration = value;
                });
            }
        }

        private NavigationMonitorConfiguration _navigationMonitorConfiguration;
        public NavigationMonitorConfiguration navigationMonitorConfiguration
        {
            get
            {
                NavigationMonitorConfiguration result = null;
                LockManager.GetLock(nameof(NavigationMonitorConfiguration), () =>
                {
                    result = _navigationMonitorConfiguration;
                });
                return result ?? NavigationMonitorConfiguration.FromFile();
            }
            set
            {
                LockManager.GetLock(nameof(NavigationMonitorConfiguration), () =>
                {
                    _navigationMonitorConfiguration = value;
                });
            }
        }
    }
}

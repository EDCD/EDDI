using System;

namespace Utilities
{
    /// <summary>
    ///  Various constants used throughout the codebase
    /// </summary>
    public class Constants
    {
        public const string EDDI_NAME = "EDDI";
        public const string EDDI_VERSION = "3.0.1-b2";
        public const string EDDI_SERVER_URL = "http://edcd.github.io/EDDP/";
        public static readonly string EDDI_SYSTEM_MUTEX_NAME = $"{EDDI_SERVER_URL}/{EDDI_NAME}/{Environment.GetEnvironmentVariable("UserName")}";

        public static readonly string DATA_DIR = Environment.GetEnvironmentVariable("AppData") + "\\" + EDDI_NAME;

        public static readonly string ENVIRONMENT_WITCH_SPACE = Properties.Resources.witch_space;
        public static readonly string ENVIRONMENT_SUPERCRUISE = Properties.Resources.supercruise;
        public static readonly string ENVIRONMENT_NORMAL_SPACE = Properties.Resources.normal_space;

        public static readonly string VEHICLE_SHIP = Properties.Resources.ship;
        public static readonly string VEHICLE_SRV = Properties.Resources.srv;
        public static readonly string VEHICLE_FIGHTER = Properties.Resources.fighter;

        // Physical Constants
        public const int lightSpeedMetersPerSecond = 299792458;
        public const int solarRadiusMeters = 695500000;
        public const double solAbsoluteMagnitude = 4.83;
        public const double solLuminosity = 3.846e26;
        public const double stefanBoltzmann = 5.670367e-8;
    }
}

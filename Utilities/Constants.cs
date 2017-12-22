using System;

namespace Utilities
{
    /// <summary>
    ///  Various constants used throughout the codebase
    /// </summary>
    public class Constants
    {
        public const string EDDI_NAME = "EDDI";
        public const string EDDI_VERSION = "2.4.6-b2";
        public const string EDDI_SERVER_URL = "http://edcd.github.io/EDDP/";

        public static readonly string DATA_DIR = Environment.GetEnvironmentVariable("AppData") + "\\" + EDDI_NAME;

        public static readonly string ENVIRONMENT_WITCH_SPACE = I18N.GetString("witch_space");
        public static readonly string ENVIRONMENT_SUPERCRUISE = I18N.GetString("supercruise");
        public static readonly string ENVIRONMENT_NORMAL_SPACE = I18N.GetString("normal_space");

        public static readonly string VEHICLE_SHIP = I18N.GetString("ship");
        public static readonly string VEHICLE_SRV = I18N.GetString("srv");
        public static readonly string VEHICLE_FIGHTER = I18N.GetString("fighter");
        // Physical Constants
        public const int lightSpeedMetersPerSecond = 299792458;
        public const double solAbsoluteMagnitude = 4.83;
        public const int solarRadiusMeters = 695500000;
        public const double solLuminosity = 3.846e26;
        public const double stefanBoltzmann = 5.670367e-8;
    }
}

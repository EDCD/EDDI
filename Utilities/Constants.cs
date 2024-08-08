using System;

namespace Utilities
{
    /// <summary>
    ///  Various constants used throughout the codebase
    /// </summary>
    public class Constants
    {
        public const string EDDI_NAME = "EDDI";
        public const string EDDI_URL_PROTOCOL = "eddi";
        public static Version EDDI_VERSION = new Version(4, 1, 0, Version.TestPhase.b, 1);
        public const string EDDI_SERVER_URL = "https://edcd.github.io/EDDP/";
        public static readonly string EDDI_SYSTEM_MUTEX_NAME = $"{EDDI_SERVER_URL}/{EDDI_NAME}/{Environment.GetEnvironmentVariable("UserName")}";

        public static readonly string DATA_DIR = Environment.GetEnvironmentVariable("AppData") + "\\" + EDDI_NAME;

        public static readonly string ENVIRONMENT_WITCH_SPACE = "Witch Space";
        public static readonly string ENVIRONMENT_SUPERCRUISE = "Supercruise";
        public static readonly string ENVIRONMENT_NORMAL_SPACE = "Normal Space";
        public static readonly string ENVIRONMENT_DOCKED = "Docked";
        public static readonly string ENVIRONMENT_LANDED = "Landed";

        public static readonly string VEHICLE_SHIP = "Ship";
        public static readonly string VEHICLE_SRV = "SRV";
        public static readonly string VEHICLE_FIGHTER = "Fighter";
        public static readonly string VEHICLE_LEGS = "On Foot";
        public static readonly string VEHICLE_TAXI = "Taxi";
        public static readonly string VEHICLE_MULTICREW = "Multicrew";

        // Physical Constants
        public const int lightSpeedMetersPerSecond = 299792458;
        public const int solarRadiusMeters = 695500000;
        public const double solAbsoluteMagnitude = 4.83;
        public const double solLuminosity = 3.846e26;
        public const double stefanBoltzmann = 5.670367e-8;
        public const decimal earthGravityMetersPerSecondSquared = 9.80665M;
        public const long astronomicalUnitsMeters = 149597870700;
        public const decimal earthPressurePascals = 101231.65625M;
        public const double earthMassKg = 5.9722e24;
        public const double solMassKg = 1.98847e30;
        public const double gravitationalConstant = 6.67430e-11; // cubic meters / (kilograms * seconds squared)

        // Fleet Carrier Constants
        public const int carrierJumpSeconds = 72; // 72 seconds from when the jump is engaged to when it completes.
        public const int carrierPostJumpSeconds = 290; // 4 minutes 50 seconds cool down
        public const int carrierLandingPadLockdownSeconds = 180; // Landing pads are locked down 3 minutes prior to jumping
    }

    public static class ConstantConverters
    {
        // NB We can take advantage of the 'lifted' operators in Nullable<T> to have the compiler do our null-checking for us

        /// <summary> Convert gravity in m/s to g </summary>
        public static decimal ms2g(decimal gravity) => gravity / Constants.earthGravityMetersPerSecondSquared;

        /// <summary> Convert meters to astronomical units (AU) </summary>
        public static decimal? meters2au(decimal? meters) => meters / Constants.astronomicalUnitsMeters;

        /// <summary> Convert meters to light seconds </summary>
        public static decimal? meters2ls(decimal? meters) => meters / Constants.lightSpeedMetersPerSecond;

        /// <summary> Convert astronomical units (AU) to light seconds </summary>
        public static decimal? au2ls(decimal? au) => au * Constants.astronomicalUnitsMeters / Constants.lightSpeedMetersPerSecond;

        /// <summary> Convert pressure in Pascals to Earth atmospheres (atm) </summary>
        public static decimal? pascals2atm(decimal? pressure) => pressure / Constants.earthPressurePascals;

        /// <summary> Convert seconds to days -- WARNING do not use for calendar date/time calculations </summary>
        public static decimal? seconds2days(decimal? seconds) => seconds / 86400;
    }
}

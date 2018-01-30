﻿using System;

namespace Utilities
{
    /// <summary>
    ///  Various constants used throughout the codebase
    /// </summary>
    public class Constants
    {
        public const string EDDI_NAME = "EDDI";
        public const string EDDI_VERSION = "2.4.6-b3";
        public const string EDDI_SERVER_URL = "http://edcd.github.io/EDDP/";

        public static readonly string DATA_DIR = Environment.GetEnvironmentVariable("AppData") + "\\" + EDDI_NAME;
        public static readonly string USER_CONCURRENCY_TOKEN = EDDI_SERVER_URL + "/EDDI/" + Environment.GetEnvironmentVariable("UserName");

        public const string ENVIRONMENT_WITCH_SPACE = "Witch space";
        public const string ENVIRONMENT_SUPERCRUISE = "Supercruise";
        public const string ENVIRONMENT_NORMAL_SPACE = "Normal space";

        public const string VEHICLE_SHIP = "Ship";
        public const string VEHICLE_SRV = "SRV";
        public const string VEHICLE_FIGHTER = "Fighter";

        // Physical Constants
        public const int lightSpeedMetersPerSecond = 299792458;
        public const int solarRadiusMeters = 695500000;
        public const double solAbsoluteMagnitude = 4.83;
        public const double solLuminosity = 3.846e26;
        public const double stefanBoltzmann = 5.670367e-8;
    }
}

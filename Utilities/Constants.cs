using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    /// <summary>
    ///  Various constants used throughout the codebase
    /// </summary>
    public class Constants
    {
        public const string EDDI_NAME = "EDDI";
        public const string EDDI_VERSION = "2.4.0-b3";
        public static readonly string DATA_DIR = Environment.GetEnvironmentVariable("AppData") + "\\" + EDDI_NAME;

        public const string ENVIRONMENT_WITCH_SPACE = "Witch space";
        public const string ENVIRONMENT_SUPERCRUISE = "Supercruise";
        public const string ENVIRONMENT_NORMAL_SPACE = "Normal space";

        public const string VEHICLE_SHIP = "Ship";
        public const string VEHICLE_SRV = "SRV";
        public const string VEHICLE_FIGHTER = "Fighter";
    }
}

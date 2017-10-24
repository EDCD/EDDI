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
        public const string EDDI_VERSION = "2.3.0";
        public static readonly string DATA_DIR = Environment.GetEnvironmentVariable("AppData") + "\\" + EDDI_NAME;

        public static readonly string ENVIRONMENT_WITCH_SPACE = I18N.GetString("wicth_space");
        public static readonly string ENVIRONMENT_SUPERCRUISE = I18N.GetString("supercruise");
        public static readonly string ENVIRONMENT_NORMAL_SPACE = I18N.GetString("normal_space");

        public static readonly string VEHICLE_SHIP = I18N.GetString("ship");
        public static readonly string VEHICLE_SRV = I18N.GetString("srv");
        public static readonly string VEHICLE_FIGHTER = I18N.GetString("fighter");
    }
}

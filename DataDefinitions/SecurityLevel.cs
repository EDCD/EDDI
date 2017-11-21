using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Security levels
    /// </summary>
    public class SecurityLevel
    {
        private static readonly List<SecurityLevel> SECURITYLEVELS = new List<SecurityLevel>();

        public string name { get; private set; }

        public string edname { get; private set; }

        private SecurityLevel(string edname, string name)
        {
            this.edname = edname;
            this.name = name;
            
            SECURITYLEVELS.Add(this);
        }

        public static readonly SecurityLevel Anarchy = new SecurityLevel("$GAlAXY_MAP_INFO_state_anarchy;", "None");
        public static readonly SecurityLevel Lawless = new SecurityLevel("$GALAXY_MAP_INFO_state_lawless;", "None");
        public static readonly SecurityLevel None = new SecurityLevel("$SYSTEM_SECURITY_none;", "None");
        public static readonly SecurityLevel Low = new SecurityLevel("$SYSTEM_SECURITY_low;", "Low");
        public static readonly SecurityLevel Medium = new SecurityLevel("$SYSTEM_SECURITY_medium;", "Medium");
        public static readonly SecurityLevel High = new SecurityLevel("$SYSTEM_SECURITY_high;", "High");
        public static readonly SecurityLevel High_Anarchy = new SecurityLevel("$SYSTEM_SECURITY_high_anarchy;", "High");

        public static SecurityLevel FromName(string from)
        {
            if (from == null)
            {
                return null;
            }

            SecurityLevel result = SECURITYLEVELS.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown Security Level name " + from);
            }
            return result;
        }

        public static SecurityLevel FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }

            string tidiedFrom = from.ToLowerInvariant();
            SecurityLevel result = SECURITYLEVELS.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown Security Level ED name " + from);
                result = new SecurityLevel(from, tidiedFrom);
            }
            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousDataDefinitions
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

        public static readonly SecurityLevel None = new SecurityLevel("None", "None");
        public static readonly SecurityLevel Low = new SecurityLevel("Low", "Low");
        public static readonly SecurityLevel Medium = new SecurityLevel("$SYSTEM_SECURITY_medium", "Medium");
        public static readonly SecurityLevel High = new SecurityLevel("High", "High");

        public static SecurityLevel FromName(string from)
        {
            SecurityLevel result = SECURITYLEVELS.First(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown Security Level name " + from);
            }
            return result;
        }

        public static SecurityLevel FromEDName(string from)
        {
            SecurityLevel result = SECURITYLEVELS.First(v => v.edname == from);
            if (result == null)
            {
                Logging.Report("Unknown Security Level ED name " + from);
            }
            return result;
        }
    }
}

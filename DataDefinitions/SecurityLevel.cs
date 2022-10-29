
namespace EddiDataDefinitions
{
    /// <summary>
    /// Security levels
    /// </summary>
    public class SecurityLevel : ResourceBasedLocalizedEDName<SecurityLevel>
    {
        static SecurityLevel()
        {
            resourceManager = Properties.SecurityLevels.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new SecurityLevel(edname);

            None = new SecurityLevel("None");
            Low = new SecurityLevel("$SYSTEM_SECURITY_low;");
            Medium = new SecurityLevel("$SYSTEM_SECURITY_medium;");
            High = new SecurityLevel("$SYSTEM_SECURITY_high;");
            High_Anarchy = new SecurityLevel("$SYSTEM_SECURITY_high_anarchy;");
            Anarchy = new SecurityLevel("$GAlAXY_MAP_INFO_state_anarchy;");
            Lawless = new SecurityLevel("$GALAXY_MAP_INFO_state_lawless;");
        }

        public static readonly SecurityLevel None;
        public static readonly SecurityLevel Low;
        public static readonly SecurityLevel Medium;
        public static readonly SecurityLevel High;
        public static readonly SecurityLevel High_Anarchy;
        public static readonly SecurityLevel Anarchy;
        public static readonly SecurityLevel Lawless;

        // dummy used to ensure that the static constructor has run
        public SecurityLevel() : this("")
        { }

        private SecurityLevel(string edname) : base(edname, edname.Replace("$", "").Replace(";", ""))
        { }
    }
}

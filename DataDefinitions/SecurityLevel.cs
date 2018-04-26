
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
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new SecurityLevel(edname);

            None = new SecurityLevel("$SYSTEM_SECURITY_none;");
            var Low = new SecurityLevel("$SYSTEM_SECURITY_low;");
            var Medium = new SecurityLevel("$SYSTEM_SECURITY_medium;");
            var High = new SecurityLevel("$SYSTEM_SECURITY_high;");
            var High_Anarchy = new SecurityLevel("$SYSTEM_SECURITY_high_anarchy;");
            var Anarchy = new SecurityLevel("$GAlAXY_MAP_INFO_state_anarchy;");
            var Lawless = new SecurityLevel("$GALAXY_MAP_INFO_state_lawless;");
        }

        public static readonly SecurityLevel None;

        // dummy used to ensure that the static constructor has run
        public SecurityLevel() : this("")
        { }

        private SecurityLevel(string edname) : base(edname, edname.Replace("$", "").Replace(";", ""))
        {}
    }
}

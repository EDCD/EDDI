namespace EddiDataDefinitions
{
    /// <summary>
    /// Superpowers
    /// </summary>
    public class Superpower : ResourceBasedLocalizedEDName<Superpower>
    {
        static Superpower()
        {
            resourceManager = Properties.Superpowers.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new Superpower(edname);

            None = new Superpower("$faction_None"); // no semicolon in EDName
            Federation = new Superpower("$faction_Federation;");
            Alliance = new Superpower("$faction_Alliance;");
            Empire = new Superpower("$faction_Empire;");
            Independent = new Superpower("$faction_Independent;");
            PilotsFederation = new Superpower("$faction_PilotsFederation;");
            Pirate = new Superpower("$faction_Pirate;");
            Guardian = new Superpower("$faction_Guardian;");
            Thargoid = new Superpower("$faction_Thargoid;");
        }

        public static readonly Superpower None;
        public static readonly Superpower Federation;
        public static readonly Superpower Alliance;
        public static readonly Superpower Empire;
        public static readonly Superpower Independent;
        public static readonly Superpower PilotsFederation;
        public static readonly Superpower Pirate;
        public static readonly Superpower Guardian;
        public static readonly Superpower Thargoid;

        // dummy used to ensure that the static constructor has run
        public Superpower() : this("")
        { }

        private Superpower(string edname) : base(edname, edname.Replace("$faction_", "").Replace(";", "").Replace(" ", ""))
        { }

        public static Superpower FromNameOrEdName(string from)
        {
            if (from == null)
            {
                return None;
            }

            Superpower result = FromName(from);
            if (result == null && from.StartsWith("$faction_"))
            {
                result = FromEDName(from);
            }
            return result;
        }
    }
}

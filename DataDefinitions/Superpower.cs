
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

            None = new Superpower("$faction_none;");
            var Federation = new Superpower("$faction_Federation;");
            var Alliance = new Superpower("$faction_Alliance;");
            var Empire = new Superpower("$faction_Empire;");
            var Independent = new Superpower("$faction_Independent;");
            var PilotsFederation = new Superpower("$faction_PilotsFederation;");
            var Pirate = new Superpower("$faction_Pirate;");
            var Guardian = new Superpower("$faction_Guardian;");
            var Thargoid = new Superpower("$faction_Thargoid;");
        }

        public static readonly Superpower None;

        // dummy used to ensure that the static constructor has run, using a default value of "None"
        public Superpower() : this("$faction_none;")
        { }

        private Superpower(string edname) : base(edname, edname.Replace("$faction_", "").Replace(";", "").Replace(" ", ""))
        { }

        public static Superpower FromNameOrEdName(string from)
        {
            if (from == null)
            {
                return null;
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


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
            var Pirate = new Superpower("$faction_Pirate;");
        }

        public static readonly Superpower None;

        // dummy used to ensure that the static constructor has run
        public Superpower() : this("")
        { }

        private Superpower(string edname) : base(edname, edname.Replace("$faction_", "").Replace(";", ""))
        { }

        public static Superpower From(string from)
        {
            if (from == null)
            {
                return null;
            }

            Superpower result = FromName(from) ?? FromEDName(from);
            return result;
        }
    }
}


namespace EddiDataDefinitions
{
    /// <summary>
    /// Economy types
    /// </summary>
    public class Happiness : ResourceBasedLocalizedEDName<Happiness>
    {
        static Happiness()
        {
            resourceManager = Properties.Happiness.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new Happiness(edname);

            None = new Happiness("");
            HappinessBand1 = new Happiness("$Faction_HappinessBand1");
            HappinessBand2 = new Happiness("$Faction_HappinessBand2");
            HappinessBand3 = new Happiness("$Faction_HappinessBand3");
            HappinessBand4 = new Happiness("$Faction_HappinessBand4");
            HappinessBand5 = new Happiness("$Faction_HappinessBand5");
        }

        public static readonly Happiness None;
        public static readonly Happiness HappinessBand1;
        public static readonly Happiness HappinessBand2;
        public static readonly Happiness HappinessBand3;
        public static readonly Happiness HappinessBand4;
        public static readonly Happiness HappinessBand5;

        // dummy used to ensure that the static constructor has run
        public Happiness () : this("")
        { }

        private Happiness(string edname) : base(edname, edname.Replace("$Faction_", "").Replace(";", ""))
        { }
    }
}

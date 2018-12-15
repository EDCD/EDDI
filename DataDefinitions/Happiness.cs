
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

            var HappinessBand1 = new Happiness("$Faction_HappinessBand1");
            var HappinessBand2 = new Happiness("$Faction_HappinessBand2");
            var HappinessBand3 = new Happiness("$Faction_HappinessBand3");
            var HappinessBand4 = new Happiness("$Faction_HappinessBand4");
            var HappinessBand5 = new Happiness("$Faction_HappinessBand5");
        }

        public static readonly Happiness None = new Happiness("");

        // dummy used to ensure that the static constructor has run
        public Happiness() : this("")
        {}

        private Happiness(string edname) : base(edname, edname.Replace("$Faction_", "").Replace(";", ""))
        {}
    }
}

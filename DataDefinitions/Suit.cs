using System.Linq;

namespace EddiDataDefinitions
{
    // Player suits
    public class Suit : ResourceBasedLocalizedEDName<Suit>
    {
        static Suit()
        {
            resourceManager = Properties.Suit.ResourceManager;
            resourceManager.IgnoreCase = true;
        }

        // dummy used to ensure that the static constructor has run
        public Suit() : this("")
        { }

        public Suit(string edname) : base(edname, edname)
        { }

        public static readonly Suit ExplorationSuit = new Suit("ExplorationSuit");
        public static readonly Suit FlightSuit = new Suit("FlightSuit");
        public static readonly Suit TacticalSuit = new Suit("TacticalSuit");
        public static readonly Suit UtilitySuit = new Suit("UtilitySuit");

        public int grade { get; private set; }

        public new static Suit FromEDName(string edname)
        {
            if (string.IsNullOrEmpty(edname)) { return null; }
            string tidiedName = edname.ToLowerInvariant().Replace("$", "").Replace(";", "").Replace("_name", "");
            if (int.TryParse(tidiedName.Last().ToString(), out var grade))
            {
                tidiedName = tidiedName.Replace("_class" + grade, "");
            }
            var result = ResourceBasedLocalizedEDName<Suit>.FromEDName(tidiedName);
            if (result != null)
            {
                result.grade = grade == 0 ? 1 : grade; // Always at least grade 1
            }
            return result;
        }
    }
}

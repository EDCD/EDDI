using System;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    // Player suits
    public class Suit ( string edname ) : ResourceBasedLocalizedEDName<Suit>( edname, edname )
    {
        static Suit()
        {
            resourceManager = Properties.Suit.ResourceManager;
            resourceManager.IgnoreCase = true;
        }

        // dummy used to ensure that the static constructor has run
        public Suit() : this("")
        { }

        public static readonly Suit ExplorationSuit = new("ExplorationSuit");
        public static readonly Suit FlightSuit = new("FlightSuit");
        public static readonly Suit TacticalSuit = new("TacticalSuit");
        public static readonly Suit UtilitySuit = new("UtilitySuit");

        [PublicAPI]
        public int grade { get; private set; }

        public long? suitId { get; private set; }

        public static Suit FromEDName(string edname, long? suitId = null)
        {
            if (string.IsNullOrEmpty(edname)) { return null; }
            var (tidiedName, grade) = titiedEDName(edname);
            var result = ResourceBasedLocalizedEDName<Suit>.FromEDName(tidiedName);
            if (result != null) { result.grade = grade; result.suitId = suitId; }
            return result;
        }

        public static bool EDNameExists(string edName)
        {
            if (edName == null) { return false; }
            return AllOfThem.Any(v => string.Equals(v.edname, titiedEDName(edName).Item1, StringComparison.InvariantCultureIgnoreCase));
        }

        private static (string, int) titiedEDName(string edName)
        {
            var tidiedName = edName?.ToLowerInvariant().Replace("$", "").Replace(";", "").Replace("_name", "");
            if (int.TryParse(tidiedName?.Last().ToString(), out var grade))
            {
                tidiedName = tidiedName?.Replace("_class" + grade, "");
            }
            grade = grade == 0 ? 1 : grade; // Always at least grade 1
            return (tidiedName, grade);
        }
    }
}

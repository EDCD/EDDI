using System;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    // Player suits
    public class Suit ( string edname, Manufacturer manufacturer, uint price ) : ResourceBasedLocalizedEDName<Suit>( edname, edname )
    {
        static Suit()
        {
            resourceManager = Properties.Suit.ResourceManager;
            resourceManager.IgnoreCase = true;
        }

        // dummy used to ensure that the static constructor has run
        public Suit() : this( "", null, 0 )
        { }

        public static readonly Suit ExplorationSuit = new( "ExplorationSuit", Manufacturer.Supratech, 150000 );
        public static readonly Suit FlightSuit = new( "FlightSuit", Manufacturer.Remlok, 0 );
        public static readonly Suit TacticalSuit = new( "TacticalSuit", Manufacturer.Manticore, 150000 );
        public static readonly Suit UtilitySuit = new( "UtilitySuit", Manufacturer.Remlok, 150000 );

        [PublicAPI( "The space suit's grade" )]
        public int grade { get; set; }

        [PublicAPI( "The space suit's manufacturer, as an object" )]
        public Manufacturer manufacturer { get; private set; } = manufacturer;

        [PublicAPI( "The space suit's standard grade 1 price" )]
        public uint price { get; } = price;

        public ulong? suitId { get; private set; }

        public static Suit FromEDName(string edname, ulong? suitId = null)
        {
            if (string.IsNullOrEmpty(edname)) { return null; }
            var (tidiedName, grade) = tidiedEDName(edname);
            var result = ResourceBasedLocalizedEDName<Suit>.FromEDName(tidiedName);
            if (result != null) { result.grade = grade; result.suitId = suitId; }
            return result;
        }

        public static bool EDNameExists(string edName)
        {
            if (edName == null) { return false; }
            return AllOfThem.Any(v => string.Equals(v.edname, tidiedEDName(edName).Item1, StringComparison.InvariantCultureIgnoreCase));
        }

        private static (string, int) tidiedEDName(string edName)
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

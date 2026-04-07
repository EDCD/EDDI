using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SuitSoldEvent ( DateTime timestamp, Suit suit, int? price, List<SuitMod> mods ) 
        : Event( timestamp, NAME )
    {
        public const string NAME = "Suit sold";
        public const string DESCRIPTION = "Triggered when you sell a space suit";
        public static readonly string[] SAMPLES = {
            @"{ ""timestamp"":""2025-02-03T08:59:16Z"", ""event"":""SellSuit"", ""SuitID"":1700534679057635, ""SuitMods"":[ ""suit_increasedshieldregen"" ], ""Name"":""tacticalsuit_class3"", ""Name_Localised"":""$TacticalSuit_Class1_Name;"", ""Price"":825000 }",
            @"{ ""timestamp"":""2021-09-05T04:56:23Z"", ""event"":""SellSuit"", ""SuitID"":1701699758193117, ""SuitMods"":[ ""suit_improvedarmourrating"" ], ""Name"":""explorationsuit_class3"", ""Name_Localised"":""$ExplorationSuit_Class1_Name;"", ""Price"":825000 }"
        };

        [PublicAPI("The space suit, as an object")]
        public Suit suit { get; } = suit;

        [PublicAPI( @"The space suit's grade" )]
        public int grade { get; } = suit?.grade ?? 1;

        [PublicAPI("The space suit's sell price")]
        public int? price { get; } = price;

        [PublicAPI( @"The suit's modifications (as objects)" )]
        public List<SuitMod> mods { get; } = mods ?? [];

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var edname = JsonParsing.getString(data, "Name");
            var fallbackName = JsonParsing.getString(data, "Name_Localised");
            var suitId = JsonParsing.getOptionalULong(data, "SuitID");
            var price = JsonParsing.getOptionalInt(data, "Price");
            var suit = Suit.FromEDName(edname, suitId);
            suit.fallbackLocalizedName = fallbackName?.StartsWith( '$' ) ?? false ? null : fallbackName;
            var mods = new List<SuitMod>();
            if ( data.TryGetValue( "SuitMods", out var suitModsVal ) )
            {
                var suitMods = ( suitModsVal as List<object> )?.Cast<string>()?.ToList() ?? [ ];
                foreach ( var modEdName in suitMods )
                {
                    mods.Add( SuitMod.FromEDName( modEdName ) );
                }
            }

            events.Add( new SuitSoldEvent( timestamp, suit, price, mods ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}

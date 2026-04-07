using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SuitUpgradedEvent ( DateTime timestamp, Suit suit, int? price ) 
        : Event( timestamp, NAME )
    {
        public const string NAME = "Suit upgraded";
        public const string DESCRIPTION = "Triggered when upgrading a space suit to a new grade";
        public static readonly string[] SAMPLES = {
            @"{ ""timestamp"":""2026-01-17T10:27:57Z"", ""event"":""UpgradeSuit"", ""Name"":""tacticalsuit_class4"", ""Name_Localised"":""$TacticalSuit_Class1_Name;"", ""SuitID"":1701686761199080, ""Class"":5, ""Cost"":7500000, ""Resources"":[ { ""Name"":""suitschematic"", ""Name_Localised"":""Suit Schematic"", ""Count"":5 }, { ""Name"":""healthmonitor"", ""Name_Localised"":""Health Monitor"", ""Count"":5 }, { ""Name"":""manufacturinginstructions"", ""Name_Localised"":""Manufacturing Instructions"", ""Count"":5 }, { ""Name"":""titaniumplating"", ""Name_Localised"":""Titanium Plating"", ""Count"":12 }, { ""Name"":""graphene"", ""Count"":12 } ] }",
            @"{ ""timestamp"":""2025-01-12T02:29:52Z"", ""event"":""UpgradeSuit"", ""Name"":""utilitysuit_class4"", ""Name_Localised"":""$UtilitySuit_Class1_Name;"", ""SuitID"":1701623800559528, ""Class"":5, ""Cost"":7500000, ""Resources"":[ { ""Name"":""suitschematic"", ""Name_Localised"":""Suit Schematic"", ""Count"":5 }, { ""Name"":""healthmonitor"", ""Name_Localised"":""Health Monitor"", ""Count"":5 }, { ""Name"":""manufacturinginstructions"", ""Name_Localised"":""Manufacturing Instructions"", ""Count"":5 }, { ""Name"":""carbonfibreplating"", ""Name_Localised"":""Carbon Fibre Plating"", ""Count"":12 }, { ""Name"":""graphene"", ""Count"":12 } ] }",
            @"{ ""timestamp"":""2021-09-05T11:50:50Z"", ""event"":""UpgradeSuit"", ""Name"":""explorationsuit_class3"", ""Name_Localised"":""$ExplorationSuit_Class1_Name;"", ""SuitID"":1700532312591455, ""Class"":4, ""Cost"":4500000 }"
       };

        [PublicAPI("The space suit, as an object")]
        public Suit suit { get; } = suit;

        [ PublicAPI( @"The space suit's new grade" ) ]
        public int grade { get; } = suit?.grade ?? 1;

        [PublicAPI("The space suit's upgrade price")]
        public int? price { get; } = price;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var edname = JsonParsing.getString(data, "Name");
            var fallbackName = JsonParsing.getString(data, "Name_Localised");
            var suitId = JsonParsing.getOptionalULong(data, "SuitID");
            var grade = JsonParsing.getInt( data, "Class" );
            var price = JsonParsing.getOptionalInt(data, "Price");
            var suit = Suit.FromEDName(edname, suitId);
            suit.fallbackLocalizedName = fallbackName?.StartsWith( '$' ) ?? false ? null : fallbackName;
            suit.grade = grade;
            // No need to worry about spent resources, a separate event will keep our microresource inventory up to date

            events.Add( new SuitUpgradedEvent( timestamp, suit, price ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}

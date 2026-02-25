using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipInterdictionEvent : Event
    {
        public const string NAME = "Ship interdiction";
        public const string DESCRIPTION = "Triggered when you interdict another ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-09-21T07:00:17Z\",\"event\":\"Interdiction\",\"Success\":true,\"Interdicted\":\"Torval's Shield\",\"IsPlayer\":false,\"Faction\":\"Zemina Torval\",\"Power\":\"Empire\"}";

        [PublicAPI("If the interdiction attempt was successful")]
        public bool succeeded { get; private set; }

        [PublicAPI("If the player being interdicted is a commander (as opposed to an NPC)")]
        public bool iscommander { get; private set; }

        [PublicAPI("The name of the commander being interdicted")]
        public string interdictee { get; private set; }

        [PublicAPI("The combat rating of the commander being interdicted")]
        public string rating { get; private set; }

        [PublicAPI("The faction of the commander being interdicted")]
        public string faction { get; private set; }

        [PublicAPI("The power of the commander being interdicted")]
        public string power { get; private set; }
        
        public ShipInterdictionEvent(DateTime timestamp, bool succeeded, bool iscommander, string interdictee, CombatRating rating, string faction, string power) : base(timestamp, NAME)
        {
            this.succeeded = succeeded;
            this.iscommander = iscommander;
            this.interdictee = interdictee;
            this.rating = rating?.localizedName;
            this.faction = faction;
            this.power = power;
        }

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var success = JsonParsing.getBool(data, "Success");
            var interdictee = JsonParsing.getString(data, "Interdicted");
            var iscommander = JsonParsing.getBool(data, "IsPlayer");
            data.TryGetValue( "CombatRank", out var val );
            var rating = val == null ? null : CombatRating.FromRank( Convert.ToInt32( val ) );
            var faction = EventParsing.FactionName(data, "Faction");
            var power = JsonParsing.getString(data, "Power");

            if ( !string.IsNullOrEmpty( JsonParsing.getString( data, "Interdicted_Localised" ) ) )
            {
                // This is an NPC with a symbolic name
                interdictee = NpcAuthorityShip.EDNameExists( interdictee )
                    ? NpcAuthorityShip.FromEDName( interdictee )?.localizedName
                    : JsonParsing.getString( data, "Interdicted_Localised" );
            }

            events.Add( new ShipInterdictionEvent( timestamp, success, iscommander, interdictee, rating, faction, power ) { raw = line, fromLoad = false } );
            return true;
        }
    }
}

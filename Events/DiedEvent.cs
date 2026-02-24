using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class DiedEvent : Event
    {
        public const string NAME = "Died";
        public const string DESCRIPTION = "Triggered when you have died";
        public const string SAMPLE = @"{ ""timestamp"":""2016-12-29T10:15:26Z"", ""event"":""Died"", ""KillerName"":""$ShipName_Military_Federation;"", ""KillerName_Localised"":""Federal Navy Ship"", ""KillerShip"":""viper"", ""KillerRank"":""Deadly"" }";

        [PublicAPI("A list of objects describing your killers")]
        public List<Killer> killers { get; private set; }

        public DiedEvent(DateTime timestamp, List<Killer> killers) : base(timestamp, NAME)
        {
            this.killers = killers;
        }

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            Killer parseKiller ( IDictionary<string, object> killerData, bool singleKiller )
            {
                // Property names differ if there is a single killer vs. multiple killers
                var name = JsonParsing.getString(killerData, singleKiller ? "KillerName" : "Name");
                if ( !string.IsNullOrEmpty( JsonParsing.getString( data, singleKiller ? "KillerName_Localised" : "Name_Localised" ) ) )
                {
                    // This is an NPC with a symbolic name
                    name = NpcAuthorityShip.EDNameExists( name )
                        ? NpcAuthorityShip.FromEDName( name )?.localizedName
                        : JsonParsing.getString( data, singleKiller ? "KillerName_Localised" : "Name_Localised" );
                }

                var equipment = JsonParsing.getString(killerData, singleKiller ? "KillerShip" : "Ship"); // May be a ship, a suit, etc.
                var rating = CombatRating.FromEDName(JsonParsing.getString(killerData, singleKiller ? "KillerRank" : "Rank"));
                return new Killer( name, equipment, rating );
            }

            var killers = new List<Killer>();
            if ( data.ContainsKey( "KillerName" ) )
            {
                // Single killer
                killers.Add( parseKiller( data, true ) );
            }
            if ( data.ContainsKey( "killers" ) )
            {
                // Multiple killers
                data.TryGetValue( "Killers", out var val );
                var killersData = (List<object>)val;
                if ( killersData != null )
                {
                    foreach ( var killerData in killersData.Cast<IDictionary<string, object>>() )
                    {
                        killers.Add( parseKiller( killerData, false ) );
                    }
                }
            }
            events.Add( new DiedEvent( timestamp, killers ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}

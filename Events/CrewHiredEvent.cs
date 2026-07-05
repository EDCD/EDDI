using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CrewHiredEvent (
        DateTime timestamp,
        string name,
        long crewid,
        string faction,
        long price,
        CombatRating combatrating )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Crew hired";
        public const string DESCRIPTION = "Triggered when you hire crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"CrewHire\",\"Name\":\"Margaret Parrish\",\"CrewID\":236064708,\"Faction\":\"The Dark Wheel\",\"Cost\":15000,\"CombatRank\":1}";

        [PublicAPI("The name of the crewmember being hired")]
        public string name { get; private set; } = name;

        [PublicAPI("The ID of the crewmember being hired")]
        public long crewid { get; private set; } = crewid;

        [PublicAPI("The faction of the crewmember being hired")]
        public string faction { get; private set; } = faction;

        [PublicAPI("The price of the crewmember being hired")]
        public long price { get; private set; } = price;

        [PublicAPI("The combat rating of the crewmember being hired")]
        public string combatrating { get; private set; } = combatrating.localizedName;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var name = JsonParsing.getString(data, "Name");
            var crewid = JsonParsing.getLong(data, "CrewID");
            var faction = EventParsing.FactionName(data, "Faction");
            var price = JsonParsing.getLong(data, "Cost");
            var rating = CombatRating.FromRank(JsonParsing.getInt(data, "CombatRank"));
            events.Add( new CrewHiredEvent( timestamp, name, crewid, faction, price, rating ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class BondAwardedEvent ( DateTime timestamp, string awardingfaction, string victimfaction, long reward )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Bond awarded";
        public const string DESCRIPTION = "Triggered when you are awarded a combat bond";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"FactionKillBond\",\"Reward\":500,\"AwardingFaction\":\"Jarildekald Public Industry\",\"VictimFaction\":\"Lencali Freedom Party\"}";

        [PublicAPI("The name of the faction awarding the bond")]
        public string awardingfaction { get; private set; } = awardingfaction;

        [PublicAPI("The name of the faction whose ship you destroyed")]
        public string victimfaction { get; private set; } = victimfaction;

        [PublicAPI("The number of credits received")]
        public long reward { get; private set; } = reward;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var reward = JsonParsing.getLong( data, "Reward" );
            var victimFaction = EventParsing.FactionName(data, "VictimFaction");
            var awardingFaction = EventParsing.FactionName(data, "AwardingFaction");
            events.Add( new BondAwardedEvent( timestamp, awardingFaction, victimFaction, reward ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}

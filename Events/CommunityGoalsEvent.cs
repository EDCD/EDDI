using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommunityGoalsEvent ( DateTime timestamp, List<CommunityGoal> goals ) : Event( timestamp, NAME )
    {
        public const string NAME = "Community goals";
        public const string DESCRIPTION = "Triggered when checking the status of community goals";
        public const string SAMPLE = "{ \"timestamp\":\"2017-08-14T13:20:28Z\", \"event\":\"CommunityGoal\", \"CurrentGoals\":[ { \"CGID\":726, \"Title\":\"Alliance Research Initiative – Trade\", \"SystemName\":\"Kaushpoos\", \"MarketName\":\"Neville Horizons\", \"Expiry\":\"2017-08-17T14:58:14Z\", \"IsComplete\":false, \"CurrentTotal\":10062, \"PlayerContribution\":562, \"NumContributors\":101, \"TopRankSize\":10, \"PlayerInTopRank\":false, \"TierReached\":\"Tier 1\", \"PlayerPercentileBand\":50, \"Bonus\":200000 } ] }";

        // Not intended to be user facing

        public List<CommunityGoal> goals { get; } = goals;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            // There may be multiple goals in each event.
            data.TryGetValue( "CurrentGoals", out var goalsVal );
            var goalsJson = JsonConvert.SerializeObject(goalsVal);
            var goals = JsonConvert.DeserializeObject<List<CommunityGoal>>(goalsJson);
            events.Add( new CommunityGoalsEvent( timestamp, goals ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}

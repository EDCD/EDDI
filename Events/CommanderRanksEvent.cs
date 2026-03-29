using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommanderRatingsEvent (
        DateTime timestamp,
        CombatRating combat,
        TradeRating trade,
        ExplorationRating exploration,
        CQCRating cqc,
        EmpireRating empire,
        FederationRating federation,
        MercenaryRating mercenary,
        ExobiologistRating exobiologist )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Commander ratings";
        public const string DESCRIPTION = "Triggered when your ratings are reported";
        public const string SAMPLE = "{ \"timestamp\":\"2021-05-21T02:17:37Z\", \"event\":\"Rank\", \"Combat\":7, \"Trade\":8, \"Explore\":8, \"Soldier\":0, \"Exobiologist\":0, \"Empire\":14, \"Federation\":14, \"CQC\":2 }";

        [PublicAPI("The commander's combat rating (this is an object)")]
        public CombatRating combat { get; private set; } = combat;

        [PublicAPI("The commander's trading rating (this is an object)")]
        public TradeRating trade { get; private set; } = trade;

        [PublicAPI("The commander's exploration rating (this is an object)")]
        public ExplorationRating exploration { get; private set; } = exploration;

        [PublicAPI("The commander's CQC rating (this is an object)")]
        public CQCRating cqc { get; private set; } = cqc;

        [PublicAPI("The commander's empire rating (this is an object)")]
        public EmpireRating empire { get; private set; } = empire;

        [PublicAPI("The commander's federation rating (this is an object)")]
        public FederationRating federation { get; private set; } = federation;

        [PublicAPI("The commander's mercenary rating  (this is an object)")]
        public MercenaryRating mercenary { get; private set; } = mercenary;

        [PublicAPI("The commander's exobiologist rating  (this is an object)")]
        public ExobiologistRating exobiologist { get; private set; } = exobiologist;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            data.TryGetValue( "Combat", out var val );
            var combat = CombatRating.FromRank((int)((long?)val ?? 0));
            data.TryGetValue( "Trade", out val );
            var trade = TradeRating.FromRank((int)((long?)val ?? 0));
            data.TryGetValue( "Explore", out val );
            var exploration = ExplorationRating.FromRank((int)((long?)val ?? 0));
            data.TryGetValue( "CQC", out val );
            var cqc = CQCRating.FromRank((int)((long?)val ?? 0));
            data.TryGetValue( "Empire", out val );
            var empire = EmpireRating.FromRank((int)((long?)val ?? 0));
            data.TryGetValue( "Federation", out val );
            var federation = FederationRating.FromRank((int)((long?)val ?? 0));
            data.TryGetValue( "Soldier", out val );
            var mercenary = MercenaryRating.FromRank((int)((long?)val ?? 0));
            data.TryGetValue( "Exobiologist", out val );
            var exobiologist = ExobiologistRating.FromRank((int)((long?)val ?? 0));

            events.Add( new CommanderRatingsEvent( timestamp, combat, trade, exploration, cqc, empire, federation, mercenary, exobiologist ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommanderProgressEvent (
        DateTime timestamp,
        decimal combat,
        decimal trade,
        decimal exploration,
        decimal cqc,
        decimal empire,
        decimal federation,
        decimal mercenary,
        decimal exobiologist )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Commander progress";
        public const string DESCRIPTION = "Triggered when your progress is reported";
        public const string SAMPLE = "{ \"timestamp\":\"2021-05-21T02:17:37Z\", \"event\":\"Progress\", \"Combat\":66, \"Trade\":100, \"Explore\":100, \"Soldier\":0, \"Exobiologist\":0, \"Empire\":100, \"Federation\":100, \"CQC\":87 }";

        [PublicAPI("The percentage progress of the commander's combat rating")]
        public decimal combat { get; private set; } = combat;

        [PublicAPI("The percentage progress of the commander's trade rating")]
        public decimal trade { get; private set; } = trade;

        [PublicAPI("The percentage progress of the commander's exploration rating")]
        public decimal exploration { get; private set; } = exploration;

        [PublicAPI("The percentage progress of the commander's CQC rating")]
        public decimal cqc { get; private set; } = cqc;

        [PublicAPI("The percentage progress of the commander's empire rating")]
        public decimal empire { get; private set; } = empire;

        [PublicAPI("The percentage progress of the commander's federation rating")]
        public decimal federation { get; private set; } = federation;

        [PublicAPI("The percentage progress of the commander's mercenary rating")]
        public decimal mercenary { get; private set; } = mercenary;

        [PublicAPI("The percentage progress of the commander's exobiologist rating")]
        public decimal exobiologist { get; private set; } = exobiologist;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            data.TryGetValue( "Combat", out var val );
            decimal combat = (long?)val ?? 0;
            data.TryGetValue( "Trade", out val );
            decimal trade = (long?)val ?? 0;
            data.TryGetValue( "Explore", out val );
            decimal exploration = (long?)val ?? 0;
            data.TryGetValue( "CQC", out val );
            decimal cqc = (long?)val ?? 0;
            data.TryGetValue( "Empire", out val );
            decimal empire = (long?)val ?? 0;
            data.TryGetValue( "Federation", out val );
            decimal federation = (long?)val ?? 0;
            data.TryGetValue( "Soldier", out val );
            decimal soldier = (long?)val ?? 0;
            data.TryGetValue( "Exobiologist", out val );
            decimal exobiologist = (long?)val ?? 0;

            events.Add( new CommanderProgressEvent( timestamp, combat, trade, exploration, cqc, empire, federation, soldier, exobiologist ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}

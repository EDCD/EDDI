using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommanderReputationEvent (
        DateTime timestamp,
        decimal empire,
        decimal federation,
        decimal independent,
        decimal alliance )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Commander reputation";
        public const string DESCRIPTION = "Triggered when your reputation is reported";
        public const string SAMPLE = "{ \"timestamp\":\"2019-08-07T06:38:38Z\", \"event\":\"Reputation\", \"Empire\":75.000000, \"Federation\":96.557602, \"Independent\":3.346750, \"Alliance\":75.000000 }";

        [PublicAPI("The percentage progress of the commander's empire superpower reputation")]
        public decimal empire { get; private set; } = empire;

        [PublicAPI("The percentage progress of the commander's federation superpower reputation")]
        public decimal federation { get; private set; } = federation;

        [PublicAPI("The percentage progress of the commander's independent faction reputation")]
        public decimal independent { get; private set; } = independent;

        [PublicAPI("The percentage progress of the commander's alliance superpower reputation")]
        public decimal alliance { get; private set; } = alliance;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var empire = JsonParsing.getOptionalDecimal(data, "Empire") ?? 0;
            var federation = JsonParsing.getOptionalDecimal(data, "Federation") ?? 0;
            var independent = JsonParsing.getOptionalDecimal(data, "Independent") ?? 0;
            var alliance = JsonParsing.getOptionalDecimal(data, "Alliance") ?? 0;
            events.Add( new CommanderReputationEvent( timestamp, empire, federation, independent, alliance ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}

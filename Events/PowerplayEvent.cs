using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class PowerplayEvent : Event
    {
        public const string NAME = "Powerplay";
        public const string DESCRIPTION = "Triggered periodically when pledged to a power";
        public const string SAMPLE = "{ \"timestamp\":\"2018-01-31T10:53:04Z\", \"event\":\"Powerplay\", \"Power\":\"Edmund Mahon\", \"Rank\":0, \"Merits\":10, \"Votes\":0, \"TimePledged\":433024 }";

        [PublicAPI("The name of the power to whom you are pledged")]
        public string power => Power?.localizedName;

        [PublicAPI("Your rank with the power")]
        public int rank { get; private set; }

        [PublicAPI("Your merits with the power")]
        public int merits { get; private set; }
        
        [PublicAPI("The amount of time that you've been pledged, in days")]
        public int timepledgeddays => (int)Math.Floor(timePledged.TotalDays);

        [PublicAPI("The amount of time that you've been pledged, in weeks")]
        public decimal timepledgedweeks => (decimal)Math.Round((double)timepledgeddays / 7, 1);

        // Not intended to be user facing

        public Power Power { get; private set; }
        
        public TimeSpan timePledged { get; private set; }

        public PowerplayEvent ( DateTime timestamp, Power Power, int rank, int merits, TimeSpan timePledged ) : base(timestamp, NAME)
        {
            this.Power = Power;
            this.rank = rank;
            this.merits = merits;
            this.timePledged = timePledged;
        }

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
            var rank = JsonParsing.getInt(data, "Rank") + 1; // This is zero based in the journal but not in the Frontier API. Adding +1 here synchronizes the two.
            var merits = JsonParsing.getInt(data, "Merits");
            var timePledged = TimeSpan.FromSeconds(JsonParsing.getLong(data, "TimePledged"));
            events.Add( new PowerplayEvent( timestamp, power, rank, merits, timePledged ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}


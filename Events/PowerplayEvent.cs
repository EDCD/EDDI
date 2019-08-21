using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class PowerplayEvent : Event
    {
        public const string NAME = "Powerplay";
        public const string DESCRIPTION = "Triggered periodically when pledged to a power";
        public const string SAMPLE = "{ \"timestamp\":\"2018-01-31T10:53:04Z\", \"event\":\"Powerplay\", \"Power\":\"Edmund Mahon\", \"Rank\":0, \"Merits\":10, \"Votes\":0, \"TimePledged\":433024 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static PowerplayEvent()
        {
            VARIABLES.Add("power", "The name of the power to whom you are pledged");
            VARIABLES.Add("rank", "Your rank with the power");
            VARIABLES.Add("merits", "Your merits with the power");
            VARIABLES.Add("timepledgeddays", "The amount of time that you've been pledged, in days");
            VARIABLES.Add("timepledgedweeks", "The amount of time that you've been pledged, in weeks");
        }

        public string power => Power?.localizedName;
        public int rank { get; private set; }
        public int merits { get; private set; }
        public int votes { get; private set; }
        public int timepledgeddays => (int)Math.Floor(timePledged.TotalDays);
        public decimal timepledgedweeks => (decimal)Math.Round((double)timepledgeddays / 7, 1);

        // Not intended to be user facing
        public Power Power { get; private set; }
        public TimeSpan timePledged { get; private set; }

        public PowerplayEvent(DateTime timestamp, Power Power, int rank, int merits, int votes, TimeSpan timePledged) : base(timestamp, NAME)
        {
            this.Power = Power;
            this.rank = rank;
            this.merits = merits;
            this.votes = votes;
            this.timePledged = timePledged;
        }
    }
}


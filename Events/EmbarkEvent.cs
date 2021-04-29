using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class EmbarkEvent : Event
    {
        public const string NAME = "Embark";
        public const string DESCRIPTION = "Triggered when you transition from on foot to a ship or SRV";
        public const string SAMPLE = "{ \"timestamp\":\"2020-10-12T09:06:17Z\", \"event\":\"Embark\", \"SRV\":false, \"Taxi\":false, \"Multicrew\":false, \"ID\":36 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static EmbarkEvent()
        {
            VARIABLES.Add("tomulticrew", "True if embarking to another player's ship");
            VARIABLES.Add("toship", "True if embarking to your own ship");
            VARIABLES.Add("tosrv", "True if embarking to an SRV");
            VARIABLES.Add("totaxi", "True if embarking to a taxi transport ship");
        }

        [PublicAPI]
        public bool tomulticrew { get; }

        [PublicAPI]
        public bool toship => toLocalId != null && !tosrv && !totaxi && !tomulticrew;

        [PublicAPI]
        public bool tosrv { get; }

        [PublicAPI]
        public bool totaxi { get; }

        // Not intended to be user facing
        public int? toLocalId { get; }

        public EmbarkEvent(DateTime timestamp, bool toSRV, bool toTaxi, bool toMultiCrew, int? toLocalId) : base(timestamp, NAME)
        {
            this.tosrv = toSRV;
            this.totaxi = toTaxi;
            this.tomulticrew = toMultiCrew;
            this.toLocalId = toLocalId;
        }
    }
}

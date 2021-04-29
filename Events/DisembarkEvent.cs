using JetBrains.Annotations;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class DisembarkEvent : Event
    {
        public const string NAME = "Disembark";
        public const string DESCRIPTION = "Triggered when you transition from a ship or SRV to on foot";
        public const string SAMPLE = "{ \"timestamp\":\"2020-10-12T09:09:55Z\", \"event\":\"Disembark\", \"SRV\":false, \"Taxi\":false, \"Multicrew\":false, \"ID\":36 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DisembarkEvent()
        {
            VARIABLES.Add("frommulticrew", "True if disembarking from another player's ship");
            VARIABLES.Add("fromship", "True if disembarking from your own ship");
            VARIABLES.Add("fromsrv", "True if disembarking from an SRV");
            VARIABLES.Add("fromtaxi", "True if disembarking from a taxi transport ship");
        }

        [PublicAPI]
        public bool frommulticrew { get; }

        [PublicAPI]
        public bool fromship => fromLocalId != null && !fromsrv && !fromtaxi && !frommulticrew;

        [PublicAPI]
        public bool fromsrv { get; }

        [PublicAPI]
        public bool fromtaxi { get; }

        // Not intended to be user facing
        public int? fromLocalId { get; }

        public DisembarkEvent(DateTime timestamp, bool fromSRV, bool fromTaxi, bool fromMultiCrew, int? fromLocalId) : base(timestamp, NAME)
        {
            this.fromsrv = fromSRV;
            this.fromtaxi = fromTaxi;
            this.frommulticrew = fromMultiCrew;
            this.fromLocalId = fromLocalId;
        }
    }
}
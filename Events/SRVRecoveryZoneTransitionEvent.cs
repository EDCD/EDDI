using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class SRVRecoveryZoneTransitionEvent : Event
    {
        public const string NAME = "SRV recovery zone transition";
        public const string DESCRIPTION = "Triggered when your SRV enters or leaves the recovery zone around your ship";
        public static SRVRecoveryZoneTransitionEvent SAMPLE = new SRVRecoveryZoneTransitionEvent(DateTime.Now, true);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SRVRecoveryZoneTransitionEvent()
        {
            VARIABLES.Add("entering", "A boolean value. True if you are entering the space under your ship");
        }

        bool entering { get; set; }

        public SRVRecoveryZoneTransitionEvent(DateTime timestamp, bool entering) : base(timestamp, NAME)
        {
            this.entering = entering;
        }
    }
}

using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class SRVLaunchedEvent : Event
    {
        public const string NAME = "SRV launched";
        public const string DESCRIPTION = "Triggered when you launch an SRV from your ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"LaunchSRV\",\"Loadout\":\"starter\",\"PlayerControlled\":true}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SRVLaunchedEvent()
        {
            VARIABLES.Add("loadout", "The SRV's loadout");
            VARIABLES.Add("playercontrolled", "True if the SRV is controlled by the player");
        }

        public string loadout { get; private set; }

        public bool playercontrolled { get; private set; }

        public SRVLaunchedEvent(DateTime timestamp, string loadout, bool playercontrolled) : base(timestamp, NAME)
        {
            this.loadout = loadout;
            this.playercontrolled = playercontrolled;
        }
    }
}

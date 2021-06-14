using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SRVLaunchedEvent : Event
    {
        public const string NAME = "SRV launched";
        public const string DESCRIPTION = "Triggered when you launch an SRV from your ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"LaunchSRV\",\"Loadout\":\"starter\",\"PlayerControlled\":true}";

        [PublicAPI("The SRV's loadout")]
        public string loadout { get; private set; }

        [PublicAPI("True if the SRV is controlled by the player")]
        public bool playercontrolled { get; private set; }

        [PublicAPI("The vehicle ID assigned to the SRV")]
        public int? id { get; private set; }

        public SRVLaunchedEvent(DateTime timestamp, string loadout, bool playercontrolled, int? id) : base(timestamp, NAME)
        {
            this.loadout = loadout;
            this.playercontrolled = playercontrolled;
            this.id = id;
        }
    }
}

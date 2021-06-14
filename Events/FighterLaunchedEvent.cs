using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class FighterLaunchedEvent : Event
    {
        public const string NAME = "Fighter launched";
        public const string DESCRIPTION = "Triggered when you launch a fighter from your ship";
        public const string SAMPLE = "{\"timestamp\":\"2019-04-29T00:07:46Z\", \"event\":\"LaunchFighter\", \"Loadout\":\"four\", \"ID\":13, \"PlayerControlled\":false}";

        [PublicAPI("The fighter's loadout")]
        public string loadout { get; private set; }

        [PublicAPI("The fighter's id")]
        public int id { get; private set; }

        [PublicAPI("True if the fighter is controlled by the player")]
        public bool playercontrolled { get; private set; }

        public FighterLaunchedEvent(DateTime timestamp, string loadout, int id, bool playercontrolled) : base(timestamp, NAME)
        {
            this.loadout = loadout;
            this.id = id;
            this.playercontrolled = playercontrolled;
        }
    }
}

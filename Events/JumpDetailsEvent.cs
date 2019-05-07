using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class JumpDetailsEvent : Event
    {
        public const string NAME = "Jump details";
        public const string DESCRIPTION = "Triggered when ship jump details are requested";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static JumpDetailsEvent()
        {
            VARIABLES.Add("jumprange", "Single jump distance, limited by max jump range, total mass and current fuel on board");
            VARIABLES.Add("fuelrange", "Total jump distance, using all current fuel on board");
            VARIABLES.Add("jumpsremaining", "Number of jumps remaining, using all current fuel on board");
            VARIABLES.Add("maxfuelrange", "Total jump distance, when completely fueled");
            VARIABLES.Add("maxjumps", "Total number of jumps, when completely fueled");
        }

        public decimal jumprange { get; private set; }

        public decimal fuelrange { get; private set; }

        public int jumpsremaining { get; private set; }

        public decimal maxfuelrange { get; private set; }

        public int maxjumps { get; private set; }

        public JumpDetailsEvent(DateTime timestamp, decimal jumprange, decimal fuelrange, int jumpsremaining, decimal maxfuelrange, int maxjumps) : base(timestamp, NAME)
        {
            this.jumprange = jumprange;
            this.fuelrange = fuelrange;
            this.maxjumps = maxjumps;
        }
    }
}

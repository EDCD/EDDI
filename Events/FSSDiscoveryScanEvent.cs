using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class FSSDiscoveryScanEvent : Event
    {
        public const string NAME = "FSS discovery scan";
        public const string DESCRIPTION = "Triggered when performing a full system scan (honk)";
        public const string SAMPLE = "{ \"timestamp\":\"2018-11-04T23:44:36Z\", \"event\":\"FSSDiscoveryScan\", \"Progress\":0.824540, \"BodyCount\":4, \"NonBodyCount\":8 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FSSDiscoveryScanEvent()
        {
            VARIABLES.Add("progress", "The percentage of the system that has been discovered");
            VARIABLES.Add("bodies", "The total number of stellar bodies in the system");
            VARIABLES.Add("nonbodies", "The number of non-body signals");
        }

        public decimal progress { get; private set; }
        public int bodies { get; private set; }
        public int nonbodies { get; private set; }

        public FSSDiscoveryScanEvent(DateTime timestamp, decimal progress, int bodies, int nonbodies) : base(timestamp, NAME)
        {
            this.progress = Math.Floor(progress * 100); // multiplied by 100 to convert to percentage
            this.bodies = bodies;
            this.nonbodies = nonbodies;
        }
    }
}

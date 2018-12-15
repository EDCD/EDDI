using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class BodyMappedEvent : Event
    {
        public const string NAME = "Body mapped";
        public const string DESCRIPTION = "Triggered after mapping a body with the Surface Area Analysis scanner";
        public const string SAMPLE = @"{ ""timestamp"":""2018-10-05T15:06:12Z"", ""event"":""SAAScanComplete"", ""BodyName"":""Eranin 5"", ""BodyID"":5, ""ProbesUsed"":6, ""EfficiencyTarget"":9 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BodyMappedEvent()
        {
            VARIABLES.Add("name", "The name of the body that has been mapped");
            VARIABLES.Add("probesused", "The number of probes used to map the body");
            VARIABLES.Add("efficiencytarget", "The efficiency target for the number of probes used to map the body");
        }

        public string name { get; private set; }

        public int probesused { get; private set; }

        public int efficiencytarget { get; private set; }

        public BodyMappedEvent(DateTime timestamp, string name, int probesUsed, int efficiencyTarget) : base(timestamp, NAME)
        {
            this.name = name;
            this.probesused = probesUsed;
            this.efficiencytarget = efficiencyTarget;
        }

    }
}
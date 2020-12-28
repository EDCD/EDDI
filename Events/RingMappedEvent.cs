using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class RingMappedEvent : Event
    {
        public const string NAME = "Ring mapped";
        public const string DESCRIPTION = "Triggered after mapping a ring with the Surface Area Analysis scanner";
        public const string SAMPLE = @"{ ""timestamp"":""2019-06-23T03:16:29Z"", ""event"":""SAAScanComplete"", ""BodyName"":""Kopernik A 3 A Ring"", ""BodyID"":16, ""SystemAddress"":42324886570137, ""ProbesUsed"":1, ""EfficiencyTarget"":0 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static RingMappedEvent()
        {
            VARIABLES.Add("ringname", "The name of the ring that was mapped");
            VARIABLES.Add("reserves", "The reserve level of the ring that was mapped, if the parent body has already been scanned");
            VARIABLES.Add("composition", "The composition of the ring that was mapped (Icy, Rocky, Metallic, etc.)");
            VARIABLES.Add("mass", "The mass of the ring that was mapped, in megatonnes");
            VARIABLES.Add("innerradius", "The inner radius of the ring that was mapped, in kilometers");
            VARIABLES.Add("outerradius", "The outer radius of the ring that was mapped, in kilometers");
            VARIABLES.Add("probesused", "The number of probes used to map the ring");
        }

        public string ringname { get; private set; }

        public string reserves => body?.reserveLevel?.localizedName;

        public string composition => ring?.localizedComposition;

        public decimal? mass => ring?.mass;

        public decimal? innerradius => ring?.innerradius;

        public decimal? outerradius => ring?.outerradius;

        public int probesused { get; private set; }

        public int efficiencytarget { get; private set; }

        // Not intended to be user facing
        public Ring ring { get; private set; }
        public Body body { get; private set; }
        public long? systemAddress { get; private set; }

        public RingMappedEvent(DateTime timestamp, string ringName, Ring ring, Body body, long? systemAddress, int probesUsed, int efficiencyTarget) : base(timestamp, NAME)
        {
            this.ringname = ringName;
            this.ring = ring;
            this.body = body;
            this.systemAddress = systemAddress;
            this.probesused = probesUsed;
            this.efficiencytarget = efficiencyTarget;
        }
    }
}

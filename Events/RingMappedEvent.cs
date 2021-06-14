using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class RingMappedEvent : Event
    {
        public const string NAME = "Ring mapped";
        public const string DESCRIPTION = "Triggered after mapping a ring with the Surface Area Analysis scanner";
        public const string SAMPLE = @"{ ""timestamp"":""2019-06-23T03:16:29Z"", ""event"":""SAAScanComplete"", ""BodyName"":""Kopernik A 3 A Ring"", ""BodyID"":16, ""SystemAddress"":42324886570137, ""ProbesUsed"":1, ""EfficiencyTarget"":0 }";

        [PublicAPI("The name of the ring that was mapped")]
        public string ringname { get; private set; }

        [PublicAPI("The reserve level of the ring that was mapped, if the parent body has already been scanned")]
        public string reserves => body?.reserveLevel?.localizedName;

        [PublicAPI("The composition of the ring that was mapped (Icy, Rocky, Metallic, etc.)")]
        public string composition => ring?.localizedComposition;

        [PublicAPI("The mass of the ring that was mapped, in megatonnes")]
        public decimal? mass => ring?.mass;

        [PublicAPI("The inner radius of the ring that was mapped, in kilometers")]
        public decimal? innerradius => ring?.innerradius;

        [PublicAPI("The outer radius of the ring that was mapped, in kilometers")]
        public decimal? outerradius => ring?.outerradius;

        [PublicAPI("The number of probes used to map the ring")]
        public int probesused { get; private set; }

        [PublicAPI("Use less probes than indicated to achieve a mapping efficiency bonus")]
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

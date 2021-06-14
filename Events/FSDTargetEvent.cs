using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class FSDTargetEvent : Event
    {
        public const string NAME = "Next jump";
        public const string DESCRIPTION = "Triggered when selecting a star system to jump to";
        public const string SAMPLE = @"{ ""timestamp"":""2020-11-14T09:19:25Z"", ""event"":""FSDTarget"", ""Name"":""Musca Dark Region CQ-Y d31"", ""SystemAddress"":1075729926531, ""StarClass"":""F"", ""RemainingJumpsInRoute"":1 }";

        [PublicAPI("The name of the destination system")]
        public string system { get; private set; }

        [PublicAPI("The remaining number of jumps in the current route")]
        public int remainingjumpsinroute { get; private set; }

        [PublicAPI("The primary star's class")]
        public string starclass { get; private set; }

        // Not intended to be user facing

        public long systemAddress { get; private set; }

        public FSDTargetEvent(DateTime timestamp, string system, long systemAddress, int remainingjumpsinroute, string starclass) : base(timestamp, NAME)
        {
            this.system = system;
            this.systemAddress = systemAddress;
            this.remainingjumpsinroute = remainingjumpsinroute;
            this.starclass = starclass;
        }
    }
}

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

        [PublicAPI("The name of the targeted system")]
        public string system { get; private set; }

        [PublicAPI( "The numeric system address of the targeted star system" )]
        public ulong systemAddress { get; private set; }

        [PublicAPI("The remaining number of jumps in the current route")]
        public int remainingjumpsinroute { get; private set; }

        [PublicAPI("The primary star's class from the targeted star system")]
        public string starclass { get; private set; }

        // Not intended to be user facing

        public FSDTargetEvent(DateTime timestamp, string system, ulong systemAddress, int remainingjumpsinroute, string starclass) : base(timestamp, NAME)
        {
            this.system = system;
            this.systemAddress = systemAddress;
            this.remainingjumpsinroute = remainingjumpsinroute;
            this.starclass = starclass;
        }
    }
}

using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SystemScanComplete : Event
    {
        public const string NAME = "System scan complete";
        public const string DESCRIPTION = "Triggered after having identified all bodies in the system";
        public const string SAMPLE = @"{""timestamp"":""2019-03-10T16:09:36Z"", ""event"":""FSSAllBodiesFound"", ""SystemName"":""Dumbae DN-I d10-6057"", ""SystemAddress"":208127228285531, ""Count"":19 }";

        [PublicAPI("The name of the scanned system")]
        public string systemname { get; private set; }

        [PublicAPI( "The numeric system address of the scanned star system" )]
        public ulong systemAddress { get; private set; }

        [PublicAPI("The count of bodies from the scanned system")]
        public int count { get; private set; }

        public SystemScanComplete(DateTime timestamp, string systemname, ulong systemAddress, int count) : base(timestamp, NAME)
        {
            this.systemname = systemname;
            this.systemAddress = systemAddress;
            this.count = count;

        }
    }
}

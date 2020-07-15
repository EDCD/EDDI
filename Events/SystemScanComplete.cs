using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class SystemScanComplete : Event
    {
        public const string NAME = "System scan complete";
        public const string DESCRIPTION = "Triggered after having identified all bodies in the system";
        public const string SAMPLE = @"{""timestamp"":""2019-03-10T16:09:36Z"", ""event"":""FSSAllBodiesFound"", ""SystemName"":""Dumbae DN-I d10-6057"", ""SystemAddress"":208127228285531, ""Count"":19 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SystemScanComplete()
        {
            VARIABLES.Add("systemname", "The name of the scanned system");
            VARIABLES.Add("count", "The count of bodies from the scanned system");
        }

        public string systemname { get; private set; }

        public int count { get; private set; }

        // Not intended to be user facing

        [VoiceAttackIgnore]
        public long systemAddress { get; private set; }

        public SystemScanComplete(DateTime timestamp, string systemname, long systemAddress, int count) : base(timestamp, NAME)
        {
            this.systemname = systemname;
            this.systemAddress = systemAddress;
            this.count = count;

        }
    }
}

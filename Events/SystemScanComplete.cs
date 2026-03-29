using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SystemScanComplete ( DateTime timestamp, string systemname, ulong systemAddress, int count )
        : Event( timestamp, NAME )
    {
        public const string NAME = "System scan complete";
        public const string DESCRIPTION = "Triggered after having identified all bodies in the system";
        public const string SAMPLE = @"{""timestamp"":""2019-03-10T16:09:36Z"", ""event"":""FSSAllBodiesFound"", ""SystemName"":""Dumbae DN-I d10-6057"", ""SystemAddress"":208127228285531, ""Count"":19 }";

        [PublicAPI("The name of the scanned system")]
        public string systemname { get; private set; } = systemname;

        [PublicAPI( "The numeric system address of the scanned star system" )]
        public ulong systemAddress { get; private set; } = systemAddress;

        [PublicAPI("The count of bodies from the scanned system")]
        public int count { get; private set; } = count;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var systemName = JsonParsing.getString(data, "SystemName");
            var systemAddress = JsonParsing.getULong(data, "SystemAddress");
            var count = JsonParsing.getInt(data, "Count");
            events.Add( new SystemScanComplete( timestamp, systemName, systemAddress, count ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}

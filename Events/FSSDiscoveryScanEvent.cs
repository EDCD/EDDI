using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class DiscoveryScanEvent : Event
    {
        public const string NAME = "Discovery scan";
        public const string DESCRIPTION = "Triggered when performing a full system scan (honk)";
        public const string SAMPLE = "{ \"timestamp\":\"2018-11-04T23:44:36Z\", \"event\":\"FSSDiscoveryScan\", \"Progress\":0.824540, \"BodyCount\":4, \"NonBodyCount\":8 }";

        [PublicAPI("the total number of discoverable bodies within the system")]
        public int totalbodies { get; private set; }

        [PublicAPI("the number of non-body signals")]
        public int nonbodies { get; private set; }

        // Not intended to be user facing

        [Obsolete("Please use `totalbodies` instead")]
        public int bodies => totalbodies;

        // "progress" is omitted because it only reports discoveries made prior to the discovery scan 
        // and excludes progress which is a direct result of the scan itself.

        public int progress { get; private set; }

        public DiscoveryScanEvent(DateTime timestamp, decimal progress, int totalbodies, int nonbodies) : base(timestamp, NAME)
        {
            this.progress = (int)Math.Round(progress * 100); // multiplied by 100 to convert to percentage
            this.totalbodies = totalbodies;
            this.nonbodies = nonbodies;
        }

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            // A journal error was introduced in Odyssey 4.0 Update 17
            // where FSSDiscoveryScan would report zero bodies and infinite progress.
            // Don't parse `Progress` unless `BodyCount` is greater than zero.

            var bodyCount = JsonParsing.getInt(data, "BodyCount"); // total number of stellar bodies in system
            var nonBodyCount = JsonParsing.getInt(data, "NonBodyCount"); // total number of non-body signals found
            var progress = bodyCount > 0 ? JsonParsing.getDecimal(data, "Progress") : 0; // value from 0-1
            events.Add( new DiscoveryScanEvent( timestamp, progress, bodyCount, nonBodyCount ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}

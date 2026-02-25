using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class NavBeaconScanEvent : Event
    {
        public const string NAME = "Nav beacon scan";
        public const string DESCRIPTION = "Triggered when you scan a nav beacon, before the scan data for all the bodies in the system is written into the journal";
        public const string SAMPLE = "{ \"timestamp\":\"2017-08-24T16:50:31Z\", \"event\":\"NavBeaconScan\", \"NumBodies\":3 }";

        [PublicAPI("The number of bodies included in the scan dump")]
        public int numbodies { get; private set; }

        [PublicAPI( "The numeric system address of the star system where the nav beacon is located" )]
        public ulong systemAddress { get; private set; }

        public NavBeaconScanEvent(DateTime timestamp, ulong systemAddress, int numbodies) : base(timestamp, NAME)
        {
            this.systemAddress = systemAddress;
            this.numbodies = numbodies;
        }

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading
            var systemAddress = JsonParsing.getULong(data, "SystemAddress");
            var numbodies = JsonParsing.getInt( data, "NumBodies" );
            events.Add( new NavBeaconScanEvent( timestamp, systemAddress, numbodies ) { raw = line, fromLoad = false } );
            return true;
        }
    }
}

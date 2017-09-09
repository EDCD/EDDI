using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class NavBeaconScanEvent : Event
    {
        public const string NAME = "Nav beacon scan";
        public const string DESCRIPTION = "Triggered when you scan a nav beacon, before the scan data for all the bodies in the system is written into the journal";
        public const string SAMPLE = "{ \"timestamp\":\"2017-08-24T16:50:31Z\", \"event\":\"NavBeaconScan\", \"NumBodies\":3 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static NavBeaconScanEvent()
        {
            VARIABLES.Add("numbodies", "The number of bodies included in the scan dump");
        }

        [JsonProperty("numbodies")]
        public int numbodies { get; private set; }

        public NavBeaconScanEvent(DateTime timestamp, int numbodies) : base(timestamp, NAME)
        {
            this.numbodies = numbodies;
        }
    }
}

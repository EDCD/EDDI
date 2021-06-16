using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class DataScannedEvent : Event
    {
        public const string NAME = "Data scanned";
        public const string DESCRIPTION = "Triggered when scanning some types of data links";
        public const string SAMPLE = "{ \"timestamp\":\"2017-08-26T01:58:24Z\", \"event\":\"DataScanned\", \"Type\":\"DataLink\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DataScannedEvent()
        {
            VARIABLES.Add("datalinktype", "The type of Data Link scanned");
        }

        public string datalinktype { get; private set; }

        public DataScannedEvent(DateTime timestamp, DataScan datalinktype) : base(timestamp, NAME)
        {
            this.datalinktype = datalinktype.localizedName;
        }
    }
}

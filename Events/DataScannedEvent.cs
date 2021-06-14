using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class DataScannedEvent : Event
    {
        public const string NAME = "Data scanned";
        public const string DESCRIPTION = "Triggered when scanning some types of data links";
        public const string SAMPLE = "{ \"timestamp\":\"2017-08-26T01:58:24Z\", \"event\":\"DataScanned\", \"Type\":\"DataLink\" }";

        [PublicAPI("The type of Data Link scanned")]
        public string datalinktype { get; private set; }

        public DataScannedEvent(DateTime timestamp, DataScan datalinktype) : base(timestamp, NAME)
        {
            this.datalinktype = datalinktype.localizedName;
        }
    }
}

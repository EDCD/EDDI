using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierNameChangeEvent : Event
    {
        public const string NAME = "Carrier name changed";
        public const string DESCRIPTION = "Triggered when your fleet carrier’s name is changed";
        public const string SAMPLE = "{ \"timestamp\": \"2020-05-15T20:54:37Z\", \"event\": \"CarrierNameChange\", \"CarrierID\": 3700571136, \"Name\": \"GARY TOBAI\", \"Callsign\": \"G53-K3Q\" }";

        [PublicAPI("The callsign (alphanumeric designation) of your fleet carrier")]
        public string callsign { get; private set; }

        [PublicAPI("The new name of your fleet carrier")]
        public string name { get; private set; }

        // Not intended to be user facing
        public long? carrierID { get; private set; }

        public CarrierNameChangeEvent(DateTime timestamp, long? carrierId, string callsign, string name) : base(timestamp, NAME)
        {
            carrierID = carrierId;
            this.callsign = callsign;
            this.name = name;
        }
    }
}
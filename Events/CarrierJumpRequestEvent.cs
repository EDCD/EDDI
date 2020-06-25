using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CarrierJumpRequestEvent : Event
    {
        public const string NAME = "Carrier jump request";
        public const string DESCRIPTION = "Triggered when you request that your fleet carrier performs a jump";
        public const string SAMPLE = "{ \"timestamp\":\"2020-05-11T18:56:09Z\", \"event\":\"CarrierJumpRequest\", \"CarrierID\":3700357376, \"SystemName\":\"Hemang\", \"Body\":\"Hemang A 2 a\", \"SystemAddress\":4756709905082, \"BodyID\":7 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CarrierJumpRequestEvent()
        {
            // System variables
            VARIABLES.Add("systemname", "The name of the destination star system");

            // Body variables
            VARIABLES.Add("bodyname", "The name of the destination body, if any");
            VARIABLES.Add("shortname", "The short name of the destination body, if any");
        }

        // System variables

        public string systemname { get; private set; }

        // Body variables

        public string bodyname { get; private set; }

        public string shortname => (string.IsNullOrEmpty(systemname) || string.IsNullOrEmpty(bodyname) || bodyname == systemname)
            ? bodyname
            : bodyname.Replace(systemname, "").Trim();

        // These properties are not intended to be user facing
        public long? systemAddress { get; private set; }
        public long? bodyId { get; private set; }
        public long? carrierId { get; private set; }

        public CarrierJumpRequestEvent(DateTime timestamp, string systemName, long systemAddress, string bodyName, long? bodyId, long? carrierId) : base(timestamp, NAME)
        {
            // System
            this.systemname = systemName;
            this.systemAddress = systemAddress;

            // Body
            this.bodyname = bodyName;
            this.bodyId = bodyId;

            // Carrier
            this.carrierId = carrierId;
        }
    }
}
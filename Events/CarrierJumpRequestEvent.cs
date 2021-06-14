using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierJumpRequestEvent : Event
    {
        public const string NAME = "Carrier jump request";
        public const string DESCRIPTION = "Triggered when you request that your fleet carrier performs a jump";
        public const string SAMPLE = "{ \"timestamp\":\"2020-05-11T18:56:09Z\", \"event\":\"CarrierJumpRequest\", \"CarrierID\":3700357376, \"SystemName\":\"Hemang\", \"Body\":\"Hemang A 2 a\", \"SystemAddress\":4756709905082, \"BodyID\":7 }";

        // System variables

        [PublicAPI("The name of the destination star system")]
        public string systemname { get; private set; }

        // Body variables

        [PublicAPI("The name of the destination body, if any")]
        public string bodyname { get; private set; }

        [PublicAPI("The short name of the destination body, if any")]
        public string shortname => Body.GetShortName(bodyname, systemname);

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
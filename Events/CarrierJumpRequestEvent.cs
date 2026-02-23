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

        [PublicAPI( "The numeric system address of the destination star system" )]
        public ulong systemAddress { get; private set; }

        // Body variables

        [PublicAPI("The name of the destination body, if any")]
        public string bodyname { get; private set; }

        [PublicAPI( "The numeric ID of the destination body, if any" )]
        public long? bodyId { get; private set; }

        [PublicAPI("The short name of the destination body, if any")]
        public string shortname => Body.GetShortName(bodyname, systemname);

        // Carrier variables

        [PublicAPI( "The carrier's numeric ID" )]
        public long carrierID { get; private set; }

        [PublicAPI( "The carrier type (e.g. Fleet Carrier or Squadron Carrier), as an object with 'localizedName' and 'invariantName' properties" )]
        public StationModel carrierType { get; private set; }
        
        // Not intended to be user facing
        
        public DateTime departureTime { get; private set; }

        public CarrierJumpRequestEvent ( DateTime timestamp, string systemName, ulong systemAddress, string bodyName,
            long? bodyId, long carrierId, StationModel carrierType, DateTime departureTime ) : base(timestamp, NAME)
        {
            // System
            this.systemname = systemName;
            this.systemAddress = systemAddress;

            // Body
            this.bodyname = bodyName;
            this.bodyId = bodyId;

            // Carrier
            this.carrierID = carrierId;
            this.carrierType = carrierType;
            this.departureTime = departureTime;
        }
    }
}
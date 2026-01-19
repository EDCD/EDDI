using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierJumpEngagedEvent : Event
    {
        public const string NAME = "Carrier jump engaged";
        public const string DESCRIPTION = "Triggered when your fleet carrier performs a jump";
        public static readonly CarrierJumpEngagedEvent[] SAMPLES = 
        {
            new CarrierJumpEngagedEvent(DateTime.UtcNow, "Aparctias", 358797513434, "Ageno", 18262335038849, "Aparctias", 0, 3700571136, StationModel.FleetCarrier ),
            new CarrierJumpEngagedEvent(DateTime.UtcNow, "Senones", 2724896688499, "Aparctias", 358797513434, "Senones 2", 3, 3707594240, StationModel.FleetCarrier )
        };

        // Destination System variables

        [PublicAPI("The name of the destination star system")]
        public string systemname { get; private set; }

        [PublicAPI( "The numeric system address of the destination star system" )]
        public ulong systemAddress { get; private set; }

        [PublicAPI( "Metadata encoded into the unique 64 bit ID for the star system." )]
        public StarSystemId64 id64 => new StarSystemId64( systemAddress );

        // Origin System variables

        [PublicAPI( "The name of the origin star system" )]
        public string originSystemName { get; private set; }

        [PublicAPI( "The numeric ID of the origin star system" )]
        public ulong originSystemAddress { get; private set; }

        [PublicAPI( "Metadata encoded into the unique 64 bit ID for the origin star system." )]
        public StarSystemId64 originId64 => new StarSystemId64( systemAddress );

        // Body variables

        [PublicAPI("The name of the destination body, if any")]
        public string bodyname { get; private set; }

        [PublicAPI( "The numeric ID of the destination body, if any" )]
        public long? bodyId { get; private set; }

        [PublicAPI("The short name of the destination body, if any")]
        public string shortname => Body.GetShortName(bodyname, systemname);

        // State variables

        [PublicAPI("True if docked with the carrier as it jumps")]
        public bool docked { get; set; }

        // Carrier variables

        [PublicAPI( "The carrier's numeric ID" )]
        public long carrierID { get; private set; }

        [PublicAPI( "The carrier type (e.g. Fleet Carrier or Squadron Carrier), as an object with 'localizedName' and 'invariantName' properties" )]
        public StationModel carrierType { get; private set; }

        public CarrierJumpEngagedEvent ( DateTime timestamp, string systemName, ulong systemAddress,
            string originSystemName, ulong originSystemAddress, string bodyName, long? bodyId, long carrierId,
            StationModel carrierType ) : base(timestamp, NAME)
        {
            // System
            this.systemname = systemName;
            this.systemAddress = systemAddress;
            this.originSystemName = originSystemName;
            this.originSystemAddress = originSystemAddress;

            // Body
            this.bodyname = bodyName;
            this.bodyId = bodyId;

            // Carrier
            this.carrierID = carrierId;
            this.carrierType = carrierType;
        }
    }
}
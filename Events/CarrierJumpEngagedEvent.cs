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
        public static CarrierJumpEngagedEvent SAMPLE = new CarrierJumpEngagedEvent(DateTime.UtcNow, "Aparctias", 358797513434, "Ageno", 18262335038849, "Aparctias", 0, 3700571136);

        // System variables

        [PublicAPI("The name of the destination star system")]
        public string systemname { get; private set; }

        // Body variables

        [PublicAPI("The name of the destination body, if any")]
        public string bodyname { get; private set; }

        [PublicAPI("The short name of the destination body, if any")]
        public string shortname => Body.GetShortName(bodyname, systemname);

        // State variables

        [PublicAPI("True if docked with the carrier as it jumps")]
        public bool docked { get; set; }

        // These properties are not intended to be user facing

        public ulong systemAddress { get; private set; }
        
        public long? bodyId { get; private set; }
        
        public long? carrierId { get; private set; }
        
        public string originSystemName { get; private set; }
        
        public ulong? originSystemAddress { get; private set; }

        public CarrierJumpEngagedEvent(DateTime timestamp, string systemName, ulong systemAddress, string originSystemName, ulong? originSystemAddress, string bodyName, long? bodyId, long? carrierId) : base(timestamp, NAME)
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
            this.carrierId = carrierId;
        }
    }
}
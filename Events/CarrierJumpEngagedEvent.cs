using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CarrierJumpEngagedEvent : Event
    {
        public const string NAME = "Carrier jump engaged";
        public const string DESCRIPTION = "Triggered when your fleet carrier performs a jump";
        public static CarrierJumpEngagedEvent SAMPLE = new CarrierJumpEngagedEvent(DateTime.UtcNow, "Aparctias", 358797513434, "Ageno", 18262335038849, "Aparctias", 0, 3700571136);

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CarrierJumpEngagedEvent()
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

        public string shortname => Body.GetShortName(bodyname, systemname);

        // These properties are not intended to be user facing
        public long? systemAddress { get; private set; }
        public long? bodyId { get; private set; }
        public long? carrierId { get; private set; }
        public string originSystemName { get; private set; }
        public long? originSystemAddress { get; private set; }

        public CarrierJumpEngagedEvent(DateTime timestamp, string systemName, long systemAddress, string originSystemName, long? originSystemAddress, string bodyName, long? bodyId, long? carrierId) : base(timestamp, NAME)
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
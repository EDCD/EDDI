using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CarrierCooldownEvent : Event
    {
        public const string NAME = "Carrier cooldown";
        public const string DESCRIPTION = "Triggered when you either were docked at a fleet carrier during a jump or are the fleet carrier owner and it completes its cooldown";
        public static CarrierCooldownEvent SAMPLE = new CarrierCooldownEvent(DateTime.UtcNow, "Aparctias", 358797513434, "Aparctias", 0, BodyType.FromEDName("Star"), "G53-K3Q", StationModel.FromEDName("FleetCarrier"), 3700571136);

        // System variables

        [PublicAPI("The name of the system in which the carrier is located after a successful jump")]
        public string systemname { get; private set; }

        // Body variables

        [PublicAPI("The nearest body to the carrier, if any, after a successful jump")]
        public string bodyname { get; private set; }

        [PublicAPI("The type of the body nearest to the carrier, if any (Star, Planet. etc.), after a successful jump  (only available if docked during the jump)")]
        public string bodytype => bodyType?.localizedName;

        [PublicAPI("The short name of the nearest body, if any, after a successful jump")]
        public string shortname => Body.GetShortName(bodyname, systemname);

        // Carrier variables

        [PublicAPI("The name of the carrier (only available if docked during a successful jump)")]
        public string carriername { get; private set; }

        // These properties are not intended to be user facing

        public long? systemAddress { get; private set; }

        public BodyType bodyType { get; private set; }

        public long? bodyId { get; private set; }

        public long? carrierId { get; private set; }

        public StationModel carrierType { get; private set; }

        public CarrierCooldownEvent(DateTime timestamp, string systemName, long? systemAddress, string bodyName, long? bodyId, BodyType bodyType, string carrierName, StationModel carrierType, long? carrierId) : base(timestamp, NAME)
        {
            // System
            this.systemname = systemName;
            this.systemAddress = systemAddress;

            // Body
            this.bodyname = bodyName;
            this.bodyId = bodyId;
            this.bodyType = bodyType ?? BodyType.None;

            // Carrier
            this.carrierId = carrierId;
            this.carriername = carrierName;
            this.carrierType = carrierType;
        }
    }
}
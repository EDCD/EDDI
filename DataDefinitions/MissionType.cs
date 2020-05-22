namespace EddiDataDefinitions
{
    /// <summary>
    /// Mission types
    /// </summary>
    public class MissionType : ResourceBasedLocalizedEDName<MissionType>
    {
        static MissionType()
        {
            resourceManager = Properties.MissionType.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new MissionType(edname);

            var Altruism = new MissionType("Altruism");
            var AltruismCredits = new MissionType("AltruismCredits");
            var Assassinate = new MissionType("Assassinate");
            var AssassinateWing = new MissionType("AssassinateWing");
            var Collect = new MissionType("Collect");
            var CollectWing = new MissionType("CollectWing");
            var CommunityGoal = new MissionType("CommunityGoal");
            var Courier = new MissionType("Courier");
            var Delivery = new MissionType("Delivery");
            var DeliveryWing = new MissionType("DeliveryWing");
            var Disable = new MissionType("Disable");
            var DisableWing = new MissionType("DisableWing");
            var GenericPermit1 = new MissionType("GenericPermit1");
            var Hack = new MissionType("Hack");
            var LongDistanceExpedition = new MissionType("LongDistanceExpedition");
            var Massacre = new MissionType("Massacre");
            var MassacreThargoid = new MissionType("MassacreThargoid");
            var MassacreWing = new MissionType("MassacreWing");
            var Mining = new MissionType("Mining");
            var MiningWing = new MissionType("MiningWing");
            var None = new MissionType("None");
            var PassengerBulk = new MissionType("PassengerBulk");
            var PassengerVIP = new MissionType("PassengerVIP");
            var Piracy = new MissionType("Piracy");
            var Rescue = new MissionType("Rescue");
            var Salvage = new MissionType("Salvage");
            var Scan = new MissionType("Scan");
            var Sightseeing = new MissionType("Sightseeing");
            var Smuggle = new MissionType("Smuggle");
            var Special = new MissionType("Special");
            var StartZone = new MissionType("StartZone");
        }

        // dummy used to ensure that the static constructor has run
        public MissionType() : this("")
        { }

        private MissionType(string edname) : base(edname, edname)
        { }
    }
}

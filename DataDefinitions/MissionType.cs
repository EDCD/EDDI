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

            _ = new MissionType("Altruism");
            _ = new MissionType("Credits"); // Donation type: credits
            _ = new MissionType("Assassinate");
            _ = new MissionType("AssassinateWing");
            _ = new MissionType("BlOps");
            _ = new MissionType("Chain");
            _ = new MissionType("Collect");
            _ = new MissionType("CollectWing");
            _ = new MissionType("CommunityGoal");
            _ = new MissionType("Courier");
            _ = new MissionType("Covert");
            _ = new MissionType("Delivery");
            _ = new MissionType("DeliveryWing");
            _ = new MissionType("Disable");
            _ = new MissionType("DisableMegaship");
            _ = new MissionType("DisableWing");
            _ = new MissionType("Founder");
            _ = new MissionType("GenericPermit1");
            _ = new MissionType("Hack");
            _ = new MissionType("HackMegaship");
            _ = new MissionType("LongDistanceExpedition");
            _ = new MissionType("Massacre");
            _ = new MissionType("MassacreThargoid");
            _ = new MissionType("MassacreWing");
            _ = new MissionType("Mining");
            _ = new MissionType("MiningWing");
            _ = new MissionType("None");
            _ = new MissionType("OnFoot");
            _ = new MissionType("Onslaught");
            _ = new MissionType("PassengerBulk");
            _ = new MissionType("PassengerVIP");
            _ = new MissionType("Piracy");
            _ = new MissionType("Planet");
            _ = new MissionType("Planetary");
            _ = new MissionType("RankEmp");
            _ = new MissionType("RankFed");
            _ = new MissionType("Rescue");
            _ = new MissionType("Sabotage");
            _ = new MissionType("Salvage");
            _ = new MissionType("Scan");
            _ = new MissionType("Sightseeing");
            _ = new MissionType("Smuggle");
            _ = new MissionType("Special");
            _ = new MissionType("StartZone");
            _ = new MissionType("Welcome");

            // Hack types
            _ = new MissionType("Download");
            _ = new MissionType("Upload");

            // Heist types
            _ = new MissionType("ProductionHeist");

            // Massacre target types
            _ = new MissionType("Skimmer");
            _ = new MissionType("ConflictCivilWar");
            _ = new MissionType("ConflictWar");

            // Mission difficulty types
            _ = new MissionType("Hard");
            _ = new MissionType("Illegal");
            _ = new MissionType("Legal");
            _ = new MissionType("NCD");

            // Odyssey settlement states
            _ = new MissionType("Offline");
            _ = new MissionType("Reboot");
            _ = new MissionType("RebootRestore");

            // Sabotage types
            _ = new MissionType("Power");
            _ = new MissionType("Production");

        }

        // dummy used to ensure that the static constructor has run
        public MissionType() : this("")
        { }

        private MissionType(string edname) : base(edname, edname)
        { }
    }
}

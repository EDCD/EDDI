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
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new MissionType(edname);

            _ = new MissionType("Altruism");
            _ = new MissionType("Assassination", true);
            _ = new MissionType("Assassinate", true);
            _ = new MissionType("AssassinateWing", true);
            _ = new MissionType("BlOps", true);
            _ = new MissionType("Collect", true);
            _ = new MissionType("CollectWing", true);
            _ = new MissionType("CommunityGoal");
            _ = new MissionType("Courier", true);
            _ = new MissionType("Credits"); // Donation type: credits
            _ = new MissionType("Delivery", true);
            _ = new MissionType("DeliveryWing", true);
            _ = new MissionType("Disable", true);
            _ = new MissionType("DisableMegaship", true);
            _ = new MissionType("DisableWing", true);
            _ = new MissionType("Founder");
            _ = new MissionType("Hack", true);
            _ = new MissionType("HackMegaship", true);
            _ = new MissionType("LongDistanceExpedition");
            _ = new MissionType("Massacre", true);
            _ = new MissionType("MassacreThargoid", true);
            _ = new MissionType("MassacreWing", true);
            _ = new MissionType("Mining", true);
            _ = new MissionType("MiningWing", true);
            _ = new MissionType("OnFoot", true);
            _ = new MissionType("Onslaught", true);
            _ = new MissionType("PassengerBulk", true);
            _ = new MissionType("PassengerVIP", true);
            _ = new MissionType("Piracy", true);
            _ = new MissionType("Planet", true);
            _ = new MissionType("Planetary", true);
            _ = new MissionType("POI", true);
            _ = new MissionType("Rescue", true);
            _ = new MissionType("Sabotage", true);
            _ = new MissionType("Salvage", true);
            _ = new MissionType("Scan", true);
            _ = new MissionType("Sightseeing", true);
            _ = new MissionType("Smuggle", true);

            // Hack types
            _ = new MissionType("Download", true);
            _ = new MissionType("Upload", true);

            // Heist types
            _ = new MissionType("Heist", true);
            _ = new MissionType("ProductionHeist", true);

            // Massacre target types
            _ = new MissionType("Skimmer", true);
            _ = new MissionType("Conflict", true);

            // Mission difficulty types
            _ = new MissionType("Covert");
            _ = new MissionType("Hard");
            _ = new MissionType("Illegal");
            _ = new MissionType("Legal");
            _ = new MissionType("NCD");

            // Mission special types
            _ = new MissionType("Chain");
            _ = new MissionType("GenericPermit1");
            _ = new MissionType("RankEmp");
            _ = new MissionType("RankFed");
            _ = new MissionType("Special");
            _ = new MissionType("StartZone");
            _ = new MissionType("Welcome");

            // Odyssey settlement states
            _ = new MissionType("Offline", true);
            _ = new MissionType("Reboot", true);
            _ = new MissionType("RebootRestore", true);

            // Sabotage types
            _ = new MissionType("Power", true);
            _ = new MissionType("Production", true);
        }

        public bool IncludeInMissionRouting { get; set; }

        // dummy used to ensure that the static constructor has run
        public MissionType() : this("")
        {
            // Include tags derived from economies, faction states, and government types
            foreach (var economy in Economy.AllOfThem)
            {
                _ = new MissionType(economy.edname) { fallbackLocalizedName = economy.localizedName, fallbackInvariantName = economy.invariantName };
            }
            foreach (var factionState in FactionState.AllOfThem)
            {
                _ = new MissionType(factionState.edname) { fallbackLocalizedName = factionState.localizedName, fallbackInvariantName = factionState.invariantName };
            }
            foreach (var government in Government.AllOfThem)
            {
                _ = new MissionType(government.edname) { fallbackLocalizedName = government.localizedName, fallbackInvariantName = government.invariantName };
            }
        }

        private MissionType(string edname, bool includeInMissionRouting = false) : base(edname, edname)
        {
            IncludeInMissionRouting = includeInMissionRouting;
        }
    }
}

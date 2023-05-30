namespace EddiDataDefinitions
{
    /// <summary>
    /// State types for systems and factions
    /// </summary>
    public class FactionState : ResourceBasedLocalizedEDName<FactionState>
    {
        static FactionState()
        {
            resourceManager = Properties.FactionStates.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new FactionState(edname);

            // Faction states have been broken out into the following categories:
            // - Economic status -reflects the wealth of a system.
            // - Security status -reflects the safety of a system.
            // - Conflict - based on the system influence and represents the control of the assets and population in a system.
            // - Movement - based on influence and our new Happiness value these states determine the movement of a faction between systems.
            // - Other - Anything not included in the above, and reserved for future states that we are not ready to talk about!
            // A faction can have one state from each of these categories active at a time in each of the systems it’s present in.

            // Lower tier states can cancel higher tier states from the same category.
        }

        public static readonly FactionState None = new FactionState("None");
        public static readonly FactionState Boom = new FactionState("Boom");                    // Economic state
        public static readonly FactionState Bust = new FactionState("Bust");                    // Economic state
        public static readonly FactionState CivilLiberty = new FactionState("CivilLiberty");    // Security state
        public static readonly FactionState CivilUnrest = new FactionState("CivilUnrest");      // Security state
        public static readonly FactionState CivilWar = new FactionState("CivilWar");            // Conflict state
        public static readonly FactionState Election = new FactionState("Election");            // Conflict state
        public static readonly FactionState Expansion = new FactionState("Expansion");          // Movement state
        public static readonly FactionState Famine = new FactionState("Famine");                // Economic state
        public static readonly FactionState Investment = new FactionState("Investment");        // Economic state
        public static readonly FactionState Lockdown = new FactionState("Lockdown");            // Security state
        public static readonly FactionState Outbreak = new FactionState("Outbreak");            // Other state (disease)
        public static readonly FactionState Retreat = new FactionState("Retreat");              // Movement state
        public static readonly FactionState War = new FactionState("War");                      // Conflict state

        // April 2019 Update
        public static readonly FactionState Drought = new FactionState("Drought");              // Other state
        public static readonly FactionState Incursion = new FactionState("Incursion");          // Other state (Thargoids)
        public static readonly FactionState PirateAttack = new FactionState("PirateAttack");    // Other state (pirates)

        // January 2020 Update
        public static readonly FactionState Blight = new FactionState("Blight");
        public static readonly FactionState ColdWar = new FactionState("ColdWar");
        public static readonly FactionState Colonisation = new FactionState("Colonisation");
        public static readonly FactionState HistoricEvent = new FactionState("HistoricEvent");
        public static readonly FactionState InfrastructureFailure = new FactionState("InfrastructureFailure");
        public static readonly FactionState NaturalDisaster = new FactionState("NaturalDisaster");
        public static readonly FactionState PublicHoliday = new FactionState("PublicHoliday");
        public static readonly FactionState Revolution = new FactionState("Revolution");
        public static readonly FactionState TechnologicalLeap = new FactionState("TechnologicalLeap");
        public static readonly FactionState Terrorism = new FactionState("Terrorism");
        public static readonly FactionState TradeWar = new FactionState("TradeWar");

        // May 2023 Update - Thargoid War States
        public static readonly FactionState ThargoidProbing = new FactionState("Thargoid_Probing");         // Thargoid Alert
        public static readonly FactionState ThargoidHarvest = new FactionState("Thargoid_Harvest");         // Thargoid Invasion
        public static readonly FactionState ThargoidControlled = new FactionState("Thargoid_Controlled");   // Thargoid Controlled
        public static readonly FactionState ThargoidStronghold = new FactionState("Thargoid_Stronghold");   // Thargoid Maelstrom
        public static readonly FactionState ThargoidRecovery = new FactionState("Thargoid_Recovery");       // Post-Thargoid Recovery
        
        // dummy used to ensure that the static constructor has run
        public FactionState () : this("")
        { }

        private FactionState(string edname) : base(edname, edname)
        { }

        public static new FactionState FromName(string from)
        {
            if (string.IsNullOrEmpty(from)) { return None; }
            // EDSM uses "Terrorist Attack" rather than "Terrorism"
            var tidiedFrom = from
                .Replace("Terrorist Attack", "Terrorism");
            return ResourceBasedLocalizedEDName<FactionState>.FromName(tidiedFrom);
        }
    }
}

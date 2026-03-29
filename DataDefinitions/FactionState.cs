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

        public static readonly FactionState None = new("None");
        public static readonly FactionState Boom = new("Boom");                    // Economic state
        public static readonly FactionState Bust = new("Bust");                    // Economic state
        public static readonly FactionState CivilLiberty = new("CivilLiberty");    // Security state
        public static readonly FactionState CivilUnrest = new("CivilUnrest");      // Security state
        public static readonly FactionState CivilWar = new("CivilWar");            // Conflict state
        public static readonly FactionState Election = new("Election");            // Conflict state
        public static readonly FactionState Expansion = new("Expansion");          // Movement state
        public static readonly FactionState Famine = new("Famine");                // Economic state
        public static readonly FactionState Investment = new("Investment");        // Economic state
        public static readonly FactionState Lockdown = new("Lockdown");            // Security state
        public static readonly FactionState Outbreak = new("Outbreak");            // Other state (disease)
        public static readonly FactionState Retreat = new("Retreat");              // Movement state
        public static readonly FactionState War = new("War");                      // Conflict state

        // April 2019 Update
        public static readonly FactionState Drought = new("Drought");              // Other state
        public static readonly FactionState Incursion = new("Incursion");          // Other state (Thargoids)
        public static readonly FactionState PirateAttack = new("PirateAttack");    // Other state (pirates)

        // January 2020 Update
        public static readonly FactionState Blight = new("Blight");
        public static readonly FactionState ColdWar = new("ColdWar");
        public static readonly FactionState Colonisation = new("Colonisation");
        public static readonly FactionState HistoricEvent = new("HistoricEvent");
        public static readonly FactionState InfrastructureFailure = new("InfrastructureFailure");
        public static readonly FactionState NaturalDisaster = new("NaturalDisaster");
        public static readonly FactionState PublicHoliday = new("PublicHoliday");
        public static readonly FactionState Revolution = new("Revolution");
        public static readonly FactionState TechnologicalLeap = new("TechnologicalLeap");
        public static readonly FactionState Terrorism = new("Terrorism");
        public static readonly FactionState TradeWar = new("TradeWar");

        // May 2023 Update - Thargoid War States
        public static readonly FactionState ThargoidProbing = new("Thargoid_Probing");         // Thargoid Alert
        public static readonly FactionState ThargoidHarvest = new("Thargoid_Harvest");         // Thargoid Invasion
        public static readonly FactionState ThargoidControlled = new("Thargoid_Controlled");   // Thargoid Controlled
        public static readonly FactionState ThargoidStronghold = new("Thargoid_Stronghold");   // Thargoid Maelstrom
        public static readonly FactionState ThargoidRecovery = new("Thargoid_Recovery");       // Post-Thargoid Recovery
        
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

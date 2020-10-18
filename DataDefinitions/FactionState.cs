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

            None = new FactionState("None");
            var Boom = new FactionState("Boom");                    // Economic state
            var Bust = new FactionState("Bust");                    // Economic state
            var CivilLiberty = new FactionState("CivilLiberty");    // Security state
            var CivilUnrest = new FactionState("CivilUnrest");      // Security state
            var CivilWar = new FactionState("CivilWar");            // Conflict state
            var Election = new FactionState("Election");            // Conflict state
            var Expansion = new FactionState("Expansion");          // Movement state
            var Famine = new FactionState("Famine");                // Economic state
            var Investment = new FactionState("Investment");        // Economic state
            var Lockdown = new FactionState("Lockdown");            // Security state
            var Outbreak = new FactionState("Outbreak");            // Other state (disease)
            var Retreat = new FactionState("Retreat");              // Movement state
            var War = new FactionState("War");                      // Conflict state

            // April 2019 Update
            var Drought = new FactionState("Drought");              // Other state
            var Incursion = new FactionState("Incursion");          // Other state (Thargoids)
            var PirateAttack = new FactionState("PirateAttack");    // Other state (pirates)

            // January 2020 Update
            var Blight = new FactionState("Blight");
            var ColdWar = new FactionState("ColdWar");
            var Colonisation = new FactionState("Colonisation");
            var HistoricEvent = new FactionState("HistoricEvent");
            var InfrastructureFailure = new FactionState("InfrastructureFailure");
            var NaturalDisaster = new FactionState("NaturalDisaster");
            var PublicHoliday = new FactionState("PublicHoliday");
            var Revolution = new FactionState("Revolution");
            var TechnologicalLeap = new FactionState("TechnologicalLeap");
            var Terrorism = new FactionState("Terrorism");
            var TradeWar = new FactionState("TradeWar");
        }

        public static readonly FactionState None;

        // dummy used to ensure that the static constructor has run
        public FactionState() : this("")
        { }

        private FactionState(string edname) : base(edname, edname)
        { }

        public new static FactionState FromName(string from)
        {
            if (string.IsNullOrEmpty(from)) { return None; }
            // EDSM uses "Terrorist Attack" rather than "Terrorism"
            var tidiedFrom = from
                .Replace("Terrorist Attack", "Terrorism");
            return ResourceBasedLocalizedEDName<FactionState>.FromName(tidiedFrom);
        }
    }
}

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
            resourceManager.IgnoreCase = false;

            // Faction states have been broken out into the following categories:
            // - Economic status -reflects the wealth of a system.
            // - Security status -reflects the safety of a system.
            // - Conflict - based on the system influence and represents the control of the assets and population in a system.
            // - Movement - based on influence and our new Happiness value these states determine the movement of a faction between systems.
            // - Other - Anything not included in the above, and reserved for future states that we are not ready to talk about!
            // A faction can have one state from each of these categories active at a time in each of the systems it’s present in.

            // Lower tier states can cancel higher tier states from the same category.

            None = new FactionState("None");
            var Retreat = new FactionState("Retreat");              // Movement state
            var War = new FactionState("War");                      // Conflict state
            var Lockdown = new FactionState("Lockdown");            // Security state
            var CivilLiberty = new FactionState("CivilLiberty");    // Security state
            var CivilUnrest = new FactionState("CivilUnrest");      // Security state
            var CivilWar = new FactionState("CivilWar");            // Conflict state
            var Boom = new FactionState("Boom");                    // Economic state
            var Expansion = new FactionState("Expansion");          // Movement state
            var Bust = new FactionState("Bust");                    // Economic state
            var Outbreak = new FactionState("Outbreak");            // Other state (disease)
            var Famine = new FactionState("Famine");                // Economic state
            var Election = new FactionState("Election");            // Conflict state
            var Investment = new FactionState("Investment");        // Economic state
            var PirateAttack = new FactionState("PirateAttack");    // Other state (pirates)
            var Incursion = new FactionState("Incursion");          // Other state (Thargoids)
        }

        public static readonly FactionState None;

        // dummy used to ensure that the static constructor has run
        public FactionState() : this("")
        { }

        private FactionState(string edname) : base(edname, edname)
        { }
    }
}

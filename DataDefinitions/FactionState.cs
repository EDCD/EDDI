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
            
            var Retreat = new FactionState("Retreat");
            var War = new FactionState("War");
            var Lockdown = new FactionState("Lockdown");
            var CivilLiberty = new FactionState("CivilLiberty");
            var CivilUnrest = new FactionState("CivilUnrest");
            var CivilWar = new FactionState("CivilWar");
            var Boom = new FactionState("Boom");
            var Expansion = new FactionState("Expansion");
            var Bust = new FactionState("Bust");
            var Outbreak = new FactionState("Outbreak");
            var Famine = new FactionState("Famine");
            var Election = new FactionState("Election");
            var Investment = new FactionState("Investment");
        }

        public static readonly FactionState None = new FactionState("None");

        // dummy used to ensure that the static constructor has run
        public FactionState() : this("")
        {}

        private FactionState(string edname) : base(edname, edname)
        {}
    }
}

using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// State types for systems and factions
    /// </summary>
    public class State : ResourceBasedLocalizedEDName<State>
    {
        static State()
        {
            resourceManager = Properties.States.ResourceManager;
            resourceManager.IgnoreCase = false;
            
            None = new State("None");
            var Retreat = new State("Retreat");
            var War = new State("War");
            var Lockdown = new State("Lockdown");
            var CivilUnrest = new State("CivilUnrest");
            var CivilWar = new State("CivilWar");
            var Boom = new State("Boom");
            var Expansion = new State("Expansion");
            var Bust = new State("Bust");
            var Outbreak = new State("Outbreak");
            var Famine = new State("Famine");
            var Election = new State("Election");
            var Investment = new State("Investment");
        }

        public static readonly State None;

        // dummy used to ensure that the static constructor has run
        public State() : this("")
        {}

        private State(string edname) : base(edname, edname)
        {}
    }
}

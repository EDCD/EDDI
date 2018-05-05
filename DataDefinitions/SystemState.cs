using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// State types for systems and factions
    /// </summary>
    public class SystemState : ResourceBasedLocalizedEDName<SystemState>
    {
        static SystemState()
        {
            resourceManager = Properties.SystemStates.ResourceManager;
            resourceManager.IgnoreCase = false;
            
            None = new SystemState("None");
            var Retreat = new SystemState("Retreat");
            var War = new SystemState("War");
            var Lockdown = new SystemState("Lockdown");
            CivilUnrest = new SystemState("CivilUnrest");
            CivilWar = new SystemState("CivilWar");
            var Boom = new SystemState("Boom");
            var Expansion = new SystemState("Expansion");
            var Bust = new SystemState("Bust");
            var Outbreak = new SystemState("Outbreak");
            var Famine = new SystemState("Famine");
            var Election = new SystemState("Election");
            var Investment = new SystemState("Investment");
        }

        public static readonly SystemState None;
        public static readonly SystemState CivilUnrest;
        public static readonly SystemState CivilWar;

        // dummy used to ensure that the static constructor has run
        public SystemState() : this("")
        {}

        private SystemState(string edname) : base(edname, edname)
        {}
    }
}

using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Crime types
    /// </summary>
    public class Crime : ResourceBasedLocalizedEDName<Crime>
    {
        static Crime()
        {
            resourceManager = Properties.Crimes.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new Crime(edname);

            var Assault = new Crime("assault");
            var Murder = new Crime("murder");
            var Piracy = new Crime("piracy");
            var Interdiction = new Crime("interdiction");
            var IllegalCargo = new Crime("illegalCargo");
            var DisobeyPolice = new Crime("disobeyPolice");
            var FireInNoFireZone = new Crime("fireInNoFireZone");
            var FireInStation = new Crime("fireInStation");
            var DumpingDangerous = new Crime("dumpingDangerous");
            var DumpingNearStation = new Crime("dumpingNearStation");
            var BlockingAirlockMinor = new Crime("dockingMinorBlockingAirlock");
            var BlockingAirlockMajor = new Crime("dockingMajorBlockingAirlock");
            var BlockingLandingPadMinor = new Crime("dockingMinorBlockingLandingPad");
            var BlockingLandingPadMajor = new Crime("dockingMajorBlockingLandingPad");
            var TrespassMinor = new Crime("dockingMinorTresspass");
            var TrespassMajor = new Crime("dockingMajorTresspass");
            var Collided = new Crime("collidedAtSpeedInNoFireZone");
            var CollidedWithDamage = new Crime("collidedAtSpeedInNoFireZone_hulldamage");
            var RecklessWeaponsDischarge = new Crime("recklessWeaponsDischarge");
            var PassengerWanted = new Crime("passengerWanted");
    }

        // dummy used to ensure that the static constructor has run
        public Crime() : this("")
        {}

        private Crime(string edname) : base(edname, edname)
        {}
    }
}

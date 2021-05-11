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

            None = new Crime("none");
            Claim = new Crime("claim");
            Fine = new Crime("fine");
            Bounty = new Crime("bounty");
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
            var MissionFine = new Crime("missionFine");

            var onFootAssault = new Crime("onFoot_assault");
            var onFootBreakingAndEntering = new Crime("onFoot_breakingAndEntering");
            var onFootDamagingDefences = new Crime("onFoot_damagingDefences");
            var onFootDetectionOfWeapon = new Crime("onFoot_detectionOfWeapon");
            var onFootfailureToSubmitToPolice = new Crime("onFoot_failureToSubmitToPolice");
            var onFootIdentityTheft = new Crime("onFoot_identityTheft");
            var onFootMurder = new Crime("onFoot_murder");
            var onFootProfileCloningIntent = new Crime("onFoot_profileCloningIntent");
            var onFootPropertyTheft = new Crime("onFoot_propertyTheft");
            var onFootRecklessEndangerment = new Crime("onFoot_recklessEndangerment");
            var onFootTheft = new Crime("onFoot_theft");
            var onFootTrespass = new Crime("onFoot_trespass");
        }

        // Faction report definition
        public static readonly Crime None;
        public static readonly Crime Claim;
        public static readonly Crime Fine;
        public static readonly Crime Bounty;

        // dummy used to ensure that the static constructor has run
        public Crime() : this("")
        { }

        private Crime(string edname) : base(edname, edname)
        { }
    }
}

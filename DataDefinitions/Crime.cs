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
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new Crime(edname);
        }

        // Faction report definitions
        public static readonly Crime None = new Crime("none");      // Claim records
        public static readonly Crime Claim = new Crime("claim");    // Claim discrepancy report (from user edits)
        public static readonly Crime Fine = new Crime("fine");      // Fine discrepancy report (from user edits)
        public static readonly Crime Bounty = new Crime("bounty");  // Bounty discrepancy report (from user edits)

        // In-Vehicle Crimes
        public static readonly Crime Assault = new Crime("assault");
        public static readonly Crime Murder = new Crime("murder");
        public static readonly Crime Piracy = new Crime("piracy");
        public static readonly Crime Interdiction = new Crime("interdiction");
        public static readonly Crime IllegalCargo = new Crime("illegalCargo");
        public static readonly Crime DisobeyPolice = new Crime("disobeyPolice");
        public static readonly Crime FireInNoFireZone = new Crime("fireInNoFireZone");
        public static readonly Crime FireInStation = new Crime("fireInStation");
        public static readonly Crime DumpingDangerous = new Crime("dumpingDangerous");
        public static readonly Crime DumpingNearStation = new Crime("dumpingNearStation");
        public static readonly Crime BlockingAirlockMinor = new Crime("dockingMinorBlockingAirlock");
        public static readonly Crime BlockingAirlockMajor = new Crime("dockingMajorBlockingAirlock");
        public static readonly Crime BlockingLandingPadMinor = new Crime("dockingMinorBlockingLandingPad");
        public static readonly Crime BlockingLandingPadMajor = new Crime("dockingMajorBlockingLandingPad");
        public static readonly Crime TrespassMinor = new Crime("dockingMinorTresspass");
        public static readonly Crime TrespassMajor = new Crime("dockingMajorTresspass");
        public static readonly Crime Collided = new Crime("collidedAtSpeedInNoFireZone");
        public static readonly Crime CollidedWithDamage = new Crime("collidedAtSpeedInNoFireZone_hulldamage");
        public static readonly Crime RecklessWeaponsDischarge = new Crime("recklessWeaponsDischarge");
        public static readonly Crime PassengerWanted = new Crime("passengerWanted");
        public static readonly Crime MissionFine = new Crime("missionFine");

        // On Foot Crimes
        public static readonly Crime onFootArcCutterUse = new Crime("onFoot_arcCutterUse");
        public static readonly Crime onFootAssault = new Crime("onFoot_assault");
        public static readonly Crime onFootBreakingAndEntering = new Crime("onFoot_breakingAndEntering");
        public static readonly Crime onFootCarryingIllegalData = new Crime("onFoot_carryingIllegalData");
        public static readonly Crime onFootCarryingStolenGoods = new Crime("onFoot_carryingStolenGoods");
        public static readonly Crime onFootDamagingDefences = new Crime("onFoot_damagingDefences");
        public static readonly Crime onFootDataTransfer = new Crime("onFoot_dataTransfer");
        public static readonly Crime onFootDetectionOfWeapon = new Crime("onFoot_detectionOfWeapon");
        public static readonly Crime onFootfailureToSubmitToPolice = new Crime("onFoot_failureToSubmitToPolice");
        public static readonly Crime onFootIdentityTheft = new Crime("onFoot_identityTheft");
        public static readonly Crime onFootMurder = new Crime("onFoot_murder");
        public static readonly Crime onFootOverchargeIntent = new Crime("onFoot_overchargeIntent");
        public static readonly Crime onFootProfileCloningIntent = new Crime("onFoot_profileCloningIntent");
        public static readonly Crime onFootPropertyTheft = new Crime("onFoot_propertyTheft");
        public static readonly Crime onFootRecklessEndangerment = new Crime("onFoot_recklessEndangerment");
        public static readonly Crime onFootTheft = new Crime("onFoot_theft");
        public static readonly Crime onFootTrespass = new Crime("onFoot_trespass");

        // dummy used to ensure that the static constructor has run
        public Crime() : this("")
        { }

        private Crime(string edname) : base(edname, edname)
        { }
    }
}

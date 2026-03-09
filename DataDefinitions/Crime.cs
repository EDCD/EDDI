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
        public static readonly Crime None = new("none");      // Claim records
        public static readonly Crime Claim = new("claim");    // Claim discrepancy report (from user edits)
        public static readonly Crime Fine = new("fine");      // Fine discrepancy report (from user edits)
        public static readonly Crime Bounty = new("bounty");  // Bounty discrepancy report (from user edits)

        // In-Vehicle Crimes
        public static readonly Crime Assault = new("assault");
        public static readonly Crime Murder = new("murder");
        public static readonly Crime Piracy = new("piracy");
        public static readonly Crime Interdiction = new("interdiction");
        public static readonly Crime IllegalCargo = new("illegalCargo");
        public static readonly Crime DisobeyPolice = new("disobeyPolice");
        public static readonly Crime FireInNoFireZone = new("fireInNoFireZone");
        public static readonly Crime FireInStation = new("fireInStation");
        public static readonly Crime DumpingDangerous = new("dumpingDangerous");
        public static readonly Crime DumpingNearStation = new("dumpingNearStation");
        public static readonly Crime BlockingAirlockMinor = new("dockingMinorBlockingAirlock");
        public static readonly Crime BlockingAirlockMajor = new("dockingMajorBlockingAirlock");
        public static readonly Crime BlockingLandingPadMinor = new("dockingMinorBlockingLandingPad");
        public static readonly Crime BlockingLandingPadMajor = new("dockingMajorBlockingLandingPad");
        public static readonly Crime TrespassMinor = new("dockingMinorTresspass");
        public static readonly Crime TrespassMajor = new("dockingMajorTresspass");
        public static readonly Crime Collided = new("collidedAtSpeedInNoFireZone");
        public static readonly Crime CollidedWithDamage = new("collidedAtSpeedInNoFireZone_hulldamage");
        public static readonly Crime RecklessWeaponsDischarge = new("recklessWeaponsDischarge");
        public static readonly Crime PassengerWanted = new("passengerWanted");
        public static readonly Crime MissionFine = new("missionFine");
        public static readonly Crime StationTamperingMinor = new( "stationTamperingMinor" );

        // On Foot Crimes
        public static readonly Crime onFootArcCutterUse = new("onFoot_arcCutterUse");
        public static readonly Crime onFootAssault = new("onFoot_assault");
        public static readonly Crime onFootBreakingAndEntering = new("onFoot_breakingAndEntering");
        public static readonly Crime onFootCarryingIllegalData = new("onFoot_carryingIllegalData");
        public static readonly Crime onFootCarryingStolenGoods = new("onFoot_carryingStolenGoods");
        public static readonly Crime onFootDamagingDefences = new("onFoot_damagingDefences");
        public static readonly Crime onFootDataTransfer = new("onFoot_dataTransfer");
        public static readonly Crime onFootDetectionOfWeapon = new("onFoot_detectionOfWeapon");
        public static readonly Crime onFootfailureToSubmitToPolice = new("onFoot_failureToSubmitToPolice");
        public static readonly Crime onFootIdentityTheft = new("onFoot_identityTheft");
        public static readonly Crime onFootMurder = new("onFoot_murder");
        public static readonly Crime onFootOverchargeIntent = new("onFoot_overchargeIntent");
        public static readonly Crime onFootProfileCloningIntent = new("onFoot_profileCloningIntent");
        public static readonly Crime onFootPropertyTheft = new("onFoot_propertyTheft");
        public static readonly Crime onFootRecklessEndangerment = new("onFoot_recklessEndangerment");
        public static readonly Crime onFootTheft = new("onFoot_theft");
        public static readonly Crime onFootTrespass = new("onFoot_trespass");

        // dummy used to ensure that the static constructor has run
        public Crime() : this("")
        { }

        private Crime(string edname) : base(edname, edname)
        { }
    }
}

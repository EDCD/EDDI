namespace EddiDataDefinitions
{
    public class SuitMod : ResourceBasedLocalizedEDName<SuitMod>
    {
        static SuitMod ()
        {
            resourceManager = Properties.SuitMod.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = edname => new SuitMod( edname );
        }

        public static readonly SuitMod AddedMeleeDamage = new( "suit_increasedmeleedamage");
        public static readonly SuitMod CombatMovementSpeed = new( "suit_adsmovementspeed");
        public static readonly SuitMod DamageResistance = new ( "suit_improvedarmourrating");
        public static readonly SuitMod EnhancedTracking = new ( "suit_improvedradar");
        public static readonly SuitMod ExtraAmmoCapacity = new( "suit_increasedammoreserves");
        public static readonly SuitMod ExtraBackpackCapacity = new( "suit_backpackcapacity");
        public static readonly SuitMod FasterShieldRegen = new( "suit_increasedshieldregen");
        public static readonly SuitMod ImprovedBatteryCapacity = new( "suit_increasedbatterycapacity");
        public static readonly SuitMod ImprovedJumpAssist = new( "suit_improvedjumpassist");
        public static readonly SuitMod IncreasedAirReserves = new( "suit_increasedo2capacity");
        public static readonly SuitMod IncreasedSprintDuration = new( "suit_increasedsprintduration");
        public static readonly SuitMod NightVision = new ( "suit_nightvision");
        public static readonly SuitMod QuieterFootsteps = new ( "suit_quieterfootsteps");
        public static readonly SuitMod ReducedToolBatteryConsumption = new ( "suit_reducedtoolbatteryconsumption");

        // dummy used to ensure that the static constructor has run
        public SuitMod () : this("")
        { }

        private SuitMod ( string edname ) : base( edname, edname )
        { }
    }
}

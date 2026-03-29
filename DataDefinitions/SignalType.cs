namespace EddiDataDefinitions
{
    public class SignalType : ResourceBasedLocalizedEDName<SignalType>
    {
        static SignalType ()
        {
            resourceManager = Properties.SignalType.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = edname => new SignalType( edname);
        }

        public static readonly SignalType Unidentified = new( "USS" );
        public static readonly SignalType Generic = new( "Generic" );

        public static readonly SignalType Combat = new( "Combat" );
        public static readonly SignalType FleetCarrier = new( "FleetCarrier" );
        public static readonly SignalType Installation = new( "Installation" );
        public static readonly SignalType Megaship = new( "Megaship" );
        public static readonly SignalType NavBeacon = new( "NavBeacon" );
        public static readonly SignalType Outpost = new( "Outpost" );
        public static readonly SignalType ResourceExtraction = new( "ResourceExtraction" );
        public static readonly SignalType SquadronCarrier = new( "SquadronCarrier" );
        public static readonly SignalType StationAsteroid = new( "StationAsteroid" );
        public static readonly SignalType StationBernalSphere = new( "StationBernalSphere" );
        public static readonly SignalType StationCoriolis = new( "StationCoriolis" );
        public static readonly SignalType StationMegaShip = new( "StationMegaShip" );
        public static readonly SignalType StationONeilCylinder = new( "StationONeilCylinder" );
        public static readonly SignalType StationONeilOrbis = new( "StationONeilOrbis" );
        public static readonly SignalType Titan = new( "Titan" );
        public static readonly SignalType TouristBeacon = new( "TouristBeacon" );

        // dummy used to ensure that the static constructor has run
        public SignalType () : this("")
        { }

        private SignalType ( string edname) : base(edname, edname)
        { }
    }
}

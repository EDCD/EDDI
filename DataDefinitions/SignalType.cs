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

        public static readonly SignalType Unidentified = new SignalType( "USS" );
        public static readonly SignalType Generic = new SignalType( "Generic" );

        public static readonly SignalType Combat = new SignalType( "Combat" );
        public static readonly SignalType FleetCarrier = new SignalType( "FleetCarrier" );
        public static readonly SignalType Installation = new SignalType( "Installation" );
        public static readonly SignalType Megaship = new SignalType( "Megaship" );
        public static readonly SignalType NavBeacon = new SignalType( "NavBeacon" );
        public static readonly SignalType Outpost = new SignalType( "Outpost" );
        public static readonly SignalType ResourceExtraction = new SignalType( "ResourceExtraction" );
        public static readonly SignalType StationAsteroid = new SignalType( "StationAsteroid" );
        public static readonly SignalType StationBernalSphere = new SignalType( "StationBernalSphere" );
        public static readonly SignalType StationCoriolis = new SignalType( "StationCoriolis" );
        public static readonly SignalType StationMegaShip = new SignalType( "StationMegaShip" );
        public static readonly SignalType StationONeilCylinder = new SignalType( "StationONeilCylinder" );
        public static readonly SignalType StationONeilOrbis = new SignalType( "StationONeilOrbis" );
        public static readonly SignalType Titan = new SignalType( "Titan" );
        public static readonly SignalType TouristBeacon = new SignalType( "TouristBeacon" );

        // dummy used to ensure that the static constructor has run
        public SignalType () : this("")
        { }

        private SignalType ( string edname) : base(edname, edname)
        { }
    }
}

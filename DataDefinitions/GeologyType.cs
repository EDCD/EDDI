namespace EddiDataDefinitions
{
    public class GeologyType : ResourceBasedLocalizedEDName<GeologyType>
    {
        static GeologyType ()
        {
            resourceManager = Properties.GeologyType.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new GeologyType( edname );
        }

        public static readonly GeologyType Fumarole = new GeologyType ( "Fumarole" );
        public static readonly GeologyType WaterGeyser = new GeologyType ( "WaterGeyser" );
        public static readonly GeologyType IceFumarole = new GeologyType ( "IceFumarole" );
        public static readonly GeologyType IceGeyser = new GeologyType ( "IceGeyser" );
        public static readonly GeologyType LavaSpout = new GeologyType ( "LavaSpout" );
        public static readonly GeologyType GasVent = new GeologyType ( "GasVent" );
        public static readonly GeologyType LagrangeCloud = new GeologyType ( "LagrangeCloud" );
        public static readonly GeologyType StormCloud = new GeologyType ( "StormCloud" );
        public static readonly GeologyType P_TypeAnomaly = new GeologyType ( "P_TypeAnomaly" );
        public static readonly GeologyType Q_TypeAnomaly = new GeologyType ( "Q_TypeAnomaly" );
        public static readonly GeologyType T_TypeAnomaly = new GeologyType ( "T_TypeAnomaly" );
        public static readonly GeologyType K_TypeAnomaly = new GeologyType ( "K_TypeAnomaly" );
        public static readonly GeologyType L_TypeAnomaly = new GeologyType ( "L_TypeAnomaly" );
        public static readonly GeologyType E_TypeAnomaly = new GeologyType ( "E_TypeAnomaly" );

        public string description => Properties.GeologyTypeDesc.ResourceManager.GetString( edname );

        // dummy used to ensure that the static constructor has run
        public GeologyType () : this( "" )
        { }

        private GeologyType ( string edname ) : base( edname, edname )
        { }
    }
}

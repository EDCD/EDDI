using Utilities;

namespace EddiDataDefinitions
{
    public class CodexGeologyOrAnomolyType : ResourceBasedLocalizedEDName<CodexGeologyOrAnomolyType>
    {
        static CodexGeologyOrAnomolyType ()
        {
            resourceManager = Properties.CodexGeologyOrAnomolyType.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new CodexGeologyOrAnomolyType( edname );
        }

        public static readonly CodexGeologyOrAnomolyType Fumarole = new( "Fumarole" );
        public static readonly CodexGeologyOrAnomolyType WaterGeyser = new( "WaterGeyser" );
        public static readonly CodexGeologyOrAnomolyType IceFumarole = new( "IceFumarole" );
        public static readonly CodexGeologyOrAnomolyType IceGeyser = new( "IceGeyser" );
        public static readonly CodexGeologyOrAnomolyType LavaSpout = new( "LavaSpout" );
        public static readonly CodexGeologyOrAnomolyType GasVent = new( "GasVent" );
        public static readonly CodexGeologyOrAnomolyType LagrangeCloud = new( "LagrangeCloud" );
        public static readonly CodexGeologyOrAnomolyType StormCloud = new( "StormCloud" );
        public static readonly CodexGeologyOrAnomolyType P_TypeAnomaly = new( "P_TypeAnomaly" );
        public static readonly CodexGeologyOrAnomolyType Q_TypeAnomaly = new( "Q_TypeAnomaly" );
        public static readonly CodexGeologyOrAnomolyType T_TypeAnomaly = new( "T_TypeAnomaly" );
        public static readonly CodexGeologyOrAnomolyType K_TypeAnomaly = new( "K_TypeAnomaly" );
        public static readonly CodexGeologyOrAnomolyType L_TypeAnomaly = new( "L_TypeAnomaly" );
        public static readonly CodexGeologyOrAnomolyType E_TypeAnomaly = new( "E_TypeAnomaly" );

        [PublicAPI( "localized description" )]
        public string description => Properties.CodexGeologyOrAnomolyTypeDesc.ResourceManager.GetString( edname );

        // dummy used to ensure that the static constructor has run
        public CodexGeologyOrAnomolyType () : this( "" )
        { }

        private CodexGeologyOrAnomolyType ( string edname ) : base( edname, edname )
        { }
    }
}
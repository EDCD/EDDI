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

        public static readonly CodexGeologyOrAnomolyType Fumarole = new CodexGeologyOrAnomolyType ( "Fumarole" );
        public static readonly CodexGeologyOrAnomolyType WaterGeyser = new CodexGeologyOrAnomolyType ( "WaterGeyser" );
        public static readonly CodexGeologyOrAnomolyType IceFumarole = new CodexGeologyOrAnomolyType ( "IceFumarole" );
        public static readonly CodexGeologyOrAnomolyType IceGeyser = new CodexGeologyOrAnomolyType ( "IceGeyser" );
        public static readonly CodexGeologyOrAnomolyType LavaSpout = new CodexGeologyOrAnomolyType ( "LavaSpout" );
        public static readonly CodexGeologyOrAnomolyType GasVent = new CodexGeologyOrAnomolyType ( "GasVent" );
        public static readonly CodexGeologyOrAnomolyType LagrangeCloud = new CodexGeologyOrAnomolyType ( "LagrangeCloud" );
        public static readonly CodexGeologyOrAnomolyType StormCloud = new CodexGeologyOrAnomolyType ( "StormCloud" );
        public static readonly CodexGeologyOrAnomolyType P_TypeAnomaly = new CodexGeologyOrAnomolyType ( "P_TypeAnomaly" );
        public static readonly CodexGeologyOrAnomolyType Q_TypeAnomaly = new CodexGeologyOrAnomolyType ( "Q_TypeAnomaly" );
        public static readonly CodexGeologyOrAnomolyType T_TypeAnomaly = new CodexGeologyOrAnomolyType ( "T_TypeAnomaly" );
        public static readonly CodexGeologyOrAnomolyType K_TypeAnomaly = new CodexGeologyOrAnomolyType ( "K_TypeAnomaly" );
        public static readonly CodexGeologyOrAnomolyType L_TypeAnomaly = new CodexGeologyOrAnomolyType ( "L_TypeAnomaly" );
        public static readonly CodexGeologyOrAnomolyType E_TypeAnomaly = new CodexGeologyOrAnomolyType ( "E_TypeAnomaly" );

        [PublicAPI]
        public string description => Properties.CodexGeologyOrAnomolyTypeDesc.ResourceManager.GetString( edname );

        // dummy used to ensure that the static constructor has run
        public CodexGeologyOrAnomolyType () : this( "" )
        { }

        private CodexGeologyOrAnomolyType ( string edname ) : base( edname, edname )
        { }
    }
}
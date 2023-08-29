using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace EddiDataDefinitions
{
    public class GeologyType : ResourceBasedLocalizedEDName<GeologyType>
    {
        public static ResourceManager rmGeologyTypeDesc = new ResourceManager("EddiDataDefinitions.Properties.GeologyTypeDesc", Assembly.GetExecutingAssembly());
        public static readonly IDictionary<string, GeologyType> TYPE = new Dictionary<string, GeologyType>();
        public string description;

        static GeologyType ()
        {
            resourceManager = Properties.GeologyType.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new GeologyType( edname );

            TYPE.Add( "Fumarole", new GeologyType( "Fumarole" ) );
            TYPE.Add( "WaterGeyser", new GeologyType( "WaterGeyser" ) );
            TYPE.Add( "IceFumarole", new GeologyType( "IceFumarole" ) );
            TYPE.Add( "IceGeyser", new GeologyType( "IceGeyser" ) );
            TYPE.Add( "LavaSpout", new GeologyType( "LavaSpout" ) );
            TYPE.Add( "GasVent", new GeologyType( "GasVent" ) );
            TYPE.Add( "LagrangeCloud", new GeologyType( "LagrangeCloud" ) );
            TYPE.Add( "StormCloud", new GeologyType( "StormCloud" ) );
            TYPE.Add( "P_TypeAnomoly", new GeologyType( "P_TypeAnomoly" ) );
            TYPE.Add( "Q_TypeAnomoly", new GeologyType( "Q_TypeAnomoly" ) );
            TYPE.Add( "T_TypeAnomoly", new GeologyType( "T_TypeAnomoly" ) );
            TYPE.Add( "K_TypeAnomoly", new GeologyType( "K_TypeAnomoly" ) );
            TYPE.Add( "L_TypeAnomoly", new GeologyType( "L_TypeAnomoly" ) );
            TYPE.Add( "E_TypeAnomoly", new GeologyType( "E_TypeAnomoly" ) );
        }

        // dummy used to ensure that the static constructor has run
        public GeologyType () : this( "" )
        {
            this.description = "";
        }

        private GeologyType ( string name ) : base( name, name )
        {
            this.description = rmGeologyTypeDesc.GetString( name );
        }

        /// <summary>
        /// Try getting data from the entryid first, then use variant name as a fallback
        /// </summary>
        public static GeologyType Lookup ( string name )
        {
            if ( name != "" )
            {
                if ( TYPE.ContainsKey( name ) )
                {
                    return TYPE[ name ];
                }
            }
            return null;
        }
    }
}

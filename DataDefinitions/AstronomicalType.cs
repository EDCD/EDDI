using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace EddiDataDefinitions
{
    public class AstronomicalType : ResourceBasedLocalizedEDName<AstronomicalType>
    {
        public static readonly IDictionary<string, AstronomicalType> TYPE = new Dictionary<string, AstronomicalType>();

        static AstronomicalType ()
        {
            resourceManager = Properties.AstronomicalType.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new AstronomicalType( edname );

            TYPE.Add( "Gas_Giants", new AstronomicalType( "Gas_Giants" ) );
            TYPE.Add( "Stars", new AstronomicalType( "Stars" ) );
            TYPE.Add( "Terrestrials", new AstronomicalType( "Terrestrials" ) );
        }

        // dummy used to ensure that the static constructor has run
        public AstronomicalType () : this( "" )
        { }

        private AstronomicalType ( string name ) : base( name, name )
        { }

        /// <summary>
        /// Try getting data from the entryid first, then use variant name as a fallback
        /// </summary>
        public static AstronomicalType Lookup ( string name )
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

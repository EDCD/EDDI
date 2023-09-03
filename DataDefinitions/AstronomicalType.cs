using System.Collections.Generic;

namespace EddiDataDefinitions
{
    public class AstronomicalType : ResourceBasedLocalizedEDName<AstronomicalType>
    {
        static AstronomicalType ()
        {
            resourceManager = Properties.AstronomicalType.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new AstronomicalType( edname );

            GasGiants = new AstronomicalType( "Gas_Giants" );
            Stars = new AstronomicalType( "Stars" );
            Terrestrials = new AstronomicalType( "Terrestrials" );
        }

        public static readonly AstronomicalType GasGiants;
        public static readonly AstronomicalType Stars;
        public static readonly AstronomicalType Terrestrials;

        // dummy used to ensure that the static constructor has run
        public AstronomicalType () : this( "" )
        { }

        private AstronomicalType ( string edname ) : base( edname, edname )
        { }
    }
}

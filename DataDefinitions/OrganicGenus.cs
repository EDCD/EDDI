using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class OrganicGenus : ResourceBasedLocalizedEDName<OrganicGenus>
    {
        static OrganicGenus ()
        {
            resourceManager = Properties.OrganicGenus.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new OrganicGenus( NormalizeGenus( edname ) );
        }

        // Terrestrial Genuses
        public static readonly OrganicGenus Aleoids = new OrganicGenus( "Aleoids", 150 );
        public static readonly OrganicGenus Vents = new OrganicGenus( "Vents", 100 );
        public static readonly OrganicGenus Sphere = new OrganicGenus( "Sphere", 100 );
        public static readonly OrganicGenus Bacterial = new OrganicGenus( "Bacterial", 500 );
        public static readonly OrganicGenus Cone = new OrganicGenus( "Cone", 100 );
        public static readonly OrganicGenus Brancae = new OrganicGenus( "Brancae", 100 );
        public static readonly OrganicGenus Cactoid = new OrganicGenus( "Cactoid", 300 );
        public static readonly OrganicGenus Clypeus = new OrganicGenus( "Clypeus", 150 );
        public static readonly OrganicGenus Conchas = new OrganicGenus( "Conchas", 150 );
        public static readonly OrganicGenus GroundStructIce = new OrganicGenus( "GroundStructIce", 100 );
        public static readonly OrganicGenus Electricae = new OrganicGenus( "Electricae", 1000 );
        public static readonly OrganicGenus Fonticulus = new OrganicGenus( "Fonticulus", 500 );
        public static readonly OrganicGenus Shrubs = new OrganicGenus( "Shrubs", 150 );
        public static readonly OrganicGenus Fumerolas = new OrganicGenus( "Fumerolas", 100 );
        public static readonly OrganicGenus Fungoids = new OrganicGenus( "Fungoids", 300 );
        public static readonly OrganicGenus Osseus = new OrganicGenus( "Osseus", 800 );
        public static readonly OrganicGenus Recepta = new OrganicGenus( "Recepta", 150 );
        public static readonly OrganicGenus Tubers = new OrganicGenus( "Tubers", 100 );
        public static readonly OrganicGenus Stratum = new OrganicGenus( "Stratum", 500 );
        public static readonly OrganicGenus Tubus = new OrganicGenus( "Tubus", 800 );
        public static readonly OrganicGenus Tussocks = new OrganicGenus( "Tussocks", 200 );

        // Genuses without any known minimum distance (including non-terrestrial genuses)
        public static readonly OrganicGenus MineralSpheres = new OrganicGenus( "MineralSpheres", 0 );
        public static readonly OrganicGenus MetallicCrystals = new OrganicGenus( "MetallicCrystals", 0 );
        public static readonly OrganicGenus SilicateCrystals = new OrganicGenus( "SilicateCrystals", 0 );
        public static readonly OrganicGenus IceCrystals = new OrganicGenus( "IceCrystals", 0 );
        public static readonly OrganicGenus MolluscReel = new OrganicGenus( "MolluscReel", 0 );
        public static readonly OrganicGenus MolluscGlobe = new OrganicGenus( "MolluscGlobe", 0 );
        public static readonly OrganicGenus MolluscBell = new OrganicGenus( "MolluscBell", 0 );
        public static readonly OrganicGenus MolluscUmbrella = new OrganicGenus( "MolluscUmbrella", 0 );
        public static readonly OrganicGenus MolluscGourd = new OrganicGenus( "MolluscGourd", 0 );
        public static readonly OrganicGenus MolluscTorus = new OrganicGenus( "MolluscTorus", 0 );
        public static readonly OrganicGenus MolluscBulb = new OrganicGenus( "MolluscBulb", 0 );
        public static readonly OrganicGenus MolluscParasol = new OrganicGenus( "MolluscParasol", 0 );
        public static readonly OrganicGenus MolluscSquid = new OrganicGenus( "MolluscSquid", 0 );
        public static readonly OrganicGenus MolluscBullet = new OrganicGenus( "MolluscBullet", 0 );
        public static readonly OrganicGenus MolluscCapsule = new OrganicGenus( "MolluscCapsule", 0 );
        public static readonly OrganicGenus CollaredPod = new OrganicGenus( "CollaredPod", 0 );
        public static readonly OrganicGenus StolonPod = new OrganicGenus( "StolonPod", 0 );
        public static readonly OrganicGenus StolonTree = new OrganicGenus( "StolonTree", 0 );
        public static readonly OrganicGenus AsterPod = new OrganicGenus( "AsterPod", 0 );
        public static readonly OrganicGenus ChalicePod = new OrganicGenus( "ChalicePod", 0 );
        public static readonly OrganicGenus PedunclePod = new OrganicGenus( "PedunclePod", 0 );
        public static readonly OrganicGenus RhizomePod = new OrganicGenus( "RhizomePod", 0 );
        public static readonly OrganicGenus QuadripartitePod = new OrganicGenus( "QuadripartitePod", 0 );
        public static readonly OrganicGenus VoidPod = new OrganicGenus( "VoidPod", 0 );
        public static readonly OrganicGenus AsterTree = new OrganicGenus( "AsterTree", 0 );
        public static readonly OrganicGenus PeduncleTree = new OrganicGenus( "PeduncleTree", 0 );
        public static readonly OrganicGenus GyreTree = new OrganicGenus( "GyreTree", 0 );
        public static readonly OrganicGenus GyrePod = new OrganicGenus( "GyrePod", 0 );
        public static readonly OrganicGenus VoidHeart = new OrganicGenus( "VoidHeart", 0 );
        public static readonly OrganicGenus CalcitePlates = new OrganicGenus( "CalcitePlates", 0 );
        public static readonly OrganicGenus ThargoidBarnacle = new OrganicGenus( "ThargoidBarnacle", 0 );

        [PublicAPI ("The minimum distance that you must travel before you can collect a fresh sample of this genus")]
        public int minimumDistanceMeters { get; private set; }

        [PublicAPI( "The maximum credit value for this genus" )]
        public long maximumValue => OrganicSpecies.AllOfThem.Where( s => s.genus == this ).Max( s => s.value );

        [PublicAPI( "The minimum credit value for this genus" )]
        public long minimumValue => OrganicSpecies.AllOfThem.Where( s => s.genus == this ).Min( s => s.value );

        [PublicAPI]
        public string description => Properties.OrganicGenusDesc.ResourceManager.GetString( NormalizeGenus( edname ) );

        // dummy used to ensure that the static constructor has run
        public OrganicGenus () : this( "" )
        { }

        private OrganicGenus ( string edname, int minimumDistanceMeters = 0 ) : base( edname, edname )
        {
            this.minimumDistanceMeters = minimumDistanceMeters;
        }

        public static new OrganicGenus FromEDName ( string edname )
        {
            return ResourceBasedLocalizedEDName<OrganicGenus>.FromEDName( NormalizeGenus( edname ) );
        }

        public static string NormalizeGenus ( string edname )
        {
            return edname?
                .Replace( "Codex_Ent_", "" )
                .Replace( "$", "" )
                .Replace( "_Genus_Name", "" )
                .Replace( "_Genus", "" )
                .Replace( "_Name;", "" )
                .Replace( "_name;", "" )
                .Replace( ";", "" );
        }
    }
}

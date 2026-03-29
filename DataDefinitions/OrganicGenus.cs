using Newtonsoft.Json;
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

        // So we can add a placeholder for missed predictions
        public static readonly OrganicGenus Unknown = new( "Unknown" );

        // Terrestrial Genuses
        public static readonly OrganicGenus Aleoids = new( "Aleoids", 150 );
        public static readonly OrganicGenus Vents = new( "Vents", 100 );
        public static readonly OrganicGenus Sphere = new( "Sphere", 100 );
        public static readonly OrganicGenus Bacterial = new( "Bacterial", 500 );
        public static readonly OrganicGenus Cone = new( "Cone", 100 );
        public static readonly OrganicGenus Brancae = new( "Brancae", 100 );
        public static readonly OrganicGenus Cactoid = new( "Cactoid", 300 );
        public static readonly OrganicGenus Clypeus = new( "Clypeus", 150 );
        public static readonly OrganicGenus Conchas = new( "Conchas", 150 );
        public static readonly OrganicGenus Ground_Struct_Ice = new( "Ground_Struct_Ice", 100 );
        public static readonly OrganicGenus Electricae = new( "Electricae", 1000 );
        public static readonly OrganicGenus Fonticulus = new( "Fonticulus", 500 );
        public static readonly OrganicGenus Shrubs = new( "Shrubs", 150 );
        public static readonly OrganicGenus Fumerolas = new( "Fumerolas", 100 );
        public static readonly OrganicGenus Fungoids = new( "Fungoids", 300 );
        public static readonly OrganicGenus Osseus = new( "Osseus", 800 );
        public static readonly OrganicGenus Recepta = new( "Recepta", 150 );
        public static readonly OrganicGenus Tubers = new( "Tubers", 100 );
        public static readonly OrganicGenus Stratum = new( "Stratum", 500 );
        public static readonly OrganicGenus Tubus = new( "Tubus", 800 );
        public static readonly OrganicGenus Tussocks = new( "Tussocks", 200 );
        // Genuses without any known minimum distance (including non-terrestrial genuses)
        public static readonly OrganicGenus MineralSpheres = new( "MineralSpheres" );
        public static readonly OrganicGenus MetallicCrystals = new( "MetallicCrystals" );
        public static readonly OrganicGenus SilicateCrystals = new( "SilicateCrystals" );
        public static readonly OrganicGenus IceCrystals = new( "IceCrystals" );
        public static readonly OrganicGenus MolluscReel = new( "MolluscReel" );
        public static readonly OrganicGenus MolluscGlobe = new( "MolluscGlobe" );
        public static readonly OrganicGenus MolluscBell = new( "MolluscBell" );
        public static readonly OrganicGenus MolluscUmbrella = new( "MolluscUmbrella" );
        public static readonly OrganicGenus MolluscGourd = new( "MolluscGourd" );
        public static readonly OrganicGenus MolluscTorus = new( "MolluscTorus" );
        public static readonly OrganicGenus MolluscBulb = new( "MolluscBulb" );
        public static readonly OrganicGenus MolluscParasol = new( "MolluscParasol" );
        public static readonly OrganicGenus MolluscSquid = new( "MolluscSquid" );
        public static readonly OrganicGenus MolluscBullet = new( "MolluscBullet" );
        public static readonly OrganicGenus MolluscCapsule = new( "MolluscCapsule" );
        public static readonly OrganicGenus CollaredPod = new( "CollaredPod" );
        public static readonly OrganicGenus StolonPod = new( "StolonPod" );
        public static readonly OrganicGenus StolonTree = new( "StolonTree" );
        public static readonly OrganicGenus AsterPod = new( "AsterPod" );
        public static readonly OrganicGenus ChalicePod = new( "ChalicePod" );
        public static readonly OrganicGenus PedunclePod = new( "PedunclePod" );
        public static readonly OrganicGenus RhizomePod = new( "RhizomePod" );
        public static readonly OrganicGenus QuadripartitePod = new( "QuadripartitePod" );
        public static readonly OrganicGenus VoidPod = new( "VoidPod" );
        public static readonly OrganicGenus AsterTree = new( "AsterTree" );
        public static readonly OrganicGenus PeduncleTree = new( "PeduncleTree" );
        public static readonly OrganicGenus GyreTree = new( "GyreTree" );
        public static readonly OrganicGenus GyrePod = new( "GyrePod" );
        public static readonly OrganicGenus VoidHeart = new( "VoidHeart" );
        public static readonly OrganicGenus CalcitePlates = new( "CalcitePlates" );
        public static readonly OrganicGenus ThargoidBarnacle = new( "ThargoidBarnacle" );
        public static readonly OrganicGenus Ingensradices = new( "Ingensradices" ); // Appears to be unique to HIP 87621.

        [JsonProperty]
        public int minimumDistanceMeters { get; private set; }

        [JsonIgnore, PublicAPI]
        public string localizedDescription => Properties.OrganicGenusDesc.ResourceManager.GetString( NormalizeGenus( edname ) );

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
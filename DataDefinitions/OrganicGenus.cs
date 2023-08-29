using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace EddiDataDefinitions
{
    public class OrganicGenus : ResourceBasedLocalizedEDName<OrganicGenus>
    {
        public static readonly IDictionary<string, OrganicGenus> GENUS = new Dictionary<string, OrganicGenus>();

        static OrganicGenus ()
        {
            resourceManager = Properties.OrganicGenus.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new OrganicGenus( edname );

            GENUS.Add( "Aleoids", new OrganicGenus( "Aleoids", 150 ) );
            GENUS.Add( "Vents", new OrganicGenus( "Vents", 100 ) );
            GENUS.Add( "Sphere", new OrganicGenus( "Sphere", 100 ) );
            GENUS.Add( "Bacterial", new OrganicGenus( "Bacterial", 500 ) );
            GENUS.Add( "Cone", new OrganicGenus( "Cone", 100 ) );
            GENUS.Add( "Brancae", new OrganicGenus( "Brancae", 100 ) );
            GENUS.Add( "Cactoid", new OrganicGenus( "Cactoid", 300 ) );
            GENUS.Add( "Clypeus", new OrganicGenus( "Clypeus", 150 ) );
            GENUS.Add( "Conchas", new OrganicGenus( "Conchas", 150 ) );
            GENUS.Add( "GroundStructIce", new OrganicGenus( "GroundStructIce", 100 ) );
            GENUS.Add( "Electricae", new OrganicGenus( "Electricae", 1000 ) );
            GENUS.Add( "Fonticulus", new OrganicGenus( "Fonticulus", 500 ) );
            GENUS.Add( "Shrubs", new OrganicGenus( "Shrubs", 150 ) );
            GENUS.Add( "Fumerolas", new OrganicGenus( "Fumerolas", 100 ) );
            GENUS.Add( "Fungoids", new OrganicGenus( "Fungoids", 300 ) );
            GENUS.Add( "Osseus", new OrganicGenus( "Osseus", 800 ) );
            GENUS.Add( "Recepta", new OrganicGenus( "Recepta", 150 ) );
            GENUS.Add( "Tubers", new OrganicGenus( "Tubers", 100 ) );
            GENUS.Add( "Stratum", new OrganicGenus( "Stratum", 500 ) );
            GENUS.Add( "Tubus", new OrganicGenus( "Tubus", 800 ) );
            GENUS.Add( "Tussocks", new OrganicGenus( "Tussocks", 200 ) );
            GENUS.Add( "MineralSpheres", new OrganicGenus( "MineralSpheres", 0 ) );
            GENUS.Add( "MetallicCrystals", new OrganicGenus( "MetallicCrystals", 0 ) );
            GENUS.Add( "SilicateCrystals", new OrganicGenus( "SilicateCrystals", 0 ) );
            GENUS.Add( "IceCrystals", new OrganicGenus( "IceCrystals", 0 ) );
            GENUS.Add( "MolluscReel", new OrganicGenus( "MolluscReel", 0 ) );
            GENUS.Add( "MolluscGlobe", new OrganicGenus( "MolluscGlobe", 0 ) );
            GENUS.Add( "MolluscBell", new OrganicGenus( "MolluscBell", 0 ) );
            GENUS.Add( "MolluscUmbrella", new OrganicGenus( "MolluscUmbrella", 0 ) );
            GENUS.Add( "MolluscGourd", new OrganicGenus( "MolluscGourd", 0 ) );
            GENUS.Add( "MolluscTorus", new OrganicGenus( "MolluscTorus", 0 ) );
            GENUS.Add( "MolluscBulb", new OrganicGenus( "MolluscBulb", 0 ) );
            GENUS.Add( "MolluscParasol", new OrganicGenus( "MolluscParasol", 0 ) );
            GENUS.Add( "MolluscSquid", new OrganicGenus( "MolluscSquid", 0 ) );
            GENUS.Add( "MolluscBullet", new OrganicGenus( "MolluscBullet", 0 ) );
            GENUS.Add( "MolluscCapsule", new OrganicGenus( "MolluscCapsule", 0 ) );
            GENUS.Add( "CollaredPod", new OrganicGenus( "CollaredPod", 0 ) );
            GENUS.Add( "StolonPod", new OrganicGenus( "StolonPod", 0 ) );
            GENUS.Add( "StolonTree", new OrganicGenus( "StolonTree", 0 ) );
            GENUS.Add( "AsterPod", new OrganicGenus( "AsterPod", 0 ) );
            GENUS.Add( "ChalicePod", new OrganicGenus( "ChalicePod", 0 ) );
            GENUS.Add( "PedunclePod", new OrganicGenus( "PedunclePod", 0 ) );
            GENUS.Add( "RhizomePod", new OrganicGenus( "RhizomePod", 0 ) );
            GENUS.Add( "QuadripartitePod", new OrganicGenus( "QuadripartitePod", 0 ) );
            GENUS.Add( "VoidPod", new OrganicGenus( "VoidPod", 0 ) );
            GENUS.Add( "AsterTree", new OrganicGenus( "AsterTree", 0 ) );
            GENUS.Add( "PeduncleTree", new OrganicGenus( "PeduncleTree", 0 ) );
            GENUS.Add( "GyreTree", new OrganicGenus( "GyreTree", 0 ) );
            GENUS.Add( "GyrePod", new OrganicGenus( "GyrePod", 0 ) );
            GENUS.Add( "VoidHeart", new OrganicGenus( "VoidHeart", 0 ) );
            GENUS.Add( "CalcitePlates", new OrganicGenus( "CalcitePlates", 0 ) );
            GENUS.Add( "ThargoidBarnacle", new OrganicGenus( "ThargoidBarnacle", 0 ) );
        }

        public static ResourceManager rmOrganicGenusDesc = new ResourceManager("EddiDataDefinitions.Properties.OrganicGenusDesc", Assembly.GetExecutingAssembly());
        public int distance;
        public string description;

        // dummy used to ensure that the static constructor has run
        public OrganicGenus () : this( "" )
        { }

        private OrganicGenus ( string genus ) : base( genus, genus )
        {
            this.distance = 0;
            if ( GENUS.ContainsKey( genus ) )
            {
                this.distance = GENUS[ genus ].distance;
            }
            this.description = rmOrganicGenusDesc.GetString( genus );
        }

        private OrganicGenus ( string genus, int distance ) : base( genus, genus )
        {
            this.distance = distance;
            this.description = rmOrganicGenusDesc.GetString( genus );
        }

        /// <summary>
        /// Try getting data from the entryid first, then use variant name as a fallback
        /// </summary>
        public static OrganicGenus Lookup ( string genus )
        {
            if ( genus != "" )
            {
                if ( GENUS.ContainsKey( genus ) )
                {
                    return GENUS[ genus ];
                }
            }
            return null;
        }
    }
}

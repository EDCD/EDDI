#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace EddiDataDefinitions
{
    public class GalacticRegion
    {
        private static readonly List<GalacticRegion> AllOfThem =
        [
            new("Galactic Centre", 1),
            new("Empyrean Straits", 2),
            new("Ryker's Hope", 3),
            new("Odin's Hold", 4),
            new("Norma Arm", 5),
            new("Arcadian Stream", 6),
            new("Izanami", 7),
            new("Inner Orion-Perseus Conflux", 8),
            new("Inner Scutum-Centaurus Arm", 9),
            new("Norma Expanse", 10),
            new("Trojan Belt", 11),
            new("The Veils", 12),
            new("Newton's Vault", 13),
            new("The Conduit", 14),
            new("Outer Orion-Perseus Conflux", 15),
            new("Orion-Cygnus Arm", 16),
            new("Temple", 17),
            new("Inner Orion Spur", 18),
            new("Hawking's Gap", 19),
            new("Dryman's Point", 20),
            new("Sagittarius-Carina Arm", 21),
            new("Mare Somnia", 22),
            new("Acheron", 23),
            new("Formorian Frontier", 24),
            new("Hieronymus Delta", 25),
            new("Outer Scutum-Centaurus Arm", 26),
            new("Outer Arm", 27),
            new("Aquila's Halo", 28),
            new("Errant Marches", 29),
            new("Perseus Arm", 30),
            new("Formidine Rift", 31),
            new("Vulcan Gate", 32),
            new("Elysian Shore", 33),
            new("Sanguineous Rim", 34),
            new("Outer Orion Spur", 35),
            new("Achilles's Altar", 36),
            new("Xibalba", 37),
            new("Lyra's Song", 38),
            new("Tenebrae", 39),
            new("The Abyss", 40),
            new("Kepler's Crest", 41),
            new("The Void", 42),
        ];
        private static readonly Dictionary<int, GalacticRegion> ByRegionID = AllOfThem.ToDictionary( region => region.regionId, region => region );

        [Utilities.PublicAPI( "The stellar region name." ) ]
        public string regionName { get; private set; }

        [ Utilities.PublicAPI( "The stellar numeric region ID." ) ]
        public int regionId { get; private set; }

        private GalacticRegion ( string name, int id )
        {
            this.regionName = name;
            this.regionId = id;
        }

        public static GalacticRegion? FromID ( int id )
        {
            return ByRegionID.GetValueOrDefault(id);
        }

        public static GalacticRegion? FromXZCoordinates ( decimal x, decimal z )
        {
            var px = (int)( ( x - GalacticRegionMap.x0 ) * 83 / 4096 );
            var pz = (int)( ( z - GalacticRegionMap.z0 ) * 83 / 4096 );

            if ( px < 0 || pz < 0 || pz >= GalacticRegionMap.MapLines.Length )
            {
                return null;
            }

            var row = GalacticRegionMap.MapLines[ pz ];
            var rx = 0;
            var pv = 0;

            foreach ( var (rl, rv) in row )
            {
                if ( px < ( rx + rl ) )
                {
                    pv = rv;
                    break;
                }

                rx += rl;
            }

            return pv == 0
                ? null
                : FromID(pv);
        }
    }
}
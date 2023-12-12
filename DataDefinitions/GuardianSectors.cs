using Rollbar.DesignPatterns.Construction.FactoryMethod;
using System.Collections.Generic;
using System.Linq;
using Utilities;
//using System.Windows;
//using System.Windows.Forms;
//using System.Threading;

namespace EddiDataDefinitions
{
    public class GuardianSector
    {
        static GuardianSector ()
        {
            //resourceManager = Properties.OrganicGenus.ResourceManager;
            //resourceManager.IgnoreCase = true;
            //missingEDNameHandler = ( edname ) => new Nebula( NormalizeGenus( edname ) );
        }

        public static List<GuardianSector> AllOfThem = new List<GuardianSector> ();

        // This does not include ALL sectors where brain trees have been found, only sectors where 8+ samples existed from Cannon
        // https://docs.google.com/spreadsheets/d/15lqZtqJk7B2qUV5Jb4tlnst6i1B7pXlAUzQnacX64Kc/edit#gid=652985927

        // Possible future implementation of Guardian Structures
        // https://docs.google.com/spreadsheets/d/1VF1R8kbeyreAfx-WoTJrzeZaXKpOKXuky9GNcD56H6Y/edit#gid=0

        public static readonly GuardianSector GS_GRAEA_HYPUE = new GuardianSector( "GRAEA HYPUE", "Norma Expanse");
        public static readonly GuardianSector GS_SYNUEFE = new GuardianSector( "SYNUEFE", "Inner Orion Spur");
        public static readonly GuardianSector GS_COL_173_SECTOR = new GuardianSector( "COL 173 SECTOR", "Inner Orion Spur");
        public static readonly GuardianSector GS_WREGOE = new GuardianSector( "WREGOE", "Inner Orion Spur");
        public static readonly GuardianSector GS_EORL_AUWSY = new GuardianSector( "EORL AUWSY", "Empyrean Straits");
        public static readonly GuardianSector GS_PRAI_HYPOO = new GuardianSector( "PRAI HYPOO", "Temple");
        public static readonly GuardianSector GS_ETA_CARINA_SECTOR = new GuardianSector( "ETA CARINA SECTOR", "Outer Orion Spur");
        public static readonly GuardianSector GS_IC_2391_SECTOR = new GuardianSector( "IC 2391 SECTOR", "Inner Orion Spur");
        public static readonly GuardianSector GS_VELA_DARK_REGION = new GuardianSector( "VELA DARK REGION", "Inner Orion Spur");
        public static readonly GuardianSector GS_COL_132_SECTOR = new GuardianSector( "COL 132 SECTOR", "Inner Orion Spur");
        public static readonly GuardianSector GS_SWOILZ = new GuardianSector( "SWOILZ", "Inner Orion Spur");
        public static readonly GuardianSector GS_COL_135_SECTOR = new GuardianSector( "COL 135 SECTOR", "Inner Orion Spur");
        public static readonly GuardianSector GS_NGC_3199_SECTOR = new GuardianSector( "NGC 3199 SECTOR", "Outer Orion Spur");
        public static readonly GuardianSector GS_SKAUDAI = new GuardianSector( "SKAUDAI", "Inner Scutum-Centaurus Arm");
        public static readonly GuardianSector GS_NUEKAU = new GuardianSector( "NUEKAU", "Norma Expanse");
        public static readonly GuardianSector GS_PRUA_PHOE = new GuardianSector( "PRUA PHOE", "Inner Scutum-Centaurus Arm");
        public static readonly GuardianSector GS_PRAEA_EUQ = new GuardianSector( "PRAEA EUQ", "Inner Orion Spur");
        public static readonly GuardianSector GS_IC_2602_SECTOR = new GuardianSector( "IC 2602 SECTOR", "Inner Orion Spur");
        public static readonly GuardianSector GS_HD_63276 = new GuardianSector( "HD 63276", "Inner Orion Spur");
        public static readonly GuardianSector GS_PRUA_DRYOAE = new GuardianSector( "PRUA DRYOAE", "Inner Orion Spur");
        public static readonly GuardianSector GS_BLAE_EORK = new GuardianSector( "BLAE EORK", "Outer Orion Spur");
        public static readonly GuardianSector GS_HIP_43956 = new GuardianSector( "HIP 43956", "Inner Orion Spur");
        public static readonly GuardianSector GS_FLYUA_DRYOAE = new GuardianSector( "FLYUA DRYOAE", "Inner Orion Spur");
        public static readonly GuardianSector GS_SHROGAAE = new GuardianSector( "SHROGAAE", "Empyrean Straits");
        public static readonly GuardianSector GS_HD_81946 = new GuardianSector( "HD 81946", "Inner Orion Spur");
        public static readonly GuardianSector GS_HIP_34377 = new GuardianSector( "HIP 34377", "Inner Orion Spur");
        public static readonly GuardianSector GS_KAPPA_1_VOLANTIS = new GuardianSector( "KAPPA-1 VOLANTIS", "Inner Orion Spur");
        public static readonly GuardianSector GS_NGC_2451A_SECTOR = new GuardianSector( "NGC 2451A SECTOR", "Inner Orion Spur");
        public static readonly GuardianSector GS_HIP_33517 = new GuardianSector( "HIP 33517", "Inner Orion Spur");
        public static readonly GuardianSector GS_PENCIL_SECTOR = new GuardianSector( "PENCIL SECTOR", "Inner Orion Spur");
        public static readonly GuardianSector GS_TV_MUSCAE = new GuardianSector( "TV MUSCAE", "Inner Orion Spur");
        public static readonly GuardianSector GS_BLAA_HYPAI = new GuardianSector( "BLAA HYPAI", "Norma Expanse");
        public static readonly GuardianSector GS_HR_4220 = new GuardianSector( "HR 4220", "Inner Orion Spur");
        public static readonly GuardianSector GS_NGC_2547_SECTOR = new GuardianSector( "NGC 2547 SECTOR", "Inner Orion Spur");
        public static readonly GuardianSector GS_PRO_EURL = new GuardianSector( "PRO EURL", "Inner Orion Spur");
        public static readonly GuardianSector GS_CLOOKEOU = new GuardianSector( "CLOOKEOU", "Norma Expanse");
        public static readonly GuardianSector GS_D_CARINAE = new GuardianSector( "D CARINAE", "Inner Orion Spur");
        public static readonly GuardianSector GS_HD_63154 = new GuardianSector( "HD 63154", "Inner Orion Spur");
        public static readonly GuardianSector GS_HIP_49394 = new GuardianSector( "HIP 49394", "Inner Orion Spur");
        public static readonly GuardianSector GS_PCYC_275 = new GuardianSector( "PCYC 275", "Outer Orion Spur");
        public static readonly GuardianSector GS_35_G_CARINAE = new GuardianSector( "35 G. CARINAE", "Inner Orion Spur");
        public static readonly GuardianSector GS_COALSACK_SECTOR = new GuardianSector( "COALSACK SECTOR", "Inner Orion Spur");
        public static readonly GuardianSector GS_HIP_39758 = new GuardianSector( "HIP 39758", "Inner Orion Spur");
        public static readonly GuardianSector GS_HIP_46193 = new GuardianSector( "HIP 46193", "Inner Orion Spur");
        public static readonly GuardianSector GS_PLIO_EURL = new GuardianSector( "PLIO EURL", "Inner Orion Spur");
        public static readonly GuardianSector GS_HIP_34322 = new GuardianSector( "HIP 34322", "Inner Orion Spur");
        public static readonly GuardianSector GS_HIP_38671 = new GuardianSector( "HIP 38671", "Inner Orion Spur");
        public static readonly GuardianSector GS_PUPPIS_DARK_REGION_B_SECTOR = new GuardianSector( "PUPPIS DARK REGION B SECTOR", "Inner Orion Spur");
        public static readonly GuardianSector GS_CHAMAELEON_SECTOR = new GuardianSector( "CHAMAELEON SECTOR", "Inner Orion Spur");
        public static readonly GuardianSector GS_HIP_37610 = new GuardianSector( "HIP 37610", "Inner Orion Spur");
        public static readonly GuardianSector GS_HIP_36823 = new GuardianSector( "HIP 36823", "Inner Orion Spur");
        public static readonly GuardianSector GS_PUEKEE = new GuardianSector( "PUEKEE", "Norma Expanse");
        public static readonly GuardianSector GS_HIP_41908 = new GuardianSector( "HIP 41908", "Inner Orion Spur");
        public static readonly GuardianSector GS_HIP_42459 = new GuardianSector( "HIP 42459", "Inner Orion Spur");
        public static readonly GuardianSector GS_BLAE_HYPUE = new GuardianSector( "BLAE HYPUE", "Norma Expanse");
        public static readonly GuardianSector GS_HIP_49815 = new GuardianSector( "HIP 49815", "Inner Orion Spur");
        public static readonly GuardianSector GS_2MASS = new GuardianSector( "2MASS", "Outer Orion Spur");
        public static readonly GuardianSector GS_DROKOE = new GuardianSector( "DROKOE", "Outer Orion Spur");
        public static readonly GuardianSector GS_HIP_43123 = new GuardianSector( "HIP 43123", "Inner Orion Spur");
        public static readonly GuardianSector GS_NGC_2516_SECTOR = new GuardianSector( "NGC 2516 SECTOR", "Inner Orion Spur");

        [PublicAPI("The name of the nebula")]
        public string name;
        public string region;

        // dummy used to ensure that the static constructor has run
        public GuardianSector ()
        { }

        private GuardianSector ( string name, string region )
        {
            this.name = name;
            this.region = region;

            AllOfThem.Add( this );
        }

        // Check if system is in a known guardian sector, ignoring the region
        public static bool TryGetGuardianSector ( string systemname )
        {
            return AllOfThem.Any( x=> systemname.Contains(x.name) );
        }

        // Check if system is in a known guardian sector and region
        public static bool TryGetGuardianSector ( string systemname, string region )
        {
            return AllOfThem.Any( x=> systemname.Contains(x.name) && x.region==region );
        }
    }
}

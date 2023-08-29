using System.Collections.Generic;
using System.Reflection;
using System.Resources;
using System.Threading;
using Utilities;

namespace EddiDataDefinitions
{
    public class Geology : ResourceBasedLocalizedEDName<Geology>
    {
        public static ResourceManager rmGeologyDesc = new ResourceManager("EddiDataDefinitions.Properties.GeologyDesc", Assembly.GetExecutingAssembly());

        public static readonly IDictionary<string, long> GEOLOGICALS = new Dictionary<string, long>();
        public static readonly IDictionary<long, Geology> ENTRYIDS = new Dictionary<long, Geology>();

        public bool exists;                 // This item exists and has been populated with information
        public GeologyType type;
        public long value;
        public string description;

        static Geology ()
        {
            resourceManager = Properties.Geology.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new Geology( edname );

            ENTRYIDS.Add( 1400109, new Geology( "Fumarole", "Fumarole_CarbonDioxideGeysers", (long)50000 ) );
            ENTRYIDS.Add( 1400114, new Geology( "Fumarole", "Fumarole_SilicateVapourGeysers", (long)50000 ) );
            ENTRYIDS.Add( 1400102, new Geology( "Fumarole", "Fumarole_SulphurDioxideMagma", (long)50000 ) );
            ENTRYIDS.Add( 1400108, new Geology( "Fumarole", "Fumarole_WaterGeysers", (long)50000 ) );
            ENTRYIDS.Add( 1400601, new Geology( "LagrangeCloud", "Gas_Clds_Blue", (long)50000 ) );
            ENTRYIDS.Add( 1400701, new Geology( "LagrangeCloud", "Gas_Clds_Green", (long)50000 ) );
            ENTRYIDS.Add( 1400702, new Geology( "LagrangeCloud", "Gas_Clds_Green_Storm", (long)50000 ) );
            ENTRYIDS.Add( 1401300, new Geology( "LagrangeCloud", "Gas_Clds_Light", (long)50000 ) );
            ENTRYIDS.Add( 1400801, new Geology( "LagrangeCloud", "Gas_Clds_Orange", (long)50000 ) );
            ENTRYIDS.Add( 1400802, new Geology( "LagrangeCloud", "Gas_Clds_Orange_Storm", (long)50000 ) );
            ENTRYIDS.Add( 1400901, new Geology( "LagrangeCloud", "Gas_Clds_Pink", (long)50000 ) );
            ENTRYIDS.Add( 1400902, new Geology( "LagrangeCloud", "Gas_Clds_Pink_Storm", (long)50000 ) );
            ENTRYIDS.Add( 1401001, new Geology( "LagrangeCloud", "Gas_Clds_Red", (long)50000 ) );
            ENTRYIDS.Add( 1401002, new Geology( "LagrangeCloud", "Gas_Clds_Red_Storm", (long)50000 ) );
            ENTRYIDS.Add( 1401101, new Geology( "LagrangeCloud", "Gas_Clds_Yellow", (long)50000 ) );
            ENTRYIDS.Add( 1401102, new Geology( "LagrangeCloud", "Gas_Clds_Yellow_Storm", (long)50000 ) );
            ENTRYIDS.Add( 1400409, new Geology( "GasVent", "Gas_Vents_CarbonDioxideGeysers", (long)50000 ) );
            ENTRYIDS.Add( 1400414, new Geology( "GasVent", "Gas_Vents_SilicateVapourGeysers", (long)50000 ) );
            ENTRYIDS.Add( 1400402, new Geology( "GasVent", "Gas_Vents_SulphurDioxideMagma", (long)50000 ) );
            ENTRYIDS.Add( 1400408, new Geology( "GasVent", "Gas_Vents_WaterGeysers", (long)50000 ) );
            ENTRYIDS.Add( 1400208, new Geology( "WaterGeyser", "Geysers_WaterGeysers", (long)50000 ) );
            ENTRYIDS.Add( 1400160, new Geology( "IceFumarole", "IceFumarole_AmmoniaGeysers", (long)50000 ) );
            ENTRYIDS.Add( 1400159, new Geology( "IceFumarole", "IceFumarole_CarbonDioxideGeysers", (long)50000 ) );
            ENTRYIDS.Add( 1400161, new Geology( "IceFumarole", "IceFumarole_MethaneGeysers", (long)50000 ) );
            ENTRYIDS.Add( 1400162, new Geology( "IceFumarole", "IceFumarole_NitrogenGeysers", (long)50000 ) );
            ENTRYIDS.Add( 1400164, new Geology( "IceFumarole", "IceFumarole_SilicateVapourGeysers", (long)50000 ) );
            ENTRYIDS.Add( 1400152, new Geology( "IceFumarole", "IceFumarole_SulphurDioxideMagma", (long)50000 ) );
            ENTRYIDS.Add( 1400158, new Geology( "IceFumarole", "IceFumarole_WaterGeysers", (long)50000 ) );
            ENTRYIDS.Add( 1400260, new Geology( "IceGeyser", "IceGeysers_AmmoniaGeysers", (long)50000 ) );
            ENTRYIDS.Add( 1400259, new Geology( "IceGeyser", "IceGeysers_CarbonDioxideGeysers", (long)50000 ) );
            ENTRYIDS.Add( 1400261, new Geology( "IceGeyser", "IceGeysers_MethaneGeysers", (long)50000 ) );
            ENTRYIDS.Add( 1400262, new Geology( "IceGeyser", "IceGeysers_NitrogenGeysers", (long)50000 ) );
            ENTRYIDS.Add( 1400258, new Geology( "IceGeyser", "IceGeysers_WaterGeysers", (long)50000 ) );
            ENTRYIDS.Add( 2401001, new Geology( "K_TypeAnomoly", "L_Phn_Part_Cld_001", (long)50000 ) );
            ENTRYIDS.Add( 2401002, new Geology( "K_TypeAnomoly", "L_Phn_Part_Cld_002", (long)50000 ) );
            ENTRYIDS.Add( 2401003, new Geology( "K_TypeAnomoly", "L_Phn_Part_Cld_003", (long)50000 ) );
            ENTRYIDS.Add( 2401004, new Geology( "K_TypeAnomoly", "L_Phn_Part_Cld_004", (long)50000 ) );
            ENTRYIDS.Add( 2401005, new Geology( "K_TypeAnomoly", "L_Phn_Part_Cld_005", (long)50000 ) );
            ENTRYIDS.Add( 2401006, new Geology( "K_TypeAnomoly", "L_Phn_Part_Cld_006", (long)50000 ) );
            ENTRYIDS.Add( 2401007, new Geology( "E_TypeAnomaly", "L_Phn_Part_Cld_007", (long)50000 ) );
            ENTRYIDS.Add( 2401008, new Geology( "K_TypeAnomoly", "L_Phn_Part_Cld_008", (long)50000 ) );
            ENTRYIDS.Add( 2401009, new Geology( "K_TypeAnomoly", "L_Phn_Part_Cld_009", (long)50000 ) );
            ENTRYIDS.Add( 2401010, new Geology( "K_TypeAnomoly", "L_Phn_Part_Cld_010", (long)50000 ) );
            ENTRYIDS.Add( 2401011, new Geology( "K_TypeAnomoly", "L_Phn_Part_Cld_011", (long)50000 ) );
            ENTRYIDS.Add( 2401012, new Geology( "K_TypeAnomoly", "L_Phn_Part_Cld_012", (long)50000 ) );
            ENTRYIDS.Add( 2401013, new Geology( "E_TypeAnomaly", "L_Phn_Part_Cld_013", (long)50000 ) );
            ENTRYIDS.Add( 2401014, new Geology( "E_TypeAnomaly", "L_Phn_Part_Cld_014", (long)50000 ) );
            ENTRYIDS.Add( 2401015, new Geology( "E_TypeAnomaly", "L_Phn_Part_Cld_015", (long)50000 ) );
            ENTRYIDS.Add( 2401016, new Geology( "K_TypeAnomoly", "L_Phn_Part_Cld_016", (long)50000 ) );
            ENTRYIDS.Add( 2401017, new Geology( "K_TypeAnomoly", "L_Phn_Part_Cld_017", (long)50000 ) );
            ENTRYIDS.Add( 2402001, new Geology( "T_TypeAnomaly", "L_Phn_Part_Clus_001", (long)50000 ) );
            ENTRYIDS.Add( 2402002, new Geology( "T_TypeAnomaly", "L_Phn_Part_Clus_002", (long)50000 ) );
            ENTRYIDS.Add( 2402003, new Geology( "L_TypeAnomoly", "L_Phn_Part_Clus_003", (long)50000 ) );
            ENTRYIDS.Add( 2402004, new Geology( "T_TypeAnomaly", "L_Phn_Part_Clus_004", (long)50000 ) );
            ENTRYIDS.Add( 2402005, new Geology( "T_TypeAnomaly", "L_Phn_Part_Clus_005", (long)50000 ) );
            ENTRYIDS.Add( 2402007, new Geology( "L_TypeAnomoly", "L_Phn_Part_Clus_007", (long)50000 ) );
            ENTRYIDS.Add( 2402008, new Geology( "L_TypeAnomoly", "L_Phn_Part_Clus_008", (long)50000 ) );
            ENTRYIDS.Add( 24020009, new Geology( "L_TypeAnomoly", "L_Phn_Part_Clus_009", (long)50000 ) );
            ENTRYIDS.Add( 24020010, new Geology( "L_TypeAnomoly", "L_Phn_Part_Clus_010", (long)50000 ) );
            ENTRYIDS.Add( 2402011, new Geology( "L_TypeAnomoly", "L_Phn_Part_Clus_011", (long)50000 ) );
            ENTRYIDS.Add( 2402012, new Geology( "L_TypeAnomoly", "L_Phn_Part_Clus_012", (long)50000 ) );
            ENTRYIDS.Add( 24020013, new Geology( "L_TypeAnomoly", "L_Phn_Part_Clus_013", (long)50000 ) );
            ENTRYIDS.Add( 2403002, new Geology( "P_TypeAnomaly", "L_Phn_Part_Eng_002", (long)50000 ) );
            ENTRYIDS.Add( 2403003, new Geology( "P_TypeAnomaly", "L_Phn_Part_Eng_003", (long)50000 ) );
            ENTRYIDS.Add( 2403004, new Geology( "P_TypeAnomaly", "L_Phn_Part_Eng_004", (long)50000 ) );
            ENTRYIDS.Add( 2403005, new Geology( "P_TypeAnomaly", "L_Phn_Part_Eng_005", (long)50000 ) );
            ENTRYIDS.Add( 2403006, new Geology( "P_TypeAnomaly", "L_Phn_Part_Eng_006", (long)50000 ) );
            ENTRYIDS.Add( 2403007, new Geology( "P_TypeAnomaly", "L_Phn_Part_Eng_007", (long)50000 ) );
            ENTRYIDS.Add( 2403008, new Geology( "P_TypeAnomaly", "L_Phn_Part_Eng_008", (long)50000 ) );
            ENTRYIDS.Add( 2403009, new Geology( "P_TypeAnomaly", "L_Phn_Part_Eng_009", (long)50000 ) );
            ENTRYIDS.Add( 2403010, new Geology( "P_TypeAnomaly", "L_Phn_Part_Eng_010", (long)50000 ) );
            ENTRYIDS.Add( 2403011, new Geology( "P_TypeAnomaly", "L_Phn_Part_Eng_011", (long)50000 ) );
            ENTRYIDS.Add( 2403012, new Geology( "P_TypeAnomaly", "L_Phn_Part_Eng_012", (long)50000 ) );
            ENTRYIDS.Add( 2403013, new Geology( "P_TypeAnomaly", "L_Phn_Part_Eng_013", (long)50000 ) );
            ENTRYIDS.Add( 2403014, new Geology( "P_TypeAnomaly", "L_Phn_Part_Eng_014", (long)50000 ) );
            ENTRYIDS.Add( 2403015, new Geology( "P_TypeAnomaly", "L_Phn_Part_Eng_015", (long)50000 ) );
            ENTRYIDS.Add( 2403016, new Geology( "P_TypeAnomaly", "L_Phn_Part_Eng_016", (long)50000 ) );
            ENTRYIDS.Add( 2406001, new Geology( "Q_TypeAnomaly", "L_Phn_Part_Orb_001", (long)50000 ) );
            ENTRYIDS.Add( 2406002, new Geology( "Q_TypeAnomaly", "L_Phn_Part_Orb_002", (long)50000 ) );
            ENTRYIDS.Add( 2406003, new Geology( "Q_TypeAnomaly", "L_Phn_Part_Orb_003", (long)50000 ) );
            ENTRYIDS.Add( 2406004, new Geology( "Q_TypeAnomaly", "L_Phn_Part_Orb_004", (long)50000 ) );
            ENTRYIDS.Add( 2406005, new Geology( "Q_TypeAnomaly", "L_Phn_Part_Orb_005", (long)50000 ) );
            ENTRYIDS.Add( 2406006, new Geology( "Q_TypeAnomaly", "L_Phn_Part_Orb_006", (long)50000 ) );
            ENTRYIDS.Add( 2406007, new Geology( "Q_TypeAnomaly", "L_Phn_Part_Orb_007", (long)50000 ) );
            ENTRYIDS.Add( 2406008, new Geology( "Q_TypeAnomaly", "L_Phn_Part_Orb_008", (long)50000 ) );
            ENTRYIDS.Add( 2406009, new Geology( "Q_TypeAnomaly", "L_Phn_Part_Orb_009", (long)50000 ) );
            ENTRYIDS.Add( 1400307, new Geology( "LavaSpout", "Lava_Spouts_IronMagma", (long)50000 ) );
            ENTRYIDS.Add( 1400306, new Geology( "LavaSpout", "Lava_Spouts_SilicateMagma", (long)50000 ) );

            GEOLOGICALS.Add( "Fumarole_CarbonDioxideGeysers", 1400109 );
            GEOLOGICALS.Add( "Fumarole_SilicateVapourGeysers", 1400114 );
            GEOLOGICALS.Add( "Fumarole_SulphurDioxideMagma", 1400102 );
            GEOLOGICALS.Add( "Fumarole_WaterGeysers", 1400108 );
            GEOLOGICALS.Add( "Gas_Clds_Blue", 1400601 );
            GEOLOGICALS.Add( "Gas_Clds_Green", 1400701 );
            GEOLOGICALS.Add( "Gas_Clds_Green_Storm", 1400702 );
            GEOLOGICALS.Add( "Gas_Clds_Light", 1401300 );
            GEOLOGICALS.Add( "Gas_Clds_Orange", 1400801 );
            GEOLOGICALS.Add( "Gas_Clds_Orange_Storm", 1400802 );
            GEOLOGICALS.Add( "Gas_Clds_Pink", 1400901 );
            GEOLOGICALS.Add( "Gas_Clds_Pink_Storm", 1400902 );
            GEOLOGICALS.Add( "Gas_Clds_Red", 1401001 );
            GEOLOGICALS.Add( "Gas_Clds_Red_Storm", 1401002 );
            GEOLOGICALS.Add( "Gas_Clds_Yellow", 1401101 );
            GEOLOGICALS.Add( "Gas_Clds_Yellow_Storm", 1401102 );
            GEOLOGICALS.Add( "Gas_Vents_CarbonDioxideGeysers", 1400409 );
            GEOLOGICALS.Add( "Gas_Vents_SilicateVapourGeysers", 1400414 );
            GEOLOGICALS.Add( "Gas_Vents_SulphurDioxideMagma", 1400402 );
            GEOLOGICALS.Add( "Gas_Vents_WaterGeysers", 1400408 );
            GEOLOGICALS.Add( "Geysers_WaterGeysers", 1400208 );
            GEOLOGICALS.Add( "IceFumarole_AmmoniaGeysers", 1400160 );
            GEOLOGICALS.Add( "IceFumarole_CarbonDioxideGeysers", 1400159 );
            GEOLOGICALS.Add( "IceFumarole_MethaneGeysers", 1400161 );
            GEOLOGICALS.Add( "IceFumarole_NitrogenGeysers", 1400162 );
            GEOLOGICALS.Add( "IceFumarole_SilicateVapourGeysers", 1400164 );
            GEOLOGICALS.Add( "IceFumarole_SulphurDioxideMagma", 1400152 );
            GEOLOGICALS.Add( "IceFumarole_WaterGeysers", 1400158 );
            GEOLOGICALS.Add( "IceGeysers_AmmoniaGeysers", 1400260 );
            GEOLOGICALS.Add( "IceGeysers_CarbonDioxideGeysers", 1400259 );
            GEOLOGICALS.Add( "IceGeysers_MethaneGeysers", 1400261 );
            GEOLOGICALS.Add( "IceGeysers_NitrogenGeysers", 1400262 );
            GEOLOGICALS.Add( "IceGeysers_WaterGeysers", 1400258 );
            GEOLOGICALS.Add( "L_Phn_Part_Cld_001", 2401001 );
            GEOLOGICALS.Add( "L_Phn_Part_Cld_002", 2401002 );
            GEOLOGICALS.Add( "L_Phn_Part_Cld_003", 2401003 );
            GEOLOGICALS.Add( "L_Phn_Part_Cld_004", 2401004 );
            GEOLOGICALS.Add( "L_Phn_Part_Cld_005", 2401005 );
            GEOLOGICALS.Add( "L_Phn_Part_Cld_006", 2401006 );
            GEOLOGICALS.Add( "L_Phn_Part_Cld_007", 2401007 );
            GEOLOGICALS.Add( "L_Phn_Part_Cld_008", 2401008 );
            GEOLOGICALS.Add( "L_Phn_Part_Cld_009", 2401009 );
            GEOLOGICALS.Add( "L_Phn_Part_Cld_010", 2401010 );
            GEOLOGICALS.Add( "L_Phn_Part_Cld_011", 2401011 );
            GEOLOGICALS.Add( "L_Phn_Part_Cld_012", 2401012 );
            GEOLOGICALS.Add( "L_Phn_Part_Cld_013", 2401013 );
            GEOLOGICALS.Add( "L_Phn_Part_Cld_014", 2401014 );
            GEOLOGICALS.Add( "L_Phn_Part_Cld_015", 2401015 );
            GEOLOGICALS.Add( "L_Phn_Part_Cld_016", 2401016 );
            GEOLOGICALS.Add( "L_Phn_Part_Cld_017", 2401017 );
            GEOLOGICALS.Add( "L_Phn_Part_Clus_001", 2402001 );
            GEOLOGICALS.Add( "L_Phn_Part_Clus_002", 2402002 );
            GEOLOGICALS.Add( "L_Phn_Part_Clus_003", 2402003 );
            GEOLOGICALS.Add( "L_Phn_Part_Clus_004", 2402004 );
            GEOLOGICALS.Add( "L_Phn_Part_Clus_005", 2402005 );
            GEOLOGICALS.Add( "L_Phn_Part_Clus_007", 2402007 );
            GEOLOGICALS.Add( "L_Phn_Part_Clus_008", 2402008 );
            GEOLOGICALS.Add( "L_Phn_Part_Clus_009", 24020009 );
            GEOLOGICALS.Add( "L_Phn_Part_Clus_010", 24020010 );
            GEOLOGICALS.Add( "L_Phn_Part_Clus_011", 2402011 );
            GEOLOGICALS.Add( "L_Phn_Part_Clus_012", 2402012 );
            GEOLOGICALS.Add( "L_Phn_Part_Clus_013", 24020013 );
            GEOLOGICALS.Add( "L_Phn_Part_Eng_002", 2403002 );
            GEOLOGICALS.Add( "L_Phn_Part_Eng_003", 2403003 );
            GEOLOGICALS.Add( "L_Phn_Part_Eng_004", 2403004 );
            GEOLOGICALS.Add( "L_Phn_Part_Eng_005", 2403005 );
            GEOLOGICALS.Add( "L_Phn_Part_Eng_006", 2403006 );
            GEOLOGICALS.Add( "L_Phn_Part_Eng_007", 2403007 );
            GEOLOGICALS.Add( "L_Phn_Part_Eng_008", 2403008 );
            GEOLOGICALS.Add( "L_Phn_Part_Eng_009", 2403009 );
            GEOLOGICALS.Add( "L_Phn_Part_Eng_010", 2403010 );
            GEOLOGICALS.Add( "L_Phn_Part_Eng_011", 2403011 );
            GEOLOGICALS.Add( "L_Phn_Part_Eng_012", 2403012 );
            GEOLOGICALS.Add( "L_Phn_Part_Eng_013", 2403013 );
            GEOLOGICALS.Add( "L_Phn_Part_Eng_014", 2403014 );
            GEOLOGICALS.Add( "L_Phn_Part_Eng_015", 2403015 );
            GEOLOGICALS.Add( "L_Phn_Part_Eng_016", 2403016 );
            GEOLOGICALS.Add( "L_Phn_Part_Orb_001", 2406001 );
            GEOLOGICALS.Add( "L_Phn_Part_Orb_002", 2406002 );
            GEOLOGICALS.Add( "L_Phn_Part_Orb_003", 2406003 );
            GEOLOGICALS.Add( "L_Phn_Part_Orb_004", 2406004 );
            GEOLOGICALS.Add( "L_Phn_Part_Orb_005", 2406005 );
            GEOLOGICALS.Add( "L_Phn_Part_Orb_006", 2406006 );
            GEOLOGICALS.Add( "L_Phn_Part_Orb_007", 2406007 );
            GEOLOGICALS.Add( "L_Phn_Part_Orb_008", 2406008 );
            GEOLOGICALS.Add( "L_Phn_Part_Orb_009", 2406009 );
            GEOLOGICALS.Add( "Lava_Spouts_IronMagma", 1400307 );
            GEOLOGICALS.Add( "Lava_Spouts_SilicateMagma", 1400306 );
        }

        // dummy used to ensure that the static constructor has run
        public Geology () : this( "" )
        { }

        private Geology ( string name ) : base( name, name )
        {
            this.exists = false;
            this.type = new GeologyType();
            this.value = 0;
            this.description = "";
        }

        private Geology ( string type, string name, long value ) : base( name, name )
        {
            this.exists = true;
            this.type = GeologyType.Lookup( type );
            this.value = value;
            this.description = rmGeologyDesc.GetString( name );
        }

        /// <summary>
        /// Try getting data from the entryid first, then use variant name as a fallback
        /// </summary>
        public static Geology Lookup ( long? entryId, string name )
        {
            Geology item;
            item = LookupByEntryId( entryId );
            if ( item == null )
            {
                item = LookupByName( name );
            }

            if ( item == null )
            {
                item = new Geology();
            }

            return item;
        }

        /// <summary>
        /// Preferred method of lookup
        /// </summary>
        public static Geology LookupByEntryId ( long? entryId )
        {
            if ( entryId != null )
            {
                if ( ENTRYIDS.ContainsKey( (long)entryId ) )
                {
                    return ENTRYIDS[ (long)entryId ];
                }
            }
            return null;
        }

        /// <summary>
        /// Populate objects with codex/extra information from the variant name.
        /// </summary>
        public static Geology LookupByName ( string name )
        {
            if ( name != "" )
            {
                if ( GEOLOGICALS.ContainsKey( name ) )
                {
                    long entryid = GEOLOGICALS[ name ];
                    return LookupByEntryId( entryid );
                }
            }
            return null;
        }
    }
}

using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace EddiDataDefinitions
{
    public class GeologyItem
    {
        public bool exists;                 // This item exists and has been populated with information
        public string class_name;
        public string class_description;
        public string name;
        public long? value;
        public string description;

        public GeologyItem ()
        {
            exists = false;
            this.class_name = "";
            this.class_description = "";
            this.name = "";
            this.value = 0;
            this.description = "";
        }

        public bool Exists ()
        {
            return exists;
        }

        public void SetExists ( bool exists )
        {
            this.exists = exists;
        }
    }

    static class GeologyInfo
    {
        public static ResourceManager rmGeoClassName = new ResourceManager("EddiDataDefinitions.Properties.GeologyClassName", Assembly.GetExecutingAssembly());
        public static ResourceManager rmGeoClassDesc = new ResourceManager("EddiDataDefinitions.Properties.GeologyClassDesc", Assembly.GetExecutingAssembly());
        public static ResourceManager rmGeoName = new ResourceManager("EddiDataDefinitions.Properties.GeologyName", Assembly.GetExecutingAssembly());
        public static ResourceManager rmGeoDesc = new ResourceManager("EddiDataDefinitions.Properties.GeologyDesc", Assembly.GetExecutingAssembly());

        public class LookupEntryId
        {
            public string geoClass;
            public string name;
            public long? value;

            public LookupEntryId ( string geoClass, string name, long? value )
            {
                this.geoClass = geoClass;
                this.name = name;
                this.value = value;
            }
        }

        public class LookupName
        {
            public long? entryId;
            public string geoClass;
            public long? value;

            public LookupName ( long? entryId, string geoClass, long? value )
            {
                this.entryId = entryId;
                this.geoClass = geoClass;
                this.value = value;
            }
        }

        // For easier reverse lookups
        public static Dictionary<long, LookupEntryId> EntryIdData = new Dictionary<long, LookupEntryId>();
        public static Dictionary<string, LookupName> NameData = new Dictionary<string, LookupName>();

        static GeologyInfo ()
        {
            EntryIdData.Add( 1400109, new LookupEntryId( "Fumarole", "Fumarole_CarbonDioxideGeysers", (long?)50000 ) );
            EntryIdData.Add( 1400114, new LookupEntryId( "Fumarole", "Fumarole_SilicateVapourGeysers", (long?)50000 ) );
            EntryIdData.Add( 1400102, new LookupEntryId( "Fumarole", "Fumarole_SulphurDioxideMagma", (long?)50000 ) );
            EntryIdData.Add( 1400108, new LookupEntryId( "Fumarole", "Fumarole_WaterGeysers", (long?)50000 ) );
            EntryIdData.Add( 1400601, new LookupEntryId( "LagrangeCloud", "Gas_Clds_Blue", (long?)50000 ) );
            EntryIdData.Add( 1400701, new LookupEntryId( "LagrangeCloud", "Gas_Clds_Green", (long?)50000 ) );
            EntryIdData.Add( 1400702, new LookupEntryId( "LagrangeCloud", "Gas_Clds_Green_Storm", (long?)50000 ) );
            EntryIdData.Add( 1401300, new LookupEntryId( "LagrangeCloud", "Gas_Clds_Light", (long?)50000 ) );
            EntryIdData.Add( 1400801, new LookupEntryId( "LagrangeCloud", "Gas_Clds_Orange", (long?)50000 ) );
            EntryIdData.Add( 1400802, new LookupEntryId( "LagrangeCloud", "Gas_Clds_Orange_Storm", (long?)50000 ) );
            EntryIdData.Add( 1400901, new LookupEntryId( "LagrangeCloud", "Gas_Clds_Pink", (long?)50000 ) );
            EntryIdData.Add( 1400902, new LookupEntryId( "LagrangeCloud", "Gas_Clds_Pink_Storm", (long?)50000 ) );
            EntryIdData.Add( 1401001, new LookupEntryId( "LagrangeCloud", "Gas_Clds_Red", (long?)50000 ) );
            EntryIdData.Add( 1401002, new LookupEntryId( "LagrangeCloud", "Gas_Clds_Red_Storm", (long?)50000 ) );
            EntryIdData.Add( 1401101, new LookupEntryId( "LagrangeCloud", "Gas_Clds_Yellow", (long?)50000 ) );
            EntryIdData.Add( 1401102, new LookupEntryId( "LagrangeCloud", "Gas_Clds_Yellow_Storm", (long?)50000 ) );
            EntryIdData.Add( 1400409, new LookupEntryId( "GasVent", "Gas_Vents_CarbonDioxideGeysers", (long?)50000 ) );
            EntryIdData.Add( 1400414, new LookupEntryId( "GasVent", "Gas_Vents_SilicateVapourGeysers", (long?)50000 ) );
            EntryIdData.Add( 1400402, new LookupEntryId( "GasVent", "Gas_Vents_SulphurDioxideMagma", (long?)50000 ) );
            EntryIdData.Add( 1400408, new LookupEntryId( "GasVent", "Gas_Vents_WaterGeysers", (long?)50000 ) );
            EntryIdData.Add( 1400208, new LookupEntryId( "WaterGeyser", "Geysers_WaterGeysers", (long?)50000 ) );
            EntryIdData.Add( 1400160, new LookupEntryId( "IceFumarole", "IceFumarole_AmmoniaGeysers", (long?)50000 ) );
            EntryIdData.Add( 1400159, new LookupEntryId( "IceFumarole", "IceFumarole_CarbonDioxideGeysers", (long?)50000 ) );
            EntryIdData.Add( 1400161, new LookupEntryId( "IceFumarole", "IceFumarole_MethaneGeysers", (long?)50000 ) );
            EntryIdData.Add( 1400162, new LookupEntryId( "IceFumarole", "IceFumarole_NitrogenGeysers", (long?)50000 ) );
            EntryIdData.Add( 1400164, new LookupEntryId( "IceFumarole", "IceFumarole_SilicateVapourGeysers", (long?)50000 ) );
            EntryIdData.Add( 1400152, new LookupEntryId( "IceFumarole", "IceFumarole_SulphurDioxideMagma", (long?)50000 ) );
            EntryIdData.Add( 1400158, new LookupEntryId( "IceFumarole", "IceFumarole_WaterGeysers", (long?)50000 ) );
            EntryIdData.Add( 1400260, new LookupEntryId( "IceGeyser", "IceGeysers_AmmoniaGeysers", (long?)50000 ) );
            EntryIdData.Add( 1400259, new LookupEntryId( "IceGeyser", "IceGeysers_CarbonDioxideGeysers", (long?)50000 ) );
            EntryIdData.Add( 1400261, new LookupEntryId( "IceGeyser", "IceGeysers_MethaneGeysers", (long?)50000 ) );
            EntryIdData.Add( 1400262, new LookupEntryId( "IceGeyser", "IceGeysers_NitrogenGeysers", (long?)50000 ) );
            EntryIdData.Add( 1400258, new LookupEntryId( "IceGeyser", "IceGeysers_WaterGeysers", (long?)50000 ) );
            EntryIdData.Add( 2401001, new LookupEntryId( "K_TypeAnomoly", "L_Phn_Part_Cld_001", (long?)50000 ) );
            EntryIdData.Add( 2401002, new LookupEntryId( "K_TypeAnomoly", "L_Phn_Part_Cld_002", (long?)50000 ) );
            EntryIdData.Add( 2401003, new LookupEntryId( "K_TypeAnomoly", "L_Phn_Part_Cld_003", (long?)50000 ) );
            EntryIdData.Add( 2401004, new LookupEntryId( "K_TypeAnomoly", "L_Phn_Part_Cld_004", (long?)50000 ) );
            EntryIdData.Add( 2401005, new LookupEntryId( "K_TypeAnomoly", "L_Phn_Part_Cld_005", (long?)50000 ) );
            EntryIdData.Add( 2401006, new LookupEntryId( "K_TypeAnomoly", "L_Phn_Part_Cld_006", (long?)50000 ) );
            EntryIdData.Add( 2401007, new LookupEntryId( "E_TypeAnomaly", "L_Phn_Part_Cld_007", (long?)50000 ) );
            EntryIdData.Add( 2401008, new LookupEntryId( "K_TypeAnomoly", "L_Phn_Part_Cld_008", (long?)50000 ) );
            EntryIdData.Add( 2401009, new LookupEntryId( "K_TypeAnomoly", "L_Phn_Part_Cld_009", (long?)50000 ) );
            EntryIdData.Add( 2401010, new LookupEntryId( "K_TypeAnomoly", "L_Phn_Part_Cld_010", (long?)50000 ) );
            EntryIdData.Add( 2401011, new LookupEntryId( "K_TypeAnomoly", "L_Phn_Part_Cld_011", (long?)50000 ) );
            EntryIdData.Add( 2401012, new LookupEntryId( "K_TypeAnomoly", "L_Phn_Part_Cld_012", (long?)50000 ) );
            EntryIdData.Add( 2401013, new LookupEntryId( "E_TypeAnomaly", "L_Phn_Part_Cld_013", (long?)50000 ) );
            EntryIdData.Add( 2401014, new LookupEntryId( "E_TypeAnomaly", "L_Phn_Part_Cld_014", (long?)50000 ) );
            EntryIdData.Add( 2401015, new LookupEntryId( "E_TypeAnomaly", "L_Phn_Part_Cld_015", (long?)50000 ) );
            EntryIdData.Add( 2401016, new LookupEntryId( "K_TypeAnomoly", "L_Phn_Part_Cld_016", (long?)50000 ) );
            EntryIdData.Add( 2401017, new LookupEntryId( "K_TypeAnomoly", "L_Phn_Part_Cld_017", (long?)50000 ) );
            EntryIdData.Add( 2402001, new LookupEntryId( "T_TypeAnomaly", "L_Phn_Part_Clus_001", (long?)50000 ) );
            EntryIdData.Add( 2402002, new LookupEntryId( "T_TypeAnomaly", "L_Phn_Part_Clus_002", (long?)50000 ) );
            EntryIdData.Add( 2402003, new LookupEntryId( "L_TypeAnomoly", "L_Phn_Part_Clus_003", (long?)50000 ) );
            EntryIdData.Add( 2402004, new LookupEntryId( "T_TypeAnomaly", "L_Phn_Part_Clus_004", (long?)50000 ) );
            EntryIdData.Add( 2402005, new LookupEntryId( "T_TypeAnomaly", "L_Phn_Part_Clus_005", (long?)50000 ) );
            EntryIdData.Add( 2402007, new LookupEntryId( "L_TypeAnomoly", "L_Phn_Part_Clus_007", (long?)50000 ) );
            EntryIdData.Add( 2402008, new LookupEntryId( "L_TypeAnomoly", "L_Phn_Part_Clus_008", (long?)50000 ) );
            EntryIdData.Add( 24020009, new LookupEntryId( "L_TypeAnomoly", "L_Phn_Part_Clus_009", (long?)50000 ) );
            EntryIdData.Add( 24020010, new LookupEntryId( "L_TypeAnomoly", "L_Phn_Part_Clus_010", (long?)50000 ) );
            EntryIdData.Add( 2402011, new LookupEntryId( "L_TypeAnomoly", "L_Phn_Part_Clus_011", (long?)50000 ) );
            EntryIdData.Add( 2402012, new LookupEntryId( "L_TypeAnomoly", "L_Phn_Part_Clus_012", (long?)50000 ) );
            EntryIdData.Add( 24020013, new LookupEntryId( "L_TypeAnomoly", "L_Phn_Part_Clus_013", (long?)50000 ) );
            EntryIdData.Add( 2403002, new LookupEntryId( "P_TypeAnomaly", "L_Phn_Part_Eng_002", (long?)50000 ) );
            EntryIdData.Add( 2403003, new LookupEntryId( "P_TypeAnomaly", "L_Phn_Part_Eng_003", (long?)50000 ) );
            EntryIdData.Add( 2403004, new LookupEntryId( "P_TypeAnomaly", "L_Phn_Part_Eng_004", (long?)50000 ) );
            EntryIdData.Add( 2403005, new LookupEntryId( "P_TypeAnomaly", "L_Phn_Part_Eng_005", (long?)50000 ) );
            EntryIdData.Add( 2403006, new LookupEntryId( "P_TypeAnomaly", "L_Phn_Part_Eng_006", (long?)50000 ) );
            EntryIdData.Add( 2403007, new LookupEntryId( "P_TypeAnomaly", "L_Phn_Part_Eng_007", (long?)50000 ) );
            EntryIdData.Add( 2403008, new LookupEntryId( "P_TypeAnomaly", "L_Phn_Part_Eng_008", (long?)50000 ) );
            EntryIdData.Add( 2403009, new LookupEntryId( "P_TypeAnomaly", "L_Phn_Part_Eng_009", (long?)50000 ) );
            EntryIdData.Add( 2403010, new LookupEntryId( "P_TypeAnomaly", "L_Phn_Part_Eng_010", (long?)50000 ) );
            EntryIdData.Add( 2403011, new LookupEntryId( "P_TypeAnomaly", "L_Phn_Part_Eng_011", (long?)50000 ) );
            EntryIdData.Add( 2403012, new LookupEntryId( "P_TypeAnomaly", "L_Phn_Part_Eng_012", (long?)50000 ) );
            EntryIdData.Add( 2403013, new LookupEntryId( "P_TypeAnomaly", "L_Phn_Part_Eng_013", (long?)50000 ) );
            EntryIdData.Add( 2403014, new LookupEntryId( "P_TypeAnomaly", "L_Phn_Part_Eng_014", (long?)50000 ) );
            EntryIdData.Add( 2403015, new LookupEntryId( "P_TypeAnomaly", "L_Phn_Part_Eng_015", (long?)50000 ) );
            EntryIdData.Add( 2403016, new LookupEntryId( "P_TypeAnomaly", "L_Phn_Part_Eng_016", (long?)50000 ) );
            EntryIdData.Add( 2406001, new LookupEntryId( "Q_TypeAnomaly", "L_Phn_Part_Orb_001", (long?)50000 ) );
            EntryIdData.Add( 2406002, new LookupEntryId( "Q_TypeAnomaly", "L_Phn_Part_Orb_002", (long?)50000 ) );
            EntryIdData.Add( 2406003, new LookupEntryId( "Q_TypeAnomaly", "L_Phn_Part_Orb_003", (long?)50000 ) );
            EntryIdData.Add( 2406004, new LookupEntryId( "Q_TypeAnomaly", "L_Phn_Part_Orb_004", (long?)50000 ) );
            EntryIdData.Add( 2406005, new LookupEntryId( "Q_TypeAnomaly", "L_Phn_Part_Orb_005", (long?)50000 ) );
            EntryIdData.Add( 2406006, new LookupEntryId( "Q_TypeAnomaly", "L_Phn_Part_Orb_006", (long?)50000 ) );
            EntryIdData.Add( 2406007, new LookupEntryId( "Q_TypeAnomaly", "L_Phn_Part_Orb_007", (long?)50000 ) );
            EntryIdData.Add( 2406008, new LookupEntryId( "Q_TypeAnomaly", "L_Phn_Part_Orb_008", (long?)50000 ) );
            EntryIdData.Add( 2406009, new LookupEntryId( "Q_TypeAnomaly", "L_Phn_Part_Orb_009", (long?)50000 ) );
            EntryIdData.Add( 1400307, new LookupEntryId( "LavaSpout", "Lava_Spouts_IronMagma", (long?)50000 ) );
            EntryIdData.Add( 1400306, new LookupEntryId( "LavaSpout", "Lava_Spouts_SilicateMagma", (long?)50000 ) );

            NameData.Add( "Fumarole_CarbonDioxideGeysers", new LookupName( (long?)1400109, "Fumarole", (long?)50000 ) );
            NameData.Add( "Fumarole_SilicateVapourGeysers", new LookupName( (long?)1400114, "Fumarole", (long?)50000 ) );
            NameData.Add( "Fumarole_SulphurDioxideMagma", new LookupName( (long?)1400102, "Fumarole", (long?)50000 ) );
            NameData.Add( "Fumarole_WaterGeysers", new LookupName( (long?)1400108, "Fumarole", (long?)50000 ) );
            NameData.Add( "Gas_Clds_Blue", new LookupName( (long?)1400601, "LagrangeCloud", (long?)50000 ) );
            NameData.Add( "Gas_Clds_Green", new LookupName( (long?)1400701, "LagrangeCloud", (long?)50000 ) );
            NameData.Add( "Gas_Clds_Green_Storm", new LookupName( (long?)1400702, "LagrangeCloud", (long?)50000 ) );
            NameData.Add( "Gas_Clds_Light", new LookupName( (long?)1401300, "LagrangeCloud", (long?)50000 ) );
            NameData.Add( "Gas_Clds_Orange", new LookupName( (long?)1400801, "LagrangeCloud", (long?)50000 ) );
            NameData.Add( "Gas_Clds_Orange_Storm", new LookupName( (long?)1400802, "LagrangeCloud", (long?)50000 ) );
            NameData.Add( "Gas_Clds_Pink", new LookupName( (long?)1400901, "LagrangeCloud", (long?)50000 ) );
            NameData.Add( "Gas_Clds_Pink_Storm", new LookupName( (long?)1400902, "LagrangeCloud", (long?)50000 ) );
            NameData.Add( "Gas_Clds_Red", new LookupName( (long?)1401001, "LagrangeCloud", (long?)50000 ) );
            NameData.Add( "Gas_Clds_Red_Storm", new LookupName( (long?)1401002, "LagrangeCloud", (long?)50000 ) );
            NameData.Add( "Gas_Clds_Yellow", new LookupName( (long?)1401101, "LagrangeCloud", (long?)50000 ) );
            NameData.Add( "Gas_Clds_Yellow_Storm", new LookupName( (long?)1401102, "LagrangeCloud", (long?)50000 ) );
            NameData.Add( "Gas_Vents_CarbonDioxideGeysers", new LookupName( (long?)1400409, "GasVent", (long?)50000 ) );
            NameData.Add( "Gas_Vents_SilicateVapourGeysers", new LookupName( (long?)1400414, "GasVent", (long?)50000 ) );
            NameData.Add( "Gas_Vents_SulphurDioxideMagma", new LookupName( (long?)1400402, "GasVent", (long?)50000 ) );
            NameData.Add( "Gas_Vents_WaterGeysers", new LookupName( (long?)1400408, "GasVent", (long?)50000 ) );
            NameData.Add( "Geysers_WaterGeysers", new LookupName( (long?)1400208, "WaterGeyser", (long?)50000 ) );
            NameData.Add( "IceFumarole_AmmoniaGeysers", new LookupName( (long?)1400160, "IceFumarole", (long?)50000 ) );
            NameData.Add( "IceFumarole_CarbonDioxideGeysers", new LookupName( (long?)1400159, "IceFumarole", (long?)50000 ) );
            NameData.Add( "IceFumarole_MethaneGeysers", new LookupName( (long?)1400161, "IceFumarole", (long?)50000 ) );
            NameData.Add( "IceFumarole_NitrogenGeysers", new LookupName( (long?)1400162, "IceFumarole", (long?)50000 ) );
            NameData.Add( "IceFumarole_SilicateVapourGeysers", new LookupName( (long?)1400164, "IceFumarole", (long?)50000 ) );
            NameData.Add( "IceFumarole_SulphurDioxideMagma", new LookupName( (long?)1400152, "IceFumarole", (long?)50000 ) );
            NameData.Add( "IceFumarole_WaterGeysers", new LookupName( (long?)1400158, "IceFumarole", (long?)50000 ) );
            NameData.Add( "IceGeysers_AmmoniaGeysers", new LookupName( (long?)1400260, "IceGeyser", (long?)50000 ) );
            NameData.Add( "IceGeysers_CarbonDioxideGeysers", new LookupName( (long?)1400259, "IceGeyser", (long?)50000 ) );
            NameData.Add( "IceGeysers_MethaneGeysers", new LookupName( (long?)1400261, "IceGeyser", (long?)50000 ) );
            NameData.Add( "IceGeysers_NitrogenGeysers", new LookupName( (long?)1400262, "IceGeyser", (long?)50000 ) );
            NameData.Add( "IceGeysers_WaterGeysers", new LookupName( (long?)1400258, "IceGeyser", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Cld_001", new LookupName( (long?)2401001, "K_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Cld_002", new LookupName( (long?)2401002, "K_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Cld_003", new LookupName( (long?)2401003, "K_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Cld_004", new LookupName( (long?)2401004, "K_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Cld_005", new LookupName( (long?)2401005, "K_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Cld_006", new LookupName( (long?)2401006, "K_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Cld_007", new LookupName( (long?)2401007, "E_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Cld_008", new LookupName( (long?)2401008, "K_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Cld_009", new LookupName( (long?)2401009, "K_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Cld_010", new LookupName( (long?)2401010, "K_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Cld_011", new LookupName( (long?)2401011, "K_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Cld_012", new LookupName( (long?)2401012, "K_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Cld_013", new LookupName( (long?)2401013, "E_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Cld_014", new LookupName( (long?)2401014, "E_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Cld_015", new LookupName( (long?)2401015, "E_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Cld_016", new LookupName( (long?)2401016, "K_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Cld_017", new LookupName( (long?)2401017, "K_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Clus_001", new LookupName( (long?)2402001, "T_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Clus_002", new LookupName( (long?)2402002, "T_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Clus_003", new LookupName( (long?)2402003, "L_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Clus_004", new LookupName( (long?)2402004, "T_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Clus_005", new LookupName( (long?)2402005, "T_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Clus_007", new LookupName( (long?)2402007, "L_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Clus_008", new LookupName( (long?)2402008, "L_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Clus_009", new LookupName( (long?)24020009, "L_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Clus_010", new LookupName( (long?)24020010, "L_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Clus_011", new LookupName( (long?)2402011, "L_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Clus_012", new LookupName( (long?)2402012, "L_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Clus_013", new LookupName( (long?)24020013, "L_TypeAnomoly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Eng_002", new LookupName( (long?)2403002, "P_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Eng_003", new LookupName( (long?)2403003, "P_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Eng_004", new LookupName( (long?)2403004, "P_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Eng_005", new LookupName( (long?)2403005, "P_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Eng_006", new LookupName( (long?)2403006, "P_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Eng_007", new LookupName( (long?)2403007, "P_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Eng_008", new LookupName( (long?)2403008, "P_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Eng_009", new LookupName( (long?)2403009, "P_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Eng_010", new LookupName( (long?)2403010, "P_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Eng_011", new LookupName( (long?)2403011, "P_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Eng_012", new LookupName( (long?)2403012, "P_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Eng_013", new LookupName( (long?)2403013, "P_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Eng_014", new LookupName( (long?)2403014, "P_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Eng_015", new LookupName( (long?)2403015, "P_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Eng_016", new LookupName( (long?)2403016, "P_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Orb_001", new LookupName( (long?)2406001, "Q_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Orb_002", new LookupName( (long?)2406002, "Q_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Orb_003", new LookupName( (long?)2406003, "Q_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Orb_004", new LookupName( (long?)2406004, "Q_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Orb_005", new LookupName( (long?)2406005, "Q_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Orb_006", new LookupName( (long?)2406006, "Q_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Orb_007", new LookupName( (long?)2406007, "Q_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Orb_008", new LookupName( (long?)2406008, "Q_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "L_Phn_Part_Orb_009", new LookupName( (long?)2406009, "Q_TypeAnomaly", (long?)50000 ) );
            NameData.Add( "Lava_Spouts_IronMagma", new LookupName( (long?)1400307, "LavaSpout", (long?)50000 ) );
            NameData.Add( "Lava_Spouts_SilicateMagma", new LookupName( (long?)1400306, "LavaSpout", (long?)50000 ) );




        }

        /// <summary>
        /// Try getting data fro mthe entryid first, then use variant name as a fallback
        /// </summary>
        public static GeologyItem Lookup ( long? entryId, string edname )
        {
            GeologyItem item = new GeologyItem();
            item = LookupByEntryId( entryId );

            if ( !item.exists )
            {
                item = LookupByName( edname );
            }

            return item;
        }

        public static GeologyItem LookupByEntryId ( long? entryId )
        {
            GeologyItem item = new GeologyItem();

            if ( entryId != null )
            {
                if ( EntryIdData.ContainsKey( (long)entryId ) )
                {

                    LookupEntryId data = EntryIdData[ (long)entryId ];

                    item.class_name = rmGeoClassName.GetString( data.geoClass );
                    item.class_description = rmGeoClassDesc.GetString( data.geoClass );

                    item.name = rmGeoName.GetString( data.name );
                    item.value = data.value;
                    item.description = rmGeoDesc.GetString( data.name );

                    item.SetExists( true );
                }
            }

            return item;
        }

        public static GeologyItem LookupByName ( string edname )
        {
            GeologyItem item = new GeologyItem();

            if ( edname != "" )
            {
                if ( NameData.ContainsKey( edname ) )
                {
                    LookupName data = NameData[ edname ];

                    item.class_name = rmGeoClassName.GetString( data.geoClass );
                    item.class_description = rmGeoClassDesc.GetString( data.geoClass );

                    item.name = rmGeoName.GetString( edname );
                    item.value = data.value;
                    item.description = rmGeoDesc.GetString( edname );

                    item.SetExists( true );
                }
            }

            // If the above fails to find an entry then we return the empty item
            // We could modify the item to say that we could not find an entry as well
            return item;
        }
    }
}

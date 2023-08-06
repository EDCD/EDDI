using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace EddiDataDefinitions
{
    public class AstrometricItem
    {
        public bool exists;   // This item exists and has been populated with information
        public string name;
        public string subCategory;
        public string description;

        public AstrometricItem ()
        {
            exists = false;
            this.name = "";
            this.subCategory = "";
            this.description = "";
        }

        public AstrometricItem ( string name, string subCategory, string desc )
        {
            exists = true;
            this.name = name;
            this.subCategory = subCategory;
            this.description = desc;
        }

        public bool Exists ()
        {
            return this.exists;
        }

        public void SetExists( bool exists ) {
            this.exists = exists;
        }
    }

    static class AstrometricInfo
    {
        public static ResourceManager rmAstroName = new ResourceManager("EddiDataDefinitions.Properties.AstronomicalName", Assembly.GetExecutingAssembly());
        public static ResourceManager rmAstroDesc = new ResourceManager("EddiDataDefinitions.Properties.AstronomicalDesc", Assembly.GetExecutingAssembly());
        public static ResourceManager rmAstroSubCategory = new ResourceManager("EddiDataDefinitions.Properties.AstronomicalSubCategory", Assembly.GetExecutingAssembly());

        public class LookupEntryId
        {
            public string className;
            public string edname;

            public LookupEntryId ( string className, string edname )
            {
                this.edname = edname;
                this.className = className;
            }
        }

        public class LookupName
        {
            public string className;
            public long? entryid;

            public LookupName ( long? entryid, string className )
            {
                this.entryid = entryid;
                this.className = className;
            }
        }

        public static Dictionary<long, LookupEntryId> EntryIdData = new Dictionary<long, LookupEntryId>();
        public static Dictionary<string, LookupName> NameData = new Dictionary<string, LookupName>();
        public static Dictionary<string, string> SubCategory = new Dictionary<string, string>();

        static AstrometricInfo ()
        {
            // Items with missing entryid's are commented out
            EntryIdData.Add( 1200402, new LookupEntryId( "Gas_Giants", "Green_Giant_With_Ammonia_Life" ) );
            EntryIdData.Add( 1200602, new LookupEntryId( "Gas_Giants", "Green_Sudarsky_Class_II" ) );
            EntryIdData.Add( 1200802, new LookupEntryId( "Gas_Giants", "Green_Sudarsky_Class_IV" ) );
            EntryIdData.Add( 1200902, new LookupEntryId( "Gas_Giants", "Green_Sudarsky_Class_V" ) );
            EntryIdData.Add( 1200102, new LookupEntryId( "Gas_Giants", "Green_Water_Giant" ) );
            EntryIdData.Add( 1200401, new LookupEntryId( "Gas_Giants", "Standard_Giant_With_Ammonia_Life" ) );
            EntryIdData.Add( 1200301, new LookupEntryId( "Gas_Giants", "Standard_Giant_With_Water_Life" ) );
            // EntryIdData.Add(, new LookupEntryId("Gas_Giants", "Standard_Helium" ) );
            EntryIdData.Add( 1201001, new LookupEntryId( "Gas_Giants", "Standard_Helium_Rich" ) );
            EntryIdData.Add( 1200501, new LookupEntryId( "Gas_Giants", "Standard_Sudarsky_Class_I" ) );
            EntryIdData.Add( 1200601, new LookupEntryId( "Gas_Giants", "Standard_Sudarsky_Class_II" ) );
            EntryIdData.Add( 1200701, new LookupEntryId( "Gas_Giants", "Standard_Sudarsky_Class_III" ) );
            EntryIdData.Add( 1200801, new LookupEntryId( "Gas_Giants", "Standard_Sudarsky_Class_IV" ) );
            EntryIdData.Add( 1200901, new LookupEntryId( "Gas_Giants", "Standard_Sudarsky_Class_V" ) );
            EntryIdData.Add( 1200101, new LookupEntryId( "Gas_Giants", "Standard_Water_Giant" ) );
            EntryIdData.Add( 1200302, new LookupEntryId( "Gas_Giants", "Green_Giant_With_Water_Life" ) );
            EntryIdData.Add( 1200502, new LookupEntryId( "Gas_Giants", "Green_Sudarsky_Class_I" ) );
            EntryIdData.Add( 1200702, new LookupEntryId( "Gas_Giants", "Green_Sudarsky_Class_III" ) );
            EntryIdData.Add( 1100301, new LookupEntryId( "Stars", "A_Type" ) );
            EntryIdData.Add( 1100302, new LookupEntryId( "Stars", "A_TypeGiant" ) );
            EntryIdData.Add( 1100303, new LookupEntryId( "Stars", "A_TypeSuperGiant" ) );
            EntryIdData.Add( 1101101, new LookupEntryId( "Stars", "AEBE_Type" ) );
            EntryIdData.Add( 1100201, new LookupEntryId( "Stars", "B_Type" ) );
            EntryIdData.Add( 1100202, new LookupEntryId( "Stars", "B_TypeGiant" ) );
            EntryIdData.Add( 1100203, new LookupEntryId( "Stars", "B_TypeSuperGiant" ) );
            EntryIdData.Add( 1102400, new LookupEntryId( "Stars", "Black_Holes" ) );
            EntryIdData.Add( 1101401, new LookupEntryId( "Stars", "C_Type" ) );
            EntryIdData.Add( 1101402, new LookupEntryId( "Stars", "C_TypeGiant" ) );
            EntryIdData.Add( 1101404, new LookupEntryId( "Stars", "C_TypeHyperGiant" ) );
            EntryIdData.Add( 1101403, new LookupEntryId( "Stars", "C_TypeSuperGiant" ) );
            // EntryIdData.Add(, new LookupEntryId("Stars", "CJ_Type" ) );
            // EntryIdData.Add(, new LookupEntryId("Stars", "CN_Type" ) );
            EntryIdData.Add( 1102201, new LookupEntryId( "Stars", "D_Type" ) );
            EntryIdData.Add( 1102202, new LookupEntryId( "Stars", "DA_Type" ) );
            EntryIdData.Add( 1102203, new LookupEntryId( "Stars", "DAB_Type" ) );
            EntryIdData.Add( 1102205, new LookupEntryId( "Stars", "DAV_Type" ) );
            EntryIdData.Add( 1102206, new LookupEntryId( "Stars", "DAZ_Type" ) );
            EntryIdData.Add( 1102207, new LookupEntryId( "Stars", "DB_Type" ) );
            EntryIdData.Add( 1102208, new LookupEntryId( "Stars", "DBV_Type" ) );
            // EntryIdData.Add(, new LookupEntryId("Stars", "DBZ_Type" ) );
            EntryIdData.Add( 1102213, new LookupEntryId( "Stars", "DC_Type" ) );
            // EntryIdData.Add(, new LookupEntryId("Stars", "DCV_Type" ) );
            EntryIdData.Add( 1102212, new LookupEntryId( "Stars", "DQ_Type" ) );
            EntryIdData.Add( 1100401, new LookupEntryId( "Stars", "F_Type" ) );
            EntryIdData.Add( 1100402, new LookupEntryId( "Stars", "F_TypeGiant" ) );
            EntryIdData.Add( 1100403, new LookupEntryId( "Stars", "F_TypeSuperGiant" ) );
            EntryIdData.Add( 1100501, new LookupEntryId( "Stars", "G_Type" ) );
            EntryIdData.Add( 1100502, new LookupEntryId( "Stars", "G_TypeGiant" ) );
            EntryIdData.Add( 1100503, new LookupEntryId( "Stars", "G_TypeSuperGiant" ) );
            EntryIdData.Add( 1100601, new LookupEntryId( "Stars", "K_Type" ) );
            EntryIdData.Add( 1100602, new LookupEntryId( "Stars", "K_TypeGiant" ) );
            EntryIdData.Add( 1100603, new LookupEntryId( "Stars", "K_TypeSuperGiant" ) );
            EntryIdData.Add( 1100801, new LookupEntryId( "Stars", "L_Type" ) );
            EntryIdData.Add( 1100701, new LookupEntryId( "Stars", "M_Type" ) );
            EntryIdData.Add( 1100702, new LookupEntryId( "Stars", "M_TypeGiant" ) );
            EntryIdData.Add( 1100703, new LookupEntryId( "Stars", "M_TypeSuperGiant" ) );
            // EntryIdData.Add(, new LookupEntryId("Stars", "MS_Type" ) );
            EntryIdData.Add( 1102300, new LookupEntryId( "Stars", "Neutron_Stars" ) );
            EntryIdData.Add( 1100101, new LookupEntryId( "Stars", "O_Type" ) );
            EntryIdData.Add( 1100102, new LookupEntryId( "Stars", "O_TypeGiant" ) );
            EntryIdData.Add( 1100103, new LookupEntryId( "Stars", "O_TypeSuperGiant" ) );
            EntryIdData.Add( 1102001, new LookupEntryId( "Stars", "S_Type" ) );
            EntryIdData.Add( 1102002, new LookupEntryId( "Stars", "S_TypeGiant" ) );
            EntryIdData.Add( 1102500, new LookupEntryId( "Stars", "SupermassiveBlack_Holes" ) );
            EntryIdData.Add( 1100901, new LookupEntryId( "Stars", "T_Type" ) );
            EntryIdData.Add( 1101001, new LookupEntryId( "Stars", "TTS_Type" ) );
            EntryIdData.Add( 1102101, new LookupEntryId( "Stars", "W_Type" ) );
            EntryIdData.Add( 1102102, new LookupEntryId( "Stars", "WC_Type" ) );
            EntryIdData.Add( 1102103, new LookupEntryId( "Stars", "WN_Type" ) );
            EntryIdData.Add( 1102104, new LookupEntryId( "Stars", "WNC_Type" ) );
            EntryIdData.Add( 1102105, new LookupEntryId( "Stars", "WO_Type" ) );
            EntryIdData.Add( 1101201, new LookupEntryId( "Stars", "Y_Type" ) );
            EntryIdData.Add( 1300100, new LookupEntryId( "Terrestrials", "Earth_Likes" ) );
            EntryIdData.Add( 1300202, new LookupEntryId( "Terrestrials", "Standard_Ammonia_Worlds" ) );
            EntryIdData.Add( 1300501, new LookupEntryId( "Terrestrials", "Standard_High_Metal_Content_No_Atmos" ) );
            EntryIdData.Add( 1300801, new LookupEntryId( "Terrestrials", "Standard_Ice_No_Atmos" ) );
            EntryIdData.Add( 1300401, new LookupEntryId( "Terrestrials", "Standard_Metal_Rich_No_Atmos" ) );
            EntryIdData.Add( 1300701, new LookupEntryId( "Terrestrials", "Standard_Rocky_Ice_No_Atmos" ) );
            EntryIdData.Add( 1300601, new LookupEntryId( "Terrestrials", "Standard_Rocky_No_Atmos" ) );
            EntryIdData.Add( 1301501, new LookupEntryId( "Terrestrials", "Standard_Ter_High_Metal_Content" ) );
            EntryIdData.Add( 1301801, new LookupEntryId( "Terrestrials", "Standard_Ter_Ice" ) );
            EntryIdData.Add( 1301401, new LookupEntryId( "Terrestrials", "Standard_Ter_Metal_Rich" ) );
            EntryIdData.Add( 1301701, new LookupEntryId( "Terrestrials", "Standard_Ter_Rocky_Ice" ) );
            EntryIdData.Add( 1301601, new LookupEntryId( "Terrestrials", "Standard_Ter_Rocky" ) );
            EntryIdData.Add( 1300301, new LookupEntryId( "Terrestrials", "Standard_Water_Worlds" ) );
            // EntryIdData.Add(, new LookupEntryId("Terrestrials", "TRF_Ammonia_Worlds" ) );
            EntryIdData.Add( 1300502, new LookupEntryId( "Terrestrials", "TRF_High_Metal_Content_No_Atmos" ) );
            EntryIdData.Add( 1300602, new LookupEntryId( "Terrestrials", "TRF_Rocky_No_Atmos" ) );
            EntryIdData.Add( 1301502, new LookupEntryId( "Terrestrials", "TRF_Ter_High_Metal_Content" ) );
            // EntryIdData.Add(, new LookupEntryId("Terrestrials", "TRF_Ter_Metal_Rich" ) );
            EntryIdData.Add( 1301602, new LookupEntryId( "Terrestrials", "TRF_Ter_Rocky" ) );
            EntryIdData.Add( 1300302, new LookupEntryId( "Terrestrials", "TRF_Water_Worlds" ) );

            // TODO:#2212........[Replaces 'Subcategory' with additional (and expandable) astrological data]
            NameData.Add( "Green_Giant_With_Ammonia_Life", new LookupName( (long?)1200402, "Gas_Giants" ) );
            NameData.Add( "Green_Sudarsky_Class_II", new LookupName( (long?)1200602, "Gas_Giants" ) );
            NameData.Add( "Green_Sudarsky_Class_IV", new LookupName( (long?)1200802, "Gas_Giants" ) );
            NameData.Add( "Green_Sudarsky_Class_V", new LookupName( (long?)1200902, "Gas_Giants" ) );
            NameData.Add( "Green_Water_Giant", new LookupName( (long?)1200102, "Gas_Giants" ) );
            NameData.Add( "Standard_Giant_With_Ammonia_Life", new LookupName( (long?)1200401, "Gas_Giants" ) );
            NameData.Add( "Standard_Giant_With_Water_Life", new LookupName( (long?)1200301, "Gas_Giants" ) );
            NameData.Add( "Standard_Helium", new LookupName( null, "Gas_Giants" ) );
            NameData.Add( "Standard_Helium_Rich", new LookupName( (long?)1201001, "Gas_Giants" ) );
            NameData.Add( "Standard_Sudarsky_Class_I", new LookupName( (long?)1200501, "Gas_Giants" ) );
            NameData.Add( "Standard_Sudarsky_Class_II", new LookupName( (long?)1200601, "Gas_Giants" ) );
            NameData.Add( "Standard_Sudarsky_Class_III", new LookupName( (long?)1200701, "Gas_Giants" ) );
            NameData.Add( "Standard_Sudarsky_Class_IV", new LookupName( (long?)1200801, "Gas_Giants" ) );
            NameData.Add( "Standard_Sudarsky_Class_V", new LookupName( (long?)1200901, "Gas_Giants" ) );
            NameData.Add( "Standard_Water_Giant", new LookupName( (long?)1200101, "Gas_Giants" ) );
            NameData.Add( "Green_Giant_With_Water_Life", new LookupName( (long?)1200302, "Gas_Giants" ) );
            NameData.Add( "Green_Sudarsky_Class_I", new LookupName( (long?)1200502, "Gas_Giants" ) );
            NameData.Add( "Green_Sudarsky_Class_III", new LookupName( (long?)1200702, "Gas_Giants" ) );
            NameData.Add( "A_Type", new LookupName( (long?)1100301, "Stars" ) );
            NameData.Add( "A_TypeGiant", new LookupName( (long?)1100302, "Stars" ) );
            NameData.Add( "A_TypeSuperGiant", new LookupName( (long?)1100303, "Stars" ) );
            NameData.Add( "AEBE_Type", new LookupName( (long?)1101101, "Stars" ) );
            NameData.Add( "B_Type", new LookupName( (long?)1100201, "Stars" ) );
            NameData.Add( "B_TypeGiant", new LookupName( (long?)1100202, "Stars" ) );
            NameData.Add( "B_TypeSuperGiant", new LookupName( (long?)1100203, "Stars" ) );
            NameData.Add( "Black_Holes", new LookupName( (long?)1102400, "Stars" ) );
            NameData.Add( "C_Type", new LookupName( (long?)1101401, "Stars" ) );
            NameData.Add( "C_TypeGiant", new LookupName( (long?)1101402, "Stars" ) );
            NameData.Add( "C_TypeHyperGiant", new LookupName( (long?)1101404, "Stars" ) );
            NameData.Add( "C_TypeSuperGiant", new LookupName( (long?)1101403, "Stars" ) );
            NameData.Add( "CJ_Type", new LookupName( null, "Stars" ) );
            NameData.Add( "CN_Type", new LookupName( null, "Stars" ) );
            NameData.Add( "D_Type", new LookupName( (long?)1102201, "Stars" ) );
            NameData.Add( "DA_Type", new LookupName( (long?)1102202, "Stars" ) );
            NameData.Add( "DAB_Type", new LookupName( (long?)1102203, "Stars" ) );
            NameData.Add( "DAV_Type", new LookupName( (long?)1102205, "Stars" ) );
            NameData.Add( "DAZ_Type", new LookupName( (long?)1102206, "Stars" ) );
            NameData.Add( "DB_Type", new LookupName( (long?)1102207, "Stars" ) );
            NameData.Add( "DBV_Type", new LookupName( (long?)1102208, "Stars" ) );
            NameData.Add( "DBZ_Type", new LookupName( null, "Stars" ) );
            NameData.Add( "DC_Type", new LookupName( (long?)1102213, "Stars" ) );
            NameData.Add( "DCV_Type", new LookupName( null, "Stars" ) );
            NameData.Add( "DQ_Type", new LookupName( (long?)1102212, "Stars" ) );
            NameData.Add( "F_Type", new LookupName( (long?)1100401, "Stars" ) );
            NameData.Add( "F_TypeGiant", new LookupName( (long?)1100402, "Stars" ) );
            NameData.Add( "F_TypeSuperGiant", new LookupName( (long?)1100403, "Stars" ) );
            NameData.Add( "G_Type", new LookupName( (long?)1100501, "Stars" ) );
            NameData.Add( "G_TypeGiant", new LookupName( (long?)1100502, "Stars" ) );
            NameData.Add( "G_TypeSuperGiant", new LookupName( (long?)1100503, "Stars" ) );
            NameData.Add( "K_Type", new LookupName( (long?)1100601, "Stars" ) );
            NameData.Add( "K_TypeGiant", new LookupName( (long?)1100602, "Stars" ) );
            NameData.Add( "K_TypeSuperGiant", new LookupName( (long?)1100603, "Stars" ) );
            NameData.Add( "L_Type", new LookupName( (long?)1100801, "Stars" ) );
            NameData.Add( "M_Type", new LookupName( (long?)1100701, "Stars" ) );
            NameData.Add( "M_TypeGiant", new LookupName( (long?)1100702, "Stars" ) );
            NameData.Add( "M_TypeSuperGiant", new LookupName( (long?)1100703, "Stars" ) );
            NameData.Add( "MS_Type", new LookupName( null, "Stars" ) );
            NameData.Add( "Neutron_Stars", new LookupName( (long?)1102300, "Stars" ) );
            NameData.Add( "O_Type", new LookupName( (long?)1100101, "Stars" ) );
            NameData.Add( "O_TypeGiant", new LookupName( (long?)1100102, "Stars" ) );
            NameData.Add( "O_TypeSuperGiant", new LookupName( (long?)1100103, "Stars" ) );
            NameData.Add( "S_Type", new LookupName( (long?)1102001, "Stars" ) );
            NameData.Add( "S_TypeGiant", new LookupName( (long?)1102002, "Stars" ) );
            NameData.Add( "SupermassiveBlack_Holes", new LookupName( (long?)1102500, "Stars" ) );
            NameData.Add( "T_Type", new LookupName( (long?)1100901, "Stars" ) );
            NameData.Add( "TTS_Type", new LookupName( (long?)1101001, "Stars" ) );
            NameData.Add( "W_Type", new LookupName( (long?)1102101, "Stars" ) );
            NameData.Add( "WC_Type", new LookupName( (long?)1102102, "Stars" ) );
            NameData.Add( "WN_Type", new LookupName( (long?)1102103, "Stars" ) );
            NameData.Add( "WNC_Type", new LookupName( (long?)1102104, "Stars" ) );
            NameData.Add( "WO_Type", new LookupName( (long?)1102105, "Stars" ) );
            NameData.Add( "Y_Type", new LookupName( (long?)1101201, "Stars" ) );
            NameData.Add( "Earth_Likes", new LookupName( (long?)1300100, "Terrestrials" ) );
            NameData.Add( "Standard_Ammonia_Worlds", new LookupName( (long?)1300202, "Terrestrials" ) );
            NameData.Add( "Standard_High_Metal_Content_No_Atmos", new LookupName( (long?)1300501, "Terrestrials" ) );
            NameData.Add( "Standard_Ice_No_Atmos", new LookupName( (long?)1300801, "Terrestrials" ) );
            NameData.Add( "Standard_Metal_Rich_No_Atmos", new LookupName( (long?)1300401, "Terrestrials" ) );
            NameData.Add( "Standard_Rocky_Ice_No_Atmos", new LookupName( (long?)1300701, "Terrestrials" ) );
            NameData.Add( "Standard_Rocky_No_Atmos", new LookupName( (long?)1300601, "Terrestrials" ) );
            NameData.Add( "Standard_Ter_High_Metal_Content", new LookupName( (long?)1301501, "Terrestrials" ) );
            NameData.Add( "Standard_Ter_Ice", new LookupName( (long?)1301801, "Terrestrials" ) );
            NameData.Add( "Standard_Ter_Metal_Rich", new LookupName( (long?)1301401, "Terrestrials" ) );
            NameData.Add( "Standard_Ter_Rocky_Ice", new LookupName( (long?)1301701, "Terrestrials" ) );
            NameData.Add( "Standard_Ter_Rocky", new LookupName( (long?)1301601, "Terrestrials" ) );
            NameData.Add( "Standard_Water_Worlds", new LookupName( (long?)1300301, "Terrestrials" ) );
            NameData.Add( "TRF_Ammonia_Worlds", new LookupName( null, "Terrestrials" ) );
            NameData.Add( "TRF_High_Metal_Content_No_Atmos", new LookupName( (long?)1300502, "Terrestrials" ) );
            NameData.Add( "TRF_Rocky_No_Atmos", new LookupName( (long?)1300602, "Terrestrials" ) );
            NameData.Add( "TRF_Ter_High_Metal_Content", new LookupName( (long?)1301502, "Terrestrials" ) );
            NameData.Add( "TRF_Ter_Metal_Rich", new LookupName( null, "Terrestrials" ) );
            NameData.Add( "TRF_Ter_Rocky", new LookupName( (long?)1301602, "Terrestrials" ) );
            NameData.Add( "TRF_Water_Worlds", new LookupName( (long?)1300302, "Terrestrials" ) );


            // Fallback, entryid is not known
            SubCategory.Add( "Green_Giant_With_Ammonia_Life", "Gas_Giants" );
            SubCategory.Add( "Green_Sudarsky_Class_II", "Gas_Giants" );
            SubCategory.Add( "Green_Sudarsky_Class_IV", "Gas_Giants" );
            SubCategory.Add( "Green_Sudarsky_Class_V", "Gas_Giants" );
            SubCategory.Add( "Green_Water_Giant", "Gas_Giants" );
            SubCategory.Add( "Standard_Giant_With_Ammonia_Life", "Gas_Giants" );
            SubCategory.Add( "Standard_Giant_With_Water_Life", "Gas_Giants" );
            SubCategory.Add( "Standard_Helium", "Gas_Giants" );
            SubCategory.Add( "Standard_Helium_Rich", "Gas_Giants" );
            SubCategory.Add( "Standard_Sudarsky_Class_I", "Gas_Giants" );
            SubCategory.Add( "Standard_Sudarsky_Class_II", "Gas_Giants" );
            SubCategory.Add( "Standard_Sudarsky_Class_III", "Gas_Giants" );
            SubCategory.Add( "Standard_Sudarsky_Class_IV", "Gas_Giants" );
            SubCategory.Add( "Standard_Sudarsky_Class_V", "Gas_Giants" );
            SubCategory.Add( "Standard_Water_Giant", "Gas_Giants" );
            SubCategory.Add( "Green_Giant_With_Water_Life", "Gas_Giants" );
            SubCategory.Add( "Green_Sudarsky_Class_I", "Gas_Giants" );
            SubCategory.Add( "Green_Sudarsky_Class_III", "Gas_Giants" );
            SubCategory.Add( "A_Type", "Stars" );
            SubCategory.Add( "A_TypeGiant", "Stars" );
            SubCategory.Add( "A_TypeSuperGiant", "Stars" );
            SubCategory.Add( "AEBE_Type", "Stars" );
            SubCategory.Add( "B_Type", "Stars" );
            SubCategory.Add( "B_TypeGiant", "Stars" );
            SubCategory.Add( "B_TypeSuperGiant", "Stars" );
            SubCategory.Add( "Black_Holes", "Stars" );
            SubCategory.Add( "C_Type", "Stars" );
            SubCategory.Add( "C_TypeGiant", "Stars" );
            SubCategory.Add( "C_TypeHyperGiant", "Stars" );
            SubCategory.Add( "C_TypeSuperGiant", "Stars" );
            SubCategory.Add( "CJ_Type", "Stars" );
            SubCategory.Add( "CN_Type", "Stars" );
            SubCategory.Add( "D_Type", "Stars" );
            SubCategory.Add( "DA_Type", "Stars" );
            SubCategory.Add( "DAB_Type", "Stars" );
            SubCategory.Add( "DAV_Type", "Stars" );
            SubCategory.Add( "DAZ_Type", "Stars" );
            SubCategory.Add( "DB_Type", "Stars" );
            SubCategory.Add( "DBV_Type", "Stars" );
            SubCategory.Add( "DBZ_Type", "Stars" );
            SubCategory.Add( "DC_Type", "Stars" );
            SubCategory.Add( "DCV_Type", "Stars" );
            SubCategory.Add( "DQ_Type", "Stars" );
            SubCategory.Add( "F_Type", "Stars" );
            SubCategory.Add( "F_TypeGiant", "Stars" );
            SubCategory.Add( "F_TypeSuperGiant", "Stars" );
            SubCategory.Add( "G_Type", "Stars" );
            SubCategory.Add( "G_TypeGiant", "Stars" );
            SubCategory.Add( "G_TypeSuperGiant", "Stars" );
            SubCategory.Add( "K_Type", "Stars" );
            SubCategory.Add( "K_TypeGiant", "Stars" );
            SubCategory.Add( "K_TypeSuperGiant", "Stars" );
            SubCategory.Add( "L_Type", "Stars" );
            SubCategory.Add( "M_Type", "Stars" );
            SubCategory.Add( "M_TypeGiant", "Stars" );
            SubCategory.Add( "M_TypeSuperGiant", "Stars" );
            SubCategory.Add( "MS_Type", "Stars" );
            SubCategory.Add( "Neutron_Stars", "Stars" );
            SubCategory.Add( "O_Type", "Stars" );
            SubCategory.Add( "O_TypeGiant", "Stars" );
            SubCategory.Add( "O_TypeSuperGiant", "Stars" );
            SubCategory.Add( "S_Type", "Stars" );
            SubCategory.Add( "S_TypeGiant", "Stars" );
            SubCategory.Add( "SupermassiveBlack_Holes", "Stars" );
            SubCategory.Add( "T_Type", "Stars" );
            SubCategory.Add( "TTS_Type", "Stars" );
            SubCategory.Add( "W_Type", "Stars" );
            SubCategory.Add( "WC_Type", "Stars" );
            SubCategory.Add( "WN_Type", "Stars" );
            SubCategory.Add( "WNC_Type", "Stars" );
            SubCategory.Add( "WO_Type", "Stars" );
            SubCategory.Add( "Y_Type", "Stars" );
            SubCategory.Add( "Earth_Likes", "Terrestrials" );
            SubCategory.Add( "Standard_Ammonia_Worlds", "Terrestrials" );
            SubCategory.Add( "Standard_High_Metal_Content_No_Atmos", "Terrestrials" );
            SubCategory.Add( "Standard_Ice_No_Atmos", "Terrestrials" );
            SubCategory.Add( "Standard_Metal_Rich_No_Atmos", "Terrestrials" );
            SubCategory.Add( "Standard_Rocky_Ice_No_Atmos", "Terrestrials" );
            SubCategory.Add( "Standard_Rocky_No_Atmos", "Terrestrials" );
            SubCategory.Add( "Standard_Ter_High_Metal_Content", "Terrestrials" );
            SubCategory.Add( "Standard_Ter_Ice", "Terrestrials" );
            SubCategory.Add( "Standard_Ter_Metal_Rich", "Terrestrials" );
            SubCategory.Add( "Standard_Ter_Rocky_Ice", "Terrestrials" );
            SubCategory.Add( "Standard_Ter_Rocky", "Terrestrials" );
            SubCategory.Add( "Standard_Water_Worlds", "Terrestrials" );
            SubCategory.Add( "TRF_Ammonia_Worlds", "Terrestrials" );
            SubCategory.Add( "TRF_High_Metal_Content_No_Atmos", "Terrestrials" );
            SubCategory.Add( "TRF_Rocky_No_Atmos", "Terrestrials" );
            SubCategory.Add( "TRF_Ter_High_Metal_Content", "Terrestrials" );
            SubCategory.Add( "TRF_Ter_Metal_Rich", "Terrestrials" );
            SubCategory.Add( "TRF_Ter_Rocky", "Terrestrials" );
            SubCategory.Add( "TRF_Water_Worlds", "Terrestrials" );
        }

        public static AstrometricItem Lookup ( long? entryId, string edname )
        {
            AstrometricItem item = new AstrometricItem();
            item = LookupByEntryId( entryId );

            if ( !item.exists )
            {
                item = LookupByName( edname );
            }

            return item;
        }

        public static AstrometricItem LookupByEntryId ( long? entryId )
        {
            AstrometricItem item = new AstrometricItem();

            // TODO:#2212........[Finish writing LookupByEntryId logic]
            if ( entryId != null )
            {
                if ( EntryIdData.ContainsKey( (long)entryId ) )
                {
                    LookupEntryId data = EntryIdData[ (long)entryId ];

                    item.name = rmAstroName.GetString( data.edname );
                    item.subCategory = rmAstroSubCategory.GetString( SubCategory[ data.edname ] );
                    item.description = rmAstroDesc.GetString( data.edname );

                    item.SetExists( true );
                }
            }

            return item;
        }

        public static AstrometricItem LookupByName ( string edname )
        {
            AstrometricItem item = new AstrometricItem();

            if ( edname != "" )
            {
                item.name = rmAstroName.GetString( edname );
                item.subCategory = rmAstroSubCategory.GetString( SubCategory[ edname ] );
                item.description = rmAstroDesc.GetString( edname );

                item.SetExists( true );
            }

            // If the above fails to find an entry then we return the empty item
            // We could modify the item to say that we could not find an entry as well
            return item;
        }
    }
}

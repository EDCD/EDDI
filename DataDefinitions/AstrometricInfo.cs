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

        public static Dictionary<long, LookupEntryId> entryIdData = new Dictionary<long, LookupEntryId>();
        public static Dictionary<string, LookupName> nameData = new Dictionary<string, LookupName>();

        static AstrometricInfo ()
        {
            // Items with missing entryid's are commented out
            entryIdData.Add( 1200402, new LookupEntryId( "Gas_Giants", "Green_Giant_With_Ammonia_Life" ) );
            entryIdData.Add( 1200602, new LookupEntryId( "Gas_Giants", "Green_Sudarsky_Class_II" ) );
            entryIdData.Add( 1200802, new LookupEntryId( "Gas_Giants", "Green_Sudarsky_Class_IV" ) );
            entryIdData.Add( 1200902, new LookupEntryId( "Gas_Giants", "Green_Sudarsky_Class_V" ) );
            entryIdData.Add( 1200102, new LookupEntryId( "Gas_Giants", "Green_Water_Giant" ) );
            entryIdData.Add( 1200401, new LookupEntryId( "Gas_Giants", "Standard_Giant_With_Ammonia_Life" ) );
            entryIdData.Add( 1200301, new LookupEntryId( "Gas_Giants", "Standard_Giant_With_Water_Life" ) );
            // entryIdData.Add(, new LookupEntryId("Gas_Giants", "Standard_Helium" ) );
            entryIdData.Add( 1201001, new LookupEntryId( "Gas_Giants", "Standard_Helium_Rich" ) );
            entryIdData.Add( 1200501, new LookupEntryId( "Gas_Giants", "Standard_Sudarsky_Class_I" ) );
            entryIdData.Add( 1200601, new LookupEntryId( "Gas_Giants", "Standard_Sudarsky_Class_II" ) );
            entryIdData.Add( 1200701, new LookupEntryId( "Gas_Giants", "Standard_Sudarsky_Class_III" ) );
            entryIdData.Add( 1200801, new LookupEntryId( "Gas_Giants", "Standard_Sudarsky_Class_IV" ) );
            entryIdData.Add( 1200901, new LookupEntryId( "Gas_Giants", "Standard_Sudarsky_Class_V" ) );
            entryIdData.Add( 1200101, new LookupEntryId( "Gas_Giants", "Standard_Water_Giant" ) );
            entryIdData.Add( 1200302, new LookupEntryId( "Gas_Giants", "Green_Giant_With_Water_Life" ) );
            entryIdData.Add( 1200502, new LookupEntryId( "Gas_Giants", "Green_Sudarsky_Class_I" ) );
            entryIdData.Add( 1200702, new LookupEntryId( "Gas_Giants", "Green_Sudarsky_Class_III" ) );
            entryIdData.Add( 1100301, new LookupEntryId( "Stars", "A_Type" ) );
            entryIdData.Add( 1100302, new LookupEntryId( "Stars", "A_TypeGiant" ) );
            entryIdData.Add( 1100303, new LookupEntryId( "Stars", "A_TypeSuperGiant" ) );
            entryIdData.Add( 1101101, new LookupEntryId( "Stars", "AEBE_Type" ) );
            entryIdData.Add( 1100201, new LookupEntryId( "Stars", "B_Type" ) );
            entryIdData.Add( 1100202, new LookupEntryId( "Stars", "B_TypeGiant" ) );
            entryIdData.Add( 1100203, new LookupEntryId( "Stars", "B_TypeSuperGiant" ) );
            entryIdData.Add( 1102400, new LookupEntryId( "Stars", "Black_Holes" ) );
            entryIdData.Add( 1101401, new LookupEntryId( "Stars", "C_Type" ) );
            entryIdData.Add( 1101402, new LookupEntryId( "Stars", "C_TypeGiant" ) );
            entryIdData.Add( 1101404, new LookupEntryId( "Stars", "C_TypeHyperGiant" ) );
            entryIdData.Add( 1101403, new LookupEntryId( "Stars", "C_TypeSuperGiant" ) );
            // entryIdData.Add(, new LookupEntryId("Stars", "CJ_Type" ) );
            // entryIdData.Add(, new LookupEntryId("Stars", "CN_Type" ) );
            entryIdData.Add( 1102201, new LookupEntryId( "Stars", "D_Type" ) );
            entryIdData.Add( 1102202, new LookupEntryId( "Stars", "DA_Type" ) );
            entryIdData.Add( 1102203, new LookupEntryId( "Stars", "DAB_Type" ) );
            entryIdData.Add( 1102205, new LookupEntryId( "Stars", "DAV_Type" ) );
            entryIdData.Add( 1102206, new LookupEntryId( "Stars", "DAZ_Type" ) );
            entryIdData.Add( 1102207, new LookupEntryId( "Stars", "DB_Type" ) );
            entryIdData.Add( 1102208, new LookupEntryId( "Stars", "DBV_Type" ) );
            // entryIdData.Add(, new LookupEntryId("Stars", "DBZ_Type" ) );
            entryIdData.Add( 1102213, new LookupEntryId( "Stars", "DC_Type" ) );
            // entryIdData.Add(, new LookupEntryId("Stars", "DCV_Type" ) );
            entryIdData.Add( 1102212, new LookupEntryId( "Stars", "DQ_Type" ) );
            entryIdData.Add( 1100401, new LookupEntryId( "Stars", "F_Type" ) );
            entryIdData.Add( 1100402, new LookupEntryId( "Stars", "F_TypeGiant" ) );
            entryIdData.Add( 1100403, new LookupEntryId( "Stars", "F_TypeSuperGiant" ) );
            entryIdData.Add( 1100501, new LookupEntryId( "Stars", "G_Type" ) );
            entryIdData.Add( 1100502, new LookupEntryId( "Stars", "G_TypeGiant" ) );
            entryIdData.Add( 1100503, new LookupEntryId( "Stars", "G_TypeSuperGiant" ) );
            entryIdData.Add( 1100601, new LookupEntryId( "Stars", "K_Type" ) );
            entryIdData.Add( 1100602, new LookupEntryId( "Stars", "K_TypeGiant" ) );
            entryIdData.Add( 1100603, new LookupEntryId( "Stars", "K_TypeSuperGiant" ) );
            entryIdData.Add( 1100801, new LookupEntryId( "Stars", "L_Type" ) );
            entryIdData.Add( 1100701, new LookupEntryId( "Stars", "M_Type" ) );
            entryIdData.Add( 1100702, new LookupEntryId( "Stars", "M_TypeGiant" ) );
            entryIdData.Add( 1100703, new LookupEntryId( "Stars", "M_TypeSuperGiant" ) );
            // entryIdData.Add(, new LookupEntryId("Stars", "MS_Type" ) );
            entryIdData.Add( 1102300, new LookupEntryId( "Stars", "Neutron_Stars" ) );
            entryIdData.Add( 1100101, new LookupEntryId( "Stars", "O_Type" ) );
            entryIdData.Add( 1100102, new LookupEntryId( "Stars", "O_TypeGiant" ) );
            entryIdData.Add( 1100103, new LookupEntryId( "Stars", "O_TypeSuperGiant" ) );
            entryIdData.Add( 1102001, new LookupEntryId( "Stars", "S_Type" ) );
            entryIdData.Add( 1102002, new LookupEntryId( "Stars", "S_TypeGiant" ) );
            entryIdData.Add( 1102500, new LookupEntryId( "Stars", "SupermassiveBlack_Holes" ) );
            entryIdData.Add( 1100901, new LookupEntryId( "Stars", "T_Type" ) );
            entryIdData.Add( 1101001, new LookupEntryId( "Stars", "TTS_Type" ) );
            entryIdData.Add( 1102101, new LookupEntryId( "Stars", "W_Type" ) );
            entryIdData.Add( 1102102, new LookupEntryId( "Stars", "WC_Type" ) );
            entryIdData.Add( 1102103, new LookupEntryId( "Stars", "WN_Type" ) );
            entryIdData.Add( 1102104, new LookupEntryId( "Stars", "WNC_Type" ) );
            entryIdData.Add( 1102105, new LookupEntryId( "Stars", "WO_Type" ) );
            entryIdData.Add( 1101201, new LookupEntryId( "Stars", "Y_Type" ) );
            entryIdData.Add( 1300100, new LookupEntryId( "Terrestrials", "Earth_Likes" ) );
            entryIdData.Add( 1300202, new LookupEntryId( "Terrestrials", "Standard_Ammonia_Worlds" ) );
            entryIdData.Add( 1300501, new LookupEntryId( "Terrestrials", "Standard_High_Metal_Content_No_Atmos" ) );
            entryIdData.Add( 1300801, new LookupEntryId( "Terrestrials", "Standard_Ice_No_Atmos" ) );
            entryIdData.Add( 1300401, new LookupEntryId( "Terrestrials", "Standard_Metal_Rich_No_Atmos" ) );
            entryIdData.Add( 1300701, new LookupEntryId( "Terrestrials", "Standard_Rocky_Ice_No_Atmos" ) );
            entryIdData.Add( 1300601, new LookupEntryId( "Terrestrials", "Standard_Rocky_No_Atmos" ) );
            entryIdData.Add( 1301501, new LookupEntryId( "Terrestrials", "Standard_Ter_High_Metal_Content" ) );
            entryIdData.Add( 1301801, new LookupEntryId( "Terrestrials", "Standard_Ter_Ice" ) );
            entryIdData.Add( 1301401, new LookupEntryId( "Terrestrials", "Standard_Ter_Metal_Rich" ) );
            entryIdData.Add( 1301701, new LookupEntryId( "Terrestrials", "Standard_Ter_Rocky_Ice" ) );
            entryIdData.Add( 1301601, new LookupEntryId( "Terrestrials", "Standard_Ter_Rocky" ) );
            entryIdData.Add( 1300301, new LookupEntryId( "Terrestrials", "Standard_Water_Worlds" ) );
            // entryIdData.Add(, new LookupEntryId("Terrestrials", "TRF_Ammonia_Worlds" ) );
            entryIdData.Add( 1300502, new LookupEntryId( "Terrestrials", "TRF_High_Metal_Content_No_Atmos" ) );
            entryIdData.Add( 1300602, new LookupEntryId( "Terrestrials", "TRF_Rocky_No_Atmos" ) );
            entryIdData.Add( 1301502, new LookupEntryId( "Terrestrials", "TRF_Ter_High_Metal_Content" ) );
            // entryIdData.Add(, new LookupEntryId("Terrestrials", "TRF_Ter_Metal_Rich" ) );
            entryIdData.Add( 1301602, new LookupEntryId( "Terrestrials", "TRF_Ter_Rocky" ) );
            entryIdData.Add( 1300302, new LookupEntryId( "Terrestrials", "TRF_Water_Worlds" ) );

            // Fallback for getting data by Name
            nameData.Add( "Green_Giant_With_Ammonia_Life", new LookupName( (long?)1200402, "Gas_Giants" ) );
            nameData.Add( "Green_Sudarsky_Class_II", new LookupName( (long?)1200602, "Gas_Giants" ) );
            nameData.Add( "Green_Sudarsky_Class_IV", new LookupName( (long?)1200802, "Gas_Giants" ) );
            nameData.Add( "Green_Sudarsky_Class_V", new LookupName( (long?)1200902, "Gas_Giants" ) );
            nameData.Add( "Green_Water_Giant", new LookupName( (long?)1200102, "Gas_Giants" ) );
            nameData.Add( "Standard_Giant_With_Ammonia_Life", new LookupName( (long?)1200401, "Gas_Giants" ) );
            nameData.Add( "Standard_Giant_With_Water_Life", new LookupName( (long?)1200301, "Gas_Giants" ) );
            nameData.Add( "Standard_Helium", new LookupName( null, "Gas_Giants" ) );
            nameData.Add( "Standard_Helium_Rich", new LookupName( (long?)1201001, "Gas_Giants" ) );
            nameData.Add( "Standard_Sudarsky_Class_I", new LookupName( (long?)1200501, "Gas_Giants" ) );
            nameData.Add( "Standard_Sudarsky_Class_II", new LookupName( (long?)1200601, "Gas_Giants" ) );
            nameData.Add( "Standard_Sudarsky_Class_III", new LookupName( (long?)1200701, "Gas_Giants" ) );
            nameData.Add( "Standard_Sudarsky_Class_IV", new LookupName( (long?)1200801, "Gas_Giants" ) );
            nameData.Add( "Standard_Sudarsky_Class_V", new LookupName( (long?)1200901, "Gas_Giants" ) );
            nameData.Add( "Standard_Water_Giant", new LookupName( (long?)1200101, "Gas_Giants" ) );
            nameData.Add( "Green_Giant_With_Water_Life", new LookupName( (long?)1200302, "Gas_Giants" ) );
            nameData.Add( "Green_Sudarsky_Class_I", new LookupName( (long?)1200502, "Gas_Giants" ) );
            nameData.Add( "Green_Sudarsky_Class_III", new LookupName( (long?)1200702, "Gas_Giants" ) );
            nameData.Add( "A_Type", new LookupName( (long?)1100301, "Stars" ) );
            nameData.Add( "A_TypeGiant", new LookupName( (long?)1100302, "Stars" ) );
            nameData.Add( "A_TypeSuperGiant", new LookupName( (long?)1100303, "Stars" ) );
            nameData.Add( "AEBE_Type", new LookupName( (long?)1101101, "Stars" ) );
            nameData.Add( "B_Type", new LookupName( (long?)1100201, "Stars" ) );
            nameData.Add( "B_TypeGiant", new LookupName( (long?)1100202, "Stars" ) );
            nameData.Add( "B_TypeSuperGiant", new LookupName( (long?)1100203, "Stars" ) );
            nameData.Add( "Black_Holes", new LookupName( (long?)1102400, "Stars" ) );
            nameData.Add( "C_Type", new LookupName( (long?)1101401, "Stars" ) );
            nameData.Add( "C_TypeGiant", new LookupName( (long?)1101402, "Stars" ) );
            nameData.Add( "C_TypeHyperGiant", new LookupName( (long?)1101404, "Stars" ) );
            nameData.Add( "C_TypeSuperGiant", new LookupName( (long?)1101403, "Stars" ) );
            nameData.Add( "CJ_Type", new LookupName( null, "Stars" ) );
            nameData.Add( "CN_Type", new LookupName( null, "Stars" ) );
            nameData.Add( "D_Type", new LookupName( (long?)1102201, "Stars" ) );
            nameData.Add( "DA_Type", new LookupName( (long?)1102202, "Stars" ) );
            nameData.Add( "DAB_Type", new LookupName( (long?)1102203, "Stars" ) );
            nameData.Add( "DAV_Type", new LookupName( (long?)1102205, "Stars" ) );
            nameData.Add( "DAZ_Type", new LookupName( (long?)1102206, "Stars" ) );
            nameData.Add( "DB_Type", new LookupName( (long?)1102207, "Stars" ) );
            nameData.Add( "DBV_Type", new LookupName( (long?)1102208, "Stars" ) );
            nameData.Add( "DBZ_Type", new LookupName( null, "Stars" ) );
            nameData.Add( "DC_Type", new LookupName( (long?)1102213, "Stars" ) );
            nameData.Add( "DCV_Type", new LookupName( null, "Stars" ) );
            nameData.Add( "DQ_Type", new LookupName( (long?)1102212, "Stars" ) );
            nameData.Add( "F_Type", new LookupName( (long?)1100401, "Stars" ) );
            nameData.Add( "F_TypeGiant", new LookupName( (long?)1100402, "Stars" ) );
            nameData.Add( "F_TypeSuperGiant", new LookupName( (long?)1100403, "Stars" ) );
            nameData.Add( "G_Type", new LookupName( (long?)1100501, "Stars" ) );
            nameData.Add( "G_TypeGiant", new LookupName( (long?)1100502, "Stars" ) );
            nameData.Add( "G_TypeSuperGiant", new LookupName( (long?)1100503, "Stars" ) );
            nameData.Add( "K_Type", new LookupName( (long?)1100601, "Stars" ) );
            nameData.Add( "K_TypeGiant", new LookupName( (long?)1100602, "Stars" ) );
            nameData.Add( "K_TypeSuperGiant", new LookupName( (long?)1100603, "Stars" ) );
            nameData.Add( "L_Type", new LookupName( (long?)1100801, "Stars" ) );
            nameData.Add( "M_Type", new LookupName( (long?)1100701, "Stars" ) );
            nameData.Add( "M_TypeGiant", new LookupName( (long?)1100702, "Stars" ) );
            nameData.Add( "M_TypeSuperGiant", new LookupName( (long?)1100703, "Stars" ) );
            nameData.Add( "MS_Type", new LookupName( null, "Stars" ) );
            nameData.Add( "Neutron_Stars", new LookupName( (long?)1102300, "Stars" ) );
            nameData.Add( "O_Type", new LookupName( (long?)1100101, "Stars" ) );
            nameData.Add( "O_TypeGiant", new LookupName( (long?)1100102, "Stars" ) );
            nameData.Add( "O_TypeSuperGiant", new LookupName( (long?)1100103, "Stars" ) );
            nameData.Add( "S_Type", new LookupName( (long?)1102001, "Stars" ) );
            nameData.Add( "S_TypeGiant", new LookupName( (long?)1102002, "Stars" ) );
            nameData.Add( "SupermassiveBlack_Holes", new LookupName( (long?)1102500, "Stars" ) );
            nameData.Add( "T_Type", new LookupName( (long?)1100901, "Stars" ) );
            nameData.Add( "TTS_Type", new LookupName( (long?)1101001, "Stars" ) );
            nameData.Add( "W_Type", new LookupName( (long?)1102101, "Stars" ) );
            nameData.Add( "WC_Type", new LookupName( (long?)1102102, "Stars" ) );
            nameData.Add( "WN_Type", new LookupName( (long?)1102103, "Stars" ) );
            nameData.Add( "WNC_Type", new LookupName( (long?)1102104, "Stars" ) );
            nameData.Add( "WO_Type", new LookupName( (long?)1102105, "Stars" ) );
            nameData.Add( "Y_Type", new LookupName( (long?)1101201, "Stars" ) );
            nameData.Add( "Earth_Likes", new LookupName( (long?)1300100, "Terrestrials" ) );
            nameData.Add( "Standard_Ammonia_Worlds", new LookupName( (long?)1300202, "Terrestrials" ) );
            nameData.Add( "Standard_High_Metal_Content_No_Atmos", new LookupName( (long?)1300501, "Terrestrials" ) );
            nameData.Add( "Standard_Ice_No_Atmos", new LookupName( (long?)1300801, "Terrestrials" ) );
            nameData.Add( "Standard_Metal_Rich_No_Atmos", new LookupName( (long?)1300401, "Terrestrials" ) );
            nameData.Add( "Standard_Rocky_Ice_No_Atmos", new LookupName( (long?)1300701, "Terrestrials" ) );
            nameData.Add( "Standard_Rocky_No_Atmos", new LookupName( (long?)1300601, "Terrestrials" ) );
            nameData.Add( "Standard_Ter_High_Metal_Content", new LookupName( (long?)1301501, "Terrestrials" ) );
            nameData.Add( "Standard_Ter_Ice", new LookupName( (long?)1301801, "Terrestrials" ) );
            nameData.Add( "Standard_Ter_Metal_Rich", new LookupName( (long?)1301401, "Terrestrials" ) );
            nameData.Add( "Standard_Ter_Rocky_Ice", new LookupName( (long?)1301701, "Terrestrials" ) );
            nameData.Add( "Standard_Ter_Rocky", new LookupName( (long?)1301601, "Terrestrials" ) );
            nameData.Add( "Standard_Water_Worlds", new LookupName( (long?)1300301, "Terrestrials" ) );
            nameData.Add( "TRF_Ammonia_Worlds", new LookupName( null, "Terrestrials" ) );
            nameData.Add( "TRF_High_Metal_Content_No_Atmos", new LookupName( (long?)1300502, "Terrestrials" ) );
            nameData.Add( "TRF_Rocky_No_Atmos", new LookupName( (long?)1300602, "Terrestrials" ) );
            nameData.Add( "TRF_Ter_High_Metal_Content", new LookupName( (long?)1301502, "Terrestrials" ) );
            nameData.Add( "TRF_Ter_Metal_Rich", new LookupName( null, "Terrestrials" ) );
            nameData.Add( "TRF_Ter_Rocky", new LookupName( (long?)1301602, "Terrestrials" ) );
            nameData.Add( "TRF_Water_Worlds", new LookupName( (long?)1300302, "Terrestrials" ) );
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

            if ( entryId != null )
            {
                if ( entryIdData.ContainsKey( (long)entryId ) )
                {
                    LookupEntryId data = entryIdData[ (long)entryId ];

                    item.name = rmAstroName.GetString( data.edname );
                    item.subCategory = rmAstroSubCategory.GetString( data.className );
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
                item.subCategory = rmAstroSubCategory.GetString( nameData[ edname ].className );
                item.description = rmAstroDesc.GetString( edname );

                item.SetExists( true );
            }

            // If the above fails to find an entry then we return the empty item
            // We could modify the item to say that we could not find an entry as well
            return item;
        }
    }
}

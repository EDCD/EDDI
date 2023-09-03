using System.Collections.Generic;
using System.Reflection;
using System.Resources;

namespace EddiDataDefinitions
{
    public class Astronomical : ResourceBasedLocalizedEDName<Astronomical>
    {
        public static ResourceManager rmAstronomicalDesc = new ResourceManager("EddiDataDefinitions.Properties.AstronomicalDesc", Assembly.GetExecutingAssembly());

        public static readonly IDictionary<string, long?> ASTRONOMICALS = new Dictionary<string, long?>();
        public static readonly IDictionary<long, Astronomical> ENTRYIDS = new Dictionary<long, Astronomical>();

        // There are some missing EntryIds, so this contains those items if the standard lookups fail
        public static readonly IDictionary<string, Astronomical> MISSINGIDS = new Dictionary<string, Astronomical>();

        public bool exists;                 // This item exists and has been populated with information
        public AstronomicalType type;
        //public long value;
        public string description;

        static Astronomical ()
        {
            resourceManager = Properties.Astronomical.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new Astronomical( edname );

            ENTRYIDS.Add( (long)1200402, new Astronomical( "Gas_Giants", "Green_Giant_With_Ammonia_Life" ) );
            ENTRYIDS.Add( (long)1200602, new Astronomical( "Gas_Giants", "Green_Sudarsky_Class_II" ) );
            ENTRYIDS.Add( (long)1200802, new Astronomical( "Gas_Giants", "Green_Sudarsky_Class_IV" ) );
            ENTRYIDS.Add( (long)1200902, new Astronomical( "Gas_Giants", "Green_Sudarsky_Class_V" ) );
            ENTRYIDS.Add( (long)1200102, new Astronomical( "Gas_Giants", "Green_Water_Giant" ) );
            ENTRYIDS.Add( (long)1200401, new Astronomical( "Gas_Giants", "Standard_Giant_With_Ammonia_Life" ) );
            ENTRYIDS.Add( (long)1200301, new Astronomical( "Gas_Giants", "Standard_Giant_With_Water_Life" ) );
            MISSINGIDS.Add( "Standard_Helium", new Astronomical( "Gas_Giants", "Standard_Helium" ) );
            ENTRYIDS.Add( (long)1201001, new Astronomical( "Gas_Giants", "Standard_Helium_Rich" ) );
            ENTRYIDS.Add( (long)1200501, new Astronomical( "Gas_Giants", "Standard_Sudarsky_Class_I" ) );
            ENTRYIDS.Add( (long)1200601, new Astronomical( "Gas_Giants", "Standard_Sudarsky_Class_II" ) );
            ENTRYIDS.Add( (long)1200701, new Astronomical( "Gas_Giants", "Standard_Sudarsky_Class_III" ) );
            ENTRYIDS.Add( (long)1200801, new Astronomical( "Gas_Giants", "Standard_Sudarsky_Class_IV" ) );
            ENTRYIDS.Add( (long)1200901, new Astronomical( "Gas_Giants", "Standard_Sudarsky_Class_V" ) );
            ENTRYIDS.Add( (long)1200101, new Astronomical( "Gas_Giants", "Standard_Water_Giant" ) );
            ENTRYIDS.Add( (long)1200302, new Astronomical( "Gas_Giants", "Green_Giant_With_Water_Life" ) );
            ENTRYIDS.Add( (long)1200502, new Astronomical( "Gas_Giants", "Green_Sudarsky_Class_I" ) );
            ENTRYIDS.Add( (long)1200702, new Astronomical( "Gas_Giants", "Green_Sudarsky_Class_III" ) );
            ENTRYIDS.Add( (long)1100301, new Astronomical( "Stars", "A_Type" ) );
            ENTRYIDS.Add( (long)1100302, new Astronomical( "Stars", "A_TypeGiant" ) );
            ENTRYIDS.Add( (long)1100303, new Astronomical( "Stars", "A_TypeSuperGiant" ) );
            ENTRYIDS.Add( (long)1101101, new Astronomical( "Stars", "AEBE_Type" ) );
            ENTRYIDS.Add( (long)1100201, new Astronomical( "Stars", "B_Type" ) );
            ENTRYIDS.Add( (long)1100202, new Astronomical( "Stars", "B_TypeGiant" ) );
            ENTRYIDS.Add( (long)1100203, new Astronomical( "Stars", "B_TypeSuperGiant" ) );
            ENTRYIDS.Add( (long)1102400, new Astronomical( "Stars", "Black_Holes" ) );
            ENTRYIDS.Add( (long)1101401, new Astronomical( "Stars", "C_Type" ) );
            ENTRYIDS.Add( (long)1101402, new Astronomical( "Stars", "C_TypeGiant" ) );
            ENTRYIDS.Add( (long)1101404, new Astronomical( "Stars", "C_TypeHyperGiant" ) );
            ENTRYIDS.Add( (long)1101403, new Astronomical( "Stars", "C_TypeSuperGiant" ) );
            MISSINGIDS.Add( "CJ_Type", new Astronomical( "Stars", "CJ_Type" ) );
            MISSINGIDS.Add( "CN_Type", new Astronomical( "Stars", "CN_Type" ) );
            ENTRYIDS.Add( (long)1102201, new Astronomical( "Stars", "D_Type" ) );
            ENTRYIDS.Add( (long)1102202, new Astronomical( "Stars", "DA_Type" ) );
            ENTRYIDS.Add( (long)1102203, new Astronomical( "Stars", "DAB_Type" ) );
            ENTRYIDS.Add( (long)1102205, new Astronomical( "Stars", "DAV_Type" ) );
            ENTRYIDS.Add( (long)1102206, new Astronomical( "Stars", "DAZ_Type" ) );
            ENTRYIDS.Add( (long)1102207, new Astronomical( "Stars", "DB_Type" ) );
            ENTRYIDS.Add( (long)1102208, new Astronomical( "Stars", "DBV_Type" ) );
            MISSINGIDS.Add( "DBZ_Type", new Astronomical( "Stars", "DBZ_Type" ) );
            ENTRYIDS.Add( (long)1102213, new Astronomical( "Stars", "DC_Type" ) );
            MISSINGIDS.Add( "DCV_Type", new Astronomical( "Stars", "DCV_Type" ) );
            ENTRYIDS.Add( (long)1102212, new Astronomical( "Stars", "DQ_Type" ) );
            ENTRYIDS.Add( (long)1100401, new Astronomical( "Stars", "F_Type" ) );
            ENTRYIDS.Add( (long)1100402, new Astronomical( "Stars", "F_TypeGiant" ) );
            ENTRYIDS.Add( (long)1100403, new Astronomical( "Stars", "F_TypeSuperGiant" ) );
            ENTRYIDS.Add( (long)1100501, new Astronomical( "Stars", "G_Type" ) );
            ENTRYIDS.Add( (long)1100502, new Astronomical( "Stars", "G_TypeGiant" ) );
            ENTRYIDS.Add( (long)1100503, new Astronomical( "Stars", "G_TypeSuperGiant" ) );
            ENTRYIDS.Add( (long)1100601, new Astronomical( "Stars", "K_Type" ) );
            ENTRYIDS.Add( (long)1100602, new Astronomical( "Stars", "K_TypeGiant" ) );
            ENTRYIDS.Add( (long)1100603, new Astronomical( "Stars", "K_TypeSuperGiant" ) );
            ENTRYIDS.Add( (long)1100801, new Astronomical( "Stars", "L_Type" ) );
            ENTRYIDS.Add( (long)1100701, new Astronomical( "Stars", "M_Type" ) );
            ENTRYIDS.Add( (long)1100702, new Astronomical( "Stars", "M_TypeGiant" ) );
            ENTRYIDS.Add( (long)1100703, new Astronomical( "Stars", "M_TypeSuperGiant" ) );
            MISSINGIDS.Add( "MS_Type", new Astronomical( "Stars", "MS_Type" ) );
            ENTRYIDS.Add( (long)1102300, new Astronomical( "Stars", "Neutron_Stars" ) );
            ENTRYIDS.Add( (long)1100101, new Astronomical( "Stars", "O_Type" ) );
            ENTRYIDS.Add( (long)1100102, new Astronomical( "Stars", "O_TypeGiant" ) );
            ENTRYIDS.Add( (long)1100103, new Astronomical( "Stars", "O_TypeSuperGiant" ) );
            ENTRYIDS.Add( (long)1102001, new Astronomical( "Stars", "S_Type" ) );
            ENTRYIDS.Add( (long)1102002, new Astronomical( "Stars", "S_TypeGiant" ) );
            ENTRYIDS.Add( (long)1102500, new Astronomical( "Stars", "SupermassiveBlack_Holes" ) );
            ENTRYIDS.Add( (long)1100901, new Astronomical( "Stars", "T_Type" ) );
            ENTRYIDS.Add( (long)1101001, new Astronomical( "Stars", "TTS_Type" ) );
            ENTRYIDS.Add( (long)1102101, new Astronomical( "Stars", "W_Type" ) );
            ENTRYIDS.Add( (long)1102102, new Astronomical( "Stars", "WC_Type" ) );
            ENTRYIDS.Add( (long)1102103, new Astronomical( "Stars", "WN_Type" ) );
            ENTRYIDS.Add( (long)1102104, new Astronomical( "Stars", "WNC_Type" ) );
            ENTRYIDS.Add( (long)1102105, new Astronomical( "Stars", "WO_Type" ) );
            ENTRYIDS.Add( (long)1101201, new Astronomical( "Stars", "Y_Type" ) );
            ENTRYIDS.Add( (long)1300100, new Astronomical( "Terrestrials", "Earth_Likes" ) );
            ENTRYIDS.Add( (long)1300202, new Astronomical( "Terrestrials", "Standard_Ammonia_Worlds" ) );
            ENTRYIDS.Add( (long)1300501, new Astronomical( "Terrestrials", "Standard_High_Metal_Content_No_Atmos" ) );
            ENTRYIDS.Add( (long)1300801, new Astronomical( "Terrestrials", "Standard_Ice_No_Atmos" ) );
            ENTRYIDS.Add( (long)1300401, new Astronomical( "Terrestrials", "Standard_Metal_Rich_No_Atmos" ) );
            ENTRYIDS.Add( (long)1300701, new Astronomical( "Terrestrials", "Standard_Rocky_Ice_No_Atmos" ) );
            ENTRYIDS.Add( (long)1300601, new Astronomical( "Terrestrials", "Standard_Rocky_No_Atmos" ) );
            ENTRYIDS.Add( (long)1301501, new Astronomical( "Terrestrials", "Standard_Ter_High_Metal_Content" ) );
            ENTRYIDS.Add( (long)1301801, new Astronomical( "Terrestrials", "Standard_Ter_Ice" ) );
            ENTRYIDS.Add( (long)1301401, new Astronomical( "Terrestrials", "Standard_Ter_Metal_Rich" ) );
            ENTRYIDS.Add( (long)1301701, new Astronomical( "Terrestrials", "Standard_Ter_Rocky_Ice" ) );
            ENTRYIDS.Add( (long)1301601, new Astronomical( "Terrestrials", "Standard_Ter_Rocky" ) );
            ENTRYIDS.Add( (long)1300301, new Astronomical( "Terrestrials", "Standard_Water_Worlds" ) );
            MISSINGIDS.Add( "TRF_Ammonia_Worlds", new Astronomical( "Terrestrials", "TRF_Ammonia_Worlds" ) );
            ENTRYIDS.Add( (long)1300502, new Astronomical( "Terrestrials", "TRF_High_Metal_Content_No_Atmos" ) );
            ENTRYIDS.Add( (long)1300602, new Astronomical( "Terrestrials", "TRF_Rocky_No_Atmos" ) );
            ENTRYIDS.Add( (long)1301502, new Astronomical( "Terrestrials", "TRF_Ter_High_Metal_Content" ) );
            MISSINGIDS.Add( "TRF_Ter_Metal_Rich", new Astronomical( "Terrestrials", "TRF_Ter_Metal_Rich" ) );
            ENTRYIDS.Add( (long)1301602, new Astronomical( "Terrestrials", "TRF_Ter_Rocky" ) );
            ENTRYIDS.Add( (long)1300302, new Astronomical( "Terrestrials", "TRF_Water_Worlds" ) );

            ASTRONOMICALS.Add( "Green_Giant_With_Ammonia_Life", (long)1200402 );
            ASTRONOMICALS.Add( "Green_Sudarsky_Class_II", (long)1200602 );
            ASTRONOMICALS.Add( "Green_Sudarsky_Class_IV", (long)1200802 );
            ASTRONOMICALS.Add( "Green_Sudarsky_Class_V", (long)1200902 );
            ASTRONOMICALS.Add( "Green_Water_Giant", (long)1200102 );
            ASTRONOMICALS.Add( "Standard_Giant_With_Ammonia_Life", (long)1200401 );
            ASTRONOMICALS.Add( "Standard_Giant_With_Water_Life", (long)1200301 );
            ASTRONOMICALS.Add( "Standard_Helium", null );
            ASTRONOMICALS.Add( "Standard_Helium_Rich", (long)1201001 );
            ASTRONOMICALS.Add( "Standard_Sudarsky_Class_I", (long)1200501 );
            ASTRONOMICALS.Add( "Standard_Sudarsky_Class_II", (long)1200601 );
            ASTRONOMICALS.Add( "Standard_Sudarsky_Class_III", (long)1200701 );
            ASTRONOMICALS.Add( "Standard_Sudarsky_Class_IV", (long)1200801 );
            ASTRONOMICALS.Add( "Standard_Sudarsky_Class_V", (long)1200901 );
            ASTRONOMICALS.Add( "Standard_Water_Giant", (long)1200101 );
            ASTRONOMICALS.Add( "Green_Giant_With_Water_Life", (long)1200302 );
            ASTRONOMICALS.Add( "Green_Sudarsky_Class_I", (long)1200502 );
            ASTRONOMICALS.Add( "Green_Sudarsky_Class_III", (long)1200702 );
            ASTRONOMICALS.Add( "A_Type", (long)1100301 );
            ASTRONOMICALS.Add( "A_TypeGiant", (long)1100302 );
            ASTRONOMICALS.Add( "A_TypeSuperGiant", (long)1100303 );
            ASTRONOMICALS.Add( "AEBE_Type", (long)1101101 );
            ASTRONOMICALS.Add( "B_Type", (long)1100201 );
            ASTRONOMICALS.Add( "B_TypeGiant", (long)1100202 );
            ASTRONOMICALS.Add( "B_TypeSuperGiant", (long)1100203 );
            ASTRONOMICALS.Add( "Black_Holes", (long)1102400 );
            ASTRONOMICALS.Add( "C_Type", (long)1101401 );
            ASTRONOMICALS.Add( "C_TypeGiant", (long)1101402 );
            ASTRONOMICALS.Add( "C_TypeHyperGiant", (long)1101404 );
            ASTRONOMICALS.Add( "C_TypeSuperGiant", (long)1101403 );
            ASTRONOMICALS.Add( "CJ_Type", null );
            ASTRONOMICALS.Add( "CN_Type", null );
            ASTRONOMICALS.Add( "D_Type", (long)1102201 );
            ASTRONOMICALS.Add( "DA_Type", (long)1102202 );
            ASTRONOMICALS.Add( "DAB_Type", (long)1102203 );
            ASTRONOMICALS.Add( "DAV_Type", (long)1102205 );
            ASTRONOMICALS.Add( "DAZ_Type", (long)1102206 );
            ASTRONOMICALS.Add( "DB_Type", (long)1102207 );
            ASTRONOMICALS.Add( "DBV_Type", (long)1102208 );
            ASTRONOMICALS.Add( "DBZ_Type", null );
            ASTRONOMICALS.Add( "DC_Type", (long)1102213 );
            ASTRONOMICALS.Add( "DCV_Type", null );
            ASTRONOMICALS.Add( "DQ_Type", (long)1102212 );
            ASTRONOMICALS.Add( "F_Type", (long)1100401 );
            ASTRONOMICALS.Add( "F_TypeGiant", (long)1100402 );
            ASTRONOMICALS.Add( "F_TypeSuperGiant", (long)1100403 );
            ASTRONOMICALS.Add( "G_Type", (long)1100501 );
            ASTRONOMICALS.Add( "G_TypeGiant", (long)1100502 );
            ASTRONOMICALS.Add( "G_TypeSuperGiant", (long)1100503 );
            ASTRONOMICALS.Add( "K_Type", (long)1100601 );
            ASTRONOMICALS.Add( "K_TypeGiant", (long)1100602 );
            ASTRONOMICALS.Add( "K_TypeSuperGiant", (long)1100603 );
            ASTRONOMICALS.Add( "L_Type", (long)1100801 );
            ASTRONOMICALS.Add( "M_Type", (long)1100701 );
            ASTRONOMICALS.Add( "M_TypeGiant", (long)1100702 );
            ASTRONOMICALS.Add( "M_TypeSuperGiant", (long)1100703 );
            ASTRONOMICALS.Add( "MS_Type", null );
            ASTRONOMICALS.Add( "Neutron_Stars", (long)1102300 );
            ASTRONOMICALS.Add( "O_Type", (long)1100101 );
            ASTRONOMICALS.Add( "O_TypeGiant", (long)1100102 );
            ASTRONOMICALS.Add( "O_TypeSuperGiant", (long)1100103 );
            ASTRONOMICALS.Add( "S_Type", (long)1102001 );
            ASTRONOMICALS.Add( "S_TypeGiant", (long)1102002 );
            ASTRONOMICALS.Add( "SupermassiveBlack_Holes", (long)1102500 );
            ASTRONOMICALS.Add( "T_Type", (long)1100901 );
            ASTRONOMICALS.Add( "TTS_Type", (long)1101001 );
            ASTRONOMICALS.Add( "W_Type", (long)1102101 );
            ASTRONOMICALS.Add( "WC_Type", (long)1102102 );
            ASTRONOMICALS.Add( "WN_Type", (long)1102103 );
            ASTRONOMICALS.Add( "WNC_Type", (long)1102104 );
            ASTRONOMICALS.Add( "WO_Type", (long)1102105 );
            ASTRONOMICALS.Add( "Y_Type", (long)1101201 );
            ASTRONOMICALS.Add( "Earth_Likes", (long)1300100 );
            ASTRONOMICALS.Add( "Standard_Ammonia_Worlds", (long)1300202 );
            ASTRONOMICALS.Add( "Standard_High_Metal_Content_No_Atmos", (long)1300501 );
            ASTRONOMICALS.Add( "Standard_Ice_No_Atmos", (long)1300801 );
            ASTRONOMICALS.Add( "Standard_Metal_Rich_No_Atmos", (long)1300401 );
            ASTRONOMICALS.Add( "Standard_Rocky_Ice_No_Atmos", (long)1300701 );
            ASTRONOMICALS.Add( "Standard_Rocky_No_Atmos", (long)1300601 );
            ASTRONOMICALS.Add( "Standard_Ter_High_Metal_Content", (long)1301501 );
            ASTRONOMICALS.Add( "Standard_Ter_Ice", (long)1301801 );
            ASTRONOMICALS.Add( "Standard_Ter_Metal_Rich", (long)1301401 );
            ASTRONOMICALS.Add( "Standard_Ter_Rocky_Ice", (long)1301701 );
            ASTRONOMICALS.Add( "Standard_Ter_Rocky", (long)1301601 );
            ASTRONOMICALS.Add( "Standard_Water_Worlds", (long)1300301 );
            ASTRONOMICALS.Add( "TRF_Ammonia_Worlds", null );
            ASTRONOMICALS.Add( "TRF_High_Metal_Content_No_Atmos", (long)1300502 );
            ASTRONOMICALS.Add( "TRF_Rocky_No_Atmos", (long)1300602 );
            ASTRONOMICALS.Add( "TRF_Ter_High_Metal_Content", (long)1301502 );
            ASTRONOMICALS.Add( "TRF_Ter_Metal_Rich", null );
            ASTRONOMICALS.Add( "TRF_Ter_Rocky", (long)1301602 );
            ASTRONOMICALS.Add( "TRF_Water_Worlds", (long)1300302 );
        }

        // dummy used to ensure that the static constructor has run
        public Astronomical () : this( "" )
        { }

        private Astronomical ( string name ) : base( name, name )
        {
            this.exists = false;
            this.type = new AstronomicalType();
            //this.value = 0;
            this.description = "";
        }

        private Astronomical ( string type, string name ) : base( name, name )
        {
            this.exists = true;
            this.type = AstronomicalType.Lookup( type );
            //this.value = value;
            this.description = rmAstronomicalDesc.GetString( name );
        }

        /// <summary>
        /// Try getting data from the entryid first, then use name as a fallback
        /// </summary>
        public static Astronomical Lookup ( long? entryId, string name )
        {
            Astronomical item;
            item = LookupByEntryId( entryId );
            
            // EntryId doesn't exist, try name
            if ( item == null )
            {
                item = LookupByName( name );
            }

            // Name doesn't exist, or ID reverse lookup unknown (null).
            // See if its in the missing ID list as a last resort.
            if ( item == null )
            {
                item = LookupMissing( name );
            }

            if ( item == null )
            {
                item = new Astronomical();
            }

            return item;
        }

        /// <summary>
        /// Preferred method of lookup
        /// </summary>
        public static Astronomical LookupByEntryId ( long? entryId )
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
        /// Lookup data by name
        /// </summary>
        public static Astronomical LookupByName ( string name )
        {
            if ( name != "" )
            {
                if ( ASTRONOMICALS.ContainsKey( name ) )
                {
                    long? entryid = ASTRONOMICALS[ name ];
                    if ( entryid != null )
                    {
                        return LookupByEntryId( entryid );
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Lookup data in missing ID list
        /// </summary>
        public static Astronomical LookupMissing ( string name )
        {
            if ( name != "" )
            {
                if ( MISSINGIDS.ContainsKey( name ) )
                {
                    return MISSINGIDS[ name ];
                }
            }
            return null;
        }
    }
}

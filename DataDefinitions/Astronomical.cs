using System;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class Astronomical : ResourceBasedLocalizedEDName<Astronomical>
    {
        static Astronomical ()
        {
            resourceManager = Properties.Astronomical.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new Astronomical( edname, null, null );
        }

        public static readonly Astronomical Green_Giant_With_Ammonia_Life = new Astronomical ( "Green_Giant_With_Ammonia_Life", 1200402, AstronomicalType.GasGiants );
        public static readonly Astronomical Green_Sudarsky_Class_I = new Astronomical ( "Green_Sudarsky_Class_I", 1200502, AstronomicalType.GasGiants );
        public static readonly Astronomical Green_Sudarsky_Class_II = new Astronomical ( "Green_Sudarsky_Class_II", 1200602, AstronomicalType.GasGiants );
        public static readonly Astronomical Green_Sudarsky_Class_III = new Astronomical ( "Green_Sudarsky_Class_III", 1200702, AstronomicalType.GasGiants );
        public static readonly Astronomical Green_Sudarsky_Class_IV = new Astronomical ( "Green_Sudarsky_Class_IV", 1200802, AstronomicalType.GasGiants );
        public static readonly Astronomical Green_Sudarsky_Class_V = new Astronomical ( "Green_Sudarsky_Class_V", 1200902, AstronomicalType.GasGiants );
        public static readonly Astronomical Green_Water_Giant = new Astronomical ( "Green_Water_Giant", 1200102, AstronomicalType.GasGiants );
        public static readonly Astronomical Green_Giant_With_Water_Life = new Astronomical ( "Green_Giant_With_Water_Life", 1200302, AstronomicalType.GasGiants );
        public static readonly Astronomical Standard_Giant_With_Ammonia_Life = new Astronomical ( "Standard_Giant_With_Ammonia_Life", 1200401, AstronomicalType.GasGiants );
        public static readonly Astronomical Standard_Giant_With_Water_Life = new Astronomical ( "Standard_Giant_With_Water_Life", 1200301, AstronomicalType.GasGiants );
        public static readonly Astronomical Standard_Helium = new Astronomical ( "Standard_Helium", null, AstronomicalType.GasGiants );
        public static readonly Astronomical Standard_Helium_Rich = new Astronomical ( "Standard_Helium_Rich", 1201001, AstronomicalType.GasGiants );
        public static readonly Astronomical Standard_Sudarsky_Class_I = new Astronomical ( "Standard_Sudarsky_Class_I", 1200501, AstronomicalType.GasGiants );
        public static readonly Astronomical Standard_Sudarsky_Class_II = new Astronomical ( "Standard_Sudarsky_Class_II", 1200601, AstronomicalType.GasGiants );
        public static readonly Astronomical Standard_Sudarsky_Class_III = new Astronomical ( "Standard_Sudarsky_Class_III", 1200701, AstronomicalType.GasGiants );
        public static readonly Astronomical Standard_Sudarsky_Class_IV = new Astronomical ( "Standard_Sudarsky_Class_IV", 1200801, AstronomicalType.GasGiants );
        public static readonly Astronomical Standard_Sudarsky_Class_V = new Astronomical ( "Standard_Sudarsky_Class_V", 1200901, AstronomicalType.GasGiants );
        public static readonly Astronomical Standard_Water_Giant = new Astronomical ( "Standard_Water_Giant", 1200101, AstronomicalType.GasGiants );

        public static readonly Astronomical A_Type = new Astronomical ( "A_Type", 1100301, AstronomicalType.Stars );
        public static readonly Astronomical A_TypeGiant = new Astronomical ( "A_TypeGiant", 1100302, AstronomicalType.Stars );
        public static readonly Astronomical A_TypeSuperGiant = new Astronomical ( "A_TypeSuperGiant", 1100303, AstronomicalType.Stars );
        public static readonly Astronomical AEBE_Type = new Astronomical ( "AEBE_Type", 1101101, AstronomicalType.Stars );
        public static readonly Astronomical B_Type = new Astronomical ( "B_Type", 1100201, AstronomicalType.Stars );
        public static readonly Astronomical B_TypeGiant = new Astronomical ( "B_TypeGiant", 1100202, AstronomicalType.Stars );
        public static readonly Astronomical B_TypeSuperGiant = new Astronomical ( "B_TypeSuperGiant", 1100203, AstronomicalType.Stars );
        public static readonly Astronomical Black_Holes = new Astronomical ( "Black_Holes", 1102400, AstronomicalType.Stars );
        public static readonly Astronomical C_Type = new Astronomical ( "C_Type", 1101401, AstronomicalType.Stars );
        public static readonly Astronomical C_TypeGiant = new Astronomical ( "C_TypeGiant", 1101402, AstronomicalType.Stars );
        public static readonly Astronomical C_TypeHyperGiant = new Astronomical ( "C_TypeHyperGiant", 1101404, AstronomicalType.Stars );
        public static readonly Astronomical C_TypeSuperGiant = new Astronomical ( "C_TypeSuperGiant", 1101403, AstronomicalType.Stars );
        public static readonly Astronomical CJ_Type = new Astronomical ( "CJ_Type", null, AstronomicalType.Stars );
        public static readonly Astronomical CN_Type = new Astronomical ( "CN_Type", null, AstronomicalType.Stars );
        public static readonly Astronomical D_Type = new Astronomical ( "D_Type", 1102201, AstronomicalType.Stars );
        public static readonly Astronomical DA_Type = new Astronomical ( "DA_Type", 1102202, AstronomicalType.Stars );
        public static readonly Astronomical DAB_Type = new Astronomical ( "DAB_Type", 1102203, AstronomicalType.Stars );
        public static readonly Astronomical DAV_Type = new Astronomical ( "DAV_Type", 1102205, AstronomicalType.Stars );
        public static readonly Astronomical DAZ_Type = new Astronomical ( "DAZ_Type", 1102206, AstronomicalType.Stars );
        public static readonly Astronomical DB_Type = new Astronomical ( "DB_Type", 1102207, AstronomicalType.Stars );
        public static readonly Astronomical DBV_Type = new Astronomical ( "DBV_Type", 1102208, AstronomicalType.Stars );
        public static readonly Astronomical DBZ_Type = new Astronomical ( "DBZ_Type", null, AstronomicalType.Stars );
        public static readonly Astronomical DC_Type = new Astronomical ( "DC_Type", 1102213, AstronomicalType.Stars );
        public static readonly Astronomical DCV_Type = new Astronomical ( "DCV_Type", null, AstronomicalType.Stars );
        public static readonly Astronomical DQ_Type = new Astronomical ( "DQ_Type", 1102212, AstronomicalType.Stars );
        public static readonly Astronomical F_Type = new Astronomical ( "F_Type", 1100401, AstronomicalType.Stars );
        public static readonly Astronomical F_TypeGiant = new Astronomical ( "F_TypeGiant", 1100402, AstronomicalType.Stars );
        public static readonly Astronomical F_TypeSuperGiant = new Astronomical ( "F_TypeSuperGiant", 1100403, AstronomicalType.Stars );
        public static readonly Astronomical G_Type = new Astronomical ( "G_Type", 1100501, AstronomicalType.Stars );
        public static readonly Astronomical G_TypeGiant = new Astronomical ( "G_TypeGiant", 1100502, AstronomicalType.Stars );
        public static readonly Astronomical G_TypeSuperGiant = new Astronomical ( "G_TypeSuperGiant", 1100503, AstronomicalType.Stars );
        public static readonly Astronomical K_Type = new Astronomical ( "K_Type", 1100601, AstronomicalType.Stars );
        public static readonly Astronomical K_TypeGiant = new Astronomical ( "K_TypeGiant", 1100602, AstronomicalType.Stars );
        public static readonly Astronomical K_TypeSuperGiant = new Astronomical ( "K_TypeSuperGiant", 1100603, AstronomicalType.Stars );
        public static readonly Astronomical L_Type = new Astronomical ( "L_Type", 1100801, AstronomicalType.Stars );
        public static readonly Astronomical M_Type = new Astronomical ( "M_Type", 1100701, AstronomicalType.Stars );
        public static readonly Astronomical M_TypeGiant = new Astronomical ( "M_TypeGiant", 1100702, AstronomicalType.Stars );
        public static readonly Astronomical M_TypeSuperGiant = new Astronomical ( "M_TypeSuperGiant", 1100703, AstronomicalType.Stars );
        public static readonly Astronomical MS_Type = new Astronomical ( "MS_Type", null, AstronomicalType.Stars );
        public static readonly Astronomical Neutron_Stars = new Astronomical ( "Neutron_Stars", 1102300, AstronomicalType.Stars );
        public static readonly Astronomical O_Type = new Astronomical ( "O_Type", 1100101, AstronomicalType.Stars );
        public static readonly Astronomical O_TypeGiant = new Astronomical ( "O_TypeGiant", 1100102, AstronomicalType.Stars );
        public static readonly Astronomical O_TypeSuperGiant = new Astronomical ( "O_TypeSuperGiant", 1100103, AstronomicalType.Stars );
        public static readonly Astronomical S_Type = new Astronomical ( "S_Type", 1102001, AstronomicalType.Stars );
        public static readonly Astronomical S_TypeGiant = new Astronomical ( "S_TypeGiant", 1102002, AstronomicalType.Stars );
        public static readonly Astronomical SupermassiveBlack_Holes = new Astronomical ( "SupermassiveBlack_Holes", 1102500, AstronomicalType.Stars );
        public static readonly Astronomical T_Type = new Astronomical ( "T_Type", 1100901, AstronomicalType.Stars );
        public static readonly Astronomical TTS_Type = new Astronomical ( "TTS_Type", 1101001, AstronomicalType.Stars );
        public static readonly Astronomical W_Type = new Astronomical ( "W_Type", 1102101, AstronomicalType.Stars );
        public static readonly Astronomical WC_Type = new Astronomical ( "WC_Type", 1102102, AstronomicalType.Stars );
        public static readonly Astronomical WN_Type = new Astronomical ( "WN_Type", 1102103, AstronomicalType.Stars );
        public static readonly Astronomical WNC_Type = new Astronomical ( "WNC_Type", 1102104, AstronomicalType.Stars );
        public static readonly Astronomical WO_Type = new Astronomical ( "WO_Type", 1102105, AstronomicalType.Stars );
        public static readonly Astronomical Y_Type = new Astronomical ( "Y_Type", 1101201, AstronomicalType.Stars );

        public static readonly Astronomical Earth_Likes = new Astronomical( "Earth_Likes", 1300100, AstronomicalType.Terrestrials );
        public static readonly Astronomical Standard_Ammonia_Worlds = new Astronomical( "Standard_Ammonia_Worlds", 1300202, AstronomicalType.Terrestrials );
        public static readonly Astronomical Standard_High_Metal_Content_No_Atmos = new Astronomical( "Standard_High_Metal_Content_No_Atmos", 1300501, AstronomicalType.Terrestrials );
        public static readonly Astronomical Standard_Ice_No_Atmos = new Astronomical ( "Standard_Ice_No_Atmos", 1300801, AstronomicalType.Terrestrials );
        public static readonly Astronomical Standard_Metal_Rich_No_Atmos = new Astronomical ( "Standard_Metal_Rich_No_Atmos", 1300401, AstronomicalType.Terrestrials );
        public static readonly Astronomical Standard_Rocky_Ice_No_Atmos = new Astronomical ( "Standard_Rocky_Ice_No_Atmos", 1300701, AstronomicalType.Terrestrials );
        public static readonly Astronomical Standard_Rocky_No_Atmos = new Astronomical ( "Standard_Rocky_No_Atmos", 1300601, AstronomicalType.Terrestrials );
        public static readonly Astronomical Standard_Ter_High_Metal_Content = new Astronomical ( "Standard_Ter_High_Metal_Content", 1301501, AstronomicalType.Terrestrials );
        public static readonly Astronomical Standard_Ter_Ice = new Astronomical ( "Standard_Ter_Ice", 1301801, AstronomicalType.Terrestrials );
        public static readonly Astronomical Standard_Ter_Metal_Rich = new Astronomical ( "Standard_Ter_Metal_Rich", 1301401, AstronomicalType.Terrestrials );
        public static readonly Astronomical Standard_Ter_Rocky_Ice = new Astronomical ( "Standard_Ter_Rocky_Ice", 1301701, AstronomicalType.Terrestrials );
        public static readonly Astronomical Standard_Ter_Rocky = new Astronomical ( "Standard_Ter_Rocky", 1301601, AstronomicalType.Terrestrials );
        public static readonly Astronomical Standard_Water_Worlds = new Astronomical ( "Standard_Water_Worlds", 1300301, AstronomicalType.Terrestrials );
        public static readonly Astronomical TRF_Ammonia_Worlds = new Astronomical ( "TRF_Ammonia_Worlds", null, AstronomicalType.Terrestrials );
        public static readonly Astronomical TRF_High_Metal_Content_No_Atmos = new Astronomical ( "TRF_High_Metal_Content_No_Atmos", 1300502, AstronomicalType.Terrestrials );
        public static readonly Astronomical TRF_Rocky_No_Atmos = new Astronomical ( "TRF_Rocky_No_Atmos", 1300602, AstronomicalType.Terrestrials );
        public static readonly Astronomical TRF_Ter_High_Metal_Content = new Astronomical ( "TRF_Ter_High_Metal_Content", 1301502, AstronomicalType.Terrestrials );
        public static readonly Astronomical TRF_Ter_Metal_Rich = new Astronomical( "TRF_Ter_Metal_Rich", null, AstronomicalType.Terrestrials );
        public static readonly Astronomical TRF_Ter_Rocky = new Astronomical( "TRF_Ter_Rocky", 1301602, AstronomicalType.Terrestrials );
        public static readonly Astronomical TRF_Water_Worlds = new Astronomical ( "TRF_Water_Worlds", 1300302, AstronomicalType.Terrestrials );

        public long? entryID { get; private set; }

        [PublicAPI]
        public AstronomicalType type { get; private set; }

        [PublicAPI]
        public string localizedDescription { get; private set; }

        // dummy used to ensure that the static constructor has run
        public Astronomical () : this( "", null, null )
        { }

        private Astronomical ( string edname, long? entryID, AstronomicalType type ) : base( edname, edname )
        {
            this.entryID = entryID;
            this.type = type;
            this.localizedDescription = AllOfThem.Any(a => a.edname == edname) 
                ? Properties.AstronomicalDesc.ResourceManager.GetString( edname ) 
                : string.Empty;
        }

        /// <summary>
        /// Try getting data from the entryid first, then use edname as a fallback
        /// </summary>
        public static Astronomical Lookup ( long? entryId, string edname )
        {
            try
            {
                if ( entryId != null )
                {
                    return AllOfThem.Single( a => a.entryID == entryId );
                }
            }
            catch ( InvalidOperationException e )
            {
                if ( AllOfThem.Count( a => a.entryID == entryId ) > 1 )
                {
                    Logging.Error( $"Duplicate EntryID value {entryId} in {nameof( Astronomical )}.", e );
                }
                else if ( AllOfThem.All( a => a.entryID != entryId ) )
                {
                    Logging.Error( $"Unknown EntryID value {entryId} with edname {edname} in {nameof( Astronomical )}.", e );
                }
            }

            return FromEDName( edname ) ??
                   new Astronomical( edname, entryId, null ); // No match.
        }
    }
}

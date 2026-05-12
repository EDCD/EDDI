using System;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class CodexStellarBody : ResourceBasedLocalizedEDName<CodexStellarBody>
    {
        static CodexStellarBody ()
        {
            resourceManager = Properties.CodexStellarBody.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new CodexStellarBody( edname, null, null );
        }

        public static readonly CodexStellarBody Green_Giant_With_Ammonia_Life = new( "Green_Giant_With_Ammonia_Life", 1200402, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody Green_Sudarsky_Class_I = new( "Green_Sudarsky_Class_I", 1200502, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody Green_Sudarsky_Class_II = new( "Green_Sudarsky_Class_II", 1200602, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody Green_Sudarsky_Class_III = new( "Green_Sudarsky_Class_III", 1200702, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody Green_Sudarsky_Class_IV = new( "Green_Sudarsky_Class_IV", 1200802, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody Green_Sudarsky_Class_V = new( "Green_Sudarsky_Class_V", 1200902, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody Green_Water_Giant = new( "Green_Water_Giant", 1200102, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody Green_Giant_With_Water_Life = new( "Green_Giant_With_Water_Life", 1200302, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody Standard_Giant_With_Ammonia_Life = new( "Standard_Giant_With_Ammonia_Life", 1200401, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody Standard_Giant_With_Water_Life = new( "Standard_Giant_With_Water_Life", 1200301, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody Standard_Helium = new( "Standard_Helium", null, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody Standard_Helium_Rich = new( "Standard_Helium_Rich", 1201001, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody Standard_Sudarsky_Class_I = new( "Standard_Sudarsky_Class_I", 1200501, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody Standard_Sudarsky_Class_II = new( "Standard_Sudarsky_Class_II", 1200601, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody Standard_Sudarsky_Class_III = new( "Standard_Sudarsky_Class_III", 1200701, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody Standard_Sudarsky_Class_IV = new( "Standard_Sudarsky_Class_IV", 1200801, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody Standard_Sudarsky_Class_V = new( "Standard_Sudarsky_Class_V", 1200901, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody Standard_Water_Giant = new( "Standard_Water_Giant", 1200101, CodexStellarBodyType.GasGiants );

        public static readonly CodexStellarBody A_Type = new("A_Type", 1100301, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody A_TypeGiant = new("A_TypeGiant", 1100302, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody A_TypeSuperGiant = new( "A_TypeSuperGiant", 1100303, CodexStellarBodyType.Stars );

        public static readonly CodexStellarBody AEBE_Type = new("AEBE_Type", 1101101, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody B_Type = new("B_Type", 1100201, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody B_TypeGiant = new("B_TypeGiant", 1100202, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody B_TypeSuperGiant = new( "B_TypeSuperGiant", 1100203, CodexStellarBodyType.Stars );

        public static readonly CodexStellarBody Black_Holes = new("Black_Holes", 1102400, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody C_Type = new("C_Type", 1101401, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody C_TypeGiant = new("C_TypeGiant", 1101402, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody C_TypeHyperGiant = new( "C_TypeHyperGiant", 1101404, CodexStellarBodyType.Stars );

        public static readonly CodexStellarBody C_TypeSuperGiant = new( "C_TypeSuperGiant", 1101403, CodexStellarBodyType.Stars );

        public static readonly CodexStellarBody CJ_Type = new("CJ_Type", null, CodexStellarBodyType.Stars);
        
        public static readonly CodexStellarBody CJ_TypeGiant = new("CJ_TypeGiant", 1101602, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody CN_Type = new("CN_Type", null, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody CN_TypeGiant = new("CN_TypeGiant", 1101502, CodexStellarBodyType.Stars);
        
        public static readonly CodexStellarBody D_Type = new("D_Type", 1102201, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody DA_Type = new("DA_Type", 1102202, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody DAB_Type = new("DAB_Type", 1102203, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody DAV_Type = new("DAV_Type", 1102205, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody DAZ_Type = new("DAZ_Type", 1102206, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody DB_Type = new("DB_Type", 1102207, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody DBV_Type = new("DBV_Type", 1102208, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody DBZ_Type = new("DBZ_Type", null, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody DC_Type = new("DC_Type", 1102213, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody DCV_Type = new("DCV_Type", null, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody DQ_Type = new("DQ_Type", 1102212, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody F_Type = new("F_Type", 1100401, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody F_TypeGiant = new("F_TypeGiant", 1100402, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody F_TypeSuperGiant = new( "F_TypeSuperGiant", 1100403, CodexStellarBodyType.Stars );

        public static readonly CodexStellarBody G_Type = new("G_Type", 1100501, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody G_TypeGiant = new("G_TypeGiant", 1100502, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody G_TypeSuperGiant = new( "G_TypeSuperGiant", 1100503, CodexStellarBodyType.Stars );

        public static readonly CodexStellarBody K_Type = new("K_Type", 1100601, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody K_TypeGiant = new("K_TypeGiant", 1100602, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody K_TypeSuperGiant = new( "K_TypeSuperGiant", 1100603, CodexStellarBodyType.Stars );

        public static readonly CodexStellarBody L_Type = new("L_Type", 1100801, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody M_Type = new("M_Type", 1100701, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody M_TypeGiant = new("M_TypeGiant", 1100702, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody M_TypeSuperGiant = new( "M_TypeSuperGiant", 1100703, CodexStellarBodyType.Stars );

        public static readonly CodexStellarBody MS_Type = new("MS_Type", null, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody MS_TypeGiant = new("MS_TypeGiant", 1101902, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody Neutron_Stars = new( "Neutron_Stars", 1102300, CodexStellarBodyType.Stars );

        public static readonly CodexStellarBody O_Type = new("O_Type", 1100101, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody O_TypeGiant = new("O_TypeGiant", 1100102, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody O_TypeSuperGiant = new( "O_TypeSuperGiant", 1100103, CodexStellarBodyType.Stars );

        public static readonly CodexStellarBody S_Type = new("S_Type", 1102001, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody S_TypeGiant = new("S_TypeGiant", 1102002, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody SupermassiveBlack_Holes = new( "SupermassiveBlack_Holes", 1102500, CodexStellarBodyType.Stars );

        public static readonly CodexStellarBody T_Type = new("T_Type", 1100901, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody TTS_Type = new("TTS_Type", 1101001, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody W_Type = new("W_Type", 1102101, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody WC_Type = new("WC_Type", 1102102, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody WN_Type = new("WN_Type", 1102103, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody WNC_Type = new("WNC_Type", 1102104, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody WO_Type = new("WO_Type", 1102105, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody Y_Type = new("Y_Type", 1101201, CodexStellarBodyType.Stars);

        public static readonly CodexStellarBody Earth_Likes = new( "Earth_Likes", 1300100, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody Standard_Ammonia_Worlds = new( "Standard_Ammonia_Worlds", 1300202, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody Standard_High_Metal_Content_No_Atmos = new( "Standard_High_Metal_Content_No_Atmos", 1300501, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody Standard_Ice_No_Atmos = new( "Standard_Ice_No_Atmos", 1300801, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody Standard_Metal_Rich_No_Atmos = new( "Standard_Metal_Rich_No_Atmos", 1300401, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody Standard_Rocky_Ice_No_Atmos = new( "Standard_Rocky_Ice_No_Atmos", 1300701, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody Standard_Rocky_No_Atmos = new( "Standard_Rocky_No_Atmos", 1300601, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody Standard_Ter_High_Metal_Content = new( "Standard_Ter_High_Metal_Content", 1301501, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody Standard_Ter_Ice = new( "Standard_Ter_Ice", 1301801, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody Standard_Ter_Metal_Rich = new( "Standard_Ter_Metal_Rich", 1301401, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody Standard_Ter_Rocky_Ice = new( "Standard_Ter_Rocky_Ice", 1301701, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody Standard_Ter_Rocky = new( "Standard_Ter_Rocky", 1301601, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody Standard_Water_Worlds = new( "Standard_Water_Worlds", 1300301, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody TRF_Ammonia_Worlds = new( "TRF_Ammonia_Worlds", null, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody TRF_High_Metal_Content_No_Atmos = new( "TRF_High_Metal_Content_No_Atmos", 1300502, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody TRF_Rocky_No_Atmos = new( "TRF_Rocky_No_Atmos", 1300602, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody TRF_Ter_High_Metal_Content = new( "TRF_Ter_High_Metal_Content", 1301502, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody TRF_Ter_Metal_Rich = new( "TRF_Ter_Metal_Rich", null, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody TRF_Ter_Rocky = new( "TRF_Ter_Rocky", 1301602, CodexStellarBodyType.Terrestrials );

        public static readonly CodexStellarBody TRF_Water_Worlds = new( "TRF_Water_Worlds", 1300302, CodexStellarBodyType.Terrestrials );

        public long? entryID { get; private set; }

        [ PublicAPI ] public CodexStellarBodyType type { get; private set; }

        [ PublicAPI ] public string localizedDescription { get; private set; }

        // dummy used to ensure that the static constructor has run
        public CodexStellarBody () : this( "", null, null )
        { }

        private CodexStellarBody ( string edname, long? entryID, CodexStellarBodyType type ) : base( edname, edname )
        {
            this.entryID = entryID;
            this.type = type;
            this.localizedDescription = AllOfThem.Any( a => a.edname == edname )
                ? Properties.CodexStellarBodyDesc.ResourceManager.GetString( edname )
                : string.Empty;
        }

        /// <summary>
        /// Try getting data from the entryid first, then use edname as a fallback
        /// </summary>
        public static CodexStellarBody Lookup ( long? entryId, string edname )
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
                    Logging.Error( $"Duplicate EntryID value {entryId} in {nameof(CodexStellarBody)}.", e );
                }
                else if ( AllOfThem.All( a => a.entryID != entryId ) )
                {
                    Logging.Error(
                        $"Unknown EntryID value {entryId} with edname {edname} in {nameof(CodexStellarBody)}.", e );
                }
            }

            return FromEDName( edname ) ?? new CodexStellarBody( edname, entryId, null ); // No match.
        }
    }
}
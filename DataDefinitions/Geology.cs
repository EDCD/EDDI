using System;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class Geology : ResourceBasedLocalizedEDName<Geology>
    {
        static Geology ()
        {
            resourceManager = Properties.Geology.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new Geology( edname );
        }

        public static readonly Geology Fumarole_CarbonDioxideGeysers = new Geology( "Fumarole_CarbonDioxideGeysers", 1400109, GeologyType.Fumarole, (long)50000 );
        public static readonly Geology Fumarole_SilicateVapourGeysers = new Geology( "Fumarole_SilicateVapourGeysers", 1400114, GeologyType.Fumarole, (long)50000 );
        public static readonly Geology Fumarole_SulphurDioxideMagma = new Geology( "Fumarole_SulphurDioxideMagma", 1400102, GeologyType.Fumarole, (long)50000 );
        public static readonly Geology Fumarole_WaterGeysers = new Geology( "Fumarole_WaterGeysers", 1400108, GeologyType.Fumarole, (long)50000 );
        public static readonly Geology Gas_Clds_Blue = new Geology( "Gas_Clds_Blue", 1400601, GeologyType.LagrangeCloud, (long)50000 );
        public static readonly Geology Gas_Clds_Green = new Geology( "Gas_Clds_Green", 1400701, GeologyType.LagrangeCloud, (long)50000 );
        public static readonly Geology Gas_Clds_Green_Storm = new Geology( "Gas_Clds_Green_Storm", 1400702, GeologyType.LagrangeCloud, (long)50000 );
        public static readonly Geology Gas_Clds_Light = new Geology( "Gas_Clds_Light", 1401300, GeologyType.LagrangeCloud, (long)50000 );
        public static readonly Geology Gas_Clds_Orange = new Geology( "Gas_Clds_Orange", 1400801, GeologyType.LagrangeCloud, (long)50000 );
        public static readonly Geology Gas_Clds_Orange_Storm = new Geology( "Gas_Clds_Orange_Storm", 1400802, GeologyType.LagrangeCloud, (long)50000 );
        public static readonly Geology Gas_Clds_Pink = new Geology( "Gas_Clds_Pink", 1400901, GeologyType.LagrangeCloud, (long)50000 );
        public static readonly Geology Gas_Clds_Pink_Storm = new Geology( "Gas_Clds_Pink_Storm", 1400902, GeologyType.LagrangeCloud, (long)50000 );
        public static readonly Geology Gas_Clds_Red = new Geology( "Gas_Clds_Red", 1401001, GeologyType.LagrangeCloud, (long)50000 );
        public static readonly Geology Gas_Clds_Red_Storm = new Geology( "Gas_Clds_Red_Storm", 1401002, GeologyType.LagrangeCloud, (long)50000 );
        public static readonly Geology Gas_Clds_Yellow = new Geology( "Gas_Clds_Yellow", 1401101, GeologyType.LagrangeCloud, (long)50000 );
        public static readonly Geology Gas_Clds_Yellow_Storm = new Geology( "Gas_Clds_Yellow_Storm", 1401102, GeologyType.LagrangeCloud, (long)50000 );
        public static readonly Geology Gas_Vents_CarbonDioxideGeysers = new Geology( "Gas_Vents_CarbonDioxideGeysers", 1400409, GeologyType.GasVent, (long)50000 );
        public static readonly Geology Gas_Vents_SilicateVapourGeysers = new Geology( "Gas_Vents_SilicateVapourGeysers", 1400414, GeologyType.GasVent, (long)50000 );
        public static readonly Geology Gas_Vents_SulphurDioxideMagma = new Geology( "Gas_Vents_SulphurDioxideMagma", 1400402, GeologyType.GasVent, (long)50000 );
        public static readonly Geology Gas_Vents_WaterGeysers = new Geology( "Gas_Vents_WaterGeysers", 1400408, GeologyType.GasVent, (long)50000 );
        public static readonly Geology Geysers_WaterGeysers = new Geology( "Geysers_WaterGeysers", 1400208, GeologyType.WaterGeyser, (long)50000 );
        public static readonly Geology IceFumarole_AmmoniaGeysers = new Geology( "IceFumarole_AmmoniaGeysers", 1400160, GeologyType.IceFumarole, (long)50000 );
        public static readonly Geology IceFumarole_CarbonDioxideGeysers = new Geology( "IceFumarole_CarbonDioxideGeysers", 1400159, GeologyType.IceFumarole, (long)50000 );
        public static readonly Geology IceFumarole_MethaneGeysers = new Geology( "IceFumarole_MethaneGeysers", 1400161, GeologyType.IceFumarole, (long)50000 );
        public static readonly Geology IceFumarole_NitrogenGeysers = new Geology( "IceFumarole_NitrogenGeysers", 1400162, GeologyType.IceFumarole, (long)50000 );
        public static readonly Geology IceFumarole_SilicateVapourGeysers = new Geology( "IceFumarole_SilicateVapourGeysers", 1400164, GeologyType.IceFumarole, (long)50000 );
        public static readonly Geology IceFumarole_SulphurDioxideMagma = new Geology( "IceFumarole_SulphurDioxideMagma", 1400152, GeologyType.IceFumarole, (long)50000 );
        public static readonly Geology IceFumarole_WaterGeysers = new Geology( "IceFumarole_WaterGeysers", 1400158, GeologyType.IceFumarole, (long)50000 );
        public static readonly Geology IceGeysers_AmmoniaGeysers = new Geology( "IceGeysers_AmmoniaGeysers", 1400260, GeologyType.IceGeyser, (long)50000 );
        public static readonly Geology IceGeysers_CarbonDioxideGeysers = new Geology( "IceGeysers_CarbonDioxideGeysers", 1400259, GeologyType.IceGeyser, (long)50000 );
        public static readonly Geology IceGeysers_MethaneGeysers = new Geology( "IceGeysers_MethaneGeysers", 1400261, GeologyType.IceGeyser, (long)50000 );
        public static readonly Geology IceGeysers_NitrogenGeysers = new Geology( "IceGeysers_NitrogenGeysers", 1400262, GeologyType.IceGeyser, (long)50000 );
        public static readonly Geology IceGeysers_WaterGeysers = new Geology( "IceGeysers_WaterGeysers", 1400258, GeologyType.IceGeyser, (long)50000 );
        public static readonly Geology L_Phn_Part_Cld_001 = new Geology( "L_Phn_Part_Cld_001", 2401001, GeologyType.K_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Cld_002 = new Geology( "L_Phn_Part_Cld_002", 2401002, GeologyType.K_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Cld_003 = new Geology( "L_Phn_Part_Cld_003", 2401003, GeologyType.K_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Cld_004 = new Geology( "L_Phn_Part_Cld_004", 2401004, GeologyType.K_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Cld_005 = new Geology( "L_Phn_Part_Cld_005", 2401005, GeologyType.K_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Cld_006 = new Geology( "L_Phn_Part_Cld_006", 2401006, GeologyType.K_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Cld_007 = new Geology( "L_Phn_Part_Cld_007", 2401007, GeologyType.E_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Cld_008 = new Geology( "L_Phn_Part_Cld_008", 2401008, GeologyType.K_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Cld_009 = new Geology( "L_Phn_Part_Cld_009", 2401009, GeologyType.K_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Cld_010 = new Geology( "L_Phn_Part_Cld_010", 2401010, GeologyType.K_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Cld_011 = new Geology( "L_Phn_Part_Cld_011", 2401011, GeologyType.K_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Cld_012 = new Geology( "L_Phn_Part_Cld_012", 2401012, GeologyType.K_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Cld_013 = new Geology( "L_Phn_Part_Cld_013", 2401013, GeologyType.E_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Cld_014 = new Geology( "L_Phn_Part_Cld_014", 2401014, GeologyType.E_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Cld_015 = new Geology( "L_Phn_Part_Cld_015", 2401015, GeologyType.E_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Cld_016 = new Geology( "L_Phn_Part_Cld_016", 2401016, GeologyType.K_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Cld_017 = new Geology( "L_Phn_Part_Cld_017", 2401017, GeologyType.K_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Clus_001 = new Geology( "L_Phn_Part_Clus_001", 2402001, GeologyType.T_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Clus_002 = new Geology( "L_Phn_Part_Clus_002", 2402002, GeologyType.T_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Clus_003 = new Geology( "L_Phn_Part_Clus_003", 2402003, GeologyType.L_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Clus_004 = new Geology( "L_Phn_Part_Clus_004", 2402004, GeologyType.T_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Clus_005 = new Geology( "L_Phn_Part_Clus_005", 2402005, GeologyType.T_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Clus_007 = new Geology( "L_Phn_Part_Clus_007", 2402007, GeologyType.L_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Clus_008 = new Geology( "L_Phn_Part_Clus_008", 2402008, GeologyType.L_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Clus_009 = new Geology( "L_Phn_Part_Clus_009", 24020009, GeologyType.L_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Clus_010 = new Geology( "L_Phn_Part_Clus_010", 24020010, GeologyType.L_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Clus_011 = new Geology( "L_Phn_Part_Clus_011", 2402011, GeologyType.L_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Clus_012 = new Geology( "L_Phn_Part_Clus_012", 2402012, GeologyType.L_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Clus_013 = new Geology( "L_Phn_Part_Clus_013", 24020013, GeologyType.L_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Eng_002 = new Geology( "L_Phn_Part_Eng_002", 2403002, GeologyType.P_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Eng_003 = new Geology( "L_Phn_Part_Eng_003", 2403003, GeologyType.P_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Eng_004 = new Geology( "L_Phn_Part_Eng_004", 2403004, GeologyType.P_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Eng_005 = new Geology( "L_Phn_Part_Eng_005", 2403005, GeologyType.P_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Eng_006 = new Geology( "L_Phn_Part_Eng_006", 2403006, GeologyType.P_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Eng_007 = new Geology( "L_Phn_Part_Eng_007", 2403007, GeologyType.P_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Eng_008 = new Geology( "L_Phn_Part_Eng_008", 2403008, GeologyType.P_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Eng_009 = new Geology( "L_Phn_Part_Eng_009", 2403009, GeologyType.P_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Eng_010 = new Geology( "L_Phn_Part_Eng_010", 2403010, GeologyType.P_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Eng_011 = new Geology( "L_Phn_Part_Eng_011", 2403011, GeologyType.P_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Eng_012 = new Geology( "L_Phn_Part_Eng_012", 2403012, GeologyType.P_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Eng_013 = new Geology( "L_Phn_Part_Eng_013", 2403013, GeologyType.P_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Eng_014 = new Geology( "L_Phn_Part_Eng_014", 2403014, GeologyType.P_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Eng_015 = new Geology( "L_Phn_Part_Eng_015", 2403015, GeologyType.P_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Eng_016 = new Geology( "L_Phn_Part_Eng_016", 2403016, GeologyType.P_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Orb_001 = new Geology( "L_Phn_Part_Orb_001", 2406001, GeologyType.Q_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Orb_002 = new Geology( "L_Phn_Part_Orb_002", 2406002, GeologyType.Q_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Orb_003 = new Geology( "L_Phn_Part_Orb_003", 2406003, GeologyType.Q_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Orb_004 = new Geology( "L_Phn_Part_Orb_004", 2406004, GeologyType.Q_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Orb_005 = new Geology( "L_Phn_Part_Orb_005", 2406005, GeologyType.Q_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Orb_006 = new Geology( "L_Phn_Part_Orb_006", 2406006, GeologyType.Q_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Orb_007 = new Geology( "L_Phn_Part_Orb_007", 2406007, GeologyType.Q_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Orb_008 = new Geology( "L_Phn_Part_Orb_008", 2406008, GeologyType.Q_TypeAnomaly, (long)50000 );
        public static readonly Geology L_Phn_Part_Orb_009 = new Geology( "L_Phn_Part_Orb_009", 2406009, GeologyType.Q_TypeAnomaly, (long)50000 );
        public static readonly Geology Lava_Spouts_IronMagma = new Geology( "Lava_Spouts_IronMagma", 1400307, GeologyType.LavaSpout, (long)50000 );
        public static readonly Geology Lava_Spouts_SilicateMagma = new Geology( "Lava_Spouts_SilicateMagma", 1400306, GeologyType.LavaSpout, (long)50000 );

        public long entryID;
        public bool exists;                 // This item exists and has been populated with information
        public GeologyType type;
        public long value;
        public string description;

        // dummy used to ensure that the static constructor has run
        public Geology () : this( "" )
        { }

        private Geology ( string edname ) : base( edname, edname )
        { }

        private Geology ( string edname, long entryID, GeologyType type, long value ) : base( edname, edname )
        {
            this.entryID = entryID;
            this.type = type;
            this.value = value;
            this.description = Properties.GeologyDesc.ResourceManager.GetString(edname);
        }

        /// <summary>
        /// Try getting data from the entryid first, then use edname as a fallback
        /// </summary>
        public static Geology Lookup ( long? entryID, string edname )
        {
            try
            {
                if ( entryID != null )
                {
                    return AllOfThem.Single( a => a.entryID == entryID );
                }
            }
            catch ( InvalidOperationException e )
            {
                if ( AllOfThem.Count( a => a.entryID == entryID ) > 1 )
                {
                    Logging.Error( $"Duplicate EntryID value {entryID} in {nameof( Geology )}.", e );
                }
                else if ( AllOfThem.All( a => a.entryID != entryID ) )
                {
                    Logging.Error( $"Unknown EntryID value {entryID} with edname {edname} in {nameof( Geology )}.", e );
                }
            }

            return FromEDName( edname ) ?? new Geology( edname ); // No match.
        }
    }
}

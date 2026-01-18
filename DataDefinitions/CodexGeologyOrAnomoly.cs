using System;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class CodexGeologyOrAnomoly : ResourceBasedLocalizedEDName<CodexGeologyOrAnomoly>
    {
        static CodexGeologyOrAnomoly ()
        {
            resourceManager = Properties.CodexGeologyOrAnomoly.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new CodexGeologyOrAnomoly( edname );
        }

        public static readonly CodexGeologyOrAnomoly Fumarole_CarbonDioxideGeysers = new CodexGeologyOrAnomoly( "Fumarole_CarbonDioxideGeysers", 1400109, CodexGeologyOrAnomolyType.Fumarole );
        public static readonly CodexGeologyOrAnomoly Fumarole_SilicateVapourGeysers = new CodexGeologyOrAnomoly( "Fumarole_SilicateVapourGeysers", 1400114, CodexGeologyOrAnomolyType.Fumarole );
        public static readonly CodexGeologyOrAnomoly Fumarole_SulphurDioxideMagma = new CodexGeologyOrAnomoly( "Fumarole_SulphurDioxideMagma", 1400102, CodexGeologyOrAnomolyType.Fumarole );
        public static readonly CodexGeologyOrAnomoly Fumarole_WaterGeysers = new CodexGeologyOrAnomoly( "Fumarole_WaterGeysers", 1400108, CodexGeologyOrAnomolyType.Fumarole );
        public static readonly CodexGeologyOrAnomoly Gas_Clds_Blue = new CodexGeologyOrAnomoly( "Gas_Clds_Blue", 1400601, CodexGeologyOrAnomolyType.LagrangeCloud );
        public static readonly CodexGeologyOrAnomoly Gas_Clds_Green = new CodexGeologyOrAnomoly( "Gas_Clds_Green", 1400701, CodexGeologyOrAnomolyType.LagrangeCloud );
        public static readonly CodexGeologyOrAnomoly Gas_Clds_Green_Storm = new CodexGeologyOrAnomoly( "Gas_Clds_Green_Storm", 1400702, CodexGeologyOrAnomolyType.LagrangeCloud );
        public static readonly CodexGeologyOrAnomoly Gas_Clds_Light = new CodexGeologyOrAnomoly( "Gas_Clds_Light", 1401300, CodexGeologyOrAnomolyType.LagrangeCloud );
        public static readonly CodexGeologyOrAnomoly Gas_Clds_Orange = new CodexGeologyOrAnomoly( "Gas_Clds_Orange", 1400801, CodexGeologyOrAnomolyType.LagrangeCloud );
        public static readonly CodexGeologyOrAnomoly Gas_Clds_Orange_Storm = new CodexGeologyOrAnomoly( "Gas_Clds_Orange_Storm", 1400802, CodexGeologyOrAnomolyType.LagrangeCloud );
        public static readonly CodexGeologyOrAnomoly Gas_Clds_Pink = new CodexGeologyOrAnomoly( "Gas_Clds_Pink", 1400901, CodexGeologyOrAnomolyType.LagrangeCloud );
        public static readonly CodexGeologyOrAnomoly Gas_Clds_Pink_Storm = new CodexGeologyOrAnomoly( "Gas_Clds_Pink_Storm", 1400902, CodexGeologyOrAnomolyType.LagrangeCloud );
        public static readonly CodexGeologyOrAnomoly Gas_Clds_Red = new CodexGeologyOrAnomoly( "Gas_Clds_Red", 1401001, CodexGeologyOrAnomolyType.LagrangeCloud );
        public static readonly CodexGeologyOrAnomoly Gas_Clds_Red_Storm = new CodexGeologyOrAnomoly( "Gas_Clds_Red_Storm", 1401002, CodexGeologyOrAnomolyType.LagrangeCloud );
        public static readonly CodexGeologyOrAnomoly Gas_Clds_Yellow = new CodexGeologyOrAnomoly( "Gas_Clds_Yellow", 1401101, CodexGeologyOrAnomolyType.LagrangeCloud );
        public static readonly CodexGeologyOrAnomoly Gas_Clds_Yellow_Storm = new CodexGeologyOrAnomoly( "Gas_Clds_Yellow_Storm", 1401102, CodexGeologyOrAnomolyType.LagrangeCloud );
        public static readonly CodexGeologyOrAnomoly Gas_Vents_CarbonDioxideGeysers = new CodexGeologyOrAnomoly( "Gas_Vents_CarbonDioxideGeysers", 1400409, CodexGeologyOrAnomolyType.GasVent );
        public static readonly CodexGeologyOrAnomoly Gas_Vents_SilicateVapourGeysers = new CodexGeologyOrAnomoly( "Gas_Vents_SilicateVapourGeysers", 1400414, CodexGeologyOrAnomolyType.GasVent );
        public static readonly CodexGeologyOrAnomoly Gas_Vents_SulphurDioxideMagma = new CodexGeologyOrAnomoly( "Gas_Vents_SulphurDioxideMagma", 1400402, CodexGeologyOrAnomolyType.GasVent );
        public static readonly CodexGeologyOrAnomoly Gas_Vents_WaterGeysers = new CodexGeologyOrAnomoly( "Gas_Vents_WaterGeysers", 1400408, CodexGeologyOrAnomolyType.GasVent );
        public static readonly CodexGeologyOrAnomoly Geysers_WaterGeysers = new CodexGeologyOrAnomoly( "Geysers_WaterGeysers", 1400208, CodexGeologyOrAnomolyType.WaterGeyser );
        public static readonly CodexGeologyOrAnomoly IceFumarole_AmmoniaGeysers = new CodexGeologyOrAnomoly( "IceFumarole_AmmoniaGeysers", 1400160, CodexGeologyOrAnomolyType.IceFumarole );
        public static readonly CodexGeologyOrAnomoly IceFumarole_CarbonDioxideGeysers = new CodexGeologyOrAnomoly( "IceFumarole_CarbonDioxideGeysers", 1400159, CodexGeologyOrAnomolyType.IceFumarole );
        public static readonly CodexGeologyOrAnomoly IceFumarole_MethaneGeysers = new CodexGeologyOrAnomoly( "IceFumarole_MethaneGeysers", 1400161, CodexGeologyOrAnomolyType.IceFumarole );
        public static readonly CodexGeologyOrAnomoly IceFumarole_NitrogenGeysers = new CodexGeologyOrAnomoly( "IceFumarole_NitrogenGeysers", 1400162, CodexGeologyOrAnomolyType.IceFumarole );
        public static readonly CodexGeologyOrAnomoly IceFumarole_SilicateVapourGeysers = new CodexGeologyOrAnomoly( "IceFumarole_SilicateVapourGeysers", 1400164, CodexGeologyOrAnomolyType.IceFumarole );
        public static readonly CodexGeologyOrAnomoly IceFumarole_SulphurDioxideMagma = new CodexGeologyOrAnomoly( "IceFumarole_SulphurDioxideMagma", 1400152, CodexGeologyOrAnomolyType.IceFumarole );
        public static readonly CodexGeologyOrAnomoly IceFumarole_WaterGeysers = new CodexGeologyOrAnomoly( "IceFumarole_WaterGeysers", 1400158, CodexGeologyOrAnomolyType.IceFumarole );
        public static readonly CodexGeologyOrAnomoly IceGeysers_AmmoniaGeysers = new CodexGeologyOrAnomoly( "IceGeysers_AmmoniaGeysers", 1400260, CodexGeologyOrAnomolyType.IceGeyser );
        public static readonly CodexGeologyOrAnomoly IceGeysers_CarbonDioxideGeysers = new CodexGeologyOrAnomoly( "IceGeysers_CarbonDioxideGeysers", 1400259, CodexGeologyOrAnomolyType.IceGeyser );
        public static readonly CodexGeologyOrAnomoly IceGeysers_MethaneGeysers = new CodexGeologyOrAnomoly( "IceGeysers_MethaneGeysers", 1400261, CodexGeologyOrAnomolyType.IceGeyser );
        public static readonly CodexGeologyOrAnomoly IceGeysers_NitrogenGeysers = new CodexGeologyOrAnomoly( "IceGeysers_NitrogenGeysers", 1400262, CodexGeologyOrAnomolyType.IceGeyser );
        public static readonly CodexGeologyOrAnomoly IceGeysers_WaterGeysers = new CodexGeologyOrAnomoly( "IceGeysers_WaterGeysers", 1400258, CodexGeologyOrAnomolyType.IceGeyser );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Cld_001 = new CodexGeologyOrAnomoly( "L_Phn_Part_Cld_001", 2401001, CodexGeologyOrAnomolyType.K_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Cld_002 = new CodexGeologyOrAnomoly( "L_Phn_Part_Cld_002", 2401002, CodexGeologyOrAnomolyType.K_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Cld_003 = new CodexGeologyOrAnomoly( "L_Phn_Part_Cld_003", 2401003, CodexGeologyOrAnomolyType.K_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Cld_004 = new CodexGeologyOrAnomoly( "L_Phn_Part_Cld_004", 2401004, CodexGeologyOrAnomolyType.K_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Cld_005 = new CodexGeologyOrAnomoly( "L_Phn_Part_Cld_005", 2401005, CodexGeologyOrAnomolyType.K_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Cld_006 = new CodexGeologyOrAnomoly( "L_Phn_Part_Cld_006", 2401006, CodexGeologyOrAnomolyType.K_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Cld_007 = new CodexGeologyOrAnomoly( "L_Phn_Part_Cld_007", 2401007, CodexGeologyOrAnomolyType.E_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Cld_008 = new CodexGeologyOrAnomoly( "L_Phn_Part_Cld_008", 2401008, CodexGeologyOrAnomolyType.K_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Cld_009 = new CodexGeologyOrAnomoly( "L_Phn_Part_Cld_009", 2401009, CodexGeologyOrAnomolyType.K_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Cld_010 = new CodexGeologyOrAnomoly( "L_Phn_Part_Cld_010", 2401010, CodexGeologyOrAnomolyType.K_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Cld_011 = new CodexGeologyOrAnomoly( "L_Phn_Part_Cld_011", 2401011, CodexGeologyOrAnomolyType.K_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Cld_012 = new CodexGeologyOrAnomoly( "L_Phn_Part_Cld_012", 2401012, CodexGeologyOrAnomolyType.K_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Cld_013 = new CodexGeologyOrAnomoly( "L_Phn_Part_Cld_013", 2401013, CodexGeologyOrAnomolyType.E_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Cld_014 = new CodexGeologyOrAnomoly( "L_Phn_Part_Cld_014", 2401014, CodexGeologyOrAnomolyType.E_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Cld_015 = new CodexGeologyOrAnomoly( "L_Phn_Part_Cld_015", 2401015, CodexGeologyOrAnomolyType.E_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Cld_016 = new CodexGeologyOrAnomoly( "L_Phn_Part_Cld_016", 2401016, CodexGeologyOrAnomolyType.K_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Cld_017 = new CodexGeologyOrAnomoly( "L_Phn_Part_Cld_017", 2401017, CodexGeologyOrAnomolyType.K_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Clus_001 = new CodexGeologyOrAnomoly( "L_Phn_Part_Clus_001", 2402001, CodexGeologyOrAnomolyType.T_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Clus_002 = new CodexGeologyOrAnomoly( "L_Phn_Part_Clus_002", 2402002, CodexGeologyOrAnomolyType.T_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Clus_003 = new CodexGeologyOrAnomoly( "L_Phn_Part_Clus_003", 2402003, CodexGeologyOrAnomolyType.L_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Clus_004 = new CodexGeologyOrAnomoly( "L_Phn_Part_Clus_004", 2402004, CodexGeologyOrAnomolyType.T_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Clus_005 = new CodexGeologyOrAnomoly( "L_Phn_Part_Clus_005", 2402005, CodexGeologyOrAnomolyType.T_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Clus_007 = new CodexGeologyOrAnomoly( "L_Phn_Part_Clus_007", 2402007, CodexGeologyOrAnomolyType.L_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Clus_008 = new CodexGeologyOrAnomoly( "L_Phn_Part_Clus_008", 2402008, CodexGeologyOrAnomolyType.L_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Clus_009 = new CodexGeologyOrAnomoly( "L_Phn_Part_Clus_009", 24020009, CodexGeologyOrAnomolyType.L_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Clus_010 = new CodexGeologyOrAnomoly( "L_Phn_Part_Clus_010", 24020010, CodexGeologyOrAnomolyType.L_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Clus_011 = new CodexGeologyOrAnomoly( "L_Phn_Part_Clus_011", 2402011, CodexGeologyOrAnomolyType.L_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Clus_012 = new CodexGeologyOrAnomoly( "L_Phn_Part_Clus_012", 2402012, CodexGeologyOrAnomolyType.L_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Clus_013 = new CodexGeologyOrAnomoly( "L_Phn_Part_Clus_013", 24020013, CodexGeologyOrAnomolyType.L_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Eng_002 = new CodexGeologyOrAnomoly( "L_Phn_Part_Eng_002", 2403002, CodexGeologyOrAnomolyType.P_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Eng_003 = new CodexGeologyOrAnomoly( "L_Phn_Part_Eng_003", 2403003, CodexGeologyOrAnomolyType.P_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Eng_004 = new CodexGeologyOrAnomoly( "L_Phn_Part_Eng_004", 2403004, CodexGeologyOrAnomolyType.P_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Eng_005 = new CodexGeologyOrAnomoly( "L_Phn_Part_Eng_005", 2403005, CodexGeologyOrAnomolyType.P_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Eng_006 = new CodexGeologyOrAnomoly( "L_Phn_Part_Eng_006", 2403006, CodexGeologyOrAnomolyType.P_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Eng_007 = new CodexGeologyOrAnomoly( "L_Phn_Part_Eng_007", 2403007, CodexGeologyOrAnomolyType.P_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Eng_008 = new CodexGeologyOrAnomoly( "L_Phn_Part_Eng_008", 2403008, CodexGeologyOrAnomolyType.P_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Eng_009 = new CodexGeologyOrAnomoly( "L_Phn_Part_Eng_009", 2403009, CodexGeologyOrAnomolyType.P_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Eng_010 = new CodexGeologyOrAnomoly( "L_Phn_Part_Eng_010", 2403010, CodexGeologyOrAnomolyType.P_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Eng_011 = new CodexGeologyOrAnomoly( "L_Phn_Part_Eng_011", 2403011, CodexGeologyOrAnomolyType.P_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Eng_012 = new CodexGeologyOrAnomoly( "L_Phn_Part_Eng_012", 2403012, CodexGeologyOrAnomolyType.P_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Eng_013 = new CodexGeologyOrAnomoly( "L_Phn_Part_Eng_013", 2403013, CodexGeologyOrAnomolyType.P_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Eng_014 = new CodexGeologyOrAnomoly( "L_Phn_Part_Eng_014", 2403014, CodexGeologyOrAnomolyType.P_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Eng_015 = new CodexGeologyOrAnomoly( "L_Phn_Part_Eng_015", 2403015, CodexGeologyOrAnomolyType.P_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Eng_016 = new CodexGeologyOrAnomoly( "L_Phn_Part_Eng_016", 2403016, CodexGeologyOrAnomolyType.P_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Orb_001 = new CodexGeologyOrAnomoly( "L_Phn_Part_Orb_001", 2406001, CodexGeologyOrAnomolyType.Q_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Orb_002 = new CodexGeologyOrAnomoly( "L_Phn_Part_Orb_002", 2406002, CodexGeologyOrAnomolyType.Q_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Orb_003 = new CodexGeologyOrAnomoly( "L_Phn_Part_Orb_003", 2406003, CodexGeologyOrAnomolyType.Q_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Orb_004 = new CodexGeologyOrAnomoly( "L_Phn_Part_Orb_004", 2406004, CodexGeologyOrAnomolyType.Q_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Orb_005 = new CodexGeologyOrAnomoly( "L_Phn_Part_Orb_005", 2406005, CodexGeologyOrAnomolyType.Q_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Orb_006 = new CodexGeologyOrAnomoly( "L_Phn_Part_Orb_006", 2406006, CodexGeologyOrAnomolyType.Q_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Orb_007 = new CodexGeologyOrAnomoly( "L_Phn_Part_Orb_007", 2406007, CodexGeologyOrAnomolyType.Q_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Orb_008 = new CodexGeologyOrAnomoly( "L_Phn_Part_Orb_008", 2406008, CodexGeologyOrAnomolyType.Q_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly L_Phn_Part_Orb_009 = new CodexGeologyOrAnomoly( "L_Phn_Part_Orb_009", 2406009, CodexGeologyOrAnomolyType.Q_TypeAnomaly );
        public static readonly CodexGeologyOrAnomoly Lava_Spouts_IronMagma = new CodexGeologyOrAnomoly( "Lava_Spouts_IronMagma", 1400307, CodexGeologyOrAnomolyType.LavaSpout );
        public static readonly CodexGeologyOrAnomoly Lava_Spouts_SilicateMagma = new CodexGeologyOrAnomoly( "Lava_Spouts_SilicateMagma", 1400306, CodexGeologyOrAnomolyType.LavaSpout );

        public long entryID;

        [PublicAPI]
        public CodexGeologyOrAnomolyType type;

        [PublicAPI]
        public string localizedDescription => Properties.CodexGeologyOrAnomolyDesc.ResourceManager.GetString( edname );

        // dummy used to ensure that the static constructor has run
        public CodexGeologyOrAnomoly () : this( "" )
        { }

        private CodexGeologyOrAnomoly ( string edname ) : base( edname, edname )
        { }

        private CodexGeologyOrAnomoly ( string edname, long entryID, CodexGeologyOrAnomolyType type ) : base( edname, edname )
        {
            this.entryID = entryID;
            this.type = type;
        }

        /// <summary>
        /// Try getting data from the entryid first, then use edname as a fallback
        /// </summary>
        public static CodexGeologyOrAnomoly Lookup ( long? entryID, string edname )
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
                    Logging.Error( $"Duplicate EntryID value {entryID} in {nameof( CodexGeologyOrAnomoly )}.", e );
                }
                else if ( AllOfThem.All( a => a.entryID != entryID ) )
                {
                    Logging.Error( $"Unknown EntryID value {entryID} with edname {edname} in {nameof( CodexGeologyOrAnomoly )}.", e );
                }
            }

            return FromEDName( edname ) ?? new CodexGeologyOrAnomoly( edname ); // No match.
        }
    }
}
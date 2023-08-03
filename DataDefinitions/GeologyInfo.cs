using JetBrains.Annotations;
using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Collections.Generic;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Net.PeerToPeer;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Threading;
using Utilities;
using Rollbar.Common;

namespace EddiDataDefinitions
{
    public class GeologyClass
    {
        public string name;
        public string description;

        public GeologyClass ()
        {
            this.name = "";
            this.description = "";
        }

        public GeologyClass ( string genus, string desc )
        {
            this.name = genus;
            this.description = desc;
        }
    };

    public class GeologyObject
    {
        public string name;
        public long? value;
        public string description;

        public GeologyObject ()
        {
            this.name = "";
            this.value = 0;
            this.description = "";
        }

        public GeologyObject ( string genus, long? value, string desc )
        {
            this.name = genus;
            this.value = value;
            this.description = desc;
        }
    };

    public class GeologyItem
    {
        // TODO:#2212........[Simplify this class so it's easier to use in scripts]

        public bool exists;   // This item exists and has been populated with information
        public GeologyClass geoClass;
        public GeologyObject geoObject;

        public GeologyItem ()
        {
            exists = false;
            geoClass = new GeologyClass();
            geoObject = new GeologyObject();
        }

        public bool Exists() {
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

        public class LookupGeoName
        {
            public long? entryId;
            public string geoClass;
            public long? value;

            public LookupGeoName ( long? entryId, string geoClass, long? value )
            {
                this.entryId = entryId;
                this.geoClass = geoClass;
                this.value = value;
            }
        }

        // For easier reverse lookups
        public static Dictionary<string, LookupGeoName> GeologyData = new Dictionary<string, LookupGeoName>();

        static GeologyInfo ()
        {
            GeologyData.Add( "L_Phn_Part_Cld_013", new LookupGeoName( (long?)2320103, "E_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Cld_014", new LookupGeoName( (long?)2320111, "E_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Cld_015", new LookupGeoName( (long?)2320102, "E_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Cld_007", new LookupGeoName( (long?)2310506, "E_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "Fumarole_SulphurDioxideMagma", new LookupGeoName( (long?)2310111, "Fumarole", (long?)50000 ) );
            GeologyData.Add( "Fumarole_WaterGeysers", new LookupGeoName( (long?)2310202, "Fumarole", (long?)50000 ) );
            GeologyData.Add( "Fumarole_SilicateVapourGeysers", new LookupGeoName( (long?)2310108, "Fumarole", (long?)50000 ) );
            GeologyData.Add( "Fumarole_CarbonDioxideGeysers", new LookupGeoName( (long?)2310107, "Fumarole", (long?)50000 ) );
            GeologyData.Add( "Gas_Vents_SulphurDioxideMagma", new LookupGeoName( (long?)2310304, "GasVent", (long?)50000 ) );
            GeologyData.Add( "Gas_Vents_WaterGeysers", new LookupGeoName( (long?)2310306, "GasVent", (long?)50000 ) );
            GeologyData.Add( "Gas_Vents_CarbonDioxideGeysers", new LookupGeoName( (long?)2310312, "GasVent", (long?)50000 ) );
            GeologyData.Add( "Gas_Vents_SilicateVapourGeysers", new LookupGeoName( (long?)2310303, "GasVent", (long?)50000 ) );
            GeologyData.Add( "IceFumarole_AmmoniaGeysers", new LookupGeoName( (long?)2310313, "IceFumarole", (long?)50000 ) );
            GeologyData.Add( "IceFumarole_SulphurDioxideMagma", new LookupGeoName( (long?)2310403, "IceFumarole", (long?)50000 ) );
            GeologyData.Add( "IceFumarole_WaterGeysers", new LookupGeoName( (long?)2310404, "IceFumarole", (long?)50000 ) );
            GeologyData.Add( "IceFumarole_CarbonDioxideGeysers", new LookupGeoName( (long?)2310307, "IceFumarole", (long?)50000 ) );
            GeologyData.Add( "IceFumarole_MethaneGeysers", new LookupGeoName( (long?)2310308, "IceFumarole", (long?)50000 ) );
            GeologyData.Add( "IceFumarole_NitrogenGeysers", new LookupGeoName( (long?)2310402, "IceFumarole", (long?)50000 ) );
            GeologyData.Add( "IceFumarole_SilicateVapourGeysers", new LookupGeoName( (long?)2310401, "IceFumarole", (long?)50000 ) );
            GeologyData.Add( "IceGeysers_AmmoniaGeysers", new LookupGeoName( (long?)2310406, "IceGeyser", (long?)50000 ) );
            GeologyData.Add( "IceGeysers_WaterGeysers", new LookupGeoName( (long?)2310408, "IceGeyser", (long?)50000 ) );
            GeologyData.Add( "IceGeysers_CarbonDioxideGeysers", new LookupGeoName( (long?)2310405, "IceGeyser", (long?)50000 ) );
            GeologyData.Add( "IceGeysers_MethaneGeysers", new LookupGeoName( (long?)2310413, "IceGeyser", (long?)50000 ) );
            GeologyData.Add( "IceGeysers_NitrogenGeysers", new LookupGeoName( (long?)2310407, "IceGeyser", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Cld_001", new LookupGeoName( (long?)2310411, "K_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Cld_002", new LookupGeoName( (long?)2310410, "K_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Cld_003", new LookupGeoName( (long?)2310502, "K_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Cld_004", new LookupGeoName( (long?)2310501, "K_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Cld_005", new LookupGeoName( (long?)2310503, "K_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Cld_006", new LookupGeoName( (long?)2310504, "K_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Cld_008", new LookupGeoName( (long?)2310505, "K_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Cld_009", new LookupGeoName( (long?)2310513, "K_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Cld_010", new LookupGeoName( (long?)2310507, "K_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Cld_011", new LookupGeoName( (long?)2310508, "K_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Cld_012", new LookupGeoName( (long?)2310510, "K_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Cld_016", new LookupGeoName( (long?)2320114, "K_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Cld_017", new LookupGeoName( (long?)2320104, "K_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Clus_003", new LookupGeoName( (long?)2320108, "L_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Clus_007", new LookupGeoName( (long?)2320101, "L_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Clus_008", new LookupGeoName( (long?)2320109, "L_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Clus_009", new LookupGeoName( (long?)2320110, "L_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Clus_010", new LookupGeoName( (long?)2320113, "L_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Clus_011", new LookupGeoName( (long?)2320112, "L_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Clus_012", new LookupGeoName( (long?)2320205, "L_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Clus_013", new LookupGeoName( (long?)2320203, "L_TypeAnomoly", (long?)50000 ) );
            GeologyData.Add( "Gas_Clds_Blue", new LookupGeoName( (long?)2310201, "LagrangeCloud", (long?)50000 ) );
            GeologyData.Add( "Gas_Clds_Green", new LookupGeoName( (long?)2310212, "LagrangeCloud", (long?)50000 ) );
            GeologyData.Add( "Gas_Clds_Orange", new LookupGeoName( (long?)2310206, "LagrangeCloud", (long?)50000 ) );
            GeologyData.Add( "Gas_Clds_Pink", new LookupGeoName( (long?)2310213, "LagrangeCloud", (long?)50000 ) );
            GeologyData.Add( "Gas_Clds_Red", new LookupGeoName( (long?)2310208, "LagrangeCloud", (long?)50000 ) );
            GeologyData.Add( "Gas_Clds_Yellow", new LookupGeoName( (long?)2310302, "LagrangeCloud", (long?)50000 ) );
            GeologyData.Add( "Gas_Clds_Light", new LookupGeoName( (long?)2310204, "LagrangeCloud", (long?)50000 ) );
            GeologyData.Add( "Lava_Spouts_SilicateMagma", new LookupGeoName( (long?)2320604, "LavaSpout", (long?)50000 ) );
            GeologyData.Add( "Lava_Spouts_IronMagma", new LookupGeoName( (long?)2320614, "LavaSpout", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Eng_002", new LookupGeoName( (long?)2320204, "P_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Eng_003", new LookupGeoName( (long?)2320201, "P_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Eng_004", new LookupGeoName( (long?)2320202, "P_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Eng_005", new LookupGeoName( (long?)2320206, "P_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Eng_006", new LookupGeoName( (long?)2320306, "P_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Eng_007", new LookupGeoName( (long?)2320301, "P_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Eng_008", new LookupGeoName( (long?)2320305, "P_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Eng_009", new LookupGeoName( (long?)2320302, "P_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Eng_010", new LookupGeoName( (long?)2320303, "P_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Eng_011", new LookupGeoName( (long?)2320304, "P_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Eng_012", new LookupGeoName( (long?)2320405, "P_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Eng_013", new LookupGeoName( (long?)2320403, "P_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Eng_014", new LookupGeoName( (long?)2320404, "P_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Eng_015", new LookupGeoName( (long?)2320401, "P_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Eng_016", new LookupGeoName( (long?)2320402, "P_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Orb_001", new LookupGeoName( (long?)2320406, "Q_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Orb_002", new LookupGeoName( (long?)2320505, "Q_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Orb_003", new LookupGeoName( (long?)2320503, "Q_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Orb_004", new LookupGeoName( (long?)2320504, "Q_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Orb_005", new LookupGeoName( (long?)2320501, "Q_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Orb_006", new LookupGeoName( (long?)2320502, "Q_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Orb_007", new LookupGeoName( (long?)2320506, "Q_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Orb_008", new LookupGeoName( (long?)2320603, "Q_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Orb_009", new LookupGeoName( (long?)2320602, "Q_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "Gas_Clds_Green_Storm", new LookupGeoName( (long?)2310203, "LagrangeCloud", (long?)50000 ) );
            GeologyData.Add( "Gas_Clds_Orange_Storm", new LookupGeoName( (long?)2310205, "LagrangeCloud", (long?)50000 ) );
            GeologyData.Add( "Gas_Clds_Pink_Storm", new LookupGeoName( (long?)2310207, "LagrangeCloud", (long?)50000 ) );
            GeologyData.Add( "Gas_Clds_Red_Storm", new LookupGeoName( (long?)2310210, "LagrangeCloud", (long?)50000 ) );
            GeologyData.Add( "Gas_Clds_Yellow_Storm", new LookupGeoName( (long?)2310301, "LagrangeCloud", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Clus_001", new LookupGeoName( (long?)2320105, "T_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Clus_002", new LookupGeoName( (long?)2320106, "T_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Clus_004", new LookupGeoName( (long?)2320107, "T_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "L_Phn_Part_Clus_005", new LookupGeoName( (long?)2320115, "T_TypeAnomaly", (long?)50000 ) );
            GeologyData.Add( "Geysers_WaterGeysers", new LookupGeoName( (long?)2310305, "WaterGeyser", (long?)50000 ) );


        }

        public static GeologyItem LookupByName ( string edname )
        {
            GeologyItem item = new GeologyItem();

            if ( edname != "" )
            {
                LookupGeoName data = GeologyData[ edname ];
                if ( data != null ) {
                    item.geoClass.name = rmGeoClassName.GetString( data.geoClass );
                    item.geoClass.description = rmGeoClassDesc.GetString( data.geoClass );

                    item.geoObject.name = rmGeoName.GetString( edname );
                    item.geoObject.value = data.value;
                    item.geoObject.description = rmGeoDesc.GetString( edname );

                    item.SetExists( true );
                }
            }

            return item;
        }
    }
}

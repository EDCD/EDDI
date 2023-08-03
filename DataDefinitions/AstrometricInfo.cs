using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Xml.Linq;
using Utilities;

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

        public static Dictionary<string, string> SubCategory = new Dictionary<string, string>();

        static AstrometricInfo ()
        {
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
            SubCategory.Add( "DCV", "Stars" );
            SubCategory.Add( "DDQ", "Stars" );
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

            return item;
        }
    }
}

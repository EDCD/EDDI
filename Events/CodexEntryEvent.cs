using EddiDataDefinitions;
using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CodexEntryEvent : Event
    {
        public const string NAME = "Codex entry obtained";
        public const string DESCRIPTION = "Triggered when a codex entry is obtained";
        public const string SAMPLE = @"{ ""timestamp"":""2023-07-22T04:10:26Z"", ""event"":""CodexEntry"", ""EntryID"":2440503, ""Name"":""$Codex_Ent_Shrubs_05_F_Name;"", ""Name_Localised"":""Frutexa Fera - Green"", ""SubCategory"":""$Codex_SubCategory_Organic_Structures;"", ""SubCategory_Localised"":""Organic structures"", ""Category"":""$Codex_Category_Biology;"", ""Category_Localised"":""Biological and Geological"", ""Region"":""$Codex_RegionName_5;"", ""Region_Localised"":""Norma Arm"", ""System"":""Greae Phio FO-G d11-1005"", ""SystemAddress"":34542299533283, ""BodyID"":42, ""Latitude"":-45.382187, ""Longitude"":173.182938 }";
    //public const string SAMPLE = @"{ ""timestamp"":""2023-07-22T05:30:39Z"", ""event"":""CodexEntry"", ""EntryID"":1100201, ""Name"":""$Codex_Ent_B_Type_Name;"", ""Name_Localised"":""B Types"", ""SubCategory"":""$Codex_SubCategory_Stars;"", ""SubCategory_Localised"":""Stars"", ""Category"":""$Codex_Category_StellarBodies;"", ""Category_Localised"":""Astronomical Bodies"", ""Region"":""$Codex_RegionName_5;"", ""Region_Localised"":""Norma Arm"", ""System"":""Greae Phio QE-Q e5-6040"", ""SystemAddress"":25942946711540, ""BodyID"":0, ""IsNewEntry"":true }";
    //public const string SAMPLE = @"{ ""timestamp"":""2023-07-23T05:43:52Z"", ""event"":""CodexEntry"", ""EntryID"":2100804, ""Name"":""$Codex_Ent_L_Cry_MetCry_Yw_Name;"", ""Name_Localised"":""Flavum Metallic Crystals"", ""SubCategory"":""$Codex_SubCategory_Organic_Structures;"", ""SubCategory_Localised"":""Organic structures"", ""Category"":""$Codex_Category_Biology;"", ""Category_Localised"":""Biological and Geological"", ""Region"":""$Codex_RegionName_5;"", ""Region_Localised"":""Norma Arm"", ""System"":""Greae Phio JU-E d12-299"", ""SystemAddress"":10284324245483, ""BodyID"":2, ""NearestDestination"":""$Fixed_Event_Life_Cloud;"", ""NearestDestination_Localised"":""Notable stellar phenomena"", ""VoucherAmount"":2500 }

    [PublicAPI( "The system the entry was discovered in." )]
        public string systemName { get; private set; }

        [PublicAPI( "The category of the entry." )]
        public string categoryName { get; private set; }

        [PublicAPI( "The subcategory of the entry." )]
        public string subCategoryName { get; private set; }

        [PublicAPI( "The name of the entry." )]
        public string entryName { get; private set; }

        [PublicAPI( "The name of the entry." )]
        public string codexName { get; private set; }

        [PublicAPI( "The region of the discovery." )]
        public string regionName { get; private set; }

        [PublicAPI( "Is this a new discovery?" )]
        public bool newEntry { get; private set; }

        [PublicAPI( "Is this a new trait?" )]
        public bool newTrait { get; private set; }

        [PublicAPI( "What is the codex entry worth?" )]
        public int voucherAmount { get; private set; }

        [PublicAPI("Get simple codex entry data")]
        public CodexEntry codexEntry { get; private set; }

        // Not intended to be user facing

        public CodexEntryEvent ( DateTime timestamp, long entryId, string codexName, string subCategoryName, string categoryName, string regionName, string systemName, bool newEntry, bool newTrait, int voucherAmount ) : base( timestamp, NAME )
        {
            if ( !fromLoad )
            {
                this.systemName = systemName;
                this.codexName = codexName;
                this.categoryName = categoryName;
                this.subCategoryName = subCategoryName;
                this.regionName = regionName;
                this.newEntry = newEntry;
                this.newTrait = newTrait;
                this.voucherAmount = voucherAmount;

                this.codexEntry = new CodexEntry( entryId, codexName, subCategoryName, categoryName, regionName, systemName );
            }
        }
    }
}

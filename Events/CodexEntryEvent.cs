using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CodexEntryEvent : Event
    {
        public const string NAME = "Codex entry obtained";
        public const string DESCRIPTION = "Triggered when a codex entry is obtained";
        public const string SAMPLE = @"{ ""timestamp"":""2023-07-22T04:10:26Z"", ""event"":""CodexEntry"", ""EntryID"":2440503, ""Name"":""$Codex_Ent_Shrubs_05_F_Name;"", ""Name_Localised"":""Frutexa Fera - Green"", ""SubCategory"":""$Codex_SubCategory_Organic_Structures;"", ""SubCategory_Localised"":""Organic structures"", ""Category"":""$Codex_Category_Biology;"", ""Category_Localised"":""Biological and Geological"", ""Region"":""$Codex_RegionName_5;"", ""Region_Localised"":""Norma Arm"", ""System"":""Greae Phio FO-G d11-1005"", ""SystemAddress"":34542299533283, ""BodyID"":42, ""Latitude"":-45.382187, ""Longitude"":173.182938 }";

        [ PublicAPI( "The system the entry was discovered in." ) ]
        public string systemName => codexEntry.systemName;

        [PublicAPI( "The category of the entry." )]
        public string categoryName => codexEntry.categoryName.Replace( "_", " " );

        [PublicAPI( "The subcategory of the entry." )]
        public string subCategoryName => codexEntry.subCategoryName.Replace( "_", " " );

        [PublicAPI( "The name of the entry." )]
        public string entryName => codexEntry.entryName.Replace( "_", " " );

        [PublicAPI( "The region of the discovery." )]
        public string localizedRegion => codexEntry.localizedRegion;

        [PublicAPI( "Is this a new discovery?" )]
        public bool newEntry { get; private set; }

        [PublicAPI( "Is this a new trait?" )]
        public bool newTrait { get; private set; }

        [PublicAPI( "What is the codex entry worth?" )]
        public int voucherAmount { get; private set; }

        [PublicAPI("Get simple codex entry data")]
        public CodexEntry codexEntry { get; private set; }

        // Not intended to be user facing

        public ulong systemAddress => codexEntry.systemAddress;
        
        public CodexEntryEvent ( DateTime timestamp, CodexEntry codexEntry, bool newEntry, bool newTrait, int voucherAmount ) : base( timestamp, NAME )
        {
            this.codexEntry = codexEntry;
            this.newEntry = newEntry;
            this.newTrait = newTrait;
            this.voucherAmount = voucherAmount;
        }
    }
}

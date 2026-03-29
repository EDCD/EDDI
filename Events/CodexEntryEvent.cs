using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CodexEntryEvent : Event
    {
        public const string NAME = "Codex entry obtained";
        public const string DESCRIPTION = "Triggered when a codex entry is obtained";
        public static readonly string[] SAMPLES =
        [
            @"{ ""timestamp"":""2023-07-22T04:10:26Z"", ""event"":""CodexEntry"", ""EntryID"":2440503, ""Name"":""$Codex_Ent_Shrubs_05_F_Name;"", ""Name_Localised"":""Frutexa Fera - Green"", ""SubCategory"":""$Codex_SubCategory_Organic_Structures;"", ""SubCategory_Localised"":""Organic structures"", ""Category"":""$Codex_Category_Biology;"", ""Category_Localised"":""Biological and Geological"", ""Region"":""$Codex_RegionName_5;"", ""Region_Localised"":""Norma Arm"", ""System"":""Greae Phio FO-G d11-1005"", ""SystemAddress"":34542299533283, ""BodyID"":42, ""Latitude"":-45.382187, ""Longitude"":173.182938, ""IsNewEntry"":true }",
            @"{ ""timestamp"":""2026-01-17T09:09:12Z"", ""event"":""CodexEntry"", ""EntryID"":2460101, ""Name"":""$Codex_Ent_Ingensradices_Unicus_Name;"", ""Name_Localised"":""Radicoida Unica"", ""SubCategory"":""$Codex_SubCategory_Organic_Structures;"", ""SubCategory_Localised"":""Organic structures"", ""Category"":""$Codex_Category_Biology;"", ""Category_Localised"":""Biological and Geological"", ""Region"":""$Codex_RegionName_18;"", ""Region_Localised"":""Inner Orion Spur"", ""System"":""HIP 87621"", ""SystemAddress"":147882789259, ""BodyID"":1, ""NearestDestination"":""Biological Site"", ""Latitude"":-22.478512, ""Longitude"":-5.011441, ""IsNewEntry"":true, ""VoucherAmount"":50000 }",
            @"{ ""timestamp"":""2025-07-07T01:58:57Z"", ""event"":""CodexEntry"", ""EntryID"":1400306, ""Name"":""$Codex_Ent_Lava_Spouts_SilicateMagma_Name;"", ""Name_Localised"":""Silicate Magma Lava Spout"", ""SubCategory"":""$Codex_SubCategory_Geology_and_Anomalies;"", ""SubCategory_Localised"":""Geology and anomalies"", ""Category"":""$Codex_Category_Biology;"", ""Category_Localised"":""Biological and Geological"", ""Region"":""$Codex_RegionName_18;"", ""Region_Localised"":""Inner Orion Spur"", ""System"":""HIP 15304"", ""SystemAddress"":2793616165211, ""BodyID"":58, ""Latitude"":-14.272595, ""Longitude"":-144.836014, ""IsNewEntry"":true, ""VoucherAmount"":50000 }",
            @"{ ""timestamp"":""2024-03-17T07:39:39Z"", ""event"":""CodexEntry"", ""EntryID"":1200601, ""Name"":""$Codex_Ent_Standard_Sudarsky_Class_II_Name;"", ""Name_Localised"":""Standard gas giant"", ""SubCategory"":""$Codex_SubCategory_Gas_Giants;"", ""SubCategory_Localised"":""Gas giant planets"", ""Category"":""$Codex_Category_StellarBodies;"", ""Category_Localised"":""Astronomical Bodies"", ""Region"":""$Codex_RegionName_34;"", ""Region_Localised"":""Sanguineous Rim"", ""System"":""Outotz LX-K c8-1"", ""SystemAddress"":359267438922, ""BodyID"":1, ""IsNewEntry"":true }",
            @"{ ""timestamp"":""2023-07-03T04:08:29Z"", ""event"":""CodexEntry"", ""EntryID"":3100501, ""Name"":""$Codex_Ent_Glaive_Name;"", ""Name_Localised"":""Thargoid Hunter Glaive"", ""SubCategory"":""$Codex_SubCategory_Thargoid;"", ""SubCategory_Localised"":""Thargoid objects"", ""Category"":""$Codex_Category_Civilisations;"", ""Category_Localised"":""Xenological"", ""Region"":""$Codex_RegionName_18;"", ""Region_Localised"":""Inner Orion Spur"", ""System"":""HIP 21991"", ""SystemAddress"":83986682554, ""BodyID"":0, ""IsNewEntry"":true, ""VoucherAmount"":50000 }",
            @"{ ""timestamp"":""2019-07-02T03:02:31Z"", ""event"":""CodexEntry"", ""EntryID"":2301801, ""Name"":""$Codex_Ent_L_Org_Moll03_V3_Def_Name;"", ""Name_Localised"":""Luteolum Umbrella Mollusc"", ""SubCategory"":""$Codex_SubCategory_Organic_Structures;"", ""SubCategory_Localised"":""Organic structures"", ""Category"":""$Codex_Category_Biology;"", ""Category_Localised"":""Biological and Geological"", ""Region"":""$Codex_RegionName_9;"", ""Region_Localised"":""Inner Scutum-Centaurus Arm"", ""System"":""Canonnia"", ""SystemAddress"":13603220441236, ""Traits"":[ ""o_l_turn01_idle"" ], ""NewTraitsDiscovered"":true }",
            @"{ ""timestamp"":""2020-10-28T13:10:57Z"", ""event"":""CodexEntry"", ""EntryID"":3200200, ""Name"":""$Codex_Ent_Guardian_Data_Logs_Name;"", ""Name_Localised"":""Guardian Codex"", ""SubCategory"":""$Codex_SubCategory_Guardian;"", ""SubCategory_Localised"":""Guardian objects"", ""Category"":""$Codex_Category_Civilisations;"", ""Category_Localised"":""Xenological"", ""Region"":""$Codex_RegionName_18;"", ""Region_Localised"":""Inner Orion Spur"", ""System"":""Synuefe NL-N c23-4"", ""SystemAddress"":1184840454858, ""NearestDestination"":""$Ancient:#index=1;"", ""NearestDestination_Localised"":""Ancient Ruins (1)"", ""IsNewEntry"":true }",
            @"{ ""timestamp"":""2019-02-04T02:49:07Z"", ""event"":""CodexEntry"", ""EntryID"":2100802, ""Name"":""$Codex_Ent_L_Cry_MetCry_Pur_Name;"", ""Name_Localised"":""Purpureum Metallic Crystals"", ""SubCategory"":""$Codex_SubCategory_Organic_Structures;"", ""SubCategory_Localised"":""Organic structures"", ""Category"":""$Codex_Category_Biology;"", ""Category_Localised"":""Biological and Geological"", ""Region"":""$Codex_RegionName_18;"", ""Region_Localised"":""Inner Orion Spur"", ""System"":""Pru Aescs NC-M d7-192"", ""SystemAddress"":6606892846275, ""IsNewEntry"":true }"
        ];
        
        [PublicAPI( "The system name of the star system where the entry was discovered." )]
        public string systemName { get; }

        [PublicAPI( "The system numerical address of the star system where the entry was discovered." )]
        public ulong systemAddress { get; }

        [PublicAPI( "An invariant category for the entry." )]
        public string categoryName => categoryEdName?
            .Replace( "Codex_Category_", "" )
            .Replace( "$", "" )
            .Replace( ";", "" )
            .Replace( "_", " " );

        [PublicAPI( "An invariant subcategory for the entry." )]
        public string subCategoryName => subCategoryEdName?
            .Replace( "Codex_SubCategory_", "" )
            .Replace( "$", "" )
            .Replace( ";", "" )
            .Replace( "_", " " );

        [PublicAPI( "An invariant name of the entry." )]
        public string entryName => edname?
            .Replace( "Codex_Ent_", "" )
            .Replace( "$", "" )
            .Replace( "_Name;", "" )
            .Replace( "_name;", "" )
            .Replace( ";", "" )
            .Replace( "_", " " );

        [PublicAPI( "The stellar region where the discovery was found." ) ]
        public string region { get; }

        [PublicAPI( "True if this is a new discovery." )]
        public bool newEntry { get; private set; }

        [PublicAPI( "True if one or more new traits were discovered." )]
        public bool newTrait { get; private set; }

        [PublicAPI( "The credit voucher amount awarded for the discovery, if any" )]
        public int voucherAmount { get; private set; }
        
        [PublicAPI( "Details of codex entries for stellar bodies." )]
        public CodexStellarBody stellarBody { get; private set; }

        [PublicAPI( "Details of codex entries for geology and anomalies." )]
        public CodexGeologyOrAnomoly geology { get; private set; }

        [PublicAPI( "Details of codex entries for the Guardian civilization." )]
        public CodexCivilizationGuardian guardian { get; private set; }

        [PublicAPI( "Details of codex entries for organics." )]
        public Organic organic { get; private set; }

        [PublicAPI( "Details of codex entries for the Thargoid civilization." )]
        public CodexCivilizationThargoid thargoid { get; private set; }

        // For internal reference only
        public long entryId { get; }
        public string edname { get; }
        public string categoryEdName { get; }
        public string subCategoryEdName { get; }

        public CodexEntryEvent ( DateTime timestamp, string systemName, ulong systemAddress, string region, long entryId,
            string edname, string subCategoryEdName, string categoryEdName, bool newEntry,
            bool newTrait, int voucherAmount ) : base( timestamp, NAME )
        {
            this.entryId = entryId;
            this.edname = edname;
            this.subCategoryEdName = subCategoryEdName;
            this.categoryEdName = categoryEdName;
            this.systemName = systemName;
            this.systemAddress = systemAddress;
            this.region = region;
            this.newEntry = newEntry;
            this.newTrait = newTrait;
            this.voucherAmount = voucherAmount;

            switch ( categoryEdName )
            {
                case "$Codex_Category_Biology;" when subCategoryEdName == "$Codex_SubCategory_Organic_Structures;":
                    organic = Organic.Lookup( entryId, edname );
                    break;
                case "$Codex_Category_Biology;" when subCategoryEdName == "$Codex_SubCategory_Geology_and_Anomalies;":
                    geology = CodexGeologyOrAnomoly.Lookup( entryId, edname );
                    break;
                case "$Codex_Category_StellarBodies;":
                    stellarBody = CodexStellarBody.Lookup( entryId, edname );
                    break;
                case "$Codex_Category_Civilisations;" when subCategoryEdName == "$Codex_SubCategory_Guardian;":
                    guardian = CodexCivilizationGuardian.Lookup( entryId, edname );
                    break;
                case "$Codex_Category_Civilisations;" when subCategoryEdName == "$Codex_SubCategory_Thargoid;":
                    thargoid = CodexCivilizationThargoid.Lookup( entryId, edname );
                    break;
            }
        }

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var entryId = JsonParsing.getLong(data, "EntryID");
            var edname = JsonParsing.getString(data, "Name");
            var subCategoryEDName = JsonParsing.getString( data, "SubCategory" );
            var categoryEDName = JsonParsing.getString( data, "Category" );
            var obtainedRegion = int.TryParse( JsonParsing.getString( data, "Region" )?.Replace( "$Codex_RegionName_", "" ).Replace( ";", "" ), out var regionIndex );
            var region = obtainedRegion ? StarSystemRegion.FromRegionId( regionIndex ) : null;
            var systemName = JsonParsing.getString(data, "System");
            var systemAddress = JsonParsing.getULong(data, "SystemAddress");
            var newEntry = JsonParsing.getOptionalBool( data, "IsNewEntry" ) ?? false;
            var newTrait = JsonParsing.getOptionalBool( data, "NewTraitsDiscovered" ) ?? false;
            var voucherAmount = JsonParsing.getOptionalInt( data, "VoucherAmount" ) ?? 0;

            events.Add( new CodexEntryEvent( timestamp, systemName, systemAddress, region, entryId, edname, subCategoryEDName, categoryEDName, newEntry, newTrait, voucherAmount ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ScanOrganicEvent : Event
    {
        public const string NAME = "Scan organic event";
        public const string DESCRIPTION = "Triggered when an organic scan is made";
        public const string SAMPLE = @"{ ""timestamp"":""2023-07-22T04:01:18Z"", ""event"":""ScanOrganic"", ""ScanType"":""Log"", ""Genus"":""$Codex_Ent_Shrubs_Genus_Name;"", ""Genus_Localised"":""Frutexa"", ""Species"":""$Codex_Ent_Shrubs_05_Name;"", ""Species_Localised"":""Frutexa Fera"", ""Variant"":""$Codex_Ent_Shrubs_05_F_Name;"", ""Variant_Localised"":""Frutexa Fera - Green"", ""SystemAddress"":34542299533283, ""Body"":42 }";

        [PublicAPI( "The ID of the body, so we know when location has changed" )]
        public int bodyId { get; private set; }

        [PublicAPI( "The type of scan which can be Log, Sample or Analyse" )]
        public string scanType { get; private set; }

        [PublicAPI( "Simple biologic name, such as 'Frutexa'" )]
        public string localisedGenus { get; private set; }
        //public string genus { get; private set; }

        [PublicAPI( "Species of the genus, such as 'Frutexa Fera'" )]
        public string localisedSpecies { get; private set; }
        //public string species { get; private set; }

        [PublicAPI( "The full type of the biolocal, such as 'Frutexa Fera - Green'")]
        public string localisedVariant { get; private set; }
        //public string variant { get; private set; }

        [PublicAPI( "The detailed data for the biological" )]
        public OrganicItem data { get; private set; }

        [PublicAPI( "The detailed data for the biological, set from DiscoveryMonitor" )]
        public OrganicItem data_new { get; set; }

        [PublicAPI( "Test variable" )]
        public string currentSystem;

        [PublicAPI( "Test variable" )]
        public string currentBody;

        // Not intended to be user facing

        public ScanOrganicEvent ( DateTime timestamp, int bodyId, string scanType, string localisedGenus, string localisedSpecies, string localisedVariant ) : base(timestamp, NAME)
        {
            this.bodyId = bodyId;
            this.scanType = scanType;
            this.localisedGenus = localisedGenus;
            //this.genus = genus;
            this.localisedSpecies = localisedSpecies;
            //this.species = species;
            this.localisedVariant = localisedVariant;
            //this.variant = variant;

            // TODO:#2212........[Update to use DiscoveryMonitor]
            this.data = new OrganicItem();

            //this.data = OrganicInfo.GetData( localisedGenus, localisedSpecies );
            //System.Windows.Forms.MessageBox.Show( "My message here" );
        }
    }
}

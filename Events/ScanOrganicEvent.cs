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
        public const string SAMPLE = @"{ ""timestamp"":""2023-07-22T04:01:18Z"", ""event"":""ScanOrganic"", ""ScanType"":""Sample"", ""Genus"":""$Codex_Ent_Shrubs_Genus_Name;"", ""Genus_Localised"":""Frutexa"", ""Species"":""$Codex_Ent_Shrubs_05_Name;"", ""Species_Localised"":""Frutexa Fera"", ""Variant"":""$Codex_Ent_Shrubs_05_F_Name;"", ""Variant_Localised"":""Frutexa Fera - Green"", ""SystemAddress"":34542299533283, ""Body"":42 }";

        [PublicAPI( "The type of scan which can be Log, Sample or Analyse" )]
        public string scanType { get; private set; }

        [PublicAPI( "An object holding all the data about the organism currently being sampled (as an object)" )]
        public Exobiology bio { get; set; } // Variable is updated by the Discovery Monitor before being handled by Responders
        
        [PublicAPI( "The other organisms for which samples are incomplete on the current body (as objects)" )]
        public List<Exobiology> remainingBios { get; set; } // Variable is updated by the Discovery Monitor before being handled by Responders

        // Not intended to be user facing
        
        public OrganicGenus genus { get; private set; }
        public OrganicSpecies species { get; private set; }
        public OrganicVariant variant { get; private set; }
        public ulong systemAddress { get; private set; }
        public int bodyId { get; private set; }

        public ScanOrganicEvent ( DateTime timestamp, ulong systemAddress, int bodyId, string scanType, OrganicGenus genus, OrganicSpecies species, OrganicVariant variant ) : base( timestamp, NAME )
        {
            this.systemAddress = systemAddress;
            this.bodyId = bodyId;
            this.scanType = scanType;
            this.genus = genus;
            this.species = species;
            this.variant = variant;
        }
    }
}

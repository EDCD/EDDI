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

        [PublicAPI( "The type of scan which can be Log, Sample or Analyse" )]
        public string scanType { get; private set; }

        //[PublicAPI( "Test variable" )]
        //public string currentSystem;

        //[PublicAPI( "Test variable" )]
        //public string currentBody;

        [PublicAPI( "The object holding all the data about the current biological." )]
        public Exobiology bio { get; set; }

        [PublicAPI]
        public int? numTotal { get; set; }

        [PublicAPI]
        public int? numComplete => numTotal - numRemaining;

        [ PublicAPI ] 
        public int? numRemaining => listRemaining.Count;

        [PublicAPI]
        public List<string> listRemaining { get; set; }

        // Not intended to be user facing

        public string genus;
        public string species;
        public string variant;
        public ulong systemAddress { get; private set; }
        public int bodyId { get; private set; }

        public ScanOrganicEvent ( DateTime timestamp, ulong systemAddress, int bodyId, Body body, string scanType, string genus, string species, string variant ) : base(timestamp, NAME)
        {
            this.systemAddress = systemAddress;
            this.bodyId = bodyId;
            this.scanType = scanType;
            this.genus = genus;
            this.species = species;
            this.variant = variant;

            this.bio = body.surfaceSignals.TryGetBio( genus, out var fetchedBio ) ? fetchedBio : new Exobiology( genus );
        }
    }
}

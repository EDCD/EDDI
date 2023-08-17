using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Threading;
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
        public int? numComplete { get; set; }

        [PublicAPI]
        public int? numRemaining { get; set; }

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
            this.bodyId = bodyId;
            this.scanType = scanType;
            this.genus = genus;
            this.species = species;
            this.variant = variant;

            try
            {
                this.bio = new Exobiology( genus );
                try
                {
                    this.bio = body.surfaceSignals.GetBio( genus );
                    //Logging.Info( $"[ScanOrganicEvent] GetBio ---------------------------------------------" );
                    //Thread.Sleep( 10 );
                    //Logging.Info( $"[ScanOrganicEvent] GetBio:    Genus = '{this.bio.genus.name}'" );
                    //Thread.Sleep( 10 );
                    //Logging.Info( $"[ScanOrganicEvent] GetBio:  Species = '{this.bio.species.name}'" );
                    //Thread.Sleep( 10 );
                    //Logging.Info( $"[ScanOrganicEvent] GetBio:  Variant = '{this.bio.variant}'" );
                    //Thread.Sleep( 10 );
                    //Logging.Info( $"[ScanOrganicEvent] GetBio:    Genus = '{this.bio.genus.name}'" );
                    //Thread.Sleep( 10 );
                    //Logging.Info( $"[ScanOrganicEvent] GetBio: Distance = '{this.bio.genus.distance}'" );
                    //Thread.Sleep( 10 );
                    //Logging.Info( $"[ScanOrganicEvent] GetBio ---------------------------------------------" );
                    //Thread.Sleep( 10 );

                    // TODO:#2212........[These are lagged by one sample if taken here, not updated until after Sample() is called by DiscoveryMonitor and only DiscoveryMonitor has access to current location]
                    //this.total = body.surfaceSignals.bio.total;
                    //this.complete = body.surfaceSignals.bio.complete;
                    //this.remaining = body.surfaceSignals.bio.remaining;
                }
                catch ( System.Exception e )
                {
                    Logging.Error( $"ScanOrganicEvent: Failed to set 'this.bio = body.surfaceSignals.GetBio( genus )' [{e}]" );
                }
            }
            catch
            {
                Logging.Error( "ScanOrganicEvent: Failed to get Surface Signals" );
            }
        }
    }
}

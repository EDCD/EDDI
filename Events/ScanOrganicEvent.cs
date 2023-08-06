using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;
using System.Threading;

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

        //[PublicAPI( "Simple biologic name, such as 'Frutexa'" )]
        //public string localisedGenus { get; private set; }
        //public string genus { get; private set; }

        //[PublicAPI( "Species of the genus, such as 'Frutexa Fera'" )]
        //public string localisedSpecies { get; private set; }
        //public string species { get; private set; }

        //[PublicAPI( "The full type of the biolocal, such as 'Frutexa Fera - Green'")]
        //public string localisedVariant { get; private set; }
        //public string variant { get; private set; }

        //[PublicAPI( "The detailed data for the biological" )]
        //public OrganicItem data { get; private set; }

        //[PublicAPI( "The detailed data for the biological, set from DiscoveryMonitor" )]
        //public OrganicItem data_new { get; set; }

        [PublicAPI( "Test variable" )]
        public string currentSystem;

        [PublicAPI( "Test variable" )]
        public string currentBody;

        [PublicAPI( "The object holding all the data about the current biological." )]
        public Exobiology bio { get; set; }

        // Not intended to be user facing

        public string genus;
        public string species;
        public string variant;

        public ulong systemAddress { get; private set; }

        public int bodyId { get; private set; }

        public ScanOrganicEvent ( DateTime timestamp, ulong systemAddress, int bodyId, Body body, string scanType, string genus, string species, string variant ) : base(timestamp, NAME)
        {
            // TODO:#2212........[Handle fromLoad events?]
            if ( !fromLoad )
            {
                this.bodyId = bodyId;
                this.scanType = scanType;
                this.genus = genus;
                this.species = species;
                this.variant = variant;

                // bio is set by DiscoveryMonitor, we don't have access to the currentSystem or Body from here.
                // ^This doesn't seem to work
                try
                {
                    this.bio = new Exobiology();
                    try
                    {
                        if ( body.surfaceSignals == null )
                        {
                            body.surfaceSignals = new SurfaceSignals();
                        }

                        if ( !body.surfaceSignals.bioList.ContainsKey( genus ) )
                        {
                            body.surfaceSignals.AddBio( genus );
                        }

                        this.bio = body.surfaceSignals.GetBio( genus );
                    }
                    catch
                    {
                        new Thread( () => System.Windows.Forms.MessageBox.Show( "Failed to set 'this.bio = body.surfaceSignals.GetBio( genus )'" ) ).Start();
                    }
                }
                catch
                {
                    new Thread( () => System.Windows.Forms.MessageBox.Show( "Failed to get Surface Signals" ) ).Start();
                }
            }
        }
    }
}

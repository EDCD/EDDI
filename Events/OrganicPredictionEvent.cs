using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class OrganicPredictionEvent : Event
    {
        public const string NAME = "Organic Surface Signals Prediction";
        public const string DESCRIPTION = "Triggered when surface signal sources are detected during FSS";
        public const string SAMPLE = @"{ ""timestamp"":""2023-04-05T02:22:19Z"", ""event"":""SAASignalsFound"", ""BodyName"":""NGC 6188 Sector XZ-M a8-1 A 4"", ""SystemAddress"":23149336745976, ""BodyID"":7, ""Signals"":[ { ""Type"":""$SAA_SignalType_Biological;"", ""Type_Localised"":""Biological"", ""Count"":1 }, { ""Type"":""$SAA_SignalType_Geological;"", ""Type_Localised"":""Geological"", ""Count"":2 } ], ""Genuses"":[ { ""Genus"":""$Codex_Ent_Bacterial_Genus_Name;"", ""Genus_Localised"":""Bacterium"" } ] }";

        [PublicAPI( "A list of the biologicals present on the body after an SAA (map) of body." )]
        public List<string> biosignals { get; private set; }

        [PublicAPI( "The body that the surface signals are on" )]
        public Body body { get; private set; }

        public OrganicPredictionEvent ( DateTime timestamp, List<string> signals ) : base(timestamp, NAME)
        {
            this.body = body;

            this.biosignals = signals;

            //if ( body != null )
            //{
            //    this.biosignals = body.surfaceSignals.GetBios();
            //}
            //else
            //{
            //    this.biosignals = new List<string>();
            //}
        }
    }
}

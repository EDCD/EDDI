using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SurfaceSignalsEvent : Event
    {
        public const string NAME = "Surface signals detected";
        public const string DESCRIPTION = "Triggered when surface signal sources are detected";
        public const string SAMPLE = @"{ ""timestamp"":""2023-04-05T02:22:19Z"", ""event"":""SAASignalsFound"", ""BodyName"":""NGC 6188 Sector XZ-M a8-1 A 4"", ""SystemAddress"":23149336745976, ""BodyID"":7, ""Signals"":[ { ""Type"":""$SAA_SignalType_Biological;"", ""Type_Localised"":""Biological"", ""Count"":1 }, { ""Type"":""$SAA_SignalType_Geological;"", ""Type_Localised"":""Geological"", ""Count"":2 } ], ""Genuses"":[ { ""Genus"":""$Codex_Ent_Bacterial_Genus_Name;"", ""Genus_Localised"":""Bacterium"" } ] }";

        [PublicAPI("The signal detection type (either 'FSS' or 'SAA'")]
        public string detectionType { get; private set; }

        [PublicAPI("The body where surface signals were detected")]
        public string bodyname { get; private set; }

        [PublicAPI("A list of signals (as objects)")]
        public List<SignalAmount> surfacesignals { get; private set; }

        //[PublicAPI( "The biological information" )]
        //public Exobiology bio { get; private set; }

        [PublicAPI( "The body that the surface signals are on" )]
        public Body body { get; private set; }

        // Not intended to be user facing

        //public List<string> biosignals { get; private set; }

        public ulong? systemAddress { get; private set; }
        
        public long bodyId { get; private set; }

        public SurfaceSignalsEvent ( DateTime timestamp, string detectionType, ulong? systemAddress, string bodyName, long bodyId, List<SignalAmount> surfaceSignals, Body body ) : base( timestamp, NAME )
        {
            this.detectionType = detectionType;
            this.systemAddress = systemAddress;
            this.bodyname = bodyName;
            this.bodyId = bodyId;
            this.surfacesignals = surfaceSignals;
            this.body = body;
        }
    }
}

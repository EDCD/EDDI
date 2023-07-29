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
        //public const string SAMPLE = @"{ ""timestamp"":""2019-08-19T00:36:40Z"", ""event"":""SAASignalsFound"", ""BodyName"":""Oponner 6 a"", ""SystemAddress"":3721345878371, ""BodyID"":30, ""Signals"":[ { ""Type"":""$SAA_SignalType_Geological;"", ""Type_Localised"":""Geological"", ""Count"":48 } ] }";
        //public const string SAMPLE = @"{ ""timestamp"":""2023-07-03T04:53:01Z"", ""event"":""SAASignalsFound"", ""BodyName"":""Trifid Sector BQ-Y d210 4 a"", ""SystemAddress"":7225888213867, ""BodyID"":6, ""Signals"":[ { ""Type"":""$SAA_SignalType_Biological;"", ""Type_Localised"":""Biological"", ""Count"":6 } ], ""Genuses"":[ { ""Genus"":""$Codex_Ent_Bacterial_Genus_Name;"", ""Genus_Localised"":""Bacterium"" }, { ""Genus"":""$Codex_Ent_Cactoid_Genus_Name;"", ""Genus_Localised"":""Cactoida"" }, { ""Genus"":""$Codex_Ent_Fungoids_Genus_Name;"", ""Genus_Localised"":""Fungoida"" }, { ""Genus"":""$Codex_Ent_Osseus_Genus_Name;"", ""Genus_Localised"":""Osseus"" }, { ""Genus"":""$Codex_Ent_Shrubs_Genus_Name;"", ""Genus_Localised"":""Frutexa"" }, { ""Genus"":""$Codex_Ent_Tussocks_Genus_Name;"", ""Genus_Localised"":""Tussock"" } ] }";
        public const string SAMPLE = @"{ ""timestamp"":""2023-04-05T02:22:19Z"", ""event"":""SAASignalsFound"", ""BodyName"":""NGC 6188 Sector XZ-M a8-1 A 4"", ""SystemAddress"":23149336745976, ""BodyID"":7, ""Signals"":[ { ""Type"":""$SAA_SignalType_Biological;"", ""Type_Localised"":""Biological"", ""Count"":1 }, { ""Type"":""$SAA_SignalType_Geological;"", ""Type_Localised"":""Geological"", ""Count"":2 } ], ""Genuses"":[ { ""Genus"":""$Codex_Ent_Bacterial_Genus_Name;"", ""Genus_Localised"":""Bacterium"" } ] }";

        [PublicAPI("The signal detection type (either 'FSS' or 'SAA'")]
        public string detectionType { get; private set; }

        [PublicAPI("The body where surface signals were detected")]
        public string bodyname { get; private set; }

        [PublicAPI("A list of signals (as objects)")]
        public List<SignalAmount> surfacesignals { get; private set; }

        [PublicAPI( "A list of biological signals (as objects)" )]
        public List<string> biosignals { get; private set; }

        // Not intended to be user facing

        public ulong? systemAddress { get; private set; }
        
        public long bodyId { get; private set; }

        public SurfaceSignalsEvent ( DateTime timestamp, string detectionType, ulong? systemAddress, string bodyName, long bodyId, List<SignalAmount> surfaceSignals ) : base( timestamp, NAME )
        {
            this.detectionType = detectionType;
            this.systemAddress = systemAddress;
            this.bodyname = bodyName;
            this.bodyId = bodyId;
            this.surfacesignals = surfaceSignals;
            this.biosignals = new List<string>();
        }

        public SurfaceSignalsEvent(DateTime timestamp, string detectionType, ulong? systemAddress, string bodyName, long bodyId, List<SignalAmount> surfaceSignals, List<string> bioSignals ) : base(timestamp, NAME)
        {
            this.detectionType = detectionType;
            this.systemAddress = systemAddress;
            this.bodyname = bodyName;
            this.bodyId = bodyId;
            this.surfacesignals = surfaceSignals;
            this.biosignals = bioSignals;
        }
    }
}

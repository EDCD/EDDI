using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [ PublicAPI ]
    public class ScanOrganicEvent (
        DateTime timestamp,
        ulong systemAddress,
        int bodyId,
        string scanType,
        int? scanStage,
        Organic organic )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Scan organic";
        public const string DESCRIPTION = "Triggered when an organic scan is made";

        public static readonly string[] SAMPLES =
        [
            @"{ ""timestamp"":""2023-07-22T04:01:18Z"", ""event"":""ScanOrganic"", ""ScanType"":""Sample"", ""Genus"":""$Codex_Ent_Shrubs_Genus_Name;"", ""Genus_Localised"":""Frutexa"", ""Species"":""$Codex_Ent_Shrubs_05_Name;"", ""Species_Localised"":""Frutexa Fera"", ""Variant"":""$Codex_Ent_Shrubs_05_F_Name;"", ""Variant_Localised"":""Frutexa Fera - Green"", ""SystemAddress"":34542299533283, ""Body"":42 }",
            @"{ ""timestamp"":""2026-01-17T09:10:43Z"", ""event"":""ScanOrganic"", ""ScanType"":""Log"", ""Genus"":""$Codex_Ent_Ingensradices_Genus_Name;"", ""Genus_Localised"":""Radicoida"", ""Species"":""$Codex_Ent_Ingensradices_Unicus_Name;"", ""Species_Localised"":""Radicoida Unica"", ""Variant"":""$Codex_Ent_Ingensradices_Unicus_Name;"", ""Variant_Localised"":""Radicoida Unica"", ""WasLogged"":false, ""SystemAddress"":147882789259, ""Body"":1 }",
            @"{ ""timestamp"":""2025-04-04T21:05:27Z"", ""event"":""ScanOrganic"", ""ScanType"":""Sample"", ""Genus"":""$Codex_Ent_Cone_Name;"", ""Genus_Localised"":""Bark Mounds"", ""Species"":""$Codex_Ent_Cone_Name;"", ""Species_Localised"":""Bark Mounds"", ""Variant"":""$Codex_Ent_Cone_Name;"", ""Variant_Localised"":""Bark Mounds"", ""SystemAddress"":7142932230673, ""Body"":88 }",
            @"{ ""timestamp"":""2025-03-08T21:38:23Z"", ""event"":""ScanOrganic"", ""ScanType"":""Analyse"", ""Genus"":""$Codex_Ent_Brancae_Name;"", ""Genus_Localised"":""Brain Trees"", ""Species"":""$Codex_Ent_SeedABCD_02_Name;"", ""Species_Localised"":""Ostrinum Brain Tree"", ""Variant"":""$Codex_Ent_SeedABCD_02_Name;"", ""Variant_Localised"":""Ostrinum Brain Tree"", ""SystemAddress"":4483241153218, ""Body"":6 }"
        ];

        [PublicAPI( "The numeric ID of the star system where the organism was scanned" )]
        public ulong systemAddress { get; private set; } = systemAddress;

        [PublicAPI( "The numeric ID of the body where the organism was scanned" )]
        public int bodyId { get; private set; } = bodyId;

        [PublicAPI( "The type of scan (e.g. 'Log' for the 1st scan, 'Sample' for the 2nd and 3rd scans, and then 'Analyse' once the genetic sampler completes processing the samples)" ) ]
        public string scanType { get; private set; } = scanType;

        [ PublicAPI( "The numerical index of the scan type, if known (e.g. 1, 2, 3, or 4)" ) ]
        public int? scanStage { get; private set; } = scanStage;

        [ PublicAPI( "The minimum distance in meters that you need to travel before you can scan this organism again (if known)" )]
        public int? minSampleDistance => organic?.minimumDistanceMeters;

        [ PublicAPI( "An object holding data about the organism currently being sampled" ) ]
        public Organic organic { get; set; } = organic;

        // Not intended to be user facing
        private static int? _scanStage;
        private static string _scanTarget;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var systemAddress = JsonParsing.getULong( data, "SystemAddress" );
            var bodyId = JsonParsing.getInt( data, "Body" ); // This is in fact the BodyID, not the body name
            var scanType = JsonParsing.getString( data, "ScanType" ); // Log, Sample (x2), then Analyse
            var variant = EventParsing.GetOrganicVariant( data );
            var organic = variant != null ? new Organic( variant ) : null;

            switch ( scanType )
            {
                case "Log":
                    _scanStage = 1;
                    break;
                case "Sample" when _scanStage != null && _scanTarget == variant?.edname:
                    _scanStage += 1;
                    break;
                case "Analyse":
                    _scanStage = 4;
                    break;
            }
            _scanTarget = variant?.edname;

            events.Add( new ScanOrganicEvent( timestamp, systemAddress, bodyId, scanType, _scanStage, organic ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
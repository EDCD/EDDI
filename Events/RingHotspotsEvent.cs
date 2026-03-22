using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class RingHotspotsEvent (
        DateTime timestamp,
        ulong? systemAddress,
        string bodyName,
        long bodyId,
        List<CommodityAmount> hotspots )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Ring hotspots detected";
        public const string DESCRIPTION = "Triggered when hotspots are detected in a ring";
        public static readonly string[] SAMPLES =
        [
            @"{ ""timestamp"":""2025-05-04T08:00:23Z"", ""event"":""SAASignalsFound"", ""BodyName"":""Col 69 Sector ZK-O d6-15 7 B Ring"", ""SystemAddress"":525974046971, ""BodyID"":19, ""Signals"":[ { ""Type"":""Serendibite"", ""Count"":5 }, { ""Type"":""Alexandrite"", ""Count"":2 }, { ""Type"":""Benitoite"", ""Count"":7 }, { ""Type"":""Monazite"", ""Count"":3 }, { ""Type"":""Musgravite"", ""Count"":4 } ], ""Genuses"":[  ] }",
            @"{ ""timestamp"":""2019-08-19T00:24:53Z"", ""event"":""SAASignalsFound"", ""BodyName"":""Oponner 6 A Ring"", ""SystemAddress"":3721345878371, ""BodyID"":29, ""Signals"":[ { ""Type"":""Bromellite"", ""Count"":3 }, { ""Type"":""Grandidierite"", ""Count"":5 }, { ""Type"":""LowTemperatureDiamond"", ""Type_Localised"":""Low Temperature Diamonds"", ""Count"":1 } ] }"
        ];

        [PublicAPI("The ring where hotspots were detected")]
        public string bodyname { get; private set; } = bodyName;

        [PublicAPI( "The numeric system address of the star system where hotspots were detected" )]
        public ulong? systemAddress { get; private set; } = systemAddress;

        [PublicAPI("A list of ring hotspots (as objects with properties 'commodity' and 'amount')")]
        public List<CommodityAmount> hotspots { get; private set; } = hotspots;

        // Not intended to be user facing

        public long bodyId { get; private set; } = bodyId;
    }
}

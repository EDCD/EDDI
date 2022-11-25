﻿using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class RingHotspotsEvent : Event
    {
        public const string NAME = "Ring hotspots detected";
        public const string DESCRIPTION = "Triggered when hotspots are detected in a ring";
        public const string SAMPLE = @"{ ""timestamp"":""2019-08-19T00:24:53Z"", ""event"":""SAASignalsFound"", ""BodyName"":""Oponner 6 A Ring"", ""SystemAddress"":3721345878371, ""BodyID"":29, ""Signals"":[ { ""Type"":""Bromellite"", ""Count"":3 }, { ""Type"":""Grandidierite"", ""Count"":5 }, { ""Type"":""LowTemperatureDiamond"", ""Type_Localised"":""Low Temperature Diamonds"", ""Count"":1 } ] }";

        [PublicAPI("The ring where hotspots were detected")]
        public string bodyname { get; private set; }

        [PublicAPI("A list of ring hotspots (as objects with properties 'commodity' and 'amount')")]
        public List<CommodityAmount> hotspots { get; private set; }

        // Not intended to be user facing

        public ulong? systemAddress { get; private set; }

        public long bodyId { get; private set; }

        public RingHotspotsEvent(DateTime timestamp, ulong? systemAddress, string bodyName, long bodyId, List<CommodityAmount> hotspots) : base(timestamp, NAME)
        {
            this.systemAddress = systemAddress;
            this.bodyname = bodyName;
            this.bodyId = bodyId;
            this.hotspots = hotspots;
        }
    }
}

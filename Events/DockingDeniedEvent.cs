﻿using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class DockingDeniedEvent : Event
    {
        public const string NAME = "Docking denied";
        public const string DESCRIPTION = "Triggered when your ship is denied docking at a station or outpost";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockingDenied\",\"StationName\":\"Jameson Memorial\", \"StationType\":\"Orbis\", \"MarketID\": 128666762,\"Reason\":\"Distance\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DockingDeniedEvent()
        {
            VARIABLES.Add("station", "The station at which the commander has been denied docking");
            VARIABLES.Add("stationtype", "The localized model / type of the station at which the commander has been denied docking");
            VARIABLES.Add("stationDefinition", "The model / type of the station at which the commander has been denied docking (this is an object)");
            VARIABLES.Add("reason", "The reason why commander has been denied docking (too far, fighter deployed etc)");
        }

        public string station { get; private set; }

        public StationModel stationDefinition { get; private set; }

        public string stationtype => stationDefinition?.localizedName;

        public string reason { get; private set; }

        // Admin

        [VoiceAttackIgnore]
        public long marketId { get; private set; }

        public DockingDeniedEvent(DateTime timestamp, string station, string stationType, long marketId, string reason) : base(timestamp, NAME)
        {
            this.station = station;
            this.stationDefinition = StationModel.FromEDName(stationType);
            this.marketId = marketId;
            this.reason = reason;
        }
    }
}

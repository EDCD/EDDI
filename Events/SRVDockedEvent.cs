﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class SRVDockedEvent : Event
    {
        public const string NAME = "SRV docked";
        public const string DESCRIPTION = "Triggered when you dock an SRV with your ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockSRV\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SRVDockedEvent()
        {
            VARIABLES.Add("id", "The srv's id");
        }

        [JsonProperty("id")]
        public int? id { get; private set; }

        public SRVDockedEvent(DateTime timestamp, int? id) : base(timestamp, NAME)
        {
            this.id = id;
        }
    }
}

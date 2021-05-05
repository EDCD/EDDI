﻿using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class EnteredNormalSpaceEvent : Event
    {
        public const string NAME = "Entered normal space";
        public const string DESCRIPTION = "Triggered when your ship enters normal space";
        //public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"SupercruiseExit\",\"StarSystem\":\"Yuetu\",\"Body\":\"Yuetu B\",\"BodyType\":\"Star\"}";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"SupercruiseExit\",\"StarSystem\":\"Shinrarta Dezhra\",\"SystemAddress\": 3932277478106,\"Body\":\"Jameson Memorial\",\"BodyType\":\"Station\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static EnteredNormalSpaceEvent()
        {
            VARIABLES.Add("systemname", "The system at which the commander has entered normal space");
            VARIABLES.Add("bodyname", "The nearest body to the commander when entering normal space");
            VARIABLES.Add("bodytype", "The localized type of the nearest body to the commander when entering normal space");
            VARIABLES.Add("bodytype_invariant", "The invariant type of the nearest body to the commander when entering normal space");
            VARIABLES.Add("taxi", "True if the ship is an Apex taxi");
            VARIABLES.Add("multicrew", "True if the ship is belongs to another player");
        }

        [JsonProperty("system")]
        public string systemname { get; private set; }

        public string bodytype => (bodyType ?? BodyType.None).localizedName;

        public string bodytype_invariant => (bodyType ?? BodyType.None).localizedName;

        [JsonProperty("body")]
        public string bodyname { get; private set; }

        [JsonProperty("taxi")]
        public bool? taxi { get; private set; }

        [JsonProperty("multicrew")]
        public bool? multicrew { get; private set; }

        // Deprecated, maintained for compatibility with user scripts
        [JsonIgnore, Obsolete("Use systemname instead")]
        public string system => systemname;
        [JsonIgnore, Obsolete("Use bodyname instead")]
        public string body => bodyname;

        // Variables below are not intended to be user facing
        public long systemAddress { get; private set; }
        public BodyType bodyType { get; private set; } = BodyType.None;
        public long? bodyId { get; private set; }

        public EnteredNormalSpaceEvent(DateTime timestamp, string systemName, long systemAddress, string bodyName, long? bodyId, BodyType bodyType, bool? taxi, bool? multicrew) : base(timestamp, NAME)
        {
            this.systemname = systemName;
            this.systemAddress = systemAddress;
            this.bodyType = bodyType;
            this.bodyname = bodyName;
            this.bodyId = bodyId;
            this.taxi = taxi;
            this.multicrew = multicrew;
        }
    }
}

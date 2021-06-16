﻿using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class NearSurfaceEvent : Event
    {
        public const string NAME = "Near surface";
        public const string DESCRIPTION = "Triggered when you enter or depart the gravity well around a surface";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static NearSurfaceEvent()
        {
            VARIABLES.Add("approaching_surface", "A boolean value. True if you are entering the gravity well and and false if you are leaving");
            VARIABLES.Add("systemname", "The name of the starsystem");
            VARIABLES.Add("bodyname", "The name of the body");
            VARIABLES.Add("shortname", "The short name of the body, less the system name");
        }

        public bool approaching_surface { get; private set; }
        public string systemname { get; private set; }
        public string bodyname { get; private set; }
        public string shortname => Body.GetShortName(bodyname, systemname);

        // Deprecated, maintained for compatibility with user scripts
        [JsonIgnore, Obsolete("Use systemname instead"), VoiceAttackIgnore]
        public string system => systemname;
        [JsonIgnore, Obsolete("Use bodyname instead"), VoiceAttackIgnore]
        public string body => bodyname;

        // Variables below are not intended to be user facing

        [VoiceAttackIgnore]
        public long systemAddress { get; private set; }

        [VoiceAttackIgnore]
        public long? bodyId { get; private set; }

        public NearSurfaceEvent(DateTime timestamp, bool approachingSurface, string systemName, long systemAddress, string bodyName, long? bodyId) : base(timestamp, NAME)
        {
            this.approaching_surface = approachingSurface;
            this.systemname = systemName;
            this.systemAddress = systemAddress;
            this.bodyname = bodyName;
            this.bodyId = bodyId;
        }
    }
}

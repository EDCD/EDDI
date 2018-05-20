using Newtonsoft.Json;
using System;
using System.Collections.Generic;

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
            VARIABLES.Add("system", "The name of the starsystem");
            VARIABLES.Add("body", "The name of the body");
        }

        public bool approaching_surface { get; private set; }
        public string system { get; private set; }
        public string body { get; private set; }

        public NearSurfaceEvent(DateTime timestamp, bool approachingSurface, string system, string body) : base(timestamp, NAME)
        {
            this.approaching_surface = approachingSurface;
            this.system = system;
            this.body = body;
        }
    }
}

using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class CrewFiredEvent : Event
    {
        public const string NAME = "Crew fired";
        public const string DESCRIPTION = "Triggered when you fire crew";
        public const string SAMPLE = "{\"timestamp\":\"2016-08-09T08: 46:29Z\",\"event\":\"CrewFire\",\"Name\":\"Margaret Parrish\",\"CrewID\":236064708}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CrewFiredEvent()
        {
            VARIABLES.Add("name", "The name of the crewmember being fired");
            VARIABLES.Add("crewid", "The ID of the crewmember being assigned");
        }

        [PublicAPI]
        public string name { get; private set; }

        [PublicAPI]
        public long crewid { get; private set; }

        public CrewFiredEvent(DateTime timestamp, string name, long crewid) : base(timestamp, NAME)
        {
            this.name = name;
            this.crewid = crewid;
        }
    }
}

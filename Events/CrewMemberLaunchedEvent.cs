using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CrewMemberLaunchedEvent : Event
    {
        public const string NAME = "Crew member launched";
        public const string DESCRIPTION = "Triggered when a crew member launches the fighter";
        public const string SAMPLE = @"{ ""timestamp"":""2017-03-09T12:28:10Z"", ""event"":""CrewLaunchFighter"", ""Crew"":""M. Volgrand"" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CrewMemberLaunchedEvent()
        {
            VARIABLES.Add("crew", "The name of the crew member who launched");
        }

        [JsonProperty("crew")]
        public string crew { get; private set; }

        public CrewMemberLaunchedEvent(DateTime timestamp, string crew) : base(timestamp, NAME)
        {
            this.crew = crew;
        }
    }
}

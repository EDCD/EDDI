using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class CommanderStartedEvent : Event
    {
        public const string NAME = "Commander started";
        public const string DESCRIPTION = "Triggered when you start a new game";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"NewCommander\",\"Name\":\"HRC1\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CommanderStartedEvent()
        {
            VARIABLES.Add("name", "The name of the new commander");
            VARIABLES.Add("package", "The starting package of the new commander");
        }

        [JsonProperty("name")]
        public string name { get; private set; }

        [JsonProperty("package")]
        public string package { get; private set; }

        public CommanderStartedEvent(DateTime timestamp, string name, string package) : base(timestamp, NAME)
        {
            this.name = name;
            this.package = package;
        }
    }
}

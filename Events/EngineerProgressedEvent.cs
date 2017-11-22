using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class EngineerProgressedEvent : Event
    {
        public const string NAME = "Engineer progressed";
        public const string DESCRIPTION = "Triggered when you reach a new rank with an engineer";
        public const string SAMPLE = @"{ ""timestamp"":""2016-11-14T12:57:18Z"", ""event"":""EngineerProgress"", ""Engineer"":""Broo Tarquin"", ""Rank"":5 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static EngineerProgressedEvent()
        {
            VARIABLES.Add("engineer", "The name of the engineer with whom you have progressed");
            VARIABLES.Add("rank", "The rank of your relationship with the engineer");
        }

        [JsonProperty("engineer")]
        public string engineer { get; private set; }

        [JsonProperty("rank")]
        public int rank { get; private set; }

        public EngineerProgressedEvent(DateTime timestamp, string engineer, int rank) : base(timestamp, NAME)
        {
            this.engineer = engineer;
            this.rank = rank;
        }
    }
}

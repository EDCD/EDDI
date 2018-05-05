using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class FineIncurredEvent : Event
    {
        public const string NAME = "Fine incurred";
        public const string DESCRIPTION = "Triggered when you incur a fine";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"CommitCrime\",\"CrimeType\":\"fireInNoFireZone\",\"Faction\":\"The Pilots Federation\",\"Victim\":\"Potapinski\",\"Fine\":210}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FineIncurredEvent()
        {
            VARIABLES.Add("crimetype", "The type of crime committed");
            VARIABLES.Add("crime", "The decription of the crime committed");
            VARIABLES.Add("victim", "The name of the victim of the crime");
            VARIABLES.Add("faction", "The name of the faction issuing the fine");
            VARIABLES.Add("bounty", "The number of credits issued as the fine");
        }

        [JsonProperty("crimetype")]
        public string crimetype { get; private set; }

        public string crime { get; private set; }

        [JsonProperty("victim")]
        public string victim { get; private set; }

        [JsonProperty("faction")]
        public string faction { get; private set; }

        [JsonProperty("fine")]
        public long fine { get; private set; }

        public FineIncurredEvent(DateTime timestamp, string crimetype, string faction, string victim, long fine) : base(timestamp, NAME)
        {
            this.crimetype = crimetype;
            this.crime = Crime.FromEDName(crimetype).localizedName;
            this.faction = faction;
            this.victim = victim;
            this.fine = fine;
        }
    }
}

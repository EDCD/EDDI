using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class BountyIncurredEvent : Event
    {
        public const string NAME = "Bounty incurred";
        public const string DESCRIPTION = "Triggered when you incur a bounty";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"CommitCrime\",\"CrimeType\":\"assault\",\"Faction\":\"The Pilots Federation\",\"Victim\":\"Potapinski\",\"Bounty\":210}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BountyIncurredEvent()
        {
            VARIABLES.Add("crimetype", "The type of crime committed");
            VARIABLES.Add("victim", "The name of the victim of the crime");
            VARIABLES.Add("faction", "The name of the faction issuing the bounty");
            VARIABLES.Add("bounty", "The number of credits issued as the bounty");
        }

        [JsonProperty("crimetype")]
        public string crimetype { get; private set; }
        public string LocalCrimeType
        {
            get
            {
                if (crimetype != null && crimetype != "")
                {
                    return Crime.FromName(crimetype).LocalName;
                }
                else return null;
            }
        }

        [JsonProperty("victim")]
        public string victim { get; private set; }

        [JsonProperty("faction")]
        public string faction { get; private set; }

        [JsonProperty("bounty")]
        public long bounty { get; private set; }

        public BountyIncurredEvent(DateTime timestamp, string crimetype, string faction, string victim, long bounty) : base(timestamp, NAME)
        {
            this.crimetype = crimetype;
            this.faction = faction;
            this.victim = victim;
            this.bounty = bounty;
        }
    }
}

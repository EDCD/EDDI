using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class MissionAcceptedEvent: Event
    {
        public const string NAME = "Mission accepted";
        public const string DESCRIPTION = "Triggered when you accept a mission";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-22T05:54:00Z\", \"event\":\"MissionAccepted\", \"Faction\":\"Mafia of Cavins\", \"Name\":\"Mission_PassengerVIP\", \"Commodity\":\"$DomesticAppliances_Name;\", \"Commodity_Localised\":\"Domestic Appliances\", \"Count\":3, \"DestinationSystem\":\"Carnoeck\", \"DestinationStation\":\"Bacon City\", \"Expiry\":\"2016-09-22T07:38:43Z\", \"PassengerCount\":3, \"PassengerVIPs\":true, \"PassengerWanted\":false, \"PassengerType\":\"Tourist\", \"MissionID\":26480079 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MissionAcceptedEvent()
        {
            VARIABLES.Add("missionid", "The ID of the mission");
            VARIABLES.Add("name", "The name of the mission");
            VARIABLES.Add("system", "The system in which the mission was obtained");
            VARIABLES.Add("faction", "The faction issuing the mission");
            VARIABLES.Add("communal", "True if the mission is a community goal");
            VARIABLES.Add("expiry", "The expiry date of the mission");
        }

        [JsonProperty("missionid")]
        public long? missionid { get; private set; }

        [JsonProperty("name")]
        public string name { get; private set; }

        [JsonProperty("system")]
        public string system { get; private set; }

        [JsonProperty("faction")]
        public string faction { get; private set; }

        [JsonProperty("communal")]
        public bool communal { get; private set; }

        [JsonProperty("expiry")]
        public DateTime? expiry { get; private set; }

        public MissionAcceptedEvent(DateTime timestamp, long? missionid, string name, string system, string faction, bool communal, DateTime? expiry) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.name = name;
            this.system = system;
            this.faction = faction;
            this.communal = communal;
            this.expiry = expiry;
        }
    }
}

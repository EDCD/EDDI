using EddiDataDefinitions;
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
            VARIABLES.Add("system", "The system at which the commander has entered normal space");
            VARIABLES.Add("body", "The nearest body to the commander when entering normal space");
            VARIABLES.Add("bodytype", "The type of the nearest body to the commander when entering normal space");
        }

        [JsonProperty("system")]
        public string system { get; private set; }

        public string bodytype => (bodyType ?? BodyType.None).localizedName;

        [JsonProperty("body")]
        public string body{ get; private set; }

        // Admin
        public long systemAddress { get; private set; }
        public BodyType bodyType { get; private set; } = BodyType.None;

        public EnteredNormalSpaceEvent(DateTime timestamp, string system, long systemAddress, string body, BodyType bodyType) : base(timestamp, NAME)
        {
            this.system = system;
            this.systemAddress = systemAddress;
            this.bodyType = bodyType;
            this.body = body;
        }
    }
}

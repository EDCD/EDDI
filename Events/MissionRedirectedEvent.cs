using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class MissionRedirectedEvent: Event
    {
        public const string NAME = "Mission redirected";
        public const string DESCRIPTION = "Triggered when a mission is redirected";
        public const string SAMPLE = "{ \"timestamp\": \"2017-08-01T09:04:07Z\", \"event\": \"MissionRedirected\", \"MissionID\": 65367315, \"MissionName\":\"Mission_Courier\", \"NewDestinationStation\": \"Metcalf Orbital\", \"OldDestinationStation\": \"Cuffey Orbital\", \"NewDestinationSystem\": \"Cemiess\", \"OldDestinationSystem\": \"Vequess\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MissionRedirectedEvent()
        {
            VARIABLES.Add("missionid", "The ID of the mission");
            VARIABLES.Add("name", "The name of the mission");
            VARIABLES.Add("newdestinationstation", "The new destination station for the mission");
            VARIABLES.Add("olddestinationstation", "The old destination station for the mission");
            VARIABLES.Add("newdestinationsystem", "The new destination system for the mission");
            VARIABLES.Add("olddestinationsystem", "The old destination system for the mission");            
        }

        public long? missionid { get; private set; }

        public string name { get; private set; }

        public string newdestinationstation { get; private set; }

        public string olddestinationstation { get; private set; }

        public string newdestinationsystem { get; private set; }

        public string olddestinationsystem { get; private set; }

        public MissionRedirectedEvent(DateTime timestamp, long? missionid, string name, string newdestinationstation, string olddestinationstation, string newdestinationsystem, string olddestinationsystem) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.name = name;
            this.newdestinationstation = newdestinationstation;
            this.olddestinationstation = olddestinationstation;
            this.newdestinationsystem = newdestinationsystem;
            this.olddestinationsystem = olddestinationsystem;
        }
    }
}

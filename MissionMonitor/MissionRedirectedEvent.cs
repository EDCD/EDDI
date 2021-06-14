using EddiEvents;
using System;
using Utilities;

namespace EddiMissionMonitor
{
    [PublicAPI]
    public class MissionRedirectedEvent : Event
    {
        public const string NAME = "Mission redirected";
        public const string DESCRIPTION = "Triggered when a mission is redirected";
        public const string SAMPLE = "{ \"timestamp\": \"2017-08-01T09:04:07Z\", \"event\": \"MissionRedirected\", \"MissionID\": 65367315, \"MissionName\":\"Mission_Courier\", \"NewDestinationStation\": \"Metcalf Orbital\", \"OldDestinationStation\": \"Cuffey Orbital\", \"NewDestinationSystem\": \"Cemiess\", \"OldDestinationSystem\": \"Vequess\" }";

        [PublicAPI("The ID of the mission")]
        public long? missionid { get; private set; }

        [PublicAPI("The name of the mission")]
        public string name { get; private set; }

        [PublicAPI("The new destination station for the mission")]
        public string newdestinationstation { get; private set; }

        [PublicAPI("The old destination station for the mission")]
        public string olddestinationstation { get; private set; }

        [PublicAPI("The new destination system for the mission")]
        public string newdestinationsystem { get; private set; }

        [PublicAPI("The old destination system for the mission")]
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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class MusicEvent : Event
    {
        public const string NAME = "Music";
        public const string DESCRIPTION = "Triggered when the game music 'mood' changes";
        public const string SAMPLE = "{ \"timestamp\":\"2017-08-24T16:46:15Z\", \"event\":\"Music\", \"MusicTrack\":\"Exploration\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MusicEvent()
        {
            VARIABLES.Add("musictrack", "Possible track names are: NoTrack, MainMenu, CQCMenu, SystemMap, GalaxyMap, GalacticPowers, CQC, DestinationFromHyperspace, DestinationFromSupercruise, Supercruise, Combat_Unknown, Unknown_Encounter, CapitalShip, CombatLargeDogFight, Combat_Dogfight, Combat_SRV, Unknown_Settlement, DockingComputer, Starport, Unknown_Exploration, Exploration. Note: Other music track names may be used in future.");
        }

        public string musictrack { get; private set; }

        public MusicEvent(DateTime timestamp, string musictrack) : base(timestamp, NAME)
        {
            this.musictrack = musictrack;
        }
    }
}

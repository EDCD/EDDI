using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace EddiDataDefinitions
{
    /// <summary>Details for a star system</summary>
    public class StarSystem
    {
        // General information

        public string name { get; set; }
        public long? EDDBID { get; set; } // The ID in EDDB
        public long? EDSMID { get; set; } // The ID in EDSM

        /// <summary>X co-ordinate for this system</summary>
        public decimal? x { get; set; }
        /// <summary>Y co-ordinate for this system</summary>
        public decimal? y { get; set; }
        /// <summary>Z co-ordinate for this system</summary>
        public decimal? z { get; set; }
        /// <summary>Unique 64 bit id value for system</summary>
        public long? systemAddress { get; set; }

        /// <summary>Details of bodies (stars/planets)</summary>
        public List<Body> bodies { get; set; }

        /// <summary>The reserve level applicable to the system's rings</summary>
        public ReserveLevel Reserve { get; set; } = ReserveLevel.None;
        [JsonIgnore]
        public string reserve => (Reserve ?? ReserveLevel.None).localizedName;

        // Populated system data

        public long? population { get; set; } = 0;
        [JsonIgnore]
        public string primaryeconomy => (Economies[0] ?? Economy.None).localizedName;
        public List<Economy> Economies { get; set; } = new List<Economy>() { Economy.None, Economy.None };

        /// <summary>The system's security level</summary>
        public SecurityLevel securityLevel { get; set; } = SecurityLevel.None;
        /// <summary>The system's security level (localized name)</summary>
        [JsonIgnore]
        public string security => (securityLevel ?? SecurityLevel.None).localizedName;

        public string power { get; set; }
        public string powerstate { get; set; }

        [JsonIgnore]
        public string state => (Faction?.presences.FirstOrDefault(p => p.systemName == name)?.FactionState ?? FactionState.None).localizedName;

        // Faction details
        public Faction Faction { get; set; } = new Faction();
        public List<Faction> factions { get; set; }

        [JsonIgnore, Obsolete("Please use Faction instead")]
        public string faction => Faction.name;
        [JsonIgnore, Obsolete("Please use Faction.Allegiance instead")]
        public string allegiance => Faction.allegiance;
        [JsonIgnore, Obsolete("Please use Faction.Government instead")]
        public string government => Faction.government;

        /// <summary>Details of stations</summary>
        public List<Station> stations { get; set; }

        /// <summary>Summary info for stations</summary>
        [JsonIgnore]
        public List<Station> planetarystations => stations.FindAll(s => s.IsPlanetary());

        [JsonIgnore]
        public List<Station> orbitalstations => stations.FindAll(s => !s.IsPlanetary());

        /// <summary> Whether this system requires a permit for visiting </summary>
        public bool requirespermit { get; set; }

        /// <summary> The name of the permit required for visiting this system, if any </summary>
        public string permitname { get; set; }

        // Other data

        /// <summary>Number of visits</summary>
        public int visits;

        /// <summary>Time of last visit</summary>
        public DateTime? lastvisit;

        /// <summary>Time of last visit, expressed as a Unix timestamp in seconds</summary>
        [JsonIgnore]
        public long? lastVisitSeconds => (visits > 1 && lastvisit != null) ? (long?)((DateTime)lastvisit).Subtract(new DateTime(1970, 1, 1)).TotalSeconds : null;

        /// <summary>comment on this starsystem</summary>
        public string comment;

        /// <summary>distance from home</summary>
        public decimal? distancefromhome;

        // Admin - the last time the information present changed
        public long? updatedat;

        // Admin - the last time the data about this system was obtained from remote repository
        public DateTime lastupdated;

        [JsonExtensionData]
        private IDictionary<string, JToken> additionalJsonData;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (Faction == null) { Faction = new Faction(); }
            FactionPresence factionPresence = Faction.presences.FirstOrDefault(p => p.systemName == name) ?? new FactionPresence();
            if (factionPresence.FactionState == null)
            {
                // Convert legacy data
                string name = (string)additionalJsonData?["state"];
                if (name != null)
                {
                    Faction.presences.FirstOrDefault(p => p.systemName == name).FactionState = 
                        FactionState.FromEDName(name ?? "None");
                }
            }
            else
            {
                // get the canonical FactionState object for the given EDName
                factionPresence.FactionState = 
                    FactionState.FromEDName(Faction.presences.FirstOrDefault(p => p.systemName == name)?.FactionState.edname ?? "None");
            }
            additionalJsonData = null;
        }

        public StarSystem()
        {
            bodies = new List<Body>();
            stations = new List<Station>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
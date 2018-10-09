using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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
        public SystemReserveLevel Reserve { get; set; } = SystemReserveLevel.None; // None if no rings are present
        public string reserve => (Reserve ?? SystemReserveLevel.None).localizedName;

        // Populated system data

        public long? population { get; set; } = 0;
        public string primaryeconomy => (economies[0] ?? Economy.None).localizedName;
        public List<Economy> economies { get; set; } = new List<Economy>() { Economy.None, Economy.None };

        /// <summary>The system's security level</summary>
        public SecurityLevel securityLevel { get; set; } = SecurityLevel.None;
        /// <summary>The system's security level (localized name)</summary>
        public string security => (securityLevel ?? SecurityLevel.None).localizedName;

        public string power { get; set; }
        public string powerstate { get; set; }

        [Obsolete("Please use SystemState instead")]
        public string state => systemState.localizedName;
        public State systemState { get; set; } = State.None;

        // Faction details
        [Obsolete("Please use Faction instead")]
        public string faction => Faction.name;
        public Faction Faction { get; set; } = new Faction();
        [Obsolete("Please use Faction.Allegiance instead")]
        public string allegiance => Faction.allegiance;
        [Obsolete("Please use Faction.Government instead")]
        public string government => Faction.government;

        /// <summary>Details of stations</summary>
        public List<Station> stations { get; set; }

        /// <summary>Summary info for stations</summary>
        [JsonIgnore]
        public List<Station> planetarystations => stations.FindAll(s => s.IsPlanetary());

        [JsonIgnore]
        public List<Station> orbitalstations => stations.FindAll(s => !s.IsPlanetary());

        // Other data

        /// <summary>Number of visits</summary>
        public int visits;

        /// <summary>Time of last visit</summary>
        public DateTime? lastvisit;

        /// <summary>Time of last visit, expressed as a Unix timestamp in seconds</summary>
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
            if (systemState == null)
            {
                string name = (string)additionalJsonData?["state"];
                if (name != null)
                {
                    systemState = State.FromEDName(name) ?? State.None;
                }
            }
            else
            {
                // get the canonical SystemState object for the given EDName
                systemState = State.FromEDName(systemState.edname) ?? State.None;
            }
            additionalJsonData = null;
        }

        public StarSystem()
        {
            bodies = new List<Body>();
            stations = new List<Station>();
        }
    }

    public class SystemReserveLevel : ResourceBasedLocalizedEDName<SystemReserveLevel>
    {
        static SystemReserveLevel()
        {
            resourceManager = Properties.SystemReserveLevel.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new SystemReserveLevel(edname);

            None = new SystemReserveLevel("None");
            var Depleted = new SystemReserveLevel("Depleted");
            var Minor = new SystemReserveLevel("Minor");
            var Common = new SystemReserveLevel("Common");
            var Major = new SystemReserveLevel("Major");
            var Pristine = new SystemReserveLevel("Pristine");
        }

        public static readonly SystemReserveLevel None;

        // dummy used to ensure that the static constructor has run, defaulting to "None"
        public SystemReserveLevel() : this("None")
        { }

        private SystemReserveLevel(string edname) : base(edname, edname)
        { }
    }
}
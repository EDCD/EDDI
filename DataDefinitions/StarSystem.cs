﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace EddiDataDefinitions
{
    /// <summary>Details for a star system</summary>
    public class StarSystem
    {
        // General information
        [JsonProperty("name"), JsonRequired]
        public string systemname { get; set; }

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

        /// <summary>Details of bodies (stars/planets/moons), kept sorted by ID</summary>
        [JsonProperty] // Required to deserialize to the private setter
        public ImmutableList<Body> bodies
        {
            get => _bodies;
            private set
            {
                if (value == _bodies)
                {
                    return;
                }
                materialsAvailable = MaterialsOnBodies(value);
                _bodies = value;
            }
        }
        private ImmutableList<Body> _bodies;

        public Body BodyWithID(long? bodyID)
        {
            if (bodyID is null) { return null; }
            Body result = bodies.Find(b => b.bodyId == bodyID);
            return result;
        }

        public void AddOrUpdateBody(Body body)
        {
            var builder = bodies.ToBuilder();
            internalAddOrUpdateBody(body, builder);
            builder.Sort(Body.CompareById);
            bodies = builder.ToImmutable();
        }

        public void AddOrUpdateBodies(IEnumerable<Body> newBodies)
        {
            var builder = bodies.ToBuilder();
            foreach (Body body in newBodies)
            {
                internalAddOrUpdateBody(body, builder);
            }
            builder.Sort(Body.CompareById);
            bodies = builder.ToImmutable();
        }

        private void internalAddOrUpdateBody(Body body, ImmutableList<Body>.Builder builder)
        {
            // although `bodies` is kept sorted by ID, IDs can be null so bodyname should be the unique identifier
            int index = builder.FindIndex(b => b.bodyname == body.bodyname);
            if (index >= 0)
            {
                builder[index] = body;
            }
            else
            {
                builder.Add(body);
            }

            // Update the system reserve level, when appropriate
            if (body.reserveLevel != ReserveLevel.None)
            {
                Reserve = body.reserveLevel;
            }
        }

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

        /// <summary> The powerplay power exerting influence within the system (null if contested)</summary>
        public Power Power { get; set; }

        [JsonIgnore]
        public string power => (Power ?? Power.None).localizedName;

        /// <summary> The state of powerplay within the system </summary>
        public PowerplayState powerState { get; set; }

        [JsonIgnore]
        public string powerstate => (powerState ?? PowerplayState.None).localizedName;

        [JsonIgnore]
        public string state => (Faction?.presences.FirstOrDefault(p => p.systemName == systemname)?.FactionState ?? FactionState.None).localizedName;

        // Faction details
        public Faction Faction { get; set; } = new Faction();
        public List<Faction> factions { get; set; }

        [JsonIgnore, Obsolete("Please use Faction instead")]
        public string faction => Faction.name;
        [JsonIgnore, Obsolete("Please use Faction.Allegiance instead")]
        public string allegiance => Faction.allegiance;
        [JsonIgnore, Obsolete("Please use Faction.Government instead")]
        public string government => Faction.government;

        [JsonIgnore]
        public List<Conflict> conflicts { get; set; }

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

        /// <summary>Types of signals detected within the system</summary>
        [JsonIgnore]
        public List<string> signalsources { get; set; } = new List<string>();

        /// <summary> Whether the system is a "green" system for exploration (containing all FSD synthesis elements) </summary>
        [JsonIgnore]
        public bool isgreen => materialsAvailable.IsSupersetOf(Material.jumponiumElements);

        /// <summary> Whether the system is a "gold" system for exploration (containing all elements available from planetary surfaces) </summary>
        [JsonIgnore]
        public bool isgold => materialsAvailable.IsSupersetOf(Material.surfaceElements);

        /// <summary>Number of visits</summary>
        [JsonIgnore]
        public long estimatedvalue => estimateSystemValue(bodies);

        /// <summary>Number of visits</summary>
        public int visits => visitLog.Count();

        /// <summary>Time of last visit</summary>
        public DateTime? lastvisit => visitLog.LastOrDefault();

        /// <summary>Visit log</summary>
        public SortedSet<DateTime> visitLog { get; set; } = new SortedSet<DateTime>();

        /// <summary>Time of last visit, expressed as a Unix timestamp in seconds</summary>
        [JsonIgnore]
        public long? lastVisitSeconds => lastvisit > DateTime.MinValue ? (long?)((DateTime)lastvisit).Subtract(new DateTime(1970, 1, 1)).TotalSeconds : null;

        /// <summary>comment on this starsystem</summary>
        public string comment;

        /// <summary>distance from home</summary>
        public decimal? distancefromhome;

        /// <summary>Whether a system scan has already been completed for this system in the current play session</summary>
        [JsonIgnore]
        public bool systemScanCompleted;     

        // Not intended to be user facing - materials available within the system
        [JsonIgnore]
        private HashSet<Material> materialsAvailable = new HashSet<Material>();

        private HashSet<Material> MaterialsOnBodies(IEnumerable<Body> bodies)
        {
            HashSet<Material> result = new HashSet<Material>();
            if (bodies == null) { return result; }
            foreach (Body body in bodies)
            {
                if (body?.materials == null) { continue; }
                foreach (MaterialPresence presence in body.materials)
                {
                    result.Add(presence.definition);
                }
            }
            return result;
        }

        // Not intended to be user facing - discoverable bodies as reported by a discovery scan "honk"
        public int discoverableBodies = 0;

        // Not intended to be user facing - the last time the information present changed
        public long? updatedat;

        // Not intended to be user facing - the last time the data about this system was obtained from remote repository
        public DateTime lastupdated;

        // Deprecated properties (preserved for backwards compatibility with Cottle and database stored values)

        // This is a key for legacy json files that cannot be changed without breaking backwards compatibility. 
        [JsonIgnore, Obsolete("Please use systemname instead.")]
        public string name => systemname;

        [JsonExtensionData]
        private IDictionary<string, JToken> additionalJsonData;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            OnFactionDeserialized();

            materialsAvailable = MaterialsOnBodies(bodies);

            additionalJsonData = null;
        }

        private void OnFactionDeserialized()
        {
            if (Faction == null) { Faction = new Faction(); }
            FactionPresence factionPresence = Faction.presences.FirstOrDefault(p => p.systemName == systemname) ?? new FactionPresence();
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
                    FactionState.FromEDName(Faction.presences.FirstOrDefault(p => p.systemName == systemname)?.FactionState.edname ?? "None");
            }
        }

        public StarSystem()
        {
            bodies = ImmutableList.Create<Body>();
            stations = new List<Station>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        private long estimateSystemValue(IList<Body> bodies)
        {
            // Credit to MattG's thread at https://forums.frontier.co.uk/showthread.php/232000-Exploration-value-formulae for scan value formulas

            if (bodies == null || bodies.Count == 0)
            {
                return 0;
            }

            long value = 0;

            // Add the estimated value for each body
            foreach (Body body in bodies)
            {
                value += body.estimatedvalue;
            }

            // Bonus for fully discovering a system
            if (discoverableBodies == bodies.Where(b => b.scanned != null).Count())
            {
                value += discoverableBodies * 1000;

                // Bonus for fully mapping a system
                int mappableBodies = bodies.Where(b => b.bodyType.invariantName != "Star").Count();
                if (mappableBodies == bodies.Where(b => b.mapped != null).Count())
                {
                    value += mappableBodies * 10000;
                }
            }

            return value;
        }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiDataDefinitions
{
    /// <summary>Details for a star system</summary>
    public class StarSystem
    {
        // The ID in EDDB
        public long EDDBID { get; set; }
        public string name { get; set; }
        public long? population { get; set; }
        public string allegiance { get; set; }
        public string government { get; set; }

        [JsonIgnore]
        public string LocalGovernment
        {
            get
            {
                if (government != null && government != "")
                {
                    return Government.FromName(government).LocalName;
                }
                else return null;
            }
        }
        public string faction { get; set; }
        public string primaryeconomy { get; set; }

        [JsonIgnore]
        public string LocalEconomy
        {
            get
            {
                if (primaryeconomy != null && primaryeconomy != "")
                {
                    return Economy.FromName(primaryeconomy).LocalName;
                }
                else return null;
            }
        }
        public string state { get; set; }

        [JsonIgnore]
        public string LocalState
        {
            get
            {
                if (state != null && state != "")
                {
                    return State.FromName(state).LocalName;
                }
                else return null;
            }
        }

        public string security { get; set; }

        [JsonIgnore]
        public string LocalSecurity
        {
            get
            {
                if (security != null && security != "")
                {
                    return SecurityLevel.FromName(security).LocalName;
                }
                else return null;
            }
        }

        public string power { get; set; }
        public string powerstate { get; set; }

        /// <summary>X co-ordinate for this system</summary>
        public decimal? x { get; set; }
        /// <summary>Y co-ordinate for this system</summary>
        public decimal? y { get; set; }
        /// <summary>Z co-ordinate for this system</summary>
        public decimal? z { get; set; }

        /// <summary>Details of stations</summary>
        public List<Station> stations { get; set; }

        /// <summary>Summary info for stations</summary>
        [JsonIgnore]
        public List<Station> planetarystations
        {
            get { return stations.FindAll(s => s.IsPlanetary()); }
        }

        [JsonIgnore]
        public List<Station> orbitalstations
        {
            get { return stations.FindAll(s => !s.IsPlanetary()); }
        }

        /// <summary>Details of bodies (stars/planets)</summary>
        public List<Body> bodies { get; set; }

        /// <summary>Number of visits</summary>
        public int visits;

        /// <summary>Time of last visit</summary>
        public DateTime? lastvisit;

        /// <summary>comment on this starsystem</summary>
        public string comment;

        /// <summary>distance from home</summary>
        public decimal? distancefromhome;

        // Admin - the last time the information present changed
        public long? updatedat;

        // Admin - the last time the data about this system was obtained from remote repository
        public DateTime lastupdated;

        public StarSystem()
        {
            stations = new List<Station>();
            bodies = new List<Body>();
        }
    }
}

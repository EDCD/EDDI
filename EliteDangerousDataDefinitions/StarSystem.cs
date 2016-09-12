using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousDataDefinitions
{
    /// <summary>Details for a star system</summary>
    public class StarSystem
    {
        // The ID in Elite: Dangerous' database
        public long EDID { get; set; }
        // The ID in EDDB
        public long EDDBID { get; set; }
        public string name { get; set; }
        public long? population { get; set; }
        public string allegiance { get; set; }
        public string government { get; set; }
        public string faction { get; set; }
        public string primaryeconomy { get; set; }
        public string state { get; set; }
        public string security { get; set; }
        public string power { get; set; }
        public string powerState { get; set; }

        /// <summary>X co-ordinate for this system</summary>
        public decimal? x { get; set; }
        /// <summary>Y co-ordinate for this system</summary>
        public decimal? y { get; set; }
        /// <summary>Z co-ordinate for this system</summary>
        public decimal? z { get; set; }

        /// <summary>Details of stations</summary>
        public List<Station> stations { get; set; }

        /// <summary>Summary info for stations</summary>
        public List<Station> planetarystations
        {
            get { return stations.FindAll(s => s.IsPlanetary()); }
        }

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

        // Admin - the last time the data about this system was obtained from remote repository
        public DateTime lastupdated;

        public StarSystem()
        {
            stations = new List<Station>();
            bodies = new List<Body>();
        }
    }
}

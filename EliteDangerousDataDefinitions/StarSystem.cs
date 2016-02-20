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
        public string Name { get; set; }
        public long? Population { get; set; }
        public string Allegiance { get; set;  }
        public string Government { get; set; }
        public string Faction { get; set; }
        public string PrimaryEconomy { get; set; }
        public string State { get; set; }
        public string Security { get; set; }
        public string Power { get; set; }
        public string PowerState { get; set; }

        /// <summary>X co-ordinate for this system</summary>
        public decimal? X { get; set; }
        /// <summary>Y co-ordinate for this system</summary>
        public decimal? Y { get; set; }
        /// <summary>Z co-ordinate for this system</summary>
        public decimal? Z { get; set; }

        /// <summary>Details of stations</summary>
        public List<Station> Stations { get; set; }

        public StarSystem()
        {
            Stations = new List<Station>();
        }
    }
}

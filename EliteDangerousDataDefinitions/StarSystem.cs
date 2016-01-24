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
        public long Population { get; set; }
        public string Allegiance { get; set;  }
        public string Government { get; set; }
        public string Faction { get; set; }
        public string PrimaryEconomy { get; set; }
        public string State { get; set; }
        public string Security { get; set; }
        public string Power { get; set; }
        public string PowerState { get; set; }
    }
}

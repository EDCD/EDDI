using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousDataProviderVAPlugin
{
    public class Ship
    {
        public string model { get; set; }
        public string bulkheads { get; set;  }
        public decimal bulkheadsIntegrity { get; set; }
        public string powerPlant { get; set; }
        public decimal powerPlantIntegrity { get; set; }
        public string thrusters { get; set; }
        public int thrustersPriority { get; set; }
        public bool thrustersEnabled { get; set; }
        public decimal thrustersIntegrity { get; set; }
    }
}

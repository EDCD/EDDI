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
        public string frameShiftDrive { get; set; }
        public int frameShiftDrivePriority { get; set; }
        public bool frameShiftDriveEnabled { get; set; }
        public decimal frameShiftDriveIntegrity { get; set; }
        public string lifeSupport { get; set; }
        public int lifeSupportPriority { get; set; }
        public bool lifeSupportEnabled { get; set; }
        public decimal lifeSupportIntegrity { get; set; }
        public string powerDistributor { get; set; }
        public int powerDistributorPriority { get; set; }
        public bool powerDistributorEnabled { get; set; }
        public decimal powerDistributorIntegrity { get; set; }
        public string sensors { get; set; }
        public int sensorsPriority { get; set; }
        public bool sensorsEnabled { get; set; }
        public decimal sensorsIntegrity { get; set; }
        public string fuelTank { get; set; }
    }
}

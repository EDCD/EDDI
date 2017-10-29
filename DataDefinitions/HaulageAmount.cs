using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiDataDefinitions
{
    public class HaulageAmount
    {
        // Definition of the vehicle
        public long missionid { get; set; }
        public int amount { get; set; }

        public HaulageAmount() { }

        public HaulageAmount(HaulageAmount HaulageAmount)
        {
            this.missionid = missionid;
            this.amount = amount;
        }

        public HaulageAmount(long MissionId, int Amount)
        {
            this.missionid = MissionId;
            this.amount = Amount;
        }
    }
}

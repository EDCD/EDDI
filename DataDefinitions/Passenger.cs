using Newtonsoft.Json;
using System;

namespace EddiDataDefinitions
{
    public class Passenger
    {
        public long missionid { get; set; }

        public string type { get; set; }

        public bool vip { get; set; }

        public bool wanted { get; set; }

        public int amount { get; set; }

        public Passenger() { }

        public Passenger(Passenger Passenger)
        {
            this.missionid = missionid;
            this.type = type;
            this.vip = vip;
            this.wanted = wanted;
            this.amount = amount;
        }

        public Passenger(long MissionId, string Type, bool VIP, bool Wanted, int Amount)
        {
            this.missionid = MissionId;
            this.type = Type;
            this.vip = VIP;
            this.wanted = Wanted;
            this.amount = Amount;
        }
    }
}
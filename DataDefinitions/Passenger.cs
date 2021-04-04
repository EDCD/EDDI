using Utilities;

namespace EddiDataDefinitions
{
    public class Passenger
    {
        [PublicAPI]
        public string type { get; set; }

        [PublicAPI]
        public bool vip { get; set; }

        [PublicAPI]
        public bool wanted { get; set; }

        [PublicAPI]
        public int amount { get; set; }

        // Not intended to be user facing

        public long missionid { get; set; }

        public Passenger() { }

        public Passenger(Passenger Passenger)
        {
            this.missionid = Passenger.missionid;
            this.type = Passenger.type;
            this.vip = Passenger.vip;
            this.wanted = Passenger.wanted;
            this.amount = Passenger.amount;
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
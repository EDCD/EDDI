using Utilities;

namespace EddiDataDefinitions
{
    public class Passenger
    {
        [PublicAPI( "the passenger's type" )]
        public string type { get; set; }

        [PublicAPI( "whether the passenger is a VIP" )]
        public bool vip { get; set; }

        [PublicAPI( "whether the passenger is wanted" )]
        public bool wanted { get; set; }

        [PublicAPI( "the number of passengers" )]
        public int amount { get; set; }

        // Not intended to be user facing

        public ulong missionid { get; set; }

        public Passenger() { }

        public Passenger(Passenger Passenger)
        {
            this.missionid = Passenger.missionid;
            this.type = Passenger.type;
            this.vip = Passenger.vip;
            this.wanted = Passenger.wanted;
            this.amount = Passenger.amount;
        }

        public Passenger(ulong MissionId, string Type, bool VIP, bool Wanted, int Amount)
        {
            this.missionid = MissionId;
            this.type = Type;
            this.vip = VIP;
            this.wanted = Wanted;
            this.amount = Amount;
        }
    }
}
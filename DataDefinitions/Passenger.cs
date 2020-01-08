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
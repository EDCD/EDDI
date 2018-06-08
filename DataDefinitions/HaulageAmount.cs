using System;

namespace EddiDataDefinitions
{
    public class HaulageAmount
    {
        public long id { get; set; }
        public string name { get; set; }
        public int amount { get; set; }
        public int depotcollected { get; set; }
        public int depotdelivered { get; set; }
        public DateTime expiry { get; set; }

        public HaulageAmount() { }

        public HaulageAmount(HaulageAmount HaulageAmount)
        {
            this.id = id;
            this.name = name;
            this.amount = amount;
            this.depotcollected = depotcollected;
            this.depotdelivered = depotdelivered;
            this.expiry = expiry;
        }

        public HaulageAmount(long Id, string Name, int Amount, DateTime Expiry)
        {
            this.id = Id;
            this.name = Name;
            this.amount = Amount;
            this.depotcollected = 0;
            this.depotdelivered = 0;
            this.expiry = Expiry;
        }
    }
}
using Newtonsoft.Json;
using System;
using System.Linq;

namespace EddiDataDefinitions
{
    public class HaulageAmount
    {
        public long id { get; set; }

        public string name { get; set; }

        [JsonIgnore]
        public string type => name.Split('_').ElementAtOrDefault(1)?.ToLowerInvariant();

        [JsonIgnore]
        public bool legal => name.Contains("illegal") ? false : true;

        [JsonIgnore]
        public bool wing => name.ToLowerInvariant().Contains("wing");

        public int amount { get; set; }

        public int collected { get; set; }

        public int delivered { get; set; }

        public bool shared { get; set; }

        public string status { get; set; }

        public DateTime expiry { get; set; }

        public HaulageAmount() { }

        public HaulageAmount(HaulageAmount HaulageAmount)
        {
            this.id = id;
            this.name = name;
            this.status = status;
            this.amount = amount;
            this.collected = collected;
            this.delivered = delivered;
            this.expiry = expiry;
            this.shared = shared;
        }

        public HaulageAmount(long Id, string Name, int Amount, DateTime Expiry, bool Shared = false)
        {
            this.id = Id;
            this.name = Name;
            this.status = "Active";
            this.amount = Amount;
            this.expiry = Expiry;
            this.shared = Shared;
        }
    }
}
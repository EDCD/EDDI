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
        public bool legal => name.ToLowerInvariant().Contains("illegal") ? false : true;

        public int amount { get; set; }

        public int collected { get; set; }

        public int delivered { get; set; }

        public DateTime expiry { get; set; }

        public bool shared { get; set; }

        public HaulageAmount() { }

        public HaulageAmount(HaulageAmount HaulageAmount)
        {
            this.id = id;
            this.name = name;
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
            this.amount = Amount;
            this.collected = 0;
            this.delivered = 0;
            this.expiry = Expiry;
            this.shared = Shared;
        }
    }
}
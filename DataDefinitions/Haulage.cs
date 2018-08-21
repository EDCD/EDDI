using Newtonsoft.Json;
using System;
using System.Linq;

namespace EddiDataDefinitions
{
    public class Haulage
    {
        public long missionid { get; set; }

        public string name { get; set; }

        public string typeEDName => name.Split('_').ElementAtOrDefault(1)?.ToLowerInvariant();

        [JsonIgnore, Obsolete("Please use localizedName or invariantName")]
        public string type => MissionType.FromEDName(typeEDName)?.localizedName;

        public string status { get; set; }

        public string originsystem { get; set; }

        [JsonIgnore]
        public bool legal => !name.ToLowerInvariant().Contains("illegal");

        [JsonIgnore]
        public bool wing => name.ToLowerInvariant().Contains("wing");

        public int amount { get; set; }

        public int remaining { get; set; }

        public int collected { get; set; }

        public int delivered { get; set; }

        public DateTime? expiry { get; set; }

        public bool shared { get; set; }

        public Haulage() { }

        public Haulage(Haulage Haulage)
        {
            this.missionid = missionid;
            this.name = name;
            this.originsystem = originsystem;
            this.status = status;
            this.amount = amount;
            this.remaining = remaining;
            this.collected = collected;
            this.delivered = delivered;
            this.expiry = expiry;
            this.shared = shared;
        }

        public Haulage(long MissionId, string Name, string OriginSystem, int Amount, DateTime? Expiry, bool Shared = false)
        {
            this.missionid = MissionId;
            this.name = Name;
            this.originsystem = OriginSystem;
            this.status = "Active";
            this.amount = Amount;
            this.remaining = Amount;
            this.collected = 0;
            this.delivered = 0;
            this.expiry = Expiry;
            this.shared = Shared;
        }
    }
}
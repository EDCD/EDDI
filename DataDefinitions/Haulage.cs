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

        public long startmarketid { get; set; }

        public long endmarketid { get; set; }

        public int collected { get; set; }

        public int delivered { get; set; }

        public DateTime? expiry { get; set; }

        public bool shared { get; set; }

        public Haulage() { }

        public Haulage(Haulage haulage)
        {
            missionid = haulage.missionid;
            name = haulage.name;
            originsystem = haulage.originsystem;
            status = haulage.status;
            amount = haulage.amount;
            startmarketid = haulage.startmarketid;
            endmarketid = haulage.endmarketid;
            remaining = haulage.remaining;
            collected = haulage.collected;
            delivered = haulage.delivered;
            expiry = haulage.expiry;
            shared = haulage.shared;
        }

        public Haulage(long MissionId, string Name, string OriginSystem, int Amount, DateTime? Expiry, bool Shared = false)
        {
            missionid = MissionId;
            name = Name;
            originsystem = OriginSystem;
            status = "Active";
            amount = Amount;
            remaining = Amount;
            expiry = Expiry;
            shared = Shared;
        }
    }
}

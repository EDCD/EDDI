using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiDataDefinitions
{
    public class Haulage
    {
        private static Dictionary<string, string> CHAINED = new Dictionary<string, string>()
        {
            {"clearingthepath", "delivery"},
            {"helpfinishtheorder", "delivery"},
            {"rescuefromthetwins", "salvage"},
            {"rescuethewares", "salvage"}
        };

        public long missionid { get; set; }

        public string name { get; set; }

        public string typeEDName { get; set; }

        [JsonIgnore, Obsolete("Please use localizedName or invariantName")]
        public string type => MissionType.FromEDName(typeEDName)?.localizedName;

        public string status { get; set; }

        public string originsystem { get; set; }

        public string sourcesystem { get; set; }

        public string sourcebody { get; set; }

        [JsonIgnore]
        public bool legal => !name.ToLowerInvariant().Contains("illegal");

        [JsonIgnore]
        public bool wing => name.ToLowerInvariant().Contains("wing");

        public int amount { get; set; }

        public int remaining { get; set; }

        public int need { get; set; }

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
            typeEDName = haulage.typeEDName;
            originsystem = haulage.originsystem;
            sourcesystem = haulage.sourcesystem;
            sourcebody = haulage.sourcebody;
            status = haulage.status;
            amount = haulage.amount;
            startmarketid = haulage.startmarketid;
            endmarketid = haulage.endmarketid;
            remaining = haulage.remaining;
            need = haulage.need;
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
            need = Amount;
            expiry = Expiry;
            shared = Shared;

            // Mechanism for identifying chained delivery and 'welcome' missions
            typeEDName = Name.Split('_').ElementAtOrDefault(1)?.ToLowerInvariant();
            if (typeEDName != null && CHAINED.TryGetValue(typeEDName, out string value))
            {
                typeEDName = value;
            }
            else if (typeEDName == "ds" || typeEDName == "rs" || typeEDName == "welcome")
            {
                typeEDName = Name.Split('_').ElementAt(2)?.ToLowerInvariant();
            }
        }
    }
}

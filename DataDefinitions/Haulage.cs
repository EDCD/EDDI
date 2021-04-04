using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

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

        [PublicAPI]
        public long missionid { get; set; }

        [PublicAPI]
        public string name { get; set; }

        public string typeEDName { get; set; }

        [PublicAPI, JsonIgnore, Obsolete("Please use localizedName or invariantName")]
        public string type => MissionType.FromEDName(typeEDName)?.localizedName;

        [PublicAPI]
        public string status { get; set; }

        [PublicAPI]
        public string originsystem { get; set; }

        [PublicAPI]
        public string sourcesystem { get; set; }

        [PublicAPI]
        public string sourcebody { get; set; }

        [PublicAPI, JsonIgnore]
        public bool legal => !name.ToLowerInvariant().Contains("illegal");

        [JsonIgnore]
        public bool wing => name.ToLowerInvariant().Contains("wing");

        [PublicAPI]
        public int amount { get; set; }

        public int remaining { get; set; }

        [PublicAPI]
        public int need { get; set; }

        public long startmarketid { get; set; }

        public long endmarketid { get; set; }

        [PublicAPI]
        public int collected { get; set; }

        [PublicAPI]
        public int delivered { get; set; }

        [PublicAPI]
        public DateTime? expiry { get; set; }

        [PublicAPI]
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

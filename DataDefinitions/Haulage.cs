using Newtonsoft.Json;
using System;
using System.Linq;

namespace EddiDataDefinitions
{
    public class Haulage
    {
        public long missionid { get; set; }

        public string name { get; set; }

        public string status { get; set; }

        public string originsystem { get; set; }

        [JsonIgnore]
        public string type => name.Split('_').ElementAtOrDefault(1)?.ToLowerInvariant();

        [JsonIgnore]
        public bool legal => name.ToLowerInvariant().Contains("illegal") ? false : true;

        public int amount { get; set; }

        public int depotcollected { get; set; }

        public int depotdelivered { get; set; }

        public DateTime expiry { get; set; }

        public bool shared { get; set; }

        public Haulage() { }

        public Haulage(Haulage Haulage)
        {
            this.missionid = missionid;
            this.name = name;
            this.originsystem = originsystem;
            this.status = status;
            this.amount = amount;
            this.depotcollected = depotcollected;
            this.depotdelivered = depotdelivered;
            this.expiry = expiry;
            this.shared = shared;
        }

        public Haulage(long MissionId, string Name, string OriginSystem, int Amount, DateTime Expiry, bool Shared = false)
        {
            this.missionid = MissionId;
            this.name = Name;
            this.originsystem = OriginSystem;
            this.status = "Active";
            this.amount = Amount;
            this.depotcollected = 0;
            this.depotdelivered = 0;
            this.expiry = Expiry;
            this.shared = Shared;
        }
    }
}
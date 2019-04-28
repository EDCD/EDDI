using Newtonsoft.Json;
using System;

namespace EddiDataDefinitions
{
    public class FactionReport
    {
        public DateTime timestamp { get; set; }

        public bool bounty { get; set; }

        public int shipId { get; set; }

        public string crimeEDName
        {
            get => crimeDef.edname;
            set
            {
                Crime cDef = Crime.FromEDName(value);
                this.crimeDef = cDef;
            }
        }

        // The crime description, localized
        [JsonIgnore]
        public string localizedCrime => (crimeDef ?? Crime.None).localizedName;

        // deprecated crime description (exposed to Cottle and VA)
        [JsonIgnore, Obsolete("Please use localizedCrime instead")]
        public string crime => localizedCrime;

        [JsonIgnore]
        public Crime crimeDef;

        public string system { get; set; }

        public string station { get; set; }

        public string body { get; set; }

        public string victim { get; set; }

        public string victimAllegiance { get; set; }

        public long amount { get; set; }

        public FactionReport() { }

        public FactionReport(FactionReport factionReport)
        {
            bounty = factionReport.bounty;
            shipId = factionReport.shipId;
            crimeDef = factionReport.crimeDef;
            crimeEDName = factionReport.crimeEDName;
            system = factionReport.system;
            station = factionReport.station;
            body = factionReport.body;
            victim = factionReport.victim;
            victimAllegiance = factionReport.victimAllegiance;
            amount = factionReport.amount;
            timestamp = factionReport.timestamp;
        }

        public FactionReport(DateTime Timestamp, bool Bounty, int ShipId, Crime Crime, string System, long Amount)
        {
            timestamp = Timestamp;
            bounty = Bounty;
            shipId = ShipId;
            crimeDef = Crime ?? Crime.None;
            system = System;
            amount = Amount;
        }
    }
}
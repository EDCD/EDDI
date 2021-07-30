using Newtonsoft.Json;
using System;
using Utilities;

namespace EddiDataDefinitions
{
    public class FactionReport
    {
        public DateTime timestamp { get; set; }

        [PublicAPI]
        public bool bounty { get; set; }

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
        [PublicAPI, JsonIgnore, Obsolete("Please use localizedCrime instead")]
        public string crime => localizedCrime;

        [JsonIgnore]
        public Crime crimeDef;

        [PublicAPI]
        public string system { get; set; }

        [PublicAPI]
        public string station { get; set; }

        [PublicAPI]
        public string body { get; set; }

        [PublicAPI]
        public string victim { get; set; }

        public string victimAllegiance { get; set; }

        [PublicAPI]
        public long amount { get; set; }

        public FactionReport() { }

        public FactionReport(FactionReport factionReport)
        {
            bounty = factionReport.bounty;
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

        public FactionReport(DateTime Timestamp, bool Bounty, Crime Crime, string System, long Amount)
        {
            timestamp = Timestamp;
            bounty = Bounty;
            crimeDef = Crime ?? Crime.None;
            system = System;
            amount = Amount;
        }
    }
}
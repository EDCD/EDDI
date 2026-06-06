using Newtonsoft.Json;
using System;
using Utilities;

namespace EddiDataDefinitions
{
    public class FactionReport
    {
        public const string BondClaimType = "bond";
        public const string BountyClaimType = "bounty";

        public DateTime timestamp { get; set; }

        [PublicAPI]
        public bool bounty { get; set; }

        public string crimeEDName
        {
            get => crimeDef.edname;
            set
            {
                var cDef = Crime.FromEDName(value);
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

        private string _claimtype;

        [PublicAPI, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string claimtype
        {
            get => _claimtype ?? (crimeDef == Crime.None ? (bounty ? BountyClaimType : BondClaimType) : null);
            set => _claimtype = value;
        }

        [PublicAPI, JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string claimvehicle { get; set; }

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
            claimtype = factionReport.claimtype;
            claimvehicle = factionReport.claimvehicle;
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

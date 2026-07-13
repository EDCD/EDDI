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

        [PublicAPI("true if the report is for a bounty (false indicates a bond)")]
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
        [PublicAPI( "localized type of crime committed, 'None' when report is a claim" ), JsonIgnore, Obsolete("Please use localizedCrime instead")]
        public string crime => localizedCrime;

        [JsonIgnore]
        public Crime crimeDef;

        private string _claimtype;

        [PublicAPI( "type of voucher claim: 'bond' or 'bounty'" ), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string claimtype
        {
            get => _claimtype ?? (crimeDef == Crime.None ? (bounty ? BountyClaimType : BondClaimType) : null);
            set => _claimtype = value;
        }

        [PublicAPI( "the vehicle in which the voucher was claimed" ), JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string claimvehicle { get; set; }

        [PublicAPI( "the system in which the voucher was claimed" )]
        public string system { get; set; }

        [PublicAPI( "the station near which the voucher was claimed" )]
        public string station { get; set; }

        [PublicAPI( "the body near which the voucher was claimed" )]
        public string body { get; set; }

        [PublicAPI( "the victim of the voucher" )]
        public string victim { get; set; }

        [PublicAPI( "the allegiance of the victim" )]   
        public string victimAllegiance { get; set; }

        [PublicAPI( "the amount of credits associated with the voucher" )]
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

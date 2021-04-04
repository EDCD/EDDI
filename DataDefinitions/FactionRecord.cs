using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Record defines the claims, fines and bounties associated with a faction, along with some additional data
    /// </summary>
    public class FactionRecord : INotifyPropertyChanged
    {
        // The faction associated with the claim, fine, or bounty
        [JsonIgnore]
        private string _faction;

        [PublicAPI]
        public string faction
        {
            get => _faction;
            set
            {
                if (_faction != value)
                {
                    _faction = value;
                    NotifyPropertyChanged("faction");
                }
            }
        }

        [PublicAPI, JsonProperty("allegiance")]
        public string allegiance
        {

            get => Allegiance?.invariantName ?? Superpower.None.invariantName;
            set
            {
                Superpower aDef = Superpower.FromName(value);
                this.Allegiance = aDef;
            }
        }

        [JsonIgnore]
        private Superpower _Allegiance = Superpower.None;

        [JsonIgnore]
        public Superpower Allegiance
        {
            get { return _Allegiance; }
            set { _Allegiance = value; }
        }

        // The home system of the faction
        [JsonIgnore]
        private string _system;

        [PublicAPI]
        public string system
        {
            get => _system;
            set
            {
                if (_system != value)
                {
                    _system = value;
                    NotifyPropertyChanged("system");
                }
            }
        }

        // The home station of the faction
        [JsonIgnore]
        private string _station;

        [PublicAPI]
        public string station
        {
            get => _station;
            set
            {
                if (_station != value)
                {
                    _station = value;
                    NotifyPropertyChanged("station");
                }
            }
        }

        // The total credit value of claims
        [JsonIgnore]
        private long _claims;

        [PublicAPI]
        public long claims
        {
            get => _claims;
            set
            {
                if (_claims != value)
                {
                    _claims = value;
                    NotifyPropertyChanged("claims");
                }
            }
        }

        // The total credit value of fines
        [JsonIgnore]
        private long _fines;

        [PublicAPI]
        public long fines
        {
            get => _fines;
            set
            {
                if (_fines != value)
                {
                    _fines = value;
                    NotifyPropertyChanged("fines");
                }
            }
        }

        // The total credit value of bounties
        [JsonIgnore]
        private long _bounties;

        [PublicAPI]
        public long bounties
        {
            get => _bounties;
            set
            {
                if (_bounties != value)
                {
                    _bounties = value;
                    NotifyPropertyChanged("bounties");
                }
            }
        }

        public List<string> factionSystems { get; set; } = new List<string>();
        public List<string> interstellarBountyFactions { get; set; } = new List<string>();
        public List<FactionReport> factionReports { get; set; } = new List<FactionReport>();

        // All bonds awareded, excluding the discrepancy report
        [PublicAPI, JsonIgnore]
        public List<FactionReport> bondsAwarded => factionReports
            .Where(r => !r.bounty && r.crimeDef == Crime.None)
            .ToList();

        [JsonIgnore]
        public long bondsAmount => bondsAwarded.Sum(r => r.amount);

        // All bounties awarded, excluding the discrepancy report
        [PublicAPI, JsonIgnore]
        public List<FactionReport> bountiesAwarded => factionReports
            .Where(r => r.bounty && r.crimeDef == Crime.None)
            .ToList();

        [JsonIgnore]
        public long bountiesAmount => bountiesAwarded.Sum(r => r.amount);

        // All fines incurred, excluding the discrepancy report
        [PublicAPI, JsonIgnore]
        public List<FactionReport> finesIncurred => factionReports
            .Where(r => !r.bounty && r.crimeDef != Crime.None && r.crimeDef != Crime.Fine)
            .ToList();

        [PublicAPI, JsonIgnore]
        // All bounties incurred, excluding the discrepancy report
        public List<FactionReport> bountiesIncurred => factionReports
            .Where(r => r.bounty && r.crimeDef != Crime.None && r.crimeDef != Crime.Bounty)
            .ToList();

        // Default Constructor
        public FactionRecord() { }

        [JsonConstructor]
        public FactionRecord(string faction)
        {
            this.faction = faction;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
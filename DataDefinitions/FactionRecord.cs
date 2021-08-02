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
        /// <summary> The faction associated with the claim, fine, or bounty </summary>
        [PublicAPI]
        public string faction
        {
            get => _faction;
            set
            {
                if (_faction != value)
                {
                    _faction = value;
                    OnPropertyChanged();
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
        public Superpower Allegiance { get; set; } = Superpower.None;

        /// <summary> The home system of the faction </summary>
        [PublicAPI]
        public string system
        {
            get => _system;
            set
            {
                if (_system != value)
                {
                    _system = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary> The home station of the faction </summary>
        [PublicAPI]
        public string station
        {
            get => _station;
            set
            {
                if (_station != value)
                {
                    _station = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary> The total credit value of claims (bounty vouchers and bonds) (including any discrepancy report) </summary>
        [PublicAPI]
        public long claims
        {
            get => _claims;
            set
            {
                _claims = value;
                OnPropertyChanged();
            }
        }

        /// <summary> The total credit value of fines incurred (including any discrepancy report) </summary>
        [PublicAPI]
        public long fines
        {
            get => _fines;
            set
            {
                _fines = value;
                OnPropertyChanged();
            }
        }

        /// <summary> The total credit value of bounties incurred (including any discrepancy report) </summary>
        [PublicAPI]
        public long bounties
        {
            get => _bounties;
            set
            {
                _bounties = value;
                OnPropertyChanged();
            }
        }

        public List<string> factionSystems { get; set; } = new List<string>();
        public List<string> interstellarBountyFactions { get; set; } = new List<string>();
        public List<FactionReport> factionReports { get; set; } = new List<FactionReport>();

        [JsonIgnore]
        private string _faction;

        [JsonIgnore]
        private string _system;

        [JsonIgnore]
        private string _station;

        [JsonIgnore]
        private long _claims;

        [JsonIgnore]
        private long _fines;

        [JsonIgnore]
        private long _bounties;

        /// <summary> All bond vouchers awarded, excluding the discrepancy report </summary>
        [PublicAPI, JsonIgnore]
        public List<FactionReport> bondsAwarded => factionReports
            .Where(r => !r.bounty && r.crimeDef == Crime.None)
            .ToList();

        [JsonIgnore] 
        public long bondsAmount => bondsAwarded.Sum(r => r.amount);

        /// <summary> All bounty vouchers awarded, including the discrepancy report </summary>
        [PublicAPI, JsonIgnore]
        public List<FactionReport> bountiesAwarded => factionReports
            .Where(r => r.bounty && r.crimeDef == Crime.None)
            .ToList();

        [JsonIgnore] 
        public long bountiesAmount => bountiesAwarded.Sum(r => r.amount);

        /// <summary> All fines incurred, including the discrepancy report </summary>
        [PublicAPI, JsonIgnore]
        public List<FactionReport> finesIncurred => factionReports
            .Where(r => !r.bounty && r.crimeDef != Crime.None)
            .ToList();

        /// <summary> All bounties incurred, including the discrepancy report </summary>
        [PublicAPI, JsonIgnore]
        public List<FactionReport> bountiesIncurred => factionReports
            .Where(r => r.bounty && r.crimeDef != Crime.None)
            .ToList();

        // Default Constructor
        public FactionRecord() { }

        [JsonConstructor]
        public FactionRecord(string faction)
        {
            this.faction = faction;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [JetBrains.Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
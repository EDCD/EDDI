using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

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

        [JsonProperty("allegiance")]
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

        public List<string> factionSystems { get; set; }
        public List<string> interstellarBountyFactions { get; set; }
        public List<FactionReport> factionReports { get; set; }

        [JsonIgnore]
        // All bonds awareded, excluding the discrepancy report
        public List<FactionReport> bondsAwarded => factionReports
            .Where(r => !r.bounty && r.crimeDef == Crime.None)
            .ToList();

        [JsonIgnore]
        public long bondsAmount => bondsAwarded.Sum(r => r.amount);

        [JsonIgnore]
        // All bounties awarded, excluding the discrepancy report
        public List<FactionReport> bountiesAwarded => factionReports
            .Where(r => r.bounty && r.crimeDef == Crime.None)
            .ToList();

        [JsonIgnore]
        public long bountiesAmount => bountiesAwarded.Sum(r => r.amount);

        [JsonIgnore]
        // All fines incurred, excluding the discrepancy report
        public List<FactionReport> finesIncurred => factionReports
            .Where(r => !r.bounty && r.crimeDef != Crime.None && r.crimeDef != Crime.Fine)
            .ToList();

        [JsonIgnore]
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
            factionReports = new List<FactionReport>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}
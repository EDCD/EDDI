using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EddiDataDefinitions
{
    public class Mission : INotifyPropertyChanged
    {
        private static Dictionary<string, string> CHAINED = new Dictionary<string, string>()
        {
            {"clearingthepath", "delivery"},
            {"drawthegeneralout", "assassinate"},
            {"findthepiratelord", "assassinate"},
            {"helpfinishtheorder", "delivery"},
            {"helpwithpreventionmeasures", "massacre"},
            {"miningtoorder", "mining"},
            {"piracyfraud", "delivery"},
            {"planetaryincursions", "scan"},
            {"rampantleadership", "assassinate"},
            {"regainfooting", "assassinate"},
            {"rescuefromthetwins", "salvage"},
            {"rescuethewares", "salvage"},
            {"safetravelling", "passengervip"},
            {"salvagejustice", "assassinate"},
            {"securingmyposition", "passengervip"},
            {"seekingasylum", "assassinate"},
            {"thedead", "special"},
            {"wrongtarget", "assassinate"},
        };

        private static List<string> ORGRETURN = new List<string>()
        {
            "altruism",
            "altruismcredits",
            "assassinate",
            "assassinatewing",
            "collect",
            "collectwing",
            "disable",
            "disablewing",
            "genericpermit1",
            "hack",
            "longdistanceexpedition",
            "massacre",
            "massacrethargoid",
            "massacrewing",
            "mining",
            "miningwing",
            "piracy",
            "rescue",
            "salvage",
            "scan",
            "sightseeing"
        };

        // The mission ID
        [Utilities.PublicAPI]
        public long missionid { get; private set; }

        // The name of the mission
        [Utilities.PublicAPI]
        public string name { get; set; }

        // The localised name of the mission
        [JsonIgnore]
        private string _localisedname;

        [Utilities.PublicAPI]
        public string localisedname 
        {
            get => _localisedname;
            set
            {
                _localisedname = value;
                OnPropertyChanged();
            }
        }
        
        // The type of mission
        public string typeEDName
        {
            get => typeDef.edname;
            set
            {
                MissionType tDef = MissionType.FromEDName(value);
                this.typeDef = tDef;
            }
        }

        [JsonIgnore]
        private MissionType _typeDef;
        [JsonIgnore]
        public MissionType typeDef
        {
            get => _typeDef;
            set
            {
                _typeDef = value;
                OnPropertyChanged("localizedType");
            }
        }

        [JsonIgnore]
        public string localizedType => typeDef?.localizedName ?? "Unknown";

        [Utilities.PublicAPI, JsonIgnore, Obsolete("Please use localizedName or invariantName")]
        public string type => localizedType;

        // Status of the mission
        public string statusEDName
        {
            get => statusDef.edname;
            set
            {
                MissionStatus sDef = MissionStatus.FromEDName(value);
                this.statusDef = sDef;
            }
        }

        [JsonIgnore]
        private MissionStatus _statusDef;
        [JsonIgnore]
        public MissionStatus statusDef
        {
            get => _statusDef;
            set
            {
                _statusDef = value;
                OnPropertyChanged("localizedStatus");
            }
        }

        [JsonIgnore]
        public string localizedStatus => statusDef?.localizedName ?? "Unknown";

        [Utilities.PublicAPI, JsonIgnore, Obsolete("Please use localizedName or invariantName")]
        public string status => localizedStatus;

        // The system in which the mission was accepted
        [Utilities.PublicAPI]
        public string originsystem { get; set; }

        // The station in which the mission was accepted
        [Utilities.PublicAPI]
        public string originstation { get; set; }

        // Mission returns to origin
        [Utilities.PublicAPI]
        public bool originreturn { get; set; }

        [Utilities.PublicAPI]
        public string faction { get; set; }

        // The state of the minor faction
        [JsonProperty("factionstate")]
        public string factionstate
        {

            get => FactionState?.localizedName ?? FactionState.None.localizedName;
            set
            {
                FactionState fsDef = FactionState.FromName(value);
                this.FactionState = fsDef;
            }
        }
        [JsonIgnore]
        private FactionState _FactionState = FactionState.None;
        [JsonIgnore]
        public FactionState FactionState
        {
            get { return _FactionState; }
            set { _FactionState = value; }
        }

        [Utilities.PublicAPI]
        public string influence { get; set; }

        [Utilities.PublicAPI]
        public string reputation { get; set; }

        public bool chained => name.ToLowerInvariant().Contains("chained");

        [Utilities.PublicAPI]
        public bool communal { get; set; }

        [Utilities.PublicAPI]
        public bool legal => !name.ToLowerInvariant().Contains("hack")
            && !name.ToLowerInvariant().Contains("illegal")
            && !name.ToLowerInvariant().Contains("piracy")
            && !name.ToLowerInvariant().Contains("smuggle");

        [Utilities.PublicAPI]
        public bool shared { get; set; }

        [Utilities.PublicAPI]
        public bool wing { get; set; }

        [Utilities.PublicAPI]
        public long? reward { get; set; }

        [Utilities.PublicAPI]
        public string commodity { get; set; }

        [Utilities.PublicAPI]
        public int? amount { get; set; }

        // THe destination system of the mission
        private string _destinationsystem;

        [Utilities.PublicAPI]
        public string destinationsystem
        {
            get
            {
                return _destinationsystem;
            }
            set
            {
                if (_destinationsystem != value)
                {
                    _destinationsystem = value;
                    OnPropertyChanged();
                }
            }
        }

        // The destination station of the mission
        private string _destinationstation;

        [Utilities.PublicAPI]
        public string destinationstation
        {
            get
            {
                return _destinationstation;
            }
            set
            {
                if (_destinationstation != value)
                {
                    _destinationstation = value;
                    OnPropertyChanged();
                }
            }
        }

        // Destination systems for chained missions

        [Utilities.PublicAPI]
        public List<DestinationSystem> destinationsystems { get; set; }

        // Community goal details, if applicable
        public int communalPercentileBand { get; set; }

        public int communalTier { get; set; }
        
        // The mission time remaining
        [JsonIgnore]
        public TimeSpan? timeRemaining => expiry != null && statusEDName == "Active" ? TimeSpanNearestSecond(expiry - DateTime.UtcNow) : null;

        private TimeSpan? TimeSpanNearestSecond(TimeSpan? utcNow)
        {
            if (utcNow is null) { return null; }
            return new TimeSpan(utcNow.Value.Days, utcNow.Value.Hours, utcNow.Value.Minutes, utcNow.Value.Seconds);
        }

        public string passengertypeEDName { get; set; }
        
        [Utilities.PublicAPI, JsonIgnore]
        public string passengertype => PassengerType.FromEDName(passengertypeEDName)?.localizedName;

        [Utilities.PublicAPI]
        public bool? passengerwanted { get; set; }

        [Utilities.PublicAPI]
        public bool? passengervips { get; set; }

        [Utilities.PublicAPI]
        public string target { get; set; }

        [Utilities.PublicAPI]
        public string targetfaction { get; set; }

        public string targetTypeEDName;

        [Utilities.PublicAPI, JsonIgnore]
        public string targettype => TargetType.FromEDName(targetTypeEDName)?.localizedName;

        [Utilities.PublicAPI]
        public DateTime? expiry { get; set; }

        [Utilities.PublicAPI, JsonIgnore]
        public long? expiryseconds => expiry != null ? (long?)Utilities.Dates.fromDateTimeToSeconds((DateTime)expiry) : null;

        [JsonIgnore]
        public bool expiring { get; set; }

        // Default Constructor
        public Mission() { }

        [JsonConstructor]
        // Main Constructor
        public Mission(long MissionId, string Name, DateTime? expiry, MissionStatus Status, bool Shared = false)
        {
            this.missionid = MissionId;
            this.name = Name;
            this.expiry = expiry?.ToUniversalTime();
            this.statusDef = Status;
            this.shared = Shared;
            this.expiring = false;
            destinationsystems = new List<DestinationSystem>();

            // Mechanism for identifying chained, 'welcome', and 'special' missions
            string type = Name.Split('_').ElementAt(1)?.ToLowerInvariant();
            if (type != null && CHAINED.TryGetValue(type, out string value))
            {
                type = value;
            }
            else if (type == "ds" || type == "rs" || type == "welcome")
            {
                type = Name.Split('_').ElementAt(2)?.ToLowerInvariant();
            }
            this.typeDef = MissionType.FromEDName(type);
            this.originreturn = ORGRETURN.Contains(type);
        }

        public void UpdateTimeRemaining()
        {
            OnPropertyChanged(nameof(timeRemaining));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) 
        { 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
        }
    }
}

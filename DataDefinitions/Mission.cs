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
        private static readonly Dictionary<string, string> CHAINED = new Dictionary<string, string>()
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

        private static readonly List<string> ORGRETURN = new List<string>()
        {
            "altruism",
            "altruismcredits",
            "assassinate",
            "assassinatewing",
            "collect",
            "collectwing",
            "deliverywing",
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
        public string name
        {
            get => _name;
            set
            {
                _name = value;
                SetTypes();
            }
        }

        [JsonIgnore]
        private string _name;

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
                GetDestinationStation();
                OnPropertyChanged();
            }
        }

        // The type of mission

        [JsonIgnore]
        public List<MissionType> tagsList { get; set; }

        [Utilities.PublicAPI, JsonIgnore]
        public List<string> invariantTags => tagsList.Select(t => t.invariantName ?? "Unknown").ToList();

        [JsonIgnore]
        public List<string> localizedTags => tagsList.Select(t => t.localizedName ?? "Unknown").ToList();

        [JsonIgnore]
        public string localizedTagsString => string.Join(", ", localizedTags);

        [JsonIgnore]
        public List<string> edTags => tagsList.Select(t => t.edname ?? "Unknown").ToList();

        [Utilities.PublicAPI, JsonIgnore, Obsolete("Please use localizedName or invariantName")]
        public List<string> tags => localizedTags;

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
        public bool originreturn => edTags
            .Any(t => ORGRETURN.Contains(t, StringComparer.InvariantCultureIgnoreCase));

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

        public bool onfoot => name.ToLowerInvariant().Contains("onfoot");

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
        public bool wing => name.ToLowerInvariant().Contains("wing") || onfoot;

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

        private void SetTypes()
        {
            if (string.IsNullOrEmpty(name)) { return; }

            var tidiedName = name.ToLowerInvariant()
                .Replace("altruismcredits", "altruism_credits")
                .Replace("assassinationillegal", "assassinate_illegal")
                .Replace("conflict_civilwar", "conflictcivilwar")
                .Replace("conflict_war", "conflictwar")
                .Replace("massacreillegal", "massacre_illegal")
                .Replace("onslaughtillegal", "onslaught_illegal")
                .Replace("salvageillegal", "salvage_illegal")
                ;

            var elements = tidiedName.Split('_').ToList();

            // Skip various obscure mission type elements that we don't need or that we're representing some other way
            elements.RemoveAll(t =>
                t == "mission" ||
                t == "arriving" ||
                t == "leaving" ||
                t == "name" ||
                t == "bs" ||
                t == "ds" || 
                t == "rs" ||
                t == "mb"
            );

            // Skip faction state elements
            elements.RemoveAll(t => FactionState
                .AllOfThem
                .Select(s => s.edname)
                .Contains(t, StringComparer.InvariantCultureIgnoreCase));

            // Skip government elements
            elements.RemoveAll(t => Government
                .AllOfThem
                .Select(s => s.edname)
                .Contains(t, StringComparer.InvariantCultureIgnoreCase));

            // Skip economy elements
            elements.RemoveAll(t => Economy
                .AllOfThem
                .Select(s => s.edname)
                .Contains(t, StringComparer.InvariantCultureIgnoreCase));

            // Skip passenger elements (we'll fill these using the `Passengers` event)
            elements.RemoveAll(t => PassengerType
                .AllOfThem
                .Select(s => s.edname)
                .Contains(t, StringComparer.InvariantCultureIgnoreCase));

            // Skip numeric elements
            elements.RemoveAll(t => int.TryParse(t, out _));

            // Replace chained mission types with conventional equivalents
            elements.ForEach(e =>
            {
                if (CHAINED.ContainsKey(e)) { e = CHAINED[e]; }
            });

            this.tagsList = new List<MissionType>();
            foreach (var type in elements)
            {
                var typeDef = MissionType.FromEDName(type);
                if (typeDef != null)
                {
                    this.tagsList.Add(typeDef);
                }
            }
        }
        private void GetDestinationStation()
        {
            if (string.IsNullOrEmpty(localisedname) || !string.IsNullOrEmpty(destinationstation)) { return; }

            var settlementNamePrefixes = new List<Tuple<string, string>>
            {
                Tuple.Create("Acquire a sample from ", ""),
                Tuple.Create("Digital Infiltration: Breach the ", " network"),
                Tuple.Create("Exterminate scavengers at ", ""),
                Tuple.Create("Heist: Acquire a sample from ", ""),
                Tuple.Create("Heist: Take a sample from ", ""),
                Tuple.Create("Reactivation: Find a regulator for ", ""),
                Tuple.Create("Reactivation: Turn on power at ", ""),
                Tuple.Create("Restore: Find a regulator and prepare ", ""),
                Tuple.Create("Restore: Prepare ", " for operation"),
                Tuple.Create("Sabotage: Disrupt production at ", ""),
                Tuple.Create("Sabotage: Halt production at ", ""),
                Tuple.Create("Settlement Raid: Exterminate scavengers at ", ""),
                Tuple.Create("Shutdown: Disable power at ", ""),
                Tuple.Create("Shutdown: Switch off power at ", "")
            };

            var tidiedLocalizedName = localisedname
                .Replace("Covert ", "")
                .Replace("Nonviolent ", "")
            ;
            string settlementName = null;

            foreach (var prefixSuffix in settlementNamePrefixes)
            {
                if (tidiedLocalizedName.StartsWith(prefixSuffix.Item1, StringComparison.InvariantCultureIgnoreCase) 
                    && tidiedLocalizedName.EndsWith(prefixSuffix.Item2, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (prefixSuffix.Item1.Length > 0)
                    {
                        tidiedLocalizedName = tidiedLocalizedName
                            .Replace(prefixSuffix.Item1, "");
                    }
                    if (prefixSuffix.Item2.Length > 0)
                    {
                        tidiedLocalizedName = tidiedLocalizedName
                            .Replace(prefixSuffix.Item2, "");
                    }
                    settlementName = tidiedLocalizedName;
                    break;
                }
            }

            if (!string.IsNullOrEmpty(settlementName))
            {
                destinationstation = settlementName;
            }
        }
    }
}

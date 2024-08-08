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
                OnPropertyChanged();
            }
        }
        [JsonIgnore]
        private string _name;

        // The localised name of the mission
        [Utilities.PublicAPI]
        public string localisedname 
        {
            get => _localisedname;
            set
            {
                _localisedname = value;
                destinationstation = !string.IsNullOrEmpty( destinationstation ) 
                    ? destinationstation 
                    : FallbackGetDestinationStation( value );
                targetfaction = !string.IsNullOrEmpty(targetfaction) 
                    ? targetfaction 
                    : FallbackGetTargetFaction( value );
                OnPropertyChanged();
            }
        }
        [JsonIgnore]
        private string _localisedname;

        #region Expiration Data

        [Utilities.PublicAPI]
        public DateTime? expiry { get; set; }

        [Utilities.PublicAPI, JsonIgnore]
        public long? expiryseconds => expiry != null ? (long?)Utilities.Dates.fromDateTimeToSeconds( (DateTime)expiry ) : null;

        [JsonIgnore]
        public bool expiring { get; set; }

        // The mission time remaining
        [JsonIgnore]
        public TimeSpan? timeRemaining => expiry != null ? TimeSpanNearestSecond( expiry - DateTime.UtcNow ) : null;

        private TimeSpan? TimeSpanNearestSecond ( TimeSpan? utcNow )
        {
            if ( utcNow is null )
            { return null; }
            return new TimeSpan( utcNow.Value.Days, utcNow.Value.Hours, utcNow.Value.Minutes, utcNow.Value.Seconds );
        }

        // While we track the time remaining constantly, we loop through each mission
        // and update the displayed time remaining at intervals defined by the Mission Monitor.
        public void UpdateTimeRemaining ()
        {
            OnPropertyChanged( nameof( timeRemaining ) );
        }

        #endregion

        #region Mission Status

        // Status of the mission
        public string statusEDName
        {
            get => statusDef?.edname;
            set
            {
                var sDef = MissionStatus.FromEDName(value);
                this.statusDef = sDef;
            }
        }

        [JsonIgnore]
        public MissionStatus statusDef
        {
            get => _statusDef;
            set
            {
                _statusDef = value;
                if ( value != null && value != MissionStatus.Active && !onfoot )
                {
                    // Missions are time constrained when active, and on-foot missions
                    // continue to be time constrained even after they are claimable.
                    expiry = null;
                }
                OnPropertyChanged( nameof( localizedStatus ) );
            }
        }
        [JsonIgnore]
        private MissionStatus _statusDef;

        [JsonIgnore]
        public string localizedStatus => statusDef?.localizedName ?? "Unknown";

        [Utilities.PublicAPI, JsonIgnore, Obsolete( "Please use localizedName or invariantName" )]
        public string status => localizedStatus;

        [Utilities.PublicAPI]
        public bool shared { get; set; }

        [Utilities.PublicAPI ("Notes you have recorded about the mission.")]
        public string notes
        {
            get => _notes;
            set
            {
                if ( _notes == value ) { return; }
                _notes = value;
                OnPropertyChanged( nameof( notes ) );
            }
        }
        [JsonIgnore]
        private string _notes;

        #endregion

        #region Mission Tags / MetaData

        [ JsonIgnore ] 
        public List<MissionType> tagsList => MissionTypes.FromMissionName( name );

        [Utilities.PublicAPI, JsonIgnore]
        public List<string> invariantTags => tagsList.Select(t => t.invariantName ?? "Unknown").ToList();

        [JsonIgnore, UsedImplicitly]
        public string localizedTagsString => string.Join(", ", tags );

        [JsonIgnore]
        public List<string> edTags => tagsList.Select(t => t.edname ?? "Unknown").ToList();

        [Utilities.PublicAPI, JsonIgnore]
        public List<string> tags => tagsList.Select( t => t.localizedName ?? "Unknown" ).ToList();

        [Utilities.PublicAPI("Obsolete: `type` has been deprecated in favor of tags"), JsonIgnore, Obsolete("`type` has been deprecated in favor of tags")]
        public string type => tags[0];

        [JsonIgnore]
        public bool chained => tagsList.Contains( MissionType.Chained );

        [JsonIgnore]
        public bool onfoot => tagsList.Contains( MissionType.OnFoot );

        [Utilities.PublicAPI]
        public bool communal { get; set; }

        [Utilities.PublicAPI, JsonIgnore]
        public bool legal => !tagsList.Any( t =>
            t == MissionType.Hack ||
            t == MissionType.Illegal ||
            t == MissionType.Piracy ||
            t == MissionType.Smuggle );

        [Utilities.PublicAPI, JsonIgnore] // On-foot missions are always shareable.
        public bool wing => tagsList.Contains( MissionType.Wing ) || onfoot;

        #endregion

        #region Mission Rewards

        [Utilities.PublicAPI]
        public string influence { get; set; }

        [Utilities.PublicAPI]
        public string reputation { get; set; }

        [Utilities.PublicAPI("Credits awarded upon successful completion")]
        public long? reward { get; set; }

        #endregion

        #region Mission Origin and Origin Faction

        // The system in which the mission was accepted
        [Utilities.PublicAPI]
        public string originsystem { get; set; }

        // The station in which the mission was accepted
        [Utilities.PublicAPI]
        public string originstation { get; set; }

        // Mission returns to origin
        [Utilities.PublicAPI, JsonIgnore]
        public bool originreturn => tagsList.Any(t => t.ClaimAtOrigin);

        // Mission delivers to a cargo depot
        [Utilities.PublicAPI, JsonIgnore]
        public bool cargodepot => tagsList.Any(t => t.ClaimAtCargoDepot);

        [Utilities.PublicAPI]
        public string faction { get; set; }

        // The state of the minor faction
        [ JsonIgnore ] 
        public FactionState FactionState => GetFactionState( name );

        private FactionState GetFactionState ( string missionName )
        {
            if ( string.IsNullOrEmpty( missionName ) )
            { return null; }

            // Get the faction state (Boom, Bust, Civil War, etc), if available
            var elements = missionName.Split( '_' );
            for ( var i = 2; i < elements.Length; i++ )
            {
                var element = elements
                    .ElementAtOrDefault(i)?
                    .ToLowerInvariant();

                // Return faction state when present
                var factionState = FactionState
                    .AllOfThem
                    .FirstOrDefault(s => s.edname.ToLowerInvariant() == element);
                if ( factionState != null )
                { return factionState; }
            }

            return null;
        }

        #endregion

        #region Mission Destination

        // The destination system of the mission
        [ Utilities.PublicAPI ]
        public string destinationsystem
        {
            get => _destinationsystem;
            set
            {
                if (_destinationsystem != value)
                {
                    _destinationsystem = value;
                    OnPropertyChanged();
                }
            }
        }
        [JsonIgnore]
        private string _destinationsystem;

        // The destination station of the mission
        [Utilities.PublicAPI]
        public string destinationstation
        {
            get => _destinationstation;
            set
            {
                if (_destinationstation != value)
                {
                    _destinationstation = value;
                    OnPropertyChanged();
                }
            }
        }
        [JsonIgnore]
        private string _destinationstation;

        // Destination systems for chained missions
        [Utilities.PublicAPI]
        public List<NavWaypoint> destinationsystems { get; set; }

        private static string FallbackGetDestinationStation ( string localisedName )
        {
            if ( string.IsNullOrEmpty( localisedName ) )
            { return string.Empty; }

            var tidiedName = localisedName
                .Replace("Covert ", "")
                .Replace("Nonviolent ", "")
                .Replace("Digital Infiltration: ", "")
                .Replace("Heist: ", "")
                .Replace("Reactivation: ", "")
                .Replace("Restore: ", "")
                .Replace("Sabotage: ", "")
                .Replace("Settlement Raid: ", "")
                .Replace("Shutdown: ", "")
                ;

            var prefixesSuffixes = new List<Tuple<string, string>>
            {
                Tuple.Create("Acquire a sample from ", ""),
                Tuple.Create("Breach the ", " network"),
                Tuple.Create("Disable power at ", ""),
                Tuple.Create("Disrupt production at ", ""),
                Tuple.Create("Exterminate scavengers at ", ""),
                Tuple.Create("Find a regulator for ", ""),
                Tuple.Create("Find a regulator and prepare ", ""),
                Tuple.Create("Halt production at ", ""),
                Tuple.Create("Prepare ", " for operation"),
                Tuple.Create("Switch off power at ", ""),
                Tuple.Create("Take a sample from ", ""),
                Tuple.Create("Turn on power at ", "")
            };

            foreach ( var prefixSuffix in prefixesSuffixes )
            {
                if ( tidiedName.StartsWith( prefixSuffix.Item1, StringComparison.InvariantCultureIgnoreCase )
                    && tidiedName.EndsWith( prefixSuffix.Item2, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    if ( prefixSuffix.Item1.Length > 0 )
                    {
                        tidiedName = tidiedName
                            .Replace( prefixSuffix.Item1, "" );
                    }
                    if ( prefixSuffix.Item2.Length > 0 )
                    {
                        tidiedName = tidiedName
                            .Replace( prefixSuffix.Item2, "" );
                    }
                    break;
                }
            }
            return tidiedName;
        }

        #endregion

        #region Community Goal Info

        // Community goal details, if applicable
        public int communalPercentileBand { get; set; }

        public int communalTier { get; set; }

        #endregion

        #region Passenger Data

        public string passengertypeEDName { get; set; }

        [Utilities.PublicAPI, JsonIgnore]
        public string passengertype => PassengerType.FromEDName( passengertypeEDName )?.localizedName;

        [Utilities.PublicAPI]
        public bool? passengerwanted { get; set; }

        [Utilities.PublicAPI]
        public bool? passengervips { get; set; }

        #endregion

        #region Mission Target

        [Utilities.PublicAPI]
        public string target { get; set; }

        [Utilities.PublicAPI]
        public string targetfaction
        {
            get => _targetfaction;
            set
            {
                if ( _targetfaction != value )
                {
                    _targetfaction = value;
                    OnPropertyChanged();
                }
            }
        }
        private string _targetfaction;

        public string targetTypeEDName;

        [Utilities.PublicAPI, JsonIgnore]
        public string targettype => TargetType.FromEDName( targetTypeEDName )?.localizedName;

        private string FallbackGetTargetFaction ( string localisedName )
        {
            if ( string.IsNullOrEmpty( localisedName ) )
            { return string.Empty; }

            var tidiedName = localisedName
                    .Replace("Settlement ", "")
                    .Replace("Massacre: ", "")
                    .Replace("Raid: ", "")
                ;

            var prefixesSuffixes = new List<Tuple<string, string>>
            {
                Tuple.Create("Exterminate ", " members"),
                Tuple.Create("Take out ", " personnel"),
            };

            foreach ( var prefixSuffix in prefixesSuffixes )
            {
                if ( tidiedName.StartsWith( prefixSuffix.Item1, StringComparison.InvariantCultureIgnoreCase )
                     && tidiedName.EndsWith( prefixSuffix.Item2, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    if ( prefixSuffix.Item1.Length > 0 )
                    {
                        tidiedName = tidiedName
                            .Replace( prefixSuffix.Item1, "" );
                    }
                    if ( prefixSuffix.Item2.Length > 0 )
                    {
                        tidiedName = tidiedName
                            .Replace( prefixSuffix.Item2, "" );
                    }
                    break;
                }
            }

            return tidiedName;
        }

        #endregion

        #region Mission Haulage Data

        [Utilities.PublicAPI( "The localized name of the commodity required to complete the mission, as applicable" ), JsonProperty( "commodity" )]
        public string commodity
        {
            get => CommodityDefinition?.localizedName;
            set
            {
                var comDef = CommodityDefinition.FromName(value);
                this.CommodityDefinition = comDef;
            }
        }

        [Utilities.PublicAPI( "The commodity object for the commodity required to complete the mission, as applicable" ), JsonIgnore]
        public CommodityDefinition CommodityDefinition { get; set; }

        [Utilities.PublicAPI( "The localized name of the micro-resource required to complete the mission, as applicable" ), JsonProperty( "microresource" )]
        public string microresource
        {
            get => MicroResourceDefinition?.localizedName;
            set
            {
                var resDef = MicroResource.FromName(value);
                this.MicroResourceDefinition = resDef;
            }
        }

        [Utilities.PublicAPI( "The micro-resource object for the micro-resource required to complete the mission, as applicable" ), JsonIgnore]
        public MicroResource MicroResourceDefinition { get; set; }

        [Utilities.PublicAPI( "The amount of the commodity or micro-resource required to complete the mission." )] 
        public int? amount { get; set; }

        [Utilities.PublicAPI( "The system where the mission cargo has been found, as applicable" )]
        public string sourcesystem { get; set; }

        [Utilities.PublicAPI ( "The body or station where the mission cargo has been found, as applicable" )]
        public string sourcebody { get; set; }

        [Utilities.PublicAPI( "The quantity of mission cargo which has been collected at a cargo depot" )]
        public int collected { get; set; }

        [Utilities.PublicAPI( "The quantity of mission cargo which has been delivered to a cargo depot" )]
        public int delivered { get; set; }

        public int wingCollected { get; set; } // The quantity of mission cargo which has been collected and not delivered by wing members

        public long startmarketid { get; set; } // The cargo depot where mission cargo is collected

        public long endmarketid { get; set; } // The cargo depot where mission cargo is delivered

        #endregion

        // Default Constructor
        public Mission () { }

        [JsonConstructor]
        // Main Constructor
        public Mission(long MissionId, string Name, DateTime? expiry, MissionStatus Status, string notes = null, bool Shared = false)
        {
            this.missionid = MissionId;
            this.name = Name;
            this.expiry = expiry?.ToUniversalTime();
            this.statusDef = Status;
            this.shared = Shared;
            this.notes = notes;
            this.expiring = false;
            destinationsystems = new List<NavWaypoint>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) 
        { 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
        }
    }
}

using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace EddiDataDefinitions
{
    public class Mission : INotifyPropertyChanged
    {
        // The mission ID
        [Utilities.PublicAPI("the numeric ID of the mission")]
        public ulong missionid { get; private set; }

        // The name of the mission
        [Utilities.PublicAPI( "name of mission" )]
        public string name
        {
            get => _name;
            set
            {
                _name = value?
                    .Replace( "$", "" )
                    .Replace( ";", "" );
                OnPropertyChanged();
                NotifyProgressAndClaimabilityChanged();
            }
        }
        [JsonIgnore]
        private string _name;

        // The localised name of the mission
        [Utilities.PublicAPI( "localised name of the mission" )]
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

        [Utilities.PublicAPI("the date and time when the mission expires, as a DateTime object")]
        public DateTime? expiry { get; set; }

        [Utilities.PublicAPI( "amount of seconds remaining before mission expiration" ), JsonIgnore]
        public long? expiryseconds => expiry != null ? (long?)Utilities.Dates.fromDateTimeToSeconds( (DateTime)expiry ) : null;

        // The mission time remaining
        [JsonIgnore]
        public TimeSpan? timeRemaining => expiry != null ? TimeSpanNearestSecond( expiry - DateTime.UtcNow ) : null;

        private static TimeSpan? TimeSpanNearestSecond ( TimeSpan? utcNow )
        {
            if ( utcNow is null ) { return null; }
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
                if ( value == _statusDef ) { return; }
                _statusDef = value;
                OnPropertyChanged( nameof( localizedStatus ) );
                NotifyProgressAndClaimabilityChanged();
            }
        }
        [JsonIgnore]
        private MissionStatus _statusDef;

        [JsonIgnore]
        public string localizedStatus => statusDef?.localizedName ?? "Unknown";

        [Utilities.PublicAPI( "percentage of observed mission progress, from 0 to 100" ), JsonIgnore]
        public decimal progress => CalculateProgress() ?? 0M;

        [Utilities.PublicAPI( "true if mission progress can be objectively measured, including a binary objective" ), JsonIgnore]
        public bool progressKnown => CalculateProgress().HasValue;

        private decimal? CalculateProgress ()
        {
            var currentStatus = statusDef;
            var hasZeroExpiryEvidence = zeroExpiryClaimability;
            var hasRedirectEvidence = redirectClaimability;
            var hasLegacyEvidence = legacyClaimability;

            if ( currentStatus == MissionStatus.Complete || hasZeroExpiryEvidence ) { return 100M; }
            if ( communal ) { return null; }

            var cargo = cargoEvidence;
            if ( UsesOnboardCargoProgress && CargoFullyDelivered( cargo ) ) { return 100M; }
            if ( UsesOnboardCargoProgress && TryGetOnboardProgress( cargo, out var onboard, out var requiredCargo ) )
            {
                return 100M * onboard / requiredCargo;
            }
            if ( hasRedirectEvidence ) { return 100M; }
            if ( TryGetReliableCargoDepotProgress( out var required, out var deliveredQuantity ) )
            {
                return 100M * deliveredQuantity / required;
            }

            if ( hasLegacyEvidence ) { return 100M; }
            if ( tagsList.Contains( MissionType.SightSeeing ) )
            {
                return TryGetSightseeingProgress( GetWaypointEvidence(), out var waypointProgress ) ? waypointProgress : null;
            }

            // Zero describes a pending binary objective, not a fabricated partial measurement.
            return tagsList.Any( tag => tag == MissionType.Courier || tag == MissionType.Scan ||
                tag == MissionType.Assassinate || tag == MissionType.Hack || tag == MissionType.Disable ||
                tag == MissionType.Altruism || tag == MissionType.Passenger || tag == MissionType.Reboot ||
                tag == MissionType.RebootRestore || tag == MissionType.Sabotage || tag == MissionType.Heist ) ? 0M : null;
        }

        [Utilities.PublicAPI( "true if observed evidence indicates that the mission can be claimed" ), JsonIgnore]
        public bool claimable
        {
            get
            {
                var currentStatus = statusDef;
                var isCommunal = communal;
                var contribution = communalContribution;
                var hasZeroExpiryEvidence = zeroExpiryClaimability;
                var hasRedirectEvidence = redirectClaimability;
                var hasLegacyEvidence = legacyClaimability;

                if ( currentStatus == MissionStatus.Failed ) { return false; }
                if ( isCommunal )
                {
                    return currentStatus == MissionStatus.Complete &&
                           ( contribution > 0 || hasLegacyEvidence );
                }
                if ( currentStatus != MissionStatus.Active ) { return false; }
                if ( hasZeroExpiryEvidence ) { return true; }
                var cargo = cargoEvidence;
                if ( UsesOnboardCargoProgress && CargoFullyDelivered( cargo ) ) { return true; }
                if ( UsesOnboardCargoProgress && TryGetOnboardProgress( cargo, out var onboard, out var requiredCargo ) )
                {
                    return ( hasRedirectEvidence || depotCollectionCompleted ) && onboard == requiredCargo;
                }
                if ( hasRedirectEvidence ) { return true; }
                if ( TryGetReliableCargoDepotProgress( out var required, out var deliveredQuantity ) )
                {
                    return tagsList.Contains( MissionType.Delivery )
                        ? deliveredQuantity >= Math.Ceiling( required / 2M )
                        : deliveredQuantity == required;
                }
                return hasLegacyEvidence;
            }
        }

        [JsonIgnore]
        public string localizedProgress
        {
            get
            {
                var currentProgress = CalculateProgress();
                return currentProgress.HasValue
                    ? $"{currentProgress.Value.ToString( "0.#", CultureInfo.CurrentCulture )}%"
                    : string.Empty;
            }
        }

        [JsonProperty]
        private bool zeroExpiryClaimability;

        [JsonProperty]
        private bool redirectClaimability;

        [JsonProperty]
        private bool legacyClaimability;

        [JsonProperty] private MissionCargoEvidence cargoEvidence;
        [JsonProperty] private DateTime? cargoChangedAt;
        [JsonProperty] private DateTime? cargoRedirectObservedAt;
        [JsonProperty] private bool depotCollectionCompleted;
        [JsonProperty] private MissionWaypointEvidence waypointEvidence;
        [JsonProperty] private bool communalProgressObserved;

        [JsonIgnore]
        public bool communityGoalProgressKnown => communal && ( communalProgressObserved || communalContribution > 0 ) &&
            communalContribution >= 0 && communalPercentileBand is >= 0 and <= 100;

        public void ObserveCommunityGoalProgress ( long contribution, int percentileBand )
        {
            if ( contribution < 0 || percentileBand is < 0 or > 100 ) { return; }
            _communalContribution = contribution;
            _communalPercentileBand = percentileBand;
            communalProgressObserved = true;
            OnPropertyChanged( nameof( communalContribution ) );
            OnPropertyChanged( nameof( communalPercentileBand ) );
            NotifyProgressAndClaimabilityChanged();
        }

        public void InitializeSightseeingWaypoints ( IEnumerable<string> systems )
        {
            if ( !tagsList.Contains( MissionType.SightSeeing ) || waypointEvidence != null ) { return; }
            waypointEvidence = new MissionWaypointEvidence { systems = systems?.Select( s => s?.Trim() ).ToArray() ?? [ ] };
            NotifyProgressAndClaimabilityChanged();
        }

        private MissionWaypointEvidence GetWaypointEvidence () => waypointEvidence ?? new MissionWaypointEvidence
        {
            systems = destinationsystems?.Select( s => s?.systemName ).ToArray() ?? [ ],
            scannedSystems = destinationsystems?.Where( s => s?.visited == true ).Select( s => s.systemName ).ToArray() ?? [ ]
        };

        private static bool TryGetSightseeingProgress ( MissionWaypointEvidence evidence, out decimal value )
        {
            value = 0M;
            var systems = evidence.systems;
            var scanned = evidence.scannedSystems;
            // The journal supplies systems, not beacon identities. Repeated systems cannot be distinguished reliably.
            if ( systems is null || systems.Length == 0 || systems.Any( string.IsNullOrWhiteSpace ) ||
                 systems.Distinct( StringComparer.OrdinalIgnoreCase ).Count() != systems.Length || scanned is null ||
                 scanned.Distinct( StringComparer.OrdinalIgnoreCase ).Count() != scanned.Length ||
                 scanned.Any( s => !systems.Contains( s, StringComparer.OrdinalIgnoreCase ) ) ) { return false; }
            value = 100M * scanned.Length / systems.Length;
            return true;
        }

        public bool ObserveSightseeingStop ( DateTime timestamp, string systemName, ulong? systemAddress )
        {
            if ( communal || statusDef != MissionStatus.Active || !tagsList.Contains( MissionType.SightSeeing ) ) { return false; }
            var evidence = GetWaypointEvidence();
            if ( !TryGetSightseeingProgress( evidence, out _ ) ||
                 ( evidence.observedAt is DateTime previous && timestamp < previous ) ) { return false; }
            var expected = evidence.systems.FirstOrDefault( s => !evidence.scannedSystems.Contains( s, StringComparer.OrdinalIgnoreCase ) );
            if ( expected is null || !string.Equals( expected, systemName, StringComparison.OrdinalIgnoreCase ) ) { return false; }
            var resolved = destinationsystems?.FirstOrDefault( s => s != null &&
                string.Equals( s.systemName, expected, StringComparison.OrdinalIgnoreCase ) );
            if ( resolved?.systemAddress is > 0 && systemAddress != resolved.systemAddress ) { return false; }
            waypointEvidence = evidence with { scannedSystems = [ .. evidence.scannedSystems, expected ], observedAt = timestamp };
            if ( resolved != null ) { resolved.visited = true; }
            NotifyProgressAndClaimabilityChanged();
            return true;
        }

        [JsonIgnore]
        public string nextSightseeingSystem
        {
            get
            {
                var evidence = GetWaypointEvidence();
                if ( !TryGetSightseeingProgress( evidence, out _ ) ) { return null; }
                return evidence.systems?.FirstOrDefault( s =>
                    !evidence.scannedSystems.Contains( s, StringComparer.OrdinalIgnoreCase ) );
            }
        }

        [JsonIgnore]
        public bool UsesOnboardCargoProgress => !tagsList.Contains( MissionType.Delivery ) && tagsList.Any( tag =>
            tag == MissionType.Salvage || tag == MissionType.Rescue || tag == MissionType.Smuggle );

        public void ObserveCargoRequirement ( DateTime timestamp )
        {
            if ( amount is not > 0 ) { return; }
            cargoEvidence = new MissionCargoEvidence { required = amount, requiredAt = timestamp, collected = 0,
                collectedAt = timestamp };
            cargoChangedAt = timestamp;
        }

        public bool ObserveCargoChange ( DateTime timestamp, bool collectedOne = false )
        {
            if ( cargoChangedAt is DateTime previous && timestamp < previous ) { return false; }
            cargoChangedAt = timestamp;
            if ( collectedOne && cargoEvidence is { collected: int knownCollected } cargo &&
                 ( cargo.collectedAt is null || timestamp >= cargo.collectedAt ) &&
                 ( cargo.onboardAt is null || timestamp > cargo.onboardAt ) )
            {
                cargoEvidence = cargo with { collected = knownCollected < cargo.required ? knownCollected + 1 : null,
                    collectedAt = timestamp };
            }
            return true;
        }

        public bool CanObserveCargoDepot ( DateTime timestamp, int required, int collectedQuantity, int deliveredQuantity,
            DateTime? fallbackTimestamp = null ) =>
            required > 0 && collectedQuantity >= 0 && collectedQuantity <= required && deliveredQuantity >= 0 &&
            deliveredQuantity <= required && ( !tagsList.Contains( MissionType.Delivery ) || deliveredQuantity <= collectedQuantity ) &&
            ( ( cargoEvidence?.deliveredAt ?? fallbackTimestamp ) is not DateTime previous || timestamp >= previous );

        public bool ObserveCargoDepot ( DateTime timestamp, int required, int collectedQuantity, int deliveredQuantity )
        {
            if ( !CanObserveCargoDepot( timestamp, required, collectedQuantity, deliveredQuantity ) )
            {
                return false;
            }
            var cargo = cargoEvidence ?? new MissionCargoEvidence();
            cargoEvidence = cargo with { required = required, requiredAt = timestamp, collected = collectedQuantity,
                collectedAt = timestamp, delivered = deliveredQuantity, deliveredAt = timestamp };
            amount = required;
            collected = collectedQuantity;
            delivered = deliveredQuantity;
            depotCollectionCompleted = collectedQuantity == required;
            ObserveCargoChange( timestamp );
            NotifyProgressAndClaimabilityChanged();
            return true;
        }

        public bool ObserveOnboardCargo ( DateTime timestamp, int quantity )
        {
            var required = amount;
            var cargo = cargoEvidence ?? new MissionCargoEvidence();
            if ( required is not > 0 || quantity < 0 || quantity > required ||
                 ( cargoChangedAt is DateTime changedAt && timestamp < changedAt ) ||
                 ( cargo.requiredAt is DateTime requiredAt && timestamp < requiredAt ) ||
                 ( cargo.onboardAt is DateTime previous && timestamp < previous ) ||
                 ( cargo.onboardAt == timestamp && cargo.onboard == quantity && cargo.onboardRequired == required ) )
            {
                return false;
            }
            cargoEvidence = cargo with { required = required, requiredAt = cargo.requiredAt ?? timestamp,
                onboard = quantity, onboardRequired = required, onboardAt = timestamp };
            NotifyProgressAndClaimabilityChanged();
            return true;
        }

        public bool ObserveCargoRedirect ( DateTime timestamp, bool qualifies )
        {
            if ( cargoRedirectObservedAt is DateTime previous && timestamp < previous ) { return false; }
            cargoRedirectObservedAt = timestamp;
            ObserveCargoChange( timestamp );
            return SetRedirectClaimability( qualifies );
        }

        public bool SetZeroExpiryClaimability ( bool value ) =>
            SetClaimabilityEvidence( ref zeroExpiryClaimability, value );

        public bool SetRedirectClaimability ( bool value ) =>
            SetClaimabilityEvidence( ref redirectClaimability, value );

        public bool SetLegacyClaimability ( bool value ) =>
            SetClaimabilityEvidence( ref legacyClaimability, value );

        public bool HasReliableCargoDepotProgress () =>
            TryGetReliableCargoDepotProgress( out _, out _ );

        private bool TryGetReliableCargoDepotProgress ( out int required, out int deliveredQuantity )
        {
            var cargo = cargoEvidence;
            var requiredQuantity = cargo?.deliveredAt is null ? amount : cargo.required;
            deliveredQuantity = cargo?.deliveredAt is null ? delivered : cargo.delivered.GetValueOrDefault();
            required = requiredQuantity.GetValueOrDefault();
            return cargodepot && requiredQuantity.HasValue && required > 0 && deliveredQuantity >= 0 &&
                   deliveredQuantity <= required;
        }

        private static bool CargoFullyDelivered ( MissionCargoEvidence cargo ) =>
            cargo?.required > 0 && cargo.deliveredAt != null && cargo.delivered == cargo.required;

        private static bool TryGetOnboardProgress ( MissionCargoEvidence cargo, out int onboard, out int required )
        {
            onboard = cargo?.onboard ?? -1;
            required = cargo?.onboardRequired ?? 0;
            return cargo?.onboardAt != null && required > 0 && onboard >= 0 && onboard <= required;
        }

        private bool SetClaimabilityEvidence ( ref bool evidence, bool value )
        {
            if ( evidence == value ) { return false; }
            evidence = value;
            NotifyProgressAndClaimabilityChanged();
            return true;
        }

        private void NotifyProgressAndClaimabilityChanged ()
        {
            OnPropertyChanged( nameof( progress ) );
            OnPropertyChanged( nameof( progressKnown ) );
            OnPropertyChanged( nameof( localizedProgress ) );
            OnPropertyChanged( nameof( communityGoalProgressKnown ) );
            OnPropertyChanged( nameof( claimable ) );
        }

        [Utilities.PublicAPI( "localized status (active, complete, failed) of the mission" ), JsonIgnore, Obsolete( "Please use localizedName or invariantName" )]
        public string status => localizedStatus;

        [Utilities.PublicAPI("true if the mission is shared")]
        public bool shared { get; set; }

        [Utilities.PublicAPI ("notes you have recorded about the mission.")]
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

        [Utilities.PublicAPI( "invariant tags (altruism, delivery, massacre, etc) describing the mission, as a list" ), JsonIgnore]
        public List<string> invariantTags => tagsList.Select(t => t.invariantName ?? "Unknown").ToList();

        [JsonIgnore, UsedImplicitly]
        public string localizedTagsString => string.Join(", ", tags );

        [JsonIgnore]
        public List<string> edTags => tagsList.Select(t => t.edname ?? "Unknown").ToList();

        [Utilities.PublicAPI( "localized tags (altruism, delivery, massacre, etc) describing the mission, as a list" ), JsonIgnore]
        public List<string> tags => tagsList.Select( t => t.localizedName ?? "Unknown" ).ToList();

        [Utilities.PublicAPI("Obsolete: `type` has been deprecated in favor of tags"), JsonIgnore, Obsolete("`type` has been deprecated in favor of tags")]
        public string type => tags[0];

        [JsonIgnore]
        public bool chained => tagsList.Contains( MissionType.Chained );

        [JsonIgnore]
        public bool onfoot => tagsList.Contains( MissionType.OnFoot );

        [Utilities.PublicAPI( "true if the mission is a community goal" )]
        public bool communal
        {
            get => _communal;
            set
            {
                if ( value == _communal ) { return; }
                _communal = value;
                NotifyProgressAndClaimabilityChanged();
            }
        }
        [JsonIgnore] private bool _communal;

        [Utilities.PublicAPI( "true if the mission is legal" ), JsonIgnore]
        public bool legal => !tagsList.Any( t =>
            t == MissionType.Hack ||
            t == MissionType.Illegal ||
            t == MissionType.Piracy ||
            t == MissionType.Smuggle );

        [Utilities.PublicAPI( "true if the mission allows wing-mates" ), JsonIgnore] // On-foot missions are always shareable.
        public bool wing => tagsList.Contains( MissionType.Wing ) || onfoot;

        #endregion

        #region Mission Rewards

        [Utilities.PublicAPI( "faction influence gained upon successful completion" )]
        public string influence { get; set; }

        [Utilities.PublicAPI( "faction reputation gained upon successful completion" )]
        public string reputation { get; set; }

        [Utilities.PublicAPI("Credits awarded upon successful completion")]
        public long? reward { get; set; }

        #endregion

        #region Mission Origin and Origin Faction

        // The system in which the mission was accepted
        [Utilities.PublicAPI( "the system in which the mission was accepted" )]
        public string originsystem { get; set; }

        // The station in which the mission was accepted
        [Utilities.PublicAPI( "the station in which the mission was accepted" )]
        public string originstation { get; set; }

        // Mission returns to origin
        [Utilities.PublicAPI( "true if the commander must return to origin to complete the mission" ), JsonIgnore]
        public bool originreturn => tagsList.Any(t => t.ClaimAtOrigin);

        // Mission delivers to a cargo depot
        [Utilities.PublicAPI( "true if the mission delivers to a cargo depot" ), JsonIgnore]
        public bool cargodepot => tagsList.Any(t => t.ClaimAtCargoDepot);

        [Utilities.PublicAPI( "the faction sponsoring the mission" )]
        public string faction { get; set; }

        // The state of the minor faction
        [ JsonIgnore ] 
        public FactionState FactionState => GetFactionState( name );

        private static FactionState GetFactionState ( string missionName )
        {
            if ( string.IsNullOrEmpty( missionName ) ) { return null; }

            // Get the faction state (Boom, Bust, Civil War, etc), if available
            var elements = missionName.Split( '_' );
            for ( var i = 2; i < elements.Length; i++ )
            {
                var element = elements
                    .ElementAtOrDefault(i);

                // Return faction state when present
                var factionState = FactionState
                    .AllOfThem
                    .FirstOrDefault(s =>
                        string.Equals( s.edname, element, StringComparison.OrdinalIgnoreCase ) );
                if ( factionState != null ) { return factionState; }
            }

            return null;
        }

        #endregion

        #region Mission Destination

        // The destination system of the mission
        [ Utilities.PublicAPI( "destination system of the mission (if applicable)" ) ]
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
        [Utilities.PublicAPI( "destination station of the mission (if applicable)" )]
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
        [Utilities.PublicAPI( "list of destination systems for missions with multiple destinations" )]
        public List<NavWaypoint> destinationsystems
        {
            get => _destinationsystems;
            set { _destinationsystems = value; NotifyProgressAndClaimabilityChanged(); }
        }
        [JsonIgnore] private List<NavWaypoint> _destinationsystems;

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
                    return tidiedName;
                }
            }
            return null;
        }

        #endregion

        #region Community Goal Info

        // Community goal details, if applicable
        [Utilities.PublicAPI( "(for community goals) the percentile band for your current contributions" )]

        public int communalPercentileBand
        {
            get => _communalPercentileBand;
            set
            {
                if ( value == _communalPercentileBand ) { return; }
                _communalPercentileBand = value;
                OnPropertyChanged();
                NotifyProgressAndClaimabilityChanged();
            }
        }
        [JsonIgnore] private int _communalPercentileBand;

        [Utilities.PublicAPI( "(for community goals) the current award tier" )]
        public int communalTier { get; set; }

        [Utilities.PublicAPI( "(for community goals) the amount contributed" )]
        public long communalContribution
        {
            get => _communalContribution;
            set
            {
                if ( value == _communalContribution ) { return; }
                _communalContribution = value;
                OnPropertyChanged();
                NotifyProgressAndClaimabilityChanged();
            }
        }
        [JsonIgnore] private long _communalContribution;

        #endregion

        #region Passenger Data

        public string passengertypeEDName { get; set; }

        [Utilities.PublicAPI( "localized type of passengers (celebrity, doctor, etc) in the mission (if applicable)" ), JsonIgnore]
        public string passengertype => PassengerType.FromEDName( passengertypeEDName )?.localizedName;

        [Utilities.PublicAPI( "true if the passengers are wanted" )]
        public bool? passengerwanted { get; set; }

        [Utilities.PublicAPI( "true if the passengers are VIPs" )]
        public bool? passengervips { get; set; }

        #endregion

        #region Mission Target

        [Utilities.PublicAPI( "the target of the mission (if applicable)" )]
        public string target { get; set; }

        [Utilities.PublicAPI( "name of the faction of the target of the mission (if applicable)" )]
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

        [Utilities.PublicAPI( "localized type of the target (civilian, pirate, etc) of the mission (if applicable)" ), JsonIgnore]
        public string targettype => TargetType.FromEDName( targetTypeEDName )?.localizedName;

        private static string FallbackGetTargetFaction ( string localisedName )
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
                    return tidiedName;
                }
            }

            return null;
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
        public int? amount
        {
            get => _amount;
            set
            {
                if ( value == _amount ) { return; }
                _amount = value;
                NotifyProgressAndClaimabilityChanged();
            }
        }
        [JsonIgnore] private int? _amount;

        [Utilities.PublicAPI( "The system where the mission cargo has been found, as applicable" )]
        public string sourcesystem { get; set; }

        [Utilities.PublicAPI ( "The body or station where the mission cargo has been found, as applicable" )]
        public string sourcebody { get; set; }

        [Utilities.PublicAPI( "The quantity of mission cargo which has been collected at a cargo depot" )]
        public int collected { get; set; }

        [Utilities.PublicAPI( "The quantity of mission cargo which has been delivered to a cargo depot" )]
        public int delivered
        {
            get => _delivered;
            set
            {
                if ( value == _delivered ) { return; }
                _delivered = value;
                NotifyProgressAndClaimabilityChanged();
            }
        }
        [JsonIgnore] private int _delivered;

        public int wingCollected { get; set; } // The quantity of mission cargo which has been collected and not delivered by wing members

        public long startmarketid { get; set; } // The cargo depot where mission cargo is collected

        public long endmarketid { get; set; } // The cargo depot where mission cargo is delivered

        #endregion

        // Default Constructor
        public Mission () { }

        [JsonConstructor]
        // Main Constructor
        public Mission(ulong MissionId, string Name, DateTime? expiry, MissionStatus Status, string notes = null, bool Shared = false)
        {
            this.missionid = MissionId;
            this.name = Name;
            this.expiry = expiry?.ToUniversalTime();
            this.statusDef = Status;
            this.shared = Shared;
            this.notes = notes;
            destinationsystems = [ ];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) 
        { 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
        }
    }
}

using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>Details for a star system</summary>
    public class StarSystem
    {
        // General information

        [ Utilities.PublicAPI( "The name of the star system" ), JsonProperty( "name" ), JsonRequired ]
        public string systemname { get; set; }

        // This is a key for legacy json files that cannot be changed without breaking backwards compatibility. 
        [JsonIgnore, Obsolete( "Please use systemname instead." )]
        public string name => systemname;

        [Utilities.PublicAPI( "The unique 64 bit ID for the star system" ), JsonProperty, JsonRequired]
        public ulong systemAddress { get; set; }

        [Utilities.PublicAPI( "The 'X' coordinates of the star system" ) ]
        public decimal? x { get; set; }

        [Utilities.PublicAPI( "The 'Y' coordinates of the star system" )]
        public decimal? y { get; set; }

        [Utilities.PublicAPI( "The 'Z' coordinates of the star system" )]
        public decimal? z { get; set; }

        #region Exploration Properties

        // Details of bodies (stars/planets/moons), kept sorted by ID
        [Utilities.PublicAPI( "The star system's bodies (stars/planets/moons), as an array of Body objects" ), JsonProperty] // Required to deserialize to the private setter
        public ImmutableList<Body> bodies
        {
            get => _bodies;
            private set { _bodies = value; OnPropertyChanged(); }
        }
        private ImmutableList<Body> _bodies;

        // Discoverable bodies as reported by a discovery scan "honk"
        [Utilities.PublicAPI( "The total number of discoverable bodies within the system (only available after a discovery scan)" ), JsonProperty( "discoverableBodies" )]
        public int totalbodies
        {
            get => _totalbodies;
            set { _totalbodies = value; OnPropertyChanged(); }
        }
        private int _totalbodies;

        [Utilities.PublicAPI( "The total number of bodies you have scanned within the system" ), JsonIgnore]
        public int scannedbodies => bodies.Count( b => b.scannedDateTime != null );

        [Utilities.PublicAPI( "The total number of bodies you have mapped within the system" ), JsonIgnore]
        public int mappedbodies => bodies.Count( b => b.mappedDateTime != null );

        [Utilities.PublicAPI( "True if a fuel scoop equipped ship can refuel at at least one star in this star system" )]
        public bool scoopable => bodies.Any( b => b.scoopable );

        [Utilities.PublicAPI( "The reserve level applicable to the system's rings, as an object" )]
        public ReserveLevel Reserve
        {
            get => _reserve;
            set { _reserve = value; OnPropertyChanged(); }
        }
        private ReserveLevel _reserve = ReserveLevel.None;

        [Utilities.PublicAPI( "The localized reserve level applicable to the system's rings" ), JsonIgnore]
        public string reserves => ( Reserve ?? ReserveLevel.None ).localizedName;

        [Utilities.PublicAPI( "The estimated exploration value of the star system (including bonuses for fully scanning and mapping)" ), JsonIgnore]
        public long estimatedvalue => estimateSystemValue();

        [Utilities.PublicAPI ("The distance in LY from the commander's home star system"), JsonIgnore]
        public decimal? distancefromhome;

        /// <summary>Whether a system scan has already been completed for this system in the current play session</summary>
        [JsonIgnore]
        public bool systemScanCompleted;

        // Not intended to be user facing - materials available within the system
        [JsonIgnore]
        private HashSet<Material> materialsAvailable => bodies?
            .SelectMany( b => b.materials )
            .Select( m => m.definition )
            .Where( m => m != null )
            .Distinct()
            .OrderByDescending( m => m.Rarity.level )
            .ToHashSet() ?? new HashSet<Material>();

        // Not intended to be user facing - materials available from system bodies
        [Utilities.PublicAPI, JsonIgnore]
        public HashSet<string> surfaceelements => materialsAvailable
            .Select( m => m.localizedName ).ToHashSet();

        [Utilities.PublicAPI( "True if bodies in this star system contain all elements required for FSD synthesis" ), JsonIgnore]
        public bool isgreen => materialsAvailable.IsSupersetOf( Material.jumponiumElements );

        [Utilities.PublicAPI( "True if bodies in this star system contain all elements available from surface prospecting" ), JsonIgnore]
        public bool isgold => materialsAvailable.IsSupersetOf( Material.surfaceElements );

        #endregion

        #region Populated System Properties

        [Utilities.PublicAPI( "The population of the star system" )]
        public long? population { get; set; } = 0;

        [Utilities.PublicAPI( "The localized primary economy in this star system (High Technology, Agriculture, etc)" ), JsonIgnore]
        public string primaryeconomy => ( Economies.FirstOrDefault() ?? Economy.None ).localizedName;

        [Utilities.PublicAPI( "The economies in this star system (High Technology, Agriculture, etc), as objects" ), JsonIgnore]
        public List<Economy> Economies
        {
            get => _economies;
            set { _economies = value; OnPropertyChanged(); }
        }
        private List<Economy> _economies = new List<Economy>();

        [Utilities.PublicAPI( "The security level in the star system, as an object" )]
        public SecurityLevel securityLevel { get; set; } = SecurityLevel.None;

        [Utilities.PublicAPI( "The localized security level in the star system (Low, Medium, High)" ), JsonIgnore]
        public string security => ( securityLevel ?? SecurityLevel.None ).localizedName;

        [ Utilities.PublicAPI( "The powerplay power controlling the star system, if any, as an object." ), JsonIgnore, CanBeNull ]
        public Power Power { get; set; }

        [Utilities.PublicAPI( "The localized powerplay power controlling the star system, if any.  If the star system is `Unoccupied` or `Contested` then this will be empty." ), JsonIgnore]
        public string power => (Power ?? Power.None).localizedName;

        [ Utilities.PublicAPI( "The state of powerplay efforts within the star system, as an object" ) ]
        public PowerplayState powerState
        {
            get => _powerState ?? PowerplayState.Unoccupied;
            set => _powerState = value;
        }
        private PowerplayState _powerState;

        [Utilities.PublicAPI( "The localized state of powerplay efforts within the star system" ), JsonIgnore]
        public string powerstate => powerState.localizedName;

        [Utilities.PublicAPI( "Powerplay powers having star systems with acquisition radii which overlap the star system, less the controlling power, if any, as objects" )]
        public List<Power> NearbyPowers { get; set; } = new List<Power>();

        [Utilities.PublicAPI( "The localized names of powerplay powers having star systems with acquisition radii which overlap the star system, less the controlling power, if any" )]
        public List<string> nearbypowers => NearbyPowers?
            .Select( p => p.localizedName )
            .ToList();

        [Utilities.PublicAPI( "Powerplay powers contesting control of an uncontrolled star system, if any, as objects" )]
        public List<Power> ContestingPowers { get; set; } = new List<Power>();

        [Utilities.PublicAPI( "The localized names of powerplay powers contesting control of an uncontrolled star system, if any" )]
        public List<string> contestingpowers => ContestingPowers?
            .Select( p => p.localizedName )
            .ToList();

        // Faction details
        [Utilities.PublicAPI( "The star system controlling faction, if any, as an object" ), CanBeNull]
        public Faction Faction
        {
            get => _faction;
            set { _faction = value; OnPropertyChanged(); }
        }
        private Faction _faction = new Faction();

        [Utilities.PublicAPI( "The star system's factions, if any, as objects" )]
        public List<Faction> factions
        {
            get => _factions;
            set { _factions = value; OnPropertyChanged(); }
        }
        private List<Faction> _factions;

        [Utilities.PublicAPI( "The name of the star system controlling faction, if any" ), JsonIgnore, Obsolete( "Please use Faction instead" )]
        public string faction => Faction?.name;

        [Utilities.PublicAPI( "The localized superpower alleginace of the star system controlling faction, if any" ), JsonIgnore, Obsolete( "Please use Faction.Allegiance instead" )]
        public string allegiance => ( Faction?.Allegiance ?? Superpower.None ).localizedName;

        [Utilities.PublicAPI( "The localized government of the star system controlling faction, if any" ), JsonIgnore, Obsolete( "Please use Faction.Government instead" )]
        public string government => ( Faction?.Government ?? Government.None ).localizedName;

        [Utilities.PublicAPI( "The state of the star system's controlling faction (Boom, War, etc)" ), JsonIgnore]
        public string state => ( Faction?.presences.FirstOrDefault( p => p.systemAddress == systemAddress )?.FactionState ?? FactionState.None ).localizedName;

        [Utilities.PublicAPI( "Faction conflicts data. Currently only available for recently visited star systems." ), JsonIgnore]
        public List<Conflict> conflicts
        {
            get => _conflicts;
            set { _conflicts = value; OnPropertyChanged(); }
        }
        private List<Conflict> _conflicts;

        [Utilities.PublicAPI( "The star system's stations (as an array of Station objects)" )]
        public ImmutableList<Station> stations
        {
            get => _stations;
            set { _stations = value; OnPropertyChanged(); }
        }
        private ImmutableList<Station> _stations;

        /// <summary>Summary info for stations</summary>
        [Utilities.PublicAPI( "The star system's stations, filtered to only return dockable and permanent planetary stations (array of Station objects)" ), JsonIgnore]
        public List<Station> planetarystations => stations.FindAll( s => ( s.hasdocking ?? false )
                                                                         && s.IsPlanetary() ).ToList();

        [Utilities.PublicAPI( "The star system's stations, filtered to only return dockable and permanent orbital stations (array of Station objects)" ), JsonIgnore]
        public List<Station> orbitalstations => stations.FindAll( s => ( s.hasdocking ?? false )
                                                                       && !s.IsPlanetary()
                                                                       && !s.IsCarrier()
                                                                       && !s.IsMegaShip() ).ToList();

        #endregion

        #region Signals

        /// <summary>Types of signals detected within the system</summary>
        [Utilities.PublicAPI( "(For the current star system only) A list of signals detected within the star system, as objects" ), JsonIgnore]
        public ImmutableList<SignalSource> signalSources
        {
            get
            {
                _signalSources = _signalSources.RemoveAll( s => s.expiry != null && s.expiry < DateTime.UtcNow );
                return _signalSources;
            }
            set
            {
                _signalSources = value;
                OnPropertyChanged();
            }
        }
        private ImmutableList<SignalSource> _signalSources = ImmutableList<SignalSource>.Empty;

        [Utilities.PublicAPI( "(For the current star system only) A localized list of signals detected within the star system" ), JsonIgnore]
        public List<string> signalsources => signalSources.Select( s => s.localizedName ).Distinct().ToList();

        // Filtered by carrier callsign
        [Utilities.PublicAPI( "(For the current star system only) A list of fleet carrier signals detected within the star system" ), JsonIgnore]
        public List<string> carriersignalsources => signalSources
            .Where( s => new Regex( "[[a-zA-Z0-9]{3}-[[a-zA-Z0-9]{3}$" ).IsMatch( s.invariantName )
                && ( s.isStation ?? false ) )
            .Select( s => s.localizedName )
            .ToList();

        #endregion

        #region Visits

        [Utilities.PublicAPI( "The number of visits that the commander has made to this star system" )]
        public int visits => visitLog.Count();

        /// <summary>Time of last visit</summary>
        public DateTime? lastvisit => visitLog.LastOrDefault();

        /// <summary>Visit log</summary>
        public readonly SortedSet<DateTime> visitLog = new SortedSet<DateTime>();

        [Utilities.PublicAPI( "The time that the commander last visited this star system, expressed as a Unix timestamp in seconds" ), JsonIgnore]
        public long? lastVisitSeconds => lastvisit > DateTime.MinValue ? (long?)Dates.fromDateTimeToSeconds( (DateTime)lastvisit ) : null;

        #endregion
        
        [Utilities.PublicAPI("Thargoid war data. Currently only available for recently visited star systems." ), JsonIgnore ]
        public ThargoidWar ThargoidWar
        {
            get => _thargoidWar;
            set { _thargoidWar = value; OnPropertyChanged(); }
        }
        private ThargoidWar _thargoidWar;

        /// <summary> Whether this system requires a permit for visiting </summary>
        [ Utilities.PublicAPI ( "Whether this system requires a permit to enter (as a boolean)" ), JsonIgnore ]
        public bool requirespermit => StarSystemPermits.IsPermitRequired( systemname, x, y, z );
        
        [Utilities.PublicAPI("Any comment the commander has made on the starsystem (via VoiceAttack or EDSM entry)")]
        public string comment;

        // Not intended to be user facing - the last time the information present changed
        [Utilities.PublicAPI]
        public long? updatedat;

        // Not intended to be user facing - the last time the data about this system was obtained from remote repository
        public DateTime lastupdated;

        public StarSystem ()
        {
            bodies = ImmutableList.Create<Body>();
            factions = new List<Faction>();
            stations = ImmutableList.Create<Station>();
        }

        #region Deserialization

        [JsonExtensionData]
        private IDictionary<string, JToken> additionalJsonData;

        [OnDeserialized]
        private void OnDeserialized ( StreamingContext context )
        {
            OnFactionDeserialized();
            additionalJsonData = null;
        }

        private void OnFactionDeserialized ()
        {
            if ( Faction == null )
            { Faction = new Faction(); }
            var factionPresence = Faction.presences.FirstOrDefault(p => p.systemAddress == systemAddress) ?? new FactionPresence();
            if ( factionPresence.FactionState == null )
            {
                // Convert legacy data
                if ( additionalJsonData.TryGetValue( "state", out var fState ) )
                {
                    string factionState = (string)fState;
                    if ( factionState != null )
                    {
                        factionPresence.FactionState = FactionState.FromEDName( factionState );
                    }
                }
            }
            else
            {
                // get the canonical FactionState object for the given EDName
                factionPresence.FactionState = FactionState.FromEDName( Faction.presences
                    .FirstOrDefault( p => p.systemAddress == systemAddress )?.FactionState.edname );
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged ( [CallerMemberName] string propName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propName ) );
        }

        #endregion

        #region Methods

        private readonly object _bodiesLock = new object();
        private readonly object _stationsLock = new object();

        public Body BodyWithID ( long? bodyID )
        {
            if ( bodyID is null )
            { return null; }
            Body result = bodies.Find(b => b.bodyId == bodyID);
            return result;
        }

        public void AddOrUpdateBody ( Body body )
        {
            lock ( _bodiesLock )
            {
            var builder = bodies.ToBuilder();
            internalAddOrUpdateBody( body, builder );
            builder.Sort( Body.CompareById );
            bodies = builder.ToImmutable();
        }
        }

        public void AddOrUpdateBodies ( IEnumerable<Body> newOrUpdatedBodies )
        {
            lock ( _bodiesLock )
        {
            var builder = bodies.ToBuilder();
                foreach ( var newOrUpdatedBody in newOrUpdatedBodies )
            {
                    internalAddOrUpdateBody( newOrUpdatedBody, builder );
            }
            builder.Sort( Body.CompareById );
            bodies = builder.ToImmutable();
        }
        }

        private void internalAddOrUpdateBody ( Body newOrUpdatedBody, ImmutableList<Body>.Builder builder )
        {
            if ( newOrUpdatedBody is null ) { return; }

            int index = builder.FindIndex(b =>
                (b.bodyId != null && newOrUpdatedBody.bodyId != null && b.bodyId == newOrUpdatedBody.bodyId) || // Matching bodyId
                (!string.IsNullOrEmpty(b.bodyname) && !string.IsNullOrEmpty(newOrUpdatedBody.bodyname) && b.bodyname == newOrUpdatedBody.bodyname) || // Matching bodyName
                (b.distance == 0M && b.distance == newOrUpdatedBody.distance)); // Matching distance (for the main entry star only)
            if ( index >= 0 )
            {
                builder[ index ] = PreserveBodyData( builder[ index ], newOrUpdatedBody );
            }
            else
            {
                builder.Add( newOrUpdatedBody );
            }

            // Update the system reserve level, when appropriate
            if ( newOrUpdatedBody.reserveLevel != ReserveLevel.None )
            {
                Reserve = newOrUpdatedBody.reserveLevel;
            }
        }

        public void ClearTemporaryStars ()
        {
            var builder = bodies.ToBuilder();
            var bodiesToRemove = builder
                .Where( b => b.bodyId is null || string.IsNullOrEmpty( b.bodyname ) )
                .ToList();
            builder.RemoveRange( bodiesToRemove );
            builder.Sort( Body.CompareById );
            bodies = builder.ToImmutable();
        }

        public void PreserveBodyData ( List<Body> oldBodies, ImmutableList<Body> newBodies )
        {
            // Update `bodies` with new data, except preserve properties not available via the server
            var newBodyBuilder = newBodies.ToBuilder();
            foreach ( Body oldBody in oldBodies )
            {
                if ( newBodyBuilder.Any( b => b.bodyname == oldBody.bodyname ) )
                {
                    int index = newBodyBuilder.FindIndex(b => b.bodyname == oldBody.bodyname);
                    newBodyBuilder[ index ] = PreserveBodyData( oldBody, newBodyBuilder[ index ] );
                }
                else
                {
                    // `newBodies` did not contain the `oldBody` so we add it here, provided we've
                    // scanned the body ourselves so that we're confident that our old data is accurate. 
                    if ( oldBody.scannedDateTime != null )
                    {
                        newBodyBuilder.Add( oldBody );
                    }
                }
            }
            newBodyBuilder.Sort( Body.CompareById );
            bodies = newBodyBuilder.ToImmutable();
        }

        private static Body PreserveBodyData ( Body oldBody, Body updatedBody )
        {
            if ( ( oldBody.scannedDateTime ?? DateTime.MinValue ) > ( updatedBody.scannedDateTime ?? DateTime.MinValue ) )
            {
                updatedBody.scannedDateTime = oldBody.scannedDateTime;
            }

            if ( oldBody.alreadydiscovered is true &&
                oldBody.alreadydiscovered != updatedBody.alreadydiscovered )
            {
                updatedBody.alreadydiscovered = oldBody.alreadydiscovered;
            }

            if ( ( oldBody.mappedDateTime ?? DateTime.MinValue ) > ( updatedBody.mappedDateTime ?? DateTime.MinValue ) )
            {
                updatedBody.mappedDateTime = oldBody.mappedDateTime;
            }

            if ( oldBody.alreadymapped is true &&
                oldBody.alreadymapped != updatedBody.alreadymapped )
            {
                updatedBody.alreadymapped = oldBody.alreadymapped;
            }

            if ( oldBody.mappedEfficiently &&
                oldBody.mappedEfficiently != updatedBody.mappedEfficiently )
            {
                updatedBody.mappedEfficiently = oldBody.mappedEfficiently;
            }

            if ( oldBody.rings?.Any() ?? false )
            {
                if ( updatedBody.rings is null )
                {
                    updatedBody.rings = new List<Ring>();
                }

                foreach ( var oldRing in oldBody.rings )
                {
                    var newRing = updatedBody.rings.FirstOrDefault(r => r.name == oldRing.name);
                    if ( oldRing.mapped != null )
                    {
                        if ( newRing != null )
                        {
                            newRing.mapped = oldRing.mapped;
                            newRing.hotspots = oldRing.hotspots;
                        }
                        else
                        {
                            // Our data source didn't contain any data about a ring we've scanned.
                            // We add it here because we scanned the ring ourselves and are confident that the data is accurate
                            updatedBody.rings.Add( oldRing );
                        }
                    }
                }
            }
            return updatedBody;
        }

        public void AddOrUpdateStation ( Station newOrUpdatedStation )
        {
            lock ( _stationsLock )
            {
            var builder = stations.ToBuilder();
            internalAddOrUpdateStation( newOrUpdatedStation, builder );
            builder.Sort( ( lhs, rhs ) => Math.Sign( ( lhs.marketId - rhs.marketId ) ?? 0 ) );
            stations = builder.ToImmutable();
        }
        }

        public void AddOrUpdateStations ( IEnumerable<Station> newOrUpdatedStations )
        {
            lock ( _stationsLock )
            {
            var builder = stations.ToBuilder();
                foreach ( var newOrUpdatedStation in newOrUpdatedStations )
            {
                    internalAddOrUpdateStation( newOrUpdatedStation, builder );
            }
                builder.Sort( ( lhs, rhs ) => Math.Sign( ( lhs.marketId - rhs.marketId ) ?? 0 ) );
            stations = builder.ToImmutable();
        }
        }

        private void internalAddOrUpdateStation ( Station newOrUpdatedStation, ImmutableList<Station>.Builder builder )
        {
            if ( newOrUpdatedStation is null ) { return; }

            var index = builder.FindIndex( s =>
                ( s.marketId != null && newOrUpdatedStation.marketId != null &&
                  s.marketId == newOrUpdatedStation.marketId ) ); // Matching marketId
            if ( index >= 0 )
            {
                builder[ index ] = newOrUpdatedStation;
            }
            else
            {
                builder.Add( newOrUpdatedStation );
            }
        }

        public void RemoveStation ( long marketId )
        {
            var builder = stations.ToBuilder();
            var index = builder.FindIndex( s => s.marketId == marketId );
            if ( index >= 0 )
            {
                builder.RemoveAt( index );
                stations = builder.ToImmutable();
            }
        }

        public void AddOrUpdateSignalSource ( SignalSource signalSource )
        {
            var builder = signalSources.ToBuilder();
            builder.Add( signalSource );
            signalSources = builder.ToImmutable();
        }

        private long estimateSystemValue ()
        {
            // Credit to MattG's thread at https://forums.frontier.co.uk/showthread.php/232000-Exploration-value-formulae for scan value formulas

            if ( bodies == null || bodies.Count == 0 )
            {
                return 0;
            }

            long value = 0;

            // Add the estimated value for each body
            foreach ( Body body in bodies )
            {
                value += body.estimatedvalue;
            }

            // Bonus for fully discovering a system
            if ( totalbodies == bodies.Count( b => b.scannedDateTime != null ) )
            {
                value += totalbodies * 1000;

                // Bonus for fully mapping a system
                int mappableBodies = bodies.Count(b => b.bodyType.invariantName != "Star");
                if ( mappableBodies == bodies.Count( b => b.mappedDateTime != null ) )
                {
                    value += mappableBodies * 10000;
                }
            }

            return value;
        }

        public decimal? DistanceFromStarSystem ( StarSystem other )
        {
            if ( other is null )
            { return null; }
            return Functions.StellarDistanceLy( x, y, z, other.x, other.y, other.z );
        }

        #endregion

        #region Overrides of Object

        public override string ToString () => systemname;

        #endregion
    }
}
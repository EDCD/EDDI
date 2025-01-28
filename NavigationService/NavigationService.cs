using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Utilities;

namespace EddiNavigationService
{
    public sealed class NavigationService : INotifyPropertyChanged
    {
        private static NavigationService _instance;
        private static readonly object InstanceLock = new object();

        private readonly Dictionary<QueryType, IQueryResolver> queryResolvers = new Dictionary<QueryType, IQueryResolver>();

        // Search variables
        public StarSystem SearchStarSystem { get; private set; }
        public Station SearchStation { get; private set; }
        public decimal SearchDistanceLy { get; set; }

        // Last query variables
        public QueryType LastQuery
        {
            get => _lastQuery;
            set
            {
                _lastQuery = value;
                OnPropertyChanged();
            }
        }
        private QueryType _lastQuery;

        public string LastQuerySystemArg
        {
            get => _lastQuerySystemArg;
            set
            {
                _lastQuerySystemArg = value; 
                OnPropertyChanged();
            }
        }
        private string _lastQuerySystemArg;

        public string LastQueryStationArg
        {
            get => _lastQueryStationArg;
            set
            {
                _lastQueryStationArg = value;
                OnPropertyChanged();
            }
        }
        private string _lastQueryStationArg;

        public string LastCarrierDestinationArg
        {
            get => _lastCarrierDestinationArg;
            set
            {
                _lastCarrierDestinationArg = value;
                OnPropertyChanged();
            }
        }
        private string _lastCarrierDestinationArg;

        public bool IsWorking
        {
            get => _isWorking;
            set
            {
                _isWorking = value;
                OnPropertyChanged();
            }
        }
        private bool _isWorking;

        public NavigationService()
        {
            // Populate our query resolvers list
            GetQueryResolvers();

            // Remember our last query
            var configuration = ConfigService.Instance.navigationMonitorConfiguration;
            if (Enum.TryParse(configuration.searchQuery, true, out QueryType queryType))
            {
                if (queryType.Group() != null)
                {
                    LastQuery = queryType;
                    LastQuerySystemArg = configuration.searchQuerySystemArg;
                    LastQueryStationArg = configuration.searchQueryStationArg;
                    LastCarrierDestinationArg = configuration.carrierDestinationArg;
                }
            }
        }

        void GetQueryResolvers ()
        {
            foreach ( var type in typeof(IQueryResolver).Assembly.GetTypes () )
            {
                if ( !type.IsInterface && 
                     !type.IsAbstract &&
                     type.GetInterfaces ().Contains ( typeof(IQueryResolver) ) )
                {
                    // Ensure that the static constructor of the class has been run
                    RuntimeHelpers.RunClassConstructor ( type.TypeHandle );

                    if ( type.InvokeMember ( type.Name, BindingFlags.CreateInstance, null, null, null ) is IQueryResolver queryResolver )
                    {
                        try
                        {
                            queryResolvers.Add ( queryResolver.Type, queryResolver );
                        }
                        catch ( ArgumentException e )
                        {
                            Logging.Error ( $"Multiple query resolvers found for query type '{queryResolver.Type}'.", e );
                        }
                    }
                }
            }
        }

        public static NavigationService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceLock)
                    {
                        if (_instance == null)
                        {
                            Logging.Debug("No Navigation instance: creating one");
                            _instance = new NavigationService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary> Obtains the result from various navigation queries </summary>
        /// <param name="queryType">The type of query</param>
        /// <param name="stringArg0">The query string0 argument, if any</param>
        /// <param name="stringArg1">The query string1 argument, if any</param>
        /// <param name="numericArg">The query numeric argument, if any</param>
        /// <param name="booleanArg">The query boolean argument, if any</param>
        /// <param name="fromUserInterface">True if the navigation query was generated from the UI thread</param>
        /// <returns>The query result</returns>
        [CanBeNull]
        public RouteDetailsEvent NavQuery(QueryType queryType, string stringArg0 = null, string stringArg1 = null, decimal? numericArg = null, bool? booleanArg = null, bool fromUserInterface = false)
        {
            IsWorking = true;
            RouteDetailsEvent result = null;
            var query = new Query ( queryType, stringArg0, stringArg1, numericArg, booleanArg, fromUserInterface );

            try
            {
                Logging.Debug( "Resolving navigation query.", query );

                // Resolve the current search query
                if ( queryResolvers.ContainsKey( queryType ) )
                {
                    if ( EDDI.Instance.CurrentStarSystem == null )
                    {
                        Logging.Debug( "Could not resolve navigation query: current star system is unknown." );
                        return null;
                    }

                    foreach ( var resolver in queryResolvers
                                 .Where( resolver => resolver.Key == queryType ) )
                    {
                        if ( queryType == QueryType.carrier )
                        {
                            var fleetCarrier = EDDI.Instance.FleetCarrier;
                            if ( fleetCarrier is null )
                            {
                                Logging.Warn( "Invalid query: no fleet carrier found." );
                                return null;
                            }
                            else
                            {
                                var carrierLocation = EDDI.Instance.DataProvider.GetOrFetchStarSystem( fleetCarrier.currentStarSystem );
                                if ( carrierLocation is null )
                                {
                                    Logging.Warn("Invalid query: unable to find fleet carrier location.");
                                    return null;
                                }
                                result = resolver.Value.Resolve( query, carrierLocation );
                            }
                        }
                        else
                        {
                            result = resolver.Value.Resolve( query, EDDI.Instance.CurrentStarSystem );
                        }
                        break;
                    }
                }
                else
                {
                    throw new ArgumentOutOfRangeException(
                        $"'{queryType}' queries have not been configured in NavigationService.cs" );
                }

            }
            catch ( Exception e )
            {
                Logging.Warn( "Nav query failed", e );
                return null;
            }
            finally
            {
                IsWorking = false;
            }

            // Keep track of the query (excluding route management queries)
            if (result != null)
            {
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;

                // Save the route data
                if (queryType is QueryType.carrier)
                {
                    LastCarrierDestinationArg = stringArg1;

                    // Save query data
                    navConfig.carrierDestinationArg = LastCarrierDestinationArg;
                    navConfig.carrierPlottedRoute = result.Route;
                    ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                }
                else
                {
                    if (queryType.Group() != null)
                    {
                        LastQuery = queryType;
                        LastQuerySystemArg = stringArg0;
                        LastQueryStationArg = stringArg1;

                        // Save query data
                        navConfig.searchQuery = LastQuery.ToString();
                        navConfig.searchQuerySystemArg = LastQuerySystemArg;
                        navConfig.searchQueryStationArg = LastQueryStationArg;
                    }

                    navConfig.plottedRouteList = result.Route;
                    ConfigService.Instance.navigationMonitorConfiguration = navConfig;

                    // Update the global `SearchSystem` and `SearchStation` variables
                    UpdateSearchData(result.systemAddress, result.marketID);
                }
            }

            return result;
        }

        internal static List<long> GetSystemMissionIds(string system)
        {
            var missionsConfig = ConfigService.Instance.missionMonitorConfiguration;
            var missions = missionsConfig.missions.ToList();
            var missionids = new List<long>();       // List of mission IDs for the system

            if (system != null)
            {
                // Get mission IDs associated with the system
                foreach (Mission mission in missions.Where(m => m.destinationsystem == system
                    || (m.originreturn && m.originsystem == system)).ToList())
                {
                    missionids.Add(mission.missionid);
                }
            }
            return missionids;
        }

        private void UpdateSearchData(ulong? searchSystemAddress, long? marketID)
        {
            // Update search system data
            if ( searchSystemAddress > 0 )
            {
                var system = EDDI.Instance.DataProvider.GetOrFetchStarSystem( (ulong)searchSystemAddress );

                //Ignore null & empty systems
                if (system != null)
                {
                    if (system.systemAddress != SearchStarSystem?.systemAddress)
                    {
                        Logging.Debug("Search star system is " + system.systemname);
                        SearchStarSystem = system;
                    }
                    // Update search system distance
                    SearchDistanceLy = EDDI.Instance.CurrentStarSystem?.DistanceFromStarSystem(system) ?? 0;
                }
            }
            else
            {
                SearchStarSystem = null;
            }

            // Update search station data
            if ( marketID > 0 && SearchStarSystem?.stations != null )
            {
                var station = SearchStarSystem.stations.FirstOrDefault(s => s.marketId == marketID);
                if (station != null && station.marketId != SearchStation?.marketId)
                {
                    Logging.Debug("Search station is " + station.name);
                    SearchStation = station;
                }
            }
            else
            {
                SearchStation = null;
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

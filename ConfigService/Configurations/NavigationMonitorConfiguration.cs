using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;

namespace EddiConfigService.Configurations
{
    /// <summary>Storage for configuration of navigation details</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\navigationmonitor.json")]
    public class NavigationMonitorConfiguration : Config
    {
        private DateTime _updatedat;
        private int? _maxSearchDistanceFromStarLs = 10000;
        private bool _prioritizeOrbitalStations = true;
        private string _searchQuery;
        private string _searchQuerySystemArg;
        private string _searchQueryStationArg;
        private string _carrierDestinationArg;
        private decimal? _tdLat;
        private decimal? _tdLong;
        private string _tdPoi;
        private ObservableCollection<NavBookmark> _bookmarks = [];
        private NavWaypointCollection _navRouteList = new(null, true);
        private NavWaypointCollection _carrierPlottedRoute = new(null, true);
        private NavWaypointCollection _plottedRouteList = new();

        public DateTime updatedat
        {
            get => _updatedat;
            set
            {
                if ( value.Equals( _updatedat ) )
                {
                    return;
                }

                _updatedat = value;
                OnPropertyChanged();
            }
        }

        // Search parameters
        public int? maxSearchDistanceFromStarLs
        {
            get => _maxSearchDistanceFromStarLs;
            set
            {
                if ( value == _maxSearchDistanceFromStarLs )
                {
                    return;
                }

                _maxSearchDistanceFromStarLs = value;
                OnPropertyChanged();
            }
        }

        public bool prioritizeOrbitalStations
        {
            get => _prioritizeOrbitalStations;
            set
            {
                if ( value == _prioritizeOrbitalStations )
                {
                    return;
                }

                _prioritizeOrbitalStations = value;
                OnPropertyChanged();
            }
        }

        // Search data
        public string searchQuery
        {
            get => _searchQuery;
            set
            {
                if ( value == _searchQuery )
                {
                    return;
                }

                _searchQuery = value;
                OnPropertyChanged();
            }
        }

        public string searchQuerySystemArg
        {
            get => _searchQuerySystemArg;
            set
            {
                if ( value == _searchQuerySystemArg )
                {
                    return;
                }

                _searchQuerySystemArg = value;
                OnPropertyChanged();
            }
        }

        public string searchQueryStationArg
        {
            get => _searchQueryStationArg;
            set
            {
                if ( value == _searchQueryStationArg )
                {
                    return;
                }

                _searchQueryStationArg = value;
                OnPropertyChanged();
            }
        }

        public string carrierDestinationArg
        {
            get => _carrierDestinationArg;
            set
            {
                if ( value == _carrierDestinationArg )
                {
                    return;
                }

                _carrierDestinationArg = value;
                OnPropertyChanged();
            }
        }

        // Ship touchdown data
        public decimal? tdLat
        {
            get => _tdLat;
            set
            {
                if ( value == _tdLat )
                {
                    return;
                }

                _tdLat = value;
                OnPropertyChanged();
            }
        }

        public decimal? tdLong
        {
            get => _tdLong;
            set
            {
                if ( value == _tdLong )
                {
                    return;
                }

                _tdLong = value;
                OnPropertyChanged();
            }
        }

        public string tdPOI
        {
            get => _tdPoi;
            set
            {
                if ( value == _tdPoi )
                {
                    return;
                }

                _tdPoi = value;
                OnPropertyChanged();
            }
        }

        // Saved bookmarks
        public ObservableCollection<NavBookmark> bookmarks
        {
            get => _bookmarks;
            set
            {
                if ( Equals( value, _bookmarks ) )
                {
                    return;
                }

                _bookmarks = value;
                OnPropertyChanged();
            }
        }

        // Current in-game route
        public NavWaypointCollection navRouteList
        {
            get => _navRouteList;
            set
            {
                if ( Equals( value, _navRouteList ) )
                {
                    return;
                }

                _navRouteList = value;
                OnPropertyChanged();
            }
        }

        // Plotted routes
        public NavWaypointCollection carrierPlottedRoute
        {
            get => _carrierPlottedRoute;
            set
            {
                if ( Equals( value, _carrierPlottedRoute ) )
                {
                    return;
                }

                _carrierPlottedRoute = value;
                OnPropertyChanged();
            }
        }

        public NavWaypointCollection plottedRouteList
        {
            get => _plottedRouteList;
            set
            {
                if ( Equals( value, _plottedRouteList ) )
                {
                    return;
                }

                _plottedRouteList = value;
                OnPropertyChanged();
            }
        }
    }
}

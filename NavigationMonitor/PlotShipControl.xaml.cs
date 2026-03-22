using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiNavigationService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Utilities;

namespace EddiNavigationMonitor
{
    /// <summary>
    /// Interaction logic for RoutePlotterControl.xaml
    /// </summary>
    public partial class PlotShipControl
    {
        private static NavigationMonitor navigationMonitor ()
        {
            return (NavigationMonitor)EDDI.Instance.ObtainMonitor( "Navigation monitor" );
        }

        public PlotShipControl ()
        {
            InitializeComponent();
            plottedRouteData.ItemsSource = navigationMonitor().PlottedRoute.Waypoints;

            ConfigureSearchGroupOptions();

            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            prioritizeOrbitalStations.IsChecked = navConfig.prioritizeOrbitalStations;
            maxSearchDistanceInt.Text = ( navConfig.maxSearchDistanceFromStarLs ?? 0 ).ToString( CultureInfo.InvariantCulture );
            searchSystemDropDown.Text = navConfig.searchQuerySystemArg;
            searchStationDropDown.Text = navConfig.searchQueryStationArg;

            if ( !Enum.TryParse( navConfig.searchQuery, out QueryType queryType ) )
            {
                queryType = QueryType.route;
            }
            searchGroupDropDown.SelectedItem = queryType.Group();
            searchQueryDropDown.SelectedItem = queryType;
            configureSearchArgumentOptions( queryType );
            configureRoutePlotterColumns( queryType );
            UpdateGuidanceLock( navigationMonitor().PlottedRoute.GuidanceEnabled );
            ClearRouteButton.IsEnabled = navigationMonitor().PlottedRoute.Waypoints.Count > 0;

            NavigationService.Instance.PropertyChanged += OnNavServiceChange;
            navigationMonitor().PlottedRoute.PropertyChanged += OnPlottedRouteChanged;
        }

        private void UpdateGuidanceLock ( bool guidanceEnabled )
        {
            if ( guidanceEnabled )
            {
                GuidanceButton.Content = Properties.NavigationMonitor.disable_guidance_button;
                GuidanceButton.ToolTip = Properties.NavigationMonitor.disable_guidance_button_tooltip;
            }
            else
            {
                GuidanceButton.Content = Properties.NavigationMonitor.enable_guidance_button;
                GuidanceButton.ToolTip = Properties.NavigationMonitor.enable_guidance_button_tooltip;
            }

            // Lock out the query UI while guidance is activated
            searchGroupDropDown.IsEnabled = !guidanceEnabled;
            searchQueryDropDown.IsEnabled = !guidanceEnabled;
            searchSystemDropDown.IsEnabled = !guidanceEnabled;
            searchStationDropDown.IsEnabled = !guidanceEnabled;
            maxSearchDistanceInt.IsEnabled = !guidanceEnabled;
            prioritizeOrbitalStations.IsEnabled = !guidanceEnabled;
            SearchButton.IsEnabled = !guidanceEnabled;
        }

        private void OnPlottedRouteChanged ( object sender, PropertyChangedEventArgs e )
        {
            if ( sender is not NavWaypointCollection navWaypointCollection )
            {
                return;
            }

            switch ( e.PropertyName )
            {
                case nameof( NavWaypointCollection.GuidanceEnabled ):
                    {
                        Dispatcher.InvokeAsync( () =>
                        {
                            UpdateGuidanceLock( navWaypointCollection.GuidanceEnabled );
                        } );
                        break;
                    }
                case nameof( NavWaypointCollection.Waypoints ):
                    {
                        Dispatcher.InvokeAsync( () =>
                        {
                            ClearRouteButton.IsEnabled = navWaypointCollection.Waypoints.Count > 0;
                        } );
                        break;
                    }
            }
        }

        private void OnNavServiceChange ( object sender, PropertyChangedEventArgs e )
        {
            // Don't update the UI while guidance is locked.
            var config = ConfigService.Instance.navigationMonitorConfiguration;
            if ( config.plottedRouteList is { } collection && collection.GuidanceEnabled )
            {
                return;
            }

            switch ( e.PropertyName )
            {
                case nameof( NavigationService.Instance.IsWorking ):
                    {
                        if ( NavigationService.Instance.IsWorking )
                        {
                            Dispatcher.InvokeAsync( () =>
                            {
                                SearchProgressBar.Visibility = Visibility.Visible;
                            } );
                        }
                        else
                        {
                            Dispatcher.InvokeAsync( () =>
                            {
                                SearchProgressBar.Visibility = Visibility.Collapsed;
                            } );
                        }
                        break;
                    }
                case nameof( NavigationService.Instance.LastQuery ):
                    {
                        var queryType = NavigationService.Instance.LastQuery;
                        Dispatcher.InvokeAsync( () =>
                        {
                            searchGroupDropDown.SelectedItem = queryType.Group();
                            searchQueryDropDown.SelectedItem = queryType;
                            configureSearchArgumentOptions( queryType );
                            configureRoutePlotterColumns( queryType );
                        } );
                        break;
                    }
                case nameof( NavigationService.Instance.LastQuerySystemArg ):
                    {
                        var querySystem = NavigationService.Instance.LastQuerySystemArg;
                        Dispatcher.InvokeAsync( () =>
                        {
                            if ( searchSystemDropDown.Text != querySystem )
                            {
                                searchSystemDropDown.Text = querySystem;
                            }
                        } );
                        break;
                    }
                case nameof( NavigationService.Instance.LastQueryStationArg ):
                    {
                        var queryStation = NavigationService.Instance.LastQueryStationArg;
                        Dispatcher.InvokeAsync( () =>
                        {
                            if ( searchStationDropDown.Text != queryStation )
                            {
                                searchStationDropDown.Text = queryStation;
                            }
                        } );
                        break;
                    }
            }
        }

        private void prioritizeOrbitalStationsEnabled ( object sender, RoutedEventArgs e )
        {
            updateOrbitalStationsCheckbox();
        }

        private void prioritizeOrbitalStationsDisabled ( object sender, RoutedEventArgs e )
        {
            updateOrbitalStationsCheckbox();
        }

        private void updateOrbitalStationsCheckbox ()
        {
            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            var isChecked = prioritizeOrbitalStations.IsChecked ?? false;
            if ( navConfig.prioritizeOrbitalStations != isChecked )
            {
                navConfig.prioritizeOrbitalStations = isChecked;
                navigationMonitor().WriteNavConfig();
            }
        }

        private void maxSearchDistance_KeyDown ( object sender, KeyEventArgs e )
        {
            if ( e.Key == Key.Return )
            {
                maxStationDistance_Changed();
            }
        }

        private void maxSearchDistance_LostFocus ( object sender, RoutedEventArgs e )
        {
            maxStationDistance_Changed();
        }

        private void maxStationDistance_Changed ()
        {
            try
            {
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                int? distance = string.IsNullOrWhiteSpace(maxSearchDistanceInt.Text)
                    ? 10000 : Convert.ToInt32(maxSearchDistanceInt.Text, CultureInfo.InvariantCulture);
                if ( distance != navConfig.maxSearchDistanceFromStarLs )
                {
                    navConfig.maxSearchDistanceFromStarLs = distance;
                    navigationMonitor().WriteNavConfig();
                }
            }
            catch
            {
                // Bad user input; ignore it
            }
        }

        private void ConfigureSearchGroupOptions ()
        {
            searchGroupDropDown.ItemsSource = ( (QueryGroup[])Enum.GetValues( typeof( QueryGroup ) ) )
                .OrderBy( g => g.LocalizedName() );
        }

        private void searchGroupDropDownUpdated ( object sender, SelectionChangedEventArgs e )
        {
            var queryGroup = (QueryGroup)searchGroupDropDown.SelectedItem;
            ConfigureSearchQueryOptions( queryGroup );

            // Set the default query
            searchQueryDropDown.SelectedItem = queryGroup.DefaultQueryType();
        }

        private void ConfigureSearchQueryOptions ( QueryGroup queryGroup )
        {
            searchQueryDropDown.ItemsSource = queryGroup.QueryTypes().OrderBy( t => t.LocalizedName() );
        }

        private void searchQueryDropDownUpdated ( object sender, SelectionChangedEventArgs e )
        {
            if ( searchQueryDropDown.SelectedItem != null )
            {
                configureSearchArgumentOptions( (QueryType)searchQueryDropDown.SelectedItem );
                ConfigService.Instance.navigationMonitorConfiguration.searchQuery =
                    searchQueryDropDown.SelectedItem.ToString();
            }
        }

        private void configureRoutePlotterColumns ( QueryType queryType )
        {
            // Configure view by query type
            switch ( queryType )
            {
                case QueryType.encoded:
                case QueryType.facilitator:
                case QueryType.guardian:
                case QueryType.human:
                case QueryType.manufactured:
                    {
                        StationColumn.Visibility = Visibility.Visible;
                        RefuelColumn.Visibility = Visibility.Collapsed;
                        break;
                    }
                case QueryType.neutron:
                    {
                        StationColumn.Visibility = Visibility.Collapsed;
                        RefuelColumn.Visibility = Visibility.Visible;
                        break;
                    }
                default:
                    {
                        StationColumn.Visibility = Visibility.Collapsed;
                        RefuelColumn.Visibility = Visibility.Collapsed;
                        break;
                    }
            }
        }

        private void configureSearchArgumentOptions ( QueryType queryType )
        {
            NavigationService.Instance.LastQuerySystemArg = string.Empty;
            NavigationService.Instance.LastQueryStationArg = string.Empty;

            switch ( queryType )
            {
                case QueryType.encoded:
                case QueryType.facilitator:
                case QueryType.guardian:
                case QueryType.human:
                case QueryType.manufactured:
                    {
                        StationParametersGrid.Visibility = Visibility.Visible;
                        navSearchSystemLabel.Visibility = Visibility.Collapsed;
                        searchSystemDropDown.Visibility = Visibility.Collapsed;
                        navSearchStationLabel.Visibility = Visibility.Collapsed;
                        searchStationDropDown.Visibility = Visibility.Collapsed;
                        break;
                    }
                case QueryType.most:
                case QueryType.neutron:
                case QueryType.route:
                case QueryType.source:
                    {
                        StationParametersGrid.Visibility = Visibility.Collapsed;
                        navSearchSystemLabel.Visibility = Visibility.Visible;
                        searchSystemDropDown.Visibility = Visibility.Visible;
                        navSearchStationLabel.Visibility = Visibility.Collapsed;
                        searchStationDropDown.Visibility = Visibility.Collapsed;
                        break;
                    }
                case QueryType.set:
                    {
                        StationParametersGrid.Visibility = Visibility.Collapsed;
                        navSearchSystemLabel.Visibility = Visibility.Visible;
                        searchSystemDropDown.Visibility = Visibility.Visible;
                        navSearchStationLabel.Visibility = Visibility.Visible;
                        searchStationDropDown.Visibility = Visibility.Visible;
                        break;
                    }
                default:
                    {
                        StationParametersGrid.Visibility = Visibility.Collapsed;
                        navSearchSystemLabel.Visibility = Visibility.Collapsed;
                        searchSystemDropDown.Visibility = Visibility.Collapsed;
                        navSearchStationLabel.Visibility = Visibility.Collapsed;
                        searchStationDropDown.Visibility = Visibility.Collapsed;
                        break;
                    }
            }
        }

        private async void executeSearch ( object sender, RoutedEventArgs e )
        {
            try
            {
                if ( !NavigationService.Instance.IsWorking )
                {
                    var systemArg = searchSystemDropDown.Text;
                    var stationArg = searchStationDropDown.Text == Properties.NavigationMonitor.no_station
                        ? null
                        : searchStationDropDown.Text;
                    var queryType = (QueryType)searchQueryDropDown.SelectedItem;
                    var @event = await NavigationService.Instance.NavQueryAsync(queryType, systemArg, stationArg, null, null, true).ConfigureAwait(true);
                    if ( @event is null ) { return; }
                    EDDI.Instance.enqueueEvent( @event );
                }
            }
            catch ( Exception exception )
            {
                Logging.Error( "Search task failed", exception );
            }
        }

        private async void SearchSystemDropDown_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            try
            {
                if ( sender is StarSystemComboBox starSystemComboBox )
                {
                    if ( !starSystemComboBox.IsLoaded ) { return; }

                    // Update configuration to new home system
                    if ( e.AddedItems.Count == 1 && e.RemovedItems.Count == 0 )
                    {
                        var newValue = e.AddedItems[0] as string;

                        // Update to new search system
                        NavigationService.Instance.LastQuerySystemArg = newValue;

                        // Update station options for new system
                        await ConfigureSearchStationOptionsAsync( NavigationService.Instance.LastQuerySystemArg ).ConfigureAwait(true);
                    }
                }
            }
            catch ( Exception ex )
            {
                Logging.Error( ex.Message, ex );
            }
        }

        private async Task ConfigureSearchStationOptionsAsync ( string system )
        {
            var searchStationOptions = new List<string>
                {
                    Properties.NavigationMonitor.no_station
                };

            if ( searchStationDropDown.Visibility == Visibility.Visible && !string.IsNullOrEmpty( system ) )
            {
                var searchSystem = await EDDI.Instance.DataProvider.GetOrFetchStarSystemAsync( system ).ConfigureAwait(false);
                if ( searchSystem?.stations != null )
                {
                    searchStationOptions.AddRange(
                        searchSystem.stations
                            .Where( s => !s.IsCarrier() && !s.IsMegaShip() )
                            .Select( s => s.name )
                    );
                }
            }
            // sort but leave "No Station" at the top
            searchStationOptions.Sort( 1, searchStationOptions.Count - 1, null );
            searchStationDropDown.ItemsSource = searchStationOptions;
        }

        private void searchStationDropDownUpdated ( object sender, SelectionChangedEventArgs e )
        {
            var searchStationName = searchStationDropDown.SelectedItem?.ToString();
            if ( NavigationService.Instance.LastQueryStationArg != searchStationName )
            {
                NavigationService.Instance.LastQueryStationArg = searchStationName == Properties.NavigationMonitor.no_station ? null : searchStationName;
            }
        }

        private void EnsureValidInteger ( object sender, TextCompositionEventArgs e )
        {
            // Swallow the character if it doesn't match the regex
            e.Handled = !GeneratedRegex.IsIntegerRegex().IsMatch( e.Text );
        }

        private void DataGrid_LoadingRow ( object sender, DataGridRowEventArgs e )
        {
            e.Row.Header = e.Row.GetIndex().ToString();
        }

        private async void GuidanceButton_Click ( object sender, RoutedEventArgs e )
        {
            try
            {
                var queryType = GuidanceButton.Content.ToString() == Properties.NavigationMonitor.disable_guidance_button
                    ? QueryType.cancel
                    : QueryType.set;

                var @event = await NavigationService.Instance
                    .NavQueryAsync(queryType, null, null, null, null, true)
                    .ConfigureAwait(true); // resume on UI thread if needed

                if ( @event != null )
                {
                    EDDI.Instance.enqueueEvent( @event );
                }
            }
            catch (Exception ex)
            {
                Logging.Error(ex.Message, ex);
            }
        }

        private async void ClearRouteButton_Click ( object sender, RoutedEventArgs e )
        {
            try
            {
                if ( plottedRouteData.Items.Count > 0 )
                {
                    if ( navigationMonitor().PlottedRoute.GuidanceEnabled )
                    {
                        await NavigationService.Instance
                            .NavQueryAsync( QueryType.cancel, null, null, null, null, true )
                            .ConfigureAwait( true );
                    }
                    navigationMonitor().PlottedRoute.Waypoints.Clear();
                    navigationMonitor().WriteNavConfig();
                }
            }
            catch ( Exception ex )
            {
                Logging.Error( ex.Message, ex );
            }
        }

        private void addBookmark ( object sender, RoutedEventArgs e )
        {
            if ( Parent is TabItem item && item.Parent is TabControl control && control.Parent is DockPanel panel && panel.Parent is ConfigurationWindow configurationWindow )
            {
                configurationWindow.SwitchToTab( Properties.NavigationMonitor.tab_bookmarks );
                configurationWindow.addBookmark( sender, e );
            }
        }

        private void copySystemNameToClipboard ( object sender, RoutedEventArgs e )
        {
            if ( sender is Button button && button.DataContext is NavWaypoint navWaypoint )
            {
                try
                {
                    Clipboard.Clear();
                    Clipboard.SetData( DataFormats.Text, navWaypoint.systemName );
                }
                catch ( Exception ex )
                {
                    Logging.Warn( "Failed to set clipboard", ex );
                }
            }
        }
    }
}

using Eddi;
using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiNavigationService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EddiNavigationMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class RoutePlotterControl : UserControl
    {
        private NavigationMonitor navigationMonitor()
        {
            return (NavigationMonitor)EDDI.Instance.ObtainMonitor("Navigation monitor");
        }

        public RoutePlotterControl()
        {
            InitializeComponent();
            plottedRouteData.ItemsSource = navigationMonitor().PlottedRouteList.Waypoints;

            ConfigureSearchGroupOptions();

            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            prioritizeOrbitalStations.IsChecked = navConfig.prioritizeOrbitalStations;
            maxSearchDistanceInt.Text = (navConfig.maxSearchDistanceFromStarLs ?? 0).ToString(CultureInfo.InvariantCulture);
            searchSystemDropDown.Text = navConfig.searchQuerySystemArg;
            searchStationDropDown.Text = navConfig.searchQueryStationArg;

            if (!Enum.TryParse(navConfig.searchQuery, out QueryType queryType))
            {
                queryType = QueryType.route;
            }
            searchGroupDropDown.SelectedItem = queryType.Group();
            searchQueryDropDown.SelectedItem = queryType;
            configureSearchArgumentOptions(queryType);
            configureRoutePlotterColumns(queryType);
            UpdateGuidanceLock(navigationMonitor().PlottedRouteList.GuidanceEnabled);
            GuidanceButton.IsEnabled = !navigationMonitor().PlottedRouteList.GuidanceEnabled
                                       && navigationMonitor().PlottedRouteList.Waypoints.Count > 1;
            ClearRouteButton.IsEnabled = navigationMonitor().PlottedRouteList.Waypoints.Count > 0;

            NavigationService.Instance.PropertyChanged += OnNavServiceChange;
            navigationMonitor().PlottedRouteList.PropertyChanged += OnPlottedRouteChanged;
        }

        private void UpdateGuidanceLock(bool guidanceEnabled)
        {
            if (guidanceEnabled)
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

        private void OnPlottedRouteChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is NavWaypointCollection navWaypointCollection)
            {
                switch (e.PropertyName)
                {
                    case nameof(NavWaypointCollection.GuidanceEnabled):
                        {
                            Dispatcher.Invoke(() =>
                            {
                                UpdateGuidanceLock(navWaypointCollection.GuidanceEnabled);
                            });
                            break;
                        }
                    case nameof(NavWaypointCollection.Waypoints):
                        {
                            Dispatcher.Invoke(() =>
                            {
                                GuidanceButton.IsEnabled = !navWaypointCollection.GuidanceEnabled && navWaypointCollection.Waypoints.Count > 1;
                                ClearRouteButton.IsEnabled = navWaypointCollection.Waypoints.Count > 0;
                            });
                            break;
                        }
                }
            }
        }

        private void OnNavServiceChange(object sender, PropertyChangedEventArgs e)
        {
            // Don't update the UI while guidance is locked.
            var config = ConfigService.Instance.navigationMonitorConfiguration;
            if (config.plottedRouteList.GuidanceEnabled)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(NavigationService.Instance.IsWorking):
                    {
                        if (NavigationService.Instance.IsWorking)
                        {
                            Dispatcher.Invoke(() => { SearchProgressBar.Visibility = Visibility.Visible; });
                        }
                        else
                        {
                            Dispatcher.Invoke(() => { SearchProgressBar.Visibility = Visibility.Collapsed; });
                        }
                        break;
                    }
                case nameof(NavigationService.Instance.LastQuery):
                    {
                        var queryType = NavigationService.Instance.LastQuery;
                        Dispatcher.Invoke(() =>
                        {
                            searchGroupDropDown.SelectedItem = queryType.Group();
                            searchQueryDropDown.SelectedItem = queryType;
                            configureSearchArgumentOptions(queryType);
                            configureRoutePlotterColumns(queryType);
                        });
                        break;
                    }
                case nameof(NavigationService.Instance.LastQuerySystemArg):
                    {
                        var querySystem = NavigationService.Instance.LastQuerySystemArg;
                        Dispatcher.Invoke(() =>
                        {
                            if (searchSystemDropDown.Text != querySystem)
                            {
                                searchSystemDropDown.Text = querySystem;
                            }
                        });
                        break;
                    }
                case nameof(NavigationService.Instance.LastQueryStationArg):
                    {
                        var queryStation = NavigationService.Instance.LastQueryStationArg;
                        Dispatcher.Invoke(() =>
                        {
                            if (searchStationDropDown.Text != queryStation)
                            {
                                searchStationDropDown.Text = queryStation;
                            }
                        });
                        break;
                    }
            }
        }

        private void prioritizeOrbitalStationsEnabled(object sender, RoutedEventArgs e)
        {
            updateOrbitalStationsCheckbox();
        }

        private void prioritizeOrbitalStationsDisabled(object sender, RoutedEventArgs e)
        {
            updateOrbitalStationsCheckbox();
        }

        private void updateOrbitalStationsCheckbox()
        {
            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            bool isChecked = prioritizeOrbitalStations.IsChecked ?? false;
            if (navConfig.prioritizeOrbitalStations != isChecked)
            {
                navConfig.prioritizeOrbitalStations = isChecked;
                navigationMonitor().WriteNavConfig();
            }
        }

        private void maxSearchDistance_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                maxStationDistance_Changed();
            }
        }

        private void maxSearchDistance_LostFocus(object sender, RoutedEventArgs e)
        {
            maxStationDistance_Changed();
        }

        private void maxStationDistance_Changed()
        {
            try
            {
                var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
                int? distance = string.IsNullOrWhiteSpace(maxSearchDistanceInt.Text)
                    ? 10000 : Convert.ToInt32(maxSearchDistanceInt.Text, CultureInfo.InvariantCulture);
                if (distance != navConfig.maxSearchDistanceFromStarLs)
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

        private void ConfigureSearchGroupOptions()
        {
            searchGroupDropDown.ItemsSource = ((QueryGroup[])Enum.GetValues(typeof(QueryGroup)))
                .OrderBy(g => g.LocalizedName());
        }

        private void searchGroupDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            var queryGroup = (QueryGroup)searchGroupDropDown.SelectedItem;
            ConfigureSearchQueryOptions(queryGroup);

            // Set the default query
            searchQueryDropDown.SelectedItem = queryGroup.DefaultQueryType();
        }

        private void ConfigureSearchQueryOptions(QueryGroup queryGroup)
        {
            searchQueryDropDown.ItemsSource = queryGroup.QueryTypes().OrderBy(t => t.LocalizedName());
        }

        private void searchQueryDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            if (searchQueryDropDown.SelectedItem != null)
            {
                configureSearchArgumentOptions((QueryType)searchQueryDropDown.SelectedItem);
                ConfigService.Instance.navigationMonitorConfiguration.searchQuery =
                    searchQueryDropDown.SelectedItem.ToString();
            }
        }

        private void configureRoutePlotterColumns(QueryType queryType)
        {
            // Configure view by query type
            switch (queryType)
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

        private void configureSearchArgumentOptions(QueryType queryType)
        {
            NavigationService.Instance.LastQuerySystemArg = string.Empty;
            NavigationService.Instance.LastQueryStationArg = string.Empty;

            switch (queryType)
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

        private async void executeSearch(object sender, RoutedEventArgs e)
        {
            var systemArg = searchSystemDropDown.Text;
            var stationArg = searchStationDropDown.Text == Properties.NavigationMonitor.no_station
                ? null
                : searchStationDropDown.Text;

            QueryType queryType = (QueryType)searchQueryDropDown.SelectedItem;
            var search = Task.Run(() =>
            {
                var @event = NavigationService.Instance.NavQuery(queryType, systemArg, stationArg);
                if (@event == null) { return; }
                EDDI.Instance?.enqueueEvent(@event);
            });
            await Task.WhenAll(search);
        }

        private void SearchSystemText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is StarSystemComboBox starSystemComboBox)
            {
                if (!starSystemComboBox.IsLoaded) { return; }

                void changeHandler()
                {
                    // Reset the search station due to selecting new search system
                    if (searchStationDropDown.Visibility == Visibility.Visible && NavigationService.Instance.LastQueryStationArg != null)
                    {
                        NavigationService.Instance.LastQueryStationArg = null;
                        searchStationDropDown.SelectedItem = Properties.NavigationMonitor.no_station;
                        ConfigureSearchStationOptions(null);
                    }
                }

                searchSystemDropDown.TextDidChange(sender, e, NavigationService.Instance.LastQuerySystemArg, changeHandler);
            }
        }

        private void SearchSystemDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is StarSystemComboBox starSystemComboBox && !starSystemComboBox.IsLoaded) { return; }

            void changeHandler(string newValue)
            {
                // Update to new search system
                NavigationService.Instance.LastQuerySystemArg = newValue;

                // Update station options for new system
                ConfigureSearchStationOptions(NavigationService.Instance.LastQuerySystemArg);
            }
            searchSystemDropDown.SelectionDidChange(changeHandler);
        }

        private void SearchSystemDropDown_LostFocus(object sender, RoutedEventArgs e)
        {
            searchSystemDropDown.DidLoseFocus(oldValue: NavigationService.Instance.LastQuerySystemArg);
        }

        private void ConfigureSearchStationOptions(string system)
        {
            List<string> SearchStationOptions = new List<string>
                {
                    Properties.NavigationMonitor.no_station
                };

            if (searchStationDropDown.Visibility == Visibility.Visible && !string.IsNullOrEmpty(system))
            {
                StarSystem SearchSystem = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(system, false);
                if (SearchSystem?.stations != null)
                {
                    foreach (Station station in SearchSystem.stations.Where(s => !s.IsCarrier() && !s.IsMegaShip()))
                    {
                        SearchStationOptions.Add(station.name);
                    }
                }
            }
            // sort but leave "No Station" at the top
            SearchStationOptions.Sort(1, SearchStationOptions.Count - 1, null);
            searchStationDropDown.ItemsSource = SearchStationOptions;
        }

        private void searchStationDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            string searchStationName = searchStationDropDown.SelectedItem?.ToString();
            if (NavigationService.Instance.LastQueryStationArg != searchStationName)
            {
                NavigationService.Instance.LastQueryStationArg = searchStationName == Properties.NavigationMonitor.no_station ? null : searchStationName;
            }
        }

        private void EnsureValidInteger(object sender, TextCompositionEventArgs e)
        {
            // Match valid characters
            Regex regex = new Regex(@"[0-9]");
            // Swallow the character doesn't match the regex
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex()).ToString();
        }

        private void GuidanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (GuidanceButton.Content.ToString() == Properties.NavigationMonitor.disable_guidance_button)
            {
                EDDI.Instance?.enqueueEvent(NavigationService.Instance.NavQuery(QueryType.cancel));
            }
            else
            {
                EDDI.Instance?.enqueueEvent(NavigationService.Instance.NavQuery(QueryType.set));
            }
        }

        private void ClearRouteButton_Click(object sender, RoutedEventArgs e)
        {
            if (plottedRouteData.Items.Count > 0)
            {
                if (navigationMonitor().PlottedRouteList.GuidanceEnabled)
                {
                    NavigationService.Instance.NavQuery(QueryType.cancel);
                }
                navigationMonitor().PlottedRouteList.Waypoints.Clear();
                navigationMonitor().WriteNavConfig();
            }
        }

        private void addBookmark(object sender, RoutedEventArgs e)
        {
            if (Parent is TabItem parentTab && parentTab.Parent is TabControl parentTabControl)
            {
                if (parentTabControl.Parent is DockPanel dockPanel)
                {
                    if (dockPanel.Parent is ConfigurationWindow configurationWindow)
                    {
                        configurationWindow.SwitchToTab(Properties.NavigationMonitor.tab_bookmarks);
                        configurationWindow.addBookmark(sender, e);
                    }
                }
            }
        }
    }
}

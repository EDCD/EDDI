using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiNavigationService;
using EddiShipMonitor;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Utilities;
using EddiStatusMonitor;

namespace EddiNavigationMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        private NavigationMonitorConfiguration navConfig = new NavigationMonitorConfiguration();
        private string searchTypeSelection = String.Empty;
        private string searchQuerySelection = String.Empty;
        private string dropdownSearchSystem = null;
        private string dropdownSearchStation = null;

        private Status currentStatus { get; set; }

        private readonly List<string> searchType = new List<string> {
            "crime",
            "missions",
            "services",
            "ship"
        };

        private readonly List<string> searchQuery = new List<string> {
            "cancel",
            "encoded",
            "expiring",
            "farthest",
            "guardian",
            "human",
            "facilitator",
            "manufactured",
            "most",
            "nearest",
            "next",
            "raw",
            "route",
            "scoop",
            "source",
            "update"
        };

        private NavigationMonitor navigationMonitor()
        {
            return (NavigationMonitor)EDDI.Instance.ObtainMonitor("Navigation monitor");
        }

        public ConfigurationWindow()
        {
            InitializeComponent();
            bookmarksData.ItemsSource = navigationMonitor()?.bookmarks;

            ConfigureSearchTypeOptions();
            searchTypeDropDown.SelectedItem = Properties.NavigationMonitor.search_type_missions;
            searchQueryDropDown.SelectedItem = Properties.NavigationMonitor.search_query_missions_route;

            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            prioritizeOrbitalStations.IsChecked = navConfig.prioritizeOrbitalStations;
            guidanceSystem.IsChecked = navConfig.guidanceSystemEnabled;
            maxSearchDistanceInt.Text = navConfig.maxSearchDistanceFromStarLs?.ToString(CultureInfo.InvariantCulture);

            StatusMonitor.StatusUpdatedEvent += OnStatusUpdated;
        }

        private void OnStatusUpdated(object sender, EventArgs e)
        {
            if (sender is Status status)
            {
                currentStatus = status;
            }
        }

        private void bookmarkLocation(object sender, RoutedEventArgs e)
        {
            bool isStation = false;
            string poi = null;
            decimal? latitude = null;
            decimal? longitude = null;
            bool landable = false;

            navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            if (EDDI.Instance.CurrentStarSystem != null)
            {
                StarSystem currentSystem = EDDI.Instance.CurrentStarSystem;
                Body currentBody = EDDI.Instance.CurrentStellarBody;
                Station currentStation = EDDI.Instance.CurrentStation;

                if (EDDI.Instance.Environment == Constants.ENVIRONMENT_LANDED)
                {
                    if (EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP)
                    {
                        latitude = (decimal)Math.Round((double)navConfig.tdLat, 4);
                        longitude = (decimal)Math.Round((double)navConfig.tdLong, 4);
                        poi = navConfig.tdPOI;
                    }
                    else if (EDDI.Instance.Vehicle == Constants.VEHICLE_SRV)
                    {
                        if (currentStatus != null)
                        {
                            latitude = currentStatus.latitude;
                            longitude = currentStatus.longitude;

                            if (navConfig.tdPOI != null)
                            {
                                // Get current distance from `Touchdown` POI
                                decimal? distanceKm = NavigationMonitor.SurfaceDistanceKm(currentStatus, navConfig?.tdLat, navConfig?.tdLong);
                                if (distanceKm < 5)
                                {
                                    poi = navConfig.tdPOI;
                                }
                            }
                        }
                    }
                    landable = true;
                }
                else if (EDDI.Instance.Environment == Constants.ENVIRONMENT_DOCKED)
                {
                    if (currentStation != null)
                    {
                        isStation = true;
                        poi = currentStation.name;
                        latitude = currentStatus?.latitude;
                        longitude = currentStatus?.longitude;
                    }
                }
                else if (EDDI.Instance.Environment == Constants.ENVIRONMENT_NORMAL_SPACE)
                {
                    if (currentStation != null)
                    {
                        isStation = true;
                        poi = currentStation.name;
                    }
                    if (currentStatus != null && currentStatus.near_surface && currentBody != null)
                    {
                        NavigationMonitor.SurfaceCoordinates(currentStatus, out latitude, out longitude);
                        landable = currentBody?.landable ?? false;
                    }
                }
                else if (EDDI.Instance.Environment == Constants.ENVIRONMENT_SUPERCRUISE)
                {
                    if (currentStatus != null && currentStatus.near_surface && currentBody != null)
                    {
                        NavigationMonitor.SurfaceCoordinates(currentStatus, out latitude, out longitude);
                        landable = currentBody?.landable ?? false;
                    }
                }

                NavBookmark navBookmark = new NavBookmark(currentSystem.systemname, currentSystem.x, currentSystem.y, currentSystem.z,
                    currentBody?.shortname, currentBody?.radius, poi, isStation, latitude, longitude, landable);
                navConfig.bookmarks.Add(navBookmark);
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                EDDI.Instance.enqueueEvent(new BookmarkDetailsEvent(DateTime.UtcNow, "location", navBookmark));
            }
        }

        private void bookmarkQuery(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(navConfig.searchSystem)) { return; }

            navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            string systemName = navConfig.searchSystem;
            StarSystem system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(systemName, false);

            string stationName = navConfig.searchStation;
            bool isStation = false;
            bool landable = false;
            if (stationName != null)
            {
                isStation = true;
                landable = system.stations.FirstOrDefault(s => s.name == stationName)?.IsPlanetary() ?? false;
            }

            NavBookmark navBookmark = new NavBookmark(systemName, system?.x, system?.y, system?.z, null, null, stationName, isStation, null, null, landable);
            navConfig.bookmarks.Add(navBookmark);
            ConfigService.Instance.navigationMonitorConfiguration = navConfig;
            EDDI.Instance.enqueueEvent(new BookmarkDetailsEvent(DateTime.UtcNow, "query", navBookmark));
        }

        private void bookmarkUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the bookmark list
            navigationMonitor()?.writeBookmarks();
        }

        private void importBookmarks(object sender, RoutedEventArgs e)
        {
            string filename = Constants.DATA_DIR + @"\import.csv";
            bool header = true;
            List<string> headerNames = new List<string>();
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            if (File.Exists(filename))
            {
                using (TextFieldParser parser = new TextFieldParser(filename))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    while (!parser.EndOfData)
                    {
                        try
                        {
                            NavBookmark navBookmark = new NavBookmark();
                            string[] fields = parser.ReadFields();
                            for (int i = 0; i < fields.Count(); i++)
                            {
                                if (header)
                                {
                                    headerNames.Add(fields[i]);
                                }
                                else
                                {
                                    switch (headerNames[i]?.ToLowerInvariant())
                                    {
                                        case "system":
                                            {
                                                navBookmark.systemname = fields[i];
                                            }
                                            break;
                                        case "x":
                                            {
                                                navBookmark.x = decimal.Parse(fields[i]);
                                            }
                                            break;
                                        case "y":
                                            {
                                                navBookmark.y = decimal.Parse(fields[i]);
                                            }
                                            break;
                                        case "z":
                                            {
                                                navBookmark.z = decimal.Parse(fields[i]);
                                            }
                                            break;
                                        case "body":
                                            {
                                                navBookmark.bodyname = fields[i];
                                            }
                                            break;
                                        case "name":
                                            {
                                                navBookmark.comment = fields[i];
                                            }
                                            break;
                                        case "category":
                                            {
                                                navBookmark.category = fields[i];
                                            }
                                            break;
                                        case "radius":
                                            {
                                                navBookmark.radius = decimal.Parse(fields[i]);
                                            }
                                            break;
                                        case "latitude":
                                            {
                                                navBookmark.latitude = decimal.Parse(fields[i]);
                                            }
                                            break;
                                        case "longitude":
                                            {
                                                navBookmark.longitude = decimal.Parse(fields[i]);
                                            }
                                            break;
                                    }
                                }
                            }
                            if (!header) { navConfig.bookmarks.Add(navBookmark); }
                            header = false;
                        }
                        catch (MalformedLineException ex)
                        {
                            Logging.Error("Line " + ex.Message + " is invalid. Skipping: ", ex);
                        }
                    }
                }
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
            }
        }

        private void removeBookmark(object sender, RoutedEventArgs e)
        {
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            var index = bookmarksData.SelectedIndex;
            string messageBoxText = Properties.NavigationMonitor.remove_message;
            string caption = Properties.NavigationMonitor.remove_caption;
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    {
                        // Remove the bookmark from the list
                        navigationMonitor().RemoveBookmarkAt(index);
                        navConfig.bookmarks = navigationMonitor()?.bookmarks;
                        ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                    }
                    break;
            }
        }

        private void setBookmark(object sender, RoutedEventArgs e)
        {
            // Clear all previously 'set' bookmarks
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            NavBookmark bm = navConfig.bookmarks.FirstOrDefault(b => b.isset);
            while (bm?.isset ?? false)
            {
                bm.isset = false;
                bm = navConfig.bookmarks.FirstOrDefault(b => b.isset);
            }

            string station = null;
            NavBookmark navBookmark = (NavBookmark)((Button)e.Source).DataContext;
            if (navBookmark.isstation) { station = navBookmark.poi; }
            navBookmark.isset = true;

            NavigationService.Instance.SetRoute(navBookmark.systemname, station);

            EDDI.Instance.enqueueEvent(new BookmarkDetailsEvent(DateTime.UtcNow, "set", navBookmark));
        }

        private void updateBookmark(object sender, RoutedEventArgs e)
        {
            decimal? latitude = null;
            decimal? longitude = null;

            navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            StarSystem currentSystem = EDDI.Instance.CurrentStarSystem;
            Body currentBody = EDDI.Instance.CurrentStellarBody;
            Station currentStation = EDDI.Instance.CurrentStation;
            Status currentStatus = NavigationService.Instance.currentStatus;

            if (e.Source is Button button)
            {
                var navBookmark = (NavBookmark)button.DataContext;

                // Update only if current system matches the bookmarked system
                if (navBookmark != null && navBookmark?.systemname == currentSystem.systemname)
                {
                    // Update latitude & longitude if current body matches the bookmarked body
                    if (currentBody != null && currentBody.bodyname == navBookmark.bodyname)
                    {
                        if (EDDI.Instance.Environment == Constants.ENVIRONMENT_LANDED)
                        {
                            if (EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP)
                            {
                                latitude = (decimal)Math.Round((double)navConfig.tdLat, 4);
                                longitude = (decimal)Math.Round((double)navConfig.tdLong, 4);
                                if (navBookmark.poi is null) { navBookmark.poi = navConfig.tdPOI; }
                            }
                            else if (EDDI.Instance.Vehicle == Constants.VEHICLE_SRV)
                            {
                                latitude = currentStatus.latitude;
                                longitude = currentStatus.longitude;

                                if (navConfig.tdPOI != null)
                                {
                                    // Get current distance from `Touchdown` POI
                                    decimal? distanceKm = NavigationMonitor.SurfaceDistanceKm(currentStatus, navConfig?.tdLat, navConfig?.tdLong);
                                    if (distanceKm < 5)
                                    {
                                        navBookmark.poi = navConfig.tdPOI;
                                    }
                                }
                            }
                            navBookmark.landable = true;
                        }
                        else if (EDDI.Instance.Environment == Constants.ENVIRONMENT_SUPERCRUISE)
                        {
                            if (currentStatus.near_surface)
                            {
                                NavigationMonitor.SurfaceCoordinates(currentStatus, out latitude, out longitude);
                                navBookmark.landable = currentBody.landable ?? false;
                            }
                        }
                    }

                    // Update if a station is instanced and a body was not previously bookmarked
                    else if (currentStation != null && navBookmark.bodyname is null)
                    {
                        if (EDDI.Instance.Environment == Constants.ENVIRONMENT_NORMAL_SPACE)
                        {
                            navBookmark.isstation = true;
                            navBookmark.poi = currentStation.name;
                        }
                    }

                    ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                    EDDI.Instance.enqueueEvent(new BookmarkDetailsEvent(DateTime.UtcNow, "update", navBookmark));
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
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            bool isChecked = prioritizeOrbitalStations.IsChecked.Value;
            if (navConfig.prioritizeOrbitalStations != isChecked)
            {
                navConfig.prioritizeOrbitalStations = isChecked;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
            }
        }

        private void guidanceSystemEngaged(object sender, RoutedEventArgs e)
        {
            updateGuidanceSystemCheckbox();
        }

        private void guidanceSystemDisengaged(object sender, RoutedEventArgs e)
        {
            updateGuidanceSystemCheckbox();
        }

        private void updateGuidanceSystemCheckbox()
        {
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            bool isChecked = guidanceSystem.IsChecked ?? false;
            if (navConfig.guidanceSystemEnabled != isChecked)
            {
                navConfig.guidanceSystemEnabled = isChecked;
                ConfigService.Instance.navigationMonitorConfiguration = navConfig;
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
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            try
            {
                int? distance = string.IsNullOrWhiteSpace(maxSearchDistanceInt.Text)
                    ? 10000 : Convert.ToInt32(maxSearchDistanceInt.Text, CultureInfo.InvariantCulture);
                if (distance != navConfig.maxSearchDistanceFromStarLs)
                {
                    navConfig.maxSearchDistanceFromStarLs = distance;
                    ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                }
            }
            catch
            {
                // Bad user input; ignore it
            }
        }

        private void ConfigureSearchTypeOptions()
        {
            List<string> SearchTypeOptions = new List<string>();

            foreach (string type in searchType)
            {
                SearchTypeOptions.Add(Properties.NavigationMonitor.ResourceManager.GetString("search_type_" + type));
            }
            searchTypeDropDown.ItemsSource = SearchTypeOptions;
        }

        private void searchTypeDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            foreach (string type in searchType)
            {
                string property = Properties.NavigationMonitor.ResourceManager.GetString("search_type_" + type);
                if (property == searchTypeDropDown.SelectedItem?.ToString())
                {
                    searchTypeSelection = type;
                    break;
                }
            }
            ConfigureSearchQueryOptions(searchTypeSelection);

            // Set the default query
            switch (searchTypeSelection)
            {
                case "crime":
                    {
                        searchQueryDropDown.SelectedItem = Properties.NavigationMonitor.search_query_crime_facilitator;
                    }
                    break;
                case "missions":
                    {
                        searchQueryDropDown.SelectedItem = Properties.NavigationMonitor.search_query_missions_route;
                    }
                    break;
                case "services":
                    {
                        searchQueryDropDown.SelectedItem = Properties.NavigationMonitor.search_query_services_encoded;
                    }
                    break;
                case "ship":
                    {
                        searchQueryDropDown.SelectedItem = Properties.NavigationMonitor.search_query_ship_scoop;
                    }
                    break;
            }
        }

        private void ConfigureSearchQueryOptions(string type)
        {
            List<string> SearchQueryOptions = new List<string> { };

            foreach (string query in searchQuery)
            {
                string property = Properties.NavigationMonitor.ResourceManager.GetString("search_query_"
                    + type + "_" + query);
                if (property != null) { SearchQueryOptions.Add(property); }
            }
            searchQueryDropDown.ItemsSource = SearchQueryOptions;
        }

        private void searchQueryDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            bool found = false;
            foreach (string type in searchType)
            {
                foreach (string query in searchQuery)
                {
                    string property = Properties.NavigationMonitor.ResourceManager.GetString("search_query_"
                        + type + "_" + query);
                    if (property == searchQueryDropDown.SelectedItem?.ToString())
                    {
                        searchQuerySelection = query;
                        found = true;
                        break;
                    }
                }
                if (found) { break; }
            }
        }

        private void executeSearch(object sender, RoutedEventArgs e)
        {
            Button searchButton = (Button)sender;
            searchButton.Foreground = Brushes.Red;
            searchButton.FontWeight = FontWeights.Bold;
            string searchSystem = null;

            Thread searchThread = new Thread(() =>
            {
                if (searchTypeSelection == "crime")
                {
                    switch (searchQuerySelection)
                    {
                        case "facilitator":
                            {
                                searchSystem = NavigationService.Instance.GetServiceRoute(searchQuerySelection);
                            }
                            break;
                    }
                }
                else if (searchTypeSelection == "missions")
                {
                    switch (searchQuerySelection)
                    {
                        case "expiring":
                            {
                                searchSystem = NavigationService.Instance.GetExpiringRoute();
                            }
                            break;
                        case "farthest":
                            {
                                searchSystem = NavigationService.Instance.GetFarthestRoute();
                            }
                            break;
                        case "most":
                            {
                                searchSystem = NavigationService.Instance.GetMostRoute();
                            }
                            break;
                        case "nearest":
                            {
                                searchSystem = NavigationService.Instance.GetNearestRoute();
                            }
                            break;
                        case "next":
                            {
                                searchSystem = NavigationService.Instance.GetNextInRoute();
                            }
                            break;
                        case "route":
                            {
                                searchSystem = NavigationService.Instance.GetMissionsRoute();
                            }
                            break;
                        case "source":
                            {
                                searchSystem = NavigationService.Instance.GetSourceRoute();
                            }
                            break;
                        case "update":
                            {
                                searchSystem = NavigationService.Instance.UpdateRoute();
                            }
                            break;
                    }
                }
                else if (searchTypeSelection == "services")
                {
                    searchSystem = NavigationService.Instance.GetServiceRoute(searchQuerySelection);
                }
                else if (searchTypeSelection == "ship")
                {
                    switch (searchQuerySelection)
                    {
                        case "cancel":
                            {
                                NavigationService.Instance.CancelRoute();
                            }
                            break;
                        case "scoop":
                            {
                                ShipMonitor.JumpDetail detail = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).JumpDetails("total");
                                searchSystem = NavigationService.Instance.GetScoopRoute(detail.distance);
                            }
                            break;
                    }
                }

                Dispatcher?.Invoke(() =>
                {
                    searchButton.Foreground = Brushes.Black;
                    searchButton.FontWeight = FontWeights.Regular;

                    // If 'search system' found, send to clipboard
                    if (searchSystem != null)
                    {
                        Clipboard.SetData(DataFormats.Text, searchSystem);
                    }
                });
            })
            {
                IsBackground = true
            };
            searchThread.Start();
        }

        private void executeSelect(object sender, RoutedEventArgs e)
        {
            NavigationService.Instance.SetRoute(dropdownSearchSystem, dropdownSearchStation);
        }

        private void SearchSystemText_TextChanged(object sender, TextChangedEventArgs e)
        {
            void changeHandler()
            {
                // Reset the search station due to selecting new search system
                if (dropdownSearchStation != null)
                {
                    dropdownSearchStation = null;
                    searchStationDropDown.SelectedItem = Properties.NavigationMonitor.no_station;
                    ConfigureSearchStationOptions(null);
                }
            }
            searchSystemDropDown.TextDidChange(sender, e, dropdownSearchSystem, changeHandler);
        }

        private void SearchSystemDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            void changeHandler(string newValue)
            {
                // Update to new search system
                dropdownSearchSystem = newValue;

                // Update station options for new system
                ConfigureSearchStationOptions(dropdownSearchSystem);
            }
            searchSystemDropDown.SelectionDidChange(changeHandler);
        }

        private void SearchSystemDropDown_LostFocus(object sender, RoutedEventArgs e)
        {
            searchSystemDropDown.DidLoseFocus(oldValue: dropdownSearchSystem);
        }

        private void ConfigureSearchStationOptions(string system)
        {
            List<string> SearchStationOptions = new List<string>
                {
                    Properties.NavigationMonitor.no_station
                };

            if (system != null)
            {
                StarSystem SearchSystem = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(system, false);
                if (SearchSystem?.stations != null)
                {
                    foreach (Station station in SearchSystem.stations)
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
            if (dropdownSearchStation != searchStationName)
            {
                dropdownSearchStation = searchStationName == Properties.NavigationMonitor.no_station ? null : searchStationName;
            }
        }

        private void EnsureValidInteger(object sender, TextCompositionEventArgs e)
        {
            // Match valid characters
            Regex regex = new Regex(@"[0-9]");
            // Swallow the character doesn't match the regex
            e.Handled = !regex.IsMatch(e.Text);
        }
    }
}

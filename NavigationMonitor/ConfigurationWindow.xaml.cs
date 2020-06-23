using EddiCore;
using EddiEvents;
using EddiConfigService;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiNavigationService;
using EddiShipMonitor;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Linq;
using Utilities;

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
            maxSearchDistanceInt.Text = navConfig.maxSearchDistanceFromStarLs?.ToString(CultureInfo.InvariantCulture);
        }

        private void bookmarkLocation(object sender, RoutedEventArgs e)
        {
            bool isStation = false;
            string poi = null;
            decimal? latitude = null;
            decimal? longitude = null;
            bool landable = false;

            navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            StarSystem system = EDDI.Instance.CurrentStarSystem;
            Body body = EDDI.Instance.CurrentStellarBody;
            Station station = EDDI.Instance.CurrentStation;
            Status status = NavigationService.Instance.currentStatus;

            if (EDDI.Instance.Environment == Constants.ENVIRONMENT_LANDED)
            {
                if (EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP)
                {
                    latitude = (decimal)Math.Round((double)navConfig.latitude, 2);
                    longitude = (decimal)Math.Round((double)navConfig.longitude, 2);
                    poi = navConfig.poi;
                }
                else if (EDDI.Instance.Vehicle == Constants.VEHICLE_SRV)
                {
                    latitude = status.latitude;
                    longitude = status.longitude;

                    if (navConfig.poi != null)
                    {
                        // Get current distance from `Touchdown` POI
                        decimal distance = navigationMonitor().CalculateDistance(status);
                        if (distance < 5)
                        {
                            poi = navConfig.poi;
                        }
                    }
                }
                landable = true;
            }
            else if (EDDI.Instance.Environment == Constants.ENVIRONMENT_NORMAL_SPACE)
            {
                if (station != null)
                {
                    isStation = true;
                    poi = station.name;
                }
            }
            else if (EDDI.Instance.Environment == Constants.ENVIRONMENT_SUPERCRUISE)
            {
                if (status.near_surface)
                {
                    navigationMonitor().CalculateCoordinates(status, ref latitude, ref longitude);
                    landable = body?.landable ?? false;
                }
            }

            Bookmark bookmark = new Bookmark(system.systemname, system.x, system.y, system.z, body?.shortname, body?.radius, poi, isStation, latitude, longitude, landable);
            navConfig.bookmarks.Add(bookmark);
            ConfigService.Instance.navigationMonitorConfiguration = navConfig;
            EDDI.Instance.enqueueEvent(new BookmarkDetailsEvent(DateTime.Now, "location", system.systemname, body?.shortname, poi, isStation, latitude, longitude, landable));
        }

        private void bookmarkQuery(object sender, RoutedEventArgs e)
        {
            bool isStation = false;
            bool landable = false;
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            string systemName = navConfig.searchSystem;
            string stationName = navConfig.searchStation;
            StarSystem system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(systemName, false);

            if (stationName != null)
            {
                isStation = true;
                Station station = system.stations.FirstOrDefault(s => s.name == stationName);
                landable = station.IsPlanetary();
            }

            Bookmark bookmark = new Bookmark(systemName, system.x, system.y, system.z, null, null, stationName, isStation, null, null, landable);
            navConfig.bookmarks.Add(bookmark);
            ConfigService.Instance.navigationMonitorConfiguration = navConfig;
            EDDI.Instance.enqueueEvent(new BookmarkDetailsEvent(DateTime.Now, "query", systemName, null, stationName, isStation, null, null, landable));
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
                            Bookmark bookmark = new Bookmark();
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
                                                bookmark.system = fields[i];
                                            }
                                            break;
                                        case "x":
                                            {
                                                bookmark.x = decimal.Parse(fields[i]);
                                            }
                                            break;
                                        case "y":
                                            {
                                                bookmark.y = decimal.Parse(fields[i]);
                                            }
                                            break;
                                        case "z":
                                            {
                                                bookmark.z = decimal.Parse(fields[i]);
                                            }
                                            break;
                                        case "body":
                                            {
                                                bookmark.body = fields[i];
                                            }
                                            break;
                                        case "name":
                                            {
                                                bookmark.comment = fields[i];
                                            }
                                            break;
                                        case "catagory":
                                            {
                                                bookmark.catagory = fields[i];
                                            }
                                            break;
                                        case "radius":
                                            {
                                                bookmark.radius = decimal.Parse(fields[i]);
                                            }
                                            break;
                                        case "latitude":
                                            {
                                                bookmark.latitude = decimal.Parse(fields[i]);
                                            }
                                            break;
                                        case "longitude":
                                            {
                                                bookmark.longitude = decimal.Parse(fields[i]);
                                            }
                                            break;
                                    }
                                }
                            }
                            if (!header) { navConfig.bookmarks.Add(bookmark); }
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

            Bookmark bookmark = (Bookmark)((Button)e.Source).DataContext;

            string messageBoxText = Properties.NavigationMonitor.remove_message;
            string caption = Properties.NavigationMonitor.remove_caption;
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    {
                        // Remove the bookmark from the list
                        navigationMonitor()._RemoveBookmark(bookmark);
                        navConfig.bookmarks = navigationMonitor()?.bookmarks;
                        ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                    }
                    break;
            }
        }

        private void setBookmark(object sender, RoutedEventArgs e)
        {
            string station = null;
            Bookmark bookmark = (Bookmark)((Button)e.Source).DataContext;
            if (bookmark.isstation) { station = bookmark.poi; }

            NavigationService.Instance.SetRoute(bookmark.system, station);
        }

        private void updateBookmark(object sender, RoutedEventArgs e)
        {
            decimal? latitude = null;
            decimal? longitude = null;

            navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            StarSystem system = EDDI.Instance.CurrentStarSystem;
            Body body = EDDI.Instance.CurrentStellarBody;
            Station station = EDDI.Instance.CurrentStation;
            Status status = NavigationService.Instance.currentStatus;

            if (e.Source is DataGrid)
            {
                Bookmark bookmark = (Bookmark)((DataGrid)e.Source).CurrentItem;

                // Update only if current system matches the bookmarked system
                if (bookmark?.system == system.systemname)
                {
                    // Update latitude & longitude if current body matches the bookmarked body
                    if (body != null && body.shortname == bookmark.body)
                    {
                        if (EDDI.Instance.Environment == Constants.ENVIRONMENT_LANDED)
                        {
                            if (EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP)
                            {
                                latitude = (decimal)Math.Round((double)navConfig.latitude, 2);
                                longitude = (decimal)Math.Round((double)navConfig.longitude, 2);
                                if (bookmark.poi is null) { bookmark.poi = navConfig.poi; }
                            }
                            else if (EDDI.Instance.Vehicle == Constants.VEHICLE_SRV)
                            {
                                latitude = status.latitude;
                                longitude = status.longitude;

                                if (navConfig.poi != null)
                                {
                                    // Get current distance from `Touchdown` POI
                                    decimal distance = navigationMonitor().CalculateDistance(status);
                                    if (distance < 5)
                                    {
                                        bookmark.poi = navConfig.poi;
                                    }
                                }
                            }
                            bookmark.landable = true;
                        }
                        else if (EDDI.Instance.Environment == Constants.ENVIRONMENT_SUPERCRUISE)
                        {
                            if (status.near_surface)
                            {
                                navigationMonitor().CalculateCoordinates(status, ref latitude, ref longitude);
                                bookmark.landable = body.landable ?? false;
                            }
                        }
                    }

                    // Update if a station is instanced and a body was not previously bookmarked
                    else if (station != null && bookmark.body is null)
                    {
                        if (EDDI.Instance.Environment == Constants.ENVIRONMENT_NORMAL_SPACE)
                        {
                            bookmark.isstation = true;
                            bookmark.poi = station.name;
                        }
                    }

                    ConfigService.Instance.navigationMonitorConfiguration = navConfig;
                    EDDI.Instance.enqueueEvent(new BookmarkDetailsEvent(DateTime.Now, "update", system.systemname, body?.shortname, bookmark.poi,
                        bookmark.isstation, latitude, longitude, bookmark.landable));
                }
            }
        }

        private void prioritizeOrbitalStationsEnabled(object sender, RoutedEventArgs e)
        {
            updateCheckbox();
        }

        private void prioritizeOrbitalStationsDisabled(object sender, RoutedEventArgs e)
        {
            updateCheckbox();
        }

        private void updateCheckbox()
        {
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            bool isChecked = prioritizeOrbitalStations.IsChecked.Value;
            if (navConfig.prioritizeOrbitalStations != isChecked)
            {
                navConfig.prioritizeOrbitalStations = isChecked;
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
            List<string> SearchQueryOptions = new List<string> {};

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

        private void executeSet(object sender, RoutedEventArgs e)
        {
            string station = null;
            navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            Bookmark bookmark = (Bookmark)((Button)e.Source).DataContext;
            if (bookmark.isstation) { station = bookmark.poi; }
            NavigationService.Instance.SetRoute(bookmark.system, station);
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

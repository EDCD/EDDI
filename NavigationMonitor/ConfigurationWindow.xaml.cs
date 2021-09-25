using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiNavigationService;
using EddiStatusMonitor;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Utilities;

namespace EddiNavigationMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        private NavigationMonitorConfiguration navConfig => ConfigService.Instance.navigationMonitorConfiguration;
        private string searchTypeSelection = String.Empty;
        private string searchQuerySelection = String.Empty;
        private string dropdownSearchSystem = null;
        private string dropdownSearchStation = null;

        private Status currentStatus { get; set; }

        private readonly List<string> searchType = new List<string> {
            "crime",
            "galaxy",
            "missions",
            "services"
        };

        private readonly List<string> searchQuery = new List<string> {
            "encoded",
            "expiring",
            "farthest",
            "guardian",
            "human",
            "facilitator",
            "manufactured",
            "most",
            "nearest",
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
            bookmarksData.ItemsSource = navigationMonitor().bookmarks;

            ConfigureSearchTypeOptions();
            searchTypeDropDown.SelectedItem = Properties.NavigationMonitor.search_type_missions;
            searchQueryDropDown.SelectedItem = Properties.NavigationMonitor.search_query_missions_route;

            prioritizeOrbitalStations.IsChecked = navConfig.prioritizeOrbitalStations;
            maxSearchDistanceInt.Text = (navConfig.maxSearchDistanceFromStarLs ?? 0).ToString(CultureInfo.InvariantCulture);

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

            if (EDDI.Instance.CurrentStarSystem != null)
            {
                StarSystem currentSystem = EDDI.Instance.CurrentStarSystem;
                Body currentBody = EDDI.Instance.CurrentStellarBody;
                Station currentStation = EDDI.Instance.CurrentStation;

                if (EDDI.Instance.Environment == Constants.ENVIRONMENT_LANDED)
                {
                    if (EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP && navConfig.tdLat != null && navConfig.tdLong != null)
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
                                decimal? distanceKm = SurfaceDistanceKm(currentStatus, navConfig?.tdLat, navConfig?.tdLong);
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
                        SurfaceCoordinates(currentStatus, out latitude, out longitude);
                        landable = currentBody?.landable ?? false;
                    }
                }
                else if (EDDI.Instance.Environment == Constants.ENVIRONMENT_SUPERCRUISE)
                {
                    if (currentStatus != null && currentStatus.near_surface && currentBody != null)
                    {
                        SurfaceCoordinates(currentStatus, out latitude, out longitude);
                        landable = currentBody?.landable ?? false;
                    }
                }

                NavBookmark navBookmark = new NavBookmark(currentSystem.systemname, currentSystem.x, currentSystem.y, currentSystem.z,
                    currentBody?.bodyname, currentBody?.radius, poi, isStation, latitude, longitude, landable);
                navigationMonitor().bookmarks.Add(navBookmark);
                navigationMonitor().writeBookmarks();
                EDDI.Instance.enqueueEvent(new BookmarkDetailsEvent(DateTime.UtcNow, "location", navBookmark));
            }
        }

        private void bookmarkQuery(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(navConfig.searchSystem)) { return; }

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
            navigationMonitor().bookmarks.Add(navBookmark);
            navigationMonitor().writeBookmarks();
            EDDI.Instance.enqueueEvent(new BookmarkDetailsEvent(DateTime.UtcNow, "query", navBookmark));
        }

        private void bookmarkUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the bookmark list
            navigationMonitor()?.writeBookmarks();
        }

        private void exportBookmarks(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void importBookmarks(object sender, RoutedEventArgs e)
        {
            string filename = Constants.DATA_DIR + @"\import.csv";
            bool header = true;
            List<string> headerNames = new List<string>();

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
                            string[] fields = parser.ReadFields() ?? new string[0];
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

                            if (!header)
                            {
                                navigationMonitor().bookmarks.Add(navBookmark);
                            }
                            header = false;
                        }
                        catch (MalformedLineException ex)
                        {
                            Logging.Error("Line " + ex.Message + " is invalid. Skipping: ", ex);
                        }
                    }
                }
                navigationMonitor().writeBookmarks();
            }
        }

        private void removeBookmark(object sender, RoutedEventArgs e)
        {
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
                        navConfig.ToFile();
                    }
                    break;
            }
        }

        private void updateBookmark(object sender, RoutedEventArgs e)
        {
            var currentSystem = EDDI.Instance.CurrentStarSystem;
            var currentBody = EDDI.Instance.CurrentStellarBody;
            var currentStation = EDDI.Instance.CurrentStation;

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
                            if (EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP && navConfig.tdLat != null && navConfig.tdLong != null)
                            {
                                navBookmark.latitude = (decimal)Math.Round((double)navConfig.tdLat, 4);
                                navBookmark.longitude = (decimal)Math.Round((double)navConfig.tdLong, 4);
                                if (navBookmark.poi is null) { navBookmark.poi = navConfig.tdPOI; }
                            }
                            else if (EDDI.Instance.Vehicle == Constants.VEHICLE_SRV)
                            {
                                navBookmark.latitude = currentStatus.latitude;
                                navBookmark.longitude = currentStatus.longitude;

                                if (navConfig.tdPOI != null)
                                {
                                    // Get current distance from `Touchdown` POI
                                    decimal? distanceKm = SurfaceDistanceKm(currentStatus, navConfig?.tdLat, navConfig?.tdLong);
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
                                SurfaceCoordinates(currentStatus, out decimal? latitude, out decimal? longitude);
                                navBookmark.landable = currentBody.landable ?? false;
                                navBookmark.latitude = latitude;
                                navBookmark.longitude = longitude;
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

                    navConfig.ToFile();
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
            bool isChecked = prioritizeOrbitalStations.IsChecked ?? false;
            if (navConfig.prioritizeOrbitalStations != isChecked)
            {
                navConfig.prioritizeOrbitalStations = isChecked;
                navConfig.ToFile();
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
                int? distance = string.IsNullOrWhiteSpace(maxSearchDistanceInt.Text)
                    ? 10000 : Convert.ToInt32(maxSearchDistanceInt.Text, CultureInfo.InvariantCulture);
                if (distance != navConfig.maxSearchDistanceFromStarLs)
                {
                    navConfig.maxSearchDistanceFromStarLs = distance;
                    navConfig.ToFile();
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
                case "galaxy":
                    {
                        searchQueryDropDown.SelectedItem = Properties.NavigationMonitor.search_query_galaxy_scoop;
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
            }
        }

        private void ConfigureSearchQueryOptions(string type)
        {
            var SearchQueryOptions = new List<string>();
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

        private async void executeSearch(object sender, RoutedEventArgs e)
        {
            Button searchButton = (Button)sender;
            searchButton.Foreground = Brushes.DarkBlue;
            searchButton.FontWeight = FontWeights.Bold;

            string resultSystem = string.Empty;
            string resultStation = string.Empty;

            var search = Task.Run(() =>
            {
                RouteDetailsEvent @event = null;
                if (searchTypeSelection == "crime")
                {
                    switch (searchQuerySelection)
                    {
                        case "facilitator":
                        {
                            @event = NavigationService.Instance.NavQuery(QueryTypes.facilitator);
                            break;
                        }
                    }
                }
                else if (searchTypeSelection == "missions")
                {
                    switch (searchQuerySelection)
                    {
                        case "expiring":
                        {
                            @event = NavigationService.Instance.NavQuery(QueryTypes.expiring);
                            break;
                        }
                        case "farthest":
                        {
                            @event = NavigationService.Instance.NavQuery(QueryTypes.farthest);
                            break;
                        }
                        case "most":
                        {
                            @event = NavigationService.Instance.NavQuery(QueryTypes.most);
                            break;
                        }
                        case "nearest":
                        {
                            @event = NavigationService.Instance.NavQuery(QueryTypes.nearest);
                            break;
                        }
                        case "route":
                        {
                            @event = NavigationService.Instance.NavQuery(QueryTypes.route);
                            break;
                        }
                        case "source":
                        {
                            @event = NavigationService.Instance.NavQuery(QueryTypes.source);
                            break;
                        }
                        case "update":
                        {
                            @event = NavigationService.Instance.NavQuery(QueryTypes.update);
                            break;
                        }
                    }
                }
                else if (searchTypeSelection == "services")
                {
                    @event = NavigationService.Instance.GetServiceSystem(searchQuerySelection);
                }
                else if (searchTypeSelection == "galaxy")
                {
                    switch (searchQuerySelection)
                    {
                        case "scoop":
                        {
                            @event = NavigationService.Instance.NavQuery(QueryTypes.scoop);
                            break;
                        }
                    }
                }
                if (@event != null)
                {
                    resultSystem = @event.system;
                    resultStation = @event.station;
                    EDDI.Instance?.enqueueEvent(@event);
                }
            });

            // Update our UI to match our results
            await Task.WhenAll(search);
            if (searchSystemDropDown.Text != resultSystem)
            {
                dropdownSearchSystem = resultSystem;
                searchSystemDropDown.Text = resultSystem;
                ConfigureSearchStationOptions(resultSystem);
            }
            if (searchStationDropDown.Text != resultStation)
            {
                dropdownSearchStation = resultStation;
                searchStationDropDown.Text = !string.IsNullOrEmpty(resultStation) ? resultStation : Properties.NavigationMonitor.no_station;
            }
            searchButton.Foreground = Brushes.Black;
            searchButton.FontWeight = FontWeights.Normal;
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

        public static void SurfaceCoordinates(Status curr, out decimal? destinationLatitude, out decimal? destinationLongitude)
        {
            Functions.SurfaceCoordinates(curr.altitude, curr.planetradius, curr.slope, curr.heading, curr.latitude, curr.longitude, out destinationLatitude, out destinationLongitude);
        }

        public static decimal? SurfaceDistanceKm(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            return Functions.SurfaceDistanceKm(curr.planetradius, curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude);
        }
    }
}

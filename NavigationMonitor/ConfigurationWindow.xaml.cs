using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiNavigationService;
using EddiStatusMonitor;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
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
        private string searchTypeSelection = string.Empty;
        private string searchQuerySelection = string.Empty;
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

            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            prioritizeOrbitalStations.IsChecked = navConfig.prioritizeOrbitalStations;
            maxSearchDistanceInt.Text = (navConfig.maxSearchDistanceFromStarLs ?? 0).ToString(CultureInfo.InvariantCulture);

            StatusMonitor.StatusUpdatedEvent += OnStatusUpdated;
        }

        private void OnStatusUpdated(object sender, EventArgs e)
        {
            if (sender is Status status)
            {
                currentStatus = status;
                foreach (var bookmark in navigationMonitor().bookmarks)
                {
                    if (status.bodyname == bookmark.bodyname)
                    {
                        bookmark.heading = SurfaceConstantHeadingDegrees(status, bookmark.latitude, bookmark.longitude);
                        bookmark.distanceKm = SurfaceConstantHeadingDistanceKm(status, bookmark.latitude, bookmark.longitude);
                    }
                    else if (bookmark.heading != null || bookmark.distanceKm != null)
                    {
                        bookmark.heading = null;
                        bookmark.distanceKm = null;
                    }
                }
            }
        }

        private void bookmarkLocation(object sender, RoutedEventArgs e)
        {
            bool isStation = false;
            string poi = null;
            decimal? latitude = null;
            decimal? longitude = null;
            bool landable = false;
            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            if (EDDI.Instance.CurrentStarSystem != null)
            {
                StarSystem currentSystem = EDDI.Instance.CurrentStarSystem;
                Station currentStation = EDDI.Instance.CurrentStation;

                if (EDDI.Instance.Environment == Constants.ENVIRONMENT_LANDED || EDDI.Instance.Environment == Constants.ENVIRONMENT_DOCKED)
                {
                    if (EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP || EDDI.Instance.Vehicle == Constants.VEHICLE_MULTICREW || EDDI.Instance.Vehicle == Constants.VEHICLE_TAXI)
                    {
                        if (navConfig.tdLat != null && navConfig.tdLong != null)
                        {
                            latitude = (decimal)Math.Round((double)navConfig.tdLat, 4);
                            longitude = (decimal)Math.Round((double)navConfig.tdLong, 4);
                            poi = navConfig.tdPOI;
                        }
                    }
                    else if (EDDI.Instance.Vehicle == Constants.VEHICLE_SRV || EDDI.Instance.Vehicle == Constants.VEHICLE_LEGS || EDDI.Instance.Vehicle == Constants.VEHICLE_FIGHTER)
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
                        landable = currentStatus?.near_surface ?? false;
                    }
                }
                else if (EDDI.Instance.Environment == Constants.ENVIRONMENT_NORMAL_SPACE)
                {
                    if (currentStation != null)
                    {
                        isStation = true;
                        poi = currentStation.name;
                    }
                    if (currentStatus != null && currentStatus.near_surface)
                    {
                        SurfaceCoordinates(currentStatus, out latitude, out longitude);
                        landable = true;
                    }
                }
                else if (EDDI.Instance.Environment == Constants.ENVIRONMENT_SUPERCRUISE)
                {
                    if (currentStatus != null && currentStatus.near_surface)
                    {
                        SurfaceCoordinates(currentStatus, out latitude, out longitude);
                        landable = true;
                    }
                }

                NavBookmark navBookmark = new NavBookmark(currentSystem.systemname, currentSystem.x, currentSystem.y, currentSystem.z,
                    currentStatus?.bodyname, poi, isStation, latitude, longitude, landable);
                navigationMonitor().bookmarks.Add(navBookmark);
                navigationMonitor().writeBookmarks();
                EDDI.Instance.enqueueEvent(new BookmarkDetailsEvent(DateTime.UtcNow, "location", navBookmark));
            }
        }

        private void bookmarkQuery(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(dropdownSearchSystem)) { return; }

            string systemName = dropdownSearchSystem;
            StarSystem system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(systemName, false);

            string stationName = dropdownSearchStation;
            bool isStation = false;
            bool landable = false;
            if (stationName != null)
            {
                isStation = true;
                landable = system.stations.FirstOrDefault(s => s.name == stationName)?.IsPlanetary() ?? false;
            }

            NavBookmark navBookmark = new NavBookmark(systemName, system?.x, system?.y, system?.z, null, stationName, isStation, null, null, landable);
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
            // Select bookmarks
            var selectedBookmarks = navigationMonitor().bookmarks;

            // Package up bookmarks
            var sb = new StringBuilder();
            foreach (var navBookmark in selectedBookmarks)
            {
                sb.AppendLine(JsonConvert.SerializeObject(navBookmark));
            }

            // Export to a file (.jsonl format)
            var fileDialog = new SaveFileDialog
            {
                InitialDirectory = Constants.DATA_DIR, 
                AddExtension = true, 
                OverwritePrompt = true, 
                ValidateNames = true,
                DefaultExt = ".bkmks",
                Filter = "Bookmark files|*.bkmks",
                FilterIndex = 0
            };
            if (fileDialog.ShowDialog() ?? false)
            {
                Files.Write(fileDialog.FileName, sb.ToString());
            }
        }

        private async void importBookmarks(object sender, RoutedEventArgs e)
        {
            // Read bookmarks from selected files (.jsonl format)
            var fileDialog = new OpenFileDialog
            {
                InitialDirectory = Constants.DATA_DIR, 
                Multiselect = true, 
                DefaultExt = ".bkmks",
                Filter = "Bookmark files|*.bkmks",
                FilterIndex = 0
            };
            if (fileDialog.ShowDialog() ?? false)
            {
                var importedBookmarks = new HashSet<NavBookmark>();
                foreach (var fileName in fileDialog.FileNames)
                {
                    var fileContents = Files.Read(fileName);
                    using (var sr = new StringReader(fileContents))
                    {
                        string line;
                        while ((line = await sr.ReadLineAsync()) != null)
                        {
                            try
                            {
                                var navBookmark = JsonConvert.DeserializeObject<NavBookmark>(line);
                                if (importedBookmarks.Add(navBookmark))
                                { }
                                else
                                {
                                    Logging.Warn("Failed to import bookmark, duplicate entry");
                                }
                            }
                            catch (Exception exception)
                            {
                                var data = new Dictionary<string, object>
                                {
                                    {"Bookmark", line},
                                    {"Exception", exception}
                                };
                                Logging.Warn("Failed to import bookmark", data);
                            }
                        }
                    }
                }

                // Select bookmarks
                var selectedBookmarks = importedBookmarks;

                // Add bookmarks to Navigation Monitor
                foreach (var navBookmark in selectedBookmarks)
                {
                    navigationMonitor().bookmarks.Add(navBookmark);
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
                        navigationMonitor().writeBookmarks();
                    }
                    break;
            }
        }

        private void updateBookmark(object sender, RoutedEventArgs e)
        {
            var currentSystem = EDDI.Instance.CurrentStarSystem;
            var currentBody = EDDI.Instance.CurrentStellarBody;
            var currentStation = EDDI.Instance.CurrentStation;
            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;

            if (e.Source is Button button)
            {
                var navBookmark = (NavBookmark)button.DataContext;

                // Update only if current system matches the bookmarked system
                if (navBookmark != null && navBookmark.systemname == currentSystem.systemname)
                {
                    // Update latitude & longitude if current body matches the bookmarked body
                    if (currentBody?.bodyname == navBookmark.bodyname || currentStation?.name == navBookmark.poi)
                    {
                        if (EDDI.Instance.Environment == Constants.ENVIRONMENT_LANDED || EDDI.Instance.Environment == Constants.ENVIRONMENT_DOCKED)
                        {
                            if (EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP || EDDI.Instance.Vehicle == Constants.VEHICLE_MULTICREW || EDDI.Instance.Vehicle == Constants.VEHICLE_TAXI)
                            {
                                if (navConfig.tdLat != null && navConfig.tdLong != null)
                                {
                                    navBookmark.latitude = (decimal) Math.Round((double) navConfig.tdLat, 4);
                                    navBookmark.longitude = (decimal) Math.Round((double) navConfig.tdLong, 4);
                                    if (navBookmark.poi is null)
                                    {
                                        navBookmark.poi = navConfig.tdPOI;
                                    }
                                }
                            }
                            else if (EDDI.Instance.Vehicle == Constants.VEHICLE_SRV || EDDI.Instance.Vehicle == Constants.VEHICLE_LEGS || EDDI.Instance.Vehicle == Constants.VEHICLE_FIGHTER)
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
                            navBookmark.bodyname = currentStatus.bodyname;
                        }
                        else if (EDDI.Instance.Environment == Constants.ENVIRONMENT_SUPERCRUISE)
                        {
                            if (currentStatus.near_surface)
                            {
                                SurfaceCoordinates(currentStatus, out decimal? latitude, out decimal? longitude);
                                navBookmark.landable = currentBody?.landable ?? false;
                                navBookmark.latitude = latitude;
                                navBookmark.longitude = longitude;
                                navBookmark.bodyname = currentStatus.bodyname;
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

                    navigationMonitor().writeBookmarks();
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
            var navConfig = ConfigService.Instance.navigationMonitorConfiguration;
            bool isChecked = prioritizeOrbitalStations.IsChecked ?? false;
            if (navConfig.prioritizeOrbitalStations != isChecked)
            {
                navConfig.prioritizeOrbitalStations = isChecked;
                navigationMonitor().writeBookmarks();
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
                    navigationMonitor().writeBookmarks();
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
                if (Enum.TryParse(searchQuerySelection, true, out QueryTypes result))
                {
                    @event = NavigationService.Instance.NavQuery(result);
                }
                if (@event == null) { return; }
                resultSystem = @event.system;
                resultStation = @event.station;
                EDDI.Instance?.enqueueEvent(@event);
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

        private static void SurfaceCoordinates(Status curr, out decimal? destinationLatitude, out decimal? destinationLongitude)
        {
            Functions.SurfaceCoordinates(curr.altitude, curr.planetradius, curr.slope, curr.heading, curr.latitude, curr.longitude, out destinationLatitude, out destinationLongitude);
        }

        private static decimal? SurfaceDistanceKm(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            return Functions.SurfaceDistanceKm(curr.planetradius, curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude);
        }

        private static decimal? SurfaceConstantHeadingDegrees(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            var radiusMeters = curr.planetradius ?? EDDI.Instance?.CurrentStarSystem?.bodies
                ?.FirstOrDefault(b => b.bodyname == curr.bodyname)
                ?.radius * 1000;
            return Functions.SurfaceConstantHeadingDegrees(radiusMeters, curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude) ?? 0;
        }

        private static decimal? SurfaceConstantHeadingDistanceKm(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            var radiusMeters = curr.planetradius ?? EDDI.Instance?.CurrentStarSystem?.bodies
                ?.FirstOrDefault(b => b.bodyname == curr.bodyname)
                ?.radius * 1000;
            return Functions.SurfaceConstantHeadingDistanceKm(radiusMeters, curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude) ?? 0;
        }
    }
}

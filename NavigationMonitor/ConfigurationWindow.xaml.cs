using Eddi;
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
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using Utilities;

namespace EddiNavigationMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        private Status currentStatus { get; set; }

        private NavigationMonitor navigationMonitor()
        {
            return (NavigationMonitor)EDDI.Instance.ObtainMonitor("Navigation monitor");
        }

        public ConfigurationWindow()
        {
            InitializeComponent();
            bookmarksData.ItemsSource = navigationMonitor().Bookmarks;
            navRouteData.ItemsSource = navigationMonitor().NavRouteList.Waypoints;
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


            if (navigationMonitor().PlottedRouteList?.GuidanceEnabled ?? false)
            {
                GuidanceButton.Content = Properties.NavigationMonitor.disable_guidance_button;
                GuidanceButton.ToolTip = Properties.NavigationMonitor.disable_guidance_button_tooltip;
            }
            else
            {
                GuidanceButton.Content = Properties.NavigationMonitor.enable_guidance_button;
                GuidanceButton.ToolTip = Properties.NavigationMonitor.enable_guidance_button_tooltip;
            }

            StatusMonitor.StatusUpdatedEvent += OnStatusUpdated;
            NavigationService.Instance.PropertyChanged += OnNavServiceChange;
        }

        private void OnStatusUpdated(object sender, EventArgs e)
        {
            if (sender is Status status)
            {
                currentStatus = status;
            }
        }

        private void OnNavServiceChange(object sender, PropertyChangedEventArgs e)
        {
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

        private void bookmarkLocation(object sender, RoutedEventArgs e)
        {
            var isStation = false;
            string poi = null;
            decimal? latitude = null;
            decimal? longitude = null;
            var nearby = false;
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
                            nearby = true;
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
                                    nearby = true;
                                }
                            }
                        }
                    }
                }
                else if (EDDI.Instance.Environment == Constants.ENVIRONMENT_DOCKED)
                {
                    if (currentStation != null)
                    {
                        isStation = true;
                        poi = currentStation.name;
                        latitude = currentStatus?.latitude;
                        longitude = currentStatus?.longitude;
                        nearby = true;
                    }
                }
                else if (EDDI.Instance.Environment == Constants.ENVIRONMENT_NORMAL_SPACE)
                {
                    if (currentStation != null)
                    {
                        isStation = true;
                        poi = currentStation.name;
                        nearby = true;
                    }
                    if (currentStatus != null && currentStatus.near_surface)
                    {
                        GetSurfaceCoordinates(currentStatus, out latitude, out longitude);
                    }
                }
                else if (EDDI.Instance.Environment == Constants.ENVIRONMENT_SUPERCRUISE)
                {
                    if (currentStatus != null && currentStatus.near_surface)
                    {
                        GetSurfaceCoordinates(currentStatus, out latitude, out longitude);
                    }
                }

                NavBookmark navBookmark = new NavBookmark(currentSystem.systemname, currentSystem.x, currentSystem.y, currentSystem.z,
                    currentStatus?.bodyname, poi, isStation, latitude, longitude, nearby);
                navigationMonitor().Bookmarks.Add(navBookmark);
                navigationMonitor().WriteNavConfig();
                EDDI.Instance.enqueueEvent(new BookmarkDetailsEvent(DateTime.UtcNow, "location", navBookmark));
            }
        }

        private void addBookmark(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is NavWaypoint navWaypoint)
                {
                    var navBookmark = new NavBookmark(navWaypoint.systemName, navWaypoint.x, navWaypoint.y, navWaypoint.z, null, null, false, null, null, false);
                    navigationMonitor().Bookmarks.Add(navBookmark);
                    navigationMonitor().WriteNavConfig();
                    SwitchToTab(Properties.NavigationMonitor.tab_bookmarks);
                    EDDI.Instance.enqueueEvent(new BookmarkDetailsEvent(DateTime.UtcNow, "add", navBookmark));
                }
            }
        }

        private void SwitchToTab(string tabHeader)
        {
            int index = 0;
            for (int i = 0; i < tabControl.Items.Count; i++)
            {
                if ((TabItem)tabControl.Items[i] is TabItem tabItem
                    && tabItem.Header.Equals(tabHeader))
                {
                    index = i;
                    break;
                }
            }
            tabControl.SelectedIndex = index;
        }

        private void bookmarkUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the bookmark list
            navigationMonitor()?.WriteNavConfig();
        }

        private void exportBookmarks(object sender, RoutedEventArgs e)
        {
            // Select bookmarks
            var bookmarksSelector = new BookmarkSelector(bookmarksData.Items.SourceCollection as IEnumerable<NavBookmark>);
            EDDI.Instance.SpeechResponderModalWait = true;
            bookmarksSelector.ShowDialog();
            EDDI.Instance.SpeechResponderModalWait = false;
            if (bookmarksSelector.DialogResult ?? false)
            {
                // Package up bookmarks (in .jsonl format)
                var sb = new StringBuilder();
                foreach (var navBookmark in bookmarksSelector.SelectedBookmarks)
                {
                    sb.AppendLine(JsonConvert.SerializeObject(navBookmark));
                }
                if (sb.Length <= 0) { return; }

                // Export to a file
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
                // Import bookmarks
                var newBookmarks = new List<NavBookmark>();
                foreach (var fileName in fileDialog.FileNames)
                {
                    if (!fileName.EndsWith(".bkmks")) { continue; }
                    var fileContents = Files.Read(fileName);
                    using (var sr = new StringReader(fileContents))
                    {
                        string line;
                        while ((line = await sr.ReadLineAsync()) != null)
                        {
                            NavBookmark navBookmark = null;
                            try
                            {
                                navBookmark = JsonConvert.DeserializeObject<NavBookmark>(line);
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
                            if (navBookmark != null)
                            {
                                newBookmarks.Add(navBookmark);
                            }
                        }
                    }
                }

                // Select bookmarks
                var bookmarksSelector = new BookmarkSelector(newBookmarks);
                EDDI.Instance.SpeechResponderModalWait = true;
                bookmarksSelector.ShowDialog();
                EDDI.Instance.SpeechResponderModalWait = false;

                // Add bookmarks to Navigation Monitor (filtering out any duplicated bookmarks)
                lock (NavigationMonitor.navConfigLock)
                {
                    foreach (var navBookmark in bookmarksSelector.SelectedBookmarks)
                    {
                        if (!navigationMonitor().Bookmarks.ToList().Any(b => b.DeepEquals(navBookmark)))
                        {
                            navigationMonitor().Bookmarks.Add(navBookmark);
                        }
                    }
                    navigationMonitor().WriteNavConfig();
                }
            }
        }

        private void removeBookmark(object sender, RoutedEventArgs e)
        {
            var index = bookmarksData.SelectedIndex;
            var bookmark = bookmarksData.SelectedItem as NavBookmark;
            string messageBoxText = Properties.NavigationMonitor.remove_message;
            string caption = Properties.NavigationMonitor.remove_caption;
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    {
                        // Remove the bookmark from the list
                        navigationMonitor().RemoveBookmarkAt(index);
                        navigationMonitor().WriteNavConfig();
                        EDDI.Instance.enqueueEvent(new BookmarkDetailsEvent(DateTime.UtcNow, "remove", bookmark));
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
                            navBookmark.bodyname = currentStatus.bodyname;
                        }
                        else if (EDDI.Instance.Environment == Constants.ENVIRONMENT_SUPERCRUISE)
                        {
                            if (currentStatus.near_surface)
                            {
                                GetSurfaceCoordinates(currentStatus, out decimal? latitude, out decimal? longitude);
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

                    navBookmark.nearby = true;
                    navigationMonitor().WriteNavConfig();
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

        private void nearbyRadius_KeyUp(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is NavBookmark selectedBookmark)
            {
                var arrivalRadiusMeters = long.Parse(textBox.Text);
                if (arrivalRadiusMeters != selectedBookmark.arrivalRadiusMeters)
                {
                    selectedBookmark.arrivalRadiusMeters = arrivalRadiusMeters;
                    navigationMonitor().CheckBookmarkPosition(selectedBookmark, currentStatus, false);
                    navigationMonitor().WriteNavConfig();
                }
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

        private static void GetSurfaceCoordinates(Status curr, out decimal? destinationLatitude, out decimal? destinationLongitude)
        {
            Functions.SurfaceCoordinates(curr.altitude, curr.planetradius, curr.slope, curr.heading, curr.latitude, curr.longitude, out destinationLatitude, out destinationLongitude);
        }

        private static decimal? SurfaceDistanceKm(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            return Functions.SurfaceDistanceKm(curr.planetradius, curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude);
        }

        private void RowDetailsButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton toggleButton)
            {
                DataGridRow selectedRow = DataGridRow.GetRowContainingElement(toggleButton);
                if (selectedRow != null)
                {
                    if (toggleButton.IsChecked ?? false)
                    {
                        toggleButton.Content = "⯆";
                        selectedRow.DetailsVisibility = Visibility.Visible;
                    }
                    else
                    {
                        toggleButton.Content = "⯈";
                        selectedRow.DetailsVisibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void UseStraightestPathButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.DataContext is NavBookmark selectedBookmark)
            {
                if (checkBox.IsChecked != selectedBookmark.useStraightPath)
                {
                    selectedBookmark.useStraightPath = checkBox.IsChecked ?? false;
                    navigationMonitor().CheckBookmarkPosition(selectedBookmark, currentStatus, false);
                    navigationMonitor().WriteNavConfig();
                }
            }
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex()).ToString();
        }

        private void GuidanceButton_Click(object sender, RoutedEventArgs e)
        {
            if (navigationMonitor().PlottedRouteList?.GuidanceEnabled ?? false)
            {
                GuidanceButton.Content = Properties.NavigationMonitor.enable_guidance_button;
                GuidanceButton.ToolTip = Properties.NavigationMonitor.enable_guidance_button_tooltip;
                EDDI.Instance?.enqueueEvent(NavigationService.Instance.NavQuery(QueryType.cancel));
            }
            else
            {
                GuidanceButton.Content = Properties.NavigationMonitor.disable_guidance_button;
                GuidanceButton.ToolTip = Properties.NavigationMonitor.disable_guidance_button_tooltip;
                EDDI.Instance?.enqueueEvent(NavigationService.Instance.NavQuery(QueryType.set));
            }
        }

        private void ClearRouteButton_Click(object sender, RoutedEventArgs e)
        {
            if (plottedRouteData.Items.Count > 0)
            {
                navigationMonitor().PlottedRouteList.Waypoints.Clear();
                navigationMonitor().WriteNavConfig();
            }
        }
    }
}

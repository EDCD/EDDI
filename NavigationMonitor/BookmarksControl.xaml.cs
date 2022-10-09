﻿using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiStatusMonitor;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class BookmarksControl : UserControl
    {
        private Status currentStatus { get; set; }

        private NavigationMonitor navigationMonitor()
        {
            return (NavigationMonitor)EDDI.Instance.ObtainMonitor("Navigation monitor");
        }

        public BookmarksControl()
        {
            InitializeComponent();
            bookmarksData.ItemsSource = navigationMonitor().Bookmarks;
            StatusMonitor.StatusUpdatedEvent += OnStatusUpdated;
        }
        private void OnStatusUpdated(object sender, EventArgs e)
        {
            if (sender is Status status)
            {
                currentStatus = status;
            }
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

        public void addBookmark(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is NavWaypoint navWaypoint)
                {
                    var navBookmark = new NavBookmark(navWaypoint.systemName, navWaypoint.x, navWaypoint.y, navWaypoint.z, null, null, false, null, null, false);
                    navigationMonitor().Bookmarks.Add(navBookmark);
                    navigationMonitor().WriteNavConfig();
                    EDDI.Instance.enqueueEvent(new BookmarkDetailsEvent(DateTime.UtcNow, "add", navBookmark));
                }
            }
        }

        private void removeBookmark(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var bookmark = button.DataContext as NavBookmark;
                var index = navigationMonitor().Bookmarks.IndexOf(bookmark);
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
                                    navBookmark.latitude = (decimal)Math.Round((double)navConfig.tdLat, 4);
                                    navBookmark.longitude = (decimal)Math.Round((double)navConfig.tdLong, 4);
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

        private void EnsureValidInteger(object sender, TextCompositionEventArgs e)
        {
            // Match valid characters
            Regex regex = new Regex(@"[0-9]");
            // Swallow the character doesn't match the regex
            e.Handled = !regex.IsMatch(e.Text);
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

        private static void GetSurfaceCoordinates(Status curr, out decimal? destinationLatitude, out decimal? destinationLongitude)
        {
            Functions.SurfaceCoordinates(curr.altitude, curr.planetradius, curr.slope, curr.heading, curr.latitude, curr.longitude, out destinationLatitude, out destinationLongitude);
        }

        private static decimal? SurfaceDistanceKm(Status curr, decimal? bookmarkLatitude, decimal? bookmarkLongitude)
        {
            return Functions.SurfaceDistanceKm(curr.planetradius, curr.latitude, curr.longitude, bookmarkLatitude, bookmarkLongitude);
        }
    }
}

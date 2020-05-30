using Eddi;
using EddiDataDefinitions;
using EddiNavigationService;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
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
        private NavigationMonitor navigationMonitor()
        {
            return (NavigationMonitor)EDDI.Instance.ObtainMonitor("Navigation monitor");
        }

        public ConfigurationWindow()
        {
            InitializeComponent();
            bookmarksData.ItemsSource = navigationMonitor()?.bookmarks;
        }

        private void addBookmark(object sender, RoutedEventArgs e)
        {
            Bookmark bookmark = new Bookmark(Properties.NavigationMonitor.blank_bookmark, null, null, null, null, false);
            navigationMonitor()?.bookmarks.Add(bookmark);
            navigationMonitor()?.writeBookmarks();
        }

        private void removeBookmark(object sender, RoutedEventArgs e)
        {
            Bookmark bookmark = (Bookmark)((Button)e.Source).DataContext;
            navigationMonitor()?._RemoveBookmark(bookmark);
            navigationMonitor()?.writeBookmarks();
        }

        private void updateBookmark(object sender, RoutedEventArgs e)
        {

        }

        private void setCourse(object sender, RoutedEventArgs e)
        {

        }

        private void bookmarksUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the cargo monitor's information
            navigationMonitor()?.writeBookmarks();
        }
    }
}

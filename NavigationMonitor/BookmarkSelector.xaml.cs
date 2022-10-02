using EddiDataDefinitions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EddiNavigationMonitor
{
    /// <summary>
    /// Interaction logic for BookmarkSelector.xaml
    /// </summary>
    public partial class BookmarkSelector : Window
    {
        public List<NavBookmark> SelectedBookmarks = new List<NavBookmark>();

        public BookmarkSelector(IEnumerable<NavBookmark> bookmarks)
        {
            InitializeComponent();
            bookmarksData.ItemsSource = bookmarks;
        }

        private void SelectionCheckboxChecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.DataContext is NavBookmark bookmark)
                {
                    if (!SelectedBookmarks.Contains(bookmark))
                    {
                        SelectedBookmarks.Add(bookmark);
                    }
                }
            }
        }

        private void SelectionCheckboxUnchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.DataContext is NavBookmark bookmark)
                {
                    if (SelectedBookmarks.Contains(bookmark))
                    {
                        SelectedBookmarks.Remove(bookmark);
                    }
                }
            }
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            DialogResult = false; 
            Close();
        }

        private void SelectedButtonClicked(object sender, RoutedEventArgs e)
        {
            if (SelectedBookmarks.Count > 0)
            {
                DialogResult = true;
            }
            else
            {
                DialogResult = false;
            }
            Close();
        }

        private void AllButtonClicked(object sender, RoutedEventArgs e)
        {
            SelectedBookmarks = ((ObservableCollection<NavBookmark>)bookmarksData.Items.SourceCollection).ToList();
            DialogResult = true;
            Close();
        }
    }
}

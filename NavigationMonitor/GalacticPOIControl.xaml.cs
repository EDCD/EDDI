using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EddiCore;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;

namespace EddiNavigationMonitor
{
    /// <summary>
    /// Interaction logic for GalacticPOIControl.xaml
    /// </summary>
    public partial class GalacticPOIControl : UserControl, INotifyPropertyChanged
    {
        public ICollectionView POIView
        {
            get => _poiView;
            private set
            {
                _poiView = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _poiView;

        private NavigationMonitor navigationMonitor()
        {
            return (NavigationMonitor)EDDI.Instance.ObtainMonitor("Navigation monitor");
        }

        public GalacticPOIControl()
        {
            InitializeComponent();
            InitializeView(navigationMonitor().GalacticPOIs);
        }

        private void InitializeView(object source)
        {
            POIView = CollectionViewSource.GetDefaultView(source);
            POIView.SortDescriptions.Add(new SortDescription(nameof(NavBookmark.distanceLy), ListSortDirection.Ascending));

            // Clear any active filters
            searchFilterText.Text = string.Empty;
            HideVisitedCheckBox.IsChecked = false;
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

        private void addBookmark(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is NavBookmark poiBookmark)
                {
                    if (Parent is TabItem parentTab && parentTab.Parent is TabControl parentTabControl)
                    {
                        if (parentTabControl.Parent is DockPanel dockPanel)
                        {
                            if (dockPanel.Parent is ConfigurationWindow configurationWindow)
                            {
                                configurationWindow.SwitchToTab(Properties.NavigationMonitor.tab_bookmarks);
                            }
                        }
                    }
                    if (!navigationMonitor().Bookmarks.Contains(poiBookmark))
                    {
                        navigationMonitor().Bookmarks.Add(poiBookmark);
                        navigationMonitor().WriteNavConfig();
                        EDDI.Instance.enqueueEvent(new BookmarkDetailsEvent(DateTime.UtcNow, "add", poiBookmark));
                    }
                }
            }
        }

        private void MarkdownWindow_OnUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is WebBrowser wb && !string.IsNullOrEmpty(wb.Tag as string))
            {
                wb.Navigate((Uri)null);
            }
        }

        private void MarkdownWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is WebBrowser wb && !string.IsNullOrEmpty(wb.Tag as string))
            {
                string html = CommonMark.CommonMarkConverter.Convert(wb.Tag as string);
                html = "<head>  <meta charset=\"UTF-8\"> </head> " + html;
                wb.NavigateToString(html);
            }
        }

        private void SearchFilterText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            using (POIView.DeferRefresh())
            {
                POIView.Filter = o => { return poiData_Filter(o); };
            }
            POIView.Refresh();
        }

        private void HideVisitedCheckBox_OnClick(object sender, RoutedEventArgs e)
        {
            using (POIView.DeferRefresh())
            {
                POIView.Filter = o => { return poiData_Filter(o); };
            }
        }

        private bool poiData_Filter(object sender, [CallerMemberName] string caller = null)
        {
            if (!(sender is NavBookmark poiBookmark)) { return true; }
            var filterTxt = searchFilterText.Text;

            // If filter applies, filter items.
            if ((poiBookmark.systemname?.ToLowerInvariant().Contains(filterTxt.ToLowerInvariant()) ?? false)
                || (poiBookmark.comment?.ToLowerInvariant().Contains(filterTxt.ToLowerInvariant()) ?? false)
                || (poiBookmark.descriptionMarkdown?.ToLowerInvariant().Contains(filterTxt.ToLowerInvariant()) ?? false))
            {
                if ((HideVisitedCheckBox.IsChecked ?? false) && poiBookmark.visited)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

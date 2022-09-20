using EddiDataDefinitions;
using EddiCore;
using System.Windows;
using System.Windows.Controls;

namespace EddiNavigationMonitor
{
    /// <summary>
    /// Interaction logic for PlottedRouteControl.xaml
    /// </summary>
    public partial class CurrentRouteControl : UserControl
    {
        private NavigationMonitor navigationMonitor()
        {
            return (NavigationMonitor)EDDI.Instance.ObtainMonitor("Navigation monitor");
        }

        public CurrentRouteControl()
        {
            InitializeComponent();
            navRouteData.ItemsSource = navigationMonitor().NavRouteList.Waypoints;
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex()).ToString();
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

        private void copySystemNameToClipboard(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is NavWaypoint navWaypoint)
                {
                    Clipboard.SetText(navWaypoint.systemName);
                }
            }
        }
    }
}

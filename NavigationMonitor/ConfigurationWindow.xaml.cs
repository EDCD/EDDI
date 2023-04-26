using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

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

        private static ConfigurationWindow _instance;
        private static readonly object InstanceLock = new object();

        public static ConfigurationWindow Instance
        {
            get
            {
                if ( _instance == null )
                {
                    lock ( InstanceLock )
                    {
                        if ( _instance == null )
                        {
                            _instance = new ConfigurationWindow();
                        }
                    }
                }
                return _instance;
            }
        }

        public ConfigurationWindow()
        {
            InitializeComponent();
            tabControl.Items.SortDescriptions.Add(new SortDescription(nameof(TabItem.Header), ListSortDirection.Ascending));
        }

        public void SwitchToTab(string tabHeader)
        {
            int index = 0;
            for (int i = 0; i < tabControl.Items.Count; i++)
            {
                if (tabControl.Items[i] is TabItem tabItem
                    && tabItem.Header.Equals(tabHeader))
                {
                    index = i;
                    break;
                }
            }
            tabControl.SelectedIndex = index;
        }

        public void addBookmark(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                if (button.DataContext is NavWaypoint navWaypoint)
                {
                    var navBookmark = new NavBookmark(navWaypoint.systemName, navWaypoint.systemAddress, navWaypoint.x, navWaypoint.y, navWaypoint.z, null, null, false, null, null, false);
                    if (!navigationMonitor().Bookmarks
                            .Any(b => b.systemname == navWaypoint.systemName 
                                      && b.x == navWaypoint.x 
                                      && b.y == navWaypoint.y 
                                      && b.z == navWaypoint.z))
                    {
                        navigationMonitor().Bookmarks.Add(navBookmark);
                        navigationMonitor().WriteNavConfig();
                        EDDI.Instance.enqueueEvent(new BookmarkDetailsEvent(DateTime.UtcNow, "add", navBookmark));
                    }
                }
            }
        }
    }
}

using Eddi;
using EddiDataDefinitions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Utilities;

namespace EddiEddpMonitor
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl, INotifyPropertyChanged
    {
        // localized text for delete button template
        public static string DeleteBtnText
        {
            get { return I18N.GetString("eddp_monitor_delete_btn"); }
        }

        private EddpConfiguration configuration;

        public List<KeyValuePair<string, string>> StatesPlusNone { get; set; }

        private ObservableCollection<Watch> watches;
        public ObservableCollection<Watch> Watches
        {
            get { return watches; }
            set { watches = value; OnPropertyChanged("Watch"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ConfigurationWindow()
        {
            InitializeComponent();
            I18NForComponents();
            DataContext = this;

            // Make a list of states plus a (anything) state that maps to NULL
            StatesPlusNone = new List<KeyValuePair<string, string>>();
            StatesPlusNone.Add(new KeyValuePair<string, string>(I18N.GetString("eddp_monitor_anything"), null));
            StatesPlusNone.AddRange(State.STATES.Select(x => new KeyValuePair<string, string>(x.name, x.name)));

            configurationFromFile();
        }

        private void I18NForComponents()
        {
            par1.Text = I18N.GetString("eddp_monitor_p1");
            watchData.Columns[0].Header = I18N.GetString("eddp_monitor_name_header");
            watchData.Columns[1].Header = I18N.GetString("eddp_monitor_system_header");
            watchData.Columns[2].Header = I18N.GetString("eddp_monitor_faction_header");
            watchData.Columns[3].Header = I18N.GetString("eddp_monitor_state_header");
            watchData.Columns[4].Header = I18N.GetString("eddp_monitor_max_dist_ship_header");
            watchData.Columns[5].Header = I18N.GetString("eddp_monitor_max_dist_home_header");
        }

        private void configurationFromFile()
        {
            configuration = EddpConfiguration.FromFile();
            ObservableCollection<Watch> watches = new ObservableCollection<Watch>();
            foreach (Watch watch in configuration.watches)
            {
                watches.Add(watch);
            }
            Watches = watches;
        }

        private void eddpWatchesUpdated(object sender, RoutedEventArgs e)
        {
            updateWatchesConfiguration();
        }

        private void eddpWatchesUpdated(object sender, DataTransferEventArgs e)
        {
            updateWatchesConfiguration();
        }

        private void eddpStateChanged(object sender, SelectionChangedEventArgs e)
        {
            updateWatchesConfiguration();
        }

        private void eddpAddWatch(object sender, RoutedEventArgs e)
        {
            Watch watch = new Watch();
            watch.Name = I18N.GetString("eddp_monitor_new_watch");

            configuration.watches.Add(watch);
            updateWatchesConfiguration();
            Watches.Add(watch);
            watchData.Items.Refresh();
        }

        private void eddpDeleteWatch(object sender, RoutedEventArgs e)
        {
            Watch watch = (Watch)((Button)e.Source).DataContext;
            configuration.watches.Remove(watch);
            updateWatchesConfiguration();
            Watches.Remove(watch);
            watchData.Items.Refresh();
        }

        private void updateWatchesConfiguration()
        {
            if (configuration != null)
            {
                configuration.ToFile();
            }
            EDDI.Instance.Reload("EDDP monitor");
        }
    }
}

using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace EddiEddpMonitor
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl, INotifyPropertyChanged
    {
        private EddpConfiguration configuration;

        public List<KeyValuePair<string, FactionState>> StatesPlusNone { get; set; }

        public ObservableCollection<BgsWatch> Watches
        {
            get { return _watches; }
            set { _watches = value; OnPropertyChanged("Watch"); }
        }
        private ObservableCollection<BgsWatch> _watches;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ConfigurationWindow()
        {
            DataContext = this;

            // Make a list of states plus a (anything) state that maps to NULL
            StatesPlusNone = new List<KeyValuePair<string, FactionState>>
            {
                new KeyValuePair<string, FactionState>(Properties.EddpResources.anything, null)
            };
            StatesPlusNone.AddRange(FactionState.AllOfThem.OrderBy(x => x.localizedName).Select(x => new KeyValuePair<string, FactionState>(x.localizedName, x)));
            configurationFromFile();
            InitializeComponent();
        }

        private void configurationFromFile()
        {
            configuration = ConfigService.Instance.eddpConfiguration;
            var watches = new ObservableCollection<BgsWatch>();
            foreach (BgsWatch watch in configuration.watches)
            {
                watches.Add(watch);
            }
            Watches = watches;
        }

        private void eddpWatchesUpdated(object sender, DataTransferEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                if (dataGrid.IsLoaded)
                {
                    updateWatchesConfiguration();
                }
            }
        }

        private void eddpStateChanged(object sender, SelectionChangedEventArgs e)
        {
            updateWatchesConfiguration();
        }

        private void eddpAddWatch(object sender, RoutedEventArgs e)
        {
            BgsWatch bgsWatch = new BgsWatch();
            bgsWatch.Name = Properties.EddpResources.new_watch;

            configuration.watches.Add(bgsWatch);
            updateWatchesConfiguration();
            Watches.Add(bgsWatch);
            watchData.Items.Refresh();
        }

        private void eddpDeleteWatch(object sender, RoutedEventArgs e)
        {
            BgsWatch bgsWatch = (BgsWatch)((Button)e.Source).DataContext;
            configuration.watches.Remove(bgsWatch);
            updateWatchesConfiguration();
            Watches.Remove(bgsWatch);
            watchData.Items.Refresh();
        }

        private void updateWatchesConfiguration()
        {
            if (configuration != null)
            {
                ConfigService.Instance.eddpConfiguration = configuration;
            }
            EDDI.Instance.Reload("EDDP monitor");
        }
    }
}

using Eddi;
using EddiDataDefinitions;
using EddiEddpMonitor;
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
            DataContext = this;

            // Make a list of states plus a (anything) state that maps to NULL
            StatesPlusNone = new List<KeyValuePair<string, string>>();
            StatesPlusNone.Add(new KeyValuePair<string, string>(Properties.EddpResources.anything, null));
            StatesPlusNone.AddRange(SystemState.AllOfThem.Select(x => new KeyValuePair<string, string>(x.localizedName, x.localizedName)));

            configurationFromFile();
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
            watch.Name = Properties.EddpResources.new_watch;

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

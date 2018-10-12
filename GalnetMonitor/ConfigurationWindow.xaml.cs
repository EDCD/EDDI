using Eddi;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GalnetMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        GalnetMonitor monitor;

        public ConfigurationWindow()
        {
            InitializeComponent();

            monitor = (GalnetMonitor)EDDI.Instance.ObtainMonitor("Galnet monitor");

            GalnetConfiguration configuration = GalnetConfiguration.FromFile();
            Dictionary<string, string> langs = monitor.GetGalnetLocales();
            languageComboBox.ItemsSource = langs.Keys;
            languageComboBox.SelectedValue = configuration.language;
            galnetAlwaysOn.IsChecked = configuration.galnetAlwaysOn;
        }

        private void onLanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            string language = (string)((ComboBox)e.Source).SelectedValue;
            GalnetConfiguration configuration = GalnetConfiguration.FromFile();
            if (language != null && language != configuration.language)
            {
                // If the language changes we clear out the old articles
                GalnetSqLiteRepository.Instance.DeleteNews();
                configuration.lastuuid = null;
                configuration.language = language;
                configuration.ToFile();
                monitor?.Reload();
            }
        }

        private void galnetAlwaysOnChecked(object sender, RoutedEventArgs e)
        {
            GalnetConfiguration configuration = GalnetConfiguration.FromFile();
            configuration.galnetAlwaysOn = galnetAlwaysOn.IsChecked.Value;
            configuration.ToFile();
            monitor.Reload();
        }

        private void galnetAlwaysOnUnchecked(object sender, RoutedEventArgs e)
        {
            GalnetConfiguration configuration = GalnetConfiguration.FromFile();
            configuration.galnetAlwaysOn = galnetAlwaysOn.IsChecked.Value;
            configuration.ToFile();
            monitor.Reload();
        }
    }
}

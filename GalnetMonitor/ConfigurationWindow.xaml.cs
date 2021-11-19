using EddiConfigService;
using EddiCore;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace EddiGalnetMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        private GalnetMonitor galnetMonitor()
        {
            return (GalnetMonitor)EDDI.Instance.ObtainMonitor("Galnet monitor");
        }

        public ConfigurationWindow()
        {
            InitializeComponent();

            var configuration = ConfigService.Instance.galnetConfiguration;
            Dictionary<string, string> langs = galnetMonitor()?.GetGalnetLocales();
            languageComboBox.ItemsSource = langs?.Keys ?? new Dictionary<string, string>().Keys;
            languageComboBox.SelectedValue = configuration.language;
            galnetAlwaysOn.IsChecked = configuration.galnetAlwaysOn;
        }

        private void onLanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            string language = (string)((ComboBox)e.Source).SelectedValue;
            var configuration = ConfigService.Instance.galnetConfiguration;
            if (language != null && language != configuration.language)
            {
                // If the language changes we clear out the old articles
                GalnetSqLiteRepository.Instance.DeleteNews();
                configuration.lastuuid = null;
                configuration.language = language;
                ConfigService.Instance.galnetConfiguration = configuration;
                galnetMonitor()?.Reload();
            }
        }

        private void galnetAlwaysOnChecked(object sender, RoutedEventArgs e)
        {
            var configuration = ConfigService.Instance.galnetConfiguration;
            configuration.galnetAlwaysOn = galnetAlwaysOn.IsChecked ?? false;
            ConfigService.Instance.galnetConfiguration = configuration;
            galnetMonitor()?.Reload();
        }

        private void galnetAlwaysOnUnchecked(object sender, RoutedEventArgs e)
        {
            var configuration = ConfigService.Instance.galnetConfiguration;
            configuration.galnetAlwaysOn = galnetAlwaysOn.IsChecked ?? false;
            ConfigService.Instance.galnetConfiguration = configuration;
            galnetMonitor()?.Reload();
        }
    }
}

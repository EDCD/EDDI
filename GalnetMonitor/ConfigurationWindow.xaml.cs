using Eddi;
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

            monitor = ((GalnetMonitor)EDDI.Instance.ObtainMonitor("Galnet monitor"));

            GalnetConfiguration configuration = GalnetConfiguration.FromFile();
            languageComboBox.SelectedValue = configuration.language;
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
    }
}

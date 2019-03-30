using Eddi;
using EddiDataDefinitions;
using EddiSpeechService;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Utilities;

namespace EddiShipMonitor
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        private ShipMonitor shipMonitor()
        {
            return (ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor");
        }

        public ConfigurationWindow()
        {
            InitializeComponent();
            shipData.ItemsSource = shipMonitor().shipyard;

            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiConfiguration.exporttarget = "Coriolis";
        }

        private void ipaClicked(object sender, RoutedEventArgs e)
        {
            IpaResourcesWindow IpaResources = new IpaResourcesWindow();
            IpaResources.Show();
        }

        private void testShipName(object sender, RoutedEventArgs e)
        {
            Ship ship = (Ship)((Button)e.Source).DataContext;
            ship.health = 100;
            SpeechServiceConfiguration speechConfiguration = SpeechServiceConfiguration.FromFile();
            string nameToSpeak = String.IsNullOrEmpty(ship.phoneticname) 
                ? ship.name 
                : $@"<phoneme alphabet=""ipa"" ph=""{ship.phoneticname}"">{ship.name}</phoneme>";
            string message = String.Format(Properties.ShipMonitor.ship_ready, nameToSpeak);
            SpeechService.Instance.Say(ship, message, 0);
        }

        private void exportShip(object sender, RoutedEventArgs e)
        {
            Ship ship = (Ship)((Button)e.Source).DataContext;
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();

            // Coriolis is the default (and currently only) export target
            string uri = ship.CoriolisUri();

            // URI can be very long so we can't use a simple Process.Start(), as that fails
            try
            {
                ProcessStartInfo proc = new ProcessStartInfo(Net.GetDefaultBrowserPath(), uri)
                {
                    UseShellExecute = false
                };
                Process.Start(proc);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed: ", ex);
                try
                {
                    // Last-gasp attempt if we have a shorter URL
                    if (uri.Length < 2048)
                    {
                        Process.Start(uri);
                    }
                    else
                    {
                        Logging.Info("Export failed: Target URL is too long.");
                    }
                }
                catch (Exception)
                {
                    Logging.Error("Failed to find a way of opening URL \"" + uri + "\"");
                }
            }
        }

        private void shipsUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the ship monitor's 'PhoneticName' information
            if (e.OriginalSource is TextBlock)
            {
                var dg = (DataGrid)e.Source;
                if (dg.SelectedItem is Ship)
                {
                    shipMonitor()?.Save();
                }
            }
        }

        private void shipsUpdated(object sender, SelectionChangedEventArgs e)
        {
            // Update the ship monitor's 'Role' information
            if (e.RemovedItems.Count > 0)
            {
                shipMonitor()?.Save();
            }
        }
    }

    public class ValidIPARule : ValidationRule
    {
        private static Regex IPA_REGEX = new Regex(@"^[bdfɡhjklmnprstvwzxaɪ˜iu\.ᵻᵿɑɐɒæɓʙβɔɕçɗɖðʤəɘɚɛɜɝɞɟʄɡ(ɠɢʛɦɧħɥʜɨɪʝɭɬɫɮʟɱɯɰŋɳɲɴøɵɸθœɶʘɹɺɾɻʀʁɽʂʃʈʧʉʊʋⱱʌɣɤʍχʎʏʑʐʒʔʡʕʢǀǁǂǃˈˌːˑʼʴʰʱʲʷˠˤ˞n̥d̥ŋ̊b̤a̤t̪d̪s̬t̬b̰a̰t̺d̺t̼d̼t̻d̻t̚ɔ̹ẽɔ̜u̟e̠ël̴n̴ɫe̽e̝ɹ̝m̩n̩l̩e̞β̞e̯e̘e̙ĕe̋éēèȅx͜xx͡x↓↑→↗↘]+$");

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return ValidationResult.ValidResult;
            }
            string val = value.ToString();
            if (IPA_REGEX.Match(val).Success)
            {
                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, "Invalid IPA");
            }
        }
    }
}

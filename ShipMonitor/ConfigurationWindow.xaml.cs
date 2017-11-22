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
        ShipMonitor monitor;
        public static string HearItBtnText { get { return I18N.GetString("ship_monitor_hear_it_button"); } }
        public static string ExportItBtnText { get { return I18N.GetString("ship_monitor_export_it_button"); } }

        public ConfigurationWindow()
        {
            InitializeComponent();
            I18NForComponents();
            monitor = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"));
            shipData.ItemsSource = monitor.shipyard;

            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            string exporttarget = eddiConfiguration.exporttarget;
            Logging.Debug("Export target from configuration: " + exporttarget);
            exportComboBox.Text = exporttarget == null ? "Coriolis" : exporttarget;
        }

        private void I18NForComponents()
        {
            p1.Text = I18N.GetString("ship_monitor_p1");
            p2.Text = I18N.GetString("ship_monitor_p2");
            linkIPAText.Text = I18N.GetString("ship_monitor_link_ipa");
            p3.Text = I18N.GetString("ship_monitor_p3");
            p4.Text = I18N.GetString("ship_monitor_p4");
            p5.Text = I18N.GetString("ship_monitor_p5");
            shipData.Columns[0].Header = I18N.GetString("ship_monitor_header_name");
            shipData.Columns[1].Header = I18N.GetString("ship_monitor_header_model");
            shipData.Columns[2].Header = I18N.GetString("ship_monitor_header_indent");
            shipData.Columns[3].Header = I18N.GetString("ship_monitor_header_value");
            shipData.Columns[4].Header = I18N.GetString("ship_monitor_header_location");
            shipData.Columns[5].Header = I18N.GetString("ship_monitor_header_role");
            shipData.Columns[6].Header = I18N.GetString("ship_monitor_header_spoken_name");
        }

        private void onExportTargetChanged(object sender, SelectionChangedEventArgs e)
        {
            string exporttarget = (string)((ComboBox)e.Source).SelectedValue;
            Logging.Debug("Export target: " + exporttarget);

            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiConfiguration.exporttarget = string.IsNullOrWhiteSpace(exporttarget) ? null : exporttarget.Trim();
            eddiConfiguration.ToFile();
        }

        private void ipaClicked(object sender, RoutedEventArgs e)
        {
            Process.Start("https://en.wikipedia.org/wiki/International_Phonetic_Alphabet");
        }

        private void testShipName(object sender, RoutedEventArgs e)
        {
            Ship ship = (Ship)((Button)e.Source).DataContext;
            ship.health = 100;
            SpeechServiceConfiguration speechConfiguration = SpeechServiceConfiguration.FromFile();
            if (string.IsNullOrEmpty(ship.phoneticname))
            {
                SpeechService.Instance.Say(ship, I18N.GetStringWithArgs("ship_ready", new string[] { ship.name }), false);
            }
            else
            {
                string phoneme = "<phoneme alphabet=\"ipa\" ph=\"" + ship.phoneticname + "\">" + ship.name + "</phoneme>";
                SpeechService.Instance.Say(ship, I18N.GetStringWithArgs("ship_ready", new string[] { phoneme }), false);
            }
        }

        private void exportShip(object sender, RoutedEventArgs e)
        {
            Ship ship = (Ship)((Button)e.Source).DataContext;
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();

            // Coriolis is the default export target
            string uri = ship.CoriolisUri();

            // Support EDShipyard as well.
            if (eddiConfiguration.exporttarget == "EDShipyard")
            {
                uri = ship.EDShipyardUri();
            }

            Logging.Debug("Export target is " + eddiConfiguration.exporttarget + ", URI is " + uri);

            // URI can be very long so we can't use a simple Process.Start(), as that fails
            try
            {
                ProcessStartInfo proc = new ProcessStartInfo(Net.GetDefaultBrowserPath(), uri);
                proc.UseShellExecute = false;
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
                        Logging.Error("Failed to find a way of opening URL \"" + uri + "\"");
                    }
                }
                catch (Exception)
                {
                    // Nothing to do
                }
            }
        }

        private void shipsUpdated(object sender, DataTransferEventArgs e)
        {
            // Update the ship monitor's information
            monitor.writeShips();
        }

        private void shipsUpdated(object sender, SelectionChangedEventArgs e)
        {
            // Update the ship monitor's information
            monitor.writeShips();
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

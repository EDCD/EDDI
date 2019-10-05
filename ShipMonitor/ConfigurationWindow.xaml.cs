using Eddi;
using EddiDataDefinitions;
using EddiSpeechService;
using System;
using System.Collections.Generic;
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
            string exportTarget = eddiConfiguration.exporttarget;

            // handle migration
            if (exportTarget == "EDShipyard")
            {
                exportTarget = "EDSY";
                eddiConfiguration.exporttarget = exportTarget;
                eddiConfiguration.ToFile();
            }

            Logging.Debug("Export target from configuration: " + exportTarget);
            exportComboBox.Text = exportTarget ?? "Coriolis";
        }

        private void onExportTargetChanged(object sender, SelectionChangedEventArgs e)
        {
            string exportTarget = (string)((ComboBox)e.Source).SelectedValue;
            Logging.Debug("Export target: " + exportTarget);

            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiConfiguration.exporttarget = string.IsNullOrWhiteSpace(exportTarget) ? null : exportTarget.Trim();
            eddiConfiguration.ToFile();
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

            string uri;
            switch (eddiConfiguration.exporttarget)
            {
                case "EDShipyard":
                case "EDSY":
                    uri = ship.EDShipyardUri();
                    break;

                case "Coriolis":
                    uri = ship.CoriolisUri();
                    break;

                default:
                    throw new NotImplementedException($"Export target {eddiConfiguration.exporttarget} not recognized.");
            }

            Logging.Debug("Export target is " + eddiConfiguration.exporttarget + ", URI is " + uri);

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

    public class ValidIpaRule : ValidationRule
    {
        // IPA symbols sourced from https://www.internationalphoneticassociation.org/content/ipa-chart (IPA 2005 edition)
        // and from https://www.phon.ucl.ac.uk/home/wells/ipa-unicode.htm

        static readonly Dictionary<int, string> validIPA = new Dictionary<int, string>()
        {
            { 103, "g" }, // This is an ordinary "g", which the IPA has ruled is acceptable in place of a vd velar plosive "ɡ" (decimal code 609)
            { 230, "æ" },
            { 231, "ç" },
            { 240, "ð" },
            { 248, "ø" },
            { 295, "ħ" },
            { 331, "ŋ" },
            { 339, "œ" },
            { 448, "ǀ" },
            { 449, "ǁ" },
            { 450, "ǂ" },
            { 451, "ǃ" },
            { 592, "ɐ" },
            { 593, "ɑ" },
            { 594, "ɒ" },
            { 595, "ɓ" },
            { 596, "ɔ" },
            { 597, "ɕ" },
            { 598, "ɖ" },
            { 599, "ɗ" },
            { 600, "ɘ" },
            { 601, "ə" },
            { 602, "ɚ" },
            { 603, "ɛ" },
            { 604, "ɜ" },
            { 605, "ɝ" },
            { 606, "ɞ" },
            { 607, "ɟ" },
            { 608, "ɠ" },
            { 609, "ɡ" },
            { 610, "ɢ" },
            { 611, "ɣ" },
            { 612, "ɤ" },
            { 613, "ɥ" },
            { 614, "ɦ" },
            { 615, "ɧ" },
            { 616, "ɨ" },
            { 618, "ɪ" },
            { 619, "ɫ" },
            { 620, "ɬ" },
            { 621, "ɭ" },
            { 622, "ɮ" },
            { 623, "ɯ" },
            { 624, "ɰ" },
            { 625, "ɱ" },
            { 626, "ɲ" },
            { 627, "ɳ" },
            { 628, "ɴ" },
            { 629, "ɵ" },
            { 630, "ɶ" },
            { 632, "ɸ" },
            { 633, "ɹ" },
            { 634, "ɺ" },
            { 635, "ɻ" },
            { 637, "ɽ" },
            { 638, "ɾ" },
            { 640, "ʀ" },
            { 641, "ʁ" },
            { 642, "ʂ" },
            { 643, "ʃ" },
            { 644, "ʄ" },
            { 648, "ʈ" },
            { 649, "ʉ" },
            { 650, "ʊ" },
            { 651, "ʋ" },
            { 652, "ʌ" },
            { 653, "ʍ" },
            { 654, "ʎ" },
            { 655, "ʏ" },
            { 656, "ʐ" },
            { 657, "ʑ" },
            { 658, "ʒ" },
            { 660, "ʔ" },
            { 661, "ʕ" },
            { 664, "ʘ" },
            { 665, "ʙ" },
            { 667, "ʛ" },
            { 668, "ʜ" },
            { 669, "ʝ" },
            { 671, "ʟ" },
            { 673, "ʡ" },
            { 674, "ʢ" },
            { 676, "ʤ" },
            { 679, "ʧ" },
            { 688, "ʰ" },
            { 689, "ʱ" },
            { 690, "ʲ" },
            { 692, "ʴ" },
            { 695, "ʷ" },
            { 700, "ʼ" },
            { 712, "ˈ" },
            { 716, "ˌ" },
            { 720, "ː" },
            { 721, "ˑ" },
            { 734, "˞" },
            { 736, "ˠ" },
            { 740, "ˤ" },
            { 768, "è" },
            { 769, "é" },
            { 771, "ẽ" },
            { 772, "ē" },
            { 774, "ĕ" },
            { 776, "ë" },
            { 778, "ŋ̊" },
            { 779, "e̋" },
            { 783, "ȅ" },
            { 792, "e̘" },
            { 793, "e̙" },
            { 794, "t̚" },
            { 796, "ɔ̜" },
            { 797, "e̝ ɹ̝" },
            { 798, "e̞ β̞" },
            { 799, "u̟" },
            { 800, "e̠" },
            { 804, "b̤ a̤" },
            { 805, "n̥ d̥" },
            { 809, "m̩ n̩ l̩" },
            { 810, "t̪ d̪" },
            { 812, "s̬ t̬" },
            { 815, "e̯" },
            { 816, "b̰ a̰" },
            { 820, "l̴ n̴" },
            { 825, "ɔ̹" },
            { 826, "t̺ d̺" },
            { 827, "t̻ d̻" },
            { 828, "t̼ d̼" },
            { 829, "e̽" },
            { 860, "x͜x" },
            { 865, "x͡x" },
            { 946, "β" },
            { 952, "θ" },
            { 967, "χ" },
            { 8593, "↑" },
            { 8594, "→" },
            { 8595, "↓" },
            { 8599, "↗" },
            { 8600, "↘" },
            { 11377, "ⱱ" }
        };

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return ValidationResult.ValidResult;
            }
            char[] chars = value.ToString().ToCharArray();

            foreach (var ch in chars)
            {
                int unicodeDecimalCode = Convert.ToUInt16(ch);
                if (!validIPA.ContainsKey(unicodeDecimalCode))
                {
                    return new ValidationResult(false, ch + " is not a valid IPA character. Please copy and paste characters directly from the source for best results.");
                }
            }
            return ValidationResult.ValidResult;
        }
    }
}

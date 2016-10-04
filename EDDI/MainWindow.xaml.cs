using EliteDangerousCompanionAppService;
using EliteDangerousDataDefinitions;
using EliteDangerousSpeechService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utilities;

namespace EDDI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Profile profile;
        private ShipsConfiguration shipsConfiguration;

        public MainWindow()
        {
            InitializeComponent();

            // Set up our app directory
            Directory.CreateDirectory(Constants.DATA_DIR);

            // Configure the EDDI tab
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiHomeSystemText.Text = eddiConfiguration.HomeSystem;
            eddiHomeStationText.Text = eddiConfiguration.HomeStation;
            eddiInsuranceDecimal.Text = eddiConfiguration.Insurance.ToString(CultureInfo.InvariantCulture);
            eddiVerboseLogging.IsChecked = eddiConfiguration.Debug;

            Logging.Verbose = eddiConfiguration.Debug;

            // Configure the Companion App tab
            CompanionAppCredentials companionAppCredentials = CompanionAppCredentials.FromFile();
            companionAppEmailText.Text = companionAppCredentials.email;
            // See if the credentials work
            try
            {
                profile = CompanionAppService.Instance.Profile();
                setUpCompanionAppComplete("Your connection to the companion app is operational, Commander " + profile.Cmdr.name);
            }
            catch (Exception)
            {
                if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.NEEDS_LOGIN)
                {
                    // Fall back to stage 1
                    setUpCompanionAppStage1();
                }
                else if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.NEEDS_CONFIRMATION)
                {
                    // Fall back to stage 2
                    setUpCompanionAppStage2();
                }
            }

            if (profile != null)
            {
                setShipyardFromConfiguration();
            }

            // Configure the Text-to-speech tab
            SpeechServiceConfiguration speechServiceConfiguration = SpeechServiceConfiguration.FromFile();
            List<string> speechOptions = new List<string>();
            speechOptions.Add("Windows TTS default");
            try
            {
                using (SpeechSynthesizer synth = new SpeechSynthesizer())
                {
                    foreach (InstalledVoice voice in synth.GetInstalledVoices())
                    {
                        if (voice.Enabled)
                        {
                            speechOptions.Add(voice.VoiceInfo.Name);
                        }
                    }
                }

                ttsVoiceDropDown.ItemsSource = speechOptions;
                ttsVoiceDropDown.Text = speechServiceConfiguration.StandardVoice == null ? "Windows TTS default" : speechServiceConfiguration.StandardVoice;
            }
            catch (Exception e)
            {
                Logging.Warn("" + Thread.CurrentThread.ManagedThreadId + ": Caught exception " + e);
            }
            ttsVolumeSlider.Value = speechServiceConfiguration.Volume;
            ttsRateSlider.Value = speechServiceConfiguration.Rate;
            ttsEffectsLevelSlider.Value = speechServiceConfiguration.EffectsLevel;
            ttsDistortCheckbox.IsChecked = speechServiceConfiguration.DistortOnDamage;

            ttsTestShipDropDown.ItemsSource = ShipDefinitions.ShipModels;
            ttsTestShipDropDown.Text = "Adder";

            foreach (EDDIMonitor monitor in Eddi.Instance.monitors)
            {
                Logging.Debug("Adding configuration tab for " + monitor.MonitorName());

                PluginSkeleton skeleton = new PluginSkeleton(monitor.MonitorName());
                skeleton.plugindescription.Text = monitor.MonitorDescription();

                bool enabled;
                if (eddiConfiguration.Plugins.TryGetValue(monitor.MonitorName(), out enabled))
                {
                    skeleton.pluginenabled.IsChecked = enabled;
                }
                else
                {
                    // Default to enabled
                    skeleton.pluginenabled.IsChecked = true;
                    eddiConfiguration.ToFile();
                }

                // Add monitor-specific configuration items
                UserControl monitorConfiguration = monitor.ConfigurationTabItem();
                if (monitorConfiguration != null)
                {
                    monitorConfiguration.Margin = new Thickness(10);
                    skeleton.panel.Children.Add(monitorConfiguration);
                }

                TabItem item = new TabItem { Header = monitor.MonitorName() };
                item.Content = skeleton;
                tabControl.Items.Add(item);
            }

            foreach (EDDIResponder responder in Eddi.Instance.responders)
            {
                Logging.Debug("Adding configuration tab for " + responder.ResponderName());

                PluginSkeleton skeleton = new PluginSkeleton(responder.ResponderName());
                skeleton.plugindescription.Text = responder.ResponderDescription();

                bool enabled;
                if (eddiConfiguration.Plugins.TryGetValue(responder.ResponderName(), out enabled))
                {
                    skeleton.pluginenabled.IsChecked = enabled;
                }
                else
                {
                    // Default to enabled
                    skeleton.pluginenabled.IsChecked = true;
                    eddiConfiguration.ToFile();
                }

                // Add responder-specific configuration items
                UserControl monitorConfiguration = responder.ConfigurationTabItem();
                if (monitorConfiguration != null)
                {
                    monitorConfiguration.Margin = new Thickness(10);
                    skeleton.panel.Children.Add(monitorConfiguration);
                }

                TabItem item = new TabItem { Header = responder.ResponderName() };
                item.Content = skeleton;
                tabControl.Items.Add(item);
            }

            Eddi.Instance.Start();
        }

        // Handle changes to the eddi tab
        private void homeSystemChanged(object sender, TextChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiConfiguration.HomeSystem = string.IsNullOrWhiteSpace(eddiHomeSystemText.Text) ? null : eddiHomeSystemText.Text.Trim();
            eddiConfiguration.ToFile();
        }

        private void homeStationChanged(object sender, TextChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiConfiguration.HomeStation = string.IsNullOrWhiteSpace(eddiHomeStationText.Text) ? null : eddiHomeStationText.Text.Trim();
            eddiConfiguration.ToFile();
        }

        private void insuranceChanged(object sender, TextChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            try
            {
                eddiConfiguration.Insurance = string.IsNullOrWhiteSpace(eddiInsuranceDecimal.Text) ? 5 : Convert.ToDecimal(eddiInsuranceDecimal.Text);
                eddiConfiguration.ToFile();
            }
            catch
            {
                // Bad user input; ignore it
            }
        }

        private void verboseLoggingEnabled(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiConfiguration.Debug = eddiVerboseLogging.IsChecked.Value;
            eddiConfiguration.ToFile();
        }

        private void verboseLoggingDisabled(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiConfiguration.Debug = eddiVerboseLogging.IsChecked.Value;
            eddiConfiguration.ToFile();
        }

        // Handle changes to the companion app tab
        private void companionAppNextClicked(object sender, RoutedEventArgs e)
        {
            // See if the user is entering their email address and password
            if (companionAppEmailText.Visibility == Visibility.Visible)
            {
                // Stage 1 of authentication - login
                CompanionAppService.Instance.Credentials.email = companionAppEmailText.Text.Trim();
                CompanionAppService.Instance.Credentials.password = companionAppPasswordText.Password.Trim();
                try
                {
                    // It is possible that we have valid cookies at this point so don't log in, but we did
                    // need the credentials
                    if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.NEEDS_LOGIN)
                    {
                        CompanionAppService.Instance.Login();
                    }
                    if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.NEEDS_CONFIRMATION)
                    {
                        setUpCompanionAppStage2();
                    }
                    else if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.READY)
                    {
                        if (profile == null)
                        {
                            profile = CompanionAppService.Instance.Profile();
                        }
                        setUpCompanionAppComplete("Your connection to the companion app is operational, Commander " + profile.Cmdr.name);
                        setShipyardFromConfiguration();
                    }
                }
                catch (EliteDangerousCompanionAppAuthenticationException ex)
                {
                    companionAppText.Text = ex.Message;
                }
                catch (EliteDangerousCompanionAppErrorException ex)
                {
                    companionAppText.Text = ex.Message;
                }
                catch (Exception ex)
                {
                    companionAppText.Text = "Unexpected problem\r\nPlease report this at http://github.com/CmdrMcDonald/EliteDangerousDataProvider/issues\r\n" + ex;
                }
            }
            else if (companionAppCodeText.Visibility == Visibility.Visible)
            {
                // Stage 2 of authentication - confirmation
                string code = companionAppCodeText.Text.Trim();
                try
                {
                    CompanionAppService.Instance.Confirm(code);
                    // All done - see if it works
                    profile = CompanionAppService.Instance.Profile();
                    setUpCompanionAppComplete("Your connection to the companion app is operational, Commander " + profile.Cmdr.name);
                    setShipyardFromConfiguration();
                }
                catch (EliteDangerousCompanionAppAuthenticationException ex)
                {
                    setUpCompanionAppStage1(ex.Message);
                }
                catch (EliteDangerousCompanionAppErrorException ex)
                {
                    setUpCompanionAppStage1(ex.Message);
                }
                catch (Exception ex)
                {
                    setUpCompanionAppStage1("Unexpected problem\r\nPlease report this at http://github.com/CmdrMcDonald/EliteDangerousDataProvider/issues\r\n" + ex);
                }
            }
        }

        private void setUpCompanionAppStage1(string message = null)
        {
            if (message == null)
            {
                companionAppText.Text = "You do not have a connection to the companion app at this time.  Please enter your Elite: Dangerous email address and password below";
            }
            else
            {
                companionAppText.Text = message;
            }

            companionAppEmailLabel.Visibility = Visibility.Visible;
            companionAppEmailText.Visibility = Visibility.Visible;
            companionAppEmailText.Text = CompanionAppService.Instance.Credentials.email;
            companionAppPasswordLabel.Visibility = Visibility.Visible;
            companionAppPasswordText.Visibility = Visibility.Visible;
            companionAppPasswordText.Password = CompanionAppService.Instance.Credentials.password;
            companionAppCodeText.Text = "";
            companionAppCodeLabel.Visibility = Visibility.Hidden;
            companionAppCodeText.Visibility = Visibility.Hidden;
            companionAppNextButton.Visibility = Visibility.Visible;
        }

        private void setUpCompanionAppStage2(string message = null)
        {
            if (message == null)
            {
                companionAppText.Text = "Please enter the verification code that should have been sent to your email address";
            }
            else
            {
                companionAppText.Text = message;
            }

            companionAppEmailLabel.Visibility = Visibility.Hidden;
            companionAppEmailText.Visibility = Visibility.Hidden;
            companionAppPasswordText.Password = "";
            companionAppPasswordLabel.Visibility = Visibility.Hidden;
            companionAppPasswordText.Visibility = Visibility.Hidden;
            companionAppCodeLabel.Visibility = Visibility.Visible;
            companionAppCodeText.Visibility = Visibility.Visible;
            companionAppNextButton.Visibility = Visibility.Visible;
        }

        private void setUpCompanionAppComplete(string message = null)
        {
            if (message == null)
            {
                companionAppText.Text = "Complete";
            }
            else
            {
                companionAppText.Text = message;
            }

            companionAppEmailLabel.Visibility = Visibility.Hidden;
            companionAppEmailText.Visibility = Visibility.Hidden;
            companionAppPasswordText.Password = "";
            companionAppPasswordLabel.Visibility = Visibility.Hidden;
            companionAppPasswordText.Visibility = Visibility.Hidden;
            companionAppCodeText.Text = "";
            companionAppCodeLabel.Visibility = Visibility.Hidden;
            companionAppCodeText.Visibility = Visibility.Hidden;
            companionAppNextButton.Visibility = Visibility.Hidden;
        }

        // Handle changes to the Shipyard tab
        private void setShipyardFromConfiguration()
        {
            shipsConfiguration = new ShipsConfiguration();
            List<Ship> ships = new List<Ship>();
            if (profile != null)
            {
                ships.Add(profile.Ship);
                ships.AddRange(profile.StoredShips);
            }
            shipsConfiguration.Ships = ships;
            shipyardData.ItemsSource = ships;
        }

        private void testShipName(object sender, RoutedEventArgs e)
        {
            Ship ship = (Ship)((Button)e.Source).DataContext;
            ship.health = 100;
            SpeechServiceConfiguration speechConfiguration = SpeechServiceConfiguration.FromFile();
            if (string.IsNullOrEmpty(ship.phoneticname))
            {
                SpeechService.Instance.Say(ship, ship.name + " stands ready.", false);
            }
            else
            {
                SpeechService.Instance.Say(ship, "<phoneme alphabet=\"ipa\" ph=\"" + ship.phoneticname + "\">" + ship.name + "</phoneme>" + " stands ready.", false);
            }
        }

        private void shipYardUpdated(object sender, DataTransferEventArgs e)
        {
            if (shipsConfiguration != null)
            {
                shipsConfiguration.ToFile();
            }
        }

        // Handle Text-to-speech tab

        private void ttsVoiceDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            ttsUpdated();
        }

        private void ttsEffectsLevelUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ttsUpdated();
        }

        private void ttsDistortionLevelUpdated(object sender, RoutedEventArgs e)
        {
            ttsUpdated();
        }

        private void ttsRateUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ttsUpdated();
        }

        private void ttsVolumeUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ttsUpdated();
        }

        private void ttsTestVoiceButtonClicked(object sender, RoutedEventArgs e)
        {
            Ship testShip = ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedValue);
            testShip.health = 100;
            SpeechService.Instance.Say(testShip, "This is how I will sound in your " + Translations.ShipModel((string)ttsTestShipDropDown.SelectedValue) + ".", false);
        }

        private void ttsTestDamagedVoiceButtonClicked(object sender, RoutedEventArgs e)
        {
            Ship testShip = ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedValue);
            testShip.health = 20;
            SpeechService.Instance.Say(testShip, "Severe damage to your " + Translations.ShipModel((string)ttsTestShipDropDown.SelectedValue) + ".", false);
        }

        /// <summary>
        /// fetch the Text-to-Speech Configuration and write it to File
        /// </summary>
        private void ttsUpdated()
        {
            SpeechServiceConfiguration speechConfiguration = new SpeechServiceConfiguration();
            speechConfiguration.StandardVoice = ttsVoiceDropDown.SelectedValue == null || ttsVoiceDropDown.SelectedValue.ToString() == "Windows TTS default" ? null : ttsVoiceDropDown.SelectedValue.ToString();
            speechConfiguration.Volume = (int)ttsVolumeSlider.Value;
            speechConfiguration.Rate = (int)ttsRateSlider.Value;
            speechConfiguration.EffectsLevel = (int)ttsEffectsLevelSlider.Value;
            speechConfiguration.DistortOnDamage = ttsDistortCheckbox.IsChecked.Value;
            speechConfiguration.ToFile();
            SpeechService.Instance.ReloadConfiguration();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Eddi.Instance.Stop();
            Application.Current.Shutdown();
        }

        private new void EnsureValidDecimal(object sender, TextCompositionEventArgs e)
        {
            // Match valid characters
            Regex regex = new Regex(@"[0-9\.]");
            // Swallow the character doesn't match the regex
            e.Handled = !regex.IsMatch(e.Text);
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

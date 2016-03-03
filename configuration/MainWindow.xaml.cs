using EDDIVAPlugin;
using EliteDangerousCompanionAppService;
using EliteDangerousDataDefinitions;
using EliteDangerousNetLogMonitor;
using EliteDangerousStarMapService;
using EliteDangerousSpeechService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Speech.Synthesis;
using EliteDangerousDataProviderService;

namespace configuration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Commander commander;
        private ShipsConfiguration shipsConfiguration;

        public MainWindow()
        {
            InitializeComponent();

            // Configured the EDDI tab
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiHomeSystemText.Text = eddiConfiguration.HomeSystem;
            eddiHomeStationText.Text = eddiConfiguration.HomeStation;
            eddiInsuranceDecimal.Value = eddiConfiguration.Insurance;

            // Configure the Companion App tab
            CompanionAppCredentials companionAppCredentials = CompanionAppCredentials.FromFile();
            // See if the credentials work
            CompanionAppService companionAppService = new CompanionAppService(companionAppCredentials);
            try
            {
                commander = companionAppService.Profile();
                setUpCompanionAppComplete("Your connection to the companion app is operational, Commander " + commander.Name);
            }
            catch (Exception ex)
            {
                // Fall back to stage 1
                setUpCompanionAppStage1();
            }

            if (commander != null)
            {
                setShipyardFromConfiguration();
            }

            // Configure the NetLog tab
            NetLogConfiguration netLogConfiguration = NetLogConfiguration.FromFile();
            netLogPathTextBox.Text = netLogConfiguration.path;

            // Configure the EDSM tab
            StarMapConfiguration starMapConfiguration = StarMapConfiguration.FromFile();
            edsmApiKeyTextBox.Text = starMapConfiguration.apiKey;
            edsmCommanderNameTextBox.Text = starMapConfiguration.commanderName;

            // Configure the Text-to-speech tab
            SpeechServiceConfiguration speechServiceConfiguration = SpeechServiceConfiguration.FromFile();
            List<String> speechOptions = new List<String>();
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
                using (System.IO.StreamWriter errLog = new System.IO.StreamWriter(Environment.GetEnvironmentVariable("AppData") + @"\EDDI\speech.log", true))
                {
                   errLog.WriteLine("" + System.Threading.Thread.CurrentThread.ManagedThreadId + ": Caught exception " + e);
                }
            }
            ttsRateSlider.Value = speechServiceConfiguration.Rate;
            ttsEffectsLevelSlider.Value = speechServiceConfiguration.EffectsLevel;
            ttsDistortCheckbox.IsChecked = speechServiceConfiguration.DistortOnDamage;

            ttsTestShipDropDown.ItemsSource = ShipDefinitions.ShipModels;
            ttsTestShipDropDown.Text = "Adder";
        }

        // Handle changes to the eddi tab
        private void homeSystemChanged(object sender, TextChangedEventArgs e)
        {
            updateEddiConfiguration();
        }

        private void homeStationChanged(object sender, TextChangedEventArgs e)
        {
            updateEddiConfiguration();
        }


        private void insuranceChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            updateEddiConfiguration();
        }

        private void updateEddiConfiguration()
        {
            EDDIConfiguration eddiConfiguration = new EDDIConfiguration();
            if (!String.IsNullOrWhiteSpace(eddiHomeSystemText.Text))
            {
                eddiConfiguration.HomeSystem = eddiHomeSystemText.Text.Trim();
            }
            if (!String.IsNullOrWhiteSpace(eddiHomeStationText.Text))
            {
                eddiConfiguration.HomeStation = eddiHomeStationText.Text.Trim();
            }
            if (eddiInsuranceDecimal.Value != null)
            {
                eddiConfiguration.Insurance = (decimal)eddiInsuranceDecimal.Value;
            }
            eddiConfiguration.ToFile();
        }

        // Handle changes to the companion app tab
        private void companionAppNextClicked(object sender, RoutedEventArgs e)
        {
            // See if the user is entering their email address and password
            if (companionAppEmailText.Visibility == Visibility.Visible)
            {
                // Stage 1 of authentication - login
                string email = companionAppEmailText.Text.Trim();
                string password = companionAppPasswordText.Password.Trim();
                try
                {
                    CompanionAppCredentials companionAppCredentials = CompanionAppCredentials.FromFile();
                    companionAppCredentials = CompanionAppService.Login(companionAppCredentials, email, password);
                    companionAppCredentials.ToFile();
                    setUpCompanionAppStage2();
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
                try
                {
                    CompanionAppCredentials companionAppCredentials = CompanionAppCredentials.FromFile();
                    companionAppCredentials = CompanionAppService.Confirm(companionAppCredentials, companionAppCodeText.Text.Trim());
                    companionAppCredentials.ToFile();
                    // All done - see if it works
                    CompanionAppService companionAppService = new CompanionAppService(companionAppCredentials);
                    commander = companionAppService.Profile();
                    setUpCompanionAppComplete("Your connection to the companion app is operational, Commander " + commander.Name);
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
            companionAppPasswordLabel.Visibility = Visibility.Visible;
            companionAppPasswordText.Visibility = Visibility.Visible;
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

        // Handle changes to NetLog tab
        private void netLogPathChanged(object sender, TextChangedEventArgs e)
        {
            updateNetLogConfiguration();
        }

        private void updateNetLogConfiguration()
        {
            NetLogConfiguration netLogConfiguration = new NetLogConfiguration();
            if (!String.IsNullOrWhiteSpace(netLogPathTextBox.Text))
            {
                netLogConfiguration.path = netLogPathTextBox.Text.Trim();
            }
            netLogConfiguration.ToFile();
        }

        private void netLogObtainClicked(object sender, RoutedEventArgs e)
        {
            List<string> processPaths = new Finder().GetPathFromProcess();
            if (processPaths.Count != 0)
            {
                netLogPathTextBox.Text = processPaths[0] + @"\Logs";
                updateNetLogConfiguration();
            }
            else
            {
                netLogText.Text = @"Unfortuantely we were unable to locate your product directory.  Please type in the location of the 'Logs' directory in your 'elite-dangerous-64' directory.  Possible locations include:";
                List<string> paths = new Finder().FindInstallationPaths();
                if (paths.Count == 0)
                {
                    paths.Add(Finder.DefProductsPath + @"\elite-dangerous-64");
                    paths.Add(Finder.DefLauncherPath + @"\elite-dangerous-64");
                    paths.Add(@"C:\Program Files (x86)\Steam\\SteamApps\common\Elite Dangerous\Products\elite-dangerous-64");
                }
                foreach (string path in paths)
                {
                    netLogText.Text += "\r\n\r\n" + path + @"\Logs";
                }
                netLogText.Text += "\r\n\r\nWhichever directory you select should contain a number of 'debugLog' files.";
            }
        }

        // Handle changes to EDSM tab
        private void edsmCommanderNameChanged(object sender, TextChangedEventArgs e)
        {
            updateEdsmConfiguration();
        }

        private void edsmApiKeyChanged(object sender, TextChangedEventArgs e)
        {
            updateEdsmConfiguration();
        }

        private void updateEdsmConfiguration()
        {
            StarMapConfiguration edsmConfiguration = new StarMapConfiguration();
            if (!String.IsNullOrWhiteSpace(edsmApiKeyTextBox.Text))
            {
                edsmConfiguration.apiKey = edsmApiKeyTextBox.Text.Trim();
            }
            if (!String.IsNullOrWhiteSpace(edsmCommanderNameTextBox.Text))
            {
                edsmConfiguration.commanderName = edsmCommanderNameTextBox.Text.Trim();
            }
            edsmConfiguration.ToFile();

        }

        // Handle changes to the Shipyard tab
        private void setShipyardFromConfiguration()
        {
            shipsConfiguration = new ShipsConfiguration();
            List<Ship> ships = new List<Ship>();
            if (commander != null)
            {
                ships.Add(commander.Ship);
                ships.AddRange(commander.StoredShips);
            }
            shipsConfiguration.Ships = ships;
            shipyardData.ItemsSource = ships;
        }

        private void testShipName(object sender, RoutedEventArgs e)
        {
            Ship ship = (Ship)((Button)e.Source).DataContext;
            ship.Health = 100;
            SpeechServiceConfiguration speechConfiguration = SpeechServiceConfiguration.FromFile();
            SpeechService speechService = new SpeechService(speechConfiguration);
            if (String.IsNullOrEmpty(ship.PhoneticName))
            {
                speechService.Say(ship, ship.Name + " stands ready.");
            }
            else
            {
                speechService.Say(ship, "<phoneme alphabet=\"ipa\" ph=\"" + ship.PhoneticName + "\">" + ship.Name + "</phoneme>" + " stands ready.");
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

        private void ttsTestVoiceButtonClicked(object sender, RoutedEventArgs e)
        {
            Ship testShip = ShipDefinitions.ShipFromModel((string)ttsTestShipDropDown.SelectedValue);
            testShip.Health = 100;
            SpeechServiceConfiguration speechConfiguration = SpeechServiceConfiguration.FromFile();
            SpeechService speechService = new SpeechService(speechConfiguration);
            speechService.Say(testShip, "This is how I will sound in your " + Translations.ShipModel((string)ttsTestShipDropDown.SelectedValue) + ".");
        }

        private void ttsTestDamagedVoiceButtonClicked(object sender, RoutedEventArgs e)
        {
            Ship testShip = ShipDefinitions.ShipFromModel((string)ttsTestShipDropDown.SelectedValue);
            testShip.Health = 20;
            SpeechServiceConfiguration speechConfiguration = SpeechServiceConfiguration.FromFile();
            SpeechService speechService = new SpeechService(speechConfiguration);
            speechService.Say(testShip, "Severe damage to your " + Translations.ShipModel((string)ttsTestShipDropDown.SelectedValue) + ".");
        }

        /// <summary>
        /// fetch the Text-to-Speech Configuration and write it to File
        /// </summary>
        private void ttsUpdated()
        {
            SpeechServiceConfiguration speechConfiguration = new SpeechServiceConfiguration();
            speechConfiguration.StandardVoice = ttsVoiceDropDown.SelectedValue == null || ttsVoiceDropDown.SelectedValue.ToString() == "Windows TTS default" ? null : ttsVoiceDropDown.SelectedValue.ToString();
            speechConfiguration.Rate = (int)ttsRateSlider.Value;
            speechConfiguration.EffectsLevel = (int)ttsEffectsLevelSlider.Value;
            speechConfiguration.DistortOnDamage = ttsDistortCheckbox.IsChecked.Value;
            speechConfiguration.ToFile();
        }

        /// <summary>
        /// Obtain the EDSM log and sync it with the local datastore
        /// </summary>
        private void edsmObtainLogClicked(object sender, RoutedEventArgs e)
        {
            IEDDIStarSystemRepository starSystemRepository = new EDDIStarSystemSqLiteRepository();
            StarMapConfiguration starMapConfiguration = StarMapConfiguration.FromFile();

            string commanderName;
            if (String.IsNullOrEmpty(starMapConfiguration.commanderName))
            {
                // Fetch the commander name from the companion app
                CompanionAppService companionAppService = new CompanionAppService(CompanionAppCredentials.FromFile());
                Commander cmdr = companionAppService.Profile();
                if (cmdr != null && cmdr.Name != null)
                {
                    commanderName = cmdr.Name;
                }
                else
                {
                    edsmFetchLogsButton.IsEnabled = false;
                    edsmFetchLogsButton.Content = "Companion app not configured and no name supplied; cannot obtain logs";
                    return;
                }
            }
            else
            {
                commanderName = starMapConfiguration.commanderName;
            }

            edsmFetchLogsButton.IsEnabled = false;
            edsmFetchLogsButton.Content = "Obtaining log...";

            StarMapService starMapService = new StarMapService(starMapConfiguration.apiKey, commanderName);

            Dictionary<string, StarMapLogInfo> systems = starMapService.getStarMapLog();
            foreach (string system in systems.Keys)
            {
                EDDIStarSystem CurrentStarSystemData = starSystemRepository.GetEDDIStarSystem(system);
                if (CurrentStarSystemData == null)
                {
                    // We have no record of this system; set it up
                    CurrentStarSystemData = new EDDIStarSystem();
                    CurrentStarSystemData.Name = system;
                    // Due to the potential large number of systems being imported we don't pull individual system data at this time
                }
                CurrentStarSystemData.TotalVisits = systems[system].visits;
                CurrentStarSystemData.LastVisit = systems[system].lastVisit;
                CurrentStarSystemData.PreviousVisit = systems[system].previousVisit;
                starSystemRepository.SaveEDDIStarSystem(CurrentStarSystemData);
            }

            edsmFetchLogsButton.Content = "Log obtained";
        }
    }
}

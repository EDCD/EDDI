using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiSpeechService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using Utilities;

namespace Eddi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Profile profile;
        private bool fromVA;

        struct LanguageDef : IComparable<LanguageDef>
        {
            public CultureInfo ci;
            public string displayName { get; set; }

            public LanguageDef(CultureInfo ci)
            {
                this.ci = ci;
                this.displayName = ci.NativeName;
            }

            public LanguageDef(CultureInfo ci, string displayName)
            {
                this.ci = ci;
                this.displayName = displayName;
            }

            public int CompareTo(LanguageDef rhs)
            {
                return this.displayName.CompareTo(rhs.displayName);
            }
        }

        public MainWindow() : this(false) { }

        private void SaveWindowState()
        {
            Rect savePosition;

            switch (WindowState)
            {
                case WindowState.Maximized:
                    savePosition = new Rect(RestoreBounds.Left, RestoreBounds.Top, RestoreBounds.Width, RestoreBounds.Height);
                    Properties.Settings.Default.Maximized = true;
                    Properties.Settings.Default.Minimized = false;
                    break;
                case WindowState.Minimized:
                    savePosition = new Rect(RestoreBounds.Left, RestoreBounds.Top, RestoreBounds.Width, RestoreBounds.Height);
                    Properties.Settings.Default.Maximized = false;

                    // If opened from VoiceAttack, don't allow minimized state
                    Properties.Settings.Default.Minimized = fromVA ? false: true;

                    break;
                default:
                    savePosition = new Rect(Left, Top, Width, Height);
                    Properties.Settings.Default.Maximized = false;
                    Properties.Settings.Default.Minimized = false;
                    break;
            }

            Properties.Settings.Default.WindowPosition = savePosition;

            // Remember which tab we have selected in EDDI
            Properties.Settings.Default.SelectedTab = this.tabControl.SelectedIndex;

            Properties.Settings.Default.Save();
        }

        private void RestoreWindowState()
        {
            const int designedHeight = 600;
            const int designedWidth = 800;

            Rect windowPosition = Properties.Settings.Default.WindowPosition;
            Visibility = Visibility.Collapsed;

            if (windowPosition != Rect.Empty && isWindowValid(windowPosition))
            {
                // Hook Loaded event to handle minimized/maximized state restore
                Loaded += windowLoaded;
                WindowStartupLocation = WindowStartupLocation.Manual;

                // Restore persisted window size & position
                Left = windowPosition.Left;
                Top = windowPosition.Top;
                Width = windowPosition.Width;
                Height = windowPosition.Height;
            }
            else
            {
                // Revert to default values if the prior size and position are no longer valid
                Left = centerWindow(Screen.PrimaryScreen.Bounds.Width, designedWidth);
                Top = centerWindow(Screen.PrimaryScreen.Bounds.Height, designedHeight);
                Width = Math.Min(Screen.PrimaryScreen.Bounds.Width, designedWidth);
                Height = Math.Min(Screen.PrimaryScreen.Bounds.Height, designedHeight);
            }

            tabControl.SelectedIndex = Eddi.Properties.Settings.Default.SelectedTab;

            // Check detected monitors to see if the saved window size and location is valid
            bool isWindowValid(Rect rect)
            {
                // Check for minimum window size
                if ((int)rect.Width < designedWidth || (int)rect.Height < designedHeight)
                {
                    return false;
                }

                // Check whether the rectangle is completely visible on-screen
                bool testUpperLeft = false;
                bool testLowerRight = false;
                foreach (Screen screen in Screen.AllScreens)
                {
                    if (rect.X >= screen.Bounds.X && rect.Y >= screen.Bounds.Y) // The upper and left bounds fall on a valid screen
                    {
                        testUpperLeft = true;
                    }
                    if (screen.Bounds.Width >= rect.X + rect.Width && screen.Bounds.Height >= rect.Y + rect.Height) // The lower and right bounds fall on a valid screen 
                    {
                        testLowerRight = true;
                    }
                }
                if (testUpperLeft && testLowerRight)
                {
                    return true;
                }
                return false;
            }

            int centerWindow(int measure, int defaultValue)
            {
                return (measure - Math.Min(measure, defaultValue)) / 2;
            }
        }

        private bool runBetaCheck = false;

        private static readonly object logLock = new object();

        public MainWindow(bool fromVA = false)
        {
            InitializeComponent();

            this.fromVA = fromVA;

            // Start the EDDI instance
            EDDI.FromVA = fromVA;
            EDDI.Instance.Start();

            // Configure the EDDI tab
            setStatusInfo();

            // Need to set up the correct information in the hero text depending on from where we were started
            if (fromVA)
            {
                // Allow the EDDI VA plugin to change window state
                VaWindowStateChange = new vaWindowStateChangeDelegate(OnVaWindowStateChange);
                heroText.Text = Properties.EddiResources.change_affect_va;
            }
            else
            {
                heroText.Text = Properties.EddiResources.if_using_va;
            }

            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiHomeSystemText.Text = eddiConfiguration.validSystem == true ? eddiConfiguration.HomeSystem : string.Empty;
            eddiHomeStationText.Text = eddiConfiguration.validStation == true ? eddiConfiguration.HomeStation : string.Empty;
            eddiInsuranceDecimal.Text = eddiConfiguration.Insurance.ToString(CultureInfo.InvariantCulture);
            eddiVerboseLogging.IsChecked = eddiConfiguration.Debug;
            eddiBetaProgramme.IsChecked = eddiConfiguration.Beta;
            if (eddiConfiguration.Gender == "Female")
            {
                eddiGenderFemale.IsChecked = true;
            }
            else if (eddiConfiguration.Gender == "Male")
            {
                eddiGenderMale.IsChecked = true;
            }
            else
            {
                eddiGenderNeither.IsChecked = true;
            }

            List<LanguageDef> langs = GetAvailableLangs();
            chooseLanguageDropDown.ItemsSource = langs;
            chooseLanguageDropDown.DisplayMemberPath = "displayName";
            chooseLanguageDropDown.SelectedItem = langs.Find(l => l.ci.Name == Eddi.Properties.Settings.Default.OverrideCulture);
            chooseLanguageDropDown.SelectionChanged += (sender, e) =>
            {
                LanguageDef cultureDef = (LanguageDef)chooseLanguageDropDown.SelectedValue;
                Eddi.Properties.Settings.Default.OverrideCulture = cultureDef.ci.Name;
            };

            // Configure the Frontier API tab
            CompanionAppCredentials companionAppCredentials = CompanionAppCredentials.FromFile();
            companionAppEmailText.Text = companionAppCredentials.email;
            // See if the credentials work
            try
            {
                profile = CompanionAppService.Instance.Profile();
                if (profile == null)
                {
                    setUpCompanionAppComplete(Properties.EddiResources.frontier_api_temp_nok);
                }
                else
                {
                    setUpCompanionAppComplete(String.Format(Properties.EddiResources.frontier_api_ok, profile.Cmdr.name));
                }
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

            // Configure the Text-to-speech tab
            ConfigureTTS();

            LoadMonitors(eddiConfiguration);

            LoadResponders(eddiConfiguration);

            RestoreWindowState();
            EDDI.Instance.Start();
        }

        private List<LanguageDef> GetAvailableLangs()
        {
            List<LanguageDef> cultures = new List<LanguageDef>();

            // Add the "Automatic" culture, we are using the InvariantCulture name "" to mean user's culture
            cultures.Add(new LanguageDef(CultureInfo.InvariantCulture, Properties.EddiResources.system_language)); 

            CultureInfo neutralInfo = new CultureInfo("en"); // Add our "neutral" language "en".
            cultures.Add(new LanguageDef(neutralInfo));

            // Add our satellite resource language folders to the list. Since these are stored according to folder name, we can interate through folder names to identify supported resources
            List<LanguageDef> satelliteCultures = new List<LanguageDef>();
            DirectoryInfo rootInfo = new DirectoryInfo(new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName);
            DirectoryInfo[] subDirs = rootInfo.GetDirectories();
            foreach (DirectoryInfo dir in subDirs)
            {
                try
                {
                    CultureInfo cInfo = new CultureInfo(dir.Name);
                    satelliteCultures.Add(new LanguageDef(cInfo));
                }
                catch
                {}
            }
            satelliteCultures.Sort();
            cultures.AddRange(satelliteCultures);

            return cultures;
        }

        private void LoadMonitors(EDDIConfiguration eddiConfiguration)
        {
            foreach (EDDIMonitor monitor in EDDI.Instance.monitors)
            {
                Logging.Debug("Adding configuration tab for " + monitor.MonitorName());

                System.Windows.Controls.UserControl monitorConfiguration = monitor.ConfigurationTabItem();
                // Only show a tab if this can be turned off or has configuration elements
                if (monitorConfiguration != null || !monitor.IsRequired())
                {
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
                    if (monitorConfiguration != null)
                    {
                        skeleton.panel.Children.Add(monitorConfiguration);
                    }

                    TabItem item = new TabItem { Header = monitor.LocalizedMonitorName() };
                    item.Content = skeleton;
                    tabControl.Items.Add(item);
                }
            }
        }

        private void LoadResponders(EDDIConfiguration eddiConfiguration)
        {
            foreach (EDDIResponder responder in EDDI.Instance.responders)
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
                System.Windows.Controls.UserControl monitorConfiguration = responder.ConfigurationTabItem();
                if (monitorConfiguration != null)
                {
                    monitorConfiguration.Margin = new Thickness(10);
                    skeleton.panel.Children.Add(monitorConfiguration);
                }

                TabItem item = new TabItem { Header = responder.LocalizedResponderName() };
                item.Content = skeleton;
                tabControl.Items.Add(item);
            }
        }

        private void ConfigureTTS()
        {
            SpeechServiceConfiguration speechServiceConfiguration = SpeechServiceConfiguration.FromFile();
            List<string> speechOptions = new List<string>();
            speechOptions.Add("Windows TTS default");
            try
            {
                using (SpeechSynthesizer synth = new SpeechSynthesizer())
                {
                    foreach (InstalledVoice voice in synth.GetInstalledVoices())
                    {
                        if (voice.Enabled && (!voice.VoiceInfo.Name.Contains("Microsoft Server Speech Text to Speech Voice")))
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
            disableSsmlCheckbox.IsChecked = speechServiceConfiguration.DisableSsml;
            enableIcaoCheckbox.IsChecked = speechServiceConfiguration.EnableIcao;

            ttsTestShipDropDown.ItemsSource = ShipDefinitions.ShipModels;
            ttsTestShipDropDown.Text = "Adder";
        }

        // Hook the window Loaded event to set minimize/maximize state at startup 
        private void windowLoaded(object sender, RoutedEventArgs e)
        {
            var senderWindow = sender as Window;

            if (Properties.Settings.Default.Maximized || Properties.Settings.Default.Minimized)
            {
                if (Properties.Settings.Default.Minimized)
                {
                    senderWindow.WindowState = WindowState.Minimized;
                }
                else
                {
                    senderWindow.WindowState = WindowState.Maximized;
                }

                Visibility = Visibility.Visible;
            }
        }

        // Handle changes to the eddi tab
        private void homeSystemChanged(object sender, TextChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiConfiguration.HomeSystem = string.IsNullOrWhiteSpace(eddiHomeSystemText.Text) ? null : eddiHomeSystemText.Text.Trim();
            eddiConfiguration = EDDI.Instance.updateHomeSystem(eddiConfiguration);
            eddiConfiguration.ToFile();

            // Update the UI for invalid results
            runValidation(eddiHomeSystemText);
        }

        private void homeStationChanged(object sender, TextChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiConfiguration.HomeStation = string.IsNullOrWhiteSpace(eddiHomeStationText.Text) ? null : eddiHomeStationText.Text.Trim();
            eddiConfiguration = EDDI.Instance.updateHomeStation(eddiConfiguration);
            eddiConfiguration.ToFile();

            // Update the UI for invalid results
            runValidation(eddiHomeStationText);
        }

        private void insuranceChanged(object sender, TextChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            try
            {
                eddiConfiguration.Insurance = string.IsNullOrWhiteSpace(eddiInsuranceDecimal.Text) ? 5 : Convert.ToDecimal(eddiInsuranceDecimal.Text, CultureInfo.InvariantCulture);
                eddiConfiguration.ToFile();
            }
            catch
            {
                // Bad user input; ignore it
            }
        }

        private void isMale_Checked(object sender, RoutedEventArgs e)
         {
             EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
             eddiConfiguration.Gender = "Male";
             eddiConfiguration.ToFile();
             EDDI.Instance.Cmdr.gender = "Male";
          }

        private void isFemale_Checked(object sender, RoutedEventArgs e)
        {
             EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
             eddiConfiguration.Gender = "Female";
             eddiConfiguration.ToFile();
             EDDI.Instance.Cmdr.gender = "Female";
        }

        private void isNeitherGender_Checked(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiConfiguration.Gender = "Neither";
            eddiConfiguration.ToFile();
            EDDI.Instance.Cmdr.gender = "Neither";
        }

        private void verboseLoggingEnabled(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiConfiguration.Debug = eddiVerboseLogging.IsChecked.Value;
            Logging.Verbose = eddiConfiguration.Debug;
            eddiConfiguration.ToFile();
        }

        private void verboseLoggingDisabled(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiConfiguration.Debug = eddiVerboseLogging.IsChecked.Value;
            Logging.Verbose = eddiConfiguration.Debug;
            eddiConfiguration.ToFile();
        }

        private void betaProgrammeEnabled(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiConfiguration.Beta = eddiBetaProgramme.IsChecked.Value;
            eddiConfiguration.ToFile();
            if (runBetaCheck)
            {
                // Because we have changed to wanting beta upgrades we need to re-check upgrade information
                EDDI.Instance.CheckUpgrade();
                setStatusInfo();
            }
            else
            {
                runBetaCheck = true;
            }
        }

        private void betaProgrammeDisabled(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiConfiguration.Beta = eddiBetaProgramme.IsChecked.Value;
            eddiConfiguration.ToFile();
            if (runBetaCheck)
            {
                // Because we have changed to not wanting beta upgrades we need to re-check upgrade information
                EDDI.Instance.CheckUpgrade();
                setStatusInfo();
            }
            else
            {
                runBetaCheck = true;
            }
        }

        // Set the fields relating to status information
        private void setStatusInfo()
        {
            versionText.Text = Constants.EDDI_VERSION;
            Title = "EDDI v." + Constants.EDDI_VERSION;

            if (EDDI.Instance.UpgradeVersion != null)
            {
                statusText.Text = String.Format(Properties.EddiResources.update_message, EDDI.Instance.UpgradeVersion);
                // Do not show upgrade button if EDDI is started from VA
                upgradeButton.Visibility = EDDI.FromVA ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                upgradeButton.Visibility = Visibility.Collapsed;
                if (CompanionAppService.Instance.CurrentState != CompanionAppService.State.READY)
                {
                    statusText.Text = Properties.EddiResources.frontier_api_nok;
                }
                else if (!EDDI.running)
                {
                    statusText.Text = Properties.EddiResources.safe_mode;
                }
                else
                {
                    statusText.Text = Properties.EddiResources.operational;
                }
            }
        }

        private void companionAppResetClicked(object sender, RoutedEventArgs e)
        {
            // Logout from the companion app and start again
            CompanionAppService.Instance.Logout();
            setUpCompanionAppStage1();
        }

        // Handle changes to the Frontier API tab
        private void companionAppNextClicked(object sender, RoutedEventArgs e)
        {
            // See if the user is entering their email address and password
            if (companionAppEmailText.Visibility == Visibility.Visible)
            {
                // Stage 1 of authentication - login
                CompanionAppService.Instance.Credentials.email = companionAppEmailText.Text.Trim();
                CompanionAppService.Instance.setPassword(companionAppPasswordText.Password.Trim());
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
                        if (profile == null)
                        {
                            setUpCompanionAppComplete(Properties.EddiResources.frontier_api_temp_nok);
                        }
                        else
                        {
                            setUpCompanionAppComplete(String.Format(Properties.EddiResources.frontier_api_ok, profile.Cmdr.name));
                        }
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
                catch (Exception)
                {
                    companionAppText.Text = Properties.EddiResources.login_nok_frontier_service;
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
                    if (profile != null)
                    {
                        setUpCompanionAppComplete(String.Format(Properties.EddiResources.frontier_api_ok, profile.Cmdr.name));
                    }
                }
                catch (EliteDangerousCompanionAppAuthenticationException ex)
                {
                    setUpCompanionAppStage1(ex.Message);
                }
                catch (EliteDangerousCompanionAppErrorException ex)
                {
                    setUpCompanionAppStage1(ex.Message);
                }
                catch (Exception)
                {
                    setUpCompanionAppStage1(Properties.EddiResources.login_nok_frontier_service);
                }
            }
        }

        private void setUpCompanionAppStage1(string message = null)
        {
            if (message == null)
            {
                companionAppText.Text = Properties.EddiResources.frontier_api_no_logins;
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
            companionAppPasswordText.Password = null;
            companionAppCodeText.Text = "";
            companionAppCodeLabel.Visibility = Visibility.Hidden;
            companionAppCodeText.Visibility = Visibility.Hidden;
            companionAppNextButton.Content = Properties.EddiResources.next;
        }

        private void setUpCompanionAppStage2(string message = null)
        {
            if (message == null)
            {
                companionAppText.Text = Properties.EddiResources.frontier_api_verification_code;
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
            companionAppNextButton.Content = Properties.EddiResources.next;
        }

        private void setUpCompanionAppComplete(string message = null)
        {
            if (message == null)
            {
                companionAppText.Text = Properties.EddiResources.complete;
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
            companionAppNextButton.Content = Properties.EddiResources.logout;
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
            string message = String.Format(Properties.EddiResources.voice_test_ship, ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedValue).SpokenModel());
            SpeechService.Instance.Say(testShip, message, false);
        }

        private void ttsTestDamagedVoiceButtonClicked(object sender, RoutedEventArgs e)
        {
            Ship testShip = ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedValue);
            testShip.health = 20;
            string message = String.Format(Properties.EddiResources.voice_test_damage, ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedValue).SpokenModel());
            SpeechService.Instance.Say(testShip, message, false);
        }

        private void disableSsmlUpdated(object sender, RoutedEventArgs e)
        {
            ttsUpdated();
        }

        private void enableICAOUpdated(object sender, RoutedEventArgs e)
        {
            ttsUpdated();
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
            speechConfiguration.DisableSsml = disableSsmlCheckbox.IsChecked.Value;
            speechConfiguration.EnableIcao = enableIcaoCheckbox.IsChecked.Value;
            speechConfiguration.ToFile();
            SpeechService.Instance.ReloadConfiguration();
        }

        // Called from the VoiceAttack plugin if the "Configure EDDI" voice command has
        // been given and the EDDI configuration window is already open. If the window
        // is minimize, restore it, otherwise the plugin will ignore the command.
        public delegate void vaWindowStateChangeDelegate(WindowState state, bool minimizeCheck);
        public vaWindowStateChangeDelegate VaWindowStateChange;
        public void OnVaWindowStateChange(WindowState state, bool minimizeCheck)
        {
            if (minimizeCheck && WindowState == WindowState.Minimized)
                WindowState = WindowState.Normal;
            else if (!minimizeCheck && WindowState != state)
                WindowState = state;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!e.Cancel)
            {
                // Save window position here as the RestoreBounds rect gets set
                // to empty somewhere between here and OnClosed.
                SaveWindowState();

                if (!fromVA)
                {
                    // When in OnClosed(), if the EDDI window was closed while minimized
                    // (under debugger), monitorThread.Join() would block waiting for a
                    // thread(s) to terminate. Strange, because it does not block when the
                    // window is closed in the normal or maximized state.
                    EDDI.Instance.Stop();
                }
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (!fromVA)
            {
                SpeechService.Instance.ShutUp();
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void EnsureValidDecimal(object sender, TextCompositionEventArgs e)
        {
            // Match valid characters
            Regex regex = new Regex(@"[0-9\.]");
            // Swallow the character doesn't match the regex
            e.Handled = !regex.IsMatch(e.Text);
        }

        private async void createGithubIssueClicked(object sender, RoutedEventArgs e)
        {
            // Write out useful information to the log before procedding
            Logging.Info("EDDI version: " + Constants.EDDI_VERSION);
            Logging.Info("Commander name: " + (EDDI.Instance.Cmdr != null ? EDDI.Instance.Cmdr.name : "unknown"));

            // Prepare a truncated log file for export if verbose logging is enabled
            if (eddiVerboseLogging.IsChecked.Value)
            {
                Logging.Debug("Preparing log for export.");
                var progress = new Progress<string>(s => githubIssueButton.Content = Properties.EddiResources.preparing_log + s);
                await Task.Factory.StartNew(() => prepareLog(progress), TaskCreationOptions.LongRunning);
            }
            
            createGithubIssue();
        }

        private void ChangeLog_Click(object sender, RoutedEventArgs e)
        {
            ChangeLogWindow changeLog = new ChangeLogWindow();
            changeLog.Show();
        }

        public static void prepareLog(IProgress<string> progress)
        {
            try
            {
                string issueLogDir = Constants.DATA_DIR + @"\logexport\";
                string issueLogFile = issueLogDir + "eddi_issue.log";
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\eddi_issue.zip";

                string[] log;
                lock (logLock)
                {
                    log = File.ReadAllLines(Constants.DATA_DIR + @"\eddi.log");
                }
                if (log.Length == 0) { return; }

                progress.Report("");
                List<string> outputLines = new List<string>();
                // Use regex to isolate DateTimes from the string
                Regex recentLogsRegex = new Regex(@"^(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2})");

                foreach (string line in log)
                {
                    try
                    {
                        // Parse log file lines so that we can examine DateTimes
                        string linedatestring = recentLogsRegex.Match(line).Value;
                        string formatString = "s"; // i.e. DateTimeFormatInfo.SortableDateTimePattern
                        if (DateTime.TryParseExact(linedatestring, formatString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime linedate))
                        {
                            linedate = linedate.ToUniversalTime();
                            double elapsedHours = (DateTime.UtcNow - linedate).TotalHours;
                            // Fill the issue log with log lines from the most recent hour only
                            if (elapsedHours < 1.0)
                            {
                                outputLines.Add(line);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        // Do nothing, adding to the debug log creates a feedback loop
                    }
                }
                if (outputLines.Count == 0)
                {
                    Logging.Error("Error parsing log. Algorithm failed to return any matches.");
                    return;
                }

                // Create a temporary issue log file, delete any remnants from prior issue reporting
                Directory.CreateDirectory(issueLogDir);
                File.WriteAllLines(issueLogFile, outputLines);

                // Copy the issue log & zip it to the desktop so that it can be added to the Github issue
                File.Delete(desktopPath);
                ZipFile.CreateFromDirectory(issueLogDir, desktopPath);

                // Clear the temporary issue log file & directory
                File.Delete(issueLogFile);
                Directory.Delete(issueLogDir);

                progress.Report(Properties.EddiResources.done);
            }
            catch (Exception ex)
            {
                progress.Report(Properties.EddiResources.failed);
                Logging.Error("Failed to prepare log", ex);

            }
        }

        private void createGithubIssue()
        {
            Process.Start("https://github.com/EDCD/EDDI/issues/new");
        }

        private void upgradeClicked(object sender, RoutedEventArgs e)
        {
            EDDI.Instance.Upgrade();
        }

        private void EDDIClicked(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/EDCD/EDDI/blob/master/README.md");
        }

        private void WikiClicked(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/EDCD/EDDI/wiki");
        }

        private void TroubleshootClicked(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/EDCD/EDDI/blob/master/TROUBLESHOOTING.md");
        }

        private void runValidation(System.Windows.Controls.TextBox textBox)
        {
            BindingExpression b = BindingOperations.GetBindingExpression(textBox, System.Windows.Controls.TextBox.TextProperty);
            try
            {
                b.ValidateWithoutUpdate();
            }
            catch { }
        }

        private void eddiHomeSystemText_LostFocus(object sender, RoutedEventArgs e)
        {
            // Discard invalid results
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            if (!eddiConfiguration.validSystem)
            {
                eddiConfiguration.HomeSystem = null;
                eddiHomeSystemText.Text = string.Empty;
                eddiConfiguration.ToFile();
            }
        }

        private void eddiHomeStationText_LostFocus(object sender, RoutedEventArgs e)
        {
            // Discard invalid results
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            if (!eddiConfiguration.validStation)
            {
                eddiConfiguration.HomeStation = null;
                eddiHomeStationText.Text = string.Empty;
                eddiConfiguration.ToFile();
            }
        }
    }

    public class ValidSystemRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();

            if (value == null)
            {
                return ValidationResult.ValidResult;
            }
            if (eddiConfiguration.validSystem)
            {
                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, Properties.EddiResources.invalid_system);
            }
        }
    }

    public class ValidStationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();

            if (value == null)
            {
                return ValidationResult.ValidResult;
            }
            if (eddiConfiguration.validStation)
            {
                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, Properties.EddiResources.invalid_station);
            }
        }
    }
}

using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiSpeechService;
using System;
using System.Collections.Generic;
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

        public MainWindow() : this(false) { }

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

            //// Need to set up the correct information in the hero text depending on from where we were started
            if (fromVA)
            {
                heroText.Text = I18N.GetString("change_affect_va");
            }
            else
            {
                heroText.Text = I18N.GetString("if_using_va");
            }

            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            eddiHomeSystemText.Text = eddiConfiguration.HomeSystem;
            eddiHomeStationText.Text = eddiConfiguration.HomeStation;
            eddiInsuranceDecimal.Text = eddiConfiguration.Insurance.ToString(CultureInfo.InvariantCulture);
            eddiVerboseLogging.IsChecked = eddiConfiguration.Debug;
            eddiBetaProgramme.IsChecked = eddiConfiguration.Beta;

            Logging.Verbose = eddiConfiguration.Debug;

            // Configure the Frontier API tab
            CompanionAppCredentials companionAppCredentials = CompanionAppCredentials.FromFile();
            companionAppEmailText.Text = companionAppCredentials.email;
            // See if the credentials work
            try
            {
                profile = CompanionAppService.Instance.Profile();
                if (profile == null)
                {
                    setUpCompanionAppComplete(I18N.GetString("frontier_api_temp_nok"));
                }
                else
                {
                    setUpCompanionAppComplete(I18N.GetStringWithArgs("frontier_api_ok", new string[]{profile.Cmdr.name}));
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

            //if (profile != null)
            //{
            //    setShipyardFromConfiguration();
            //}

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

            foreach (EDDIMonitor monitor in EDDI.Instance.monitors)
            {
                Logging.Debug("Adding configuration tab for " + monitor.MonitorName());

                UserControl monitorConfiguration = monitor.ConfigurationTabItem();
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

                    TabItem item = new TabItem { Header = monitor.MonitorLocalName() };
                    item.Content = skeleton;
                    tabControl.Items.Add(item);
                }
            }

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
                UserControl monitorConfiguration = responder.ConfigurationTabItem();
                if (monitorConfiguration != null)
                {
                    monitorConfiguration.Margin = new Thickness(10);
                    skeleton.panel.Children.Add(monitorConfiguration);
                }

                TabItem item = new TabItem { Header = responder.ResponderLocalName() };
                item.Content = skeleton;
                tabControl.Items.Add(item);
            }

            chooseLanguageDropDown.ItemsSource = I18N.GetAvailableLangs();
            chooseLanguageDropDown.SelectedItem = I18N.GetLang();
            // Setting the handler there to avoid EDDI to restart indefinitely
            chooseLanguageDropDown.SelectionChanged += (sender, e) =>
            {
                EDDIConfiguration conf = EDDIConfiguration.FromFile();
                conf.Lang = chooseLanguageDropDown.SelectedValue.ToString();
                conf.ToFile();
                // Restarting EDDI
                Process.Start(Application.ResourceAssembly.Location);
                Application.Current.Shutdown();
            };
            I18NForComponents();

            EDDI.Instance.Start();
        }

        private void I18NForComponents()
        {
            versionHyperlinkText.Text = I18N.GetString("version_hyperlink");
            upgradeButton.Content = I18N.GetString("upgrade_button");
            statusPreText.Text = I18N.GetString("status_pre_text");
            par0.Text = I18N.GetString("pres_par0");
            par1.Text = I18N.GetString("pres_par1");
            par2.Text = I18N.GetString("pres_par2");
            par3.Text = I18N.GetString("pres_par3");
            par4.Text = I18N.GetString("pres_par4");
            wikiHyperLink.Text = I18N.GetString("wiki_hyperlink");
            verboseLoggingText.Text = I18N.GetString("verbose_logging");
            logExplaination.Text = I18N.GetString("log_explaination");
            githubIssueButton.Content = I18N.GetString("report_issue");
            accessBetaText.Text = I18N.GetString("access_beta");
            cmdDetailsTab.Header = I18N.GetString("tab_cmd_details_header");
            eddi2Text.Text = I18N.GetString("tab_cmd_par1");
            eddi3Text.Text = I18N.GetString("tab_cmd_par2");
            eddiHomeSystemLabel.Content = I18N.GetString("tab_cmd_system");
            eddiHomeStationLabel.Content = I18N.GetString("tab_cmd_station");
            eddiInsuranceLabel.Content = I18N.GetString("tab_cmd_insurance");
            frontierAPITab.Header = I18N.GetString("tab_frontier_header");
            frontierAPITabDesc.Text = I18N.GetString("tab_frontier_desc");
            companionAppResetText.Text = I18N.GetString("tab_frontier_reset_desc");
            companionAppResetButton.Content = I18N.GetString("next");
            companionAppText.Text = I18N.GetString("frontier_api_no_logins");
            companionAppEmailLabel.Content = I18N.GetString("label_email");
            companionAppPasswordLabel.Content = I18N.GetString("label_password");
            companionAppNextButton.Content = I18N.GetString("next");
            companionAppCodeLabel.Content = I18N.GetString("label_code");
            ttsTab.Header = I18N.GetString("tab_tts_header");
            ttsText.Text = I18N.GetString("tab_tts_desc");
            ttsVoiceLabel.Content = I18N.GetString("tab_tts_voice_label");
            ttsVolumeLabel.Content = I18N.GetString("tab_tts_volume_label");
            ttsRateLabel.Content = I18N.GetString("tab_tts_rate_label");
            ttsEffectsLevelLabel.Content = I18N.GetString("tab_tts_level_label");
            ttsDistortLabel.Content = I18N.GetString("tab_tts_distort_label");
            ttsTestShipLabel.Content = I18N.GetString("tab_tts_test_ship_label");
            ttsTestButton.Content = I18N.GetString("tab_tts_test_button");
            ttsTestDamagedButton.Content = I18N.GetString("tab_tts_test_damaged_button");
            ttsPhoneticSpeechDesc.Text = I18N.GetString("tab_tts_phonetic_speech_desc");
            disableSsmltLabel.Content = I18N.GetString("tts_tab_disable_phonetic_speech_label");
            SSmlnote.Content = I18N.GetString("tts_tab_disable_phonetic_speech_note");
            ICAODesc.Text = I18N.GetString("tts_tab_icao_desc");
            enableIcaoLabel.Content = I18N.GetString("tts_tab_icao_label");
            ttsTestDesc.Text = I18N.GetString("tts_tab_test_desc");
            chooseLanguageText.Text = I18N.GetString("choose_lang_label");
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
                statusText.Text = I18N.GetStringWithArgs("update_message", new string[] { EDDI.Instance.UpgradeVersion });
                // Do not show upgrade button if EDDI is started from VA
                upgradeButton.Visibility = EDDI.FromVA ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                upgradeButton.Visibility = Visibility.Collapsed;
                if (CompanionAppService.Instance.CurrentState != CompanionAppService.State.READY)
                {
                    statusText.Text = I18N.GetString("frontier_api_nok");
                }
                else
                {
                    statusText.Text = I18N.GetString("operational");
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
                            setUpCompanionAppComplete(I18N.GetString("frontier_api_temp_nok"));
                        }
                        else
                        {
                            setUpCompanionAppComplete(I18N.GetStringWithArgs("frontier_api_ok", new string[] { profile.Cmdr.name }));
                            //setShipyardFromConfiguration();
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
                    companionAppText.Text = I18N.GetString("login_nok_frontier_service");
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
                        setUpCompanionAppComplete(I18N.GetStringWithArgs("frontier_api_ok", new string[] { profile.Cmdr.name }));
                        //setShipyardFromConfiguration();
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
                    setUpCompanionAppStage1(I18N.GetString("login_nok_frontier_service"));
                }
            }
        }

        private void setUpCompanionAppStage1(string message = null)
        {
            if (message == null)
            {
                companionAppText.Text = I18N.GetString("frontier_api_no_logins");
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
            companionAppNextButton.Content = I18N.GetString("next");
        }

        private void setUpCompanionAppStage2(string message = null)
        {
            if (message == null)
            {
                companionAppText.Text = I18N.GetString("frontier_api_verification_code");
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
            companionAppNextButton.Content = I18N.GetString("next");
        }

        private void setUpCompanionAppComplete(string message = null)
        {
            if (message == null)
            {
                companionAppText.Text = I18N.GetString("complete");
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
            companionAppNextButton.Content = I18N.GetString("logout");
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
            SpeechService.Instance.Say(testShip, I18N.GetStringWithArgs("voice_test_ship", new string[] { ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedValue).SpokenModel()}), false);
        }

        private void ttsTestDamagedVoiceButtonClicked(object sender, RoutedEventArgs e)
        {
            Ship testShip = ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedValue);
            testShip.health = 20;
            SpeechService.Instance.Say(testShip, I18N.GetStringWithArgs("voice_test_damage", new string[] { ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedValue).SpokenModel() }), false);
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

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (!fromVA)
            {
                SpeechService.Instance.ShutUp();
                EDDI.Instance.Stop();
                Application.Current.Shutdown();
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
                var progress = new Progress<string>(s => githubIssueButton.Content = I18N.GetString("preparing_log") + s);
                
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

                progress.Report(I18N.GetString("done"));
            }
            catch (Exception ex)
            {

                progress.Report(I18N.GetString("failed"));
                Logging.Error("Failed to upload log", ex);

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
    }
}

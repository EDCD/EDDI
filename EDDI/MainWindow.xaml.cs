using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiSpeechService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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
                heroText.Text = "Any changes made here will take effect automatically in VoiceAttack.  You can close this window when you have finished.";
            }
            else
            {
                heroText.Text = "If you are using VoiceAttack then please close this window before you start VoiceAttack for your changes to take effect.  You can access this window from VoiceAttack with the \"Configure EDDI\" command.";
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
                    setUpCompanionAppComplete("Your connection to the Frontier API is good but experiencing temporary issues.  Your information should be available soon");
                }
                else
                {
                    setUpCompanionAppComplete("Your connection to the Frontier API is operational, Commander " + profile.Cmdr.name);
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

                    TabItem item = new TabItem { Header = monitor.MonitorName() };
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

                TabItem item = new TabItem { Header = responder.ResponderName() };
                item.Content = skeleton;
                tabControl.Items.Add(item);
            }

            EDDI.Instance.Start();
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
                statusText.Text = "Version " + EDDI.Instance.UpgradeVersion + " is available";
                // Do not show upgrade button if EDDI is started from VA
                upgradeButton.Visibility = EDDI.FromVA ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                upgradeButton.Visibility = Visibility.Collapsed;
                if (CompanionAppService.Instance.CurrentState != CompanionAppService.State.READY)
                {
                    statusText.Text = "Frontier API connection not operational";
                }
                else
                {
                    statusText.Text = "Operational";
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
                            setUpCompanionAppComplete("Your connection to the Frontier API is good but experiencing temporary issues.  Your information should be available soon");
                        }
                        else
                        {
                            setUpCompanionAppComplete("Your connection to the Frontier API is operational, Commander " + profile.Cmdr.name);
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
                    companionAppText.Text = "Unable to log in.  This is usually a temporary issue with Frontier's service; please try again later";
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
                        setUpCompanionAppComplete("Your connection to the Frontier API is operational, Commander " + profile.Cmdr.name);
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
                    setUpCompanionAppStage1("Unable to log in.  This is usually a temporary issue with Frontier's service; please try again later");
                }
            }
        }

        private void setUpCompanionAppStage1(string message = null)
        {
            if (message == null)
            {
                companionAppText.Text = "You do not have a connection to the Frontier API at this time.  Please enter your Elite: Dangerous email address and password below";
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
            companionAppNextButton.Content = "Next";
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
            companionAppNextButton.Content = "Next";
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
            companionAppNextButton.Content = "Log out";
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
            SpeechService.Instance.Say(testShip, "This is how I will sound in your " + ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedValue).SpokenModel() + ".", false);
        }

        private void ttsTestDamagedVoiceButtonClicked(object sender, RoutedEventArgs e)
        {
            Ship testShip = ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedValue);
            testShip.health = 20;
            SpeechService.Instance.Say(testShip, "Severe damage to your " + ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedValue).SpokenModel() + ".", false);
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
                var progress = new Progress<string>(s => githubIssueButton.Content = "Preparing log..." + s);
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

                progress.Report("done");
            }
            catch (Exception ex)
            {
                progress.Report("failed");
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
    }
}

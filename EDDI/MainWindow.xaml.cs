using EddiCompanionAppService;
using EddiDataDefinitions;
using EddiDataProviderService;
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
            int designedHeight = (int)MinHeight;
            int designedWidth = (int)MinWidth;

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
            if (eddiConfiguration.validHomeSystem)
            {
                ConfigureHomeStationOptions(eddiConfiguration.HomeSystem);
                eddiHomeSystemText.Text = eddiConfiguration.HomeSystem;
            }
            else
            {
                eddiHomeSystemText.Text = string.Empty;
            }
            homeStationDropDown.SelectedItem = eddiConfiguration.HomeStation == null ? "None" : eddiConfiguration.HomeStation;
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
            eddiSquadronNameText.Text = eddiConfiguration.SquadronName == null ? string.Empty : eddiConfiguration.SquadronName;
            eddiSquadronIDText.Text = eddiConfiguration.SquadronID == null ? string.Empty : eddiConfiguration.SquadronID;
            squadronRankDropDown.SelectedItem = eddiConfiguration.SquadronRank == null
                ? SquadronRank.FromEDName("None").localizedName : eddiConfiguration.SquadronRank.localizedName;
            ConfigureSquadronRankOptions(eddiConfiguration);
            squadronAllegianceDropDown.SelectedItem = eddiConfiguration.SquadronAllegiance == null
                ? Superpower.FromEDName("None").localizedName : eddiConfiguration.SquadronAllegiance.localizedName;
            ConfigureSquadronAllegianceOptions(eddiConfiguration);
            squadronPowerDropDown.SelectedItem = eddiConfiguration.SquadronPower == null
                ? Power.FromEDName("None").localizedName : eddiConfiguration.SquadronPower.localizedName;
            ConfigureSquadronPowerOptions(eddiConfiguration);
            if (eddiConfiguration.validSquadronSystem)
            {
                ConfigureSquadronFactionOptions(eddiConfiguration);
                eddiSquadronSystemText.Text = eddiConfiguration.SquadronSystem;
            }
            else
            {
                eddiSquadronSystemText.Text = string.Empty;
            }
            squadronFactionDropDown.SelectedItem = eddiConfiguration.SquadronFaction == null ? "None" : eddiConfiguration.SquadronFaction;

            List<LanguageDef> langs = GetAvailableLangs();
            chooseLanguageDropDown.ItemsSource = langs;
            chooseLanguageDropDown.DisplayMemberPath = "displayName";
            chooseLanguageDropDown.SelectedItem = langs.Find(l => l.ci.Name == Eddi.Properties.Settings.Default.OverrideCulture);
            chooseLanguageDropDown.SelectionChanged += (sender, e) =>
            {
                LanguageDef cultureDef = (LanguageDef)chooseLanguageDropDown.SelectedItem;
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

            LoadAndSortTabs(eddiConfiguration);

            RestoreWindowState();
            EDDI.Instance.Start();
        }

        class TabItemComparer : Comparer<TabItem>
        {
            public StringComparer stringComparer { get; }

            public TabItemComparer(StringComparer stringComparer)
            {
                this.stringComparer = stringComparer;
            }

            public override int Compare(TabItem x, TabItem y)
            {
                return stringComparer.Compare(x.Header, y.Header);
            }
        }

        private void LoadAndSortTabs(EDDIConfiguration eddiConfiguration)
        {
            // Sort all but the first TabItem by name, ignoring case
            ItemCollection items = tabControl.Items;
            List<TabItem> sortedItems = new List<TabItem>();
            foreach (var item in items)
            {
                sortedItems.Add(item as TabItem);
            }

            List<TabItem> monitors = LoadMonitors(eddiConfiguration);
            sortedItems.AddRange(monitors);
            List<TabItem> responders = LoadResponders(eddiConfiguration);
            sortedItems.AddRange(responders);

            TabItemComparer comparer = new TabItemComparer(StringComparer.CurrentCultureIgnoreCase);
            sortedItems.Sort(1, sortedItems.Count - 1, comparer);

            items.Clear();
            foreach (var item in sortedItems)
            {
                items.Add(item);
            }
        }

        private List<LanguageDef> GetAvailableLangs()
        {
            List<LanguageDef> cultures = new List<LanguageDef>
            {

                // Add the "Automatic" culture, we are using the InvariantCulture name "" to mean user's culture
                new LanguageDef(CultureInfo.InvariantCulture, Properties.EddiResources.system_language)
            };

            CultureInfo neutralInfo = new CultureInfo("en"); // Add our "neutral" language "en".
            cultures.Add(new LanguageDef(neutralInfo));

            // Add our satellite resource language folders to the list. Since these are stored according to folder name, we can interate through folder names to identify supported resources
            List<LanguageDef> satelliteCultures = new List<LanguageDef>();
            DirectoryInfo rootInfo = new DirectoryInfo(new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName);
            DirectoryInfo[] subDirs = rootInfo.GetDirectories();
            foreach (DirectoryInfo dir in subDirs)
            {
                string name = dir.Name;
                if (name == "x86" || name == "x64")
                {
                    continue;
                }
                try
                {
                    CultureInfo cInfo = new CultureInfo(name);
                    satelliteCultures.Add(new LanguageDef(cInfo));
                }
                catch
                {}
            }
            satelliteCultures.Sort();
            cultures.AddRange(satelliteCultures);

            return cultures;
        }

        private List<TabItem> LoadMonitors(EDDIConfiguration eddiConfiguration)
        {
            List<TabItem> result = new List<TabItem>();
            foreach (EDDIMonitor monitor in EDDI.Instance.monitors)
            {
                Logging.Debug("Adding configuration tab for " + monitor.MonitorName());

                System.Windows.Controls.UserControl monitorConfiguration = monitor.ConfigurationTabItem();
                // Only show a tab if this can be turned off or has configuration elements
                if (monitorConfiguration != null || !monitor.IsRequired())
                {
                    PluginSkeleton skeleton = new PluginSkeleton(monitor.MonitorName());
                    skeleton.plugindescription.Text = monitor.MonitorDescription();

                    if (eddiConfiguration.Plugins.TryGetValue(monitor.MonitorName(), out bool enabled))
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
                    result.Add(item);
                }
            }
            return result;
        }

        private List<TabItem> LoadResponders(EDDIConfiguration eddiConfiguration)
        {
            List<TabItem> result = new List<TabItem>();
            foreach (EDDIResponder responder in EDDI.Instance.responders)
            {
                Logging.Debug("Adding configuration tab for " + responder.ResponderName());

                PluginSkeleton skeleton = new PluginSkeleton(responder.ResponderName());
                skeleton.plugindescription.Text = responder.ResponderDescription();

                if (eddiConfiguration.Plugins.TryGetValue(responder.ResponderName(), out bool enabled))
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
                result.Add(item);
            }
            return result;
        }

        private void ConfigureTTS()
        {
            SpeechServiceConfiguration speechServiceConfiguration = SpeechServiceConfiguration.FromFile();
            List<string> speechOptions = new List<string>
            {
                "Windows TTS default"
            };
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
                ttsVoiceDropDown.Text = speechServiceConfiguration.StandardVoice ?? "Windows TTS default";
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
            if (eddiConfiguration.HomeSystem != eddiHomeSystemText.Text)
            {
                eddiConfiguration.HomeSystem = string.IsNullOrWhiteSpace(eddiHomeSystemText.Text) ? null : eddiHomeSystemText.Text.Trim();
                eddiConfiguration = EDDI.Instance.updateHomeSystem(eddiConfiguration);
                if (eddiConfiguration.HomeStation != null)
                {
                    eddiConfiguration.HomeStation = null;
                    homeStationDropDown.SelectedItem = "None";
                    ConfigureHomeStationOptions(null);
                }
                eddiConfiguration.ToFile();

                // Update the UI for invalid results
                runValidation(eddiHomeSystemText);
            }
        }

        private void eddiHomeSystemText_LostFocus(object sender, RoutedEventArgs e)
        {
            // Discard invalid results
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            if (eddiConfiguration.validHomeSystem)
            {
                ConfigureHomeStationOptions(eddiConfiguration.HomeSystem);
            }
            else
            {
                eddiConfiguration.HomeSystem = null;
                eddiHomeSystemText.Text = string.Empty;
                eddiConfiguration.ToFile();
            }
        }

        private void ConfigureHomeStationOptions(string system)
        {
            List<string> HomeStationOptions = new List<string>
                {
                    "None"
                };

            if (system != null)
            {
                StarSystem HomeSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(system, true);
                if (HomeSystem.stations != null)
                {
                    foreach (Station station in HomeSystem.stations)
                    {
                        HomeStationOptions.Add(station.name);
                    }
                }
            }
            homeStationDropDown.ItemsSource = HomeStationOptions;
        }

        private void homeStationDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            string homeStationName = homeStationDropDown.SelectedItem.ToString();
            if (eddiConfiguration.HomeStation != homeStationName)
            {
                eddiConfiguration.HomeStation = homeStationName == "None" ? null : homeStationName;
                eddiConfiguration = EDDI.Instance.updateHomeStation(eddiConfiguration);
                eddiConfiguration.ToFile();
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

        private void squadronNameChanged(object sender, TextChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            if (eddiConfiguration.SquadronName != eddiSquadronNameText.Text)
            {
                eddiConfiguration.SquadronName = string.IsNullOrWhiteSpace(eddiSquadronNameText.Text) ? null : eddiSquadronNameText.Text.Trim();
                if (eddiConfiguration.SquadronName == null)
                {
                    eddiConfiguration.SquadronID = null;
                    eddiSquadronIDText.Text = string.Empty;
                }
                eddiConfiguration = resetSquadronRank(eddiConfiguration);
                eddiConfiguration.ToFile();

                EDDI.Instance.Cmdr.squadronname = eddiConfiguration.SquadronName;
            }
        }

        private void eddiSquadronNameText_LostFocus(object sender, RoutedEventArgs e)
        {
            // Discard invalid results
            if (eddiSquadronNameText.Text == string.Empty)
            {
                EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
                eddiConfiguration.SquadronName = null;
                eddiConfiguration.ToFile();
                EDDI.Instance.Cmdr.squadronname = string.Empty;
            }
        }

        private void squadronIDChanged(object sender, TextChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            if (eddiConfiguration.SquadronID != eddiSquadronIDText.Text)
            {
                eddiConfiguration.SquadronID = string.IsNullOrWhiteSpace(eddiSquadronIDText.Text) ? null : eddiSquadronIDText.Text.Trim();
                eddiConfiguration.ToFile();

                EDDI.Instance.Cmdr.squadronid = eddiConfiguration.SquadronID;
            }
        }

        private void eddiSquadronIDText_LostFocus(object sender, RoutedEventArgs e)
        {
            // Discard invalid results
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            if (eddiConfiguration.SquadronID != null)
            {
                if (eddiConfiguration.SquadronID.Contains(" ") || eddiConfiguration.SquadronID.Length > 4)
                {
                    eddiConfiguration.SquadronID = null;
                    eddiSquadronSystemText.Text = string.Empty;
                    eddiConfiguration.ToFile();
                }
            }
        }

        private void squadronRankDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            string squadronRank = squadronRankDropDown.SelectedItem.ToString();

            if (eddiConfiguration.SquadronRank.edname != squadronRank)
            {
                eddiConfiguration.SquadronRank = SquadronRank.FromName(squadronRank);
                eddiConfiguration.ToFile();

                EDDI.Instance.Cmdr.squadronrank = eddiConfiguration.SquadronRank;
            }
        }

        private void squadronAllegianceDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            string squadronAllegiance = squadronAllegianceDropDown.SelectedItem?.ToString();

            if (eddiConfiguration.SquadronAllegiance.localizedName != squadronAllegiance)
            {
                eddiConfiguration.SquadronAllegiance = Superpower.FromName(squadronAllegiance);
                eddiConfiguration.SquadronPower = Power.None;
                eddiConfiguration = resetSquadronPower(eddiConfiguration);
                eddiConfiguration = EDDI.Instance.updateSquadronSystem(eddiConfiguration);
                eddiConfiguration.ToFile();

                EDDI.Instance.Cmdr.squadronallegiance = eddiConfiguration.SquadronAllegiance;
                EDDI.Instance.Cmdr.squadronpower = eddiConfiguration.SquadronPower;
                EDDI.Instance.Cmdr.squadronfaction = eddiConfiguration.SquadronFaction;
            }
        }

        private void squadronPowerDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            string squadronPower = squadronPowerDropDown.SelectedItem?.ToString();

            if (eddiConfiguration.SquadronPower.localizedName != squadronPower)
            {
                eddiConfiguration.SquadronPower = Power.FromName(squadronPower);
                eddiConfiguration = resetSquadronPower(eddiConfiguration);
                eddiConfiguration.ToFile();

                EDDI.Instance.Cmdr.squadronpower = eddiConfiguration.SquadronPower;
            }
        }

        private void squadronSystemChanged(object sender, TextChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            if (eddiConfiguration.SquadronSystem != eddiSquadronSystemText.Text)
            {
                eddiConfiguration.SquadronSystem = string.IsNullOrWhiteSpace(eddiSquadronSystemText.Text) ? null : eddiSquadronSystemText.Text.Trim();
                eddiConfiguration = EDDI.Instance.updateSquadronSystem(eddiConfiguration);
                if (eddiConfiguration.SquadronFaction != null)
                {
                    eddiConfiguration.SquadronFaction = null;
                    squadronFactionDropDown.SelectedItem = "None";
                    ConfigureSquadronFactionOptions(eddiConfiguration);
                }
                eddiConfiguration.ToFile();

                // Update the UI for invalid results
                runValidation(eddiSquadronSystemText);
            }
        }

        private void eddiSquadronSystemText_LostFocus(object sender, RoutedEventArgs e)
        {
            // Discard invalid results
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            if (eddiConfiguration.validSquadronSystem)
            {
                ConfigureSquadronFactionOptions(eddiConfiguration);
            }
            else
            {
                eddiConfiguration.SquadronSystem = null;
                eddiSquadronSystemText.Text = string.Empty;
                eddiConfiguration.ToFile();
            }
        }

        private void squadronFactionDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();
            string squadronFaction = squadronFactionDropDown.SelectedItem?.ToString();

            if (eddiConfiguration.SquadronFaction != squadronFaction)
            {
                eddiConfiguration.SquadronFaction = squadronFaction;
                EDDI.Instance.Cmdr.squadronfaction = eddiConfiguration.SquadronFaction;
                eddiConfiguration.ToFile();
            }
        }

        private void ConfigureSquadronRankOptions(EDDIConfiguration configuration)
        {
            List<string> SquadronRankOptions = new List<string>();

            foreach (SquadronRank squadronrank in SquadronRank.AllOfThem)
            {
                if (configuration.SquadronName == null && squadronrank != SquadronRank.None)
                {
                    break;
                }
                SquadronRankOptions.Add(squadronrank.localizedName);
            }
            squadronRankDropDown.ItemsSource = SquadronRankOptions;
        }

        private void ConfigureSquadronAllegianceOptions(EDDIConfiguration configuration)
        {
            List<string> SquadronAllegianceOptions = new List<string>();

            SquadronAllegianceOptions.Add(Superpower.None.localizedName);
            if (configuration.SquadronName != null)
            {
                SquadronAllegianceOptions.Add(Superpower.Alliance.localizedName);
                SquadronAllegianceOptions.Add(Superpower.Empire.localizedName);
                SquadronAllegianceOptions.Add(Superpower.Federation.localizedName);
                SquadronAllegianceOptions.Add(Superpower.Independent.localizedName);
            }
            squadronAllegianceDropDown.ItemsSource = SquadronAllegianceOptions;
        }

        private void ConfigureSquadronPowerOptions(EDDIConfiguration configuration)
        {
            List<string> SquadronPowerOptions = new List<string>();

            SquadronPowerOptions.Add(Power.None.localizedName);
            if (configuration.SquadronAllegiance != Superpower.None)
            {
                foreach (Power power in Power.AllOfThem)
                {
                    if (configuration.SquadronAllegiance == power.allegiance)
                    {
                        SquadronPowerOptions.Add(power.localizedName);
                    }
                }
            }
            squadronPowerDropDown.ItemsSource = SquadronPowerOptions;
        }

        private void ConfigureSquadronFactionOptions(EDDIConfiguration configuration)
        {
            List<string> SquadronFactionOptions = new List<string>
                {
                    "None"
                };

            if (configuration.SquadronSystem != null)
            {
                StarSystem FactionSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(configuration.SquadronSystem, true);
                if (FactionSystem.factions != null)
                {
                    foreach (Faction faction in FactionSystem.factions)
                    {
                        if (faction.Allegiance == configuration.SquadronAllegiance)
                        {
                            SquadronFactionOptions.Add(faction.name);
                        }
                    }
                }
            }
            squadronFactionDropDown.ItemsSource = SquadronFactionOptions;
        }

        public EDDIConfiguration resetSquadronRank(EDDIConfiguration configuration)
        {
            if (configuration.SquadronName == null)
            {
                configuration.SquadronRank = SquadronRank.None;
                squadronRankDropDown.SelectedItem = configuration.SquadronRank.localizedName;
            }
            ConfigureSquadronRankOptions(configuration);
            configuration = resetSquadronAllegiance(configuration);

            return configuration;
        }

        private EDDIConfiguration resetSquadronAllegiance(EDDIConfiguration configuration)
        {
            if (configuration.SquadronName == null)
            {
                configuration.SquadronAllegiance = Superpower.None;
                squadronAllegianceDropDown.SelectedItem = configuration.SquadronAllegiance.localizedName;
            }
            ConfigureSquadronAllegianceOptions(configuration);
            configuration.SquadronPower = Power.None;
            configuration = resetSquadronPower(configuration);

            return configuration;
        }

        private EDDIConfiguration resetSquadronPower(EDDIConfiguration configuration)
        {
            squadronPowerDropDown.SelectedItem = configuration.SquadronPower.localizedName;
            configuration.SquadronSystem = null;
            eddiSquadronSystemText.Text = string.Empty;
            configuration = EDDI.Instance.updateSquadronSystem(configuration);
            runValidation(eddiSquadronSystemText);

            ConfigureSquadronPowerOptions(configuration);
            configuration = resetSquadronFaction(configuration);

            return configuration;
        }

        private EDDIConfiguration resetSquadronFaction(EDDIConfiguration configuration)
        {
            if (configuration.SquadronSystem == null)
            {
                configuration.SquadronFaction = null;
                squadronFactionDropDown.SelectedItem = "None";
            }
            ConfigureSquadronFactionOptions(configuration);

            return configuration;
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
            versionText.Text = Constants.EDDI_VERSION.ToString();
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
            Ship testShip = ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedItem);
            testShip.health = 100;
            string message = String.Format(Properties.EddiResources.voice_test_ship, ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedItem).SpokenModel());
            SpeechService.Instance.Say(testShip, message, false);
        }

        private void ttsTestDamagedVoiceButtonClicked(object sender, RoutedEventArgs e)
        {
            Ship testShip = ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedItem);
            testShip.health = 20;
            string message = String.Format(Properties.EddiResources.voice_test_damage, ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedItem).SpokenModel());
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
            SpeechServiceConfiguration speechConfiguration = new SpeechServiceConfiguration
            {
                StandardVoice = ttsVoiceDropDown.SelectedItem == null || ttsVoiceDropDown.SelectedItem.ToString() == "Windows TTS default" ? null : ttsVoiceDropDown.SelectedItem.ToString(),
                Volume = (int)ttsVolumeSlider.Value,
                Rate = (int)ttsRateSlider.Value,
                EffectsLevel = (int)ttsEffectsLevelSlider.Value,
                DistortOnDamage = ttsDistortCheckbox.IsChecked.Value,
                DisableSsml = disableSsmlCheckbox.IsChecked.Value,
                EnableIcao = enableIcaoCheckbox.IsChecked.Value
            };
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
            {
                WindowState = WindowState.Normal;
            }
            else if (!minimizeCheck && WindowState != state)
            {
                WindowState = state;
            }
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

                // Create a temporary issue log file, delete any remnants from prior issue reporting
                lock (logLock)
                {
                    Directory.CreateDirectory(issueLogDir);
                    File.WriteAllText(issueLogFile, File.ReadAllText(Constants.DATA_DIR + @"\eddi.log"));
                }

                // Truncate log files more than the specified size MB in size
                const long maxLogSizeBytes = 0x200000; // 2 MB
                FileInfo logFile = new FileInfo(issueLogFile);
                if (logFile.Length > maxLogSizeBytes) 
                {
                    using (MemoryStream ms = new MemoryStream((int)maxLogSizeBytes))
                    {
                        using (FileStream issueLog = new FileStream(issueLogFile, FileMode.Open, FileAccess.ReadWrite))
                        {
                            issueLog.Seek(-1 * maxLogSizeBytes, SeekOrigin.End);
                            // advance to after next line end
                            int c = 0;
                            while ( (c = issueLog.ReadByte() ) != -1)
                            {
                                if (c == '\n')
                                {
                                    break;
                                }
                                c++;
                            }
                            issueLog.CopyTo(ms);
                            issueLog.SetLength(maxLogSizeBytes);
                            issueLog.Position = 0;
                            ms.Position = 0; // Begin from the start of the memory stream
                            ms.CopyTo(issueLog);
                        }
                    }
                }

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
    }

    public class ValidHomeSystemRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();

            if (value == null)
            {
                return ValidationResult.ValidResult;
            }
            if (eddiConfiguration.validHomeSystem)
            {
                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, Properties.EddiResources.invalid_system);
            }
        }
    }

    public class ValidSquadronSystemRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();

            if (value == null)
            {
                return ValidationResult.ValidResult;
            }
            if (eddiConfiguration.validSquadronSystem)
            {
                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, Properties.EddiResources.invalid_system);
            }
        }
    }

    public class ValidSquadronIDRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            EDDIConfiguration eddiConfiguration = EDDIConfiguration.FromFile();

            if (value == null)
            {
                return ValidationResult.ValidResult;
            }
            if (eddiConfiguration.validSquadronSystem)
            {
                return ValidationResult.ValidResult;
            }
            else
            {
                return new ValidationResult(false, Properties.EddiResources.invalid_system);
            }
        }
    }
}

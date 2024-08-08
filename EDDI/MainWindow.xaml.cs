using EddiCompanionAppService;
using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiCore.Upgrader;
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
using System.Linq;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Utilities;
using Application = System.Windows.Application;

namespace Eddi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        struct LanguageDef : IComparable<LanguageDef>
        {
            public CultureInfo ci;
            public string displayName { get; set; }

            public LanguageDef(CultureInfo ci)
            {
                this.ci = ci;
                this.displayName = $"{ci.NativeName} ({ci.DisplayName})";
            }

            public LanguageDef(CultureInfo ci, string displayName)
            {
                this.ci = ci;
                this.displayName = displayName;
            }

            public int CompareTo(LanguageDef rhs)
            {
                return string.Compare(displayName, rhs.displayName, StringComparison.Ordinal);
            }
        }

        private void SaveWindowState()
        {
            Rect savePosition;
            var eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            switch (WindowState)
            {
                case WindowState.Maximized:
                    savePosition = new Rect(RestoreBounds.Left, RestoreBounds.Top, RestoreBounds.Width, RestoreBounds.Height);
                    eddiConfiguration.Maximized = true;
                    eddiConfiguration.Minimized = false;
                    break;
                case WindowState.Minimized:
                    savePosition = new Rect(RestoreBounds.Left, RestoreBounds.Top, RestoreBounds.Width, RestoreBounds.Height);
                    eddiConfiguration.Maximized = false;

                    // If opened from VoiceAttack, don't allow minimized state
                    eddiConfiguration.Minimized = !App.FromVA;

                    break;
                default:
                    savePosition = new Rect(Left, Top, Width, Height);
                    eddiConfiguration.Maximized = false;
                    eddiConfiguration.Minimized = false;
                    break;
            }

            eddiConfiguration.MainWindowPosition = savePosition;

            // Remember which tab we have selected in EDDI
            eddiConfiguration.SelectedTab = tabControl.SelectedIndex;

            ConfigService.Instance.eddiConfiguration = eddiConfiguration;
        }

        private void RestoreWindowState()
        {
            int designedHeight = (int)MinHeight;
            int designedWidth = (int)MinWidth;

            var eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            Rect windowPosition = eddiConfiguration.MainWindowPosition;
            Visibility = Visibility.Collapsed;

            // WPF uses DPI scaled units rather than true pixels.
            // Retrieve the DPI scaling for the controlling monitor (where the top left pixel is located).
            var dpiScale = VisualTreeHelper.GetDpi(this); 
            if (windowPosition != Rect.Empty && isWindowValid(windowPosition, dpiScale))
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
                Left = centerWindow(applyDpiScale(Screen.PrimaryScreen.Bounds.Width, dpiScale.DpiScaleX), designedWidth);
                Top = centerWindow(applyDpiScale(Screen.PrimaryScreen.Bounds.Height, dpiScale.DpiScaleY), designedHeight);
                Width = Math.Min(Screen.PrimaryScreen.Bounds.Width / dpiScale.DpiScaleX, designedWidth);
                Height = Math.Min(Screen.PrimaryScreen.Bounds.Height / dpiScale.DpiScaleY, designedHeight);
            }

            tabControl.SelectedIndex = eddiConfiguration.SelectedTab;

            // Check detected monitors to see if the saved window size and location is valid
            bool isWindowValid(Rect rect, DpiScale dpi)
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
                    if (rect.X >= applyDpiScale(screen.Bounds.X, dpi.DpiScaleX) && rect.Y >= applyDpiScale(screen.Bounds.Y, dpi.DpiScaleY)) // The upper and left bounds fall on a valid screen
                    {
                        testUpperLeft = true;
                    }
                    if (applyDpiScale(screen.Bounds.Width, dpi.DpiScaleX) >= rect.X + rect.Width && applyDpiScale(screen.Bounds.Height, dpi.DpiScaleY) >= rect.Y + rect.Height) // The lower and right bounds fall on a valid screen 
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

            int applyDpiScale(int originalValue, double dpiScaleFactor)
            {
                return (int)Math.Round(originalValue / dpiScaleFactor);
            }
        }

        private bool runBetaCheck = false;

        public MainWindow()
        {
            if (!App.FromVA)
            {
                SplashScreen splashScreen = new SplashScreen("logo-with-alpha.png");
                splashScreen.Show(true);
            }

            InitializeComponent();
            DataContext = EDDI.Instance;

            // Start the EDDI instance
            EDDI.Instance.Start();

            // Configure the EDDI tab
            setStatusInfo();

            // Need to set up the correct information in the hero text depending on from where we were started
            if (App.FromVA)
            {
                // Allow the EDDI VA plugin to change window state
                VaWindowStateChange += OnVaWindowStateChange;
                heroText.Text = Properties.EddiResources.change_affect_va;
                chooseLanguageText.Text = Properties.MainWindow.choose_lang_label_va;
            }
            else
            {
                heroText.Text = Properties.EddiResources.if_using_va;
                chooseLanguageText.Text = Properties.MainWindow.choose_lang_label;
            }

            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;

            // Setup home system & station from config file
            homeSystemDropDown.ItemsSource = new List<string> { eddiConfiguration.HomeSystem ?? string.Empty };
            homeSystemDropDown.SelectedItem = eddiConfiguration.HomeSystem ?? string.Empty;
            ConfigureHomeStationOptions(eddiConfiguration.HomeSystem);
            homeStationDropDown.SelectedItem = eddiConfiguration.HomeStation ?? Properties.MainWindow.no_station;

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
            eddiCommanderPhoneticNameText.Text = eddiConfiguration.PhoneticName ?? string.Empty;
            eddiSquadronNameText.Text = eddiConfiguration.SquadronName ?? string.Empty;
            eddiSquadronIDText.Text = eddiConfiguration.SquadronID ?? string.Empty;
            squadronRankDropDown.SelectedItem = (eddiConfiguration.SquadronRank ?? SquadronRank.None).localizedName;
            ConfigureSquadronRankOptions(eddiConfiguration);

            // Setup squadron home system from config file
            squadronSystemDropDown.ItemsSource = new List<string> { eddiConfiguration.SquadronSystem ?? string.Empty };
            squadronSystemDropDown.SelectedItem = eddiConfiguration.SquadronSystem ?? string.Empty;

            squadronFactionDropDown.SelectedItem = eddiConfiguration.SquadronFaction ?? Power.None.localizedName;
            squadronPowerDropDown.SelectedItem = (eddiConfiguration.SquadronPower ?? Power.None).localizedName;
            ConfigureSquadronPowerOptions(eddiConfiguration);

            List<LanguageDef> langs = GetAvailableLangs(); // already correctly sorted
            chooseLanguageDropDown.ItemsSource = langs;
            chooseLanguageDropDown.DisplayMemberPath = "displayName";
            chooseLanguageDropDown.SelectedItem = string.IsNullOrEmpty(eddiConfiguration.OverrideCulture) 
                ? langs.Find(l => Equals(l.ci, CultureInfo.InvariantCulture))
                : langs.Find(l => l.ci.Name == eddiConfiguration.OverrideCulture);
            chooseLanguageDropDown.SelectionChanged += (sender, e) =>
            {
                LanguageDef cultureDef = (LanguageDef)chooseLanguageDropDown.SelectedItem;
                eddiConfiguration.OverrideCulture = cultureDef.ci.Name;
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;
            };

            // Configure the Frontier API tab
            CompanionAppCredentials companionAppCredentials = CompanionAppCredentials.Load();
            CompanionAppService.Instance.StateChanged += companionApiStatusChanged;

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
                return stringComparer.Compare(x?.Header, y?.Header);
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
            var cultures = new List<LanguageDef>
            {

                // Add the "Automatic" culture, we are using the InvariantCulture name "" to mean user's culture
                new LanguageDef(CultureInfo.InvariantCulture, Properties.EddiResources.system_language)
            };

            var neutralInfo = new CultureInfo("en"); // Add our "neutral" language "en".
            cultures.Add(new LanguageDef(neutralInfo));

            // Add our satellite resource language folders to the list. Since these are stored according to folder name, we can interate through folder names to identify supported resources
            var satelliteCultures = new List<LanguageDef>();
            var fileInfo = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName;
            if ( fileInfo == null ) { return cultures; }
            var rootInfo = new DirectoryInfo(fileInfo);
            var subDirs = rootInfo.GetDirectories();
            foreach ( var dir in subDirs )
            {
                string name = dir.Name;
                if ( name == "x86" || 
                     name == "x64" || 
                     !dir.GetFiles().Any( f => f.Extension.Equals( ".dll" ) )
                )
                {
                    continue;
                }

                try
                {
                    var cInfo = new CultureInfo(name);
                    satelliteCultures.Add( new LanguageDef( cInfo ) );
                }
                catch
                {
                    // Ignore any exceptions here
                }
            }
            satelliteCultures.Sort();
            cultures.AddRange( satelliteCultures );

            return cultures;
        }

        private List<TabItem> LoadMonitors(EDDIConfiguration eddiConfiguration)
        {
            var result = new List<TabItem>();
            foreach (var monitor in EDDI.Instance.monitors)
            {
                Logging.Debug("Adding configuration tab for " + monitor.MonitorName());

                var monitorConfiguration = monitor.ConfigurationTabItem();
                // Only show a tab if this can be turned off or has configuration elements
                if (monitorConfiguration != null || !monitor.IsRequired())
                {
                    PluginSkeleton skeleton = new PluginSkeleton(monitor.MonitorName(), !monitor.IsRequired());
                    skeleton.plugindescription.Text = monitor.MonitorDescription();

                    if (eddiConfiguration.Plugins.TryGetValue(monitor.MonitorName(), out bool enabled))
                    {
                        skeleton.pluginenabled.IsChecked = enabled;
                    }
                    else
                    {
                        // Default to enabled
                        skeleton.pluginenabled.IsChecked = true;
                        ConfigService.Instance.eddiConfiguration = eddiConfiguration;
                    }

                    // Add monitor-specific configuration items
                    if (monitorConfiguration != null)
                    {
                        skeleton.panel.Children.Add(monitorConfiguration);
                    }

                    var item = new TabItem { Header = monitor.LocalizedMonitorName(), Content = skeleton };
                    result.Add(item);
                }
            }
            return result;
        }

        private List<TabItem> LoadResponders(EDDIConfiguration eddiConfiguration)
        {
            var result = new List<TabItem>();
            foreach (IEddiResponder responder in EDDI.Instance.responders)
            {
                Logging.Debug("Adding configuration tab for " + responder.ResponderName());

                var skeleton = new PluginSkeleton(responder.ResponderName());
                skeleton.plugindescription.Text = responder.ResponderDescription();

                if (eddiConfiguration.Plugins.TryGetValue(responder.ResponderName(), out bool enabled))
                {
                    skeleton.pluginenabled.IsChecked = enabled;
                }
                else
                {
                    // Default to enabled
                    skeleton.pluginenabled.IsChecked = true;
                    ConfigService.Instance.eddiConfiguration = eddiConfiguration;
                }

                // Add responder-specific configuration items
                System.Windows.Controls.UserControl monitorConfiguration = responder.ConfigurationTabItem();
                if (monitorConfiguration != null)
                {
                    monitorConfiguration.Margin = new Thickness(10);
                    skeleton.panel.Children.Add(monitorConfiguration);
                }

                var item = new TabItem { Header = responder.LocalizedResponderName(), Content = skeleton };
                result.Add(item);
            }
            return result;
        }

        public void ConfigureTTS()
        {
            var speechServiceConfiguration = SpeechServiceConfiguration.FromFile();
            var speechOptions = new List<string>
            {
                "Windows TTS default"
            };
            try
            {
                foreach (var voice in SpeechService.Instance.allvoices)
                {
                    speechOptions.Add(voice);
                }
                ttsVoiceDropDown.ItemsSource = speechOptions;
                ttsVoiceDropDown.Text =  speechOptions.Any(v => v == speechServiceConfiguration.StandardVoice) 
                    ? speechServiceConfiguration.StandardVoice
                    : "Windows TTS default";

                // If the prior selected voice is no longer a valid option, we revert to the system default.
                if (speechServiceConfiguration.StandardVoice != ttsVoiceDropDown.Text)
                {
                    speechServiceConfiguration.ToFile();
                }
            }
            catch (Exception e)
            {
                Logging.Warn("" + Thread.CurrentThread.ManagedThreadId + ": Caught exception " + e);
            }
            ttsVolumeSlider.Value = speechServiceConfiguration.Volume;
            ttsRateSlider.Value = speechServiceConfiguration.Rate;
            ttsEffectsLevelSlider.Value = speechServiceConfiguration.EffectsLevel;
            ttsDistortCheckbox.IsChecked = speechServiceConfiguration.DistortOnDamage;
            DisableIpaCheckbox.IsChecked = speechServiceConfiguration.DisableIpa;
            enableIcaoCheckbox.IsChecked = speechServiceConfiguration.EnableIcao;

            ttsTestShipDropDown.ItemsSource = ShipDefinitions.ShipModels; // already sorted
            ttsTestShipDropDown.Text = "Adder";
        }

        // Hook the window Loaded event to set minimize/maximize state at startup 
        private void windowLoaded(object sender, RoutedEventArgs e)
        {
            var eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            if (sender is Window senderWindow && (eddiConfiguration.Maximized || eddiConfiguration.Minimized))
            {
                if (eddiConfiguration.Minimized && !App.FromVA)
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

        // Handle changes to the editable home system combo box
        private void HomeSystemText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is StarSystemComboBox starSystemComboBox && !starSystemComboBox.IsLoaded) { return; }

            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            void changeHandler()
            {
                // Reset the home station due to selecting new home system
                if (eddiConfiguration.HomeStation != null)
                {
                    eddiConfiguration.HomeStation = null;
                    homeStationDropDown.SelectedItem = Properties.MainWindow.no_station;
                    ConfigureHomeStationOptions(null);
                    ConfigService.Instance.eddiConfiguration = eddiConfiguration;
                }
            }
            homeSystemDropDown.TextDidChange(sender, e, eddiConfiguration.HomeSystem, changeHandler);
        }

        private void HomeSystemDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is StarSystemComboBox starSystemComboBox && !starSystemComboBox.IsLoaded) { return; }
        
            void changeHandler(string newValue)
            {
                // Update configuration to new home system
                EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
                eddiConfiguration.HomeSystem = newValue;
                eddiConfiguration = EDDI.Instance.updateHomeSystem(eddiConfiguration);
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;

                // Update station options for new system
                ConfigureHomeStationOptions(eddiConfiguration.HomeSystem);
            }
            homeSystemDropDown.SelectionDidChange(changeHandler);
        }

        private void HomeSystemDropDown_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is StarSystemComboBox starSystemComboBox && !starSystemComboBox.IsLoaded) { return; }

            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            homeSystemDropDown.DidLoseFocus(oldValue: eddiConfiguration.HomeSystem);
        }

        private void ConfigureHomeStationOptions(string system)
        {
            List<string> HomeStationOptions = new List<string>
                {
                    Properties.MainWindow.no_station
                };

            if (system != null)
            {
                StarSystem HomeSystem = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(system, true, true, false, true, false);
                if (HomeSystem?.stations != null)
                {
                    foreach (Station station in HomeSystem.stations)
                    {
                        HomeStationOptions.Add(station.name);
                    }
                }
            }
            // sort but leave "No Station" at the top
            HomeStationOptions.Sort(1, HomeStationOptions.Count - 1, null);
            homeStationDropDown.ItemsSource = HomeStationOptions;
        }

        private void homeStationDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            string homeStationName = homeStationDropDown.SelectedItem?.ToString();
            if (eddiConfiguration.HomeStation != homeStationName)
            {
                eddiConfiguration.HomeStation = homeStationName == Properties.MainWindow.no_station ? null : homeStationName;
                eddiConfiguration = EDDI.Instance.updateHomeStation(eddiConfiguration);
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;
            }
        }

        private void isMale_Checked(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            eddiConfiguration.Gender = "Male";
            ConfigService.Instance.eddiConfiguration = eddiConfiguration;
        }

        private void isFemale_Checked(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            eddiConfiguration.Gender = "Female";
            ConfigService.Instance.eddiConfiguration = eddiConfiguration;
            if ( EDDI.Instance.Cmdr != null )
            {
                EDDI.Instance.Cmdr.gender = "Female";
            }
        }

        private void isNeitherGender_Checked(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            eddiConfiguration.Gender = "Neither";
            ConfigService.Instance.eddiConfiguration = eddiConfiguration;
            if ( EDDI.Instance.Cmdr != null )
            {
                EDDI.Instance.Cmdr.gender = "Neither";
            }
        }

        private void commanderPhoneticNameChanged(object sender, TextChangedEventArgs e)
        {
            // Replace any spaces, maintaining the original caret position
            int caretIndex = eddiCommanderPhoneticNameText.CaretIndex;
            eddiCommanderPhoneticNameText.Text = eddiCommanderPhoneticNameText.Text.Replace(" ", "ˈ");
            eddiCommanderPhoneticNameText.CaretIndex = Math.Max(caretIndex, eddiCommanderPhoneticNameText.Text.Length);

            // Update our config file
            if (eddiCommanderPhoneticNameText.IsLoaded)
            {
                EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
                if (eddiConfiguration.PhoneticName != eddiCommanderPhoneticNameText.Text)
                {
                    eddiConfiguration.PhoneticName = string.IsNullOrWhiteSpace(eddiCommanderPhoneticNameText.Text) ? string.Empty : eddiCommanderPhoneticNameText.Text.Trim();
                    ConfigService.Instance.eddiConfiguration = eddiConfiguration;
                }
            }
        }

        private void eddiCmdrPhoneticNameTestButtonClicked(object sender, RoutedEventArgs e)
        {
            SpeechService.Instance.Say(null, EDDI.Instance.Cmdr?.SpokenName(), 0);
        }

        private void ipaClicked(object sender, RoutedEventArgs e)
        {
            IpaResourcesWindow IpaResources = new IpaResourcesWindow();
            IpaResources.Show();
        }

        private void squadronNameChanged(object sender, TextChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            if (eddiConfiguration.SquadronName != eddiSquadronNameText.Text)
            {
                eddiConfiguration.SquadronName = string.IsNullOrWhiteSpace(eddiSquadronNameText.Text) ? null : eddiSquadronNameText.Text.Trim();
                if (eddiConfiguration.SquadronName == null)
                {
                    eddiConfiguration.SquadronID = null;
                    eddiSquadronIDText.Text = string.Empty;

                    squadronSystemDropDown.Text = string.Empty;
                }
                eddiConfiguration = resetSquadronRank(eddiConfiguration);
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;

                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronname = eddiConfiguration.SquadronName;
                }
            }
        }

        private void eddiCommanderPhoneticNameText_LostFocus(object sender, RoutedEventArgs e)
        {
            // Discard invalid results
            if (eddiCommanderPhoneticNameText.Text == string.Empty)
            {
                EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
                eddiConfiguration.PhoneticName = null;
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;
                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.phoneticName = string.Empty;
                }
            }
        }

        private void CommanderDetailsTab_GotFocus(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            if (eddiConfiguration.SquadronName != eddiSquadronNameText.Text)
            {
                eddiSquadronNameText.Text = eddiConfiguration.SquadronName;
            }
            if (eddiConfiguration.SquadronID != eddiSquadronIDText.Text)
            {
                eddiSquadronIDText.Text = eddiConfiguration.SquadronID;
            }
        }

        private void eddiSquadronNameText_LostFocus(object sender, RoutedEventArgs e)
        {
            // Discard invalid results
            if (eddiSquadronNameText.Text == string.Empty)
            {
                EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
                eddiConfiguration.SquadronName = null;
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;
                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronname = string.Empty;
                }
            }
        }

        private void squadronIDChanged(object sender, TextChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            if (eddiConfiguration.SquadronID != eddiSquadronIDText.Text)
            {
                eddiConfiguration.SquadronID = string.IsNullOrWhiteSpace(eddiSquadronIDText.Text) ? null : eddiSquadronIDText.Text.Trim();
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;

                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronid = eddiConfiguration.SquadronID;
                }
            }
        }

        private void eddiSquadronIDText_LostFocus(object sender, RoutedEventArgs e)
        {
            // Discard invalid results
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            if (eddiConfiguration.SquadronID != null)
            {
                if (eddiConfiguration.SquadronID.Contains(" ") || eddiConfiguration.SquadronID.Length > 4)
                {
                    eddiConfiguration.SquadronID = null;
                    squadronSystemDropDown.Text = string.Empty;
                    ConfigService.Instance.eddiConfiguration = eddiConfiguration;
                }
            }
        }

        private void squadronRankDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            string squadronRank = squadronRankDropDown.SelectedItem.ToString();

            if (eddiConfiguration.SquadronRank.edname != squadronRank)
            {
                eddiConfiguration.SquadronRank = SquadronRank.FromName(squadronRank);
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;

                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronrank = eddiConfiguration.SquadronRank;
                }
            }
        }

        // Handle changes to the editable squadron system combo box
        private void SquadronSystemText_TextChanged(object sender, TextChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            string oldValue = eddiConfiguration.SquadronSystem;
            void changeHandler()
            {
                // Reset the squadron data due to selecting new squadron system
                if (eddiConfiguration.SquadronFaction != null)
                {
                    eddiConfiguration.SquadronFaction = null;

                    eddiConfiguration.SquadronAllegiance = Superpower.None;
                    eddiConfiguration.SquadronPower = Power.None;
                    ConfigService.Instance.eddiConfiguration = eddiConfiguration;

                    squadronFactionDropDown.SelectedItem = Power.None.localizedName;
                    ConfigureSquadronFactionOptions(eddiConfiguration);
                    squadronPowerDropDown.SelectedItem = eddiConfiguration.SquadronPower.localizedName;
                    ConfigureSquadronPowerOptions(eddiConfiguration);

                    if ( EDDI.Instance.Cmdr != null )
                    {
                        EDDI.Instance.Cmdr.squadronallegiance = Superpower.None;
                        EDDI.Instance.Cmdr.squadronpower = Power.None;
                    }

                    ConfigService.Instance.eddiConfiguration = eddiConfiguration;
                }
            }
            squadronSystemDropDown.TextDidChange(sender, e, oldValue, changeHandler);
        }

        private void SquadronSystemDropDown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            void changeHandler(string newValue)
            {
                // Update configuration to new squadron system
                EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
                eddiConfiguration.SquadronSystem = newValue;
                eddiConfiguration = EDDI.Instance.updateSquadronSystem(eddiConfiguration);
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;

                // Update squadron faction options for new system
                ConfigureSquadronFactionOptions(eddiConfiguration);
            }
            squadronSystemDropDown.SelectionDidChange(changeHandler);
        }

        private void SquadronSystemDropDown_LostFocus(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            squadronSystemDropDown.DidLoseFocus(oldValue: eddiConfiguration.SquadronSystem);
        }

        private void squadronFactionDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            string squadronFaction = squadronFactionDropDown.SelectedItem?.ToString(); // This can be a localized "None"

            if (eddiConfiguration.SquadronFaction != squadronFaction)
            {
                eddiConfiguration.SquadronFaction = squadronFaction == Power.None.localizedName ? null : squadronFaction;

                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronfaction = eddiConfiguration.SquadronFaction;
                }
                
                if (squadronFaction != Power.None.localizedName)
                {
                    StarSystem system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(eddiConfiguration.SquadronSystem, true);
                    Faction faction = system?.factions.Find(f => f.name == squadronFaction);

                    if (faction != null && eddiConfiguration.SquadronAllegiance != faction.Allegiance)
                    {
                        eddiConfiguration.SquadronAllegiance = faction.Allegiance;
                        ConfigService.Instance.eddiConfiguration = eddiConfiguration;

                        if ( EDDI.Instance.Cmdr != null )
                        {
                            EDDI.Instance.Cmdr.squadronallegiance = faction.Allegiance;
                        }

                        squadronPowerDropDown.SelectedItem = Power.None.localizedName;
                        ConfigureSquadronPowerOptions(eddiConfiguration);
                    }
                }
                else
                {
                    eddiConfiguration.SquadronAllegiance = Superpower.None;
                    ConfigService.Instance.eddiConfiguration = eddiConfiguration;

                    if ( EDDI.Instance.Cmdr != null )
                    {
                        EDDI.Instance.Cmdr.squadronallegiance = Superpower.None;
                    }

                    squadronPowerDropDown.SelectedItem = Power.None.localizedName;
                    ConfigureSquadronPowerOptions(eddiConfiguration);
                }
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;
            }
        }

        private void squadronPowerDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            string squadronPower = squadronPowerDropDown.SelectedItem?.ToString();

            if ((eddiConfiguration.SquadronPower?.localizedName ?? "") != squadronPower)
            {
                eddiConfiguration.SquadronPower = Power.FromName(squadronPower);
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;

                if ( EDDI.Instance.Cmdr != null )
                {
                    EDDI.Instance.Cmdr.squadronpower = eddiConfiguration.SquadronPower;
                }
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
            // Don't sort
            squadronRankDropDown.ItemsSource = SquadronRankOptions;
        }

        public void ConfigureSquadronFactionOptions(EDDIConfiguration configuration)
        {
            List<string> SquadronFactionOptions = new List<string>
            {
                Power.None.localizedName
            };

            if (configuration.SquadronSystem != null)
            {
                var system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(configuration.SquadronSystem, true, true, false, false, true );
                if (system?.factions != null)
                {
                    foreach (Faction faction in system.factions)
                    {
                        SquadronFactionOptions.Add(faction.name);
                    }
                }
            }
            // sort but leave "None" at the top
            SquadronFactionOptions.Sort(1, SquadronFactionOptions.Count - 1, null);
            squadronFactionDropDown.ItemsSource = SquadronFactionOptions;
        }

        public void ConfigureSquadronPowerOptions(EDDIConfiguration configuration)
        {
            List<string> SquadronPowerOptions = new List<string>
            {
                Power.None.localizedName
            };
            if (configuration.SquadronAllegiance != Superpower.None)
            {
                foreach (Power power in Power.AllOfThem)
                {
                    if (configuration.SquadronAllegiance == power.Allegiance)
                    {
                        SquadronPowerOptions.Add(power.localizedName);
                    }
                }
            }
            // sort but leave "None" at the top
            SquadronPowerOptions.Sort(1, SquadronPowerOptions.Count - 1, null);
            squadronPowerDropDown.ItemsSource = SquadronPowerOptions;
        }

        public EDDIConfiguration resetSquadronRank(EDDIConfiguration configuration)
        {
            if (configuration.SquadronName == null)
            {
                configuration.SquadronRank = SquadronRank.None;
                squadronRankDropDown.SelectedItem = configuration.SquadronRank.localizedName;
            }
            ConfigureSquadronRankOptions(configuration);

            return configuration;
        }

        private void verboseLoggingEnabled(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            eddiConfiguration.Debug = eddiVerboseLogging.IsChecked ?? false;
            Logging.Verbose = eddiConfiguration.Debug;
            ConfigService.Instance.eddiConfiguration = eddiConfiguration;
        }

        private void verboseLoggingDisabled(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            eddiConfiguration.Debug = eddiVerboseLogging.IsChecked ?? false;
            Logging.Verbose = eddiConfiguration.Debug;
            ConfigService.Instance.eddiConfiguration = eddiConfiguration;
        }

        private void betaProgrammeEnabled(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            eddiConfiguration.Beta = eddiBetaProgramme.IsChecked ?? false;
            ConfigService.Instance.eddiConfiguration = eddiConfiguration;
            if (runBetaCheck)
            {
                // Because we have changed to wanting beta upgrades we need to re-check upgrade information
                EddiUpgrader.CheckUpgrade();
                setStatusInfo();
            }
            else
            {
                runBetaCheck = true;
            }
        }

        private void betaProgrammeDisabled(object sender, RoutedEventArgs e)
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            eddiConfiguration.Beta = eddiBetaProgramme.IsChecked ?? false;
            ConfigService.Instance.eddiConfiguration = eddiConfiguration;
            if (runBetaCheck)
            {
                // Because we have changed to not wanting beta upgrades we need to re-check upgrade information
                EddiUpgrader.CheckUpgrade();
                setStatusInfo();
            }
            else
            {
                runBetaCheck = true;
            }
        }

        private void companionApiStatusChanged(CompanionAppService.State oldState, CompanionAppService.State newState)
        {
            // The calling thread for this method may not have direct access to the MainWindow dispatcher so we invoke the dispatcher here.
            Application.Current?.Dispatcher?.InvokeAsync( () =>
            {
                MainWindow mainwindow = (MainWindow)Application.Current?.MainWindow;
                mainwindow?.Dispatcher?.InvokeAsync( setStatusInfo);
            });

            if (oldState == CompanionAppService.State.AwaitingCallback &&
                newState == CompanionAppService.State.Authorized)
            {
                SpeechService.Instance.Say(null, string.Format(Properties.EddiResources.frontier_api_ok, EDDI.Instance.Cmdr?.phoneticname), 0);
                SpeechService.Instance.Say(null, Properties.EddiResources.frontier_api_close_browser, 0);
            }
            else if (oldState == CompanionAppService.State.LoggedOut &&
                newState == CompanionAppService.State.AwaitingCallback)
            {
                SpeechService.Instance.Say(null, Properties.EddiResources.frontier_api_please_authenticate, 0);
            }
        }

        // Set the fields relating to status information
        private void setStatusInfo()
        {
            versionText.Text = Constants.EDDI_VERSION.ToString();
            Title = "EDDI v." + Constants.EDDI_VERSION;

            if (EddiUpgrader.UpgradeVersion != null)
            {
                statusText.Text = String.Format(Properties.EddiResources.update_message, EddiUpgrader.UpgradeVersion);
                // Do not show upgrade button if EDDI is started from VA
                upgradeButton.Visibility = App.FromVA ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                upgradeButton.Visibility = Visibility.Collapsed;
                var capiState = CompanionAppService.Instance.CurrentState;
                if (!EDDI.running)
                {
                    statusText.Text = Properties.EddiResources.safe_mode;
                }
                else if (capiState == CompanionAppService.State.NoClientIDConfigured)
                {
                    statusText.Text = Properties.EddiResources.frontier_api_not_enabled;
                }
                else if (capiState != CompanionAppService.State.Authorized)
                {
                    statusText.Text = Properties.EddiResources.frontier_api_nok;
                }
                else
                {
                    statusText.Text = Properties.EddiResources.operational;
                }
            }

            switch (CompanionAppService.Instance.CurrentState)
            {
                case CompanionAppService.State.LoggedOut:
                case CompanionAppService.State.ConnectionLost:
                    companionAppStatusValue.Text = Properties.EddiResources.frontierApiNotConnected;
                    companionAppButton.Content = Properties.EddiResources.login;
                    companionAppButton.IsEnabled = !App.FromVA;
                    companionAppText.Text = !App.FromVA ? "" : Properties.EddiResources.frontier_api_cant_login_from_va;
                    break;
                case CompanionAppService.State.AwaitingCallback:
                    companionAppStatusValue.Text = Properties.EddiResources.frontierApiConnecting;
                    companionAppButton.Content = Properties.MainWindow.reset_button;
                    companionAppButton.IsEnabled = true;
                    companionAppText.Text = Properties.EddiResources.frontier_api_please_authenticate;
                    break;
                case CompanionAppService.State.Authorized:
                case CompanionAppService.State.TokenRefresh:
                    companionAppStatusValue.Text = Properties.EddiResources.frontierApiConnected;
                    companionAppButton.Content = Properties.MainWindow.reset_button;
                    companionAppButton.IsEnabled = true;
                    companionAppText.Text = Properties.MainWindow.tab_frontier_reset_desc;
                    break;
                case CompanionAppService.State.NoClientIDConfigured:
                    companionAppStatusValue.Text = Properties.EddiResources.frontierApiNotEnabled;
                    companionAppButton.Content = Properties.EddiResources.login;
                    companionAppButton.IsEnabled = false;
                    companionAppText.Text = Properties.MainWindow.tab_frontier_not_enabled_desc;
                    break;
            }
        }

        // Handle changes to the Frontier API tab
        private void companionAppClicked(object sender, RoutedEventArgs e)
        {
            if (CompanionAppService.Instance.CurrentState == CompanionAppService.State.LoggedOut)
            {
                if (IsAdministrator())
                {
                    SpeechService.Instance.Say(null, Properties.EddiResources.frontier_api_admin_mode, 0);
                }
                CompanionAppService.Instance.Login();
            }
            else
            {
                // Logout from the companion EddiApplication and start again
                CompanionAppService.Instance.Logout();
                SpeechService.Instance.Say(null, Properties.EddiResources.frontier_api_reset, 0);
                if (App.FromVA)
                {
                    SpeechService.Instance.Say(null, Properties.EddiResources.frontier_api_cant_login_from_va, 0);
                }
            }
        }

        private static bool IsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        // Handle Text-to-speech tab

        private void ttsVoiceDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.IsLoaded)
            {
                ttsUpdated();
            }
        }

        private void ttsEffectsLevelUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.IsLoaded)
            {
                ttsUpdated();
            }
        }

        private void ttsDistortionLevelUpdated(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.IsLoaded)
            {
                ttsUpdated();
            }
        }

        private void ttsRateUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.IsLoaded)
            {
                ttsUpdated();
            }
        }

        private void ttsVolumeUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.IsLoaded)
            {
                ttsUpdated();
            }
        }

        private void ttsTestVoiceButtonClicked(object sender, RoutedEventArgs e)
        {
            Ship testShip = ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedItem);
            testShip.health = 100;
            string message = String.Format(Properties.EddiResources.voice_test_ship, ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedItem).SpokenModel());
            SpeechService.Instance.Say(testShip, message, 0);
        }

        private void ttsTestDamagedVoiceButtonClicked(object sender, RoutedEventArgs e)
        {
            Ship testShip = ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedItem);
            testShip.health = 20;
            string message = String.Format(Properties.EddiResources.voice_test_damage, ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedItem).SpokenModel());
            SpeechService.Instance.Say(testShip, message, 0);
        }

        private void disableIpaUpdated(object sender, RoutedEventArgs e)
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
                StandardVoice = ttsVoiceDropDown.SelectedItem == null || 
                                ttsVoiceDropDown.SelectedItem.ToString() == "Windows TTS default" 
                    ? null 
                    : ttsVoiceDropDown.SelectedItem.ToString(),
                Volume = (int)ttsVolumeSlider.Value,
                Rate = (int)ttsRateSlider.Value,
                EffectsLevel = (int)ttsEffectsLevelSlider.Value,
                DistortOnDamage = ttsDistortCheckbox.IsChecked ?? false,
                DisableIpa = DisableIpaCheckbox.IsChecked ?? false,
                EnableIcao = enableIcaoCheckbox.IsChecked ?? false
            };
            SpeechService.Instance.Configuration = speechConfiguration;
            speechConfiguration.ToFile();
        }

        // Called from the VoiceAttack plugin if the "Configure EDDI" voice command has
        // been given and the EDDI configuration window is already open. If the window
        // is minimized, restore it, otherwise the plugin will ignore the command.
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

            // Save window position here as the RestoreBounds rect gets set
            // to empty somewhere between here and OnClosed.
            System.Windows.Application.Current?.Dispatcher?.Invoke(SaveWindowState);

            if (App.FromVA)
            {
                e.Cancel = true;
                Hide();
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
            // Write out useful information to the log before proceeding
            Logging.Info("EDDI version: " + Constants.EDDI_VERSION);
            if (App.FromVA)
            {
                Logging.Info("VoiceAttack version: " + EDDI.Instance.vaVersion);
            }
            Logging.Info("Commander name: " + (EDDI.Instance.Cmdr != null ? EDDI.Instance.Cmdr.name : "null"));
            Logging.Info("Default UI culture: " + (CultureInfo.DefaultThreadCurrentUICulture?.IetfLanguageTag ?? "automatic"));
            Logging.Info("Current UI culture: " + (CultureInfo.CurrentUICulture?.IetfLanguageTag));

            // Prepare a truncated log file for export if verbose logging is enabled
            if (eddiVerboseLogging.IsChecked ?? false)
            {
                Logging.Debug("Preparing log for export.");
                var progress = new Progress<string>(s => githubIssueButton.Content = Properties.EddiResources.preparing_log + s);
                await Task.Factory.StartNew(() => prepareLogs(progress), TaskCreationOptions.LongRunning);
            }

            createGithubIssue();
        }

        private void ChangeLog_Click(object sender, RoutedEventArgs e)
        {
            ChangeLogWindow changeLog = new ChangeLogWindow();
            changeLog.Show();
        }

        public static void prepareLogs(IProgress<string> progress)
        {
            try
            {
                string issueLogDir = Constants.DATA_DIR + @"\logexport\";
                Directory.CreateDirectory(issueLogDir);

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\eddi_issue.zip";

                // Create temporary, truncated issue log files
                prepareLog(issueLogDir + "eddi.log", Constants.DATA_DIR + @"\eddi.log");
                prepareLog(issueLogDir + "eddi1.log", Constants.DATA_DIR + @"\eddi1.log");

                // Zip our log files and send them to the desktop so that it can be added to the Github issue
                File.Delete(desktopPath);
                ZipFile.CreateFromDirectory(issueLogDir, desktopPath);

                // Clear temporary issue log files & the temporary directory
                foreach (string filePath in Directory.GetFiles(issueLogDir))
                {
                    File.Delete(filePath);
                }
                Directory.Delete(issueLogDir);

                progress.Report(Properties.EddiResources.done);
            }
            catch (Exception ex)
            {
                progress.Report(Properties.EddiResources.failed);
                Logging.Error("Failed to prepare log", ex);

            }
        }

        private static void prepareLog(string toTruncatedLogPath, string fromLogPath)
        {
            if (File.Exists(fromLogPath))
            {
                Files.Write(toTruncatedLogPath, Files.Read(fromLogPath));

                // Truncate log files more than the specified size MB in size
                const long maxLogSizeBytes = 5 * 1024 * 1024; // 5 MB (before zipping)
                FileInfo logFile = new FileInfo(toTruncatedLogPath);
                if (logFile.Length > maxLogSizeBytes)
                {
                    using (MemoryStream ms = new MemoryStream((int)maxLogSizeBytes))
                    {
                        using (FileStream issueLog = new FileStream(toTruncatedLogPath, FileMode.Open, FileAccess.ReadWrite))
                        {
                            issueLog.Seek(-1 * maxLogSizeBytes, SeekOrigin.End);
                            // advance to after next line end
                            int c = 0;
                            while ((c = issueLog.ReadByte()) != -1)
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
            }
        }

        private void createGithubIssue()
        {
            Process.Start("https://github.com/EDCD/EDDI/issues/new");
        }

        private void upgradeClicked(object sender, RoutedEventArgs e)
        {
            EddiUpgrader.Upgrade();
        }

        private void EDDIClicked(object sender, RoutedEventArgs e)
        {
            if (EDDI.Instance.EddiIsBeta())
            {
                Process.Start("https://github.com/EDCD/EDDI/blob/develop/README.md");
            }
            else
            {
                Process.Start("https://github.com/EDCD/EDDI/blob/stable/README.md");
            }
        }

        private void WikiClicked(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/EDCD/EDDI/wiki");
        }

        private void TroubleshootClicked(object sender, RoutedEventArgs e)
        {
            if (EDDI.Instance.EddiIsBeta())
            {
                Process.Start("https://github.com/EDCD/EDDI/blob/develop/TROUBLESHOOTING.md");
            }
            else
            {
                Process.Start("https://github.com/EDCD/EDDI/blob/stable/TROUBLESHOOTING.md");
            }
        }
    }
}

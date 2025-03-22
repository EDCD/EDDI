using EddiCompanionAppService;
using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiCore.Upgrader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Utilities;

namespace EddiUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public System.Windows.Controls.TabControl MainTabControl => tabControl;

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
                    eddiConfiguration.Minimized = !EDDI.FromVA;

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
            if (!EDDI.FromVA)
            {
                var splashScreen = new SplashScreen("logo-with-alpha.png");
                splashScreen.Show(true);
            }

            InitializeComponent();
            DataContext = EDDI.Instance;

            // Start the EDDI instance
            EDDI.Instance.Start();

            versionText.Text = Constants.EDDI_VERSION.ToString();
            Title = "EDDI v." + Constants.EDDI_VERSION;
            setStatusInfo();
            CompanionAppService.Instance.StateChanged += ( s, e ) => setStatusInfo();

            // Configure the EDDI tab
            // Need to set up the correct information in the hero text depending on from where we were started
            if ( EDDI.FromVA)
            {
                // Allow the EDDI VA plugin to change window state
                VaWindowStateChange += OnVaWindowStateChange;
                heroText.Text = Properties.Resources.change_affect_va;
                chooseLanguageText.Text = Properties.Resources.choose_lang_label_va;
            }
            else
            {
                heroText.Text = Properties.Resources.if_using_va;
                chooseLanguageText.Text = Properties.Resources.choose_lang_label;
            }

            var eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            eddiVerboseLogging.IsChecked = eddiConfiguration.Debug;
            eddiBetaProgramme.IsChecked = eddiConfiguration.Beta;
            var langs = GetAvailableLangs(); // already correctly sorted
            chooseLanguageDropDown.ItemsSource = langs;
            chooseLanguageDropDown.DisplayMemberPath = "displayName";
            chooseLanguageDropDown.SelectedItem = string.IsNullOrEmpty(eddiConfiguration.OverrideCulture) 
                ? langs.Find(l => Equals(l.ci, CultureInfo.InvariantCulture))
                : langs.Find(l => l.ci.Name == eddiConfiguration.OverrideCulture);
            chooseLanguageDropDown.SelectionChanged += (sender, e) =>
            {
                var cultureDef = (LanguageDef)chooseLanguageDropDown.SelectedItem;
                eddiConfiguration.OverrideCulture = cultureDef.ci.Name;
                ConfigService.Instance.eddiConfiguration = eddiConfiguration;
            };

            LoadAndSortTabs(eddiConfiguration);
            RestoreWindowState();
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
                new LanguageDef(CultureInfo.InvariantCulture, Properties.Resources.system_language)
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

        // Hook the window Loaded event to set minimize/maximize state at startup 
        private void windowLoaded(object sender, RoutedEventArgs e)
        {
            var eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            if (sender is Window senderWindow && (eddiConfiguration.Maximized || eddiConfiguration.Minimized))
            {
                if (eddiConfiguration.Minimized && !EDDI.FromVA)
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

        private void betaProgrammeDisabled ( object sender, RoutedEventArgs e )
        {
            EDDIConfiguration eddiConfiguration = ConfigService.Instance.eddiConfiguration;
            eddiConfiguration.Beta = eddiBetaProgramme.IsChecked ?? false;
            ConfigService.Instance.eddiConfiguration = eddiConfiguration;
            if ( runBetaCheck )
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

        // Set the fields relating to status information
        private void setStatusInfo()
        {
            if (EddiUpgrader.UpgradeVersion != null)
            {
                statusText.Text = string.Format(Properties.Resources.update_message, EddiUpgrader.UpgradeVersion);
                // Do not show upgrade button if EDDI is started from VA
                upgradeButton.Visibility = EDDI.FromVA ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                upgradeButton.Visibility = Visibility.Collapsed;
                var capiState = CompanionAppService.Instance.CurrentState;
                if (!EDDI.running)
                {
                    statusText.Text = Properties.Resources.safe_mode;
                }
                else if (capiState == CompanionAppService.State.NoClientIDConfigured)
                {
                    statusText.Text = Properties.Resources.frontier_api_not_enabled;
                }
                else if (capiState != CompanionAppService.State.Authorized)
                {
                    statusText.Text = Properties.Resources.frontier_api_nok;
                }
                else
                {
                    statusText.Text = Properties.Resources.operational;
                }
            }
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

            if (EDDI.FromVA)
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
            if (EDDI.FromVA)
            {
                Logging.Info("VoiceAttack version: " + EDDI.Instance.vaVersion);
            }

            Logging.Info( $"Commander name: {ConfigService.Instance.commanderConfiguration.commanderName ?? "null"}" );
            Logging.Info( $"Default UI culture: {CultureInfo.DefaultThreadCurrentUICulture?.IetfLanguageTag ?? "automatic"}" );
            Logging.Info( $"Current UI culture: {CultureInfo.CurrentUICulture.IetfLanguageTag}" );

            // Prepare a truncated log file for export if verbose logging is enabled
            if (eddiVerboseLogging.IsChecked ?? false)
            {
                Logging.Debug("Preparing log for export.");
                var progress = new Progress<string>(s => githubIssueButton.Content = Properties.Resources.preparing_log + s);
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

                progress.Report(Properties.Resources.done);
            }
            catch (Exception ex)
            {
                progress.Report(Properties.Resources.failed);
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
            Process.Start( "https://github.com/EDCD/EDDI/issues/new?template=bug_report.md" );
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

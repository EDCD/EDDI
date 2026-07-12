using Microsoft.Win32;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace EddiUI.Themes
{
    public static partial class ThemeManager
    {
        private static bool isInitialized = false;
        private const string ClassicTheme = "Classic";
        private const string LightThemeDictionaryName = "ThemeLight.xaml";
        private const string DarkThemeDictionaryName = "ThemeDark.xaml";
        private const string ClassicThemeDictionaryName = "ThemeClassic.xaml";
        private const string ModernThemeDictionaryName = "ThemeModern.xaml";
        private const int DwmwaUseImmersiveDarkMode = 20;
        private const int DwmwaUseImmersiveDarkModeBefore20H1 = 19;
        private const int DwmwaBorderColor = 34;
        private const int DwmwaCaptionColor = 35;
        private const int DwmwaTextColor = 36;
        private const int DwmColorDefault = -1;

        public static void Initialize()
        {
            if (isInitialized) return;

            EventManager.RegisterClassHandler(typeof(Window), FrameworkElement.LoadedEvent, new RoutedEventHandler(OnWindowLoaded));

            // Apply theme on startup
            ApplyTheme();

            // Listen to Windows theme and color preference shifts at runtime
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;

            isInitialized = true;
        }

        private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (ShouldReapplyThemeForUserPreferenceChange(e.Category))
            {
                Application.Current?.Dispatcher?.InvokeAsync(() =>
                {
                    ApplyTheme();
                });
            }
        }

        private static void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Window window)
            {
                ApplyWindowFrame(window);
            }
        }

        public static void ApplyTheme()
        {
            var currentResources = Application.Current?.Resources;
            if (currentResources == null) return;

            var overrideTheme = EddiConfigService.ConfigService.Instance.eddiConfiguration.OverrideTheme;
            var themeDictionaryName = ResolveThemeDictionaryName(overrideTheme, IsWindowsDarkTheme());

            var themeUri = new Uri($"pack://application:,,,/EddiUI;component/Themes/{themeDictionaryName}", UriKind.Absolute);
            var newTheme = new ResourceDictionary { Source = themeUri };

            RemoveExistingThemeDictionaries(currentResources);

            var existingModern = currentResources.MergedDictionaries.FirstOrDefault(IsModernThemeDictionary);
            if (existingModern != null)
            {
                int index = currentResources.MergedDictionaries.IndexOf(existingModern);
                currentResources.MergedDictionaries.Insert(index, newTheme);
            }
            else
            {
                currentResources.MergedDictionaries.Add(newTheme);
            }

            existingModern = currentResources.MergedDictionaries.FirstOrDefault(IsModernThemeDictionary);
            var shouldLoadModern = ShouldLoadModernThemeDictionary(overrideTheme);

            if (shouldLoadModern)
            {
                if (existingModern == null)
                {
                    var modernUri = new Uri($"pack://application:,,,/EddiUI;component/Themes/{ModernThemeDictionaryName}", UriKind.Absolute);
                    currentResources.MergedDictionaries.Add(new ResourceDictionary { Source = modernUri });
                }
            }
            else if (existingModern != null)
            {
                currentResources.MergedDictionaries.Remove(existingModern);
            }

            if (ShouldApplyWindowsAccentColor(overrideTheme))
            {
                ApplyWindowsAccentColor(currentResources);
            }
            else
            {
                ClearWindowsAccentColor(currentResources);
            }

            ApplyWindowFrame(themeDictionaryName, currentResources);
        }

        internal static string ResolveThemeDictionaryName(string overrideTheme, bool isWindowsDarkTheme)
        {
            if (string.Equals(overrideTheme, "Light", StringComparison.OrdinalIgnoreCase))
            {
                return LightThemeDictionaryName;
            }
            if (string.Equals(overrideTheme, "Dark", StringComparison.OrdinalIgnoreCase))
            {
                return DarkThemeDictionaryName;
            }
            if (string.Equals(overrideTheme, ClassicTheme, StringComparison.OrdinalIgnoreCase))
            {
                return ClassicThemeDictionaryName;
            }

            return isWindowsDarkTheme ? DarkThemeDictionaryName : LightThemeDictionaryName;
        }

        internal static bool ShouldApplyWindowsAccentColor(string overrideTheme)
        {
            return !string.Equals(overrideTheme, ClassicTheme, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool ShouldReapplyThemeForUserPreferenceChange(UserPreferenceCategory category)
        {
            // General covers light/dark app mode; Color covers Windows accent color changes.
            return category is UserPreferenceCategory.General or UserPreferenceCategory.Color;
        }

        internal static bool ShouldUseDarkWindowFrame(string themeDictionaryName)
        {
            return string.Equals(themeDictionaryName, DarkThemeDictionaryName, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool ShouldLoadModernThemeDictionary(string overrideTheme)
        {
            return !string.Equals(overrideTheme, ClassicTheme, StringComparison.OrdinalIgnoreCase);
        }

        internal static void ApplyWindowFrame(Window window)
        {
            if (window == null)
            {
                return;
            }

            var currentResources = Application.Current?.Resources;
            if (currentResources == null)
            {
                return;
            }

            var overrideTheme = EddiConfigService.ConfigService.Instance.eddiConfiguration.OverrideTheme;
            var themeDictionaryName = ResolveThemeDictionaryName(overrideTheme, IsWindowsDarkTheme());
            ApplyWindowFrame(window, themeDictionaryName, currentResources);
        }

        internal static void RemoveExistingThemeDictionaries(ResourceDictionary resources)
        {
            if (resources == null)
            {
                return;
            }

            foreach (var dictionary in resources.MergedDictionaries.Where(IsBaseThemeDictionary).ToList())
            {
                resources.MergedDictionaries.Remove(dictionary);
            }
        }

        internal static bool IsBaseThemeDictionary(ResourceDictionary dictionary)
        {
            return IsBaseThemeDictionarySource(dictionary?.Source?.OriginalString);
        }

        internal static bool IsModernThemeDictionary(ResourceDictionary dictionary)
        {
            return IsModernThemeDictionarySource(dictionary?.Source?.OriginalString);
        }

        internal static bool IsBaseThemeDictionarySource(string source)
        {
            return source != null &&
                (source.Contains(LightThemeDictionaryName) ||
                 source.Contains(DarkThemeDictionaryName) ||
                 source.Contains(ClassicThemeDictionaryName));
        }

        internal static bool IsModernThemeDictionarySource(string source)
        {
            return source?.Contains(ModernThemeDictionaryName) == true;
        }

        private static bool IsWindowsDarkTheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("AppsUseLightTheme");
                        if (value is int useLight)
                        {
                            return useLight == 0;
                        }
                    }
                }
            }
            catch
            {
                // Fallback to light mode in case of read exceptions
            }
            return false;
        }

        private static void ApplyWindowsAccentColor(ResourceDictionary resources)
        {
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM"))
                {
                    if (key != null)
                    {
                        var value = key.GetValue("ColorizationColor");
                        if (value is int argbColor)
                        {
                            // Convert ARGB representation (from DWM registry) to standard Color
                            byte a = (byte)((argbColor >> 24) & 0xFF);
                            byte r = (byte)((argbColor >> 16) & 0xFF);
                            byte g = (byte)((argbColor >> 8) & 0xFF);
                            byte b = (byte)(argbColor & 0xFF);

                            // Always keep opacity fully solid for readability
                            var color = Color.FromArgb(255, r, g, b);
                            var brandBrush = new SolidColorBrush(color);
                            brandBrush.Freeze();
                            resources["PrimaryBrandBrush"] = brandBrush;

                            // Accent color hover variant (slightly lighter color representation)
                            var hoverColor = Color.FromArgb(220, r, g, b);
                            var accentBrush = new SolidColorBrush(hoverColor);
                            accentBrush.Freeze();
                            resources["AccentColorBrush"] = accentBrush;
                            resources["ComboBoxItemSelectedBackgroundBrush"] = accentBrush;
                            return;
                        }
                    }
                }
            }
            catch
            {
                // Suppress and fallback below
            }

            // Standard WPF Fallback using SystemParameters
            try
            {
                var color = SystemParameters.WindowGlassColor;
                var brush = new SolidColorBrush(color);
                brush.Freeze();
                resources["PrimaryBrandBrush"] = brush;
                resources["AccentColorBrush"] = brush;
                resources["ComboBoxItemSelectedBackgroundBrush"] = brush;
            }
            catch
            {
                // Hardcoded fallback standard blue
                var defaultColor = Color.FromRgb(0, 120, 212);
                var brush = new SolidColorBrush(defaultColor);
                brush.Freeze();
                resources["PrimaryBrandBrush"] = brush;
                resources["AccentColorBrush"] = brush;
                resources["ComboBoxItemSelectedBackgroundBrush"] = brush;
            }
        }

        private static void ClearWindowsAccentColor(ResourceDictionary resources)
        {
            resources.Remove("PrimaryBrandBrush");
            resources.Remove("AccentColorBrush");
        }

        private static void ApplyWindowFrame(string themeDictionaryName, ResourceDictionary resources)
        {
            var application = Application.Current;
            if (application == null) { return; }

            foreach (Window window in application.Windows)
            {
                ApplyWindowFrame(window, themeDictionaryName, resources);
            }
        }

        private static void ApplyWindowFrame(Window window, string themeDictionaryName, ResourceDictionary resources)
        {
            if (window == null || resources == null)
            {
                return;
            }

            var handle = new WindowInteropHelper(window).Handle;
            if (handle == IntPtr.Zero) { return; }

            ApplyWindowFrame(handle, themeDictionaryName, resources);
        }

        private static void ApplyWindowFrame(IntPtr handle, string themeDictionaryName, ResourceDictionary resources)
        {
            try
            {
                var useDarkFrame = ShouldUseDarkWindowFrame(themeDictionaryName);
                var useDarkFrameValue = useDarkFrame ? 1 : 0;
                _ = NativeMethods.DwmSetWindowAttribute( handle, DwmwaUseImmersiveDarkMode, ref useDarkFrameValue, sizeof(int));
                _ = NativeMethods.DwmSetWindowAttribute(handle, DwmwaUseImmersiveDarkModeBefore20H1, ref useDarkFrameValue, sizeof(int));

                if (useDarkFrame)
                {
                    var captionColor = ToColorRef(GetBrushColor(resources, "WindowBackgroundBrush", Colors.Black));
                    var textColor = ToColorRef(GetBrushColor(resources, "TextPrimaryBrush", Colors.White));
                    _ = NativeMethods.DwmSetWindowAttribute(handle, DwmwaCaptionColor, ref captionColor, sizeof(int));
                    _ = NativeMethods.DwmSetWindowAttribute(handle, DwmwaBorderColor, ref captionColor, sizeof(int));
                    _ = NativeMethods.DwmSetWindowAttribute(handle, DwmwaTextColor, ref textColor, sizeof(int));
                }
                else
                {
                    var defaultColor = DwmColorDefault;
                    _ = NativeMethods.DwmSetWindowAttribute(handle, DwmwaCaptionColor, ref defaultColor, sizeof(int));
                    _ = NativeMethods.DwmSetWindowAttribute(handle, DwmwaBorderColor, ref defaultColor, sizeof(int));
                    _ = NativeMethods.DwmSetWindowAttribute(handle, DwmwaTextColor, ref defaultColor, sizeof(int));
                }
            }
            catch
            {
                // DWM frame attributes are best-effort and unavailable on older Windows builds.
            }
        }

        private static int ToColorRef(Color color)
        {
            return color.R | (color.G << 8) | (color.B << 16);
        }

        private static Color GetBrushColor(ResourceDictionary resources, string key, Color fallback)
        {
            return resources[key] is SolidColorBrush brush ? brush.Color : fallback;
        }

        private partial class NativeMethods
        {
            [LibraryImport("dwmapi.dll", SetLastError = true)]
            internal static partial int DwmSetWindowAttribute(IntPtr hwnd, int dwAttribute, ref int pvAttribute, int cbAttribute);
        }
    }
}

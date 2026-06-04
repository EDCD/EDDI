using Microsoft.Win32;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace EddiUI.Themes
{
    public static class ThemeManager
    {
        private static bool isInitialized = false;

        public static void Initialize()
        {
            if (isInitialized) return;

            // Apply theme on startup
            ApplyTheme();

            // Listen to Windows theme and color preference shifts at runtime
            SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;

            isInitialized = true;
        }

        private static void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            // React when the user switches Light/Dark mode or changes accent colors
            if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.Color)
            {
                Application.Current?.Dispatcher?.InvokeAsync(() =>
                {
                    ApplyTheme();
                });
            }
        }

        public static void ApplyTheme()
        {
            var currentResources = Application.Current?.Resources;
            if (currentResources == null) return;

            bool isDarkTheme;
            var overrideTheme = EddiConfigService.ConfigService.Instance.eddiConfiguration.OverrideTheme;
            if (string.Equals(overrideTheme, "Light", StringComparison.OrdinalIgnoreCase))
            {
                isDarkTheme = false;
            }
            else if (string.Equals(overrideTheme, "Dark", StringComparison.OrdinalIgnoreCase))
            {
                isDarkTheme = true;
            }
            else
            {
                isDarkTheme = IsWindowsDarkTheme();
            }

            // Swap out Light/Dark resource dictionary
            var existingTheme = currentResources.MergedDictionaries.FirstOrDefault(d =>
                d.Source != null && (d.Source.OriginalString.Contains("ThemeLight.xaml") || d.Source.OriginalString.Contains("ThemeDark.xaml"))
            );

            var themeUri = isDarkTheme
                ? new Uri("pack://application:,,,/EddiUI;component/Themes/ThemeDark.xaml", UriKind.Absolute)
                : new Uri("pack://application:,,,/EddiUI;component/Themes/ThemeLight.xaml", UriKind.Absolute);

            var newTheme = new ResourceDictionary { Source = themeUri };

            if (existingTheme != null)
            {
                int index = currentResources.MergedDictionaries.IndexOf(existingTheme);
                currentResources.MergedDictionaries[index] = newTheme;
            }
            else
            {
                currentResources.MergedDictionaries.Add(newTheme);
            }

            // Ensure ThemeModern.xaml is always present in MergedDictionaries
            var existingModern = currentResources.MergedDictionaries.FirstOrDefault(d =>
                d.Source != null && d.Source.OriginalString.Contains("ThemeModern.xaml")
            );

            if (existingModern == null)
            {
                var modernUri = new Uri("pack://application:,,,/EddiUI;component/Themes/ThemeModern.xaml", UriKind.Absolute);
                currentResources.MergedDictionaries.Add(new ResourceDictionary { Source = modernUri });
            }

            // Also, dynamically inject the Windows Accent Color directly
            ApplyWindowsAccentColor(currentResources);
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
            }
            catch
            {
                // Hardcoded fallback standard blue
                var defaultColor = Color.FromRgb(0, 120, 212);
                var brush = new SolidColorBrush(defaultColor);
                brush.Freeze();
                resources["PrimaryBrandBrush"] = brush;
                resources["AccentColorBrush"] = brush;
            }
        }
    }
}

using System;
using System.Linq;
using System.Windows;

namespace Eddi
{
    public static class ThemeService
    {
        private static readonly Uri DarkThemeUri = new("/EDDI;component/Themes/DarkTheme.xaml", UriKind.Relative);

        public static void ApplyTheme()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var dictionaries = Application.Current.Resources.MergedDictionaries;
                var existing = dictionaries.FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("DarkTheme.xaml"));
                if (existing != null)
                {
                    dictionaries.Remove(existing);
                }

                try
                {
                    var rd = new ResourceDictionary() { Source = DarkThemeUri }; 
                    dictionaries.Add(rd);
                }
                catch (Exception)
                {
                    // Swallow - fail safe to existing resources
                }
            });
        }
    }
}

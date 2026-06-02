using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml;

namespace EddiSpeechResponder.AvalonEdit
{
    public class CottleHighlighting
    {
        public IHighlightingDefinition Definition;

        public CottleHighlighting( IEnumerable<string> customFunctions = null, IEnumerable<string> customProperties = null)
        {
            Register( customFunctions, customProperties );
        }

        private void Register( IEnumerable<string> customFunctions = null, IEnumerable<string> customProperties = null )
        {
            XshdSyntaxDefinition xshd = null;
            using (var s = typeof(CottleHighlighting).Assembly.GetManifestResourceStream("EddiSpeechResponder.AvalonEdit.Cottle.xshd"))
            {
                if (s != null)
                {
                    using (XmlReader reader = new XmlTextReader(s))
                    {
                        xshd = HighlightingLoader.LoadXshd(reader);
                    }
                }
            }
            AddHighlightWords( xshd, "Custom functions", customFunctions );
            AddHighlightWords( xshd, "Custom properties", customProperties );
            Definition = HighlightingLoader.Load( xshd, HighlightingManager.Instance );
            ApplyThemeColors();

            // Register our definition against the file extension
            HighlightingManager.Instance.RegisterHighlighting("Cottle", [ ".cottle" ], Definition);
        }

        private static void AddHighlightWords ( XshdSyntaxDefinition xshd, string ruleSetName, IEnumerable<string> words )
        {
            if ( words is null ) { return; }
            var ruleSet = xshd.Elements.OfType<XshdRuleSet>().First( o => string.Equals(o.Name, ruleSetName, StringComparison.InvariantCultureIgnoreCase));
            var newKeyWords = new XshdKeywords { ColorReference = new XshdReference<XshdColor>( null, ruleSetName ) };
            foreach ( var w in words )
            {
                newKeyWords.Words.Add( w );
            }
            ruleSet.Elements.Add( newKeyWords );
        }

        public void ApplyThemeColors()
        {
            bool isDarkTheme = false;
            if (Application.Current != null)
            {
                isDarkTheme = Application.Current.Resources.MergedDictionaries.Any(d => d.Source != null && d.Source.OriginalString.Contains("ThemeDark.xaml"));
            }

            if (isDarkTheme)
            {
                // Dark Theme Colors (Fluent/Antigravity style: highly readable and color-blind friendly)
                SetForegroundColor("Body text", Color.FromRgb(212, 212, 212));          // #D4D4D4 Light Gray
                SetForegroundColor("Comment", Color.FromRgb(106, 153, 85));            // #6A9955 Green
                SetForegroundColor("Code", Color.FromRgb(212, 212, 212));               // #D4D4D4 Light Gray
                SetForegroundColor("Delimiter", Color.FromRgb(212, 212, 212));          // #D4D4D4 Light Gray
                SetForegroundColor("Unexpected delimiter", Color.FromRgb(255, 255, 255)); // White
                SetBackgroundColor("Unexpected delimiter", Color.FromRgb(197, 59, 59));   // #C53B3B Red Background
                SetForegroundColor("Escape delimiter", Color.FromRgb(215, 186, 125));    // #D7BA7D Light Yellow
                SetForegroundColor("Keyword", Color.FromRgb(86, 156, 214));             // #569CD6 Vibrant Blue
                SetForegroundColor("Quote mark", Color.FromRgb(206, 145, 120));          // #CE9178 Peach/Terracotta (Strings)
                SetForegroundColor("Operator", Color.FromRgb(212, 212, 212));           // #D4D4D4 Light Gray
                SetForegroundColor("Literals", Color.FromRgb(181, 206, 168));           // #B5CEA8 Light Olive (Numbers)
                SetForegroundColor("Built-in functions", Color.FromRgb(220, 220, 170)); // #DCDCAA Gold/Light Yellow
                SetForegroundColor("Custom functions", Color.FromRgb(79, 193, 255));     // #4FC1FF Light Cyan/Blue
                SetForegroundColor("Custom properties", Color.FromRgb(156, 220, 254));   // #9CDCFE Light Blue
            }
            else
            {
                // Light Theme Colors (Clean & High Contrast)
                SetForegroundColor("Body text", Color.FromRgb(47, 79, 79));             // DarkSlateGray
                SetForegroundColor("Comment", Color.FromRgb(0, 128, 0));                // Green
                SetForegroundColor("Code", Color.FromRgb(128, 0, 128));                 // Purple
                SetForegroundColor("Delimiter", Color.FromRgb(128, 0, 128));            // Purple
                SetForegroundColor("Unexpected delimiter", Color.FromRgb(128, 0, 128)); // Purple
                SetBackgroundColor("Unexpected delimiter", Color.FromRgb(255, 255, 0)); // Yellow background
                SetForegroundColor("Escape delimiter", Color.FromRgb(218, 112, 214));   // Orchid
                SetForegroundColor("Keyword", Color.FromRgb(199, 21, 133));             // MediumVioletRed
                SetForegroundColor("Quote mark", Color.FromRgb(0, 0, 255));               // Blue
                SetForegroundColor("Operator", Color.FromRgb(199, 21, 133));            // MediumVioletRed
                SetForegroundColor("Literals", Color.FromRgb(0, 0, 255));               // Blue
                SetForegroundColor("Built-in functions", Color.FromRgb(186, 85, 211));  // MediumOrchid
                SetForegroundColor("Custom functions", Color.FromRgb(30, 144, 255));     // DodgerBlue
                SetForegroundColor("Custom properties", Color.FromRgb(32, 178, 170));   // LightSeaGreen
            }
        }

        public void SetForegroundColor(string colorKey, Color newColor)
        {
            var color = Definition.GetNamedColor(colorKey) ?? 
                        throw new KeyNotFoundException($"Color key \"{colorKey}\" not found.");
            color.Foreground = new SimpleHighlightingBrush(newColor);
        }

        public void SetBackgroundColor(string colorKey, Color newColor)
        {
            var color = Definition.GetNamedColor(colorKey) ?? 
                        throw new KeyNotFoundException($"Color key \"{colorKey}\" not found.");
            color.Background = new SimpleHighlightingBrush(newColor);
        }
    }
}

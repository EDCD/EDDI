using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using System;
using System.Collections.Generic;
using System.Linq;
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

            // Register our definition against the file extension
            HighlightingManager.Instance.RegisterHighlighting("Cottle", new string[] { ".cottle" }, Definition);
        }

        private static void AddHighlightWords ( XshdSyntaxDefinition xshd, string ruleSetName, IEnumerable<string> words )
        {
            var ruleSet = xshd.Elements.OfType<XshdRuleSet>().First( o => string.Equals(o.Name, ruleSetName, StringComparison.InvariantCultureIgnoreCase));
            var newKeyWords = new XshdKeywords { ColorReference = new XshdReference<XshdColor>( null, ruleSetName ) };
            foreach ( var w in words )
            {
                newKeyWords.Words.Add( w );
            }
            ruleSet.Elements.Add( newKeyWords );
        }

        // Keep this: it will be used when we come to implement custom user color schemes
        public void SetBackgroundColor(string colorKey, Color newColor)
        {
            HighlightingColor color = Definition.GetNamedColor(colorKey);
            if (color == null)
            {
                throw new KeyNotFoundException($"Color key \"{colorKey}\" not found.");
            }
            color.Background = new SimpleHighlightingBrush(newColor);
        }
    }
}

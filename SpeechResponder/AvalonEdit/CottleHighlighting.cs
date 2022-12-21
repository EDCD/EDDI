using ICSharpCode.AvalonEdit.Highlighting;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Xml;

namespace EddiSpeechResponder.AvalonEdit
{
    public class CottleHighlighting
    {
        public IHighlightingDefinition Definition;

        public CottleHighlighting()
        {
            Register();
        }

        private void Register()
        {
            using (Stream s = typeof(CottleHighlighting).Assembly.GetManifestResourceStream("EddiSpeechResponder.AvalonEdit.Cottle.xshd"))
            {
                if (s != null)
                {
                    using (XmlReader reader = new XmlTextReader(s))
                    {
                        Definition = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
                    }
                }
            }

            // Register our definition against the file extension
            HighlightingManager.Instance.RegisterHighlighting("Cottle", new string[] { ".cottle" }, Definition);
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

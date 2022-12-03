using ICSharpCode.AvalonEdit.Highlighting;
using System.IO;
using System.Windows.Media;
using System.Xml;

namespace EddiSpeechResponder.AvalonEdit
{
    internal class CottleHighlighting
    {
        public IHighlightingDefinition Definition;

        internal CottleHighlighting()
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

#pragma warning disable IDE0051 // Remove unused private members -- this will be used later
        private void SetBackgroundColor(string colorName, Color newColor)
#pragma warning restore IDE0051 // Remove unused private members
        {
            HighlightingColor color = Definition.GetNamedColor(colorName);
            color.Background = new SimpleHighlightingBrush(newColor);
        }
    }
}

using ICSharpCode.AvalonEdit.Highlighting;
using System.IO;
using System.Windows.Media;
using System.Xml;

namespace EddiSpeechResponder
{
    internal class CottleHighlightingDefinition
    {
        private IHighlightingDefinition cottleHighlightingDef;

        internal CottleHighlightingDefinition()
        {
            RegisterCottleDefinition();
        }

        private void RegisterCottleDefinition()
        {
            using (Stream s = typeof(CottleHighlightingDefinition).Assembly.GetManifestResourceStream("EddiSpeechResponder.Cottle.xshd"))
            {
                using (XmlReader reader = new XmlTextReader(s))
                {
                    cottleHighlightingDef = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }

            // Tweak a HighlightingColor at runtime to prove that we can
            SetBackgroundColor("Comment", Colors.White);

            // Register our definition against the file extension
            HighlightingManager.Instance.RegisterHighlighting("Cottle", new string[] { ".cottle" }, cottleHighlightingDef);
        }

        private void SetBackgroundColor(string colorName, Color newColor)
        {
            HighlightingColor color = cottleHighlightingDef.GetNamedColor(colorName);
            color.Background = new SimpleHighlightingBrush(newColor);
        }

    }
}

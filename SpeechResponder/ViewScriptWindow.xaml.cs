using ICSharpCode.AvalonEdit.Highlighting;
using JetBrains.Annotations;
using System.Windows;

namespace EddiSpeechResponder
{
    public partial class ViewScriptWindow : Window
    {
        public string ScriptName { get; private set; }
        public string ScriptDescription { get; private set; }
        public string ScriptValue { get; private set; }

#pragma warning disable IDE0052 // Remove unused private members -- this may be used later
        private readonly DocumentHighlighter documentHighlighter;
#pragma warning restore IDE0052 // Remove unused private members

        public ViewScriptWindow(Script script, [NotNull]AvalonEdit.CottleHighlighting cottleHighlighting)
        {
            InitializeComponent();
            DataContext = this;

            if (script != null)
            {
                ScriptName = script.Name;
                ScriptDescription = script.Description;
                ScriptValue = script.Value;
                scriptView.Text = ScriptValue;
            }

            // Set up our Cottle highlighter
            documentHighlighter = new DocumentHighlighter(scriptView.Document, cottleHighlighting.Definition);
        }

        private void okButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

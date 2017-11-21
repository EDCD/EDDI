using System.Windows;

namespace EddiSpeechResponder
{
    public partial class ShowScriptWindow : Window
    {
        public ShowScriptWindow(string script)
        {
            InitializeComponent();
            scriptText.Text = script;
        }
    }
}

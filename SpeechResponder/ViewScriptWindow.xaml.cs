using System.Windows;

namespace EddiSpeechResponder
{
    public partial class ViewScriptWindow : Window
    {
        public string ScriptName { get; set; }
        public string ScriptDescription { get; set; }
        public string ScriptValue { get; set; }

        public ViewScriptWindow(Script script)
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
        }

        private void okButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

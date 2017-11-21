using System.Collections.Generic;
using System.Windows;

namespace EddiSpeechResponder
{
    public partial class ViewScriptWindow : Window
    {
        private Dictionary<string, Script> scripts;
        private Script script;

        public string ScriptName { get; set; }
        public string ScriptDescription { get; set; }
        public string ScriptValue { get; set; }

        public ViewScriptWindow(Dictionary<string, Script> scripts, string name)
        {
            InitializeComponent();
            DataContext = this;

            this.scripts = scripts;

            scripts.TryGetValue(name, out script);
            if (script != null)
            {
                ScriptName = script.Name;
                ScriptDescription = script.Description;
                ScriptValue = script.Value;
            }
        }

        private void okButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

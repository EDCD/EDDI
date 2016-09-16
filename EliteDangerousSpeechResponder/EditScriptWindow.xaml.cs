using EDDI;
using EliteDangerousEvents;
using EliteDangerousSpeechResponder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Utilities;

namespace EliteDangerousSpeechResponder
{
    /// <summary>
    /// Interaction logic for EditScriptWindow.xaml
    /// </summary>
    public partial class EditScriptWindow : Window, INotifyPropertyChanged
    {
        private Dictionary<string, Script> scripts;
        private Script script;

        private string scriptName;
        public string ScriptName
        {
            get { return scriptName; }
            set { scriptName = value;  OnPropertyChanged("ScriptName");  }
        }
        private string scriptDescription;
        public string ScriptDescription
        {
            get { return scriptDescription; }
            set { scriptDescription = value; OnPropertyChanged("ScriptDescription"); }
        }
        private string scriptValue;
        public string ScriptValue
        {
            get { return scriptValue; }
            set { scriptValue = value; OnPropertyChanged("ScriptValue"); }
        }
        private bool responder;
        public bool Responder
        {
            get { return responder; }
            set { responder = value; OnPropertyChanged("Responder"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public EditScriptWindow(Dictionary<string, Script> scripts, string name)
        {
            InitializeComponent();
            DataContext = this;

            this.scripts = scripts;

            scripts.TryGetValue(name, out script);
            if (script == null)
            {
                // This is a new script
                ScriptName = "New script";
                ScriptDescription = null;
                ScriptValue = null;
                Responder = false;
            }
            else
            {
                // This is an existing script
                ScriptName = script.Name;
                ScriptDescription = script.Description;
                ScriptValue = script.Value;
                Responder = script.Responder;
            }
        }

        private void acceptButtonClick(object sender, RoutedEventArgs e)
        {
            if (script != null)
            {
                // Updated an existing script so remove it from the list
                scripts.Remove(script.Name);
            }
            script = new Script(scriptName, scriptDescription, script == null ? false : script.Responder, scriptValue);
            scripts.Add(script.Name, script);

            this.Close();
        }

        private void cancelButtonClick(object sender, RoutedEventArgs e)
        {
            // Nothing to do
            this.Close();
        }

        private void helpButtonClick(object sender, RoutedEventArgs e)
        {
        }

        private void variablesButtonClick(object sender, RoutedEventArgs e)
        {
        }

        private void resetButtonClick(object sender, RoutedEventArgs e)
        {
            ScriptValue = script.Value;
        }

        private void testButtonClick(object sender, RoutedEventArgs e)
        {
            // Splice the new script in to the existing scripts
            Dictionary<string, Script> newScripts = new Dictionary<string, Script>(scripts);
            Script testScript = new Script(ScriptName, ScriptDescription, false, ScriptValue);
            newScripts.Remove(ScriptName);
            newScripts.Add(ScriptName, testScript);

            // Obtain the sample event
            Event sampleEvent = Events.SampleByName(ScriptName);

            SpeechResponder responder = new SpeechResponder();
            responder.Start();
            ScriptResolver scriptResolver = new ScriptResolver(newScripts);
            responder.Say(scriptResolver, ScriptName, sampleEvent);
        }
    }
}

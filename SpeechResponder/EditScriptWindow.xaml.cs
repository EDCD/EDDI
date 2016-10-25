using EddiEvents;
using EddiJournalMonitor;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using Utilities;

namespace EddiSpeechResponder
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
            set { if (value == null || value.Trim() == "") scriptValue = null; else scriptValue = value; OnPropertyChanged("ScriptValue"); }
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

            // See if there is a default for this script
            Personality defaultPersonality = Personality.Default();
            Script defaultScript;
            defaultPersonality.Scripts.TryGetValue(scriptName, out defaultScript);
            if (defaultScript == null || defaultScript.Value == null)
            {
                // No default; disable reset and show
                showDefaultButton.IsEnabled = false;
                resetToDefaultButton.IsEnabled = false;
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

            DialogResult = true;
            this.Close();
        }

        private void cancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void helpButtonClick(object sender, RoutedEventArgs e)
        {
            HelpWindow helpWindow = new HelpWindow();
            helpWindow.Show();
        }

        private void variablesButtonClick(object sender, RoutedEventArgs e)
        {
            VariablesWindow variablesWindow = new VariablesWindow(ScriptName);
            variablesWindow.Show();
        }

        private void resetButtonClick(object sender, RoutedEventArgs e)
        {
            // Resetting the script resets it to its value in the default personality
            Personality defaultPersonality = Personality.Default();
            Script defaultScript;
            defaultPersonality.Scripts.TryGetValue(scriptName, out defaultScript);
            ScriptValue = defaultScript.Value;
        }

        private void testButtonClick(object sender, RoutedEventArgs e)
        {
            // Splice the new script in to the existing scripts
            Dictionary<string, Script> newScripts = new Dictionary<string, Script>(scripts);
            Script testScript = new Script(ScriptName, ScriptDescription, false, ScriptValue);
            newScripts.Remove(ScriptName);
            newScripts.Add(ScriptName, testScript);

            SpeechResponder responder = new SpeechResponder();
            responder.Start();

            // See if we have a sample
            Event sampleEvent;
            object sample = Events.SampleByName(script.Name);
            if (sample == null)
            {
                sampleEvent = null;
            }
            else if (sample.GetType() == typeof(string))
            {
                // It's as tring so a journal entry.  Parse it
                sampleEvent = JournalMonitor.ParseJournalEntry((string)sample);
            }
            else if (sample.GetType() == typeof(Event))
            {
                // It's a direct event
                sampleEvent = (Event)sample;
            }
            else
            {
                Logging.Warn("Unknown sample type " + sample.GetType());
                sampleEvent = null;
            }

            ScriptResolver scriptResolver = new ScriptResolver(newScripts);
            responder.Say(scriptResolver, ScriptName, sampleEvent, 3, false);
        }

        private void showDefaultButtonClick(object sender, RoutedEventArgs e)
        {
            Personality defaultPersonality = Personality.Default();
            Script defaultScript;
            defaultPersonality.Scripts.TryGetValue(scriptName, out defaultScript);

            if (defaultScript != null)
            {
                new ShowScriptWindow(defaultScript.Value).Show();
            }
        }
    }
}

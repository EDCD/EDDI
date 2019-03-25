using Eddi;
using EddiEvents;
using EddiJournalMonitor;
using EddiShipMonitor;
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
        private string originalName;

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
            set
            {
                scriptValue = string.IsNullOrWhiteSpace(value) ? null : value;
                OnPropertyChanged("ScriptValue");
            }
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
            this.originalName = name;

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
            defaultPersonality.Scripts.TryGetValue(scriptName, out Script defaultScript);
            if (defaultScript == null || defaultScript.Value == null)
            {
                // No default; disable reset and show
                showDefaultButton.IsEnabled = false;
                resetToDefaultButton.IsEnabled = false;
            }
        }

        private void acceptButtonClick(object sender, RoutedEventArgs e)
        {
            // Fetch the default script and mark this as default if it matches
            script = new Script(scriptName, scriptDescription, script == null ? false : script.Responder, scriptValue, script.Priority, false);
            Script defaultScript = null;
            if (Personality.Default().Scripts?.TryGetValue(script.Name, out defaultScript) ?? false)
            {
                script = Personality.UpgradeScript(script, defaultScript);
            }

            // Might be updating an existing script so remove it from the list before adding
            scripts.Remove(originalName);

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
            MarkdownWindow helpWindow = new MarkdownWindow("Help.md");
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
            defaultPersonality.Scripts.TryGetValue(scriptName, out Script defaultScript);
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
            List<Event> sampleEvents;
            object sample = Events.SampleByName(script.Name);
            if (sample == null)
            {
                sampleEvents = new List<Event>();
            }
            else if (sample is string)
            {
                // It's a string so a journal entry.  Parse it
                sampleEvents = JournalMonitor.ParseJournalEntry((string)sample);
            }
            else if (sample is Event)
            {
                // It's a direct event
                sampleEvents = new List<Event>(){ (Event)sample };
            }
            else
            {
                Logging.Warn("Unknown sample type " + sample.GetType());
                sampleEvents = new List<Event>();
            }

            ScriptResolver scriptResolver = new ScriptResolver(newScripts);
            if (sampleEvents.Count == 0)
            {
                sampleEvents.Add(null);
            }
            foreach (Event sampleEvent in sampleEvents)
            {
                responder.Say(scriptResolver, ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"))?.GetCurrentShip(), ScriptName, sampleEvent, scriptResolver.priority(script.Name));
            }
        }

        private void showDefaultButtonClick(object sender, RoutedEventArgs e)
        {
            Personality defaultPersonality = Personality.Default();
            if (defaultPersonality.Scripts.TryGetValue(scriptName, out Script defaultScript))
            {
                new ShowDiffWindow(defaultScript.Value, ScriptValue).Show();
                //new ShowScriptWindow(defaultScript.Value).Show();
            }
        }
    }
}

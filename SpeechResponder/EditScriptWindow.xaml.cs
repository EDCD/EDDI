using System;
using Eddi;
using EddiEvents;
using EddiJournalMonitor;
using EddiShipMonitor;
using ICSharpCode.AvalonEdit.Search;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using EddiSpeechResponder.Service;
using Utilities;

namespace EddiSpeechResponder
{
    /// <summary>
    /// Interaction logic for EditScriptWindow.xaml
    /// </summary>
    public partial class EditScriptWindow : Window, INotifyPropertyChanged
    {
        private readonly Dictionary<string, Script> _scripts;
        private Script _script;
        private readonly string originalName;

        private string scriptName;
        public string ScriptName
        {
            get { return scriptName; }
            set { scriptName = value; OnPropertyChanged("ScriptName"); }
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

        private int _priority;

        public int Priority
        {
            get { return _priority; }
            set
            {
                _priority = value;
                OnPropertyChanged(nameof(Priority));
            }
        }

        public string ScriptDefaultValue;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ScriptRecoveryService ScriptRecoveryService { get; set; }

        public EditScriptWindow(Dictionary<string, Script> scripts, string name)
        {
            InitializeComponent();
            DataContext = this;
            SearchPanel.Install(scriptView);

            this._scripts = scripts;
            this.originalName = name;

            scripts.TryGetValue(name, out _script);
            if (_script == null)
            {
                // This is a new script
                ScriptName = "New script";
                ScriptDescription = null;
                ScriptValue = null;
                Responder = false;
                Priority = 3;
            }
            else
            {
                // This is an existing script
                ScriptName = _script.Name;
                ScriptDescription = _script.Description;
                ScriptValue = _script.Value;
                ScriptDefaultValue = _script.defaultValue;
                Responder = _script.Responder;
                Priority = _script.Priority;
            }

            // See if there is the default value for this script is empty
            if (string.IsNullOrWhiteSpace(ScriptDefaultValue))
            {
                // No default; disable reset and show
                showDiffButton.IsEnabled = false;
                resetToDefaultButton.IsEnabled = false;
            }

            scriptView.Text = scriptValue;
            ScriptRecoveryService = new ScriptRecoveryService(this);
            ScriptRecoveryService.BeginScriptRecovery();
            scriptView.TextChanged += ScriptView_TextChanged;
        }

        private void ScriptView_TextChanged(object sender, System.EventArgs e)
        {
            ScriptValue = scriptView.Text;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ScriptRecoveryService.StopScriptRecovery();
        }

        private void acceptButtonClick(object sender, RoutedEventArgs e)
        {
            // Update the script
            string newScriptText = string.IsNullOrWhiteSpace(scriptView.Text) ? null : scriptView.Text;
            if (_script != null)
            {
                Script newScript = new Script(scriptName, 
                    scriptDescription, 
                    _script?.Responder ?? false,
                    newScriptText, 
                    Priority, 
                    _script.defaultValue);
                _script = newScript;
            }

            Script defaultScript = null;
            if (Personality.Default().Scripts?.TryGetValue(_script.Name, out defaultScript) ?? false)
            {
                _script = Personality.UpgradeScript(_script, defaultScript);
            }

            // Might be updating an existing script so remove it from the list before adding
            _scripts.Remove(originalName);

            _scripts.Add(_script.Name, _script);

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
            ScriptValue = ScriptDefaultValue;
            scriptView.Text = ScriptValue;
        }

        private void testButtonClick(object sender, RoutedEventArgs e)
        {
            ScriptRecoveryService.SaveRecoveryScript(ScriptValue, 
                ScriptName, 
                ScriptDescription, 
                Responder,
                Priority, 
                _script.defaultValue);
            // Splice the new script in to the existing scripts
            ScriptValue = scriptView.Text;
            Dictionary<string, Script> newScripts = new Dictionary<string, Script>(_scripts);
            Script testScript = new Script(ScriptName, ScriptDescription, false, ScriptValue);
            newScripts.Remove(ScriptName);
            newScripts.Add(ScriptName, testScript);

            SpeechResponder speechResponder = new SpeechResponder();
            speechResponder.Start();

            // See if we have a sample
            List<Event> sampleEvents;
            object sample = Events.SampleByName(_script.Name);
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
                sampleEvents = new List<Event>() { (Event)sample };
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
                speechResponder.Say(scriptResolver, ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"))?.GetCurrentShip(), ScriptName, sampleEvent, scriptResolver.priority(_script.Name));
            }
        }

        private void showDiffButtonClick(object sender, RoutedEventArgs e)
        {
            ScriptValue = scriptView.Text;
            if (!string.IsNullOrWhiteSpace(ScriptDefaultValue))
            {
                new ShowDiffWindow(ScriptDefaultValue, ScriptValue).Show();
            }
        }
    }
}

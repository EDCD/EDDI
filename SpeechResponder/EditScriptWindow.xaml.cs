using Eddi;
using EddiEvents;
using EddiJournalMonitor;
using EddiShipMonitor;
using ICSharpCode.AvalonEdit.Search;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Utilities;

namespace EddiSpeechResponder
{
    /// <summary>
    /// Interaction logic for EditScriptWindow.xaml
    /// </summary>
    public partial class EditScriptWindow : Window, INotifyPropertyChanged
    {
        private readonly Dictionary<string, Script> scripts;
        private Script script;
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

        public string ScriptDefaultValue;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public EditScriptWindow(Dictionary<string, Script> scripts, string name)
        {
            InitializeComponent();
            DataContext = this;
            SearchPanel.Install(scriptView);

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
                ScriptDefaultValue = script.defaultValue;
                Responder = script.Responder;
            }

            // See if there is the default value for this script is empty
            if (string.IsNullOrWhiteSpace(ScriptDefaultValue))
            {
                // No default; disable reset and show
                showDiffButton.IsEnabled = false;
                resetToDefaultButton.IsEnabled = false;
            }

            scriptView.Text = scriptValue;
        }

        private void acceptButtonClick(object sender, RoutedEventArgs e)
        {
            // Update the script
            string newScriptText = scriptView.Text;
            if (script != null)
            {
                Script newScript = new Script(scriptName, scriptDescription, script?.Responder ?? false, newScriptText, script.Priority, script.defaultValue);
                script = newScript;
            }

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
            ScriptValue = ScriptDefaultValue;
            scriptView.Text = ScriptValue;
        }

        private void testButtonClick(object sender, RoutedEventArgs e)
        {
            // Splice the new script in to the existing scripts
            ScriptValue = scriptView.Text;
            Dictionary<string, Script> newScripts = new Dictionary<string, Script>(scripts);
            Script testScript = new Script(ScriptName, ScriptDescription, false, ScriptValue);
            newScripts.Remove(ScriptName);
            newScripts.Add(ScriptName, testScript);

            SpeechResponder speechResponder = new SpeechResponder();
            speechResponder.Start();

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
                speechResponder.Say(scriptResolver, ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor"))?.GetCurrentShip(), ScriptName, sampleEvent, scriptResolver.priority(script.Name));
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

        private void textEditorMouseUp(object sender, MouseButtonEventArgs e) 
        {
            if (e.ChangedButton != MouseButton.Right) return; 

            ContextMenu contextMenu = new ContextMenu();

            MenuItem menuItem = new MenuItem() { Header = Properties.Tooltips.cut };
            menuItem.Click += CutAction;
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem() { Header = Properties.Tooltips.copy };
            menuItem.Click += CopyAction;
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem() { Header = Properties.Tooltips.paste };
            menuItem.Click += PasteAction;
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem() { Header = Properties.Tooltips.delete };
            menuItem.Click += DeleteAction;
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem() { Header = Properties.Tooltips.undo };
            menuItem.Click += UndoAction;
            contextMenu.Items.Add(menuItem);

            menuItem = new MenuItem() { Header = Properties.Tooltips.redo };
            menuItem.Click += RedoAction;
            contextMenu.Items.Add(menuItem);

            scriptView.ContextMenu = contextMenu;
        }

        private void CutAction(object sender, RoutedEventArgs e)
        {
            scriptView.Cut();
        }

        private void CopyAction(object sender, RoutedEventArgs e)
        {
            scriptView.Copy();
        }

        private void PasteAction(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsText())
            {
                scriptView.Paste();
            }
        }

        private void DeleteAction(object sender, RoutedEventArgs e)
        {
            scriptView.Delete();
        }

        private void UndoAction(object sender, RoutedEventArgs e)
        {
            if (scriptView.CanUndo)
            {
                scriptView.Undo();
            }
        }

        private void RedoAction(object sender, RoutedEventArgs e)
        {
            if (scriptView.CanRedo)
            {
                scriptView.Redo();
            }
        }
    }
}

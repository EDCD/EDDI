using EddiSpeechResponder.Service;
using EddiSpeechService;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Collections.Generic;
using System.Windows;

namespace EddiSpeechResponder
{
    /// <summary>
    /// Interaction logic for EditScriptWindow.xaml
    /// </summary>
    public partial class EditScriptWindow : Window
    {
        public Script script { get; private set; }
        public Script editorScript { get; private set; }

        private readonly Dictionary<string, Script> _scripts;

        public ScriptRecoveryService ScriptRecoveryService { get; set; }

        public EditScriptWindow(Script script, Dictionary<string, Script> scripts)
        {
            InitializeComponent();
            DataContext = this;
            SearchPanel.Install(scriptView);

            this._scripts = scripts;
            this.script = script;

            if (script == null)
            {
                // This is a new script
                editorScript = new Script("New script", null, false, null);
            }
            else
            {
                // This is an existing script
                editorScript = script.Copy();
            }

            // See if there is the default value for this script is empty
            if (string.IsNullOrWhiteSpace(editorScript.defaultValue))
            {
                // No default; disable reset and show
                showDiffButton.IsEnabled = false;
                resetToDefaultButton.IsEnabled = false;
            }

            scriptView.Text = editorScript.Value;
            ScriptRecoveryService = new ScriptRecoveryService(this);
            ScriptRecoveryService.BeginScriptRecovery();
            scriptView.TextChanged += ScriptView_TextChanged;
        }

        private void ScriptView_TextChanged(object sender, System.EventArgs e)
        {
            editorScript.Value = scriptView.Text;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ScriptRecoveryService.StopScriptRecovery();
        }

        private void acceptButtonClick(object sender, RoutedEventArgs e)
        {
            // Update the output script
            script = editorScript;

            // Make sure default values are set as required
            Script defaultScript = null;
            if (Personality.Default().Scripts?.TryGetValue(script.Name, out defaultScript) ?? false)
            {
                script = Personality.UpgradeScript(script, defaultScript);
            }

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
            VariablesWindow variablesWindow = new VariablesWindow(editorScript.Name);
            variablesWindow.Show();
        }

        private void resetButtonClick(object sender, RoutedEventArgs e)
        {
            // Resetting the script resets it to its value in the default personality
            editorScript.Value = editorScript.defaultValue;
            scriptView.Text = editorScript.Value;
        }

        private void testButtonClick(object sender, RoutedEventArgs e)
        {
            if (!SpeechService.Instance.eddiSpeaking)
            {
                ScriptRecoveryService.SaveRecoveryScript(editorScript);

                // Splice the new script in to the existing scripts
                editorScript.Value = scriptView.Text;
                Dictionary<string, Script> newScripts = new Dictionary<string, Script>(_scripts);
                Script testScript = new Script(editorScript.Name, editorScript.Description, false, editorScript.Value);
                newScripts.Remove(editorScript.Name);
                newScripts.Add(editorScript.Name, testScript);

                SpeechResponder speechResponder = new SpeechResponder();
                speechResponder.Start();
                speechResponder.TestScript(editorScript.Name, newScripts);
            }
            else
            {
                SpeechService.Instance.ShutUp();
            }
        }

        private void showDiffButtonClick(object sender, RoutedEventArgs e)
        {
            editorScript.Value = scriptView.Text;
            if (!string.IsNullOrWhiteSpace(editorScript.defaultValue))
            {
                new ShowDiffWindow(editorScript.defaultValue, editorScript.Value).Show();
            }
        }
    }
}

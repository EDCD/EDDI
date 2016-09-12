using EDDI;
using EliteDangerousSpeechResponder;
using System;
using System.Collections.Generic;
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

namespace EliteDangerousSpeechResponder
{
    /// <summary>
    /// Interaction logic for EditScriptWindow.xaml
    /// </summary>
    public partial class EditScriptWindow : Window
    {
        private Dictionary<string, Script> scripts;
        private Script script;

        private const string DEFAULT_DESCRIPTION = "foo";

        public EditScriptWindow(Dictionary<string, Script> scripts, string name)
        {
            this.scripts = scripts;
            script = this.scripts[name];

            InitializeComponent();

            descriptionText.Text = script.Description == null ? DEFAULT_DESCRIPTION : script.Description;
            scriptText.Text = script.Value;
        }

        private void acceptButtonClick(object sender, RoutedEventArgs e)
        {
            script.Value = scriptText.Text;
            this.Close();
        }

        private void cancelButtonClick(object sender, RoutedEventArgs e)
        {
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
            scriptText.Text = script.Value;
        }

        private void defaultButtonClick(object sender, RoutedEventArgs e)
        {
            scriptText.Text = script.DefaultValue;
        }

        private void testButtonClick(object sender, RoutedEventArgs e)
        {
            // Splice the new script in to the existing scripts
            Dictionary<string, Script> newScripts = new Dictionary<string, Script>(scripts);
            Script testScript = new Script(script.Name, script.Description, scriptText.Text);
            newScripts.Remove(script.Name);
            newScripts.Add(script.Name, testScript);

            SpeechResponder responder = new SpeechResponder();
            responder.Start();
            ScriptResolver scriptResolver = new ScriptResolver(newScripts);
            responder.Say(scriptResolver, script.Name);
        }
    }
}

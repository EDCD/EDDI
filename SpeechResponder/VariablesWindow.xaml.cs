using EddiEvents;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EddiSpeechResponder
{
    /// <summary>
    /// Interaction logic for VariablesWindow.xaml
    /// </summary>
    public partial class VariablesWindow : Window
    {
        public VariablesWindow(string scriptName)
        {
            InitializeComponent();

            // Read Markdown and convert it to HTML
            string markdown = File.ReadAllText("Variables.md");

            string description;
            if (Events.DESCRIPTIONS.TryGetValue(scriptName, out description))
            {
                // The user is editing an event, add event-specific information
                markdown += "## Event\n\n" + description + ".\n\n";
                IDictionary<string, string> variables;
                if (Events.VARIABLES.TryGetValue(scriptName, out variables))
                {
                    if (variables.Count == 0)
                    {
                        markdown += "This event has no variables.";
                    }
                    else
                    {
                        foreach (KeyValuePair<string, string> variable in Events.VARIABLES[scriptName])
                        {
                            markdown += "    - " + variable.Key + " " + variable.Value + "\n";
                        }
                    }
                }
            }

            string html = CommonMark.CommonMarkConverter.Convert(markdown);

            // Insert the HTML
            textBrowser.NavigateToString(html);
        }
    }
}

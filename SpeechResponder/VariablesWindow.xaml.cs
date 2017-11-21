using EddiEvents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using Utilities;

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
            string markdown;
            try
            {
                DirectoryInfo dir = new DirectoryInfo(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                markdown = Files.Read(dir.FullName + @"\Variables.md");
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to find variables.md", ex);
                markdown = "";
            }

            string description;
            if (Events.DESCRIPTIONS.TryGetValue(scriptName, out description))
            {
                // The user is editing an event, add event-specific information
                markdown += "\n\n## " + scriptName + " event\n\n" + description + ".\n\n";
                IDictionary<string, string> variables;
                if (Events.VARIABLES.TryGetValue(scriptName, out variables))
                {
                    if (variables.Count == 0)
                    {
                        markdown += "This event has no variables.";
                    }
                    else
                    {
                        markdown += "Information about this event is available under the `event` object.  Note that these variables are only valid for this particular script; other scripts triggered by different events will have different variables available to them.\n\n";
                        foreach (KeyValuePair<string, string> variable in Events.VARIABLES[scriptName])
                        {
                            markdown += "    - " + variable.Key + " " + variable.Value + "\n";
                        }
                    }
                }
            }

            string html = CommonMark.CommonMarkConverter.Convert(markdown);
            html = "<head>  <meta charset=\"UTF-8\"> </head> " + html;

            // Insert the HTML
            textBrowser.NavigateToString(html);
        }
    }
}

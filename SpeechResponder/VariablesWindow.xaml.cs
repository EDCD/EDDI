using EddiEvents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public VariablesWindow(Script script)
        {
            InitializeComponent();

            // Read Markdown and convert it to HTML
            string markdown;
            try
            {
                DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty);
                markdown = Files.Read(dir.FullName + @"\Variables.md");
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to find variables.md", ex);
                markdown = "";
            }

            // If the user is editing an event-based script, add event-specific information
            var @type = Events.TYPES.SingleOrDefault(t => t.Key == script.Name).Value;
            if (@type != null)
            {
                var vars = new MetaVariables(@type).Results;
                var CottleVars = vars.AsCottleVariables();
                if (CottleVars.Any())
                {
                    markdown += "Information about this event is available under the `event` object.  Note that these variables are only valid for this particular script; other scripts triggered by different events will have different variables available to them.\n";
                    if (vars.Any(v => v.keysPath.Any(k => k.Contains(@"<index"))))
                    {
                        markdown += "Where values are indexed (the compartments on a ship for example), the index will be represented by '*\\<index\\>*'.\n\n";
                    }
                    markdown += "\n";
                    foreach (var cottleVariable in CottleVars.OrderBy(i => i.key))
                    {
                        var description = !string.IsNullOrEmpty(cottleVariable.description) ? $" - {cottleVariable.description}" : "";
                        markdown += $"  - *{cottleVariable.key}* {description}\n";
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

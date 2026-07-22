using EddiEvents;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using Utilities;
using Utilities.MetaVariables;

namespace EddiSpeechResponder
{
    /// <summary>
    /// Interaction logic for VariablesWindow.xaml
    /// </summary>
    public partial class VariablesWindow : Window
    {
        private const string EventVariablesPlaceholder = "Event-specific variables are available under the `event` object while editing an event script and are documented on each event page.";
        private const string VariablesHeading = "## Root Variables";
        private const string ObjectReferenceHeading = "## Object reference";

        public VariablesWindow(Script script)
        {
            InitializeComponent();

            // Read Markdown and convert it to HTML
            string markdown;
            try
            {
                var dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty);
                markdown = Files.Read(dir.FullName + @"\Wiki\Variables.md");
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to find variables.md", ex);
                markdown = "";
            }

            markdown = InjectEventVariables( markdown, script );

            var html = CommonMark.CommonMarkConverter.Convert(markdown);
            html = Utilities.MarkdownDecorator.Decorate(html);

            // Insert the HTML
            textBrowser.NavigateToString(html);
        }

        private static string InjectEventVariables ( string markdown, Script script )
        {
            if ( script?.Name == null )
            {
                return markdown;
            }

            // If the user is editing an event-based script, add event-specific information.
            var @type = Events.TYPES.SingleOrDefault(t => t.Key == script.Name).Value;
            if (@type != null)
            {
                var vars = new MetaVariables(@type).Results;
                var CottleVars = vars.AsCottleVariables();
                if (CottleVars.Count > 0 )
                {
                    var intro = new StringBuilder();
                    intro.AppendLine( $"For this `{script.Name}` event script, the `event` variable contains event-specific values. Other event scripts have different event variables." );
                    if (vars.Any(v => v.keysPath.Any(k => k.Contains(@"<index"))))
                    {
                        intro.AppendLine();
                        intro.AppendLine( "List object values can be accessed using an index (key) between square brackets, for example `event.items[<index\\>].name`." );
                    }

                    markdown = markdown.Replace( EventVariablesPlaceholder, intro.ToString().TrimEnd(), StringComparison.Ordinal );

                    markdown = InsertAfterHeading( markdown, VariablesHeading, $"  - *event* - Details about the `{script.Name}` event.{Environment.NewLine}" );

                    var eventReference = new StringBuilder();
                    eventReference.AppendLine( "### Event" );
                    eventReference.AppendLine();
                    eventReference.AppendLine( "Used by: `event`" );
                    eventReference.AppendLine();
                    foreach (var cottleVariable in CottleVars.OrderBy(i => i.key))
                    {
                        var description = !string.IsNullOrEmpty(cottleVariable.description) ? cottleVariable.description : "";
                        eventReference.AppendLine( $"  - *{cottleVariable.key}* - {description}" );
                    }
                    eventReference.AppendLine();
                    markdown = InsertAfterHeading( markdown, ObjectReferenceHeading, eventReference.ToString() );
                }
            }

            return markdown;
        }

        private static string InsertAfterHeading ( string markdown, string heading, string insertion )
        {
            var headingIndex = markdown.IndexOf( heading, StringComparison.Ordinal );
            if ( headingIndex < 0 )
            {
                return $"{insertion}{Environment.NewLine}{markdown}";
            }

            var insertIndex = markdown.IndexOf( '\n', headingIndex + heading.Length );
            if ( insertIndex < 0 )
            {
                return $"{markdown}{Environment.NewLine}{insertion}";
            }

            insertIndex++;
            while ( insertIndex < markdown.Length && ( markdown[ insertIndex ] == '\r' || markdown[ insertIndex ] == '\n' ) )
            {
                insertIndex++;
            }

            return markdown.Insert( insertIndex, insertion );
        }
    }
}

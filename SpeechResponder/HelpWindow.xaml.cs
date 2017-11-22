using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Utilities;

namespace EddiSpeechResponder
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();

            // Read Markdown and convert it to HTML
            string markdown;
            try
            {
                DirectoryInfo dir = new DirectoryInfo(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                string helpfile = $@"\Help{(I18N.GetLang() == "en" ? "" : $"_{I18N.GetLang()}")}.md"; // choose help file in the current language if it exists
                if(File.Exists(dir.FullName + helpfile))
                {
                    markdown = Files.Read(dir.FullName + helpfile);
                }
                else
                {
                    markdown = Files.Read(dir.FullName + @"\Help.md");
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to find variables.md", ex);
                markdown = "";
            }
            string html = CommonMark.CommonMarkConverter.Convert(markdown);
            html = "<head>  <meta charset=\"UTF-8\"> </head> " + html;

            // Insert the HTML
            textBrowser.NavigateToString(html);
        }
    }
}

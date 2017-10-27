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
                markdown = Files.Read(dir.FullName + @"\Help.md");
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

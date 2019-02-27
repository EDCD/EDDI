using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Utilities;

namespace EddiSpeechResponder
{
    /// <summary>
    /// Interaction logic for opening a generic SpeechResponder project markdown 
    /// </summary>
    public partial class MarkdownWindow : Window
    {
        public MarkdownWindow(string fileName)
        {
            InitializeComponent();

            // Read Markdown and convert it to HTML
            string markdown;
            try
            {
                DirectoryInfo dir = new DirectoryInfo(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                markdown = Files.Read(dir.FullName + @"\" + fileName);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to find " + fileName, ex);
                markdown = "";
            }
            string html = CommonMark.CommonMarkConverter.Convert(markdown);
            html = "<head>  <meta charset=\"UTF-8\"> </head> " + html;

            // Insert the HTML
            textBrowser.NavigateToString(html);
        }
    }
}

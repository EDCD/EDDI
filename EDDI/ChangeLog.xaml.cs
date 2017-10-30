using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Utilities;

namespace Eddi
{
    /// <summary>
    /// Interaction logic for ChangeLogWindow.xaml
    /// </summary>
    public partial class ChangeLogWindow : Window
    {
        public ChangeLogWindow()
        {
            InitializeComponent();

            // Read Markdown and convert it to HTML
            string markdown;
            try
            {
                DirectoryInfo dir = new DirectoryInfo(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                markdown = Files.Read(dir + @"\ChangeLog.md");
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to find ChangeLog.md", ex);
                markdown = "";
            }
            string html = CommonMark.CommonMarkConverter.Convert(markdown);
            html = "<head>  <meta charset=\"UTF-8\"> </head> " + html;

            // Insert the HTML
            textBrowser.NavigateToString(html);
        }
    }
}

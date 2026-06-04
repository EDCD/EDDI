using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Utilities;

namespace EddiUI
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
                var dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new InvalidOperationException());
                var fileInfo = new FileInfo( dir + @"\ChangeLog.md" );
                if ( !fileInfo.Exists ) { throw new FileNotFoundException(); }
                markdown = Files.Read( dir + @"\ChangeLog.md" );
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to find ChangeLog.md", ex);
                markdown = "";
            }
            var html = CommonMark.CommonMarkConverter.Convert(markdown);
            html = Utilities.MarkdownDecorator.Decorate(html);

            // Insert the HTML
            textBrowser.NavigateToString(html);
        }
    }
}

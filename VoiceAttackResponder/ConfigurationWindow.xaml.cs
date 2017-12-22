using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Utilities;

namespace EddiVoiceAttackResponder
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl
    {
        public ConfigurationWindow()
        {
            InitializeComponent();
            I18NForComponents();
        }

        private void I18NForComponents()
        {
            p1.Text = I18N.GetString("va_responder_p1");
            p2.Text = I18N.GetString("va_responder_p2");
            p3.Text = I18N.GetString("va_responder_p3");
            Hyperlink link_p4 = new Hyperlink();
            link_p4.Inlines.Add(new Run(I18N.GetString("va_responder_p4_link")));
            link_p4.Click += VAVariablesClicked;
            p4.Inlines.Clear();
            p4.Inlines.Add(new Run(I18N.GetString("va_responder_p4")));
            p4.Inlines.Add(link_p4);
            p4.Inlines.Add(new Run("."));
            //p4Link.Name = I18N.GetString("va_responder_p4_link");
            Hyperlink link_p5 = new Hyperlink();
            link_p5.Inlines.Add(new Run(I18N.GetString("va_responder_p5_link")));
            link_p5.Click += VAVariablesClicked;
            p5.Inlines.Clear();
            p5.Inlines.Add(new Run(I18N.GetString("va_responder_p5")));
            p5.Inlines.Add(link_p5);
            p5.Inlines.Add(new Run("."));
            //p5Link.Name = I18N.GetString("va_responder_p5_link");
        }

        private void VAExampleClicked(object sender, RoutedEventArgs e)
        {
            // TODO: Add links to a youtube playlist?
            Process.Start("https://youtube.com/");
        }

        private void VAVariablesClicked(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/EDCD/EDDI/wiki/VoiceAttack-Integration");
        }
    }
}

using System.Diagnostics;
using System.Windows;

namespace Utilities
{
    /// <summary>
    /// Interaction logic for IpaResourcesWindow.xaml
    /// </summary>
    public partial class IpaResourcesWindow : Window
    {
        public IpaResourcesWindow()
        {
            InitializeComponent();
        }

        private void pageClicked(object sender, RoutedEventArgs e)
        {
            string url = Properties.IPA.ipa_page;
            Process.Start(url);
        }

        private void resource1Clicked(object sender, RoutedEventArgs e)
        {
            string url = Properties.IPA.ipa_resource1;
            Process.Start(url);
        }

        private void resource2Clicked(object sender, RoutedEventArgs e)
        {
            string url = Properties.IPA.ipa_resource2;
            Process.Start(url);
        }

        private void resource3Clicked(object sender, RoutedEventArgs e)
        {
            string url = Properties.IPA.ipa_resource3;
            Process.Start(url);
        }
    }
}

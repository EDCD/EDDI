using EddiCompanionAppService;
using EddiCore;
using EddiSpeechService;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;

namespace EddiUI
{
    public partial class FrontierApiTab : UserControl
    {
        public FrontierApiTab ()
        {
            InitializeComponent();
            setStatusInfo();
            CompanionAppCredentials companionAppCredentials = CompanionAppCredentials.Load();
            CompanionAppService.Instance.StateChanged += companionApiStatusChanged;
        }

        private void companionApiStatusChanged ( CompanionAppService.State oldState, CompanionAppService.State newState )
        {
            // The calling thread for this method may not have direct access to the MainWindow dispatcher so we invoke the dispatcher here.
            Application.Current?.Dispatcher?.InvokeAsync( () =>
            {
                MainWindow mainwindow = (MainWindow)Application.Current?.MainWindow;
                mainwindow?.Dispatcher?.InvokeAsync( setStatusInfo );
            } );

            if ( oldState == CompanionAppService.State.AwaitingCallback &&
                newState == CompanionAppService.State.Authorized )
            {
                SpeechService.Instance.Say( null, string.Format( Properties.EddiResources.frontier_api_ok, EDDI.Instance.Cmdr?.phoneticname ), 0 );
                SpeechService.Instance.Say( null, Properties.EddiResources.frontier_api_close_browser, 0 );
            }
            else if ( oldState == CompanionAppService.State.LoggedOut &&
                newState == CompanionAppService.State.AwaitingCallback )
            {
                SpeechService.Instance.Say( null, Properties.EddiResources.frontier_api_please_authenticate, 0 );
            }
        }

        // Set the fields relating to status information
        private void setStatusInfo ()
        {
            switch ( CompanionAppService.Instance.CurrentState )
            {
                case CompanionAppService.State.LoggedOut:
                case CompanionAppService.State.ConnectionLost:
                    companionAppStatusValue.Text = Properties.EddiResources.frontierApiNotConnected;
                    companionAppButton.Content = Properties.EddiResources.login;
                    companionAppButton.IsEnabled = !EDDI.FromVA;
                    companionAppText.Text = !EDDI.FromVA ? "" : Properties.EddiResources.frontier_api_cant_login_from_va;
                    break;
                case CompanionAppService.State.AwaitingCallback:
                    companionAppStatusValue.Text = Properties.EddiResources.frontierApiConnecting;
                    companionAppButton.Content = Properties.MainWindow.reset_button;
                    companionAppButton.IsEnabled = true;
                    companionAppText.Text = Properties.EddiResources.frontier_api_please_authenticate;
                    break;
                case CompanionAppService.State.Authorized:
                case CompanionAppService.State.TokenRefresh:
                    companionAppStatusValue.Text = Properties.EddiResources.frontierApiConnected;
                    companionAppButton.Content = Properties.MainWindow.reset_button;
                    companionAppButton.IsEnabled = true;
                    companionAppText.Text = Properties.MainWindow.tab_frontier_reset_desc;
                    break;
                case CompanionAppService.State.NoClientIDConfigured:
                    companionAppStatusValue.Text = Properties.EddiResources.frontierApiNotEnabled;
                    companionAppButton.Content = Properties.EddiResources.login;
                    companionAppButton.IsEnabled = false;
                    companionAppText.Text = Properties.MainWindow.tab_frontier_not_enabled_desc;
                    break;
            }
        }

        // Handle changes to the Frontier API tab
        private void companionAppClicked ( object sender, RoutedEventArgs e )
        {
            if ( CompanionAppService.Instance.CurrentState == CompanionAppService.State.LoggedOut )
            {
                if ( IsAdministrator() )
                {
                    SpeechService.Instance.Say( null, Properties.EddiResources.frontier_api_admin_mode, 0 );
                }
                CompanionAppService.Instance.Login();
            }
            else
            {
                // Logout from the companion EddiApplication and start again
                CompanionAppService.Instance.Logout();
                SpeechService.Instance.Say( null, Properties.EddiResources.frontier_api_reset, 0 );
                if ( EDDI.FromVA )
                {
                    SpeechService.Instance.Say( null, Properties.EddiResources.frontier_api_cant_login_from_va, 0 );
                }
            }
        }

        private static bool IsAdministrator ()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole( WindowsBuiltInRole.Administrator );
        }
    }
}

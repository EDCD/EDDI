using EddiConfigService;
using EddiConfigService.Configurations;
using EddiDataDefinitions;
using EddiSpeechService;
using EddiSpeechService.SpeechProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Navigation;
using Utilities;

namespace EddiUI
{
    public partial class WebVoiceProvidersWindow : Window
    {
        private readonly List<WebSpeechProvider> profiles;
        private readonly IReadOnlyList<WebSpeechProviderDescriptor> providerDescriptors;
        private readonly Dictionary<string, FrameworkElement> settingControls = [];
        private bool loadingProfile;

        public WebVoiceProvidersWindow()
        {
            InitializeComponent();
            ApplyLocalizedText();
            providerDescriptors = SpeechService.Instance.SpeechManager.WebProviderDescriptors;
            var defaultProviderDescriptor = providerDescriptors.Count > 0 ? providerDescriptors[0] : null;
            newProviderTypeDropDown.ItemsSource = providerDescriptors;
            newProviderTypeDropDown.SelectedItem = defaultProviderDescriptor;
            newProviderTypeDropDown.IsEnabled = providerDescriptors.Count > 0;
            providerTypeDropDown.ItemsSource = providerDescriptors;
            providerTypeDropDown.SelectedItem = defaultProviderDescriptor;
            addProfileButton.IsEnabled = providerDescriptors.Count > 0;

            profiles = ConfigService.Instance.speechServiceConfiguration.SpeechProviderConfigurations
                .Select( profile => profile.Clone() )
                .ToList();
            RefreshProfileList( profiles.FirstOrDefault() );
        }

        private void ApplyLocalizedText()
        {
            Title = Properties.Resources.web_voice_providers_title;
            providerTypeLabel.Content = Properties.Resources.web_voice_provider_type_label;
            profileNameLabel.Content = Properties.Resources.web_voice_provider_profile_name_label;
            profileEnabledLabel.Content = Properties.Resources.web_voice_provider_enabled_label;
            profileLocaleFiltersLabel.Content = Properties.Resources.web_voice_provider_locale_filters_label;
            profileLocaleFiltersHelpTextBlock.Text = Properties.Resources.web_voice_provider_locale_filters_help;
            profileSettingsLabel.Content = Properties.Resources.web_voice_provider_settings_label;
            addProfileButton.Content = Properties.Resources.web_voice_provider_add_profile_button;
            saveButton.Content = Properties.Resources.web_voice_provider_save_button;
            cancelButton.Content = Properties.Resources.web_voice_provider_cancel_button;
            verifyProfileButton.Content = Properties.Resources.web_voice_provider_verify_button;
            clearCredentialsButton.Content = Properties.Resources.web_voice_provider_clear_credentials_button;
            removeProfileButton.Content = Properties.Resources.web_voice_provider_remove_button;
        }

        private void RefreshProfileList ( WebSpeechProvider selectedProfile )
        {
            loadingProfile = true;
            providerProfilesList.ItemsSource = null;
            providerProfilesList.ItemsSource = profiles;
            providerProfilesList.SelectedItem = selectedProfile ?? profiles.FirstOrDefault();
            loadingProfile = false;
            LoadSelectedProfile();
        }

        private void LoadSelectedProfile()
        {
            loadingProfile = true;
            try
            {
                if ( providerProfilesList.SelectedItem is not WebSpeechProvider profile )
                {
                    providerTypeDropDown.SelectedItem = newProviderTypeDropDown.SelectedItem ?? (providerDescriptors.Count > 0 ? providerDescriptors[0] : null);
                    providerTypeDropDown.IsEnabled = false;
                    profileNameTextBox.Text = string.Empty;
                    profileEnabledCheckBox.IsChecked = false;
                    profileLocaleFiltersTextBox.Text = string.Empty;
                    RenderProfileSettings( null, null );
                    UpdateProviderInfoLinks( providerTypeDropDown.SelectedItem as WebSpeechProviderDescriptor );
                    SetProfileFieldsEnabled( false );
                    return;
                }

                var descriptor = GetDescriptor( profile.ProviderType );
                providerTypeDropDown.SelectedItem = descriptor;
                providerTypeDropDown.IsEnabled = false;
                profileNameTextBox.Text = profile.DisplayName;
                profileEnabledCheckBox.IsChecked = profile.Enabled;
                profileLocaleFiltersTextBox.Text = string.Join( ", ", profile.LocaleFilters );
                UpdateProviderInfoLinks( descriptor );
                RenderProfileSettings( profile, descriptor );
                SetProfileFieldsEnabled( true );
            }
            finally
            {
                loadingProfile = false;
            }
        }

        private void RenderProfileSettings (
            WebSpeechProvider profile,
            WebSpeechProviderDescriptor descriptor )
        {
            profileSettingsPanel.Children.Clear();
            settingControls.Clear();
            if ( profile == null || descriptor == null )
            {
                return;
            }

            foreach ( var field in descriptor.ProfileFields )
            {
                var row = new DockPanel
                {
                    LastChildFill = true,
                    Margin = new Thickness( 0, 0, 0, 6 )
                };
                var label = new Label
                {
                    Content = ProviderFieldLabel( descriptor, field ),
                    Width = 100,
                    Padding = new Thickness( 0, 0, 8, 0 ),
                    VerticalContentAlignment = VerticalAlignment.Center
                };
                DockPanel.SetDock( label, Dock.Left );
                row.Children.Add( label );

                FrameworkElement control;
                if ( field.IsSecret )
                {
                    var passwordBox = new PasswordBox
                    {
                        Password = profile.GetSetting( field.Key ) ?? string.Empty,
                        Tag = field.Key
                    };
                    passwordBox.PasswordChanged += profileField_Changed;
                    control = passwordBox;
                }
                else
                {
                    var textBox = new TextBox
                    {
                        Text = profile.GetSetting( field.Key ) ?? string.Empty,
                        Tag = field.Key
                    };
                    textBox.TextChanged += profileField_Changed;
                    control = textBox;
                }

                row.Children.Add( control );
                profileSettingsPanel.Children.Add( row );
                settingControls[ field.Key ] = control;
            }
        }

        private void SetProfileFieldsEnabled ( bool enabled )
        {
            profileNameTextBox.IsEnabled = enabled;
            profileEnabledCheckBox.IsEnabled = enabled;
            profileLocaleFiltersTextBox.IsEnabled = enabled;
            profileSettingsPanel.IsEnabled = enabled;
            verifyProfileButton.IsEnabled = enabled;
            clearCredentialsButton.IsEnabled = enabled;
            removeProfileButton.IsEnabled = enabled;
        }

        private void UpdateProviderInfoLinks ( WebSpeechProviderDescriptor descriptor )
        {
            providerInfoLinksTextBlock.Inlines.Clear();
            if ( descriptor == null )
            {
                providerInfoLinksTextBlock.Visibility = Visibility.Collapsed;
                return;
            }

            AddProviderInfoLink(
                providerInfoLinksTextBlock,
                Properties.Resources.web_voice_provider_setup_link,
                descriptor.SetupUrl );
            AddProviderInfoLink(
                providerInfoLinksTextBlock,
                Properties.Resources.web_voice_provider_account_link,
                descriptor.AccountUrl );

            providerInfoLinksTextBlock.Visibility = providerInfoLinksTextBlock.Inlines.Count > 0
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void AddProviderInfoLink (
            TextBlock textBlock,
            string label,
            string url )
        {
            if ( string.IsNullOrWhiteSpace( url ) ||
                 !Uri.TryCreate( url, UriKind.Absolute, out var uri ) )
            {
                return;
            }

            if ( textBlock.Inlines.Count > 0 )
            {
                textBlock.Inlines.Add( new Run( "   " ) );
            }

            var hyperlink = new Hyperlink( new Run( label ) )
            {
                NavigateUri = uri
            };
            hyperlink.RequestNavigate += providerInfoLink_RequestNavigate;
            textBlock.Inlines.Add( hyperlink );
        }

        private void providerProfilesList_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            if ( loadingProfile )
            {
                return;
            }

            foreach ( var removedProfile in e.RemovedItems.OfType<WebSpeechProvider>() )
            {
                SaveProfileFromFields( removedProfile );
            }
            LoadSelectedProfile();
        }

        private void providerTypeDropDown_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            if ( loadingProfile )
            {
                return;
            }

            UpdateProviderInfoLinks( providerTypeDropDown.SelectedItem as WebSpeechProviderDescriptor );
        }

        private void newProviderTypeDropDown_SelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            if ( loadingProfile || providerProfilesList.SelectedItem != null )
            {
                return;
            }

            providerTypeDropDown.SelectedItem = newProviderTypeDropDown.SelectedItem;
            UpdateProviderInfoLinks( newProviderTypeDropDown.SelectedItem as WebSpeechProviderDescriptor );
        }

        private void providerInfoLink_RequestNavigate ( object sender, RequestNavigateEventArgs e )
        {
            Process.Start( new ProcessStartInfo( e.Uri.AbsoluteUri ) { UseShellExecute = true } );
            e.Handled = true;
        }

        private void profileField_Changed ( object sender, RoutedEventArgs e )
        {
            if ( loadingProfile )
            {
                return;
            }

            SaveCurrentProfileFromFields();
        }

        private void SaveCurrentProfileFromFields()
        {
            if ( providerProfilesList.SelectedItem is WebSpeechProvider profile )
            {
                SaveProfileFromFields( profile );
            }
        }

        private void SaveProfileFromFields ( WebSpeechProvider profile )
        {
            var descriptor = GetDescriptor( profile.ProviderType );
            profile.DisplayName = string.IsNullOrWhiteSpace( profileNameTextBox.Text )
                ? descriptor?.DisplayName ?? profile.ProviderType
                : profileNameTextBox.Text.Trim();
            profile.Enabled = profileEnabledCheckBox.IsChecked ?? false;
            profile.LocaleFilters = ParseLocaleFilters( profileLocaleFiltersTextBox.Text );

            foreach ( var field in descriptor?.ProfileFields ?? [] )
            {
                if ( !settingControls.TryGetValue( field.Key, out var control ) )
                {
                    continue;
                }

                var value = control switch
                {
                    PasswordBox passwordBox => passwordBox.Password,
                    TextBox textBox => textBox.Text,
                    _ => null
                };
                profile.SetSetting( field.Key, value?.Trim() );
            }
        }

        private void addProfileButton_Click ( object sender, RoutedEventArgs e )
        {
            SaveCurrentProfileFromFields();
            var descriptor = newProviderTypeDropDown.SelectedItem as WebSpeechProviderDescriptor
                             ?? providerTypeDropDown.SelectedItem as WebSpeechProviderDescriptor
                             ?? (providerDescriptors.Count > 0 ? providerDescriptors[0] : null);
            if ( descriptor == null )
            {
                return;
            }

            var profile = SpeechService.Instance.SpeechManager.CreateWebProviderProfile( descriptor.ProviderType );
            profile.DisplayName = NextProfileName( descriptor.DisplayName );
            profiles.Add( profile );
            RefreshProfileList( profile );
        }

        private async void verifyProfileButton_Click ( object sender, RoutedEventArgs e )
        {
            if ( providerProfilesList.SelectedItem is not WebSpeechProvider profile )
            {
                return;
            }

            SaveCurrentProfileFromFields();
            await VerifyProfileAsync( profile.Clone() ).ConfigureAwait( true );
        }

        private async Task VerifyProfileAsync ( WebSpeechProvider profile )
        {
            verifyProfileButton.IsEnabled = false;
            verifyProfileButton.Content = Properties.Resources.web_voice_provider_verifying_button;
            try
            {
                using var timeoutSource = new CancellationTokenSource( TimeSpan.FromSeconds( 20 ) );
                await SpeechService.Instance.SpeechManager
                    .ValidateWebProviderProfileAsync( profile, timeoutSource.Token )
                    .ConfigureAwait( true );

                MessageBox.Show(
                    this,
                    Properties.Resources.web_voice_provider_verify_success,
                    Properties.Resources.web_voice_provider_verify_title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information );
            }
            catch ( Exception )
            {
                MessageBox.Show(
                    this,
                    Properties.Resources.web_voice_provider_verify_failure,
                    Properties.Resources.web_voice_provider_verify_title,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error );
            }
            finally
            {
                verifyProfileButton.Content = Properties.Resources.web_voice_provider_verify_button;
                verifyProfileButton.IsEnabled = providerProfilesList.SelectedItem != null;
            }
        }

        private void clearCredentialsButton_Click ( object sender, RoutedEventArgs e )
        {
            if ( providerProfilesList.SelectedItem is not WebSpeechProvider profile )
            {
                return;
            }

            var result = MessageBox.Show(
                this,
                string.Format(
                    Properties.Resources.web_voice_provider_clear_credentials_confirm,
                    profile.DisplayName ),
                Properties.Resources.web_voice_provider_clear_credentials_title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning );
            if ( result != MessageBoxResult.Yes )
            {
                return;
            }

            foreach ( var field in GetDescriptor( profile.ProviderType )?.ProfileFields.Where( field => field.IsSecret ) ?? [] )
            {
                if ( settingControls.TryGetValue( field.Key, out var control ) )
                {
                    switch ( control )
                    {
                        case PasswordBox passwordBox:
                            passwordBox.Clear();
                            break;
                        case TextBox textBox:
                            textBox.Clear();
                            break;
                    }
                }
            }
            SaveCurrentProfileFromFields();
        }

        private void removeProfileButton_Click ( object sender, RoutedEventArgs e )
        {
            if ( providerProfilesList.SelectedItem is not WebSpeechProvider profile )
            {
                return;
            }

            var result = MessageBox.Show(
                this,
                string.Format(
                    Properties.Resources.web_voice_provider_remove_profile_confirm,
                    profile.DisplayName ),
                Properties.Resources.web_voice_provider_remove_profile_title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning );
            if ( result != MessageBoxResult.Yes )
            {
                return;
            }

            profiles.Remove( profile );
            RefreshProfileList( profiles.FirstOrDefault() );
        }

        private async void saveButton_Click ( object sender, RoutedEventArgs e )
        {
            SaveCurrentProfileFromFields();
            var currentConfiguration = ConfigService.Instance.speechServiceConfiguration;
            var updatedConfiguration = new SpeechServiceConfiguration
            {
                AudioDevice = currentConfiguration.AudioDevice,
                StandardVoice = ResetRemovedProviderVoice( currentConfiguration.StandardVoice ),
                Volume = currentConfiguration.Volume,
                Rate = currentConfiguration.Rate,
                EffectsLevel = currentConfiguration.EffectsLevel,
                DistortOnDamage = currentConfiguration.DistortOnDamage,
                DisableIpa = currentConfiguration.DisableIpa,
                EnableIcao = currentConfiguration.EnableIcao,
                SpeechProviderConfigurations = profiles
                    .Select( profile => profile.Clone() )
                    .ToList()
            };

            ConfigService.Instance.speechServiceConfiguration = updatedConfiguration;
            ConfigService.Instance.SaveConfiguration( updatedConfiguration );

            try
            {
                await SpeechService.Instance.SpeechManager.ReloadVoicesAsync().ConfigureAwait( true );
            }
            catch ( Exception ex )
            {
                Logging.Warn( "Failed to reload text-to-speech voices after saving provider profiles.", ex );
            }

            DialogResult = true;
            Close();
        }

        private void cancelButton_Click ( object sender, RoutedEventArgs e )
        {
            DialogResult = false;
            Close();
        }

        private string ResetRemovedProviderVoice ( string standardVoice )
        {
            if ( string.IsNullOrWhiteSpace( standardVoice ) )
            {
                return standardVoice;
            }

            var parts = standardVoice.Split( ':' );
            if ( parts.Length < 3 )
            {
                return standardVoice;
            }

            var providerType = parts[0];
            var profileId = parts[1];
            return profiles.Any( profile =>
                string.Equals( profile.ProviderType, providerType, StringComparison.InvariantCultureIgnoreCase ) &&
                string.Equals( profile.Id, profileId, StringComparison.InvariantCultureIgnoreCase ) )
                ? standardVoice
                : null;
        }

        private string NextProfileName ( string baseName )
        {
            if ( profiles.All( profile =>
                    !string.Equals( profile.DisplayName, baseName, StringComparison.InvariantCultureIgnoreCase ) ) )
            {
                return baseName;
            }

            var suffix = 2;
            while ( profiles.Any( profile =>
                       string.Equals( profile.DisplayName, $"{baseName} {suffix}", StringComparison.InvariantCultureIgnoreCase ) ) )
            {
                suffix++;
            }
            return $"{baseName} {suffix}";
        }

        private WebSpeechProviderDescriptor GetDescriptor ( string providerType )
        {
            return providerDescriptors.FirstOrDefault( descriptor =>
                string.Equals( descriptor.ProviderType, providerType, StringComparison.InvariantCultureIgnoreCase ) );
        }

        private static string ProviderFieldLabel (
            WebSpeechProviderDescriptor descriptor,
            WebSpeechProviderProfileField field )
        {
            var resourceKey = $"web_voice_provider_{descriptor.ProviderType}_{field.Key}_label";
            var localized = Properties.Resources.ResourceManager.GetString( resourceKey );
            return string.IsNullOrWhiteSpace( localized ) ? field.DisplayName : localized;
        }

        private static List<string> ParseLocaleFilters ( string localeFilters )
        {
            return ( localeFilters ?? string.Empty )
                .Split( [ ',', ';', ' ', '\r', '\n', '\t' ], StringSplitOptions.RemoveEmptyEntries )
                .Select( filter => filter.Trim() )
                .Where( filter => !string.IsNullOrWhiteSpace( filter ) )
                .Distinct( StringComparer.InvariantCultureIgnoreCase )
                .ToList();
        }
    }
}

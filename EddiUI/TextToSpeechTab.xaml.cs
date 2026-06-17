using EddiConfigService;
using EddiConfigService.Configurations;
using EddiDataDefinitions;
using EddiSpeechService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using Utilities;

namespace EddiUI
{
    public partial class TextToSpeechTab : UserControl
    {
        private bool isConfiguring;

        public TextToSpeechTab ()
        {
            InitializeComponent();
            manageWebVoiceProvidersButton.Content = Properties.Resources.tab_tts_manage_web_voice_providers_button;
            ConfigureTTS();
            this.Loaded += TextToSpeechTab_Loaded;
            this.Unloaded += TextToSpeechTab_Unloaded;
        }

        private void TextToSpeechTab_Loaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                var helper = new WindowInteropHelper(window);
                var source = HwndSource.FromHwnd(helper.Handle);
                source?.AddHook(HwndMessageHook);
            }
        }

        private void TextToSpeechTab_Unloaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                var helper = new WindowInteropHelper(window);
                var source = HwndSource.FromHwnd(helper.Handle);
                source?.RemoveHook(HwndMessageHook);
            }
        }

        private IntPtr HwndMessageHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_DEVICECHANGE = 0x0219;
            if (msg == WM_DEVICECHANGE)
            {
                RefreshAudioDevices();
            }
            return IntPtr.Zero;
        }

        private void RefreshAudioDevices()
        {
            var activeDevices = AudioDeviceService.GetAudioDevices();
            if ( ttsAudioDeviceDropDown.ItemsSource is not List<AudioDevice> currentOptions ) { return; }

            var currentIds = currentOptions.Skip(1).Select(d => d.Id).ToList();
            var activeIds = activeDevices.Select(d => d.Id).ToList();

            bool changed = currentIds.Count != activeIds.Count || !currentIds.SequenceEqual(activeIds);

            if (changed)
            {
                var speechServiceConfiguration = ConfigService.Instance.speechServiceConfiguration;
                var configuredDevice = speechServiceConfiguration?.AudioDevice;

                var audioDeviceOptions = new List<AudioDevice>
                {
                    new() { Name = Properties.Resources.tts_default_audio_device, Id = null }
                };
                audioDeviceOptions.AddRange(activeDevices);

                ttsAudioDeviceDropDown.SelectionChanged -= ttsAudioDeviceDropDownUpdated;
                ttsAudioDeviceDropDown.ItemsSource = audioDeviceOptions;

                if (configuredDevice != null && audioDeviceOptions.Any(d => d.Id == configuredDevice))
                {
                    ttsAudioDeviceDropDown.SelectedValue = configuredDevice;
                }
                else
                {
                    ttsAudioDeviceDropDown.SelectedIndex = 0;
                    if (configuredDevice != null)
                    {
                        speechServiceConfiguration.AudioDevice = null;
                        ConfigService.Instance.speechServiceConfiguration = speechServiceConfiguration;
                    }
                }
                ttsAudioDeviceDropDown.SelectionChanged += ttsAudioDeviceDropDownUpdated;
            }
        }

        public void ConfigureTTS()
        {
            try
            {
                isConfiguring = true;
                var speechServiceConfiguration = ConfigService.Instance.speechServiceConfiguration;
                _ = speechServiceConfiguration.SpeechProviderConfigurations;

                // Populate audio devices
                var audioDeviceOptions = new List<AudioDevice>
                {
                    new() { Name = Properties.Resources.tts_default_audio_device, Id = null }
                };
                audioDeviceOptions.AddRange(AudioDeviceService.GetAudioDevices());
                ttsAudioDeviceDropDown.ItemsSource = audioDeviceOptions;
                var configuredDevice = speechServiceConfiguration.AudioDevice;
                if (configuredDevice != null && audioDeviceOptions.Any(d => d.Id == configuredDevice))
                {
                    ttsAudioDeviceDropDown.SelectedValue = configuredDevice;
                }
                else
                {
                    ttsAudioDeviceDropDown.SelectedIndex = 0;
                    if (configuredDevice != null)
                    {
                        speechServiceConfiguration.AudioDevice = null;
                        ConfigService.Instance.speechServiceConfiguration = speechServiceConfiguration;
                    }
                }

                var speechOptions = new List<VoiceOption>
                {
                    new() { Value = null, DisplayName = Properties.Resources.tts_default_voice }
                };
                try
                {
                    SpeechService.Instance.SpeechManager.InitializeAsync().GetResultOrTimeout( TimeSpan.FromSeconds( 15 ) );
                    var voicesList = new List<VoiceOption>();
                    foreach (var voice in SpeechService.Instance.SpeechManager.validatedVoices)
                    {
                        if (voice.hideVoice) continue;
                        if (voice.synthType == nameof(System) && voice.name.Contains( "Online", StringComparison.OrdinalIgnoreCase ) )
                        {
                            continue;
                        }
                        voicesList.Add(new VoiceOption
                        {
                            Value = voice.voiceKey,
                            DisplayName = GetFriendlyVoiceName(voice)
                        });
                    }

                    MakeDuplicateVoiceNamesDistinct(voicesList);
                    voicesList = voicesList.OrderBy(v => v.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToList();
                    speechOptions.AddRange(voicesList);

                    if ( speechOptions.Count == 1 )
                    {
                        Logging.Warn( "No speech synthesis voices were available." );
                    }
                    ttsVoiceDropDown.ItemsSource = speechOptions;
                    SelectConfiguredVoice(speechOptions, speechServiceConfiguration);
                }
                catch (Exception e)
                {
                    Logging.Warn( "Failed to enumerate text-to-speech voices.", e );
                    ttsVoiceDropDown.ItemsSource = speechOptions;
                    ttsVoiceDropDown.SelectedIndex = 0;
                }

                ttsVolumeSlider.Value = speechServiceConfiguration.Volume;
                ttsRateSlider.Value = speechServiceConfiguration.Rate;
                ttsEffectsLevelSlider.Value = speechServiceConfiguration.EffectsLevel;
                ttsDistortCheckbox.IsChecked = speechServiceConfiguration.DistortOnDamage;
                DisableIpaCheckbox.IsChecked = speechServiceConfiguration.DisableIpa;
                enableIcaoCheckbox.IsChecked = speechServiceConfiguration.EnableIcao;

                ttsTestShipDropDown.ItemsSource = ShipDefinitions.ShipModels; // already sorted
                ttsTestShipDropDown.Text = "Adder";
            }
            finally
            {
                isConfiguring = false;
            }
        }

        private void SelectConfiguredVoice ( List<VoiceOption> speechOptions, SpeechServiceConfiguration configuration )
        {
            var configuredVoice = configuration.StandardVoice;
            var selectedOption = speechOptions.FirstOrDefault( v =>
                string.Equals( v.Value, configuredVoice, StringComparison.InvariantCultureIgnoreCase ) );

            if ( selectedOption == null && !string.IsNullOrWhiteSpace( configuredVoice ) )
            {
                var legacyMatches = SpeechService.Instance.SpeechManager.validatedVoices
                    .Where( v => string.Equals( v.name, configuredVoice, StringComparison.InvariantCultureIgnoreCase ) )
                    .Take( 2 )
                    .ToList();
                if ( legacyMatches.Count == 1 )
                {
                    selectedOption = speechOptions.FirstOrDefault( v =>
                        string.Equals( v.Value, legacyMatches[0].voiceKey, StringComparison.InvariantCultureIgnoreCase ) );
                }
            }

            if ( selectedOption != null )
            {
                ttsVoiceDropDown.SelectedItem = selectedOption;
                if ( configuration.StandardVoice != selectedOption.Value )
                {
                    configuration.StandardVoice = selectedOption.Value;
                    ConfigService.Instance.speechServiceConfiguration = configuration;
                }
                return;
            }

            ttsVoiceDropDown.SelectedIndex = 0;
            if ( configuration.StandardVoice != null )
            {
                configuration.StandardVoice = null;
                ConfigService.Instance.speechServiceConfiguration = configuration;
            }
        }

        private static void MakeDuplicateVoiceNamesDistinct ( IReadOnlyCollection<VoiceOption> voiceOptions )
        {
            foreach ( var group in voiceOptions
                         .Where( option => !string.IsNullOrWhiteSpace( option.Value ) )
                         .GroupBy( option => option.DisplayName )
                         .Where( group => group.Count() > 1 ) )
            {
                foreach ( var option in group )
                {
                    option.DisplayName = $"{option.DisplayName} ({option.Value})";
                }
            }
        }

        private void ttsAudioDeviceDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            if (sender is FrameworkElement element && element.IsLoaded && !isConfiguring )
            {
                ttsUpdated();
            }
        }

        private void manageWebVoiceProvidersButton_Click(object sender, RoutedEventArgs e)
        {
            var owner = Window.GetWindow(this);
            var dialog = new WebVoiceProvidersWindow
            {
                Owner = owner
            };
            if ( dialog.ShowDialog() == true )
            {
                ConfigureTTS();
            }
        }

        private void ttsVoiceDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            if (sender is FrameworkElement element && element.IsLoaded && !isConfiguring )
            {
                ttsUpdated();
            }
        }

        private void ttsEffectsLevelUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is FrameworkElement element && element.IsLoaded && !isConfiguring )
            {
                ttsUpdated();
            }
        }

        private void ttsDistortionLevelUpdated(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.IsLoaded && !isConfiguring )
            {
                ttsUpdated();
            }
        }

        private void ttsRateUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is FrameworkElement element && element.IsLoaded && !isConfiguring )
            {
                ttsUpdated();
            }
        }

        private void ttsVolumeUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is FrameworkElement element && element.IsLoaded && !isConfiguring )
            {
                ttsUpdated();
            }
        }

        private async void ttsTestVoiceButtonClickedAsync ( object sender, RoutedEventArgs e )
        {
            try
            {
                if ( SpeechService.Instance.eddiAudioPlaying || SpeechService.Instance.eddiSpeaking )
                {
                    SpeechService.Instance.StopAudio();
                    SpeechService.Instance.ShutUp();
                    return;
                }

                var testShip = ShipDefinitions.FromModel( (string)ttsTestShipDropDown.SelectedItem );
                testShip.health = 100;
                var message = string.Format( Properties.Resources.voice_test_ship,
                    ShipDefinitions.FromModel( (string)ttsTestShipDropDown.SelectedItem ).SpokenModel() );
                var selectedVoice = ttsVoiceDropDown.SelectedValue as string;
                Logging.Debug($"Test Voice button clicked. Selected voice: '{selectedVoice}', Ship: '{testShip?.model}'");
                await SpeechService.Instance.SayAsync( testShip, message, 0, selectedVoice ).ConfigureAwait( false );
            }
            catch ( Exception ex )
            {
                Logging.Warn( ex.Message, ex );
            }
        }

        private async void ttsTestDamagedVoiceButtonClickedAsync(object sender, RoutedEventArgs e)
        {
            try
            {
                if ( SpeechService.Instance.eddiAudioPlaying || SpeechService.Instance.eddiSpeaking )
                {
                    SpeechService.Instance.StopAudio();
                    SpeechService.Instance.ShutUp();
                    return;
                }
                
                var testShip = ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedItem);
                testShip.health = 20;
                var message = string.Format(Properties.Resources.voice_test_damage, ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedItem).SpokenModel());
                var selectedVoice = ttsVoiceDropDown.SelectedValue as string;
                Logging.Debug($"Test Damaged Voice button clicked. Selected voice: '{selectedVoice}', Ship: '{testShip?.model}'");
                await SpeechService.Instance.SayAsync( testShip, message, 0, selectedVoice ).ConfigureAwait( false );
            }
            catch ( Exception ex )
            {
                Logging.Warn( ex.Message, ex );
            }
        }

        private void disableIpaUpdated(object sender, RoutedEventArgs e)
        {
            if ( !isConfiguring )
            {
                ttsUpdated();
            }
        }

        private void enableICAOUpdated(object sender, RoutedEventArgs e)
        {
            if ( !isConfiguring )
            {
                ttsUpdated();
            }
        }

        /// <summary>
        /// fetch the Text-to-Speech Configuration and write it to File
        /// </summary>
        private void ttsUpdated()
        {
            var currentConfiguration = ConfigService.Instance.speechServiceConfiguration;
            var speechConfiguration = new SpeechServiceConfiguration
            {
                AudioDevice = ttsAudioDeviceDropDown.SelectedValue?.ToString(),
                StandardVoice = ttsVoiceDropDown.SelectedValue as string,
                Volume = (int)ttsVolumeSlider.Value,
                Rate = (int)ttsRateSlider.Value,
                EffectsLevel = (int)ttsEffectsLevelSlider.Value,
                DistortOnDamage = ttsDistortCheckbox.IsChecked ?? false,
                DisableIpa = DisableIpaCheckbox.IsChecked ?? false,
                EnableIcao = enableIcaoCheckbox.IsChecked ?? false,
                SpeechProviderConfigurations = currentConfiguration.SpeechProviderConfigurations
                    .Select( profile => profile.Clone() )
                    .ToList()
            };
            ConfigService.Instance.speechServiceConfiguration = speechConfiguration;
        }

        private static string GetFriendlyVoiceName(VoiceDetails voice)
        {
            if ( !string.IsNullOrWhiteSpace( voice.providerProfileId ) )
            {
                var cultureName = voice.cultureinvariantname ?? "Unknown Language";
                var simpleName = voice.name;
                var lastDashIndex = voice.name.LastIndexOf('-');
                if (lastDashIndex >= 0 && lastDashIndex < voice.name.Length - 1)
                {
                    simpleName = voice.name.Substring(lastDashIndex + 1);
                }
                if (simpleName.EndsWith("MultilingualNeural", StringComparison.OrdinalIgnoreCase))
                {
                    simpleName = string.Concat( simpleName.AsSpan(0, simpleName.Length - "MultilingualNeural".Length), " (Multilingual)" );
                }
                else if (simpleName.EndsWith("Neural", StringComparison.OrdinalIgnoreCase))
                {
                    simpleName = simpleName.Substring(0, simpleName.Length - "Neural".Length);
                }
                var providerName = string.IsNullOrWhiteSpace( voice.providerDisplayName )
                    ? voice.synthType
                    : voice.providerDisplayName;
                return $"{cultureName} {simpleName} - Neural [{providerName}]";
            }
            else if (voice.synthType == nameof(System) && voice.name.StartsWith("Microsoft ", StringComparison.OrdinalIgnoreCase))
            {
                var cultureName = voice.cultureinvariantname ?? "Unknown Language";
                var simpleName = voice.name.Substring("Microsoft ".Length);
                if (simpleName.EndsWith(" Desktop", StringComparison.OrdinalIgnoreCase))
                {
                    simpleName = string.Concat( simpleName.AsSpan(0, simpleName.Length - " Desktop".Length), " (Desktop)" );
                }
                else if (simpleName.EndsWith(" Online", StringComparison.OrdinalIgnoreCase))
                {
                    simpleName = string.Concat( simpleName.AsSpan(0, simpleName.Length - " Online".Length), " (Online)" );
                }
                return $"{cultureName} {simpleName} - Local";
            }
            return voice.name;
        }

        public class VoiceOption
        {
            public string Value { get; set; }
            public string DisplayName { get; set; }
            public override string ToString() => Value;
        }
    }
}

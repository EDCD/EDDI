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
        public TextToSpeechTab ()
        {
            InitializeComponent();
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
            var speechServiceConfiguration = ConfigService.Instance.speechServiceConfiguration;

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
                new VoiceOption { Value = "Windows TTS default", DisplayName = Properties.Resources.tts_default_voice }
            };
            try
            {
                SpeechService.Instance.SpeechManager.InitializeAsync().GetResultOrTimeout( TimeSpan.FromSeconds( 15 ) );
                var voicesList = new List<VoiceOption>();
                foreach (var voice in SpeechService.Instance.SpeechManager.validatedVoices)
                {
                    if (voice.hideVoice) continue;
                    if (voice.synthType == "System" && voice.name.IndexOf("Online", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        continue;
                    }
                    voicesList.Add(new VoiceOption { Value = voice.name, DisplayName = GetFriendlyVoiceName(voice) });
                }

                // Sort the voices alphabetically by DisplayName
                voicesList = voicesList.OrderBy(v => v.DisplayName, StringComparer.CurrentCultureIgnoreCase).ToList();
                speechOptions.AddRange(voicesList);

                if ( speechOptions.Count == 1 )
                {
                    Logging.Warn( "No speech synthesis voices were available." );
                }
                ttsVoiceDropDown.ItemsSource = speechOptions;
                ttsVoiceDropDown.SelectedValue =  speechOptions.Any(v => v.Value == speechServiceConfiguration.StandardVoice) 
                    ? speechServiceConfiguration.StandardVoice
                    : Properties.Resources.tts_default_voice;

                // If the prior selected voice is no longer a valid option, we revert to the system default.
                if (speechServiceConfiguration.StandardVoice != (string)ttsVoiceDropDown.SelectedValue)
                {
                    speechServiceConfiguration.StandardVoice = (string)ttsVoiceDropDown.SelectedValue;
                    ConfigService.Instance.speechServiceConfiguration = speechServiceConfiguration;
                }
            }
            catch (Exception e)
            {
                Logging.Warn( "Failed to enumerate text-to-speech voices.", e );
                ttsVoiceDropDown.ItemsSource = speechOptions;
                ttsVoiceDropDown.Text = Properties.Resources.tts_default_voice;
            }
            ttsVolumeSlider.Value = speechServiceConfiguration.Volume;
            ttsRateSlider.Value = speechServiceConfiguration.Rate;
            ttsEffectsLevelSlider.Value = speechServiceConfiguration.EffectsLevel;
            ttsDistortCheckbox.IsChecked = speechServiceConfiguration.DistortOnDamage;
            DisableIpaCheckbox.IsChecked = speechServiceConfiguration.DisableIpa;
            enableIcaoCheckbox.IsChecked = speechServiceConfiguration.EnableIcao;

            ttsTestShipDropDown.ItemsSource = ShipDefinitions.ShipModels; // already sorted
            ttsTestShipDropDown.Text = "Adder";

            ttsAzureApiKey.Password = speechServiceConfiguration.AzureApiKey;
            ttsAzureRegion.Text = speechServiceConfiguration.AzureRegion;
        }

        private void ttsAudioDeviceDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            if (sender is FrameworkElement element && element.IsLoaded )
            {
                ttsUpdated();
            }
        }

        private async void ttsAzureSaveButton_Click(object sender, RoutedEventArgs e)
        {
            var speechServiceConfiguration = ConfigService.Instance.speechServiceConfiguration;
            speechServiceConfiguration.AzureApiKey = ttsAzureApiKey.Password?.Trim();
            speechServiceConfiguration.AzureRegion = ttsAzureRegion.Text?.Trim();
            ConfigService.Instance.speechServiceConfiguration = speechServiceConfiguration;
            ConfigService.Instance.SaveConfiguration(speechServiceConfiguration);

            ttsAzureSaveButton.IsEnabled = false;
            ttsAzureSaveButton.Content = "Saving...";

            try
            {
                await SpeechService.Instance.SpeechManager.ReloadVoicesAsync();
                ConfigureTTS();
                
                var azureVoices = SpeechService.Instance.validatedVoices.Where(v => v.synthType == "Azure").ToList();
                if (azureVoices.Count > 0)
                {
                    var testVoice = azureVoices.FirstOrDefault(v => v.name.Contains("Sonia")) ?? azureVoices.First();
                    _ = SpeechService.Instance.SayAsync(null, "Azure Speech Services are successfully configured and verified!", 0, testVoice.name);
                }

                MessageBox.Show("Azure Speech configuration saved and voices reloaded successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to reload Azure voices: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ttsAzureSaveButton.IsEnabled = true;
                ttsAzureSaveButton.Content = "Save & Verify";
            }
        }

        private void ttsVoiceDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            if (sender is FrameworkElement element && element.IsLoaded )
            {
                ttsUpdated();
            }
        }

        private void ttsEffectsLevelUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is FrameworkElement element && element.IsLoaded )
            {
                ttsUpdated();
            }
        }

        private void ttsDistortionLevelUpdated(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.IsLoaded )
            {
                ttsUpdated();
            }
        }

        private void ttsRateUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is FrameworkElement element && element.IsLoaded )
            {
                ttsUpdated();
            }
        }

        private void ttsVolumeUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is FrameworkElement element && element.IsLoaded )
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
                var selectedVoice = ttsVoiceDropDown.SelectedItem == null || 
                                    ttsVoiceDropDown.SelectedItem.ToString() == "Windows TTS default" 
                    ? null 
                    : ttsVoiceDropDown.SelectedItem.ToString();
                Logging.Info($"Test Voice button clicked. Selected voice: '{selectedVoice}', Ship: '{testShip?.model}'");
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
                var selectedVoice = ttsVoiceDropDown.SelectedItem == null || 
                                    ttsVoiceDropDown.SelectedItem.ToString() == "Windows TTS default" 
                    ? null 
                    : ttsVoiceDropDown.SelectedItem.ToString();
                Logging.Info($"Test Damaged Voice button clicked. Selected voice: '{selectedVoice}', Ship: '{testShip?.model}'");
                await SpeechService.Instance.SayAsync( testShip, message, 0, selectedVoice ).ConfigureAwait( false );
            }
            catch ( Exception ex )
            {
                Logging.Warn( ex.Message, ex );
            }
        }

        private void disableIpaUpdated(object sender, RoutedEventArgs e)
        {
            ttsUpdated();
        }

        private void enableICAOUpdated(object sender, RoutedEventArgs e)
        {
            ttsUpdated();
        }

        /// <summary>
        /// fetch the Text-to-Speech Configuration and write it to File
        /// </summary>
        private void ttsUpdated()
        {
            var speechConfiguration = new SpeechServiceConfiguration
            {
                AudioDevice = ttsAudioDeviceDropDown.SelectedValue?.ToString(),
                StandardVoice = ttsVoiceDropDown.SelectedItem == null || 
                                ttsVoiceDropDown.SelectedItem.ToString() == Properties.Resources.tts_default_voice 
                    ? null 
                    : ttsVoiceDropDown.SelectedItem.ToString(),
                Volume = (int)ttsVolumeSlider.Value,
                Rate = (int)ttsRateSlider.Value,
                EffectsLevel = (int)ttsEffectsLevelSlider.Value,
                DistortOnDamage = ttsDistortCheckbox.IsChecked ?? false,
                DisableIpa = DisableIpaCheckbox.IsChecked ?? false,
                EnableIcao = enableIcaoCheckbox.IsChecked ?? false,
                AzureApiKey = ttsAzureApiKey.Password?.Trim(),
                AzureRegion = ttsAzureRegion.Text?.Trim()
            };
            ConfigService.Instance.speechServiceConfiguration = speechConfiguration;
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
                e.Handled = true;
            }
            catch (Exception ex)
            {
                Logging.Warn("Failed to open hyperlink: " + ex.Message, ex);
            }
        }

        private static string GetFriendlyVoiceName(VoiceDetails voice)
        {
            if (voice.synthType == "Azure")
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
                    simpleName = simpleName.Substring(0, simpleName.Length - "MultilingualNeural".Length) + " (Multilingual)";
                }
                else if (simpleName.EndsWith("Neural", StringComparison.OrdinalIgnoreCase))
                {
                    simpleName = simpleName.Substring(0, simpleName.Length - "Neural".Length);
                }
                return $"{cultureName} {simpleName} - Neural";
            }
            else if (voice.synthType == "System" && voice.name.StartsWith("Microsoft ", StringComparison.OrdinalIgnoreCase))
            {
                var cultureName = voice.cultureinvariantname ?? "Unknown Language";
                var simpleName = voice.name.Substring("Microsoft ".Length);
                if (simpleName.EndsWith(" Desktop", StringComparison.OrdinalIgnoreCase))
                {
                    simpleName = simpleName.Substring(0, simpleName.Length - " Desktop".Length) + " (Desktop)";
                }
                else if (simpleName.EndsWith(" Online", StringComparison.OrdinalIgnoreCase))
                {
                    simpleName = simpleName.Substring(0, simpleName.Length - " Online".Length) + " (Online)";
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

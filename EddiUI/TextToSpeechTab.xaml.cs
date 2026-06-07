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

            var speechOptions = new List<string>
            {
                Properties.Resources.tts_default_voice
            };
            try
            {
                SpeechService.Instance.SpeechManager.InitializeAsync().GetResultOrTimeout( TimeSpan.FromSeconds( 5 ) );
                foreach (var voice in SpeechService.Instance.displayedVoiceNames)
                {
                    speechOptions.Add(voice);
                }
                if ( speechOptions.Count == 1 )
                {
                    Logging.Warn( "No speech synthesis voices were available." );
                }
                ttsVoiceDropDown.ItemsSource = speechOptions;
                ttsVoiceDropDown.Text =  speechOptions.Any(v => v == speechServiceConfiguration.StandardVoice) 
                    ? speechServiceConfiguration.StandardVoice
                    : Properties.Resources.tts_default_voice;

                // If the prior selected voice is no longer a valid option, we revert to the system default.
                if (speechServiceConfiguration.StandardVoice != ttsVoiceDropDown.Text)
                {
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
        }

        private void ttsAudioDeviceDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            if (sender is FrameworkElement element && element.IsLoaded )
            {
                ttsUpdated();
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
                await SpeechService.Instance.SayAsync( testShip, message, 0 ).ConfigureAwait( false );
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
                await SpeechService.Instance.SayAsync( testShip, message, 0 ).ConfigureAwait( false );
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
                EnableIcao = enableIcaoCheckbox.IsChecked ?? false
            };
            ConfigService.Instance.speechServiceConfiguration = speechConfiguration;
        }
    }
}

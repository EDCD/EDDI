using EddiDataDefinitions;
using EddiSpeechService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Utilities;

namespace EddiUI
{
    public partial class TextToSpeechTab : UserControl
    {
        public TextToSpeechTab ()
        {
            InitializeComponent();
            ConfigureTTS();
        }

        public void ConfigureTTS()
        {
            var speechServiceConfiguration = SpeechServiceConfiguration.FromFile();
            var speechOptions = new List<string>
            {
                "Windows TTS default"
            };
            try
            {
                foreach (var voice in SpeechService.Instance.allvoices)
                {
                    speechOptions.Add(voice);
                }
                ttsVoiceDropDown.ItemsSource = speechOptions;
                ttsVoiceDropDown.Text =  speechOptions.Any(v => v == speechServiceConfiguration.StandardVoice) 
                    ? speechServiceConfiguration.StandardVoice
                    : "Windows TTS default";

                // If the prior selected voice is no longer a valid option, we revert to the system default.
                if (speechServiceConfiguration.StandardVoice != ttsVoiceDropDown.Text)
                {
                    speechServiceConfiguration.ToFile();
                }
            }
            catch (Exception e)
            {
                Logging.Warn("" + Thread.CurrentThread.ManagedThreadId + ": Caught exception " + e);
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

        private void ttsVoiceDropDownUpdated(object sender, SelectionChangedEventArgs e)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.IsLoaded)
            {
                ttsUpdated();
            }
        }

        private void ttsEffectsLevelUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.IsLoaded)
            {
                ttsUpdated();
            }
        }

        private void ttsDistortionLevelUpdated(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.IsLoaded)
            {
                ttsUpdated();
            }
        }

        private void ttsRateUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.IsLoaded)
            {
                ttsUpdated();
            }
        }

        private void ttsVolumeUpdated(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is FrameworkElement frameworkElement && frameworkElement.IsLoaded)
            {
                ttsUpdated();
            }
        }

        private void ttsTestVoiceButtonClicked(object sender, RoutedEventArgs e)
        {
            Ship testShip = ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedItem);
            testShip.health = 100;
            string message = String.Format(Properties.Resources.voice_test_ship, ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedItem).SpokenModel());
            SpeechService.Instance.Say(testShip, message, 0);
        }

        private void ttsTestDamagedVoiceButtonClicked(object sender, RoutedEventArgs e)
        {
            Ship testShip = ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedItem);
            testShip.health = 20;
            string message = String.Format(Properties.Resources.voice_test_damage, ShipDefinitions.FromModel((string)ttsTestShipDropDown.SelectedItem).SpokenModel());
            SpeechService.Instance.Say(testShip, message, 0);
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
            SpeechServiceConfiguration speechConfiguration = new SpeechServiceConfiguration
            {
                StandardVoice = ttsVoiceDropDown.SelectedItem == null || 
                                ttsVoiceDropDown.SelectedItem.ToString() == "Windows TTS default" 
                    ? null 
                    : ttsVoiceDropDown.SelectedItem.ToString(),
                Volume = (int)ttsVolumeSlider.Value,
                Rate = (int)ttsRateSlider.Value,
                EffectsLevel = (int)ttsEffectsLevelSlider.Value,
                DistortOnDamage = ttsDistortCheckbox.IsChecked ?? false,
                DisableIpa = DisableIpaCheckbox.IsChecked ?? false,
                EnableIcao = enableIcaoCheckbox.IsChecked ?? false
            };
            SpeechService.Instance.Configuration = speechConfiguration;
            speechConfiguration.ToFile();
        }
    }
}

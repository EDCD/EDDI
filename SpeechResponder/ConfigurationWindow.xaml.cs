﻿using Eddi;
using EddiEvents;
using EddiJournalMonitor;
using EddiShipMonitor;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Utilities;

namespace EddiSpeechResponder
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<Personality> personalities;
        public ObservableCollection<Personality> Personalities
        {
            get { return personalities; }
            set { personalities = value; OnPropertyChanged("Personalities"); }
        }
        private Personality personality;
        public Personality Personality
        {
            get { return personality; }
            set
            {
                personality = value;
                viewEditContent = value != null && value.IsEditable ? "Edit" : "View";
                OnPropertyChanged("Personality");
            }
        }
        public string viewEditContent = "View";

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ConfigurationWindow()
        {
            InitializeComponent();
            I18NForComponents();
            DataContext = this;

            ObservableCollection<Personality> personalities = new ObservableCollection<Personality>();
            // Add our default personality
            personalities.Add(Personality.Default());
            foreach (Personality personality in Personality.AllFromDirectory())
            {
                personalities.Add(personality);
            }
            // Add local personalities
            foreach (Personality personality in personalities)
            {
                Logging.Debug("Found personality " + personality.Name);
            }
            Personalities = personalities;

            SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
            subtitlesCheckbox.IsChecked = configuration.Subtitles;
            subtitlesOnlyCheckbox.IsChecked = configuration.SubtitlesOnly;

            foreach (Personality personality in Personalities)
            {
                if (personality.Name == configuration.Personality)
                {
                    Personality = personality;
                    personalityDefaultTxt(personality);
                    break;
                }
            }
        }

        private void I18NForComponents()
        {
            subsLabel.Text = I18N.GetString("speech_responder_subs");
            subsOnlyLabel.Text = I18N.GetString("speech_responder_subs_only");
            defaultText.Text = I18N.GetString("speech_responder_default_text");
            activeLabel.Text = I18N.GetString("speech_responder_active_personnality");
            newBtn.Content = I18N.GetString("speech_responder_button_new");
            copyBtn.Content = I18N.GetString("speech_responder_button_copy");
            deleteBtn.Content = I18N.GetString("speech_responder_button_delete");
            scriptsData.Columns[0].Header = I18N.GetString("speech_responder_header_name");
            scriptsData.Columns[1].Header = I18N.GetString("speech_responder_header_enabled");
            scriptsData.Columns[2].Header = I18N.GetString("speech_responder_header_description");
        }

        private void eddiScriptsUpdated(object sender, RoutedEventArgs e)
        {
            updateScriptsConfiguration();
        }

        private void eddiScriptsUpdated(object sender, DataTransferEventArgs e)
        {
            updateScriptsConfiguration();
        }

        private void editScript(object sender, RoutedEventArgs e)
        {
            Script script = ((KeyValuePair<string, Script>)((Button)e.Source).DataContext).Value;
            EditScriptWindow editScriptWindow = new EditScriptWindow(Personality.Scripts, script.Name);
            editScriptWindow.ShowDialog();
            scriptsData.Items.Refresh();
        }

        private void viewScript(object sender, RoutedEventArgs e)
        {
            Script script = ((KeyValuePair<string, Script>)((Button)e.Source).DataContext).Value;
            ViewScriptWindow viewScriptWindow = new ViewScriptWindow(Personality.Scripts, script.Name);
            viewScriptWindow.Show();
        }

        private void testScript(object sender, RoutedEventArgs e)
        {
            Script script = ((KeyValuePair<string, Script>)((Button)e.Source).DataContext).Value;
            SpeechResponder responder = new SpeechResponder();
            responder.Start();
            // See if we have a sample
            List<Event> sampleEvents;
            object sample = Events.SampleByName(script.Name);
            if (sample == null)
            {
                sampleEvents = new List<Event>();
            }
            else if (sample is string)
            {
                // It's as tring so a journal entry.  Parse it
                sampleEvents = JournalMonitor.ParseJournalEntry((string)sample);
            }
            else if (sample is Event)
            {
                // It's a direct event
                sampleEvents = new List<Event>() { (Event)sample };
            }
            else
            {
                Logging.Warn("Unknown sample type " + sample.GetType());
                sampleEvents = new List<Event>();
            }

            ScriptResolver scriptResolver = new ScriptResolver(Personality.Scripts);
            if (sampleEvents.Count == 0)
            {
                sampleEvents.Add(null);
            }
            foreach (Event sampleEvent in sampleEvents)
            {
                responder.Say(scriptResolver, ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip(), script.Name, sampleEvent, null, null, false);
            }
        }

        private void deleteScript(object sender, RoutedEventArgs e)
        {
            Script script = ((KeyValuePair<string, Script>)((Button)e.Source).DataContext).Value;
            string messageBoxText = I18N.GetStringWithArgs("speech_responder_delete_script_message", new string[]{script.Name});
            string caption = I18N.GetString("speech_responder_delete_script_caption");
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    // Remove the script from the list
                    Personality.Scripts.Remove(script.Name);
                    Personality.ToFile();
                    EDDI.Instance.Reload("Speech responder");
                    // We updated a property of the personality but not the personality itself so need to manually update items
                    scriptsData.Items.Refresh();
                    break;
            }
        }

        private void resetScript(object sender, RoutedEventArgs e)
        {
            Script script = ((KeyValuePair<string, Script>)((Button)e.Source).DataContext).Value;
            script.Value = null;
            eddiScriptsUpdated(sender, e);
            scriptsData.Items.Refresh();
        }

        private void updateScriptsConfiguration()
        {
            if (Personality != null)
            {
                Personality.ToFile();
                EDDI.Instance.Reload("Speech responder");
            }
        }

        private void personalityChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Personality != null)
            {
                personalityDefaultTxt(Personality);
                SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
                configuration.Personality = Personality.Name;
                configuration.ToFile();
                EDDI.Instance.Reload("Speech responder");
            }
        }

        private void newScriptClicked(object sender, RoutedEventArgs e)
        {
            string baseName = "New function";
            string scriptName = baseName;
            int i = 2;
            while (Personality.Scripts.ContainsKey(scriptName))
            {
                scriptName = baseName + " " + i++;
            }
            Script script = new Script(scriptName, null, false, null);
            Personality.Scripts.Add(script.Name, script);

            // Now fire up an edit
            EditScriptWindow editScriptWindow = new EditScriptWindow(Personality.Scripts, script.Name);
            if (editScriptWindow.ShowDialog() == true)
            {
                Personality.ToFile();
                EDDI.Instance.Reload("Speech responder");
            }
            else
            {
                Personality.Scripts.Remove(script.Name);
            }
            scriptsData.Items.Refresh();
        }

        private void copyPersonalityClicked(object sender, RoutedEventArgs e)
        {
            CopyPersonalityWindow window = new CopyPersonalityWindow(Personality);
            if (window.ShowDialog() == true)
            {
                string PersonalityName = window.PersonalityName == null ? null : window.PersonalityName.Trim();
                string PersonalityDescription = window.PersonalityDescription == null ? null : window.PersonalityDescription.Trim();
                Personality newPersonality = Personality.Copy(PersonalityName, PersonalityDescription);
                Personalities.Add(newPersonality);
                Personality = newPersonality;
            }
        }

        private void deletePersonalityClicked(object sender, RoutedEventArgs e)
        {
            string messageBoxText = I18N.GetStringWithArgs("speech_responder_delete_personality_message", new string[] { Personality.Name });
            string caption = I18N.GetString("speech_responder_delete_personality_caption");
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    // Remove the personality from the list and the local filesystem
                    Personality oldPersonality = Personality;
                    Personalities.Remove(oldPersonality);
                    Personality = Personalities[0];
                    oldPersonality.RemoveFile();
                    break;
            }
        }

        private void subtitlesEnabled(object sender, RoutedEventArgs e)
        {
            SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
            configuration.Subtitles = true;
            configuration.ToFile();
            EDDI.Instance.Reload("Speech responder");
        }

        private void subtitlesDisabled(object sender, RoutedEventArgs e)
        {
            SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
            configuration.Subtitles = false;
            configuration.ToFile();
            EDDI.Instance.Reload("Speech responder");
        }

        private void subtitlesOnlyEnabled(object sender, RoutedEventArgs e)
        {
            SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
            configuration.SubtitlesOnly = true;
            configuration.ToFile();
            EDDI.Instance.Reload("Speech responder");
        }

        private void subtitlesOnlyDisabled(object sender, RoutedEventArgs e)
        {
            SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
            configuration.SubtitlesOnly = false;
            configuration.ToFile();
            EDDI.Instance.Reload("Speech responder");
        }

        private void personalityDefaultTxt(Personality personality)
        {
            if (personality.IsDefault)
            {
                defaultText.Text = I18N.GetString("speech_responder_default_text");
                defaultText.FontWeight = FontWeights.Bold;
                defaultText.FontStyle = FontStyles.Italic;
                defaultText.FontSize = 13;
            }
            else
            {
                defaultText.Text = I18N.GetString("speech_responder_warning_triggered");
                defaultText.FontWeight = FontWeights.Normal;
                defaultText.FontStyle = FontStyles.Italic;
                defaultText.FontSize = 13;
            }
        }
    }
}

using EddiConfigService;
using EddiCore;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;

namespace EddiSpeechResponder
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : INotifyPropertyChanged
    {
        public ICollectionView ScriptsView
        {
            get => scriptsView;
            private set
            {
                scriptsView = value;
                OnPropertyChanged();
            }
        }

        public List<int?> Priorities => SpeechService.Instance.speechQueue.priorities;

        private SpeechResponder speechResponder { get; }

        private ICollectionView scriptsView;
        private static string filterTxt;

        public ConfigurationWindow(SpeechResponder speechResponder)
        {
            if (speechResponder is null) { return; }
            this.speechResponder = speechResponder;

            InitializeComponent();
            DataContext = speechResponder;

            // Set up the scripts view
            InitializeView(speechResponder.CurrentPersonality.Scripts.Values);

            // Set up other preferences
            subtitlesCheckbox.IsChecked = speechResponder.Configuration?.Subtitles ?? false;
            subtitlesOnlyCheckbox.IsChecked = speechResponder.Configuration?.SubtitlesOnly ?? false;

            SpeechResponder.PersonalityChanged += PersonalityChanged;
            speechResponder.Personalities.CollectionChanged += PersonalitiesCollectionChanged;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                var recoveredScript = ScriptRecoveryService.GetRecoveredScript();
                if (recoveredScript != null)
                {
                    var messageBoxResult = MessageBox.Show(Properties.SpeechResponder.messagebox_recoveredScript,
                        Properties.SpeechResponder.messagebox_recoveredScript_title,
                        MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes,
                        MessageBoxOptions.DefaultDesktopOnly);
                    if (messageBoxResult == MessageBoxResult.Yes && speechResponder?.CurrentPersonality?.Scripts != null)
                    {
                        speechResponder.CurrentPersonality.Scripts[recoveredScript.Name] = recoveredScript;
                        OpenEditScriptWindow(recoveredScript);
                    }
                }
            }), DispatcherPriority.ApplicationIdle);
        }

        private void PersonalitiesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems?.Count > 0)
            {
                personalityComboBox.SelectedItem = speechResponder.Personalities.ElementAt(e.NewStartingIndex);
            }
            else if (e.OldItems?.Count > 0)
            {
                personalityComboBox.SelectedItem = speechResponder.Personalities.FirstOrDefault();
            }
        }

        private void PersonalityChanged(object sender, EventArgs e)
        {
            if (sender is Personality personality)
            {
                InitializeView(personality.Scripts?.Values);
            }
        }

        private void InitializeView(object source)
        {
            ScriptsView = CollectionViewSource.GetDefaultView(source);
            ScriptsView.SortDescriptions.Add(new SortDescription(nameof(Script.Name), ListSortDirection.Ascending));

            // Re-apply text filter, as needed
            if (!string.IsNullOrEmpty(filterTxt))
            {
                using (ScriptsView.DeferRefresh())
                {
                    ScriptsView.Filter = o => { return scriptsData_Filter(o); };
                }
            }
        }

        private void eddiScriptsEnabledUpdated(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox)
            {
                if (checkbox.IsLoaded && checkbox.DataContext is Script script)
                {
                    if (script.Enabled == checkbox.IsChecked)
                    {
                        speechResponder.SavePersonality();
                    }
                }
            }
        }

        private void eddiScriptsPriorityUpdated(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if (comboBox.IsLoaded && (comboBox.IsDropDownOpen || comboBox.IsKeyboardFocused))
                {
                    speechResponder.SavePersonality();
                }
            }
        }

        private static Script getScriptFromContext(object sender)
        {
            if (!(sender is FrameworkElement element)) { return null; }
            if (!(element.DataContext is Script script)) { return null; }
            return script;
        }

        private void editScript(object sender, RoutedEventArgs e)
        {
            var script = getScriptFromContext(sender);
            OpenEditScriptWindow(script);
        }

        private void OpenEditScriptWindow(Script script)
        {
            if (speechResponder?.CurrentPersonality?.Scripts is null) { return; }

            var editScriptWindow = new EditScriptWindow(script, speechResponder.CurrentPersonality.Scripts);
            EDDI.Instance.SpeechResponderModalWait = true;
            editScriptWindow.ShowDialog();
            EDDI.Instance.SpeechResponderModalWait = false;
            if (editScriptWindow.DialogResult ?? false)
            {
                // Non-responder scripts can be renamed, handle that here.
                if (script.Name == editScriptWindow.script.Name)
                {
                    // The script name is unchanged. Update in place.
                    speechResponder.CurrentPersonality.Scripts[script.Name] = editScriptWindow.script;
                }
                else
                {
                    // The script has been renamed.
                    speechResponder.CurrentPersonality.Scripts.Remove(script.Name);
                    speechResponder.CurrentPersonality.Scripts.Add(editScriptWindow.script.Name, editScriptWindow.script);
                }

                speechResponder.SavePersonality();

                // Refresh, then refocus on the current selected script
                var i = scriptsData.SelectedIndex;
                scriptsView.Refresh();
                scriptsData.Focus();
                scriptsData.SelectedIndex = i;
            }
        }

        private void viewScript(object sender, RoutedEventArgs e)
        {
            var script = getScriptFromContext(sender);
            ViewScriptWindow viewScriptWindow = new ViewScriptWindow(script);
            viewScriptWindow.Show();
        }

        private void testScript(object sender, RoutedEventArgs e)
        {
            if (speechResponder?.CurrentPersonality?.Scripts is null) { return; }

            if (SpeechService.Instance.eddiAudioPlaying & !SpeechService.Instance.eddiSpeaking)
            {
                SpeechService.Instance.StopAudio();
            }
            else
            {
                if (!SpeechService.Instance.eddiSpeaking)
                {
                    var script = getScriptFromContext(sender);
                    SpeechResponder responder = new SpeechResponder();
                    responder.Start();
                    responder.TestScript(script.Name, speechResponder.CurrentPersonality.Scripts);
                }
                else
                {
                    SpeechService.Instance.ShutUp();
                    SpeechService.Instance.StopAudio();
                }
            }
        }

        private void resetOrDeleteScript(object sender, RoutedEventArgs e)
        {
            var script = getScriptFromContext(sender);
            if (script != null)
            {
                if (script.IsResettable)
                {
                    resetScript(sender, e);
                }
                else
                {
                    deleteScript(sender, e);
                }
            }
        }

        private void deleteScript(object sender, RoutedEventArgs e)
        {
            if (speechResponder?.CurrentPersonality?.Scripts is null) { return; }

            EDDI.Instance.SpeechResponderModalWait = true;
            var script = getScriptFromContext(sender);
            string messageBoxText = string.Format(Properties.SpeechResponder.delete_script_message, script.Name);
            string caption = Properties.SpeechResponder.delete_script_caption;
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    // Remove the script from the list
                    speechResponder.CurrentPersonality.Scripts.Remove(script.Name);
                    speechResponder.SavePersonality();
                    scriptsView.Refresh();
                    break;
            }
            EDDI.Instance.SpeechResponderModalWait = false;
        }
        private void resetScript(object sender, RoutedEventArgs e)
        {
            if (speechResponder?.CurrentPersonality?.Scripts is null) { return; }

            var script = getScriptFromContext(sender);
            // Resetting the script resets it to its value in the default personality
            if (speechResponder.CurrentPersonality.Scripts.ContainsKey(script.Name))
            {
                string messageBoxText = string.Format(Properties.SpeechResponder.reset_script_message, script.Name);
                string caption = Properties.SpeechResponder.reset_script_button;
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        script.Value = script.defaultValue;
                        speechResponder.CurrentPersonality.Scripts[script.Name] = script;
                        speechResponder.SavePersonality();
                        scriptsData.Items.Refresh();
                        break;
                }
            }
        }

        private void newScriptClicked(object sender, RoutedEventArgs e)
        {
            if (speechResponder?.CurrentPersonality?.Scripts is null) { return; }
            EDDI.Instance.SpeechResponderModalWait = true;
            EditScriptWindow editScriptWindow = new EditScriptWindow(null, speechResponder.CurrentPersonality.Scripts);
            if (editScriptWindow.ShowDialog() == true)
            {
                var newScript = editScriptWindow.script;
                speechResponder.CurrentPersonality.Scripts[newScript.Name] = newScript;
                speechResponder.SavePersonality();
                scriptsView.Refresh();
            }
            EDDI.Instance.SpeechResponderModalWait = false;
        }

        private void copyPersonalityClicked(object sender, RoutedEventArgs e)
        {
            if (speechResponder?.Personalities is null) { return; }
            EDDI.Instance.SpeechResponderModalWait = true;
            CopyPersonalityWindow window = new CopyPersonalityWindow(speechResponder.Personalities)
            {
                Owner = Window.GetWindow(this)
            };
            if (window.ShowDialog() == true)
            {
                speechResponder.CopyCurrentPersonality(window.PersonalityName, window.PersonalityDescription, window.PersonalityDisableScripts);
            }
            EDDI.Instance.SpeechResponderModalWait = false;
        }

        private void deletePersonalityClicked(object sender, RoutedEventArgs e)
        {
            if (speechResponder?.Personalities is null) { return; }
            EDDI.Instance.SpeechResponderModalWait = true;
            string messageBoxText = string.Format(Properties.SpeechResponder.delete_personality_message, speechResponder.CurrentPersonality.Name);
            string caption = Properties.SpeechResponder.delete_personality_caption;
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    speechResponder.RemoveCurrentPersonality();
                    break;
            }
            EDDI.Instance.SpeechResponderModalWait = false;
        }

        private void subtitlesEnabled(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.IsLoaded && speechResponder?.Configuration != null)
                {
                    speechResponder.Configuration.Subtitles = true;
                    ConfigService.Instance.speechResponderConfiguration = speechResponder.Configuration;
                }
            }
        }

        private void subtitlesDisabled(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.IsLoaded && speechResponder?.Configuration != null)
                {
                    speechResponder.Configuration.Subtitles = false;
                    ConfigService.Instance.speechResponderConfiguration = speechResponder.Configuration;
                }
            }
        }

        private void subtitlesOnlyEnabled(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.IsLoaded && speechResponder?.Configuration != null)
                {
                    speechResponder.Configuration.SubtitlesOnly = true;
                    ConfigService.Instance.speechResponderConfiguration = speechResponder.Configuration;
                }
            }
        }

        private void subtitlesOnlyDisabled(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.IsLoaded && speechResponder?.Configuration != null)
                {
                    speechResponder.Configuration.SubtitlesOnly = false;
                    ConfigService.Instance.speechResponderConfiguration = speechResponder.Configuration;
                }
            }
        }

        private void SpeechResponderHelp_Click(object sender, RoutedEventArgs e)
        {
            var speechResponderHelpWindow = new MarkdownWindow("speechResponderHelp.md");
            speechResponderHelpWindow.Show();
        }

        private void SearchFilterText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            using (ScriptsView.DeferRefresh())
            {
                filterTxt = searchFilterText.Text;
                ScriptsView.Filter = o => { return scriptsData_Filter(o); };
            }
        }

        private bool scriptsData_Filter(object sender)
        {
            if (string.IsNullOrEmpty(filterTxt)) { return true; }
            if (!(sender is Script script)) { return true; }

            // If filter applies, filter items.
            if ((script.Name?.ToLowerInvariant().Contains(filterTxt.ToLowerInvariant()) ?? false)
                || (script.Description?.ToLowerInvariant().Contains(filterTxt.ToLowerInvariant()) ?? false)
                || (script.Value?.ToLowerInvariant().Contains(filterTxt.ToLowerInvariant()) ?? false))
            {
                return true;
            }
            return false;
        }

        private void EnableAll_Clicked(object sender, RoutedEventArgs e) 
        {
            speechResponder?.EnableOrDisableAllScripts(speechResponder.CurrentPersonality, true);
        }

        private void DisableAll_Clicked(object sender, RoutedEventArgs e)
        {
            speechResponder?.EnableOrDisableAllScripts(speechResponder.CurrentPersonality, false);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BooleanAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            foreach (object value in values)
            {
                if ((value is bool b) && b == false)
                {
                    return false;
                }
            }
            return true;
        }
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException("BooleanAndConverter is a OneWay converter.");
        }
    }
}

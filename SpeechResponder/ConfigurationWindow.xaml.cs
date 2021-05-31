using EddiCore;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Utilities;

namespace EddiSpeechResponder
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl, INotifyPropertyChanged
    {
        private SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
        private readonly Personality defaultPersonality = Personality.Default();

        public ObservableCollection<Personality> Personalities
        {
            get => personalities;
            set
            {
                personalities = value;
                OnPropertyChanged();
            }
        }

        public Personality Personality
        {
            get => personality ?? Personalities.FirstOrDefault(p => p.Name == configuration.Personality) ?? Personalities[0];
            set
            {
                personality = value ?? personalities[0] ?? defaultPersonality;
                InitializeView(personality.Scripts);
                OnPropertyChanged();
            }
        }

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

        private ObservableCollection<Personality> personalities;
        private Personality personality;
        private ICollectionView scriptsView;

        public ConfigurationWindow()
        {
            InitializeComponent();
            DataContext = this;

            Personalities = GetPersonalities();
            Personality = GetPersonality();

            InitializeView(Personality.Scripts);

            subtitlesCheckbox.IsChecked = configuration.Subtitles;
            subtitlesOnlyCheckbox.IsChecked = configuration.SubtitlesOnly;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                var recoveredScript = ScriptRecoveryService.GetRecoveredScript();
                if (recoveredScript != null)
                {
                    var messageBoxResult = MessageBox.Show(Properties.SpeechResponder.messagebox_recoveredScript,
                        Properties.SpeechResponder.messagebox_recoveredScript_title,
                        MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes,
                        MessageBoxOptions.DefaultDesktopOnly);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        Personality.Scripts[recoveredScript.Name] = recoveredScript;
                        OpenEditScriptWindow(recoveredScript);
                    }
                }
            }), DispatcherPriority.ApplicationIdle);
        }

        private void InitializeView(object source)
        {
            ScriptsView = CollectionViewSource.GetDefaultView(source);
            ScriptsView.SortDescriptions.Add(new SortDescription("Value.Name", ListSortDirection.Ascending));
            searchFilterText.Text = string.Empty; // Clear any active filters
        }

        private ObservableCollection<Personality> GetPersonalities()
        {
            if (personalities is null)
            {
                // Initialize our collection and add our default personality
                personalities = new ObservableCollection<Personality> { defaultPersonality };

                // Add our custom personalities
                foreach (var customPersonality in Personality.AllFromDirectory())
                {
                    if (customPersonality != null)
                    {
                        personalities.Add(customPersonality);
                    }
                }
            }
            return personalities;
        }

        private Personality GetPersonality()
        {
            return Personality
                ?? Personalities.SingleOrDefault(p => p.Name == configuration.Personality)
                ?? personalities[0]
                ?? defaultPersonality;
        }

        private void eddiScriptsEnabledUpdated(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkbox)
            {
                if (checkbox.IsLoaded)
                {
                    updateScriptsConfiguration();
                }
            }
        }

        private void eddiScriptsPriorityUpdated(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if (comboBox.IsLoaded && (comboBox.IsDropDownOpen || comboBox.IsKeyboardFocused))
                {
                    updateScriptsConfiguration();
                }
            }
        }

        private static Script getScriptFromContext(object sender)
        {
            if (!(sender is FrameworkElement element)) { return null; }
            if (!(element.DataContext is KeyValuePair<string, Script> kvp)) { return null; }
            return kvp.Value;
        }

        private void editScript(object sender, RoutedEventArgs e)
        {
            var script = getScriptFromContext(sender);
            OpenEditScriptWindow(script);
        }

        private void OpenEditScriptWindow(Script script)
        {
            EditScriptWindow editScriptWindow = new EditScriptWindow(script, Personality.Scripts);
            EDDI.Instance.SpeechResponderModalWait = true;
            editScriptWindow.ShowDialog();
            EDDI.Instance.SpeechResponderModalWait = false;
            if (editScriptWindow.DialogResult ?? false)
            {
                Personality.Scripts[script.Name] = editScriptWindow.script;
                updateScriptsConfiguration();

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
            if (!SpeechService.Instance.eddiSpeaking)
            {
                var script = getScriptFromContext(sender);
                SpeechResponder responder = new SpeechResponder();
                responder.Start();
                responder.TestScript(script.Name, Personality.Scripts);
            }
            else
            {
                SpeechService.Instance.ShutUp();
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
            EDDI.Instance.SpeechResponderModalWait = true;
            var script = getScriptFromContext(sender);
            string messageBoxText = string.Format(Properties.SpeechResponder.delete_script_message, script.Name);
            string caption = Properties.SpeechResponder.delete_script_caption;
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    // Remove the script from the list
                    Personality.Scripts.Remove(script.Name);
                    updateScriptsConfiguration();
                    scriptsView.Refresh();
                    break;
            }
            EDDI.Instance.SpeechResponderModalWait = false;
        }
        private void resetScript(object sender, RoutedEventArgs e)
        {
            var script = getScriptFromContext(sender);
            // Resetting the script resets it to its value in the default personality
            if (Personality.Scripts.ContainsKey(script.Name))
            {
                string messageBoxText = string.Format(Properties.SpeechResponder.reset_script_message, script.Name);
                string caption = Properties.SpeechResponder.reset_script_button;
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        script.Value = script.defaultValue;
                        Personality.Scripts[script.Name] = script;
                        updateScriptsConfiguration();
                        scriptsData.Items.Refresh();
                        break;
                }
            }
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
            if (sender is ComboBox comboBox && !comboBox.IsLoaded) { return; }
            if (Personality != null)
            {
                configuration = SpeechResponderConfiguration.FromFile();
                configuration.Personality = Personality.Name;
                configuration.ToFile();
                EDDI.Instance.Reload("Speech responder");
            }
        }

        private void newScriptClicked(object sender, RoutedEventArgs e)
        {
            EDDI.Instance.SpeechResponderModalWait = true;
            EditScriptWindow editScriptWindow = new EditScriptWindow(null, Personality.Scripts);
            if (editScriptWindow.ShowDialog() == true)
            {
                var newScript = editScriptWindow.script;
                Personality.Scripts[newScript.Name] = newScript;
                updateScriptsConfiguration();
                scriptsView.Refresh();
            }
            EDDI.Instance.SpeechResponderModalWait = false;
        }

        private void copyPersonalityClicked(object sender, RoutedEventArgs e)
        {
            EDDI.Instance.SpeechResponderModalWait = true;
            CopyPersonalityWindow window = new CopyPersonalityWindow(Personalities)
            {
                Owner = Window.GetWindow(this)
            };
            if (window.ShowDialog() == true)
            {
                string PersonalityName = window.PersonalityName?.Trim();
                string PersonalityDescription = window.PersonalityDescription?.Trim();
                bool disableScripts = window.PersonalityDisableScripts;
                Personality newPersonality = Personality.Copy(PersonalityName, PersonalityDescription);
                if (disableScripts) { EnableOrDisableAll(newPersonality, false); }
                Personalities.Add(newPersonality);
                Personality = newPersonality;
            }
            EDDI.Instance.SpeechResponderModalWait = false;
        }

        private void deletePersonalityClicked(object sender, RoutedEventArgs e)
        {
            EDDI.Instance.SpeechResponderModalWait = true;
            string messageBoxText = string.Format(Properties.SpeechResponder.delete_personality_message, Personality.Name);
            string caption = Properties.SpeechResponder.delete_personality_caption;
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    // Remove the personality from the list and the local filesystem
                    LockManager.GetLock("DeletePersonality", () => 
                    {
                        Personality oldPersonality = Personality;
                        Personality = null; // Forces bindings to update
                        Personalities.Remove(oldPersonality);
                        oldPersonality.RemoveFile();
                        Personality = Personalities[0];
                    });
                    break;
            }
            EDDI.Instance.SpeechResponderModalWait = false;
        }

        private void subtitlesEnabled(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.IsLoaded)
                {
                    configuration = SpeechResponderConfiguration.FromFile();
                    configuration.Subtitles = true;
                    configuration.ToFile();
                    EDDI.Instance.Reload("Speech responder");
                }
            }
        }

        private void subtitlesDisabled(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.IsLoaded)
                {
                    configuration = SpeechResponderConfiguration.FromFile();
                    configuration.Subtitles = false;
                    configuration.ToFile();
                    EDDI.Instance.Reload("Speech responder");
                }
            }
        }

        private void subtitlesOnlyEnabled(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.IsLoaded)
                {
                    configuration = SpeechResponderConfiguration.FromFile();
                    configuration.SubtitlesOnly = true;
                    configuration.ToFile();
                    EDDI.Instance.Reload("Speech responder");
                }
            }
        }

        private void subtitlesOnlyDisabled(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.IsLoaded)
                {
                    configuration = SpeechResponderConfiguration.FromFile();
                    configuration.SubtitlesOnly = false;
                    configuration.ToFile();
                    EDDI.Instance.Reload("Speech responder");
                }
            }
        }

        private void SpeechResponderHelp_Click(object sender, RoutedEventArgs e)
        {
            MarkdownWindow speechResponderHelpWindow = new MarkdownWindow("speechResponderHelp.md");
            speechResponderHelpWindow.Show();
        }

        private void SearchFilterText_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            using (ScriptsView.DeferRefresh())
            {
                ScriptsView.Filter = o => { return scriptsData_Filter(o); };
            }
        }

        private bool scriptsData_Filter(object sender)
        {
            if (string.IsNullOrEmpty(searchFilterText.Text)) { return true; }
            if (!(sender is KeyValuePair<string, Script> kvp)) { return true; }
            var script = kvp.Value;
            var filterTxt = searchFilterText.Text;

            // If filter applies, filter items.
            if ((script.Name?.ToLowerInvariant().Contains(filterTxt.ToLowerInvariant()) ?? false)
                || (script.Description?.ToLowerInvariant().Contains(filterTxt.ToLowerInvariant()) ?? false)
                || (script.Value?.ToLowerInvariant().Contains(filterTxt.ToLowerInvariant()) ?? false))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void EnableAll_Clicked(object sender, RoutedEventArgs e) 
        {
            EnableOrDisableAll(Personality, true);
        }

        private void DisableAll_Clicked(object sender, RoutedEventArgs e)
        {
            EnableOrDisableAll(Personality, false);
        }

        private void EnableOrDisableAll(Personality targetPersonality, bool desiredState)
        {
            foreach (var kvScript in targetPersonality.Scripts)
            {
                var script = kvScript.Value;
                if (script.Responder)
                {
                    script.Enabled = desiredState;
                }
            }
            updateScriptsConfiguration();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
                if ((value is bool) && (bool)value == false)
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

using EddiConfigService;
using EddiCore;
using EddiEvents;
using EddiSpeechResponder.ScriptResolverService;
using EddiSpeechService;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
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
    public partial class ConfigurationWindow : INotifyPropertyChanged
    {
        private static EditScriptWindow editScriptWindow { get; set; }

        public ICollectionView ScriptsView
        {
            get => scriptsView;
            private set
            {
                scriptsView = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<int?> Priorities => SpeechService.Instance.speechQueue.priorities;

        private SpeechResponder SpeechResponder { get; }

        private ICollectionView scriptsView;
        private static string filterTxt;

        private static IEnumerable<string> customFunctionNames { get; set; }
        private static IEnumerable<MetaVariable> standardMetaVariables { get; set; } = new List<MetaVariable>();

        private IEnumerable<MetaVariable> GetMetaVariables ( string scriptName = null )
        {
            var vars = new List<MetaVariable>();

            // Fetch our pre-loaded standard MetaVariables
            vars.AddRange( new List<MetaVariable>( standardMetaVariables ) );

            // Get any additional Event MetaVariables
            if ( !string.IsNullOrEmpty( scriptName ) )
            {
                var type = Events.TYPES.SingleOrDefault( t => t.Key == scriptName ).Value;
                if ( type != null )
                {
                    var eventVars = new MetaVariables( type ).Results;
                    foreach ( var v in eventVars )
                    {
                        v.keysPath = v.keysPath.Prepend( "event" ).ToList();
                    }
                    vars.AddRange( eventVars );
                }
            }

            return vars;
        }

        // we may revise this in future to support custom user color schemes
        private static AvalonEdit.CottleHighlighting GetHighlighting ( IEnumerable<MetaVariable> metaVars )
        {
            return new AvalonEdit.CottleHighlighting( customFunctionNames, metaVars
                .SelectMany( v => v.keysPath )
                .Where(v => !string.IsNullOrEmpty(v))
                .Distinct()
                .ToList() 
            );
        }
        
        public ConfigurationWindow(SpeechResponder speechResponder)
        {
            if (speechResponder is null) { return; }
            this.SpeechResponder = speechResponder;
            customFunctionNames = ScriptResolver.GetCustomFunctions().Select( f => f.name );
            Task.Run( GetStandardVariables );

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
                var recoveredScript = ScriptRecoveryService.ScriptRecoveryService.GetRecoveredScript();
                if (recoveredScript != null)
                {
                    var messageBoxResult = MessageBox.Show(Properties.SpeechResponder.messagebox_recoveredScript,
                        Properties.SpeechResponder.messagebox_recoveredScript_title,
                        MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.Yes,
                        MessageBoxOptions.DefaultDesktopOnly);
                    if (messageBoxResult == MessageBoxResult.Yes && speechResponder.CurrentPersonality?.Scripts != null)
                    {
                        OpenEditScriptWindow(speechResponder, recoveredScript, true);
                    }
                }
            }), DispatcherPriority.ApplicationIdle);
        }

        private void GetStandardVariables ()
        {
            // Get MetaVariables for standard object variables available from the script resolver
            var metaVars = new HashSet<MetaVariable>();
            var varsLock = new object();
            var standardVars = SpeechResponder.ScriptResolver.CompileVariables();
            standardVars.AsParallel().ForAll( kvp =>
            {
                if ( kvp.Value.Item1 is null ) { return; }
                var vars = new MetaVariables ( kvp.Value.Item1 ).Results;
                foreach ( var v in vars )
                {
                    v.keysPath = v.keysPath.Prepend ( kvp.Key ).ToList ();
                }
                lock ( varsLock )
                {
                    metaVars.UnionWith ( vars );
                }
            } );
            standardMetaVariables = metaVars;

            Dispatcher.InvokeAsync( () =>
            {
                if ( editScriptWindow?.IsLoaded ?? false )
                {
                    editScriptWindow.AddStandardMetaVariables( metaVars );
                }
            } );
        }

        private void PersonalitiesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems?.Count > 0)
            {
                personalityComboBox.SelectedItem = SpeechResponder.Personalities.ElementAt(e.NewStartingIndex);
            }
            else if (e.OldItems?.Count > 0)
            {
                personalityComboBox.SelectedItem = SpeechResponder.Personalities.FirstOrDefault();
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
                    ScriptsView.Filter = scriptsData_Filter;
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
                        SpeechResponder.SavePersonality();
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
                    SpeechResponder.SavePersonality();
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
            OpenEditScriptWindow( SpeechResponder, script );
        }

        private void OpenEditScriptWindow(SpeechResponder speechResponder, Script script, bool isRecoveredScript = false)
        {
            if (speechResponder?.CurrentPersonality?.Scripts is null) { return; }

            var metaVars = GetMetaVariables( script.Name ).ToList();
            var highlighting = GetHighlighting( metaVars );
            editScriptWindow = new EditScriptWindow(speechResponder, script, speechResponder.CurrentPersonality.Scripts, metaVars, highlighting, isRecoveredScript);
            EDDI.Instance.SpeechResponderModalWait = true;
            try
            {
                editScriptWindow.ShowDialog();
            }
            catch ( Win32Exception ex )
            {
                Logging.Warn( ex.Message, ex );
            }
            EDDI.Instance.SpeechResponderModalWait = false;
            if (editScriptWindow.DialogResult ?? false)
            {
                // Non-responder scripts can be renamed, handle that here.
                if (script.Name == editScriptWindow.revisedScript.Name)
                {
                    var updatedScript = speechResponder.CurrentPersonality.Scripts[script.Name];
                    updatedScript.Value = editScriptWindow.revisedScript.Value;
                    updatedScript.Description = editScriptWindow.revisedScript.Description;
                    updatedScript.includes = editScriptWindow.revisedScript.includes;
                }
                else
                {
                    // The script has been renamed.
                    speechResponder.CurrentPersonality.Scripts.Remove(script.Name);
                    speechResponder.CurrentPersonality.Scripts.Add(editScriptWindow.revisedScript.Name, editScriptWindow.revisedScript );

                    // Update any included script references
                    foreach (var currentPersonalityScript in speechResponder.CurrentPersonality.Scripts.Values)
                    {
                        currentPersonalityScript.includes =
                            (currentPersonalityScript.includes ?? string.Empty).Replace( script.Name,
                                editScriptWindow.revisedScript.Name );
                    }
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
            var viewScriptWindow = new ViewScriptWindow(script, GetHighlighting( GetMetaVariables( script.Name ) ));
            viewScriptWindow.Show();
        }

        private void testScript(object sender, RoutedEventArgs e)
        {
            if (SpeechResponder?.CurrentPersonality?.Scripts is null) { return; }

            if (SpeechService.Instance.eddiAudioPlaying & !SpeechService.Instance.eddiSpeaking)
            {
                SpeechService.Instance.StopAudio();
            }
            else
            {
                if (!SpeechService.Instance.eddiSpeaking)
                {
                    var script = getScriptFromContext(sender);
                    SpeechResponder responder = (SpeechResponder)EDDI.Instance.ObtainResponder("Speech Responder");
                    responder?.TestScript( script.Name, SpeechResponder.CurrentPersonality.Scripts );
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
            if (SpeechResponder?.CurrentPersonality?.Scripts is null) { return; }

            EDDI.Instance.SpeechResponderModalWait = true;
            var script = getScriptFromContext(sender);
            string messageBoxText = string.Format(Properties.SpeechResponder.delete_script_message, script.Name);
            string caption = Properties.SpeechResponder.delete_script_caption;
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    // Remove the script from the list
                    SpeechResponder.CurrentPersonality.Scripts.Remove( script.Name );

                    // Remove any references to the removed script in the `includes` scring of other scripts
                    SpeechResponder.CurrentPersonality.Scripts.AsParallel().ForAll( kv =>
                        kv.Value.includes = kv.Value.includes is null
                            ? string.Empty
                            : string.Join( "; ",
                                kv.Value.includes.Split( ';' ).Select( s => s.Trim() )
                                    .Except( new[] { script.Name } ) ) );

                    SpeechResponder.SavePersonality();
                    scriptsView.Refresh();
                    break;
            }
            EDDI.Instance.SpeechResponderModalWait = false;
        }
        private void resetScript(object sender, RoutedEventArgs e)
        {
            if (SpeechResponder?.CurrentPersonality?.Scripts is null) { return; }

            var script = getScriptFromContext(sender);
            // Resetting the script resets it to its value in the default personality
            if (SpeechResponder.CurrentPersonality.Scripts.ContainsKey(script.Name))
            {
                string messageBoxText = string.Format(Properties.SpeechResponder.reset_script_message, script.Name);
                string caption = Properties.SpeechResponder.reset_script_button;
                MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        script.Value = script.defaultValue;
                        SpeechResponder.CurrentPersonality.Scripts[script.Name] = script;
                        SpeechResponder.SavePersonality();
                        scriptsData.Items.Refresh();
                        break;
                }
            }
        }

        private void newScriptClicked(object sender, RoutedEventArgs e)
        {
            if (SpeechResponder?.CurrentPersonality?.Scripts is null) { return; }
            EDDI.Instance.SpeechResponderModalWait = true;
            var metaVars = GetMetaVariables ().ToList ();
            var highlighting = GetHighlighting( metaVars );
            editScriptWindow = new EditScriptWindow( SpeechResponder, null, SpeechResponder.CurrentPersonality.Scripts, metaVars, highlighting, true);
            try
            {
                if ( editScriptWindow.ShowDialog() == true )
                {
                    var newScript = editScriptWindow.revisedScript;
                    SpeechResponder.CurrentPersonality.Scripts[ newScript.Name ] = newScript;
                    SpeechResponder.SavePersonality();
                    scriptsView.Refresh();
                }
            }
            catch ( Win32Exception ex )
            {
                Logging.Warn( ex.Message, ex );
            }
            EDDI.Instance.SpeechResponderModalWait = false;
        }

        private void copyPersonalityClicked(object sender, RoutedEventArgs e)
        {
            if (SpeechResponder?.Personalities is null) { return; }
            EDDI.Instance.SpeechResponderModalWait = true;
            CopyPersonalityWindow window = new CopyPersonalityWindow(SpeechResponder.Personalities)
            {
                Owner = Window.GetWindow(this)
            };
            try
            {
                if ( window.ShowDialog() == true )
                {
                    SpeechResponder.CopyCurrentPersonality( window.PersonalityName, window.PersonalityDescription, window.PersonalityDisableScripts );
                }
            }
            catch ( Win32Exception ex )
            {
                Logging.Warn( ex.Message, ex );
            }
            EDDI.Instance.SpeechResponderModalWait = false;
        }

        private void deletePersonalityClicked(object sender, RoutedEventArgs e)
        {
            if (SpeechResponder?.Personalities is null) { return; }
            EDDI.Instance.SpeechResponderModalWait = true;
            string messageBoxText = string.Format(Properties.SpeechResponder.delete_personality_message, SpeechResponder.CurrentPersonality.Name);
            string caption = Properties.SpeechResponder.delete_personality_caption;
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    SpeechResponder.RemoveCurrentPersonality();
                    break;
            }
            EDDI.Instance.SpeechResponderModalWait = false;
        }

        private void subtitlesEnabled(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.IsLoaded && SpeechResponder?.Configuration != null)
                {
                    SpeechResponder.Configuration.Subtitles = true;
                    ConfigService.Instance.speechResponderConfiguration = SpeechResponder.Configuration;
                }
            }
        }

        private void subtitlesDisabled(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.IsLoaded && SpeechResponder?.Configuration != null)
                {
                    SpeechResponder.Configuration.Subtitles = false;
                    ConfigService.Instance.speechResponderConfiguration = SpeechResponder.Configuration;
                }
            }
        }

        private void subtitlesOnlyEnabled(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.IsLoaded && SpeechResponder?.Configuration != null)
                {
                    SpeechResponder.Configuration.SubtitlesOnly = true;
                    ConfigService.Instance.speechResponderConfiguration = SpeechResponder.Configuration;
                }
            }
        }

        private void subtitlesOnlyDisabled(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox)
            {
                if (checkBox.IsLoaded && SpeechResponder?.Configuration != null)
                {
                    SpeechResponder.Configuration.SubtitlesOnly = false;
                    ConfigService.Instance.speechResponderConfiguration = SpeechResponder.Configuration;
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
                ScriptsView.Filter = scriptsData_Filter;
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
            SpeechResponder?.EnableOrDisableAllScripts(SpeechResponder.CurrentPersonality, true);
        }

        private void DisableAll_Clicked(object sender, RoutedEventArgs e)
        {
            SpeechResponder?.EnableOrDisableAllScripts(SpeechResponder.CurrentPersonality, false);
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
                if (value is bool b && b == false)
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

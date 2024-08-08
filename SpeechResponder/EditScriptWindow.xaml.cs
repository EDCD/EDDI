using EddiSpeechResponder.AvalonEdit;
using EddiSpeechResponder.Properties;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Utilities;

namespace EddiSpeechResponder
{
    /// <summary>
    /// Interaction logic for EditScriptWindow.xaml
    /// </summary>
    public partial class EditScriptWindow : Window
    {
        public Script script { get; private set; }
        public Script editorScript { get; private set; }

        private readonly Dictionary<string, Script> _scripts;
        private readonly bool isNewOrRecoveredScript;

        public ScriptRecoveryService ScriptRecoveryService { get; set; }

#pragma warning disable IDE0052 // Remove unused private members -- this may be used later
        private readonly DocumentHighlighter documentHighlighter;
#pragma warning restore IDE0052 // Remove unused private members

        private TextCompletionWindow completionWindow;

        private readonly FoldingStrategy foldingStrategy;
        private FoldingMargin foldingMargin;

        private readonly List<MetaVariable> metaVars = new List<MetaVariable>();
        private readonly List<ICustomFunction> customFunctions;
        private static readonly object metaVarLock = new object();

        public EditScriptWindow ( ScriptResolver scriptResolver, Script script, Dictionary<string, Script> scripts, [NotNull][ItemNotNull] IEnumerable<MetaVariable> metaVars, [NotNull] CottleHighlighting cottleHighlighting, bool isNewOrRecoveredScript )
        {
            InitializeComponent();
            DataContext = this;

            this.customFunctions = ScriptResolver.GetCustomFunctions();
            this.isNewOrRecoveredScript = isNewOrRecoveredScript;
            _scripts = scripts;
            this.script = script;
            this.metaVars.AddRange(metaVars);

            if ( script == null )
            {
                // This is a new script
                editorScript = new Script( "New script", null, false, null );
            }
            else
            {
                // This is an existing script
                editorScript = script.Copy();
            }

            // See if there is the default value for this script is empty
            if ( string.IsNullOrWhiteSpace( editorScript.defaultValue ) )
            {
                // No default; disable reset and show
                showDiffButton.IsEnabled = false;
                resetToDefaultButton.IsEnabled = false;
            }

            // Set our editor content
            scriptView.Text = editorScript.Value;

            // Convert tabs to spaces
            scriptView.Options.ConvertTabsToSpaces = true;

            // Set up our Script Recovery Service
            ScriptRecoveryService = new ScriptRecoveryService( this );
            ScriptRecoveryService.BeginScriptRecovery();
            scriptView.TextChanged += ScriptView_TextChanged;

            // Implement collapsible sections (called `Foldings`)
            foldingStrategy = new FoldingStrategy( '{', '}' );
            foldingStrategy.CreateNewFoldings( scriptView.Document );
            InitializeOrUpdateFolding();
            scriptView.Options.AllowScrollBelowDocument = true;

            // Set up our search window
            SearchPanel.Install( scriptView );

            // Set up our Cottle highlighter
            documentHighlighter = new DocumentHighlighter( scriptView.Document, cottleHighlighting.Definition );

            // Implement text completion
            scriptView.TextArea.TextEntering += ScriptView_TextArea_TextEntering;
            scriptView.TextArea.TextEntered += ScriptView_TextArea_TextEntered;

            // Monitor window size and position
            WindowStartupLocation = WindowStartupLocation.Manual;
            SourceInitialized += EditScriptWindow_SourceInitialized;
            Loaded += OnLoaded;
            Closed += EditScriptWindow_SaveWindowStatePosition;
            LocationChanged += EditScriptWindow_SaveWindowStatePosition;
            SizeChanged += EditScriptWindow_SaveWindowStatePosition;
            StateChanged += EditScriptWindow_SaveWindowStatePosition;
        }

        private void OnLoaded ( object sender, RoutedEventArgs e )
        {
            // Don't allow the window to start minimized
            if ( WindowState == WindowState.Minimized )
            {
                WindowState = WindowState.Normal;
                Visibility = Visibility.Visible;
            }
        }

        private void EditScriptWindow_SaveWindowStatePosition ( object sender, EventArgs e )
        {
            if ( IsLoaded )
            {
                Settings.Default.Save();
            }
        }

        private void EditScriptWindow_SourceInitialized ( object sender, EventArgs e )
        {
            // Validate window position on opening
            int designedHeight = (int)MinHeight;
            int designedWidth = (int)MinWidth;

            // WPF uses DPI scaled units rather than true pixels.
            // Retrieve the DPI scaling for the controlling monitor (where the top left pixel is located).
            var dpiScale = VisualTreeHelper.GetDpi(this);
            var windowPosition = new Rect(new Point(Settings.Default.Left, Settings.Default.Top), new Size(Settings.Default.Width, Settings.Default.Height));
            if ( windowPosition == Rect.Empty || !isWindowValid( windowPosition, dpiScale ) )
            {
                // Revert to default values if the prior size and position are no longer valid
                Left = centerWindow( applyDpiScale( Screen.PrimaryScreen.Bounds.Width, dpiScale.DpiScaleX ),
                    designedWidth );
                Top = centerWindow( applyDpiScale( Screen.PrimaryScreen.Bounds.Height, dpiScale.DpiScaleY ),
                    designedHeight );
                Width = Math.Min( Screen.PrimaryScreen.Bounds.Width / dpiScale.DpiScaleX, designedWidth );
                Height = Math.Min( Screen.PrimaryScreen.Bounds.Height / dpiScale.DpiScaleY, designedHeight );
            }

            // Check detected monitors to see if the saved window size and location is valid
            bool isWindowValid ( Rect rect, DpiScale dpi )
            {
                // Check for minimum window size
                if ( (int)rect.Width < designedWidth || (int)rect.Height < designedHeight )
                {
                    return false;
                }

                // Check whether the rectangle is completely visible on-screen
                bool testUpperLeft = false;
                bool testLowerRight = false;
                foreach ( Screen screen in Screen.AllScreens )
                {
                    if ( rect.X >= applyDpiScale( screen.Bounds.X, dpi.DpiScaleX ) && rect.Y >= applyDpiScale( screen.Bounds.Y, dpi.DpiScaleY ) ) // The upper and left bounds fall on a valid screen
                    {
                        testUpperLeft = true;
                    }
                    if ( applyDpiScale( screen.Bounds.Width, dpi.DpiScaleX ) >= (rect.X + rect.Width) && 
                         applyDpiScale( screen.Bounds.Height, dpi.DpiScaleY ) >= (rect.Y + rect.Height) ) // The lower and right bounds fall on a valid screen 
                    {
                        testLowerRight = true;
                    }
                }
                if ( testUpperLeft && testLowerRight )
                {
                    return true;
                }
                return false;
            }

            int centerWindow ( int measure, int defaultValue )
            {
                return ( measure - Math.Min( measure, defaultValue ) ) / 2;
            }

            int applyDpiScale ( int originalValue, double dpiScaleFactor )
            {
                return (int)Math.Round( originalValue / dpiScaleFactor );
            }
        }

        private void ScriptView_TextChanged ( object sender, EventArgs e )
        {
            editorScript.Value = scriptView.Text;
            InitializeOrUpdateFolding();
        }

        private void ScriptView_TextArea_TextEntered ( object sender, TextCompositionEventArgs e )
        {
            // open code completion after the user has pressed dot:
            if ( e.Text == "." )
            {
                if ( !( sender is TextArea textArea ) ) { return; }

                // Select the specific data we need to obtain
                var line = textArea.Document.GetLineByOffset(textArea.Caret.Offset);
                var lineTxt = textArea.Document.GetText(line.Offset, textArea.Caret.Offset - line.Offset);
                var lookupItem = GetTextCompletionLookupItem(lineTxt);
                var priorText = textArea.Document.GetText(0, textArea.Caret.Offset);
                var textCompletionItems = GetCompletionItems(lookupItem, priorText);

                // Send the result to the text completion window
                if ( textCompletionItems.Any() )
                {
                    completionWindow = new TextCompletionWindow( scriptView.TextArea, textCompletionItems );
                    completionWindow.Closed += delegate { completionWindow = null; };
                }
            }
        }

        public static string GetTextCompletionLookupItem(string lineTxt)
        {
            var lookupItem = string.Empty;
            var lineMatch = Regex.Match(lineTxt, @"(?<={)[^:}]*?(\S+(?>\[\d\])?\.)+$");
            if (lineMatch.Success)
            {
                lookupItem = lineMatch.Groups[lineMatch.Groups.Count - 1].Value.TrimEnd('.');
                if (!string.IsNullOrEmpty(lookupItem))
                {
                    // Replace any enumeration value for enumerable values (e.g. 'bodies[5]') with a standard index marker
                    lookupItem = Regex.Replace( lookupItem, @"(?<=\S)+\[\d+\]", $".{MetaVariables.indexMarker}" );
                }
            }
            return lookupItem;
        }

        private List<TextCompletionItem> GetCompletionItems(string lookupItem, string priorText)
        {
            var textCompletionItems = new List<TextCompletionItem>();
            if ( string.IsNullOrEmpty( lookupItem ) || string.IsNullOrEmpty( priorText ) )
            {
                return textCompletionItems;
            }

            // Remove any leading "!" and split our lookup item into its constituent parts / objects
            var lookupKeys = lookupItem.Replace("!", "").Split('.').ToList();
            if (!lookupKeys.Any())
            {
                return textCompletionItems;
            }

            // Resolve any simple text aliases (e.g. {set a to b.c}).
            var simpleAliases = Regex.Matches( priorText, @"{set (?<key>\w*) to (?<value>[\w|\.]*)}" );
            foreach ( var obj in simpleAliases )
            {
                if ( obj is Match match )
                {
                    if ( lookupKeys[ 0 ] == match.Groups[ "key" ].Value )
                    {
                        lookupKeys.RemoveAt(0);
                        lookupKeys.InsertRange(0, match.Groups[ "value" ].Value.Split( '.' ) );
                    }
                }
            }

            var filteredMetaVars = new List<MetaVariable>();

            List<MetaVariable> FilterMetaVars ( List<MetaVariable> metaVariables )
            {
                // Remove any nested keys or keys that don't match our lookup value
                var filteredMetaVariables = metaVariables
                    .Where( v => v.keysPath.Count == ( lookupKeys.Count + 1 ) )
                    .Where( v => string.Join( ".", v.keysPath ).StartsWith( string.Join( ".", lookupKeys ) ) )
                    .ToList();

                // Remove any redundant localized names
                var localizedNameVar = filteredMetaVars.FirstOrDefault( v => v.keysPath.Last() == "localizedName" );
                if ( filteredMetaVars.Any( v => localizedNameVar != null &&
                                                v.keysPath.Last() == "name" &&
                                                v.value == localizedNameVar.value ) )
                {
                    filteredMetaVars.Remove( localizedNameVar );
                }

                return filteredMetaVariables;
            }

            // Resolve any direct function invocations (e.g. `{function(x).`)
            if ( !filteredMetaVars.Any() )
            {
                if ( lookupKeys[ 0 ].Contains( "(" ) )
                {
                    var functionKey = Regex.Replace( lookupKeys[ 0 ], @"(?=\().*", "" );
                    // If a match is found then we won't need to search our metavariables for a match.
                    var customFunction = customFunctions.FirstOrDefault( f => f.name == functionKey );
                    if ( customFunction != null )
                    {
                        var unfilteredMetaVars = new MetaVariables( customFunction.ReturnType, null, lookupKeys.Count + 1 ).Results;
                        unfilteredMetaVars.ForEach( mV =>
                            mV.keysPath = mV.keysPath.Prepend( lookupKeys[ 0 ] ).ToList() );
                        filteredMetaVars = FilterMetaVars( unfilteredMetaVars );
                    }
                }
            }

            // Resolve any function aliases (e.g. `{set a to function()} {a.`).
            if ( !filteredMetaVars.Any() )
            {
                var functionAliases = Regex.Matches( priorText, @"{set (?<key>\w*) to (?<function>\w*(?=\(.*\).*}))" );

                foreach ( var obj in functionAliases )
                {
                    if ( obj is Match match )
                    {
                        if ( lookupKeys[ 0 ] == match.Groups[ "key" ].Value )
                        {
                            // If a match is found then we won't need to search our metavariables for a match.
                            var customFunction =
                                customFunctions.FirstOrDefault( f => f.name == match.Groups[ "function" ].Value );
                            if ( customFunction != null )
                            {
                                var unfilteredMetaVars = new MetaVariables( customFunction.ReturnType, null, lookupKeys.Count + 1 ).Results;
                                unfilteredMetaVars.ForEach( mV =>
                                    mV.keysPath = mV.keysPath.Prepend( lookupKeys[ 0 ] ).ToList() );
                                filteredMetaVars = FilterMetaVars( unfilteredMetaVars );
                            }
                        }
                    }
                }
            }

            // Search our compiled metavariables list for a matching key.
            if ( !filteredMetaVars.Any() )
            {
                lock ( metaVarLock )
                {
                    filteredMetaVars = FilterMetaVars(metaVars);
                }
            }

            // Generate textCompletionItems
            foreach ( var item in filteredMetaVars.OrderBy(v => string.Concat(v.keysPath, '.')))
            {
                var itemKey = item.keysPath.Last();
                if (!string.IsNullOrEmpty(itemKey) &&
                    textCompletionItems.All(d => d.Text != itemKey) &&
                    MetaVariables.indexMarker != itemKey)
                {
                    if (item.type == typeof(bool))
                    {
                        textCompletionItems.Add(new TextCompletionItem(itemKey, typeof(Cottle.Values.BooleanValue),
                            item.description));
                    }
                    else if (item.type == typeof(int) ||
                             item.type == typeof(double) ||
                             item.type == typeof(float) ||
                             item.type == typeof(long) ||
                             item.type == typeof(ulong))
                    {
                        // Convert int, doubles, floats, and longs to number values
                        textCompletionItems.Add(new TextCompletionItem(itemKey, typeof(Cottle.Values.NumberValue),
                            item.description));
                    }
                    else if (item.type == typeof(string))
                    {
                        textCompletionItems.Add(new TextCompletionItem(itemKey, typeof(Cottle.Values.StringValue),
                            item.description));
                    }
                    else if ( item.type == typeof( IEnumerable<>) )
                    {
                        textCompletionItems.Add(new TextCompletionItem(itemKey, typeof(Cottle.Values.MapValue),
                            item.description));
                    }
                    else
                    {
                        textCompletionItems.Add(new TextCompletionItem(itemKey, item.type, item.description));
                    }
                }
            }

            return textCompletionItems;
        }

        private void ScriptView_TextArea_TextEntering ( object sender, TextCompositionEventArgs e )
        {
            if ( e.Text.Length > 0 && completionWindow != null )
            {
                if ( !char.IsLetterOrDigit( e.Text[ 0 ] ) )
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    completionWindow.CompletionList.RequestInsertion( e );
                }
            }
            // do not set e.Handled=true - we still want to insert the character that was typed
        }

        public void AddStandardMetaVariables ( IEnumerable<MetaVariable> stdMetaVars )
        {
            lock ( metaVarLock )
            {
                metaVars.AddRange(stdMetaVars);
            }
        }

        protected override void OnClosed ( EventArgs e )
        {
            base.OnClosed( e );
            ScriptRecoveryService.StopScriptRecovery();
        }

        private void acceptButtonClick ( object sender, RoutedEventArgs e )
        {
            if ( isNewOrRecoveredScript
                || script?.Name != editorScript.Name
                || script?.Description != editorScript.Description
                || script?.Value != editorScript.Value )
            {
                // Update the output script
                script = editorScript;

                // Make sure default values are set as required
                // ReSharper disable once InlineOutVariableDeclaration - Continuous Integration seems to require this variable be declared separately rather than in-line
#pragma warning disable IDE0018 // Inline variable declaration
                Script defaultScript = null;
#pragma warning restore IDE0018 // Inline variable declaration
                if ( Personality.Default().Scripts?.TryGetValue( script.Name, out defaultScript ) ?? false )
                {
                    script = Personality.UpgradeScript( script, defaultScript );
                }

                DialogResult = true;
            }
            else
            {
                DialogResult = false;
            }
            Close();
        }

        private void cancelButtonClick ( object sender, RoutedEventArgs e )
        {
            DialogResult = false;
            Close();
        }

        private void helpButtonClick ( object sender, RoutedEventArgs e )
        {
            MarkdownWindow helpWindow = new MarkdownWindow("Help.md");
            helpWindow.Show();
        }

        private void variablesButtonClick ( object sender, RoutedEventArgs e )
        {
            VariablesWindow variablesWindow = new VariablesWindow(editorScript);
            variablesWindow.Show();
        }

        private void resetButtonClick ( object sender, RoutedEventArgs e )
        {
            // Resetting the script resets it to its value in the default personality
            editorScript.Value = editorScript.defaultValue;
            scriptView.Text = editorScript.Value;
        }

        private void testButtonClick ( object sender, RoutedEventArgs e )
        {
            if ( SpeechService.Instance.eddiAudioPlaying & !SpeechService.Instance.eddiSpeaking )
            {
                SpeechService.Instance.StopAudio();
            }
            else
            {
                if ( !SpeechService.Instance.eddiSpeaking )
                {
                    ScriptRecoveryService.SaveRecoveryScript( editorScript );

                    // Splice the new script in to the existing scripts
                    editorScript.Value = scriptView.Text;
                    Dictionary<string, Script> newScripts = new Dictionary<string, Script>(_scripts);
                    Script testScript = new Script(editorScript.Name, editorScript.Description, false, editorScript.Value);
                    newScripts.Remove( editorScript.Name );
                    newScripts.Add( editorScript.Name, testScript );

                    SpeechResponder speechResponder = new SpeechResponder();
                    speechResponder.Start();
                    speechResponder.TestScript( editorScript.Name, newScripts );
                }
                else
                {
                    SpeechService.Instance.ShutUp();
                    SpeechService.Instance.StopAudio();
                }
            }
        }

        private void showDiffButtonClick ( object sender, RoutedEventArgs e )
        {
            editorScript.Value = scriptView.Text;
            if ( !string.IsNullOrWhiteSpace( editorScript.defaultValue ) )
            {
                new ShowDiffWindow( editorScript.defaultValue, editorScript.Value ).Show();
            }
        }

        private void foldingButtonClick ( object sender, RoutedEventArgs e )
        {
            if ( sender is CheckBox )
            {
                InitializeOrUpdateFolding();
            }
        }

        private void InitializeOrUpdateFolding ()
        {
            if ( Folding.IsChecked ?? false )
            {
                if ( foldingMargin is null )
                {
                    foldingMargin = new FoldingMargin { FoldingManager = FoldingManager.Install( scriptView.TextArea ) };
                }
                foldingStrategy.UpdateFoldings( foldingMargin.FoldingManager, scriptView.Document );
            }
            else
            {
                if ( foldingMargin != null )
                {
                    foldingMargin.FoldingManager.Clear();
                    FoldingManager.Uninstall( foldingMargin.FoldingManager );
                }
                foldingMargin = null;
            }
        }

        // TODO: Variable descriptions on mouse hover?
        /*
        private void ScriptView_OnMouseHover ( object sender, MouseEventArgs e )
        {
            if ( sender is TextEditor textEditor  )
            {
                var mousePoint = e.GetPosition( textEditor );
                var textLocation = textEditor.GetPositionFromPoint( mousePoint )?.Location;
                if ( textLocation != null )
                {
                    if ( !textLocation.Value.IsEmpty ) { return; }
                    var line = textEditor.TextArea.TextView.Document.GetLineByNumber( textLocation.Value.Line );
                }
            }
        }
        */
    }
}
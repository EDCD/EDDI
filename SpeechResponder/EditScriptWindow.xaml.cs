using EddiSpeechResponder.Service;
using EddiSpeechService;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

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

        public ScriptRecoveryService ScriptRecoveryService { get; set; }

#pragma warning disable IDE0052 // Remove unused private members -- this may be used later
        private readonly DocumentHighlighter documentHighlighter;
#pragma warning restore IDE0052 // Remove unused private members

        private readonly AvalonEdit.FoldingStrategy foldingStrategy;
        private FoldingMargin foldingMargin;

        public EditScriptWindow(Script script, Dictionary<string, Script> scripts, [NotNull]AvalonEdit.CottleHighlighting cottleHighlighting)
        {
            InitializeComponent();
            DataContext = this;

            this._scripts = scripts;
            this.script = script;

            if (script == null)
            {
                // This is a new script
                editorScript = new Script("New script", null, false, null);
            }
            else
            {
                // This is an existing script
                editorScript = script.Copy();
            }

            // See if there is the default value for this script is empty
            if (string.IsNullOrWhiteSpace(editorScript.defaultValue))
            {
                // No default; disable reset and show
                showDiffButton.IsEnabled = false;
                resetToDefaultButton.IsEnabled = false;
            }

            // Set our editor content
            scriptView.Text = editorScript.Value;

            // Set up our Script Recovery Service
            ScriptRecoveryService = new ScriptRecoveryService(this);
            ScriptRecoveryService.BeginScriptRecovery();
            scriptView.TextChanged += ScriptView_TextChanged;

            // Implement collapsible sections (called `Foldings`)
            foldingStrategy = new AvalonEdit.FoldingStrategy('{', '}');
            foldingStrategy.CreateNewFoldings(scriptView.Document);
            InitializeOrUpdateFolding();
            scriptView.Options.AllowScrollBelowDocument = true;

            // Set up our search window
            SearchPanel.Install(scriptView);

            // Set up our Cottle highlighter
            documentHighlighter = new DocumentHighlighter(scriptView.Document, cottleHighlighting.Definition);

            // Monitor window size and position
            WindowStartupLocation = WindowStartupLocation.Manual;
            SourceInitialized += EditScriptWindow_SourceInitialized;
            Loaded += OnLoaded;
            Closed += EditScriptWindow_SaveWindowStatePosition;
            LocationChanged += EditScriptWindow_SaveWindowStatePosition;
            SizeChanged += EditScriptWindow_SaveWindowStatePosition;
            StateChanged += EditScriptWindow_SaveWindowStatePosition;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Don't allow the window to start minimized
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
                Visibility = Visibility.Visible;
            }
        }

        private void EditScriptWindow_SaveWindowStatePosition(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                Properties.Settings.Default.Save();
            }
        }

        private void EditScriptWindow_SourceInitialized(object sender, EventArgs e)
        {
            // Validate window position on opening
            int designedHeight = (int)MinHeight;
            int designedWidth = (int)MinWidth;

            // WPF uses DPI scaled units rather than true pixels.
            // Retrieve the DPI scaling for the controlling monitor (where the top left pixel is located).
            var dpiScale = VisualTreeHelper.GetDpi(this);
            var windowPosition = new Rect(new Point(Properties.Settings.Default.Left, Properties.Settings.Default.Top), new Size(Properties.Settings.Default.Width, Properties.Settings.Default.Height));
            if (windowPosition == Rect.Empty || !isWindowValid(windowPosition, dpiScale))
            {
                // Revert to default values if the prior size and position are no longer valid
                Left = centerWindow(applyDpiScale(Screen.PrimaryScreen.Bounds.Width, dpiScale.DpiScaleX),
                    designedWidth);
                Top = centerWindow(applyDpiScale(Screen.PrimaryScreen.Bounds.Height, dpiScale.DpiScaleY),
                    designedHeight);
                Width = Math.Min(Screen.PrimaryScreen.Bounds.Width / dpiScale.DpiScaleX, designedWidth);
                Height = Math.Min(Screen.PrimaryScreen.Bounds.Height / dpiScale.DpiScaleY, designedHeight);
            }

            // Check detected monitors to see if the saved window size and location is valid
            bool isWindowValid(Rect rect, DpiScale dpi)
            {
                // Check for minimum window size
                if ((int)rect.Width < designedWidth || (int)rect.Height < designedHeight)
                {
                    return false;
                }

                // Check whether the rectangle is completely visible on-screen
                bool testUpperLeft = false;
                bool testLowerRight = false;
                foreach (Screen screen in Screen.AllScreens)
                {
                    if (rect.X >= applyDpiScale(screen.Bounds.X, dpi.DpiScaleX) && rect.Y >= applyDpiScale(screen.Bounds.Y, dpi.DpiScaleY)) // The upper and left bounds fall on a valid screen
                    {
                        testUpperLeft = true;
                    }
                    if (applyDpiScale(screen.Bounds.Width, dpi.DpiScaleX) >= rect.X + rect.Width && applyDpiScale(screen.Bounds.Height, dpi.DpiScaleY) >= rect.Y + rect.Height) // The lower and right bounds fall on a valid screen 
                    {
                        testLowerRight = true;
                    }
                }
                if (testUpperLeft && testLowerRight)
                {
                    return true;
                }
                return false;
            }

            int centerWindow(int measure, int defaultValue)
            {
                return (measure - Math.Min(measure, defaultValue)) / 2;
            }

            int applyDpiScale(int originalValue, double dpiScaleFactor)
            {
                return (int)Math.Round(originalValue / dpiScaleFactor);
            }
        }

        private void ScriptView_TextChanged(object sender, System.EventArgs e)
        {
            editorScript.Value = scriptView.Text;
            InitializeOrUpdateFolding();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ScriptRecoveryService.StopScriptRecovery();
        }

        private void acceptButtonClick(object sender, RoutedEventArgs e)
        {
            if (script?.Name != editorScript.Name 
                || script?.Description != editorScript.Description 
                || script?.Value != editorScript.Value)
            {
                // Update the output script
                script = editorScript;

                // Make sure default values are set as required
                // ReSharper disable once InlineOutVariableDeclaration - Continuous Integration seems to require this variable be declared separately rather than in-line
#pragma warning disable IDE0018 // Inline variable declaration
                Script defaultScript = null;
#pragma warning restore IDE0018 // Inline variable declaration
                if (Personality.Default().Scripts?.TryGetValue(script.Name, out defaultScript) ?? false)
                {
                    script = Personality.UpgradeScript(script, defaultScript);
                }

                DialogResult = true;
            }
            else
            {
                DialogResult = false;
            }
            this.Close();
        }

        private void cancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }

        private void helpButtonClick(object sender, RoutedEventArgs e)
        {
            MarkdownWindow helpWindow = new MarkdownWindow("Help.md");
            helpWindow.Show();
        }

        private void variablesButtonClick(object sender, RoutedEventArgs e)
        {
            VariablesWindow variablesWindow = new VariablesWindow(editorScript);
            variablesWindow.Show();
        }

        private void resetButtonClick(object sender, RoutedEventArgs e)
        {
            // Resetting the script resets it to its value in the default personality
            editorScript.Value = editorScript.defaultValue;
            scriptView.Text = editorScript.Value;
        }

        private void testButtonClick(object sender, RoutedEventArgs e)
        {
            if (SpeechService.Instance.eddiAudioPlaying & !SpeechService.Instance.eddiSpeaking)
            {
                SpeechService.Instance.StopAudio();
            }
            else
            {
                if (!SpeechService.Instance.eddiSpeaking)
                {
                    ScriptRecoveryService.SaveRecoveryScript(editorScript);

                    // Splice the new script in to the existing scripts
                    editorScript.Value = scriptView.Text;
                    Dictionary<string, Script> newScripts = new Dictionary<string, Script>(_scripts);
                    Script testScript = new Script(editorScript.Name, editorScript.Description, false, editorScript.Value);
                    newScripts.Remove(editorScript.Name);
                    newScripts.Add(editorScript.Name, testScript);

                    SpeechResponder speechResponder = new SpeechResponder();
                    speechResponder.Start();
                    speechResponder.TestScript(editorScript.Name, newScripts);
                }
                else
                {
                    SpeechService.Instance.ShutUp();
                    SpeechService.Instance.StopAudio();
                }
            }
        }

        private void showDiffButtonClick(object sender, RoutedEventArgs e)
        {
            editorScript.Value = scriptView.Text;
            if (!string.IsNullOrWhiteSpace(editorScript.defaultValue))
            {
                new ShowDiffWindow(editorScript.defaultValue, editorScript.Value).Show();
            }
        }

        private void foldingButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.CheckBox)
            {
                InitializeOrUpdateFolding();
            }
        }

        private void InitializeOrUpdateFolding()
        {
            if (Folding.IsChecked ?? false)
            {
                if (foldingMargin is null)
                {
                    foldingMargin = new FoldingMargin { FoldingManager = FoldingManager.Install(scriptView.TextArea) };
                }
                foldingStrategy.UpdateFoldings(foldingMargin.FoldingManager, scriptView.Document);
            }
            else
            {
                if (foldingMargin != null)
                {
                    foldingMargin.FoldingManager.Clear();
                    FoldingManager.Uninstall(foldingMargin.FoldingManager);
                }
                foldingMargin = null;
            }
        }
    }
}
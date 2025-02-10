using EddiCore;
using EddiSpeechService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EddiSpeechResponder
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class HotkeysWindow : Window
    {
        public HotkeysWindow ()
        {
            InitializeComponent();
            actionComboBox.SelectedIndex = 0;

            // Configure hotkeys
            ConfigureHotkeys();
        }

        private readonly HashSet<Key> pressedKeys = new HashSet<Key>();
        private KeyGesture currentKeyGesture;
        private static readonly Dictionary<string, KeyGesture> actionKeyGestures = new Dictionary<string, KeyGesture>();

        private void ConfigureHotkeys ()
        {
            // Populate actionComboBox with available actions
            actionComboBox.ItemsSource = new Dictionary<string, Action>
            {
                { "Enable Event Responses", () =>
                {
                    EDDI.Instance.State["speechresponder_quiet"] = false;
                } },
                { "Disable Event Responses", () =>
                {
                    EDDI.Instance.State["speechresponder_quiet"] = true;
                    SpeechService.Instance.ShutUp();
                } },
                { "Stop the Current Speech", () =>
                {
                    SpeechService.Instance.ShutUp();
                } }
            };
            actionComboBox.DisplayMemberPath = "Key";
        }

        private void ActionComboBoxOnSelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            // Start capturing the hotkey
            currentKeyGesture = null;
            if ( actionComboBox.SelectedItem is KeyValuePair<string, Action> selectedAction )
            {
                if ( actionKeyGestures.TryGetValue( selectedAction.Key, out var savedKeyGesture ) )
                {
                    hotkeyTextBlock.Text = savedKeyGesture.GetDisplayStringForCulture( CultureInfo.CurrentCulture );
                }
                else
                {
                    hotkeyTextBlock.Text = "Press the desired key combination.";
                }
            }
        }

        private void RegisterHotkeyButton_KeyDown ( object sender, KeyEventArgs e )
        {
            // Capture the key and modifier keys
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            var modifiers = Keyboard.Modifiers;

            // Add the key to the pressed keys set
            pressedKeys.Add( key );

            // Validate the key and modifier combination
            if ( IsValidKeyGesture( key, modifiers ) )
            {
                // Create a KeyGesture
                currentKeyGesture = new KeyGesture( key, modifiers );

                // Display the hotkey in the TextBlock
                hotkeyTextBlock.Text = currentKeyGesture.GetDisplayStringForCulture( CultureInfo.CurrentCulture ) ??
                                       currentKeyGesture.GetDisplayStringForCulture( CultureInfo.InvariantCulture );

                // Prevent the default behavior
                e.Handled = true;
            }
            else
            {
                currentKeyGesture = null;
            }
        }

        private bool IsValidKeyGesture ( Key key, ModifierKeys modifiers )
        {
            // Avoid using the same key as both the main key and a modifier
            if ( ( modifiers.HasFlag( ModifierKeys.Control ) && key == Key.LeftCtrl ) ||
                 ( modifiers.HasFlag( ModifierKeys.Control ) && key == Key.RightCtrl ) ||
                 ( modifiers.HasFlag( ModifierKeys.Alt ) && key == Key.LeftAlt ) ||
                 ( modifiers.HasFlag( ModifierKeys.Alt ) && key == Key.RightAlt ) ||
                 ( modifiers.HasFlag( ModifierKeys.Shift ) && key == Key.LeftShift ) ||
                 ( modifiers.HasFlag( ModifierKeys.Shift ) && key == Key.RightShift ) )
            {
                return false;
            }

            // Avoid using function keys (F1-F12) with Alt modifier
            if ( modifiers.HasFlag( ModifierKeys.Alt ) && key >= Key.F1 && key <= Key.F12 )
            {
                return false;
            }

            // Avoid using Shift with certain keys
            if ( modifiers.HasFlag( ModifierKeys.Shift ) && ( key == Key.LeftShift || key == Key.RightShift ) )
            {
                return false;
            }

            return true;
        }

        private void RegisterHotkeyButton_KeyUp ( object sender, KeyEventArgs e )
        {
            // Remove the key from the pressed keys set
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            pressedKeys.Remove( key );

            // Check if all keys are released
            if ( pressedKeys.Count == 0 && currentKeyGesture != null )
            {
                // Register the hotkey
                RegisterHotkey();
            }

            // Prevent the default behavior
            e.Handled = true;
        }

        private void RegisterHotkey ()
        {
            if ( currentKeyGesture != null && actionComboBox.SelectedItem is KeyValuePair<string, Action> hotkeyAction )
            {
                // Register the hotkey
                EDDI.Instance.HotkeyManager.RegisterHotkey( currentKeyGesture, hotkeyAction.Value );

                // Save the key gesture for the action
                actionKeyGestures[ hotkeyAction.Key ] = currentKeyGesture;

                // Provide feedback to the user
                hotkeyTextBlock.Text = currentKeyGesture.GetDisplayStringForCulture( CultureInfo.CurrentCulture );
            }
        }

        private void acceptButtonClick ( object sender, RoutedEventArgs e )
        {
            DialogResult = true;
            Close();
        }

        private void cancelButtonClick ( object sender, RoutedEventArgs e )
        {
            DialogResult = false;
            Close();
        }
    }
}

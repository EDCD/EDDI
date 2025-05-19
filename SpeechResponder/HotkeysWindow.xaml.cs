using EddiCore;
using EddiSpeechService;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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

        public readonly HotkeyActionCollection HotkeyActionCollection = new HotkeyActionCollection(
            new List<HotkeyAction>
            {
                new HotkeyAction( "EnableEventResponses", "Enable Event Responses", () =>
                {
                    EDDI.Instance.State[ "speechresponder_quiet" ] = false;
                } ),
                new HotkeyAction( "DisableEventResponses", "Disable Event Responses", () =>
                {
                    EDDI.Instance.State[ "speechresponder_quiet" ] = true;
                    SpeechService.Instance.ShutUp();
                } ),
                new HotkeyAction( "Shutup", "Stop the Current Speech", () =>
                {
                    SpeechService.Instance.ShutUp();
                } )
            } );

        private void ConfigureHotkeys ()
        {
            // Populate actionComboBox with available actions
            actionComboBox.ItemsSource = HotkeyActionCollection.HotkeyActions;
            actionComboBox.DisplayMemberPath = nameof(HotkeyAction.DisplayName);
        }

        private void ActionComboBoxOnSelectionChanged ( object sender, SelectionChangedEventArgs e )
        {
            // Start capturing the hotkey
            currentKeyGesture = null;
            if ( actionComboBox.SelectedItem is HotkeyAction selectedAction )
            {
                hotkeyTextBlock.Text = selectedAction.KeyGesture != null
                    ? selectedAction.KeyGesture.GetDisplayStringForCulture( CultureInfo.CurrentCulture )
                    : "Press the desired key combination.";
            }
        }

        private void HotkeysWindow_KeyDown ( object sender, KeyEventArgs e )
        {
            // Cancel hotkey registration if Escape is pressed
            if ( e.Key == Key.Escape )
            {
                pressedKeys.Clear();
                currentKeyGesture = null;
                hotkeyTextBlock.Text = "Hotkey registration canceled.";
                Task.Run( () =>
                {
                    Task.Delay( TimeSpan.FromSeconds( 5 ) );
                    hotkeyTextBlock.Text = "Press the desired key combination.";
                } );
                e.Handled = true;
                return;
            }

            // Capture the key and modifier keys
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            var modifiers = Keyboard.Modifiers;

            // Add the key to the pressed keys set
            if ( pressedKeys.Add( key ) )
            {
                // Validate the key and modifier combination
                if ( IsValidKeyGesture( pressedKeys, modifiers ) )
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
        }

        private bool IsValidKeyGesture ( HashSet<Key> keys, ModifierKeys modifiers )
        {
            var modifierKeySet = new HashSet<Key>
            {
                Key.LeftCtrl,
                Key.RightCtrl,
                Key.LeftAlt,
                Key.RightAlt,
                Key.LeftShift,
                Key.RightShift,
                Key.LWin,
                Key.RWin
            };
            var primaryKeys = keys.Except( modifierKeySet ).ToList();

            // Disallow combinations with only modifier keys
            if ( primaryKeys.Count == 0 )
            {
                hotkeyTextBlock.Text = "A non-modifier key is required.";
                return false;
            }

            // Disallow combinations with multiple primary keys
            if ( primaryKeys.Count > 1 )
            {
                hotkeyTextBlock.Text = "Only one non-modifier key is permitted.";
                return false;
            }

            // We have confirmed that we have exactly one primary key
            var key = primaryKeys[ 0 ];

            // Disallow single alphanumeric keys without modifiers
            if ( modifiers == ModifierKeys.None &&
                 ( ( key >= Key.A && key <= Key.Z ) || ( key >= Key.D0 && key <= Key.D9 ) ) )
            {
                hotkeyTextBlock.Text = "Please add a modifier key (Ctrl, Alt, Shift).";
                return false;
            }

            // Disallow Shift as the only modifier for alphanumerics, numpad, and Oem keys
            if ( modifiers == ModifierKeys.Shift )
            {
                if ( ( key >= Key.A && key <= Key.Z ) ||
                     ( key >= Key.D0 && key <= Key.D9 ) ||
                     ( key >= Key.NumPad0 && key <= Key.NumPad9 ) ||
                     key.ToString().StartsWith( "Oem" ) )
                {
                    hotkeyTextBlock.Text = "Please add another modifier key (Ctrl, Alt).";
                    return false;
                }
            }

            // Disallow reserved/system shortcuts
            if ( ( modifiers == ModifierKeys.Alt && key == Key.F4 ) ||
                 ( modifiers == ( ModifierKeys.Control | ModifierKeys.Alt ) && key == Key.Delete ) ||
                 ( modifiers == ModifierKeys.Control && ( key == Key.C || key == Key.V || key == Key.X ) ) )
            {
                hotkeyTextBlock.Text = "This key combination is reserved by the system.";
                return false;
            }

            // Disallow duplicate hotkeys
            if ( actionComboBox.SelectedItem is HotkeyAction selectedAction )
            {
                if ( HotkeyActionCollection.IsKeyGestureAssigned(selectedAction.Name, key, modifiers) )
                {
                    hotkeyTextBlock.Text = "This key combination is already assigned to another action.";
                    return false;
                }
            }

            // If all checks pass, the combination is valid
            return true;
        }

        private void HotkeysWindow_KeyUp ( object sender, KeyEventArgs e )
        {
            // Remove the key from the pressed keys set
            var key = e.Key == Key.System ? e.SystemKey : e.Key;

            pressedKeys.Remove( key );

            // Check if all keys are released
            if ( pressedKeys.Count == 0 )
            {
                if ( currentKeyGesture != null )
                {
                    // Record the hotkey
                    RecordHotkey();
                }
                else
                {
                    hotkeyTextBlock.Text = "Press the desired key combination.";
                }
            }

            // Prevent the default behavior
            e.Handled = true;
        }

        private void RecordHotkey ()
        {
            if ( IsActive && currentKeyGesture != null && actionComboBox.SelectedItem is HotkeyAction hotkeyAction )
            {
                // Save the key gesture for the action
                HotkeyActionCollection.AddGesture( hotkeyAction.Name, currentKeyGesture );

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

        private void ClearHotkeyButton_Click ( object sender, RoutedEventArgs e )
        {
            if ( actionComboBox.SelectedItem is HotkeyAction hotkeyAction )
            {
                hotkeyAction.KeyGesture = null;
            }
        }
    }

    public class HotkeyAction
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public Action Action { get; set; }
        public KeyGesture KeyGesture { get; set; }

        public HotkeyAction ( string name, string displayName, Action action, KeyGesture keyGesture = null )
        {
            Name = name;
            DisplayName = displayName;
            Action = action;
            KeyGesture = keyGesture;
        }
    }

    public class HotkeyActionCollection
    {
        public List<HotkeyAction> HotkeyActions { get; }

        public HotkeyActionCollection ( List<HotkeyAction> hotkeyActions )
        {
            HotkeyActions = hotkeyActions;
        }

        public void AddGesture ( string name, KeyGesture gesture )
        {
            var action = HotkeyActions.FirstOrDefault( a => a.Name == name );
            if ( action != null )
            {
                action.KeyGesture = gesture;
            }
        }

        public bool IsKeyGestureAssigned (string name, Key key, ModifierKeys modifiers )
        {
            var action = HotkeyActions.FirstOrDefault( a => a.Name == name );
            if ( action != null )
            {
                return HotkeyActions.Any( a =>
                    a.Name != name && 
                    a.KeyGesture != null && 
                    a.KeyGesture.Key == key &&
                    a.KeyGesture.Modifiers == modifiers );
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EddiCore.Hotkeys
{
    public partial class HotkeysWindow : Window
    {
        private HotkeyManager hotkeyManager { get; }
        public HotkeyActionCollection HotkeyActionCollection { get; }

        public HotkeysWindow ( HotkeyManager hkm )
        {
            hotkeyManager = hkm;
            HotkeyActionCollection = hkm.Hotkeys.Collection;

            InitializeComponent();
            actionComboBox.SelectedIndex = 0;

            // Configure hotkeys
            ConfigureHotkeys();
        }

        private readonly HashSet<Key> pressedKeys = [ ];
        private KeyGesture currentKeyGesture;

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
                    : Properties.Resources.hotkey_input_prompt;
            }
        }

        private void HotkeysWindow_KeyDown ( object sender, KeyEventArgs e )
        {
            // Cancel hotkey registration if Escape is pressed
            if ( e.Key == Key.Escape )
            {
                pressedKeys.Clear();
                currentKeyGesture = null;
                hotkeyTextBlock.Text = Properties.Resources.hotkey_input_cancelled;
                Task.Run( () =>
                {
                    Task.Delay( TimeSpan.FromSeconds( 5 ) );
                    hotkeyTextBlock.Text = Properties.Resources.hotkey_input_prompt;
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
                if ( IsKeyGestureValid( pressedKeys, modifiers ) )
                {
                    try
                    {
                        // Create a KeyGesture
                        currentKeyGesture = new KeyGesture( key, modifiers );

                        // Display the hotkey in the TextBlock
                        hotkeyTextBlock.Text = currentKeyGesture.GetDisplayStringForCulture( CultureInfo.CurrentCulture ) ??
                                               currentKeyGesture.GetDisplayStringForCulture( CultureInfo.InvariantCulture );

                        // Prevent the default behavior
                        e.Handled = true;
                    }
                    catch ( System.NotSupportedException )
                    {
                        currentKeyGesture = null;
                    }
                }
                else
                {
                    currentKeyGesture = null;
                }
            }
        }

        private bool IsKeyGestureValid ( HashSet<Key> keys, ModifierKeys modifiers )
        {
            var modifierKeySet = new HashSet<Key>
            {
                Key.LeftCtrl, Key.RightCtrl,
                Key.LeftAlt, Key.RightAlt,
                Key.LeftShift, Key.RightShift,
                Key.LWin, Key.RWin
            };
            var primaryKeys = keys.Except( modifierKeySet ).ToList();

            // Disallow combinations with only modifier keys
            if ( primaryKeys.Count == 0 )
            {
                hotkeyTextBlock.Text = Properties.Resources.hotkey_input_need_nonmodifier;
                return false;
            }

            // Disallow combinations with multiple primary keys
            if ( primaryKeys.Count > 1 )
            {
                hotkeyTextBlock.Text = Properties.Resources.hotkey_input_need_one_nonmodifier;
                return false;
            }

            // We have confirmed that we have exactly one primary key
            var key = primaryKeys[ 0 ];

            // Disallow single alphanumeric keys without modifiers
            if ( modifiers == ModifierKeys.None &&
                 ( key is >= Key.A and <= Key.Z || key is >= Key.D0 and <= Key.D9 ) )
            {
                hotkeyTextBlock.Text = Properties.Resources.hotkey_input_need_modifier;
                return false;
            }

            // Disallow Shift as the only modifier for alphanumerics, numpad, and Oem keys
            if ( modifiers == ModifierKeys.Shift && ( key is >= Key.A and <= Key.Z ||
                                                      key is >= Key.D0 and <= Key.D9 ||
                                                      key is >= Key.NumPad0 and <= Key.NumPad9 ||
                                                      key.ToString().StartsWith( "Oem" ) ) )
            {
                hotkeyTextBlock.Text = Properties.Resources.hotkey_input_need_more_modifiers;
                return false;
            }

            // Disallow reserved/system shortcuts
            if ( ( modifiers == ModifierKeys.Alt && key == Key.F4 ) ||
                 ( modifiers == ( ModifierKeys.Control | ModifierKeys.Alt ) && key == Key.Delete ) ||
                 ( modifiers == ModifierKeys.Control && ( key == Key.C || key == Key.V || key == Key.X ) ) )
            {
                hotkeyTextBlock.Text = Properties.Resources.hotkey_input_reserved;
                return false;
            }

            // Disallow duplicate hotkeys
            if ( actionComboBox.SelectedItem is HotkeyAction selectedAction && 
                 HotkeyActionCollection.IsKeyGestureAssigned(selectedAction.Name, key, modifiers) )
            {
                hotkeyTextBlock.Text = Properties.Resources.hotkey_input_already_used;
                return false;
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
                    hotkeyTextBlock.Text = Properties.Resources.hotkey_input_prompt;
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
                hotkeyTextBlock.Text = Properties.Resources.hotkey_input_prompt;
            }
        }
    }
}

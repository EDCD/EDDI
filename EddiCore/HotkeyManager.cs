using EddiConfigService;
using EddiSpeechService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace EddiCore
{
    public class HotkeyManager
    {
        private static readonly List<HotkeyAction> HotkeyActions = new List<HotkeyAction>
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
        };

        public HotkeyRegistration Hotkeys;
        private const int WM_HOTKEY = 0x0312;

        public void SetHandle ( IntPtr newHandle )
        {
            ConfigService.Instance.eddiConfiguration.Hotkeys = ConfigService.Instance.eddiConfiguration.Hotkeys ??
                                                               new Dictionary<string, string>();
            Hotkeys?.UnregisterAll();
            Hotkeys = new HotkeyRegistration( newHandle, new HotkeyActionCollection( HotkeyActions ) );
            Hotkeys.RegisterAll();
        }

        public IntPtr HandleHotkeyMessage ( int msg, IntPtr wParam, ref bool handled )
        {
            if ( msg == WM_HOTKEY )
            {
                var id = wParam.ToInt32();
                if ( Hotkeys.Collection?.TryGetValue( id, out var hotkeyAction ) ?? false )
                {
                    hotkeyAction.Action.Invoke();
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        public void RegisterHotkey ( string name, KeyGesture keyGesture ) => Hotkeys.RegisterHotkey( name, keyGesture );

        public void UnregisterHotkey ( string name ) => Hotkeys.UnregisterHotkey( name );

        public void UnregisterAllHotkeys () => Hotkeys.UnregisterAll();
    }

    public class HotkeyRegistration
    {
        public HotkeyActionCollection Collection { get; }

        private int hotkeyIdCounter;
        private readonly IntPtr handle;

        public HotkeyRegistration ( IntPtr handle, HotkeyActionCollection collection )
        {
            this.handle = handle;
            this.Collection = collection;
        }

        [DllImport( "user32.dll" )]
        private static extern bool RegisterHotKey ( IntPtr hWnd, int id, uint fsModifiers, uint vk );

        internal void RegisterAll ()
        {
            foreach ( var configuredHotkey in ConfigService.Instance.eddiConfiguration.Hotkeys )
            {
                if ( Collection.TryGetValue( configuredHotkey.Key, out var hotkeyAction ) )
                {
                    TryRegisterHotkey( hotkeyAction.Name, HotkeyConverter.FromString( configuredHotkey.Value ),
                        out var id );
                    Collection.AddId( hotkeyAction.Name, id );
                }
            }
        }

        public void RegisterHotkey ( string name, KeyGesture keyGesture )
        {
            if ( TryRegisterHotkey ( name, keyGesture, out var id ) )
            {
                Collection.AddGesture( name, keyGesture, id );
                ConfigService.Instance.eddiConfiguration.Hotkeys.Add( name, HotkeyConverter.ToString( keyGesture ) );
            }
            else
            {
                throw new InvalidOperationException( "Hotkey registration failed." );
            }
        }

        private bool TryRegisterHotkey ( string name, KeyGesture keyGesture, out int id )
        {
            if ( string.IsNullOrWhiteSpace( name ) )
            {
                throw new ArgumentNullException( nameof( name ) );
            }

            if ( keyGesture == null )
            {
                throw new ArgumentNullException( nameof( keyGesture ) );
            }

            // Unregister previous hotkey for this name, if any
            TryUnregisterHotkey( name );

            // Register a new hotkey
            var modifiers = (uint)keyGesture.Modifiers;
            var key = (uint)KeyInterop.VirtualKeyFromKey( keyGesture.Key );
            id = hotkeyIdCounter++;
            return RegisterHotKey( handle, id, modifiers, key );
        }

        private bool TryUnregisterHotkey ( string name )
        {
            // Unregister previous hotkey for this name, if any
            if ( Collection.TryGetValue( name, out var hotkeyAction ) && hotkeyAction.id is int oldId )
            {
                return UnregisterHotKey( handle, oldId );
            }

            return false;
        }

        [DllImport( "user32.dll" )]
        private static extern bool UnregisterHotKey ( IntPtr hWnd, int id );

        public void UnregisterHotkey ( string name )
        {
            if ( TryUnregisterHotkey( name ) )
            {
                Collection.RemoveKeyGestures( name );
                ConfigService.Instance.eddiConfiguration.Hotkeys.Remove( name );
            }
        }

        internal void UnregisterAll ()
        {
            foreach ( var hotkeyAction in Collection.HotkeyActions )
            {
                if ( hotkeyAction.id is int id )
                {
                    UnregisterHotKey( handle, id );
                }
            }

            Collection.ClearAllKeyGestures();
        }
    }

    public static class HotkeyConverter
    {
        private static readonly KeyGestureConverter keyGestureConverter = new KeyGestureConverter();

        public static KeyGesture FromString ( string keyGestureStr )
        {
            return keyGestureConverter.ConvertFromString( keyGestureStr ) as KeyGesture;
        }

        public static string ToString ( KeyGesture keyGesture )
        {
            return keyGestureConverter.ConvertToString( keyGesture );
        }
    }

    public class HotkeyAction
    {
        public int? id { get; set; }
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
        public HotkeyActionCollection ( List<HotkeyAction> hotkeyActions )
        {
            HotkeyActions = hotkeyActions;
        }

        public readonly List<HotkeyAction> HotkeyActions;

        public void AddGesture ( string name, KeyGesture gesture, int? id = null )
        {
            var action = HotkeyActions.FirstOrDefault( a => a.Name == name );
            if ( action != null )
            {
                action.KeyGesture = gesture;
                action.id = id;
            }
        }

        public void AddId ( string name, int id )
        {
            var action = HotkeyActions.FirstOrDefault( a => a.Name == name );
            if ( action != null )
            {
                action.id = id;
            }
        }

        public void ClearAllKeyGestures ()
        {
            foreach ( var action in HotkeyActions )
            {
                RemoveKeyGestures( action.Name );
            }
        }

        public bool IsKeyGestureAssigned ( string name, Key key, ModifierKeys modifiers )
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

        public void RemoveKeyGestures ( string name )
        {
            var action = HotkeyActions.FirstOrDefault( a => a.Name == name );
            if ( action != null )
            {
                action.id = null;
                action.KeyGesture = null;                
            }
        }

        public bool TryGetValue ( string name, out HotkeyAction action )
        {
            action = HotkeyActions.FirstOrDefault( a => a.Name == name );
            return action != null;
        }

        public bool TryGetValue ( int id, out HotkeyAction action )
        {
            action = HotkeyActions.FirstOrDefault( a => a.id != null && a.id == id );
            return action != null;
        }
    }
}

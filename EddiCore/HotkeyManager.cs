using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace EddiCore
{
    public class HotkeyManager
    {
        private const int WM_HOTKEY = 0x0312;
        private readonly Dictionary<string, Action> hotkeyActions = new Dictionary<string, Action>();
        private readonly Dictionary<string, int> nameToId = new Dictionary<string, int>();
        private readonly Dictionary<int, string> idToName = new Dictionary<int, string>();
        private int hotkeyIdCounter;
        private IntPtr handle;

        [DllImport( "user32.dll" ) ]
        private static extern bool RegisterHotKey ( IntPtr hWnd, int id, uint fsModifiers, uint vk );

        [ DllImport( "user32.dll" ) ]
        private static extern bool UnregisterHotKey ( IntPtr hWnd, int id );

        public void SetHandle ( IntPtr newHandle )
        {
            this.handle = newHandle;
        }

        public void UnregisterAllHotKeys ()
        {
            foreach ( var id in idToName.Keys )
            {
                UnregisterHotKey( handle, id );
            }
            hotkeyActions.Clear();
            nameToId.Clear();
            idToName.Clear();
        }

        public IntPtr HandleHotkeyMessage ( int msg, IntPtr wParam, ref bool handled )
        {
            if ( msg == WM_HOTKEY )
            {
                var id = wParam.ToInt32();
                if ( idToName.TryGetValue( id, out var name ) && hotkeyActions.TryGetValue( name, out var action ) )
                {
                    action.Invoke();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        public void RegisterHotkey ( string name, KeyGesture actionHotkey, Action action )
        {
            if ( string.IsNullOrWhiteSpace( name ) )
            {
                throw new ArgumentNullException( nameof( name ) );
            }
            if ( actionHotkey == null )
            {
                throw new ArgumentNullException( nameof( actionHotkey ) );
            }

            // Unregister previous hotkey for this name, if any
            if ( nameToId.TryGetValue( name, out var oldId ) )
            {
                UnregisterHotKey( handle, oldId );
                idToName.Remove( oldId );
                nameToId.Remove( name );
                hotkeyActions.Remove( name );
            }

            var modifiers = (uint)actionHotkey.Modifiers;
            var key = (uint)KeyInterop.VirtualKeyFromKey(actionHotkey.Key);
            var id = hotkeyIdCounter++;

            if ( RegisterHotKey( handle, id, modifiers, key ) )
            {
                hotkeyActions[ name ] = action;
                nameToId[ name ] = id;
                idToName[ id ] = name;
            }
            else
            {
                throw new InvalidOperationException( "Hotkey registration failed." );
            }
        }

        public void UnregisterHotkey ( string name )
        {
            if ( nameToId.TryGetValue( name, out var id ) )
            {
                UnregisterHotKey( handle, id );
                idToName.Remove( id );
                nameToId.Remove( name );
                hotkeyActions.Remove( name );
            }
        }
    }
}

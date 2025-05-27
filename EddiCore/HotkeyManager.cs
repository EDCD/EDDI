using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace EddiCore
{
    public class HotkeyManager
    {
        private const int WM_HOTKEY = 0x0312;
        private readonly Dictionary<int, Action> hotkeyActions = new Dictionary<int, Action>();
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
            foreach ( var id in hotkeyActions.Keys )
            {
                UnregisterHotKey( handle, id );
            }

            hotkeyActions.Clear();
        }

        public IntPtr HandleHotkeyMessage ( int msg, IntPtr wParam, ref bool handled )
        {
            if ( msg == WM_HOTKEY )
            {
                var id = wParam.ToInt32();
                if ( hotkeyActions.TryGetValue( id, out var action ) )
                {
                    action.Invoke();
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        public void RegisterHotkey(KeyGesture actionHotkey, Action action)
        {
            if (actionHotkey == null)
            {
                throw new ArgumentNullException(nameof(actionHotkey));
            }

            var modifiers = (uint)actionHotkey.Modifiers;
            var key = (uint)KeyInterop.VirtualKeyFromKey(actionHotkey.Key);
            var id = hotkeyIdCounter++;

            if (RegisterHotKey(handle, id, modifiers, key))
            {
                hotkeyActions[id] = action;
            }
            else
            {
                throw new InvalidOperationException("Hotkey registration failed.");
            }
        }
    }
}

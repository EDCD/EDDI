using EddiConfigService;
using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace EddiCore.Hotkeys
{
    public class HotkeyRegistration
    {
        public HotkeyActionCollection Collection { get; }

        private int hotkeyIdCounter;
        private readonly IntPtr handle;

        public HotkeyRegistration ( IntPtr handle, HotkeyActionCollection collection )
        {
            this.handle = handle;
            Collection = collection;
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
}
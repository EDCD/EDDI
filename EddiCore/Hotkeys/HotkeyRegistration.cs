using EddiConfigService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;
using Utilities;

namespace EddiCore.Hotkeys
{
    public class HotkeyRegistration : IDisposable
    {
        public HotkeyActionCollection Collection { get; }

        public HotkeyRegistration ( HotkeyActionCollection collection )
            : this( collection, installHook: true, invoke: null )
        {
            Collection = collection ?? throw new ArgumentNullException( nameof( collection ) );
            syncContext = SynchronizationContext.Current;
            StartHook();
        }

        internal HotkeyRegistration ( HotkeyActionCollection collection, bool installHook, Action<Action> invoke )
        {
            Collection = collection ?? throw new ArgumentNullException( nameof( collection ) );
            this.invoke = invoke ?? DefaultInvoke;
            if ( installHook )
            { StartHook(); }
        }

        private readonly Action<Action> invoke;
        private readonly SynchronizationContext syncContext = SynchronizationContext.Current;

        private void DefaultInvoke ( Action a )
        {
            if ( syncContext != null )
            {
                syncContext.Post( _ => a(), null );
            }
            else
            {
                a();
            }
        }

        // Hook internals
        private IntPtr hookHandle = IntPtr.Zero;
        private LowLevelKeyboardProc hookProc; // keep delegate alive
        private readonly HashSet<int> keysDown = new HashSet<int>();

        // Tracked modifier state
        private bool shiftDown;
        private bool ctrlDown;
        private bool altDown;
        private bool winDown;

        internal void RegisterAll ()
        {
            foreach ( var configuredHotkey in ConfigService.Instance.eddiConfiguration.GetHotkeysCopy() )
            {
                if ( Collection.TryGetValue( configuredHotkey.Key, out var hotkeyAction ) )
                {
                    var gesture = HotkeyConverter.FromString(configuredHotkey.Value);
                    RegisterHotkeyInternal( hotkeyAction.Name, gesture, persist: false );
                }
            }
        }

        public void RegisterHotkey ( string name, KeyGesture keyGesture )
            => RegisterHotkeyInternal( name, keyGesture, persist: true );

        private void RegisterHotkeyInternal ( string name, KeyGesture keyGesture, bool persist )
        {
            try
            {
                if ( string.IsNullOrWhiteSpace( name ) )
                {
                    throw new ArgumentNullException( nameof( name ) );
                }

                if ( keyGesture == null )
                {
                    throw new ArgumentNullException( nameof( keyGesture ) );
                }

                // Enforce uniqueness across actions
                if ( Collection.IsKeyGestureAssigned( name, keyGesture.Key, keyGesture.Modifiers ) )
                {
                    throw new InvalidOperationException( "Hotkey is already assigned to another action." );
                }

                // Overwrite previous gesture for this action
                Collection.AddGesture( name, keyGesture );

                if ( persist )
                {
                    ConfigService.Instance.eddiConfiguration.AddHotkey( name, HotkeyConverter.ToString( keyGesture ) );
                }
            }
            catch ( Exception e )
            {
                Logging.Error( e.Message, e );
            }
        }

        public void UnregisterHotkey ( string name )
        {
            try
            {
                Collection.RemoveKeyGestures( name );
                ConfigService.Instance.eddiConfiguration.RemoveHotkey( name );
            }
            catch ( Exception e )
            {
                Logging.Error( e.Message, e );
            }
        }

        internal void UnregisterAll ()
        {
            Collection.ClearAllKeyGestures();
        }

        public void Dispose ()
        {
            StopHook();
        }

        private void Fire ( HotkeyAction action )
        {
            if ( syncContext != null )
            {
                syncContext.Post( _ => InvokeAction(), null );
            }
            else
            {
                InvokeAction();
            }

            return;

            // Run on the installing thread’s SynchronizationContext (typically the WPF UI thread)
            void InvokeAction ()
            {
                try
                {
                    action.Action?.Invoke();
                }
                catch ( Exception e )
                {
                    Logging.Error( e.Message, e );
                }
            }
        }

        private ModifierKeys CurrentModifiers ()
        {
            var mods = ModifierKeys.None;
            if ( shiftDown )
            {
                mods |= ModifierKeys.Shift;
            }

            if ( ctrlDown )
            {
                mods |= ModifierKeys.Control;
            }

            if ( altDown )
            {
                mods |= ModifierKeys.Alt;
            }

            if ( winDown )
            {
                mods |= ModifierKeys.Windows;
            }
            return mods;
        }

        private static bool IsModifierVk ( int vk )
        {
            return vk == VK_SHIFT || vk == VK_LSHIFT || vk == VK_RSHIFT
                || vk == VK_CONTROL || vk == VK_LCONTROL || vk == VK_RCONTROL
                || vk == VK_MENU || vk == VK_LMENU || vk == VK_RMENU
                || vk == VK_LWIN || vk == VK_RWIN;
        }

        private void UpdateModifierState ( int vk, bool isDownEvent )
        {
            switch ( vk )
            {
                case VK_SHIFT:
                case VK_LSHIFT:
                case VK_RSHIFT:
                    shiftDown = isDownEvent;
                    break;
                case VK_CONTROL:
                case VK_LCONTROL:
                case VK_RCONTROL:
                    ctrlDown = isDownEvent;
                    break;
                case VK_MENU:
                case VK_LMENU:
                case VK_RMENU:
                    altDown = isDownEvent;
                    break;
                case VK_LWIN:
                case VK_RWIN:
                    winDown = isDownEvent;
                    break;
            }
        }

        private void StartHook ()
        {
            if ( hookHandle != IntPtr.Zero )
            {
                return;
            }

            hookProc = HookCallback;

            var moduleHandle = IntPtr.Zero;
            try
            {
                using ( var curProcess = Process.GetCurrentProcess() )
                {
                    using ( var curModule = curProcess.MainModule )
                    {
                        if ( curModule != null )
                        {
                            moduleHandle = GetModuleHandle( curModule.ModuleName );
                        }
                    }
                }
            }
            catch
            {
                // Best-effort. If module handle is IntPtr.Zero, SetWindowsHookEx may still succeed for WH_KEYBOARD_LL.
            }

            hookHandle = SetWindowsHookEx( WH_KEYBOARD_LL, hookProc, moduleHandle, 0 );
            if ( hookHandle == IntPtr.Zero )
            {
                Logging.Error( $"Failed to install keyboard hook. Win32Error={Marshal.GetLastWin32Error()}" );
            }
        }

        private void StopHook ()
        {
            if ( hookHandle == IntPtr.Zero )
            {
                return;
            }
            UnhookWindowsHookEx( hookHandle );
            hookHandle = IntPtr.Zero;

            keysDown.Clear();
            shiftDown = ctrlDown = altDown = winDown = false;
        }

        private IntPtr HookCallback ( int nCode, IntPtr wParam, IntPtr lParam )
        {
            // Must CallNextHookEx if nCode < 0
            if ( nCode < 0 )
            {
                return CallNextHookEx( hookHandle, nCode, wParam, lParam );
            }

            var msg = wParam.ToInt32();
            var isDownEvent = msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN;
            var isUpEvent = msg == WM_KEYUP || msg == WM_SYSKEYUP;

            if ( isDownEvent || isUpEvent )
            {
                var data = Marshal.PtrToStructure<Kbdllhookstruct>(lParam);
                var vk = unchecked((int)data.vkCode);

                // Track modifier state
                if ( IsModifierVk( vk ) )
                {
                    UpdateModifierState( vk, isDownEvent );
                }

                if ( isDownEvent )
                {
                    // Debounce repeats
                    if ( !keysDown.Add( vk ) )
                    {
                        return CallNextHookEx( hookHandle, nCode, wParam, lParam );
                    }

                    // Only evaluate for non-modifier key presses
                    if ( !IsModifierVk( vk ) )
                    {
                        var key = KeyInterop.KeyFromVirtualKey(vk);
                        var mods = CurrentModifiers();

                        if ( Collection.TryGetValue( key, mods, out var action ) )
                        {
                            Fire( action );
                        }
                    }
                }
                else
                {
                    keysDown.Remove( vk );
                }
            }

            // Always pass through: do NOT block other apps / hooks
            return CallNextHookEx( hookHandle, nCode, wParam, lParam );
        }

        internal void TestTrigger ( Key key, ModifierKeys modifiers )
        {
            if ( Collection.TryGetValue( key, modifiers, out var action ) )
            {
                invoke( () => action.Action?.Invoke() );
            }
        }

        // Win32 interop
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        private const int VK_SHIFT = 0x10;
        private const int VK_CONTROL = 0x11;
        private const int VK_MENU = 0x12; // ALT
        private const int VK_LSHIFT = 0xA0;
        private const int VK_RSHIFT = 0xA1;
        private const int VK_LCONTROL = 0xA2;
        private const int VK_RCONTROL = 0xA3;
        private const int VK_LMENU = 0xA4;
        private const int VK_RMENU = 0xA5;
        private const int VK_LWIN = 0x5B;
        private const int VK_RWIN = 0x5C;

        private delegate IntPtr LowLevelKeyboardProc ( int nCode, IntPtr wParam, IntPtr lParam );

        [StructLayout( LayoutKind.Sequential )]
        private struct Kbdllhookstruct
        {
            public uint vkCode;
            public uint scanCode;
            public uint flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [DllImport( "user32.dll", SetLastError = true )]
        private static extern IntPtr SetWindowsHookEx ( int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId );

        [DllImport( "user32.dll", SetLastError = true )]
        private static extern bool UnhookWindowsHookEx ( IntPtr hhk );

        [DllImport( "user32.dll" )]
        private static extern IntPtr CallNextHookEx ( IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam );

        [DllImport( "kernel32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        private static extern IntPtr GetModuleHandle ( string lpModuleName );
    }
}
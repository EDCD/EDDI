using EddiConfigService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Utilities;

namespace EddiCore.Hotkeys
{
    public partial class HotkeyRegistration : IDisposable
    {
        public HotkeyActionCollection Collection { get; }

        public HotkeyRegistration ( HotkeyActionCollection collection )
            : this( collection, installHook: true, invoke: null )
        { }

        internal HotkeyRegistration ( HotkeyActionCollection collection, bool installHook, Action<Action> invoke )
            : this( collection, installHook, invoke, null, null, true )
        { }

        internal HotkeyRegistration (
            HotkeyActionCollection collection,
            bool installHook,
            Action<Action> invoke,
            Func<IntPtr> installHookOverride,
            Action<string> logHookUnavailable,
            bool scheduleRetries )
        {
            Collection = collection ?? throw new ArgumentNullException( nameof( collection ) );
            this.invoke = invoke ?? DefaultInvoke;
            this.installHookOverride = installHookOverride;
            this.logHookUnavailable = logHookUnavailable ?? ( message => Logging.Warn( message ) );
            this.scheduleRetries = scheduleRetries;
            if ( installHook )
            { StartHook(); }
        }

        private readonly Action<Action> invoke;
        private readonly SynchronizationContext syncContext = SynchronizationContext.Current;
        private readonly Func<IntPtr> installHookOverride;
        private readonly Action<string> logHookUnavailable;
        private readonly bool scheduleRetries;

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
        private readonly HashSet<int> keysDown = [ ];
        private bool disposed;
        internal int HookInstallAttempts { get; private set; }
        internal bool IsHookInstalled => hookHandle != IntPtr.Zero;
        internal bool IsHookUnavailable => HookInstallAttempts >= MaxHookInstallAttempts && hookHandle == IntPtr.Zero;

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
                ArgumentNullException.ThrowIfNull( keyGesture );

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
            disposed = true;
            StopHook();
            GC.SuppressFinalize( this );
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
            return vk is VK_SHIFT or VK_LSHIFT or VK_RSHIFT or VK_CONTROL or VK_LCONTROL or VK_RCONTROL or VK_MENU or VK_LMENU or VK_RMENU or VK_LWIN or VK_RWIN;
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
            RetryHookInstallation();
        }

        internal bool RetryHookInstallation ()
        {
            if ( disposed || hookHandle != IntPtr.Zero || HookInstallAttempts >= MaxHookInstallAttempts )
            {
                return hookHandle != IntPtr.Zero;
            }

            hookProc = HookCallback;
            HookInstallAttempts++;

            var moduleHandle = IntPtr.Zero;
            var moduleName = string.Empty;
            var modulePath = string.Empty;
            var getModuleHandleWin32Error = 0;
            string moduleResolutionError = null;
            using var currentProcess = Process.GetCurrentProcess();
            if ( installHookOverride is null )
            {
                try
                {
                    using ( var curModule = currentProcess.MainModule )
                    {
                        if ( curModule != null )
                        {
                            moduleName = curModule.ModuleName;
                            modulePath = curModule.FileName;
                            moduleHandle = NativeMethods.GetModuleHandle( moduleName );
                            getModuleHandleWin32Error = moduleHandle == IntPtr.Zero ? Marshal.GetLastWin32Error() : 0;
                        }
                    }
                }
                catch ( Exception ex )
                {
                    // Best-effort. If module handle is IntPtr.Zero, SetWindowsHookEx may still succeed for WH_KEYBOARD_LL.
                    moduleResolutionError = $"{ex.GetType().Name}: {ex.Message}";
                }
            }

            hookHandle = installHookOverride?.Invoke() ?? NativeMethods.SetWindowsHookEx( WH_KEYBOARD_LL, hookProc, moduleHandle, 0 );
            var setHookWin32Error = hookHandle == IntPtr.Zero ? Marshal.GetLastWin32Error() : 0;
            if ( hookHandle == IntPtr.Zero )
            {
                logHookUnavailable(
                    "Failed to install keyboard hook. " +
                    $"Win32Error={setHookWin32Error}; " +
                    $"HookId={WH_KEYBOARD_LL}; " +
                    "ThreadId=0; " +
                    $"Attempt={HookInstallAttempts}/{MaxHookInstallAttempts}; " +
                    $"ModuleHandle=0x{moduleHandle.ToInt64():X}; " +
                    $"ModuleName='{moduleName}'; " +
                    $"ModulePath='{modulePath}'; " +
                    $"GetModuleHandleWin32Error={getModuleHandleWin32Error}; " +
                    $"ModuleResolutionError='{moduleResolutionError ?? "none"}'; " +
                    $"ProcessId={currentProcess.Id}; " +
                    $"ProcessName='{currentProcess.ProcessName}'; " +
                    $"Is64BitProcess={Environment.Is64BitProcess}; " +
                    $"Is64BitOperatingSystem={Environment.Is64BitOperatingSystem}; " +
                    $"UserInteractive={Environment.UserInteractive}; " +
                    $"HasSynchronizationContext={SynchronizationContext.Current != null}; " +
                    $"HasDispatcher={Dispatcher.FromThread( Thread.CurrentThread ) != null}" );

                ScheduleHookRetry();
                return false;
            }

            return true;
        }

        private void ScheduleHookRetry ()
        {
            if ( !scheduleRetries || disposed || hookHandle != IntPtr.Zero || HookInstallAttempts >= MaxHookInstallAttempts )
            {
                return;
            }

            var dispatcher = Dispatcher.FromThread( Thread.CurrentThread );
            if ( dispatcher is null )
            {
                return;
            }

            _ = dispatcher.InvokeAsync( async () =>
            {
                await Task.Delay( HookRetryDelayMilliseconds );
                RetryHookInstallation();
            }, DispatcherPriority.Background );
        }

        private void StopHook ()
        {
            if ( hookHandle == IntPtr.Zero )
            {
                return;
            }
            NativeMethods.UnhookWindowsHookEx( hookHandle );
            hookHandle = IntPtr.Zero;

            keysDown.Clear();
            shiftDown = ctrlDown = altDown = winDown = false;
        }

        private IntPtr HookCallback ( int nCode, IntPtr wParam, IntPtr lParam )
        {
            // Must CallNextHookEx if nCode < 0
            if ( nCode < 0 )
            {
                return NativeMethods.CallNextHookEx( hookHandle, nCode, wParam, lParam );
            }

            var msg = wParam.ToInt32();
            var isDownEvent = msg is WM_KEYDOWN or WM_SYSKEYDOWN;
            var isUpEvent = msg is WM_KEYUP or WM_SYSKEYUP;

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
                        return NativeMethods.CallNextHookEx( hookHandle, nCode, wParam, lParam );
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
            return NativeMethods.CallNextHookEx( hookHandle, nCode, wParam, lParam );
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
        private const int MaxHookInstallAttempts = 3;
        private const int HookRetryDelayMilliseconds = 5000;

        [UnmanagedFunctionPointer( CallingConvention.Winapi )]
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

        private partial class NativeMethods
        {
            [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowsHookExW")]
            [System.Diagnostics.CodeAnalysis.SuppressMessage( "Interoperability", "SYSLIB1054:Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time", Justification = "Code generation fails." )]
            private static extern IntPtr SetWindowsHookExImpl(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

            internal static IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId)
                => SetWindowsHookExImpl(idHook, lpfn, hMod, dwThreadId);

            [LibraryImport("user32.dll", SetLastError = true, EntryPoint = "UnhookWindowsHookEx")]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static partial bool UnhookWindowsHookExImpl(IntPtr hhk);

            internal static bool UnhookWindowsHookEx(IntPtr hhk)
                => UnhookWindowsHookExImpl(hhk);

            [LibraryImport("user32.dll", EntryPoint = "CallNextHookEx")]
            private static partial IntPtr CallNextHookExImpl(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            internal static IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam)
                => CallNextHookExImpl(hhk, nCode, wParam, lParam);

            [LibraryImport("kernel32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true, EntryPoint = "GetModuleHandleW")]
            private static partial IntPtr GetModuleHandleImpl(string lpModuleName);

            internal static IntPtr GetModuleHandle(string lpModuleName)
                => GetModuleHandleImpl(lpModuleName);
        }
    }
}

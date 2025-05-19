using EddiConfigService;
using EddiSpeechService;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Input;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiCore.Hotkeys
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
}

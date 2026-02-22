using EddiSpeechService;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiCore.Hotkeys
{
    public class HotkeyManager
    {
        private static readonly List<HotkeyAction> HotkeyActions = new List<HotkeyAction>
        {
            new HotkeyAction( "EnableEventResponses", Properties.Resources.hotkey_definition_enable_speech, () =>
            {
                EDDI.Instance.State[ "speechresponder_quiet" ] = false;
                Logging.Info("Hotkey action triggered: Enable Speech");
            } ),
            new HotkeyAction( "DisableEventResponses", Properties.Resources.hotkey_definition_disable_speech, () =>
            {
                EDDI.Instance.State[ "speechresponder_quiet" ] = true;
                SpeechService.Instance.ShutUp();
                Logging.Info("Hotkey action triggered: Disable Speech");
            } ),
            new HotkeyAction( "Shutup", Properties.Resources.hotkey_definition_stop_speech, () =>
            {
                SpeechService.Instance.ShutUp();
                Logging.Info("Hotkey action triggered: Stop the Current Speech");
            } )
        };

        public HotkeyRegistration Hotkeys;

        public void InitializeHotkeys ()
        {
            Hotkeys?.Dispose(); // unhook any previous instance
            Hotkeys = new HotkeyRegistration( new HotkeyActionCollection( HotkeyActions ) );
            Hotkeys?.RegisterAll();
        }

        public void RegisterHotkey ( string name, KeyGesture keyGesture ) => Hotkeys.RegisterHotkey( name, keyGesture );

        public void UnregisterHotkey ( string name ) => Hotkeys.UnregisterHotkey( name );

        public void UnregisterAllHotkeys () => Hotkeys.UnregisterAll();
    }
}

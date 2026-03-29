using System;
using System.Windows.Input;

namespace EddiCore.Hotkeys
{
    public class HotkeyAction ( string name, string displayName, Action action, KeyGesture keyGesture = null )
    {
        public string Name { get; set; } = name;
        public string DisplayName { get; set; } = displayName;
        public Action Action { get; set; } = action;
        public KeyGesture KeyGesture { get; set; } = keyGesture;
    }
}
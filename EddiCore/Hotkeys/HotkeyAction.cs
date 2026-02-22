using System;
using System.Windows.Input;

namespace EddiCore.Hotkeys
{
    public class HotkeyAction
    {
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
}
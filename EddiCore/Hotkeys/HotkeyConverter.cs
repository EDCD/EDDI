using System.Windows.Input;

namespace EddiCore.Hotkeys
{
    public static class HotkeyConverter
    {
        private static readonly KeyGestureConverter keyGestureConverter = new();

        public static KeyGesture FromString ( string keyGestureStr )
        {
            return keyGestureConverter.ConvertFromString( keyGestureStr ) as KeyGesture;
        }

        public static string ToString ( KeyGesture keyGesture )
        {
            return keyGestureConverter.ConvertToString( keyGesture );
        }
    }
}
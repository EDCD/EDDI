using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace EddiCore.Hotkeys
{
    public class HotkeyActionCollection
    {
        public HotkeyActionCollection ( List<HotkeyAction> hotkeyActions )
        {
            HotkeyActions = hotkeyActions;
        }

        public readonly List<HotkeyAction> HotkeyActions;

        public void AddGesture ( string name, KeyGesture gesture, int? id = null )
        {
            var action = HotkeyActions.FirstOrDefault( a => a.Name == name );
            if ( action != null )
            {
                action.KeyGesture = gesture;
                action.id = id;
                return;
            }

            throw new KeyNotFoundException( $"The key name '{name}' was not found in the action list." );
        }

        public void AddId ( string name, int id )
        {
            var action = HotkeyActions.FirstOrDefault( a => a.Name == name );
            if ( action != null )
            {
                action.id = id;
                return;
            }

            throw new KeyNotFoundException( $"The key name '{name}' was not found in the action list." );
        }

        public void ClearAllKeyGestures ()
        {
            foreach ( var action in HotkeyActions )
            {
                RemoveKeyGestures( action.Name );
            }
        }

        public bool IsKeyGestureAssigned ( string name, Key key, ModifierKeys modifiers )
        {
            var action = HotkeyActions.FirstOrDefault( a => a.Name == name );
            if ( action != null )
            {
                return HotkeyActions.Any( a =>
                    a.Name != name &&
                    a.KeyGesture != null &&
                    a.KeyGesture.Key == key &&
                    a.KeyGesture.Modifiers == modifiers );
            }

            throw new KeyNotFoundException( $"The key name '{name}' was not found in the action list." );
        }

        public void RemoveKeyGestures ( string name )
        {
            var action = HotkeyActions.FirstOrDefault( a => a.Name == name );
            if ( action != null )
            {
                action.id = null;
                action.KeyGesture = null;     
                return;
            }

            throw new KeyNotFoundException( $"The key name '{name}' was not found in the action list." );
        }

        public bool TryGetValue ( string name, out HotkeyAction action )
        {
            action = HotkeyActions.FirstOrDefault( a => a.Name == name );
            return action != null;
        }

        public bool TryGetValue ( int id, out HotkeyAction action )
        {
            action = HotkeyActions.FirstOrDefault( a => a.id != null && a.id == id );
            return action != null;
        }
    }
}
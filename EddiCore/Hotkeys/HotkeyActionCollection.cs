using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace EddiCore.Hotkeys
{
    public class HotkeyActionCollection
    {
        public HotkeyActionCollection ( List<HotkeyAction> hotkeyActions )
        {
            HotkeyActions = hotkeyActions ?? throw new ArgumentNullException( nameof( hotkeyActions ) );
            byName = HotkeyActions.ToDictionary( a => a.Name, a => a );
            RebuildGestureIndex();
        }

        public readonly List<HotkeyAction> HotkeyActions;

        private readonly Dictionary<string, HotkeyAction> byName;
        private readonly Dictionary<(Key key, ModifierKeys mods), HotkeyAction> byGesture = new();

        private void RebuildGestureIndex ()
        {
            byGesture.Clear();
            foreach ( var a in HotkeyActions )
            {
                if ( a.KeyGesture != null )
                {
                    byGesture[ (a.KeyGesture.Key, a.KeyGesture.Modifiers) ] = a;
                }
            }
        }

        public void AddGesture ( string name, KeyGesture gesture )
        {
            if ( !byName.TryGetValue( name, out var action ) )
            {
                throw new KeyNotFoundException( $"The key name '{name}' was not found in the action list." );
            }

            action.KeyGesture = gesture;
            RebuildGestureIndex();
        }

        public void ClearAllKeyGestures ()
        {
            foreach ( var action in HotkeyActions )
            {
                action.KeyGesture = null;
            }
            RebuildGestureIndex();
        }

        public bool IsKeyGestureAssigned ( string name, Key key, ModifierKeys modifiers )
        {
            if ( !byName.ContainsKey( name ) )
            {
                throw new KeyNotFoundException( $"The key name '{name}' was not found in the action list." );
            }

            return byGesture.TryGetValue( (key, modifiers), out var existing ) && existing.Name != name;
        }

        public void RemoveKeyGestures ( string name )
        {
            if ( !byName.TryGetValue( name, out var action ) )
            {
                throw new KeyNotFoundException( $"The key name '{name}' was not found in the action list." );
            }

            action.KeyGesture = null;
            RebuildGestureIndex();
        }

        public bool TryGetValue ( string name, out HotkeyAction action ) => byName.TryGetValue( name, out action );

        public bool TryGetValue ( Key key, ModifierKeys modifiers, out HotkeyAction action ) =>
            byGesture.TryGetValue( ( key, modifiers ), out action );
    }
}
using EddiDataDefinitions;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Utilities;

namespace EddiCore
{
    /// <summary>A subclass of ComboBox for selecting star systems</summary>
    public class StarSystemComboBox : FilterableComboBox
    {
        private readonly ConcurrentStack<NavWaypoint> systemList = new ConcurrentStack<NavWaypoint>();
        private readonly ConcurrentDictionary<string, List<NavWaypoint>> systemListCache = new ConcurrentDictionary<string, List<NavWaypoint>>();
        private readonly object systemListLock = new object();

        public StarSystemComboBox ()
        {
            DisplayMemberPath = nameof( NavWaypoint.systemName );
            ItemsSource = systemList;
            MaxDisplayListSize = 10;
            EnableFiltering = false;
            OnlyValuesInList = true;
            OnTextChangedAction = UpdateSystemList;
            LostFocus += OnLostFocus;
        }

        private void OnLostFocus ( object sender, RoutedEventArgs e )
        {
            Unloaded -= OnLostFocus;
            lock ( systemListLock )
            {
                systemList.Clear();
                systemListCache.Clear();
            }
        }

        private void UpdateSystemList ()
        {
            try
            {
                var partialSystemName = CurrentFilter;
                if ( partialSystemName.Length > 1 )
                {
                    Task.Run( () => {
                        try
                        {
                            var newSystems = GetTypeAheadSystemNames( partialSystemName )
                                .OrderBy( wp => wp.systemName.Contains( partialSystemName, StringComparison.InvariantCultureIgnoreCase ) )
                                .ThenByDescending( wp => wp.systemName.LevenshteinDistance( partialSystemName ) )
                                .ToHashSet();
                            if ( newSystems.Any() )
                            {
                                lock ( systemListLock )
                                {
                                    systemList.Clear();
                                    systemList.PushRange( newSystems.ToArray() );
                                }
                            }
                        }
                        catch ( Exception ex )
                        {
                            Logging.Warn( ex.Message, ex );
                        }
                    } );
                }
            }
            catch ( Exception ex )
            {
                Logging.Error( ex.Message, ex );
            }
        }

        private HashSet<NavWaypoint> GetTypeAheadSystemNames ( string partialSystemName )
        {
            // We'll need to request a new list if our cache does not already contain the key value
            if ( !systemListCache.ContainsKey( partialSystemName ) )
            {
                // Request a new list
                systemListCache[ partialSystemName ] = EDDI.Instance.DataProvider.GetTypeAheadSystems( partialSystemName );
            }

            return systemListCache
                .SelectMany( kv => kv.Value )
                .AsParallel()
                .GroupBy( wp => wp.systemAddress )
                .Select( g => g.First() )
                .ToHashSet();
        }
    }

    public class FilterableComboBox : ComboBox
    {
        /// <summary>
        /// If true, on lost focus or enter key pressed, checks the text in the combobox. If the text is not present
        /// in the list, it leaves it blank.
        /// </summary>
        public bool OnlyValuesInList
        {
            get => (bool)GetValue( OnlyValuesInListProperty );
            set => SetValue( OnlyValuesInListProperty, value );
        }

        public static readonly DependencyProperty OnlyValuesInListProperty =
            DependencyProperty.Register( nameof(OnlyValuesInList), typeof(bool), typeof(FilterableComboBox) );

        /// <summary>
        /// Selected item, changes only on lost focus or enter key pressed
        /// </summary>
        public object EffectivelySelectedItem
        {
            get => (bool)GetValue( EffectivelySelectedItemProperty );
            set => SetValue( EffectivelySelectedItemProperty, value );
        }

        public static readonly DependencyProperty EffectivelySelectedItemProperty =
            DependencyProperty.Register( nameof(EffectivelySelectedItem), typeof(object), typeof(FilterableComboBox) );

        /// <summary>
        /// If true, filters all items that do not contain the current filter text
        /// </summary>
        public bool EnableFiltering
        {
            get => (bool)GetValue( EnableFilteringProperty );
            set => SetValue( EnableFilteringProperty, value );
        }

        public static readonly DependencyProperty EnableFilteringProperty =
            DependencyProperty.Register( nameof(EnableFiltering), typeof(bool), typeof(FilterableComboBox) );

        protected int? MaxDisplayListSize = 10;
        protected Action OnTextChangedAction;

        internal string CurrentFilter = string.Empty;
        private bool IsTextBoxFrozen;

        private TextBox EditableTextBox
        {
            get => _editableTextBox ?? ( _editableTextBox = GetEditableTextBox() );
            set => _editableTextBox = value;
        }
        private TextBox _editableTextBox;

        private TextBoxBaseUserChangeTracker textBoxBaseUserChangeTracker;
        private readonly UserChange<bool> IsDropDownOpen_UserChange;
        private readonly DispatcherTimer debounceTimer;

        /// <summary>
        /// Triggers on lost focus or enter key pressed, if the selected item changed since the last time focus was lost or enter was pressed.
        /// </summary>
        public event Action<FilterableComboBox, object> SelectionEffectivelyChanged;

        internal FilterableComboBox ()
        {
            IsDropDownOpen_UserChange = new UserChange<bool>( v => IsDropDownOpen = v );
            DropDownOpened += FilteredComboBox_DropDownOpened;

            IsEditable = true;
            IsTextSearchEnabled = true;
            StaysOpenOnEdit = true;
            IsReadOnly = false;

            SelectionChanged += ( _, __ ) => shouldTriggerSelectedItemChanged = true;
            SelectionEffectivelyChanged += ( _, o ) => EffectivelySelectedItem = o;

            debounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds( 300 ) // Set debounce interval (e.g., 300ms)
            };
            debounceTimer.Tick += DebounceTimer_Tick;
        }

        private void DebounceTimer_Tick ( object sender, EventArgs e )
        {
            debounceTimer.Stop();
            OnTextChangedAction?.Invoke();
            RefreshFilter();
        }

        public override void OnApplyTemplate ()
        {
            base.OnApplyTemplate();
            EditableTextBox = GetEditableTextBox();
        }

        private TextBox GetEditableTextBox()
        {
            var textBox = GetTemplateChild( "PART_EditableTextBox" ) as TextBox;
            if ( textBox != null )
            {
                if ( !string.IsNullOrEmpty( textBox.Text ) )
                {
                    CurrentFilter = textBox.Text;
                    OnTextChangedAction?.Invoke();
                }

                textBoxBaseUserChangeTracker = new TextBoxBaseUserChangeTracker( textBox );
                textBoxBaseUserChangeTracker.UserTextChanged += FilteredComboBox_UserTextChange;
            }
            return textBox;
        }

        protected override void OnPreviewKeyDown ( KeyEventArgs e )
        {
            base.OnPreviewKeyDown( e );
            if ( e.Key == Key.Down && !IsDropDownOpen )
            {
                IsDropDownOpen = Items.Count > 0;
                e.Handled = true;
            }
            else if ( e.Key == Key.Escape )
            {
                ClearFilter();
                CheckSelectedItem();
                IsDropDownOpen = false;
            }
            else if ( e.Key == Key.Enter || e.Key == Key.Tab )
            {
                // Select any matching item in the current list
                foreach ( var item in Items )
                {
                    if ( item.ToString().Equals( CurrentFilter, StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        SelectedItem = item;
                        break;
                    }
                }

                // If there is no matching item, select the first item in the current list
                if ( SelectedItem is null && Items.Count > 0 )
                {
                    SelectedItem = Items.GetItemAt( 0 );
                }

                CheckSelectedItem();
                TriggerSelectedItemChanged();
            }
        }

        protected override void OnPreviewLostKeyboardFocus ( KeyboardFocusChangedEventArgs e )
        {
            base.OnPreviewLostKeyboardFocus( e );
            CheckSelectedItem();
            if ( ( ReferenceEquals( e.OldFocus, this ) || ReferenceEquals( e.OldFocus, EditableTextBox ) ) && !ReferenceEquals( e.NewFocus, this ) &&
                 !ReferenceEquals( e.NewFocus, EditableTextBox ) )
            {
                TriggerSelectedItemChanged();
            }
        }

        private void CheckSelectedItem ()
        {
            if ( OnlyValuesInList && IsDropDownOpen )
            {
                Text = SelectedItem?.ToString() ?? "";
            }
        }

        private bool shouldTriggerSelectedItemChanged;

        private void TriggerSelectedItemChanged ()
        {
            if ( shouldTriggerSelectedItemChanged )
            {
                SelectionEffectivelyChanged?.Invoke( this, SelectedItem );
                shouldTriggerSelectedItemChanged = false;
            }
        }

        public void ClearFilter ()
        {
            if ( string.IsNullOrEmpty( CurrentFilter ) )
            {
                return;
            }

            CurrentFilter = "";
            CollectionViewSource.GetDefaultView( ItemsSource ).Refresh();
        }

        private void FilteredComboBox_DropDownOpened ( object sender, EventArgs e )
        {
            if ( IsDropDownOpen_UserChange.IsUserChange )
            {
                ClearFilter();
            }
        }

        private void FilteredComboBox_UserTextChange ( object sender, EventArgs e )
        {
            if ( IsTextBoxFrozen )
            {
                return;
            }

            var tb = EditableTextBox;
            CurrentFilter = ( tb.SelectionStart + tb.SelectionLength ) == tb.Text.Length
                ? tb.Text.Substring( 0, tb.SelectionStart ).ToLower()
                : tb.Text.ToLower();

            debounceTimer.Stop();
            debounceTimer.Start();
        }

        protected override void OnItemsSourceChanged ( IEnumerable oldValue, IEnumerable newValue )
        {
            if ( newValue != null )
            {
                var view = CollectionViewSource.GetDefaultView( newValue );
                view.Filter += FilterItem;
            }

            if ( oldValue != null )
            {
                var view = CollectionViewSource.GetDefaultView( oldValue );
                if ( view != null )
                {
                    view.Filter -= FilterItem;
                }
            }

            base.OnItemsSourceChanged( oldValue, newValue );
        }

        private void RefreshFilter ()
        {
            if ( ItemsSource == null )
            {
                return;
            }

            var view = CollectionViewSource.GetDefaultView( ItemsSource );
            FreezeTextBoxState( () =>
            {
                var isDropDownOpen = IsDropDownOpen;
                //always hide because showing it enables the user to pick with up and down keys, otherwise it's not working because of the glitch in view.Refresh()
                IsDropDownOpen_UserChange.Set( false );
                view.Refresh();

                if ( ( !string.IsNullOrEmpty( CurrentFilter ) && CurrentFilter.Length > 1 && Items.Count > 0 ) || isDropDownOpen )
                {
                    IsDropDownOpen_UserChange.Set( true );
                }

                if ( SelectedItem == null )
                {
                    foreach ( var itm in ItemsSource )
                    {
                        if ( itm.ToString() == Text && IsDropDownOpen )
                        {
                            SelectedItem = itm;
                            break;
                        }
                    }
                }
            } );
        }

        private void FreezeTextBoxState ( Action action )
        {
            IsTextBoxFrozen = true;
            var tb = EditableTextBox;
            var text = Text;
            var selStart = tb.SelectionStart;
            var selLen = tb.SelectionLength;
            action();
            Text = text;
            tb.SelectionStart = selStart;
            tb.SelectionLength = selLen;
            IsTextBoxFrozen = false;
        }

        private bool FilterItem ( object value )
        {
            if ( value == null )
            {
                return false;
            }

            if ( MaxDisplayListSize != null && Items.Count >= MaxDisplayListSize )
            {
                return false;
            }

            if ( CurrentFilter.Length == 0 )
            {
                return true;
            }

            if ( EnableFiltering )
            {
                return value.ToString().ToLower().Contains( CurrentFilter );
            }

            return true;
        }

        private class TextBoxBaseUserChangeTracker
        {
            private bool IsTextInput { get; set; }

            private TextBoxBase TextBoxBase { get; }
            private readonly List<Key> PressedKeys = new List<Key>();
            public event EventHandler UserTextChanged;

            public TextBoxBaseUserChangeTracker ( TextBoxBase textBoxBase )
            {
                TextBoxBase = textBoxBase;
                var lastText = TextBoxBase.ToString();

                textBoxBase.PreviewTextInput += ( s, e ) =>
                {
                    IsTextInput = true;
                };

                textBoxBase.TextChanged += ( s, e ) =>
                {
                    var isUserChange = PressedKeys.Count > 0 || IsTextInput || lastText == TextBoxBase.ToString();
                    IsTextInput = false;
                    lastText = TextBoxBase.ToString();
                    if ( isUserChange )
                    {
                        UserTextChanged?.Invoke( this, e );
                    }
                };

                textBoxBase.PreviewKeyDown += ( s, e ) =>
                {
                    switch ( e.Key )
                    {
                        case Key.Back:
                        case Key.Space:
                            if ( !PressedKeys.Contains( e.Key ) )
                            {
                                PressedKeys.Add( e.Key );
                            }

                            break;
                    }

                    if ( e.Key == Key.Back )
                    {
                        if ( textBoxBase is TextBox textBox &&
                             textBox.SelectionStart > 0 &&
                             textBox.SelectionLength > 0 &&
                             ( textBox.SelectionStart + textBox.SelectionLength ) == textBox.Text.Length )
                        {
                            textBox.SelectionStart--;
                            textBox.SelectionLength++;
                            e.Handled = true;
                            UserTextChanged?.Invoke( this, e );
                        }
                    }
                };

                textBoxBase.PreviewKeyUp += ( s, e ) =>
                {
                    if ( PressedKeys.Contains( e.Key ) )
                    {
                        PressedKeys.Remove( e.Key );
                    }
                };

                textBoxBase.LostFocus += ( s, e ) =>
                {
                    PressedKeys.Clear();
                    IsTextInput = false;
                };
            }
        }

        private class UserChange<T>
        {
            private readonly Action<T> action;

            public bool IsUserChange { get; private set; } = true;

            public UserChange ( Action<T> action )
            {
                this.action = action;
            }

            public void Set ( T val )
            {
                try
                {
                    IsUserChange = false;
                    action( val );
                }
                finally
                {
                    IsUserChange = true;
                }
            }
        }
    }
}

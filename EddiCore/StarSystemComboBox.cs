using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Utilities;

namespace EddiCore
{
    /// <summary>A subclass of ComboBox for selecting star systems</summary>
    public class StarSystemComboBox : ComboBox
    {
        private ObservableCollection<string> SourceItems { get; } = new();
        private readonly ConcurrentDictionary<string, List<string>> ItemCache = new();
        private CancellationTokenSource cancellationTokenSource;
        private readonly object ItemLock = new();

        public StarSystemComboBox ()
        {
            ItemsSource = SourceItems;
            IsEditable = true; // Enable text input
            Loaded += OnLoaded; // Hook into the Loaded event to access the internal TextBox
            LostFocus += OnLostFocus;
        }

        private void OnLoaded ( object sender, RoutedEventArgs e )
        {
            // Access the internal TextBox in the ComboBox template
            if ( GetTemplateChild( "PART_EditableTextBox" ) is TextBox textBox )
            {
                textBox.TextChanged += OnTextChanged;
            }
        }

        private void OnLostFocus ( object sender, RoutedEventArgs e )
        {
            if ( SourceItems.Count > 0 )
            {
                SelectedItem = SourceItems.First();
                Text = SourceItems.First();
                IsDropDownOpen = false;
            }
        }

        private async void OnTextChanged ( object sender, TextChangedEventArgs e )
        {
            try
            {
                var input = Text;
                
                // Cancel any ongoing task
                cancellationTokenSource?.Cancel();
                cancellationTokenSource = new CancellationTokenSource();
                
                if ( string.IsNullOrWhiteSpace( input ) || input.Length <= 1 )
                {
                    lock ( ItemLock )
                    {
                        SourceItems.Clear();
                        IsDropDownOpen = false;
                    }
                    return;
                }

                try
                {
                    await Task.Delay( 200, cancellationTokenSource.Token ).ConfigureAwait( true ); // Debounce user input
                    await UpdateItemsSourceAsync( input, cancellationTokenSource.Token ).ConfigureAwait( true );
                }
                catch ( TaskCanceledException )
                {
                    // Ignore task cancellation
                }
            }
            catch ( Exception ex )
            {
                Logging.Error( ex.Message, ex );
            }
        }
        
        private async Task UpdateItemsSourceAsync ( string input, CancellationToken cancellationToken )
        {
            try
            {
                // We'll need to request a new list if our cache does not already contain the input value
                if ( !ItemCache.ContainsKey( input ) )
                {
                    // Request a new list
                    ItemCache[ input ] = await EDDI.Instance.DataProvider.GetTypeAheadSystemsAsync( input, cancellationToken ).ConfigureAwait( true );
                }
                
                var fetchedSystems = ItemCache[ input ].ToHashSet();

                if ( GetTemplateChild( "PART_EditableTextBox" ) is TextBox textBox )
                {
                    lock ( ItemLock )
                    {
                        // Preserve the cursor position and selection
                        var caretIndex = textBox.CaretIndex;
                        var selectionLength = textBox.SelectionLength;

                        var ItemsToRemove = SourceItems.Except(fetchedSystems).ToList();
                        foreach ( var removeItem in ItemsToRemove )
                        {
                            SourceItems.Remove( removeItem );
                        }

                        fetchedSystems = fetchedSystems.Except( SourceItems ).ToHashSet();
                        foreach ( var fetchedSystem in fetchedSystems )
                        {
                            SourceItems.Add( fetchedSystem );
                        }

                        // Update dropdown behavior
                        if ( SourceItems.Count > 0 )
                        {
                            IsDropDownOpen = true;
                        }

                        // Restore the cursor position and selection
                        textBox.CaretIndex = caretIndex;
                        textBox.SelectionLength = selectionLength;
                    }
                }
            }
            catch ( Exception ex ) when ( ex is not TaskCanceledException )
            {
                Logging.Warn( ex.Message, ex );
            }
        }
    }
}

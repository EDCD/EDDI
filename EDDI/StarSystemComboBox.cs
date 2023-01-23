using EddiSpanshService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Eddi
{
    /// <summary>A subclass of ComboBox for selecting star systems</summary>
    public class StarSystemComboBox : ComboBox
    {
        public TextBox TextBox { get; private set; }

        private List<string> systemList = new List<string>();
        private readonly Dictionary<string, List<string>> systemListCache = new Dictionary<string, List<string>>();
        private const int systemDisplayListSize = 10;
        private readonly ISpanshService spanshService = new SpanshService();

        public StarSystemComboBox()
        {
            this.GotFocus += OnGotFocus;
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            if (GetTemplateChild("PART_EditableTextBox") is TextBox textBox)
            {
                TextBox = textBox;
            }
        }

        private List<string> GetTypeAheadSystemNames(string partialSystemName)
        {
            // We'll need to request a new list if our cache does not already contain the key value
            if (!systemListCache.ContainsKey(partialSystemName))
            {
                // Request a new list
                systemListCache[partialSystemName] = spanshService.GetTypeAheadStarSystems(partialSystemName) ?? new List<string>();
            }

            return systemListCache[partialSystemName];
        }

        public void TextDidChange(object sender, TextChangedEventArgs e, string oldValue, Action changeHandler)
        {
            if (Text == oldValue) { return; }

            string systemName = Text.ToLowerInvariant();
            if (systemName.Length > 1)
            {
                // Obtain a new systemList when the string is being shortened or when the current systemList no longer contains a valid entry
                if (e.Changes.All(t => t.RemovedLength == 0) &&
                    e.Changes.Any(t => t.AddedLength > 0) &&
                    systemList.Any(s => s.Contains(systemName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    // Filter down from our prior list
                    systemList = systemList.Where(s => s.Contains(systemName, StringComparison.InvariantCultureIgnoreCase)).ToList();
                    // If the filtered list is less than our desired display list size, try to fetch more systems
                    if (systemList.Count < systemDisplayListSize)
                    {
                        systemList = GetTypeAheadSystemNames(systemName);
                    }
                }
                else
                {
                    systemList = GetTypeAheadSystemNames(systemName);
                }

                var caretIndex = TextBox.CaretIndex;
                if (systemList.Count == 1 && systemName.Equals(systemList[0], StringComparison.InvariantCultureIgnoreCase))
                {
                    ItemsSource = systemList.Take(1);
                    SelectedIndex = 0;
                    IsDropDownOpen = false;
                }
                else
                {
                    ItemsSource = systemList.Take(systemDisplayListSize);
                    Text = systemName;
                    IsDropDownOpen = true;
                }
                TextBox.CaretIndex = caretIndex;
            }
            else
            {
                IsDropDownOpen = false;
                // don't change the ItemSource or SelectedIndex
            }

            changeHandler?.Invoke();
        }

        public void SelectionDidChange(Action<string> changeHandler)
        {
            if (ItemsSource != null)
            {
                string newValue = SelectedItem?.ToString();
                changeHandler(newValue);
            }
        }

        public void DidLoseFocus(string oldValue)
        {
            if (Text != oldValue)
            {
                Text = oldValue;
                IsDropDownOpen = false;
                ItemsSource = null;
            }
            systemList.Clear();
            systemListCache.Clear();
        }
    }
}

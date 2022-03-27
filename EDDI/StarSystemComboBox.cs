using EddiStarMapService;
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
        private int systemListSize = 10;
        private readonly IEdsmService edsmService = new StarMapService();

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

        private List<string> SystemsBeginningWith(string partialSystemName)
        {
            return edsmService.GetTypeAheadStarSystems(partialSystemName) ?? new List<string>();
        }

        public void TextDidChange(object sender, TextChangedEventArgs e, string oldValue, Action changeHandler)
        {
            if (Text == oldValue) { return; }

            string systemName = Text;
            if (systemName.Length > 1)
            {
                // Obtain a new systemList when the string is being shortened or when the current systemList no longer contains a valid entry
                systemList = Text.Length > oldValue?.Length && systemList.Any(s => s.StartsWith(Text, StringComparison.InvariantCultureIgnoreCase)) 
                    ? systemList.Where(s => s.StartsWith(Text)).ToList() 
                    : SystemsBeginningWith(systemName);

                var caretIndex = TextBox.CaretIndex;
                if (systemList.Count == 1 && systemName.Equals(systemList[0], StringComparison.InvariantCultureIgnoreCase))
                {
                    ItemsSource = systemList.Take(1);
                    SelectedIndex = 0;
                    IsDropDownOpen = false;
                }
                else
                {
                    ItemsSource = systemList.Take(systemListSize);
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
        }
    }
}

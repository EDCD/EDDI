using EddiStarMapService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Eddi
{
    /// <summary>A subclass of ComboBox for selecting star systems</summary>
    public class StarSystemComboBox : ComboBox
    {
        private List<string> systemList = new List<string>();
        private int systemListSize = 10;

        private List<string> SystemsBeginningWith(string systemName)
        {
            return StarMapService.GetStarMapSystemsPartial(systemName, false, false).Select(s => s.name).ToList();
        }

        internal void TextDidChange(object sender, TextChangedEventArgs e, string oldValue, Action changeHandler)
        {
            if (Text == oldValue) { return; }

            string systemName = Text;
            if (systemName.Length > 1)
            {
                systemList = systemList.Where(s => s.StartsWith(systemName, StringComparison.InvariantCultureIgnoreCase)).ToList();
                if (systemList.Count < systemListSize)
                {
                    systemList = SystemsBeginningWith(systemName);

                    if (systemList.Count == 1 && systemName.Equals(systemList[0], StringComparison.InvariantCultureIgnoreCase))
                    {
                        ItemsSource = systemList.Take(1);
                        SelectedItem = systemList[0];
                        IsDropDownOpen = false;
                        return;
                    }
                }
                ItemsSource = systemList.Take(systemListSize);

                if (IsDropDownOpen == false)
                {
                    IsDropDownOpen = true;
                    var cmbTextBox = (TextBox)Template.FindName("PART_EditableTextBox", this);
                    cmbTextBox.CaretIndex = Text.Length;
                }
            }
            else if (systemName.Length == 1)
            {
                systemList = SystemsBeginningWith(systemName);
                return;
            }
            else
            {
                if (ItemsSource != null)
                {
                    IsDropDownOpen = false;
                    ItemsSource = null;
                }
            }

            changeHandler?.Invoke();
        }

        internal void SelectionDidChange(Action<string> changeHandler)
        {
            if (ItemsSource != null)
            {
                string newValue = SelectedItem?.ToString();
                changeHandler(newValue);
            }
        }

        internal void DidLoseFocus(string oldValue)
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

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

        private List<string> SystemsBeginningWith(string partialSystemName)
        {
            IEdsmService edsmService = new StarMapService();
            return edsmService.GetTypeAheadStarSystems(partialSystemName) ?? new List<string>();
        }

        public void TextDidChange(object sender, TextChangedEventArgs e, string oldValue, Action changeHandler)
        {
            if (Text == oldValue) { return; }

            string systemName = Text;
            if (systemName.Length > 1)
            {
                systemList = SystemsBeginningWith(systemName);
                if (systemList.Count == 1 && systemName.Equals(systemList[0], StringComparison.InvariantCultureIgnoreCase))
                {
                    ItemsSource = systemList.Take(1);
                    SelectedIndex = 0;
                    IsDropDownOpen = false;
                }
                else
                {
                    ItemsSource = systemList.Take(systemListSize);
                    IsDropDownOpen = true;
                    var cmbTextBox = (TextBox)Template.FindName("PART_EditableTextBox", this);
                    cmbTextBox.CaretIndex = systemName.Length;
                }
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

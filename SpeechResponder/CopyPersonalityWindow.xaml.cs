using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace EddiSpeechResponder
{
    using resx = Properties.SpeechResponder;

    /// <summary>
    /// Interaction logic for CopyPersonalityWindow.xaml
    /// </summary>
    public partial class CopyPersonalityWindow : Window, INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly HashSet<string> existingNames = new HashSet<string>();

        private string personalityName;
        public string PersonalityName
        {
            get => personalityName;
            set
            {
                personalityName = value;
                OnPropertyChanged(PersonalityName);
            }
        }
        private string personalityDescription;

        public string PersonalityDescription
        {
            get => personalityDescription;
            set
            {
                personalityDescription = value; 
                OnPropertyChanged(PersonalityDescription);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public CopyPersonalityWindow(IEnumerable<Personality> personalities)
        {
            InitializeComponent();
            DataContext = this;
            foreach (var personality in personalities)
            {
                existingNames.Add(personality.Name.ToLower());
            }
        }

        private void acceptButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = !Validation.GetHasError(this.PersonalityNameField);
            Close();
        }

        private void cancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

#region DataErrorInfo
        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case "PersonalityName":
                        return ValidatePersonalityName();
                    default:
                        return "";
                }
            }
        }

        string ValidatePersonalityName()
        {
            string trimmedName = personalityName?.Trim()?.ToLower();

            if (string.IsNullOrEmpty(trimmedName))
            {
                return resx.validation_tooltip_name_empty;
            }

            if (existingNames.Contains(trimmedName))
            {
                return resx.validation_tooltip_name_taken;
            }

            return string.Empty;
        }
#endregion

    }
}

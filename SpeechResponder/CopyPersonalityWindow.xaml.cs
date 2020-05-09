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

        bool CanAccept()
        {
            return string.IsNullOrWhiteSpace(Error);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            acceptButton.IsEnabled = CanAccept();
        }

        public CopyPersonalityWindow(IEnumerable<Personality> personalities)
        {
            InitializeComponent();
            DataContext = this;
            foreach (var personality in personalities)
            {
                existingNames.Add(personality.Name.ToLower());
            }

            acceptButton.IsEnabled = CanAccept();
        }

        private void acceptButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = !Validation.GetHasError(PersonalityNameField);
            Close();
        }

        private void cancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

#region DataErrorInfo
        // C#'s lack of string-based enums is annoying here. We have to resort to string literals, which are vulnerable to typos.
        public string Error
        {
            get
            {
                string result = this["PersonalityName"];
                if (!string.IsNullOrEmpty(result)) { return result; }

                return result;
            }
        }

        public string this[string fieldName]
        {
            get
            {
                switch (fieldName)
                {
                    case "PersonalityName":
                        return ValidatePersonalityName();

                    default:
                        return string.Empty;
                }
            }
        }

        string ValidatePersonalityName()
        {
            string trimmedName = personalityName?.Trim().ToLower();

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

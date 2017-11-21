using System.ComponentModel;
using System.Windows;
using Utilities;

namespace EddiSpeechResponder
{
    /// <summary>
    /// Interaction logic for CopyPersonalityWindow.xaml
    /// </summary>
    public partial class CopyPersonalityWindow : Window, INotifyPropertyChanged
    {
        private Personality personality;

        private string personalityName;
        public string PersonalityName
        {
            get { return personalityName; }
            set { personalityName = value;  OnPropertyChanged("PersonalityName");  }
        }
        private string personalityDescription;
        public string PersonalityDescription
        {
            get { return personalityDescription; }
            set { personalityDescription = value; OnPropertyChanged("PersonalityDescription"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public CopyPersonalityWindow(Personality personality)
        {
            InitializeComponent();
            DataContext = this;

            this.personality = personality;
        }

        private void acceptButtonClick(object sender, RoutedEventArgs e)
        {
            Logging.Info("Name is " + PersonalityName);
            DialogResult = (PersonalityName != null && PersonalityName.Trim() != "");
            Logging.Info("Dialog result is " + DialogResult);
            this.Close();
        }

        private void cancelButtonClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}

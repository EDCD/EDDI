using EDDI;
using EliteDangerousEvents;
using EliteDangerousSpeechResponder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Utilities;

namespace EliteDangerousSpeechResponder
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

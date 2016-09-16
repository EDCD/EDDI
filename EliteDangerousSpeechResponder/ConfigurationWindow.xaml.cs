using EliteDangerousEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Utilities;

namespace EliteDangerousSpeechResponder
{
    /// <summary>
    /// Interaction logic for ConfigurationWindow.xaml
    /// </summary>
    public partial class ConfigurationWindow : UserControl, INotifyPropertyChanged
    {
        private ObservableCollection<Personality> personalities;
        public ObservableCollection<Personality> Personalities
        {
            get { return personalities; }
            set { personalities = value; OnPropertyChanged("Personalities"); }
        }
        private Personality personality;
        public Personality Personality
        {
            get { return personality; }
            set { personality = value; OnPropertyChanged("Personality"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public ConfigurationWindow()
        {
            InitializeComponent();
            DataContext = this;

            ObservableCollection<Personality> personalities = new ObservableCollection<Personality>();
            // Add our default personality
            personalities.Add(Personality.Default());
            foreach (Personality personality in Personality.AllFromDirectory())
            {
                personalities.Add(personality);
            }
            // Add local personalities
            foreach (Personality personality in personalities)
            {
                Logging.Debug("Found personality " + personality.Name);
            }
            Personalities = personalities;

            SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();

            foreach (Personality personality in Personalities)
            {
                if (personality.Name == configuration.Personality)
                {
                    Personality = personality;
                    break;
                }
            }
        }

        private void eddiScriptsUpdated(object sender, RoutedEventArgs e)
        {
            updateScriptsConfiguration();
        }

        private void eddiScriptsUpdated(object sender, DataTransferEventArgs e)
        {
            updateScriptsConfiguration();
        }

        private void editScript(object sender, RoutedEventArgs e)
        {
            Script script = ((KeyValuePair<string, Script>)((Button)e.Source).DataContext).Value;
            EditScriptWindow editScriptWindow = new EditScriptWindow(Personality.Scripts, script.Name);
            editScriptWindow.ShowDialog();
            scriptsData.Items.Refresh();
        }

        private void testScript(object sender, RoutedEventArgs e)
        {
            Script script = ((KeyValuePair<string, Script>)((Button)e.Source).DataContext).Value;
            SpeechResponder responder = new SpeechResponder();
            responder.Start();
            Event sampleEvent = Events.SampleByName(script.Name);
            ScriptResolver scriptResolver = new ScriptResolver(Personality.Scripts);
            responder.Say(scriptResolver, script.Name, sampleEvent);
        }
        private void deleteScript(object sender, RoutedEventArgs e)
        {
            Script script = ((KeyValuePair<string, Script>)((Button)e.Source).DataContext).Value;
            string messageBoxText = "Are you sure you want to delete the \"" + script.Name + "\" script?";
            string caption = "Delete Script";
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    // Remove the script from the list
                    Personality.Scripts.Remove(script.Name);
                    Personality.ToFile();
                    // We updated a property of the personality but not the personality itself so need to manually update items
                    scriptsData.Items.Refresh();
                    break;
            }
        }

        private void resetScript(object sender, RoutedEventArgs e)
        {
            Script script = ((KeyValuePair<string, Script>)((Button)e.Source).DataContext).Value;
            script.Value = null;
            eddiScriptsUpdated(sender, e);
            scriptsData.Items.Refresh();
        }

        private void updateScriptsConfiguration()
        {
            if (Personality != null)
            {
                Personality.ToFile();
            }
        }

        private void personalityChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Personality != null)
            {
                SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
                configuration.Personality = Personality.Name;
                configuration.ToFile();
            }
        }

        private void newScriptClicked(object sender, RoutedEventArgs e)
        {
            Script script = new Script("foo", null, true, "Hello");
            Personality.Scripts.Add(script.Name, script);
            Personality.ToFile();
            // We updated a property of the personality but not the personality itself so need to manually update items
            scriptsData.Items.Refresh();

            //NewScriptWindow newScriptWindow = new NewScriptWindow(Personality.Scripts);
            //newScriptWindow.ShowDialog();
            //scriptsData.Items.Refresh();
        }

        private void copyPersonalityClicked(object sender, RoutedEventArgs e)
        {
            Personality newPersonality = Personality.Copy("Fred Bloggs");
            Personalities.Add(newPersonality);
            Personality = newPersonality;
            //string messageBoxText = "Are you sure you want to delete the \"" + Personality.Name + "\" personality?";
            //string caption = "Delete Personality";
            //MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            //CopyPersonalityWindow copyPersonalityWindow = new CopyPersonalityWindow(Personality.Name);
            //copyPersonalityWindow.ShowDialog();
        }

        private void deletePersonalityClicked(object sender, RoutedEventArgs e)
        {
            string messageBoxText = "Are you sure you want to delete the \"" + Personality.Name + "\" personality?";
            string caption = "Delete Personality";
            MessageBoxResult result = MessageBox.Show(messageBoxText, caption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    // Remove the personality from the list and the local filesystem
                    Personality oldPersonality = Personality;
                    Personalities.Remove(oldPersonality);
                    Personality = Personalities[0];
                    oldPersonality.RemoveFile();
                    break;
            }
        }
    }
}

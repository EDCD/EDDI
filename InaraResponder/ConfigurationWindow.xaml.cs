using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EddiInaraService;
using System.Timers;
using System.Windows.Controls;

namespace EddiInaraResponder
{
    /// <summary> Interaction logic for ConfigurationWindow.xaml </summary>
    public partial class ConfigurationWindow : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        // Set up a timer... wait 3 seconds before reconfiguring the InaraService for any change in the API key
        private const int delayMilliseconds = 3000;
        private readonly Timer inputTimer = new Timer(delayMilliseconds);

        public string apiKey
        {
            get => _apiKey;
            set
            {
                OnPropertyChanged();
                _apiKey = value;
            }
        }
        private string _apiKey;

        public ConfigurationWindow()
        {
            // Subscribe to events that require our attention
            InaraService.invalidAPIkey += (s, e) => { OnInvalidAPIkey((InaraConfiguration)s); };
            inputTimer.Elapsed += InputTimer_Elapsed;

            DataContext = this;
            InitializeComponent();

            InaraConfiguration inaraConfiguration = InaraConfiguration.FromFile();
            inaraApiKeyTextBox.Text = inaraConfiguration.apiKey;
        }

        private void InaraApiKeyChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox.Name == "inaraApiKeyTextBox")
                {
                    SetAPIKeyValidity(true);
                    inputTimer.Stop();
                    inputTimer.Start();
                }
            }
        }

        private void InputTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            inputTimer.Stop();
            UpdateConfiguration();
        }

        private void UpdateConfiguration()
        {
            InaraConfiguration inaraConfiguration = InaraConfiguration.FromFile();

            // Reset API key validity when it is edited.
            inaraConfiguration.isAPIkeyValid = true;

            // Update the changed API key in our configuration
            inaraConfiguration.apiKey = apiKey;

            // Save the updated configuration
            inaraConfiguration.ToFile();
        }

        private void OnInvalidAPIkey(InaraConfiguration inaraConfiguration)
        {
            Dispatcher.Invoke(() =>
            {
                SetAPIKeyValidity(inaraConfiguration.isAPIkeyValid);
            });

        }

        private void SetAPIKeyValidity(bool isAPIkeyValid)
        {
            if (isAPIkeyValid)
            {
                ClearErrors(nameof(apiKey));
            }
            else
            {
                ReportError(nameof(apiKey), Properties.InaraResources.invalidKeyErr);
            }
            inaraApiKeyTextBox.Text = apiKey; // Forces validation to update
        }

        // Implement INotifyDataErrorInfo for validation
        public void ReportError(string propertyName, string errorMessage)
        {
            if (string.IsNullOrEmpty(propertyName)) { return; }
            if (!Errors.ContainsKey(propertyName)) { Errors.Add(propertyName, new List<string>()); }
            Errors[propertyName].Add(errorMessage);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || (!HasErrors)) { return null; }
            if (!Errors.ContainsKey(propertyName)) { Errors.Add(propertyName, new List<string>()); }
            return Errors[propertyName];
        }
        public void ClearErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || (!HasErrors)) { return; }
            if (Errors.ContainsKey(propertyName)) { Errors.Remove(propertyName); }
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
        private readonly Dictionary<string, List<string>> Errors = new Dictionary<string, List<string>>();
        public bool HasErrors => Errors.Count > 0;
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        // Implement INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) 
        { 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
        }
    }
}

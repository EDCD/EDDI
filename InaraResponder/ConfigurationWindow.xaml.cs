using EddiConfigService;
using EddiConfigService.Configurations;
using EddiInaraService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace EddiInaraResponder
{
    /// <summary> Interaction logic for ConfigurationWindow.xaml </summary>
    public partial class ConfigurationWindow : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        // Set up a timer... wait 3 seconds before reconfiguring the InaraService for any change in the API key
        private const int delayMilliseconds = 3000;
        private readonly Timer inputTimer = new(delayMilliseconds);

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

            var inaraConfiguration = ConfigService.Instance.inaraConfiguration;
            inaraApiKeyTextBox.Text = inaraConfiguration.apiKey;
        }

        private void InaraApiKeyChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox box && box.Name == "inaraApiKeyTextBox" )
            {
                SetAPIKeyValidity(true);
                inputTimer.Stop();
                inputTimer.Start();
            }
        }

        private void InputTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            inputTimer.Stop();
            UpdateConfiguration();
        }

        private void UpdateConfiguration()
        {
            var inaraConfiguration = ConfigService.Instance.inaraConfiguration;

            // Reset API key validity when it is edited.
            inaraConfiguration.isAPIkeyValid = true;

            // Update the changed API key in our configuration
            inaraConfiguration.apiKey = apiKey;

            // Save the updated configuration
            ConfigService.Instance.inaraConfiguration = inaraConfiguration;
        }

        private void OnInvalidAPIkey(InaraConfiguration inaraConfiguration)
        {
            SetAPIKeyValidity( inaraConfiguration.isAPIkeyValid );
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
        }

        #region Implement INotifyDataErrorInfo for validation

        private readonly Dictionary<string, List<string>> Errors = new();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public bool HasErrors => Errors.Count > 0;
        
        private void ReportError(string propertyName, string errorMessage)
        {
            if (string.IsNullOrEmpty(propertyName)) { return; }
            if (!Errors.ContainsKey(propertyName)) { Errors.Add(propertyName, new List<string>()); }
            Errors[propertyName].Add(errorMessage);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
        
        public IEnumerable GetErrors ( string propertyName )
        {
            ArgumentNullException.ThrowIfNull(propertyName);
            
            return Errors.TryGetValue( propertyName, out var propertyErrors ) 
                ? propertyErrors 
                : [ ];
        }

        private void ClearErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || (!HasErrors)) { return; }
            Errors.Remove(propertyName);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        #endregion

        #region Implement INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void CmdrSettings_RequestNavigate ( object sender, RequestNavigateEventArgs e )
        {
            Process.Start( new ProcessStartInfo( e.Uri.ToString() ) { UseShellExecute = true } );
        }
        
        #endregion
    }
}

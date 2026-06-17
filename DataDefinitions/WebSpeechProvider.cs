using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EddiDataDefinitions
{
    public class WebSpeechProvider : INotifyPropertyChanged
    {
        private string _id;
        private string _providerType;
        private string _displayName;
        private bool _enabled = true;
        private List<string> _localeFilters = [];
        private Dictionary<string, string> _settings = [];

        [JsonProperty("id")]
        public string Id
        {
            get => _id;
            set
            {
                if ( value == _id ) { return; }
                _id = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("providerType")]
        public string ProviderType
        {
            get => _providerType;
            set
            {
                if ( value == _providerType ) { return; }
                _providerType = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("displayName")]
        public string DisplayName
        {
            get => _displayName;
            set
            {
                if ( value == _displayName ) { return; }
                _displayName = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("enabled")]
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if ( value == _enabled ) { return; }
                _enabled = value;
                OnPropertyChanged();
            }
        }

        [JsonProperty("localeFilters")]
        public List<string> LocaleFilters
        {
            get => _localeFilters ??= [];
            set
            {
                _localeFilters = value ?? [];
                OnPropertyChanged();
            }
        }

        [JsonProperty("settings")]
        public Dictionary<string, string> Settings
        {
            get => _settings ??= [];
            set
            {
                _settings = value ?? [];
                OnPropertyChanged();
            }
        }

        public string GetSetting ( string key )
        {
            return !string.IsNullOrWhiteSpace( key ) &&
                   Settings.TryGetValue( key, out var value )
                ? value
                : null;
        }

        public void SetSetting ( string key, string value )
        {
            if ( string.IsNullOrWhiteSpace( key ) )
            {
                return;
            }

            if ( string.IsNullOrWhiteSpace( value ) )
            {
                if ( Settings.Remove( key ) )
                {
                    OnPropertyChanged( nameof( Settings ) );
                }
                return;
            }

            if ( Settings.TryGetValue( key, out var existingValue ) && existingValue == value )
            {
                return;
            }

            Settings[ key ] = value;
            OnPropertyChanged( nameof( Settings ) );
        }

        public WebSpeechProvider Clone()
        {
            return new WebSpeechProvider
            {
                Id = Id,
                ProviderType = ProviderType,
                DisplayName = DisplayName,
                Enabled = Enabled,
                LocaleFilters = LocaleFilters is null ? [] : [.. LocaleFilters],
                Settings = Settings is null ? [] : new Dictionary<string, string>( Settings )
            };
        }

        public override string ToString()
        {
            var displayName = string.IsNullOrWhiteSpace( DisplayName )
                ? ProviderType
                : DisplayName;
            return Enabled ? displayName : $"{displayName} (disabled)";
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged ( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        #endregion
    }
}

using Newtonsoft.Json;
using System;

namespace EddiConfigService.Configurations
{
    /// <summary>Storage of credentials for a single Elite: Dangerous user to access EDSM</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\edsm.json")]
    public class StarMapConfiguration : Config
    {
        private string _apiKey;
        private DateTime _lastJournalSync = DateTime.MinValue;
        private DateTime _lastFlightLogSync = DateTime.MinValue;
        private string _commanderName;

        [ JsonProperty( "apiKey" ) ]
        public string apiKey
        {
            get => _apiKey;
            set
            {
                if ( value == _apiKey )
                {
                    return;
                }

                _apiKey = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "commanderName" ) ]
        public string commanderName
        {
            get => _commanderName;
            set
            {
                if ( value == _commanderName )
                {
                    return;
                }

                _commanderName = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "lastSync" ) ]
        public DateTime lastFlightLogSync
        {
            get => _lastFlightLogSync;
            set
            {
                if ( value.Equals( _lastFlightLogSync ) )
                {
                    return;
                }

                _lastFlightLogSync = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "lastJournalSync" ) ]
        public DateTime lastJournalSync
        {
            get => _lastJournalSync;
            set
            {
                if ( value.Equals( _lastJournalSync ) )
                {
                    return;
                }

                _lastJournalSync = value;
                OnPropertyChanged();
            }
        }
    }
}

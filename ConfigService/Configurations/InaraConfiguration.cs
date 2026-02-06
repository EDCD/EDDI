using Newtonsoft.Json;
using System;

namespace EddiConfigService.Configurations
{
    /// <summary>Storage of credentials for a single Elite: Dangerous user to access Inara</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\inara.json")]
    public class InaraConfiguration : Config
    {
        private string _apiKey;
        private string _commanderName;
        private string _commanderFrontierId;
        private int? _inaraId;
        private DateTime _lastSync;
        private bool _isApIkeyValid = true;

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

        [ JsonProperty( "commanderFrontierID" ) ]
        public string commanderFrontierID
        {
            get => _commanderFrontierId;
            set
            {
                if ( value == _commanderFrontierId )
                {
                    return;
                }

                _commanderFrontierId = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "inaraID" ) ]
        public int? inaraID
        {
            get => _inaraId;
            set
            {
                if ( value == _inaraId )
                {
                    return;
                }

                _inaraId = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "lastSync" ) ]
        public DateTime lastSync
        {
            get => _lastSync;
            set
            {
                if ( value.Equals( _lastSync ) )
                {
                    return;
                }

                _lastSync = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "isAPIkeyValid" ) ]
        public bool isAPIkeyValid
        {
            get => _isApIkeyValid;
            set
            {
                if ( value == _isApIkeyValid )
                {
                    return;
                }

                _isApIkeyValid = value;
                OnPropertyChanged();
            }
        }
    }
}

using Newtonsoft.Json;

namespace EddiConfigService.Configurations
{
    /// <summary>
    /// Storage for the Text-to-Speech Configs
    /// </summary>
    [JsonObject( MemberSerialization.OptOut ), RelativePath( @"\speech.json" )]
    public class SpeechServiceConfiguration : Config
    {
        private bool _enableIcao;
        private bool _disableIpa;
        private int _rate;
        private bool _distortOnDamage = true;
        private int _effectsLevel = 50;
        private int _volume = 80;
        private string _standardVoice;
        private string _audioDevice;

        [ JsonProperty( "standardVoice" ) ]
        public string StandardVoice
        {
            get => _standardVoice;
            set
            {
                if ( value == _standardVoice )
                {
                    return;
                }

                _standardVoice = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "audioDevice" ) ]
        public string AudioDevice
        {
            get => _audioDevice;
            set
            {
                if ( value == _audioDevice )
                {
                    return;
                }

                _audioDevice = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "volume" ) ]
        public int Volume
        {
            get => _volume;
            set
            {
                if ( value == _volume )
                {
                    return;
                }

                _volume = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "effectsLevel" ) ]
        public int EffectsLevel
        {
            get => _effectsLevel;
            set
            {
                if ( value == _effectsLevel )
                {
                    return;
                }

                _effectsLevel = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "distortOnDamage" ) ]
        public bool DistortOnDamage
        {
            get => _distortOnDamage;
            set
            {
                if ( value == _distortOnDamage )
                {
                    return;
                }

                _distortOnDamage = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "rate" ) ]
        public int Rate
        {
            get => _rate;
            set
            {
                if ( value == _rate )
                {
                    return;
                }

                _rate = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "disableipa" ) ]
        public bool DisableIpa
        {
            get => _disableIpa;
            set
            {
                if ( value == _disableIpa )
                {
                    return;
                }

                _disableIpa = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "enableicao" ) ]
        public bool EnableIcao
        {
            get => _enableIcao;
            set
            {
                if ( value == _enableIcao )
                {
                    return;
                }

                _enableIcao = value;
                OnPropertyChanged();
            }
        }

        private string _azureApiKey;
        private string _azureRegion;

        [ JsonProperty( "azureApiKey" ) ]
        public string AzureApiKey
        {
            get => _azureApiKey;
            set
            {
                if ( value == _azureApiKey )
                {
                    return;
                }

                _azureApiKey = value;
                OnPropertyChanged();
            }
        }

        [ JsonProperty( "azureRegion" ) ]
        public string AzureRegion
        {
            get => _azureRegion;
            set
            {
                if ( value == _azureRegion )
                {
                    return;
                }

                _azureRegion = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Clear the information held by speech
        /// </summary>
        public void Clear()
        {
            StandardVoice = null;
            AudioDevice = null;
            Volume = 100;
            EffectsLevel = 50;
            DistortOnDamage = true;
            DisableIpa = false;
            EnableIcao = false;
            AzureApiKey = null;
            AzureRegion = null;
        }
    }
}

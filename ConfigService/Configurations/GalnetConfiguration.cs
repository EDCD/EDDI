using Newtonsoft.Json;

namespace EddiConfigService.Configurations
{
    /// <summary>Configuration for the Galnet monitor</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\galnetmonitor.json")]
    public class GalnetConfiguration : Config
    {
        private string _lastuuid;
        private string _language = "English";
        private bool _galnetAlwaysOn = false;

        public string lastuuid
        {
            get => _lastuuid;
            set
            {
                if ( value == _lastuuid )
                {
                    return;
                }

                _lastuuid = value;
                OnPropertyChanged();
            }
        }

        public string language
        {
            get => _language;
            set
            {
                if ( value == _language )
                {
                    return;
                }

                _language = value;
                OnPropertyChanged();
            }
        }

        public bool galnetAlwaysOn
        {
            get => _galnetAlwaysOn;
            set
            {
                if ( value == _galnetAlwaysOn )
                {
                    return;
                }

                _galnetAlwaysOn = value;
                OnPropertyChanged();
            }
        }
    }
}

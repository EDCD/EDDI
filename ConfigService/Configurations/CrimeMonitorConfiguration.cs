using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EddiConfigService.Configurations
{
    /// <summary>Storage for configuration of criminal record details</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\crimemonitor.json")]
    public class CrimeMonitorConfiguration : Config
    {
        private ObservableCollection<FactionRecord> _criminalrecord = new();
        private Dictionary<string, string> _homeSystems = new();
        private DateTime _updatedat;

        public ObservableCollection<FactionRecord> criminalrecord
        {
            get => _criminalrecord;
            set
            {
                if ( Equals( value, _criminalrecord ) )
                {
                    return;
                }

                _criminalrecord = value;
                OnPropertyChanged();
            }
        }

        public Dictionary<string, string> homeSystems
        {
            get => _homeSystems;
            set
            {
                if ( Equals( value, _homeSystems ) )
                {
                    return;
                }

                _homeSystems = value;
                OnPropertyChanged();
            }
        }

        public DateTime updatedat
        {
            get => _updatedat;
            set
            {
                if ( value.Equals( _updatedat ) )
                {
                    return;
                }

                _updatedat = value;
                OnPropertyChanged();
            }
        }
    }
}
using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;

namespace EddiConfigService.Configurations
{
    /// <summary>Storage for configuration of cargo details</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\cargomonitor.json")]
    public class CargoMonitorConfiguration : Config
    {
        private ObservableCollection<Cargo> _cargo = [ ];
        private int _cargocarried;
        private DateTime _updatedat;

        public ObservableCollection<Cargo> cargo
        {
            get => _cargo;
            set
            {
                if ( Equals( value, _cargo ) )
                {
                    return;
                }

                _cargo = value;
                OnPropertyChanged();
            }
        }

        public int cargocarried
        {
            get => _cargocarried;
            set
            {
                if ( value == _cargocarried )
                {
                    return;
                }

                _cargocarried = value;
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
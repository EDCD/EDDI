using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiConfigService.Configurations
{
    /// <summary>Storage for configuration of mission details</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\missionmonitor.json")]
    public class MissionMonitorConfiguration : Config
    {
        private List<Mission> _missions = new();
        private DateTime _updatedat;
        private int _goalsCount;
        private int _missionsCount;
        private int? _missionWarning = 60;

        public List<Mission> missions
        {
            get => _missions;
            set
            {
                if ( Equals( value, _missions ) )
                {
                    return;
                }

                _missions = value;
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

        public int goalsCount
        {
            get => _goalsCount;
            set
            {
                if ( value == _goalsCount )
                {
                    return;
                }

                _goalsCount = value;
                OnPropertyChanged();
            }
        }

        public int missionsCount
        {
            get => _missionsCount;
            set
            {
                if ( value == _missionsCount )
                {
                    return;
                }

                _missionsCount = value;
                OnPropertyChanged();
            }
        }

        public int? missionWarning
        {
            get => _missionWarning;
            set
            {
                if ( value == _missionWarning )
                {
                    return;
                }

                _missionWarning = value;
                OnPropertyChanged();
            }
        }
    }
}

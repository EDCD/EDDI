using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;


namespace EddiDataDefinitions
{
    public class Mission : INotifyPropertyChanged
    {
        // The mission ID
        public long missionid { get; set; }

        // The name of the mission
        public string name;

        // The type of mission
        public string typeEDName
        {
            get => typeDef.edname;
            set
            {
                MissionType tDef = MissionType.FromEDName(value);
                this.typeDef = tDef;
            }
        }

        [JsonIgnore]
        private MissionType _typeDef;
        [JsonIgnore]
        public MissionType typeDef
        {
            get => _typeDef;
            set
            {
                _typeDef = value;
                NotifyPropertyChanged("localizedType");
            }
        }

        [JsonIgnore]
        public string localizedType => typeDef?.localizedName ?? "Unknown";
        [JsonIgnore, Obsolete("Please use localizedName or invariantName")]
        public string type => localizedType;

        // Status of the mission
        public string statusEDName
        {
            get => statusDef.edname;
            set
            {
                MissionStatus sDef = MissionStatus.FromEDName(value);
                this.statusDef = sDef;
            }
        }

        [JsonIgnore]
        private MissionStatus _statusDef;
        [JsonIgnore]
        public MissionStatus statusDef
        {
            get => _statusDef;
            set
            {
                _statusDef = value;
                NotifyPropertyChanged("localizedStatus");
            }
        }

        [JsonIgnore]
        public string localizedStatus => statusDef?.localizedName ?? "Unknown";
        [JsonIgnore, Obsolete("Please use localizedName or invariantName")]
        public string status => localizedStatus;

        // The system in which the mission was accepted
        public string originsystem { get; set; }

		// The station in which the mission was accepted
        public string originstation { get; set; }

        public string faction { get; set; }

        public string factionstate { get; set; }

        public string influence { get; set; }

        public string reputation { get; set; }

        public bool wing { get; set; }

        public long? reward { get; set; }

        public string commodity { get; set; }

        public int? amount { get; set; }

		// THe destination system of the mission
        private string _destinationsystem;
		public string destinationsystem
        {
            get
            {
                return _destinationsystem;
            }
            set
            {
                if (_destinationsystem != value)
                {
                    _destinationsystem = value;
                    NotifyPropertyChanged("destinationsystem");
                }
            }
        }

		// THe destination station of the mission
        private string _destinationstation;
		public string destinationstation
        {
            get
            {
                return _destinationstation;
            }
            set
            {
                if (_destinationstation != value)
                {
                    _destinationstation = value;
                    NotifyPropertyChanged("destinationstation");
                }
            }
        }

        // THe destination station of the mission
        [JsonIgnore]
        private string _timeremaining;
        [JsonIgnore]
        public string timeremaining
        {
            get
            {
                return _timeremaining;
            }
            set
            {
                if (_timeremaining != value)
                {
                    _timeremaining = value;
                    NotifyPropertyChanged("timeremaining");
                }
            }
        }

        public string passengertype { get; set; }
        public bool? passengerwanted { get; set; }
        public bool? passengervips { get; set; }

        public string target { get; set; }
        public string targettype { get; set; }
        public string targetfaction { get; set; }

        public DateTime expiry { get; set; }

        public Mission() { }

        [JsonConstructor]
        public Mission(long MissionId, string Name, DateTime expiry, MissionStatus Status)
		{
			this.missionid = MissionId;
			this. name = Name;
            this.typeDef = MissionType.FromEDName(Name.Split('_').ElementAt(1));
			this.expiry = expiry;
			this.statusDef = Status;
		}

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

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
        public MissionType type;
        public string localizedType => type?.localizedName ?? "Unknown";

        // Status of the mission - active, failed, complete
        private MissionStatus _status;
		public MissionStatus status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    NotifyPropertyChanged("localizedStatus");
                }
            }
        }
        public string localizedStatus => status?.localizedName ?? "Unknown";

        // The system in which the mission was accepted
        public string originsystem { get; set; }

		// The station in which the mission was accepted
        public string originstation { get; set; }

        public string faction { get; set; }

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
            this.type = MissionType.FromEDName(Name.Split('_').ElementAt(1));
			this.expiry = expiry;
			this.status = Status;
		}

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

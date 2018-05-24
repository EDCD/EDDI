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
        public string type;

        // Status of the mission - active, failed, complete
        private string _status;
		public string status
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
                    NotifyPropertyChanged("status");
                }
            }
        }		

		// The system in which the mission was accepted
        public string originsystem { get; set; }

		// The station in which the mission was accepted
        public string originstation { get; set; }

        public string faction { get; set; }

        public string influence { get; set; }

        public string reputation { get; set; }

        public bool wing { get; set; }

        public int reward { get; set; }


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

        public int? passengercount { get; set; }

        public string target { get; set; }

        public string targettype { get; set; }

        public string targetfaction { get; set; }

        public int? killcount { get; set; }

        public DateTime expiry { get; set; }

        public Mission() { }

        // Constructor for 'Missions' event
        [JsonConstructor]
        public Mission(long MissionId, string Name, double Expires, string Status)
		{
			this.missionid = MissionId;
			this. name = Name;
			this.type = Name.Split('_').ElementAt(1).ToLowerInvariant();
			this.expiry = DateTime.Now.AddSeconds(Expires);
			this.status = Status;
		}

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

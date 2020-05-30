using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EddiDataDefinitions
{
    public class Bookmark : INotifyPropertyChanged
    {
        // The bookmark system
        [JsonIgnore]
        private string _system;
        public string system
        {
            get => _system;
            set
            {
                if (_system != value)
                {
                    _system = value;
                    NotifyPropertyChanged("system");
                }
            }
        }

        // The bookmark body
        [JsonIgnore]
        private string _body;
        public string body
        {
            get => _body;
            set
            {
                if (_body != value)
                {
                    _body = value;
                    NotifyPropertyChanged("body");
                }
            }
        }

        public string poi { get; set; }

        public decimal? latitude { get; set; }

        public decimal? longitude { get; set; }

        public bool landable { get; set; }

        // Default Constructor
        public Bookmark() { }

        [JsonConstructor]
        public Bookmark(string System, string Body, string POI, decimal? Latitude, decimal? Longitude, bool Landable)
        {
            this.system = System;
            this.body = Body;
            this.poi = POI;
            this.latitude = Latitude;
            this.longitude = Longitude;
            this.landable = Landable;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

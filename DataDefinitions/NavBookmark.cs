using Newtonsoft.Json;
using System.ComponentModel;

namespace EddiDataDefinitions
{
    public class NavBookmark : INotifyPropertyChanged
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

        public decimal? x { get; set; }
        public decimal? y { get; set; }
        public decimal? z { get; set; }

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

        public decimal? radius { get; set; }

        public string poi { get; set; }

        public bool isstation { get; set; }

        public string category { get; set; }

        public string comment { get; set; }

        public decimal? latitude { get; set; }

        public decimal? longitude { get; set; }

        public bool landable { get; set; }

        public bool isset { get; set; }

        // Default Constructor
        public NavBookmark() { }

        [JsonConstructor]
        public NavBookmark(string system, decimal? x, decimal? y, decimal? z, string body, decimal? radius, string poi, bool isstation, decimal? latitude, decimal? longitude, bool landable)
        {
            this.system = system;
            this.x = x;
            this.y = y;
            this.z = z;
            this.body = body;
            this.radius = radius;
            this.poi = poi;
            this.isstation = isstation;
            this.latitude = latitude;
            this.longitude = longitude;
            this.landable = landable;
            this.isset = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

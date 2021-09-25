using Newtonsoft.Json;
using System.ComponentModel;

namespace EddiDataDefinitions
{
    public class NavBookmark : INotifyPropertyChanged
    {
        // The bookmark system
        [JsonProperty("system")]
        public string systemname
        {
            get => _systemname;
            set
            {
                if (_systemname != value)
                {
                    _systemname = value;
                    NotifyPropertyChanged(nameof(systemname));
                }
            }
        }
        [JsonIgnore] private string _systemname;

        public decimal? x { get; set; }
        public decimal? y { get; set; }
        public decimal? z { get; set; }

        // The bookmark body
        [JsonProperty("body")]
        public string bodyname
        {
            get => _bodyname;
            set
            {
                if (_bodyname != value)
                {
                    _bodyname = value;
                    NotifyPropertyChanged(nameof(bodyname));
                }
            }
        }
        [JsonIgnore] private string _bodyname;

        [JsonIgnore]
        public string bodyshortname => Body.GetShortName(bodyname, systemname);

        public decimal? radius { get; set; }

        public string poi { get; set; }

        public bool isstation { get; set; }

        public string category { get; set; }

        public string comment { get; set; }

        public decimal? latitude { get; set; }

        public decimal? longitude { get; set; }

        public bool landable { get; set; }

        [JsonIgnore]
        public decimal? heading
        {
            get => _heading;
            set
            {
                if (_heading != value)
                {
                    _heading = value;
                    NotifyPropertyChanged(nameof(heading));
                }
            }
        }
        [JsonIgnore] private decimal? _heading;

        [JsonIgnore]
        public decimal? distanceKm
        {
            get => _distanceKm;
            set
            {
                if (_distanceKm != value)
                {
                    _distanceKm = value;
                    NotifyPropertyChanged(nameof(distanceKm));
                }
            }
        }
        [JsonIgnore] private decimal? _distanceKm;

        // Default Constructor
        public NavBookmark() { }

        [JsonConstructor]
        public NavBookmark(string systemname, decimal? x, decimal? y, decimal? z, string bodyname, decimal? radius, string poi, bool isstation, decimal? latitude, decimal? longitude, bool landable)
        {
            this.systemname = systemname;
            this.x = x;
            this.y = y;
            this.z = z;
            this.bodyname = bodyname;
            this.radius = radius;
            this.poi = poi;
            this.isstation = isstation;
            this.latitude = latitude;
            this.longitude = longitude;
            this.landable = landable;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

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

        public ulong? systemAddress { get; set; }

        public decimal? x { get; set; }
        public decimal? y { get; set; }
        public decimal? z { get; set; }

        [JsonIgnore]
        public decimal? distanceLy
        {
            get => _distanceLy;
            set
            {
                if (_distanceLy != value)
                {
                    _distanceLy = value;
                    NotifyPropertyChanged(nameof(distanceLy));
                }
            }
        }
        [JsonIgnore] private decimal? _distanceLy;

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

        public string poi { get; set; }

        public bool isstation { get; set; }

        public string comment { get; set; }

        public decimal? latitude
        {
            get => _latitude;
            set
            {
                if (_latitude != value)
                {
                    _latitude = value;
                    NotifyPropertyChanged(nameof(latitude));
                }
            }
        }
        [JsonIgnore] private decimal? _latitude;

        public decimal? longitude
        {
            get => _longitude;
            set
            {
                if (_longitude != value)
                {
                    _longitude = value;
                    NotifyPropertyChanged(nameof(longitude));
                }
            }
        }
        [JsonIgnore] private decimal? _longitude;

        [JsonIgnore] 
        public bool landable => latitude != null && longitude != null;

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

        public long? arrivalRadiusMeters
        {
            get => _arrivalRadiusMeters;
            set
            {
                if (_arrivalRadiusMeters != value)
                {
                    _arrivalRadiusMeters = value;
                    NotifyPropertyChanged(nameof(arrivalRadiusMeters));
                }
            }
        }
        [JsonIgnore] private long? _arrivalRadiusMeters = 1000;

        public bool nearby = false;

        public bool useStraightPath
        {
            get => _useStraightPath;
            set => _useStraightPath = value;
        }
        [JsonIgnore] private bool _useStraightPath;

        [JsonIgnore]
        public SortedSet<DateTime> visitLog
        {
            get => _visitLog;
            set
            {
                if (_visitLog != value)
                {
                    _visitLog = value;
                    NotifyPropertyChanged(nameof(visitLog));
                }
            }
        }
        [JsonIgnore] private SortedSet<DateTime> _visitLog = new SortedSet<DateTime>();

        [JsonIgnore] public bool visited => visitLog.Any();

        // Drop down visibility
        [JsonIgnore, UsedImplicitly] public bool hasRowDetails => landable;

        // Default Constructor
        public NavBookmark() { }

        [JsonConstructor]
        public NavBookmark(string systemname, ulong? systemAddress, decimal? x, decimal? y, decimal? z, string bodyname, string poi, bool isstation, decimal? latitude, decimal? longitude, bool nearby)
        {
            this.systemname = systemname;
            this.systemAddress = systemAddress;
            this.x = x;
            this.y = y;
            this.z = z;
            this.bodyname = bodyname;
            this.poi = poi;
            this.isstation = isstation;
            this.latitude = latitude;
            this.longitude = longitude;
            this.nearby = nearby;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

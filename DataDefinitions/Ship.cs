﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>A ship</summary>
    public class Ship : INotifyPropertyChanged
    {
        /// <summary>the ID of this ship for this commander</summary>
        public int LocalId { get; set; }

        /// <summary>the manufacturer of the ship (Lakon, CoreDynamics etc.)</summary>
        [JsonIgnore]
        public string manufacturer { get; set; }

        /// <summary>the spoken manufacturer of the ship (Lakon, CoreDynamics etc.)</summary>
        [JsonIgnore]
        public List<Translation> phoneticmanufacturer { get; set; }

        /// <summary>the spoken model of the ship (Python, Anaconda, etc.)</summary>
        [JsonIgnore]
        public List<Translation> phoneticmodel { get; set; }

        /// <summary>the size of this ship</summary>
        [JsonIgnore]
        public LandingPadSize size { get; set; }

        /// <summary>the size of the military compartment slots</summary>
        [JsonIgnore]
        public int? militarysize { get; set; }

        /// <summary>the total tonnage cargo capacity</summary>
        public int cargocapacity { get; set; }

        private long _value;
        /// <summary>the value of the ship without cargo, in credits</summary>
        public long value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    NotifyPropertyChanged("value");
                }
            }
        }

        /// <summary>the value of the ship's rebuy, in credits</summary>
        public long rebuy { get; set; }

        private string _name;
        /// <summary>the name of this ship</summary>
        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged("name");
                }
            }
        }

        private string _model;
        /// <summary>the model of the ship (Python, Anaconda, etc.)</summary>
        public string model
        {
            get
            {
                return _model;
            }
            set
            {
                if (_model != value)
                {
                    _model = value;
                    NotifyPropertyChanged("model");
                }
            }
        }

        private string _ident;
        /// <summary>the identifier of this ship</summary>
        public string ident
        {
            get
            {
                return _ident;
            }
            set
            {
                if (_ident != value)
                {
                    _ident = value;
                    NotifyPropertyChanged("ident");
                }
            }
        }

        [JsonIgnore]
        private string PhoneticName;
        /// <summary>the phonetic name of this ship</summary>
        public string phoneticname
        {
            get { return PhoneticName; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    PhoneticName = null;
                }
                else
                {
                    NotifyPropertyChanged("phoneticname");
                    PhoneticName = value;
                }
            }
        }

        // The type of mission
        public string roleEDName
        {
            get => Role.edname;
            set
            {
                Role rDef = Role.FromEDName(value);
                this.Role = rDef;
            }
        }

        /// <summary>the role of this ship</summary>
        private Role _Role = Role.MultiPurpose;
        [JsonIgnore]
        public Role Role
        {
            get
            {
                return _Role;
            }
            set
            {
                if (_Role != value)
                {
                    _Role = value;
                    NotifyPropertyChanged("Role");
                }
            }
        }

        [JsonIgnore, Obsolete("Please use localizedName or invariantName")]
        public string role => Role?.localizedName; // This string is made available for Cottle scripts that vary depending on the ship's role. 

        [JsonExtensionData]
        private IDictionary<string, JToken> additionalJsonData;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (Role == null) // legacy shipmonitor JSON
            {
                string roleName = (string)additionalJsonData["role"];
                Role = Role.FromEDName(roleName) ?? Role.FromName(roleName);
            }
            else
            {
                // get the canonical role object for the given EDName
                Role = Role.FromEDName(Role.edname);
            }
            if (EDName is null && !(model is null)) // legacy shipmonitor JSON may not include EDName or EDID
            {
                Ship template = ShipDefinitions.FromModel(model);
                EDName = EDName ?? template?.EDName;
            }
            additionalJsonData = null;
        }

        /// <summary>
        /// The raw JSON from the companion API for this ship
        /// </summary>
        private string _raw;
        public string raw
        {
            get
            {
                return _raw;
            }
            set
            {
                if (_raw != value)
                {
                    _raw = value;
                    NotifyPropertyChanged("RawIsNotNull");
                }
            }
        }

        public bool RawIsNotNull
        {
            get { return !string.IsNullOrEmpty(_raw); }
        }

        /// <summary>
        /// The wanted/hot status of this ship
        /// </summary>
        private bool _hot = false;
        [JsonIgnore]
        public bool hot
        {
            get
            {
                return _hot;
            }
            set
            {
                if (_hot != value)
                {
                    _hot = value;
                    NotifyPropertyChanged("hot");
                }
            }
        }

        /// <summary>the name of the system in which this ship is stored; null if the commander is in this ship</summary>
        private string _starsystem;
        public string starsystem
        {
            get
            {
                return _starsystem;
            }
            set
            {
                if (_starsystem != value)
                {
                    _starsystem = value;
                    NotifyPropertyChanged("starsystem");
                }
            }
        }

        /// <summary>the name of the station in which this ship is stored; null if the commander is in this ship</summary>
        public string station { get; set; }
        public long? marketid { get; set; }

        // Other properties for when this ship is stored
        public bool intransit { get; set; }
        public long? transferprice { get; set; }
        public long? transfertime { get; set; }

        public decimal health { get; set; }
        public Module cargohatch { get; set; }
        public Module bulkheads { get; set; }
        public Module canopy { get; set; }
        public Module powerplant { get; set; }
        public Module thrusters { get; set; }
        public Module frameshiftdrive { get; set; }
        public Module lifesupport { get; set; }
        public Module powerdistributor { get; set; }
        public Module sensors { get; set; }
        public Module fueltank { get; set; }
        public Module datalinkscanner { get; set; }
        public List<Hardpoint> hardpoints { get; set; }
        public List<Compartment> compartments { get; set; }
        public List<LaunchBay> launchbays { get; set; }
        public string paintjob { get; set; }

        public decimal? fueltankcapacity { get; set; } // Core capacity
        public decimal? fueltanktotalcapacity { get; set; } // Capacity including additional tanks
        public decimal activeFuelReservoirCapacity { get; set; }

        // Ship jump properties
        public decimal maxjumprange { get; set; }

        [JsonIgnore, Obsolete("Please use maxjumprange instead")]
        public decimal maxjump => maxjumprange;

        public decimal maxfuelperjump { get; set; }

        [JsonIgnore, Obsolete("Please use maxfuelperjump instead")]
        public decimal maxfuel => maxfuelperjump;

        public decimal optimalmass { get; set; }
        public decimal unladenmass { get; set; }

        // Admin
        // The ID in Elite: Dangerous' database
        [JsonIgnore]
        public long EDID { get; set; }
        // The name in Elite: Dangerous' database
        public string EDName { get; set; }

        public Ship()
        {
            health = 100M;
            hardpoints = new List<Hardpoint>();
            compartments = new List<Compartment>();
            launchbays = new List<LaunchBay>();
        }

        public Ship(long EDID, string EDName, string Manufacturer, List<Translation> PhoneticManufacturer, string Model, List<Translation> PhoneticModel, string Size, int? MilitarySize, decimal reservoirFuelTankSize)
        {
            this.EDID = EDID;
            this.EDName = EDName;
            manufacturer = Manufacturer;
            phoneticmanufacturer = PhoneticManufacturer;
            model = Model;
            phoneticmodel = PhoneticModel;
            size = LandingPadSize.FromEDName(Size);
            militarysize = MilitarySize;
            health = 100M;
            hardpoints = new List<Hardpoint>();
            compartments = new List<Compartment>();
            launchbays = new List<LaunchBay>();
            this.activeFuelReservoirCapacity = reservoirFuelTankSize;
        }

        public override string ToString()
        {
            // This is mostly to help with debugging
            return name ?? $"{Role.localizedName} {model}";
        }

        public string SpokenName(string defaultname = null)
        {
            string model = (defaultname ?? SpokenModel()) ?? "ship";
            string result = ("your " + model);
            if (!string.IsNullOrWhiteSpace(phoneticname))
            {
                result = "<phoneme alphabet=\"ipa\" ph=\"" + phoneticname + "\">" + name + "</phoneme>";
            }
            else if (!string.IsNullOrWhiteSpace(name))
            {
                result = name;
            }
            return result;
        }

        public string SpokenModel()
        {
            string result;
            if (phoneticmodel == null)
            {
                result = model;
            }
            else
            {
                result = "";
                foreach (Translation item in phoneticmodel)
                {
                    result += "<phoneme alphabet=\"ipa\" ph=\"" + item.to + "\">" + item.from + "</phoneme> ";
                }
            }
            return result;
        }

        public string SpokenManufacturer()
        {
            string result;
            if (phoneticmanufacturer == null)
            {
                result = manufacturer;
            }
            else
            {
                result = "";
                foreach (Translation item in phoneticmanufacturer)
                {
                    result += "<phoneme alphabet=\"ipa\" ph=\"" + item.to + "\">" + item.from + "</phoneme> ";
                }
            }
            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct    
        public string CoriolisUri()
        {
            if (raw != null)
            {
                // Generate a Coriolis import URI to retain as much information as possible
                string uri = "https://coriolis.io/import?";

                // Take the ship's JSON, gzip it, then turn it in to base64 and attach it to the base uri
                var bytes = Encoding.UTF8.GetBytes(raw);
                using (var streamIn = new MemoryStream(bytes))
                using (var streamOut = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(streamOut, CompressionLevel.Optimal, true))
                    {
                        streamIn.CopyTo(gzipStream);
                    }
                    uri += "data=" + Uri.EscapeDataString(Convert.ToBase64String(streamOut.ToArray()));
                }

                // Add the ship's name
                string bn = name ?? $"{Role.localizedName} {model}";
                uri += "&bn=" + Uri.EscapeDataString(bn);

                return uri;
            }
            return null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct
        public string EDShipyardUri()
        {
            // Once Coriolis supports POSTing, we can switch to POSTing to https://edsy.org/import

            if (raw != null)
            {
                // Generate an EDShipyard import URI to retain as much information as possible
                string uri = "https://edsy.org/";

                // Take the ship's JSON, gzip it, then turn it in to base64 and attach it to the base uri
                string unescapedraw = raw.Replace(@"\""", @"""");
                var bytes = Encoding.UTF8.GetBytes(unescapedraw);
                using (var streamIn = new MemoryStream(bytes))
                using (var streamOut = new MemoryStream())
                {
                    using (var gzipStream = new GZipStream(streamOut, CompressionLevel.Optimal, true))
                    {
                        streamIn.CopyTo(gzipStream);
                    }
                    uri += "#/I=" + Uri.EscapeDataString(Convert.ToBase64String(streamOut.ToArray()));
                }

                return uri;
            }
            return null;
        }

        /// <summary>
        /// Augment the ship's information from the model
        /// </summary>
        public void Augment()
        {
            Ship template = ShipDefinitions.FromModel(model);
            if (template != null)
            {
                EDID = template.EDID;
                EDName = template.EDName;
                manufacturer = template.manufacturer;
                phoneticmanufacturer = template.phoneticmanufacturer;
                phoneticmodel = template.phoneticmodel;
                size = template.size;
                militarysize = template.militarysize;
                activeFuelReservoirCapacity = template.activeFuelReservoirCapacity;
                if (Role == null)
                {
                    Role = EddiDataDefinitions.Role.MultiPurpose;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public static Ship FromShipyardInfo(ShipyardInfo item)
        {
            Ship ship = ShipDefinitions.FromEliteID(item.id) ?? ShipDefinitions.FromEDModel(item.shiptype);
            if (ship == null)
            {
                // Unknown ship; report the full object so that we can update the definitions 
                Logging.Info("Ship definition error: " + item.shiptype);

                // Create a basic ship definition & supplement from the info available 
                ship = new Ship
                {
                    EDName = item.shiptype
                };
            }
            ship.value = item.shipprice;

            return ship;
        }
    }
}

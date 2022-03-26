using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
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
        [PublicAPI, JsonIgnore]
        public string manufacturer { get; set; }

        /// <summary>the spoken manufacturer of the ship (Lakon, CoreDynamics etc.) (rendered using ssml and IPA)</summary>
        [PublicAPI, JsonIgnore]
        public string phoneticmanufacturer => SpokenManufacturer();

        /// <summary>the spoken model of the ship (Python, Anaconda, etc.) (rendered using ssml and IPA)</summary>
        [PublicAPI, JsonIgnore]
        public string phoneticmodel => SpokenModel();

        [JsonIgnore]
        public List<Translation> phoneticModel { get; set; }

        /// <summary>the size of this ship</summary>
        [JsonIgnore]
        public LandingPadSize Size { get; set; } = LandingPadSize.Small;

        /// <summary>the spoken size of this ship</summary>
        [PublicAPI, JsonIgnore]
        public string size => (Size ?? LandingPadSize.Small).localizedName;

        /// <summary>the size of the military compartment slots</summary>
        [JsonIgnore]
        public int? militarysize { get; set; }

        /// <summary>the total tonnage cargo capacity</summary>
        [PublicAPI]
        public int cargocapacity { get; set; }

        /// <summary>the value of the ship without cargo, in credits</summary>

        [PublicAPI]
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
                    OnPropertyChanged();
                }
            }
        }

        private long _value;

        /// <summary>the value of the ship's hull, in credits</summary>
        
        [PublicAPI]
        public long? hullvalue
        {
            get
            {
                return _hullvalue;
            }
            set
            {
                if (_hullvalue != value)
                {
                    _hullvalue = value;
                    OnPropertyChanged();
                }
            }
        }

        private long? _hullvalue;

        /// <summary>the value of the ship's hull, in credits</summary>
        
        [PublicAPI]
        public long? modulesvalue
        {
            get
            {
                return _modulesvalue;
            }
            set
            {
                if (_modulesvalue != value)
                {
                    _modulesvalue = value;
                    OnPropertyChanged();
                }
            }
        }

        private long? _modulesvalue;

        /// <summary>the value of the ship's rebuy, in credits</summary>
        [PublicAPI]
        public long rebuy
        {
            get => _rebuy;
            set { _rebuy = value; OnPropertyChanged();}
        }

        private long _rebuy;

        /// <summary>the name of this ship</summary>
        [PublicAPI]
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
                    OnPropertyChanged();
                }
            }
        }
        private string _name;

        /// <summary>the model of the ship (Python, Anaconda, Cobra Mk. III, etc.)</summary>
        [PublicAPI]
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
                    OnPropertyChanged();
                }
            }
        }

        private string _model;

        /// <summary>the identifier of this ship</summary>

        [PublicAPI]
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
                    OnPropertyChanged();
                }
            }
        }

        private string _ident;

        /// <summary>the phonetic name of this ship</summary>
        
        [JsonProperty("phoneticname")]
        public string phoneticName
        {
            get => _phoneticName;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _phoneticName = null;
                }
                else if (IPA.IsValid(value))
                {
                    OnPropertyChanged();
                    _phoneticName = value;
                }
                OnPropertyChanged();
            }
        }
        
        /// <summary>The ship's spoken name (rendered using ssml and IPA)</summary>

        [PublicAPI, JsonIgnore]
        public string phoneticname => SpokenName();

        [JsonIgnore]
        private string _phoneticName;

        /// <summary>the role of this ship</summary>

        [JsonProperty]
        public string roleEDName
        {
            get => Role.edname;
            set
            {
                Role rDef = Role.FromEDName(value);
                this.Role = rDef;
                OnPropertyChanged();
            }
        }

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
                    OnPropertyChanged();
                }
            }
        }

        private Role _Role = Role.MultiPurpose;

        [PublicAPI, JsonIgnore, Obsolete("Please use localizedName or invariantName")]
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
                    OnPropertyChanged();
                }
            }
        }
        private string _raw;

        public bool RawIsNotNull => !string.IsNullOrEmpty(_raw);

        /// <summary>
        /// The wanted/hot status of this ship
        /// </summary>
        [PublicAPI, JsonIgnore]
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
                    OnPropertyChanged();
                }
            }
        }

        private bool _hot = false;

        // <summary>the location where this ship is stored; null if the commander is in this ship</summary>

        /// <summary>the name of the system in which this ship is stored; null if the commander is in this ship</summary>

        [PublicAPI]
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
                    OnPropertyChanged();
                }
            }
        }

        private string _starsystem;

        [Obsolete("Please use 'starsystem' instead")]
        public string system => starsystem; // Legacy Cottle scripts may use `system` rather than `starsystem`. 

        /// <summary>the name of the station in which this ship is stored; null if the commander is in this ship</summary>

        [PublicAPI]
        public string station
        {
            get => _station;
            set { _station = value; OnPropertyChanged();}
        }
        private string _station;

        [PublicAPI]
        public long? marketid
        {
            get => _marketid;
            set { _marketid = value; OnPropertyChanged();}
        }
        private long? _marketid;

        public decimal? x { get; set; }
        
        public decimal? y { get; set; }
        
        public decimal? z { get; set; }

        public bool intransit { get; set; }

        public long? transferprice { get; set; }

        public long? transfertime { get; set; }

        [PublicAPI]
        public decimal? distance
        {
            get => _distance;
            set { _distance = value; OnPropertyChanged();}
        }

        private decimal? _distance;

        [PublicAPI]
        public decimal health
        {
            get => _health;
            set { _health = value; OnPropertyChanged();}
        }

        private decimal _health;

        [PublicAPI]
        public Module cargohatch
        {
            get => _cargohatch;
            set { _cargohatch = value; OnPropertyChanged();}
        }
        private Module _cargohatch;

        [PublicAPI]
        public Module bulkheads
        {
            get => _bulkheads;
            set { _bulkheads = value; OnPropertyChanged();}
        }
        private Module _bulkheads;

        [PublicAPI]
        public Module canopy
        {
            get => _canopy;
            set { _canopy = value; OnPropertyChanged();}
        }
        private Module _canopy;

        [PublicAPI]
        public Module powerplant
        {
            get => _powerplant;
            set { _powerplant = value; OnPropertyChanged();}
        }

        private Module _powerplant;

        [PublicAPI]
        public Module thrusters
        {
            get => _thrusters;
            set { _thrusters = value; OnPropertyChanged();}
        }
        private Module _thrusters;

        [PublicAPI]
        public Module frameshiftdrive
        {
            get => _frameshiftdrive;
            set { _frameshiftdrive = value; OnPropertyChanged();}
        }
        private Module _frameshiftdrive;

        [PublicAPI]
        public Module lifesupport
        {
            get => _lifesupport;
            set { _lifesupport = value; OnPropertyChanged();}
        }
        private Module _lifesupport;

        [PublicAPI]
        public Module powerdistributor
        {
            get => _powerdistributor;
            set { _powerdistributor = value; OnPropertyChanged();}
        }
        private Module _powerdistributor;

        [PublicAPI]
        public Module sensors
        {
            get => _sensors;
            set { _sensors = value; OnPropertyChanged();}
        }
        private Module _sensors;

        [PublicAPI]
        public Module fueltank
        {
            get => _fueltank;
            set { _fueltank = value; OnPropertyChanged();}
        }
        private Module _fueltank;

        [PublicAPI]
        public List<Hardpoint> hardpoints
        {
            get => _hardpoints;
            set { _hardpoints = value; OnPropertyChanged();}
        }
        private List<Hardpoint> _hardpoints;

        [PublicAPI]
        public List<Compartment> compartments
        {
            get => _compartments;
            set { _compartments = value; OnPropertyChanged();}
        }
        private List<Compartment> _compartments;

        [PublicAPI]
        public List<LaunchBay> launchbays
        {
            get => _launchbays;
            set { _launchbays = value; OnPropertyChanged();}
        }
        private List<LaunchBay> _launchbays;

        public string paintjob { get; set; }

        [PublicAPI]
        public decimal? fueltankcapacity // Core capacity
        {
            get => _fueltankcapacity;
            set { _fueltankcapacity = value; OnPropertyChanged();}
        }
        private decimal? _fueltankcapacity;

        [PublicAPI]
        public decimal? fueltanktotalcapacity // Capacity including additional tanks
        {
            get => _fueltanktotalcapacity;
            set { _fueltanktotalcapacity = value; OnPropertyChanged();}
        }
        private decimal? _fueltanktotalcapacity;

        public decimal activeFuelReservoirCapacity { get; set; }

        // Ship jump and mass properties

        [PublicAPI]
        public decimal maxjumprange
        {
            get => _maxjumprange;
            set { _maxjumprange = value; OnPropertyChanged();}
        }
        private decimal _maxjumprange;

        [JsonIgnore, Obsolete("Please use maxjumprange instead")]
        public decimal maxjump => maxjumprange;

        [PublicAPI, JsonIgnore]
        public decimal maxfuelperjump => MaxFuelPerJump();

        [JsonIgnore, Obsolete("Please use maxfuelperjump instead")]
        public decimal maxfuel => maxfuelperjump;

        public decimal optimalmass { get; set; }

        public decimal unladenmass { get; set; }

        public decimal? fuelInTanks { get; set; }

        public int cargoCarried { get; set; }

        // Admin
        // The ID in Elite: Dangerous' database
        [JsonIgnore]
        public long EDID { get; set; }
        // The name in Elite: Dangerous' database
        public string EDName { get; set; }
        [JsonIgnore]
        internal string possessiveYour { get; set; }

        public Ship()
        {
            health = 100M;
            hardpoints = new List<Hardpoint>();
            compartments = new List<Compartment>();
            launchbays = new List<LaunchBay>();
            bulkheads = new Module();
            powerplant = new Module();
            thrusters = new Module();
            powerdistributor = new Module();
            frameshiftdrive = new Module();
            lifesupport = new Module();
            sensors = new Module();
            fueltank = new Module();
            cargohatch = new Module();
        }

        public Ship(long EDID, string EDName, string Manufacturer, string Model, string possessiveYour, List<Translation> PhoneticModel, LandingPadSize Size, int? MilitarySize, decimal reservoirFuelTankSize)
        {
            this.EDID = EDID;
            this.EDName = EDName;
            manufacturer = Manufacturer;
            model = Model;
            this.possessiveYour = possessiveYour;
            phoneticModel = PhoneticModel;
            this.Size = Size;
            militarysize = MilitarySize;
            health = 100M;
            hardpoints = new List<Hardpoint>();
            compartments = new List<Compartment>();
            launchbays = new List<LaunchBay>();
            bulkheads = new Module();
            powerplant = new Module();
            thrusters = new Module();
            powerdistributor = new Module();
            frameshiftdrive = new Module();
            lifesupport = new Module();
            sensors = new Module();
            fueltank = new Module();
            cargohatch = new Module();
            this.activeFuelReservoirCapacity = reservoirFuelTankSize;
        }

        public override string ToString()
        {
            // This is mostly to help with debugging
            return name ?? $"{Role.localizedName} {model}";
        }

        public string SpokenName(string defaultname = null)
        {
            string result;
            if (!string.IsNullOrWhiteSpace(phoneticName))
            {
                result = "<phoneme alphabet=\"ipa\" ph=\"" + phoneticName + "\">" + name + "</phoneme>";
            }
            else if (!string.IsNullOrWhiteSpace(name))
            {
                result = name;
            }
            else
            {
                result = $"{Properties.Ship.ResourceManager.GetString(possessiveYour) ?? Properties.Ship.your} {(defaultname ?? phoneticmodel) ?? Properties.Ship._ship}";
            }
            return result;
        }

        public string SpokenModel()
        {
            string result;
            if (phoneticModel == null)
            {
                result = model;
            }
            else
            {
                result = "";
                foreach (Translation item in phoneticModel)
                {
                    result += "<phoneme alphabet=\"ipa\" ph=\"" + item.to + "\">" + item.from + "</phoneme> ";
                }
            }
            return result;
        }

        public string SpokenManufacturer() => ShipDefinitions.SpokenManufacturer(manufacturer) ?? manufacturer;

        /// <summary> Calculates the distance from the specified coordinates to the ship's recorded x, y, and z coordinates </summary>
        public decimal? Distance(decimal? fromX, decimal? fromY, decimal? fromZ)
        {
            // Work out the distance to the system where the ship is stored if we can
            return Functions.StellarDistanceLy(x, y, z, fromX, fromY, fromZ);
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
                possessiveYour = template.possessiveYour;
                phoneticModel = template.phoneticModel;
                Size = template.Size;
                militarysize = template.militarysize;
                activeFuelReservoirCapacity = template.activeFuelReservoirCapacity;
                if (Role == null)
                {
                    Role = EddiDataDefinitions.Role.MultiPurpose;
                }
            }
        }

        public JumpDetail JumpDetails(string type, decimal? fuelInTanksOverride = null, int? cargoCarriedOverride = null)
        {
            var currentFuel = fuelInTanksOverride ?? fuelInTanks;
            var cargoTonnage = cargoCarriedOverride ?? cargoCarried;

            if (string.IsNullOrEmpty(type) || currentFuel is null) { return null; }

            decimal maxFuel = fueltanktotalcapacity ?? 0;

            if (!string.IsNullOrEmpty(type))
            {
                switch (type)
                {
                    case "next":
                        {
                            decimal jumpRange = JumpRange(currentFuel ?? 0, cargoTonnage);
                            return new JumpDetail(jumpRange, 1);
                        }
                    case "max":
                        {
                            decimal jumpRange = JumpRange(maxfuelperjump, cargoTonnage);
                            return new JumpDetail(jumpRange, 1);
                        }
                    case "total":
                        {
                            decimal total = 0;
                            int jumps = 0;
                            while (currentFuel > 0)
                            {
                                total += JumpRange(currentFuel ?? 0, cargoTonnage);
                                jumps++;
                                currentFuel -= Math.Min(currentFuel ?? 0, maxfuelperjump);
                            }
                            return new JumpDetail(total, jumps);
                        }
                    case "full":
                        {
                            decimal total = 0;
                            int jumps = 0;
                            while (maxFuel > 0)
                            {
                                total += JumpRange(maxFuel, cargoTonnage);
                                jumps++;
                                maxFuel -= Math.Min(maxFuel, maxfuelperjump);
                            }
                            return new JumpDetail(total, jumps);
                        }
                }
            }
            return null;
        }

        private decimal JumpRange(decimal currentFuel, int cargoCarried)
        {
            decimal boostConstant = 0;
            Module module = compartments.FirstOrDefault(c => c.module.edname.Contains("Int_GuardianFSDBooster"))?.module;
            if (module != null)
            {
                Constants.guardianBoostFSD.TryGetValue(module.@class, out boostConstant);
            }

            Constants.ratingConstantFSD.TryGetValue(frameshiftdrive.grade, out decimal ratingConstant);
            Constants.powerConstantFSD.TryGetValue(frameshiftdrive.@class, out decimal powerConstant);
            decimal massRatio = optimalmass / (unladenmass + currentFuel + cargoCarried);
            decimal fuel = Math.Min(currentFuel, maxfuelperjump);

            return ((decimal)Math.Pow((double)(1000 * fuel / ratingConstant), (double)(1 / powerConstant)) * massRatio) + boostConstant;
        }

        private decimal MaxFuelPerJump()
        {
            // Max fuel per jump calculated using unladen mass and max jump range w/ just enough fuel to complete max jump
            decimal boostConstant = 0;
            Module module = compartments.FirstOrDefault(c => c?.module?.edname != null && c.module.edname.Contains("Int_GuardianFSDBooster"))?.module;
            if (module != null)
            {
                Constants.guardianBoostFSD.TryGetValue(module.@class, out boostConstant);
            }
            Constants.ratingConstantFSD.TryGetValue(frameshiftdrive.grade, out decimal ratingConstant);
            Constants.powerConstantFSD.TryGetValue(frameshiftdrive.@class, out decimal powerConstant);
            decimal maxJumpRange = Math.Max(maxjumprange - boostConstant, 0);
            decimal massRatio = (unladenmass + maxfuelperjump) / optimalmass;

            return ratingConstant * (decimal)Math.Pow((double)(maxJumpRange * massRatio), (double)powerConstant) / 1000;
        }

        public static Ship FromShipyardInfo(ShipyardInfoItem item)
        {
            Ship ship = ShipDefinitions.FromEliteID(item.EliteID) ?? ShipDefinitions.FromEDModel(item.edModel, false);
            if (ship == null)
            {
                // Unknown ship; report the full object so that we can update the definitions 
                Logging.Info("Ship definition error: " + item.edModel);

                // Create a basic ship definition & supplement from the info available 
                ship = new Ship
                {
                    EDName = item.edModel
                };
            }
            ship.value = item.shipPrice;

            return ship;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

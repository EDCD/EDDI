using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utilities;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace EddiDataDefinitions
{
    /// <summary>A ship</summary>
    public class Ship : INotifyPropertyChanged
    {
        private static Regex IPA_REGEX = new Regex(@"^[bdfɡhjklmnprstvwzxaɪ˜iu\.ᵻᵿɑɐɒæɓʙβɔɕçɗɖðʤəɘɚɛɜɝɞɟʄɡ(ɠɢʛɦɧħɥʜɨɪʝɭɬɫɮʟɱɯɰŋɳɲɴøɵɸθœɶʘɹɺɾɻʀʁɽʂʃʈʧʉʊʋⱱʌɣɤʍχʎʏʑʐʒʔʡʕʢǀǁǂǃˈˌːˑʼʴʰʱʲʷˠˤ˞n̥d̥ŋ̊b̤a̤t̪d̪s̬t̬b̰a̰t̺d̺t̼d̼t̻d̻t̚ɔ̹ẽɔ̜u̟e̠ël̴n̴ɫe̽e̝ɹ̝m̩n̩l̩e̞β̞e̯e̘e̙ĕe̋éēèȅx͜xx͡x↓↑→↗↘]+$");

        /// <summary>the ID of this ship for this commander</summary>
        public int LocalId { get; set; }
        /// <summary>the manufacturer of the ship (Lakon, CoreDynamics etc.)</summary>
        [JsonIgnore]
        public string manufacturer { get; set; }
        /// <summary>the spoken manufacturer of the ship (Lakon, CoreDynamics etc.)</summary>
        [JsonIgnore]
        public List<Translation> phoneticmanufacturer { get; set; }
        /// <summary>the model of the ship (Python, Anaconda, etc.)</summary>
        public string model { get; set; }
        /// <summary>the spoken model of the ship (Python, Anaconda, etc.)</summary>
        [JsonIgnore]
        public List<Translation> phoneticmodel { get; set; }
        /// <summary>the size of this ship</summary>
        [JsonIgnore]
        public string size { get; set; }
        /// <summary>the size of the military compartment slots</summary>
        [JsonIgnore]
        public int? militarysize { get; set; }
        /// <summary>the total tonnage cargo capacity</summary>
        public int cargocapacity { get; set; }
        /// <summary>the current tonnage cargo carried</summary>
        [JsonIgnore]
        public int cargocarried { get; set; }

        /// <summary>the specific cargo carried</summary>
        [JsonIgnore]
        public List<Cargo> cargo { get; set; }

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
                if (value == null || value == "")
                {
                    PhoneticName = null;
                }
                else
                {
                    if (!IPA_REGEX.Match(value).Success)
                    {
                        // Invalid - drop silently
                        Logging.Debug("Invalid IPA " + value + "; discarding");
                        PhoneticName = null;
                    }
                    else
                    {
                        PhoneticName = value;
                    }
                }
            }
        }
        /// <summary>the role of this ship</summary>
        private string Role;
        public string role
        { get { return Role; }
            set
            {
                // Map old roles
                if (value == "0") Role = EddiDataDefinitions.Role.MultiPurpose;
                else if (value == "1") Role = EddiDataDefinitions.Role.Exploration;
                else if (value == "2") Role = EddiDataDefinitions.Role.Trading;
                else if (value == "3") Role = EddiDataDefinitions.Role.Mining;
                else if (value == "4") Role = EddiDataDefinitions.Role.Smuggling;
                else if (value == "5") Role = EddiDataDefinitions.Role.Piracy;
                else if (value == "6") Role = EddiDataDefinitions.Role.BountyHunting;
                else if (value == "7") Role = EddiDataDefinitions.Role.Combat;
                else Role = value;
            }
        }

        /// <summary>
        /// The raw JSON from the companion API for this ship
        /// </summary>
        public string raw { get; set; }

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
        private string _station;
        public string station
        {
            get
            {
                return _station;
            }
            set
            {
                if (_station != value)
                {
                    _station = value;
                    NotifyPropertyChanged("station");
                }
            }
        }

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
        public decimal? fueltankcapacity { get; set; } // Core capacity
        public decimal? fueltanktotalcapacity { get; set; } // Capacity including additional tanks
        public List<Hardpoint> hardpoints { get; set; }
        public List<Compartment> compartments { get; set; }
        public string paintjob { get; set; }

        // Admin
        // The ID in Elite: Dangerous' database
        [JsonIgnore]
        public long EDID { get; set; }
        // The name in Elite: Dangerous' database
        [JsonIgnore]
        public string EDName { get; set; }

        public Ship()
        {
            hardpoints = new List<Hardpoint>();
            compartments = new List<Compartment>();
            cargo = new List<Cargo>();
        }

        public Ship(long EDID, string EDName, string Manufacturer, List<Translation> PhoneticManufacturer, string Model, List<Translation> PhoneticModel, string Size, int? MilitarySize)
        {
            this.EDID = EDID;
            this.EDName = EDName;
            manufacturer = Manufacturer;
            phoneticmanufacturer = PhoneticManufacturer;
            model = Model;
            phoneticmodel = PhoneticModel;
            size = Size;
            militarysize = MilitarySize;
            hardpoints = new List<Hardpoint>();
            compartments = new List<Compartment>();
        }

        public string SpokenName(string defaultname = null)
        {
            string model = (defaultname == null ? SpokenModel() : defaultname) ?? "ship";
            string result = ("your " + model);
            if (!string.IsNullOrEmpty(phoneticname))
            {
                result = "<phoneme alphabet=\"ipa\" ph=\"" + phoneticname + "\">" + name + "</phoneme>";
            }
            else if (!string.IsNullOrEmpty(name))
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

        public string CoriolisUri()
        {
            if (raw != null)
            {
                // Generate a Coriolis import URI to retain as much information as possible
                string uri = "https://coriolis.edcd.io/import?";

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
                string bn;
                if (name == null)
                {
                    bn = role + " " + model;
                }
                else
                {
                    bn = name;
                }
                uri += "&bn=" + Uri.EscapeDataString(bn);

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
                if (role == null)
                {
                    role = EddiDataDefinitions.Role.MultiPurpose;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

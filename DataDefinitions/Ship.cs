using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>A ship</summary>
    public class Ship
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
        [JsonIgnore]
        public string model { get; set; }
        /// <summary>the spoken model of the ship (Python, Anaconda, etc.)</summary>
        [JsonIgnore]
        public List<Translation> phoneticmodel { get; set; }
        /// <summary>the size of this ship</summary>
        [JsonIgnore]
        public string size { get; set; }
        /// <summary>the value of the ship without cargo, in credits</summary>
        [JsonIgnore]
        public long value { get; set; }
        /// <summary>the total tonnage cargo capacity</summary>
        [JsonIgnore]
        public int cargocapacity { get; set; }
        /// <summary>the current tonnage cargo carried</summary>
        [JsonIgnore]
        public int cargocarried { get; set; }

        /// <summary>the specific cargo carried</summary>
        [JsonIgnore]
        public List<Cargo> cargo { get; set; }

        /// <summary>the name of this ship</summary>
        public string name { get; set; }

        /// <summary>the identifier of this ship</summary>
        public string ident { get; set; }

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
        [JsonIgnore]
        public string json { get; set; }
        
            /// <summary>the name of the system in which this ship is stored; null if the commander is in this ship</summary>
        [JsonIgnore]
        public string starsystem { get; set; }
        /// <summary>the name of the station in which this ship is stored; null if the commander is in this ship</summary>
        [JsonIgnore]
        public string station { get; set; }

        [JsonIgnore]
        public decimal health { get; set; }
        [JsonIgnore]
        public Module bulkheads { get; set; }
        [JsonIgnore]
        public Module powerplant { get; set; }
        [JsonIgnore]
        public Module thrusters { get; set; }
        [JsonIgnore]
        public Module frameshiftdrive { get; set; }
        [JsonIgnore]
        public Module lifesupport { get; set; }
        [JsonIgnore]
        public Module powerdistributor { get; set; }
        [JsonIgnore]
        public Module sensors { get; set; }
        [JsonIgnore]
        public Module fueltank { get; set; }
        [JsonIgnore]
        public decimal fueltankcapacity { get; set; } // Core capacity
        [JsonIgnore]
        public decimal fueltanktotalcapacity { get; set; } // Capacity including additional tanks
        [JsonIgnore]
        public List<Hardpoint> hardpoints { get; set; }
        [JsonIgnore]
        public List<Compartment> compartments { get; set; }

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
        }

        public Ship(long EDID, string EDName, string Manufacturer, List<Translation> PhoneticManufacturer, string Model, List<Translation> PhoneticModel, string Size)
        {
            this.EDID = EDID;
            this.EDName = EDName;
            manufacturer = Manufacturer;
            phoneticmanufacturer = PhoneticManufacturer;
            model = Model;
            phoneticmodel = PhoneticModel;
            size = Size;
            hardpoints = new List<Hardpoint>();
            compartments = new List<Compartment>();
        }

        public string SpokenName(string defaultname = null)
        {
            string model = defaultname == null ? SpokenModel() : defaultname;
            if (model == null)
            {
                model = "ship";
            }
            string result = ("your " + model);
            if (phoneticname != null)
            {
                result = "<phoneme alphabet=\"ipa\" ph=\"" + phoneticname + "\">" + name + "</phoneme>";
            }
            else if (name != null)
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
    }
}

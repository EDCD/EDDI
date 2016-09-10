using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utilities;

namespace EliteDangerousDataDefinitions
{
    /// <summary>A ship</summary>
    public class Ship
    {
        private static Regex IPA_REGEX = new Regex(@"^[bdfɡhjklmnprstvwzxaɪ˜iu\.ᵻᵿɑɐɒæɓʙβɔɕçɗɖðʤəɘɚɛɜɝɞɟʄɡ(ɠɢʛɦɧħɥʜɨɪʝɭɬɫɮʟɱɯɰŋɳɲɴøɵɸθœɶʘɹɺɾɻʀʁɽʂʃʈʧʉʊʋⱱʌɣɤʍχʎʏʑʐʒʔʡʕʢǀǁǂǃˈˌːˑʼʴʰʱʲʷˠˤ˞n̥d̥ŋ̊b̤a̤t̪d̪s̬t̬b̰a̰t̺d̺t̼d̼t̻d̻t̚ɔ̹ẽɔ̜u̟e̠ël̴n̴ɫe̽e̝ɹ̝m̩n̩l̩e̞β̞e̯e̘e̙ĕe̋éēèȅx͜xx͡x↓↑→↗↘]+$");

        /// <summary>the ID of this ship for this commander</summary>
        public int LocalId { get; set; }
        /// <summary>the model of the ship (Python, Anaconda, etc.)</summary>
        [JsonIgnore]
        public string model { get; set; }
        /// <summary>the size of this ship</summary>
        [JsonIgnore]
        public ShipSize size { get; set; }
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
        public List<Cargo> cargo { get; set; }

        /// <summary>the callsign of this ship</summary>
        public string callsign { get; set;  }
        /// <summary>the name of this ship</summary>
        public string name { get; set; }
        [JsonIgnore]
        private string PhoneticName;
        /// <summary>the phonetic name of this ship</summary>
        public string phoneticname
        {
            get { return this.PhoneticName; }
            set {
                if (value == null || value == "")
                {
                    this.PhoneticName = null;
                }
                else
                {
                    if (!IPA_REGEX.Match(value).Success)
                    {
                        // Invalid - drop silently
                        Logging.Debug("Invalid IPA " + value + "; discarding");
                        this.PhoneticName = null;
                    }
                    else
                    {
                        this.PhoneticName = value;
                    }
                }
            }
        }
        /// <summary>the role of this ship</summary>
        public ShipRole role { get; set; }

        /// <summary>the name of the system in which this ship is stored; null if the commander is in this ship</summary>
        [JsonIgnore]
        public string StarSystem { get; set; }
        /// <summary>the name of the station in which this ship is stored; null if the commander is in this ship</summary>
        [JsonIgnore]
        public string Station { get; set; }

        [JsonIgnore]
        public decimal Health { get; set; }
        [JsonIgnore]
        public Module Bulkheads { get; set; }
        [JsonIgnore]
        public Module PowerPlant { get; set; }
        [JsonIgnore]
        public Module Thrusters { get; set; }
        [JsonIgnore]
        public Module FrameShiftDrive { get; set; }
        [JsonIgnore]
        public Module LifeSupport { get; set; }
        [JsonIgnore]
        public Module PowerDistributor { get; set; }
        [JsonIgnore]
        public Module Sensors { get; set; }
        [JsonIgnore]
        public Module FuelTank { get; set; }
        [JsonIgnore]
        public decimal FuelTankCapacity { get; set; }
        [JsonIgnore]
        public List<Hardpoint> Hardpoints { get; set; }
        [JsonIgnore]
        public List<Compartment> Compartments { get; set; }

        // Admin
        // The ID in Elite: Dangerous' database
        public long EDID { get; set; }
        // The name in Elite: Dangerous' database
        public string EDName { get; set; }

        public Ship()
        {
            Hardpoints = new List<Hardpoint>();
            Compartments = new List<Compartment>();
        }

        public Ship(long EDID, string EDName, string Model, ShipSize Size)
        {
            this.EDID = EDID;
            this.EDName = EDName;
            this.model = Model;
            this.size = Size;
            Hardpoints = new List<Hardpoint>();
            Compartments = new List<Compartment>();
        }

        private static Random random = new Random();
        /// <summary>
        /// A callsign is a set of three letters followed by a dash and then four numbers
        /// </summary>
        public static string generateCallsign()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numerics = "0123456789";
            return new string(Enumerable.Repeat(chars, 3)
              .Select(s => s[random.Next(s.Length)]).ToArray())
              + "-"
              + new string(Enumerable.Repeat(numerics, 4)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    /// <summary>The role of a ship</summary>
    public enum ShipRole
    {
        Multipurpose,
        Exploring,
        Trading,
        Mining,
        Smuggling,
        Piracy,
        BountyHunting,
        Combat
    }

    /// <summary>The size of a ship</summary>
    public enum ShipSize
    {
        Small,
        Medium,
        Large,
        Huge
    }
}

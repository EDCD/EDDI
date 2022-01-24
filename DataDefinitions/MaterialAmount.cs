using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Utilities;

namespace EddiDataDefinitions
{
    public class MaterialAmount : INotifyPropertyChanged
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue(null)]
        public string edname { get; private set; }

        [JsonIgnore]
        private string _material;

        [PublicAPI, JsonIgnore]
        public string material
        {
            get
            {
                return _material;
            }
            set
            {
                if (_material != value)
                {
                    Material My_material = Material.FromName(value) ?? Material.FromEDName(value);
                    _material = My_material?.localizedName ?? value;
                    edname = My_material?.edname ?? value;
                    category = My_material?.Category.localizedName;
                    NotifyPropertyChanged("material");
                }
            }
        }

        [JsonIgnore]
        private int _amount;

        [PublicAPI]
        public int amount
        {
            get
            {
                return _amount;
            }
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    NotifyPropertyChanged("amount");
                }
            }
        }

        [JsonIgnore]
        private int? _minimum;

        [PublicAPI]
        public int? minimum
        {
            get
            {
                return _minimum;
            }
            set
            {
                if (_minimum != value)
                {
                    _minimum = value;
                    NotifyPropertyChanged("minimum");
                }
            }
        }

        [JsonIgnore]
        private int? _desired;

        [PublicAPI]
        public int? desired
        {
            get
            {
                return _desired;
            }
            set
            {
                if (_desired != value)
                {
                    _desired = value;
                    NotifyPropertyChanged("desired");
                }
            }
        }

        [JsonIgnore]
        private int? _maximum;

        [PublicAPI]
        public int? maximum
        {
            get
            {
                return _maximum;
            }
            set
            {
                if (_maximum != value)
                {
                    _maximum = value;
                    NotifyPropertyChanged("maximum");
                }
            }
        }

        [JsonIgnore]
        private string _Category;
        
        [PublicAPI, JsonIgnore]
        public string category
        {
            get
            {
                return _Category;
            }
            set
            {
                if (_Category != value)
                {
                    _Category = value;
                    NotifyPropertyChanged("category");
                }
            }
        }

        [JsonIgnore] 
        private Rarity _Rarity;

        [PublicAPI]
        public Rarity Rarity
        {
            get => _Rarity;
            set
            {
                _Rarity = value;
                NotifyPropertyChanged("Rarity");
            }
        }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalData = new Dictionary<string, JToken>();

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (material == null)
            {
                string materialName = (string)_additionalData["material"];
                material = materialName;
            }

            if (_Rarity is null)
            {
                _Rarity = Material.FromEDName(edname)?.Rarity ?? Rarity.Unknown;
            }

            _additionalData = null;
        }

        public MaterialAmount(Material material, int amount)
            : this(material.edname, material.Rarity, amount, null, null, null)
        { }

        public MaterialAmount(Material material, int amount, int? minimum, int? desired, int? maximum)
            : this(material.edname, material.Rarity, amount, minimum, desired, maximum)
        { }

        [JsonConstructor]
        public MaterialAmount(string edname, Rarity Rarity, int amount, int? minimum, int? desired, int? maximum)
        {
            Material My_material = Material.FromEDName(edname);
            this.material = My_material?.localizedName;
            this.edname = My_material?.edname;
            this.amount = amount;
            this.minimum = minimum;
            this.desired = desired;
            this.maximum = maximum;
            this.category = My_material?.Category.localizedName;
            this.Rarity = Rarity;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

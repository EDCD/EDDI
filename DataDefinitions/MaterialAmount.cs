using Newtonsoft.Json;
using System.ComponentModel;

namespace EddiDataDefinitions
{
    public class MaterialAmount : INotifyPropertyChanged
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(null)]
        public string edname { get; private set; }

        [JsonIgnore]
        private string _material;
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
                    _material = value;
                    NotifyPropertyChanged("material");
                }
            }
        }

        [JsonIgnore]
        private int _amount;
        public int amount { get
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

        private string _Category;
        public string Category
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

        public MaterialAmount(Material material, int amount)
        {
            Material My_material = Material.FromEDName(material.EDName);
            this.material = My_material.name;
            this.edname = My_material.EDName;
            this.amount = amount;
            this.Category = My_material.category;
        }

        public MaterialAmount(Material material, int amount, int? minimum, int? desired, int? maximum)
        {
            Material My_material = Material.FromEDName(material.EDName);
            this.material = My_material.name;
            this.edname = My_material.EDName;
            this.amount = amount;
            this.minimum = minimum;
            this.desired = desired;
            this.maximum = maximum;
            this.Category = My_material.category;
        }

        [JsonConstructor]
        public MaterialAmount(string material, int amount, int? minimum, int? desired, int? maximum)
        {
            Material My_material = Material.FromName(material);
            this.material = material;
            this.edname = My_material.EDName;
            this.amount = amount;
            this.minimum = minimum;
            this.desired = desired;
            this.maximum = maximum;
            this.Category = My_material.category;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

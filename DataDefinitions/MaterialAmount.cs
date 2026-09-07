using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using Utilities;

namespace EddiDataDefinitions
{
    public class MaterialAmount : INotifyPropertyChanged
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate), DefaultValue(null)]
        public string edname
        {
            get => MaterialDef?.edname;
            set => SetDefinition(Material.FromEDName(value));
        }

        [JsonIgnore]
        public Material MaterialDef { get; private set; }

        [PublicAPI( "the material's localized name" ), JsonIgnore]
        public string material
        {
            get => MaterialDef?.localizedName;
            set => edname = Material.FromName(value)?.edname ?? value;
        }

        [JsonIgnore]
        private int _amount;

        [PublicAPI( "the amount" )]
        public int amount
        {
            get => _amount;
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

        [PublicAPI( "your minimum amount" )]
        public int? minimum
        {
            get => _minimum;
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

        [PublicAPI( "your desired amount" )]
        public int? desired
        {
            get => _desired;
            set
            {
                if (_desired != value)
                {
                    _desired = value;
                    NotifyPropertyChanged("desired");
                }
            }
        }

        [PublicAPI( "your maximum amount" )]
        public int? maximum => 350 - ( 50 * Math.Max(Rarity.level, 1 ) );

        [JsonIgnore]
        private string _Category;
        
        [PublicAPI( "the material's localized category" ), JsonIgnore]
        public string category
        {
            get => _Category;
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

        [PublicAPI( "the material's rarity, as an object" ), JsonIgnore]
        public Rarity Rarity
        {
            get => _Rarity ?? Rarity.Unknown;
            set
            {
                if ( _Rarity != value )
                {
                    _Rarity = value;
                    NotifyPropertyChanged( "Rarity" );
                    NotifyPropertyChanged("maximum");
                }
            }
        }

        [JsonExtensionData]
        private Dictionary<string, JToken> _additionalData = [];

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (material == null && _additionalData.TryGetValue("material", out var legacyMaterial))
            {
                material = (string)legacyMaterial;
            }

            _additionalData = null;
        }

        public MaterialAmount(Material material, int amount)
            : this(material, amount, null, null)
        { }

        public MaterialAmount(Material material, int amount, int? minimum, int? desired)
        {
            SetDefinition(material);
            this.amount = amount;
            this.minimum = minimum;
            this.desired = desired;
        }

        [JsonConstructor]
        public MaterialAmount(string edname, int amount, int? minimum, int? desired)
        {
            this.edname = edname;
            this.amount = amount;
            this.minimum = minimum;
            this.desired = desired;
        }

        private void SetDefinition(Material definition)
        {
            if (ReferenceEquals(MaterialDef, definition)) { return; }
            MaterialDef = definition;
            category = definition?.Category?.localizedName;
            Rarity = definition?.Rarity ?? Rarity.Unknown;
            NotifyPropertyChanged(nameof(MaterialDef));
            NotifyPropertyChanged(nameof(edname));
            NotifyPropertyChanged(nameof(material));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

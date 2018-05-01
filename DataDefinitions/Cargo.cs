using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Cargo defines a number of commodities carried along with some additional data
    /// </summary>
    public class Cargo : INotifyPropertyChanged
    {
        // The commodity name
        [JsonIgnore]
        public string invariantName => commodityDef?.invariantName ?? "";
        [JsonIgnore]
        public string localizedName => commodityDef?.localizedName ?? "";

        public string edname
        {
            get => commodityDef.edname;
            set
            {
                CommodityDefinition cDef = CommodityDefinition.FromEDName(value);
                this.commodityDef = cDef;
            }
        }

        // The number of stolen items
        [JsonIgnore]
        private int _stolen;
        public int stolen
        {
            get => _stolen;
            set
            {
                if (_stolen != value)
                {
                    _stolen = value;
                    NotifyPropertyChanged("stolen");
                }
            }
        }

        // The number of items related to a mission
        [JsonIgnore]
        private int _haulage;
        public int haulage
        {
            get => _haulage;
            set
            {
                if (_haulage != value)
                {
                    _haulage = value;
                    NotifyPropertyChanged("haulage");
                }
            }
        }

        // The number of collected/purchased items
        [JsonIgnore]
        private int _other;
        public int other
        {
            get => _other;
            set
            {
                if (_other != value)
                {
                    _other = value;
                    NotifyPropertyChanged("other");
                }
            }
        }

        // Total amount of the commodity
        public int total { get; set; }

        // How much cargo has been ejected during a game session
        public int ejected { get; set; }

        // How much we actually paid for it (per unit)
        public int price { get; set; }

        // The commodity category, localized
        [JsonIgnore]
        public string localizedCategory => commodityDef?.category?.localizedName ?? null;

        // deprecated commodity category (exposed to Cottle and VA)
        [JsonIgnore, Obsolete("Please use localizedCategory instead")]
        public string category => localizedCategory;

        [JsonIgnore]
        private CommodityDefinition _commodityDef;
        [JsonIgnore]
        public CommodityDefinition commodityDef
        {
            get => _commodityDef;
            set
            {
                _commodityDef = value;
                NotifyPropertyChanged("invariantName");
                NotifyPropertyChanged("localizedName");
                NotifyPropertyChanged("localizedCategory");
            }
        }

        public List<HaulageAmount> haulageamounts { get; set; }

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalJsonData;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (commodityDef == null)
            {
                // legacy JSON with no edname in the top level
                string edname = (string)_additionalJsonData["commodity"]["EDName"];
                commodityDef = CommodityDefinition.FromEDName(edname);
            }

            _additionalJsonData = null;
        }

        [JsonConstructor]
        public Cargo(string edname, int total, int? price = null)
        {
            this.commodityDef = CommodityDefinition.FromEDName(edname);
            this.price = price ?? commodityDef.avgprice;
            this.total = total;
            this.ejected = 0;
            haulageamounts = new List<HaulageAmount>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            this.total = this.stolen + this.haulage + this.other;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

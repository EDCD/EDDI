using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Cargo defines a number of commodities carried along with some additional data
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Cargo : INotifyPropertyChanged
    {
        // The commodity name
        public string invariantName => commodityDef?.invariantName ?? "";

        public string localizedName => commodityDef?.localizedName ?? "";
        
        [Obsolete("Please use localizedName or invariantName")]
        public string name => localizedName;

        [JsonProperty("edname")]
        public string edname
        {
            get => commodityDef.edname;
            set
            {
                this.commodityDef = CommodityDefinition.FromEDName(value);
            }
        }

        // The number of stolen items
        [JsonProperty("stolen")]
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
        [JsonProperty("haulage")]
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
        [JsonProperty("owned")]
        private int _owned;
        public int owned
        {
            get => _owned;
            set
            {
                if (_owned != value)
                {
                    _owned = value;
                    NotifyPropertyChanged("owned");
                }
            }
        }

        [Obsolete("please use owned instead")]
        public int other => owned;

        // The number of items needed for missions
        [JsonProperty("need")]
        private int _need;
        public int need
        {
            get => _need;
            set
            {
                if (_need != value)
                {
                    _need = value;
                    NotifyPropertyChanged("need");
                }
            }
        }

        // Total amount of the commodity
        public int total => haulage + stolen + owned;

        // How much we actually paid for it (per unit)
        public int price => decimal.ToInt32(weightedAvgPrice);

        [JsonProperty("price")]
        private decimal weightedAvgPrice;

        // The commodity category, localized
        public string localizedCategory => commodityDef?.category?.localizedName ?? null;

        // deprecated commodity category (exposed to Cottle and VA)
        [Obsolete("Please use localizedCategory instead")]
        public string category => localizedCategory;

        private CommodityDefinition _commodityDef;
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

        [Obsolete]
        public CommodityDefinition commodity => commodityDef;

        [JsonProperty("haulageData")]
        public List<Haulage> haulageData { get; set; } = new List<Haulage>();

        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalJsonData;

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (commodityDef == null)
            {
                // legacy JSON with no edname in the top level
                edname = (string)_additionalJsonData["commodity"]["edname"];
                owned = (int)_additionalJsonData["other"];
            }

            _additionalJsonData = null;
        }

        // Default Constructor
        public Cargo() { }

        [JsonConstructor]
        public Cargo(string edname)
        {
            commodityDef = CommodityDefinition.FromEDName(edname);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public void CalculateNeed()
        {
            if (haulageData != null && haulageData.Any())
            {
                need = haulageData.Sum(h => h.need);
            }
        }

        public void CalculateWeightedPrice(int thisPrice, int amount)
        {
            if (amount == 0) { return; }
            var weightedValueSum = (weightedAvgPrice * total) + (thisPrice * amount);
            var weightedQtySum = total + amount;
            weightedAvgPrice = weightedValueSum / weightedQtySum;
            NotifyPropertyChanged("price");
        }

        public void AddDetailedQty(CargoType cargoType, int amount, Haulage cargoHaulageData = null, int _price = 0)
        {
            CalculateWeightedPrice(_price, amount);
            switch (cargoType)
            {
                case CargoType.haulage:
                    {
                        haulage += amount;
                        if (cargoHaulageData != null) { haulageData.Add(cargoHaulageData); }
                        break;
                    }
                case CargoType.stolen:
                    {
                        stolen += amount;
                        break;
                    }
                default:
                    {
                        owned += amount;
                        break;
                    }
            }
        }

        public void RemoveDetailedQty(CargoType cargoType, int amount, long? missionId)
        {
            var thisHaulageData = haulageData.FirstOrDefault(h => h.missionid == missionId);
            RemoveDetailedQty(cargoType, amount, thisHaulageData);
        }

        public void RemoveDetailedQty(CargoType cargoType, int amount, Haulage cargoHaulageData = null)
        {
            switch (cargoType)
            {
                case CargoType.haulage:
                    {
                        haulage -= amount;
                        if (cargoHaulageData != null) { haulageData.Remove(cargoHaulageData); }
                        break;
                    }
                case CargoType.stolen:
                    {
                        stolen -= amount;
                        break;
                    }
                default:
                    {
                        owned -= amount;
                        break;
                    }
            }
        }
    }

    public enum CargoType
    { 
        haulage,
        owned,
        stolen
    }
}

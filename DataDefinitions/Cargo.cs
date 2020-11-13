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

        public void UpdateWeightedPrice(decimal newPrice, int newAmount)
        {
            if (newAmount == 0) { return; }
            var weightedValueSum = (weightedAvgPrice * total) + (newPrice * newAmount);
            var weightedQtySum = total + newAmount;

            if (weightedQtySum > 0)
            {
                weightedAvgPrice = weightedValueSum / weightedQtySum;
                NotifyPropertyChanged("price");
            }
        }

        /// <summary> </summary>
        /// <param name="cargoType">The type of cargo to add</param>
        /// <param name="acquistionAmount">The amount of cargo to add</param>
        /// <param name="cargoHaulageData">Add or update haulage instance</param>
        /// <param name="acquistionPrice">The acquisition price per unit (if not zero)</param>
        public void AddDetailedQty(CargoType cargoType, int acquistionAmount, decimal acquistionPrice, Haulage cargoHaulageData = null)
        {
            UpdateWeightedPrice(acquistionPrice, acquistionAmount);
            switch (cargoType)
            {
                case CargoType.haulage:
                    {
                        haulage += acquistionAmount;
                        if (cargoHaulageData != null) 
                        {
                            var haulageIndex = haulageData.FindIndex(h => h.missionid == cargoHaulageData.missionid);
                            if (haulageIndex > -1)
                            {
                                haulageData[haulageIndex] = cargoHaulageData;
                            }
                            else
                            {
                                haulageData.Add(cargoHaulageData);
                            }
                        }
                        break;
                    }
                case CargoType.stolen:
                    {
                        stolen += acquistionAmount;
                        break;
                    }
                default:
                    {
                        owned += acquistionAmount;
                        break;
                    }
            }
        }

        /// <param name="cargoType">The type of cargo to remove</param>
        /// <param name="removedAmount">The amount of cargo to remove</param>
        /// <param name="missionId">Remove haulage instance by mission ID</param>
        public void RemoveDetailedQty(CargoType cargoType, int removedAmount, long? missionId)
        {
            var thisHaulageData = haulageData.FirstOrDefault(h => h.missionid == missionId);
            RemoveDetailedQty(cargoType, removedAmount, thisHaulageData);
        }

        /// <param name="cargoType">The type of cargo to remove</param>
        /// <param name="removedAmount">The amount of cargo to remove</param>
        /// <param name="cargoHaulageData">Remove haulage instance</param>
        public void RemoveDetailedQty(CargoType cargoType, int removedAmount, Haulage cargoHaulageData = null)
        {
            switch (cargoType)
            {
                case CargoType.haulage:
                    {
                        haulage -= removedAmount;
                        if (cargoHaulageData != null) { haulageData.Remove(cargoHaulageData); }
                        break;
                    }
                case CargoType.stolen:
                    {
                        stolen -= removedAmount;
                        break;
                    }
                default:
                    {
                        owned -= removedAmount;
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

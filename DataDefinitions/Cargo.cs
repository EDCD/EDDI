using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Cargo defines a number of commodities carried along with some additional data
    /// </summary>
    [JsonObject( MemberSerialization.OptIn )]
    public class Cargo : INotifyPropertyChanged
    {
        // The commodity name
        public string invariantName => commodityDef?.invariantName ?? "";

        public string localizedName => commodityDef?.localizedName ?? "";

        [PublicAPI, Obsolete( "Please use localizedName or invariantName" )]
        public string name => localizedName;

        [JsonProperty( nameof( edname ) )]
        public string edname
        {
            get => commodityDef.edname;
            set => commodityDef = CommodityDefinition.FromEDName( value );
        }

        // The number of stolen items
        [PublicAPI]
        public int stolen
        {
            get => _stolen;
            set
            {
                if ( _stolen != value )
                {
                    _stolen = value;
                    NotifyPropertyChanged( nameof( stolen ) );
                }
            }
        }
        [JsonProperty(nameof(stolen))]
        private int _stolen;

        // The number of items related to a mission currently on-board
        public int haulage => missionCargo.Values.Sum();

        // The number of collected/purchased items
        [PublicAPI]
        public int owned
        {
            get => _owned;
            set
            {
                if ( _owned != value )
                {
                    _owned = value;
                    NotifyPropertyChanged( nameof( owned ) );
                }
            }
        }
        [JsonProperty(nameof(owned))]
        private int _owned;

        [Obsolete( "please use owned instead" )]
        public int other => owned;

        // Mission items on board (with MissionID and count)
        [PublicAPI("A dictionary where the key is a mission ID and the value is the amount of cargo associated with that mission ID")]
        public Dictionary<long, int> missionCargo
        {
            get => _missionCargo;
            set
            {
                if ( _missionCargo != value )
                {
                    _missionCargo = value;
                    NotifyPropertyChanged( nameof( missionCargo ) );
                }
            }
        }
        [ JsonProperty(nameof(missionCargo)) ]
        private Dictionary<long, int> _missionCargo = new Dictionary<long, int>();

        [ PublicAPI ] public int need { get; set; }

        // Total amount of the commodity

        [PublicAPI]
        public int total => haulage + stolen + owned;

        // How much we actually paid for it (per unit)

        [PublicAPI]
        public int price => decimal.ToInt32( weightedAvgPrice );

        [JsonProperty(nameof(price))]
        private decimal weightedAvgPrice;

        // The commodity category, localized
        public string localizedCategory => commodityDef?.Category?.localizedName;

        // deprecated commodity category (exposed to Cottle and VA)
        [PublicAPI, Obsolete( "Please use localizedCategory instead" )]
        public string category => localizedCategory;

        private CommodityDefinition _commodityDef;
        public CommodityDefinition commodityDef
        {
            get => _commodityDef;
            set
            {
                _commodityDef = value;
                NotifyPropertyChanged( nameof(invariantName) );
                NotifyPropertyChanged( nameof(localizedName) );
                NotifyPropertyChanged( nameof(localizedCategory) );
            }
        }

        [PublicAPI, Obsolete]
        public CommodityDefinition commodity => commodityDef;
        
        [JsonExtensionData]
        private IDictionary<string, JToken> _additionalJsonData;

        [OnDeserialized]
        private void OnDeserialized ( StreamingContext context )
        {
            if ( commodityDef == null )
            {
                // legacy JSON with no edname in the top level
                edname = (string)_additionalJsonData[ "commodity" ][ "edname" ];
                owned = (int)_additionalJsonData[ "other" ];
            }

            _additionalJsonData = null;
        }

        // Default Constructor
        public Cargo () { }

        [JsonConstructor]
        public Cargo ( string edname )
        {
            commodityDef = CommodityDefinition.FromEDName( edname );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged ( string propName )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propName ) );
        }

        public void UpdateWeightedPrice ( decimal newPrice, int newAmount )
        {
            if ( newAmount == 0 ) { return; }
            var weightedValueSum = (weightedAvgPrice * owned) + (newPrice * newAmount);
            var weightedQtySum = owned + newAmount;

            if ( weightedQtySum > 0 )
            {
                weightedAvgPrice = weightedValueSum / weightedQtySum;
                NotifyPropertyChanged( "price" );
            }
        }

        /// <summary> Add non-mission cargo </summary>
        /// <param name="cargoType">The type of cargo to add (e.g. legal or stolen)</param>
        /// <param name="acquistionAmount">The amount of cargo to add</param>
        /// <param name="acquistionPrice">The acquisition price per unit (if not zero)</param>
        public void AddDetailedQty ( CargoType cargoType, int acquistionAmount, decimal? acquistionPrice = 0)
        {
            UpdateWeightedPrice( acquistionPrice ?? 0, acquistionAmount );

            switch ( cargoType )
            {
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

        /// <summary> Add mission cargo </summary>
        /// <param name="missionID">Add mission cargo by mission ID</param>
        /// <param name="acquistionAmount">The amount of cargo to add</param>
        public void AddDetailedQty ( long missionID, int acquistionAmount )
        {
            if ( missionCargo.ContainsKey( missionID ) )
            {
                missionCargo[ missionID ] += acquistionAmount;
            }
            else
            {
                missionCargo.Add( missionID, acquistionAmount );
            }

            NotifyPropertyChanged( nameof(missionCargo) );
        }

        /// <summary> Remove non-mission cargo </summary>
        /// <param name="cargoType">The type of cargo to remove (e.g. legal or stolen)</param>
        /// <param name="removedAmount">The amount of cargo to remove</param>
        public void RemoveDetailedQty ( CargoType cargoType, int removedAmount )
        {
            switch ( cargoType )
            {
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

        /// <summary> Remove mission cargo </summary>
        /// <param name="missionID">Remove mission cargo by mission ID</param>
        /// <param name="removedAmount">The amount of cargo to remove</param>
        public void RemoveDetailedQty ( long missionID, int removedAmount )
        {
            if ( missionCargo.ContainsKey( missionID ) )
            {
                var cargoAmount = missionCargo[ missionID ];
                missionCargo[ missionID ] -= Math.Min( removedAmount, cargoAmount );

                if ( missionCargo[ missionID ] == 0 )
                {
                    missionCargo.Remove( missionID );
                }
            }
        }
    }

    public enum CargoType
    {
        legal,
        stolen
    }
}
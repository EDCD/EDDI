using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using Utilities;

namespace EddiDataDefinitions
{
    public class FleetCarrier : FrontierApiFleetCarrier, INotifyPropertyChanged
    {
        // Parameters not obtained from the Frontier API
        // Note: Any information not updated from the Frontier API will need to be reset when the Frontier API refreshes the fleet carrier definition.

        [PublicAPI]
        public long? carrierID
        {
            get => _carrierId;
            set
            {
                if (value == _carrierId) return;
                _carrierId = value;
                OnPropertyChanged();
            }
        }
        private long? _carrierId;

        public static FleetCarrier FromFrontierApiFleetCarrier(FleetCarrier currentFleetCarrier, FrontierApiFleetCarrier frontierApiFleetCarrier, DateTime apiTimeStamp, DateTime journalTimeStamp, out bool carrierMatches)
        {
            if (frontierApiFleetCarrier is null) { carrierMatches = true; return currentFleetCarrier; }

            // Copy our current fleet carrier to a new object and update that new object
            var fleetCarrier = currentFleetCarrier.Copy() ?? new FleetCarrier();

            // Verify that the profile information matches the current fleet carrier callsign
            if (fleetCarrier.callsign != null && frontierApiFleetCarrier.callsign != fleetCarrier.callsign)
            {
                Logging.Warn("Frontier API incorrectly configured: Returning information for Fleet Carrier " +
                    frontierApiFleetCarrier.callsign + " rather than for " + fleetCarrier.callsign + ". Disregarding incorrect information.");
                carrierMatches = false;
                return fleetCarrier;
            }

            carrierMatches = true;
            fleetCarrier.callsign = frontierApiFleetCarrier.callsign;
            fleetCarrier.carrierID = frontierApiFleetCarrier.Market.marketId;

            // Information exclusively available from the Frontier API
            fleetCarrier.currentStarSystem = frontierApiFleetCarrier.currentStarSystem;
            fleetCarrier.nextStarSystem = frontierApiFleetCarrier.nextStarSystem;
            fleetCarrier.fuelInCargo = frontierApiFleetCarrier.fuelInCargo;
            fleetCarrier.Cargo = frontierApiFleetCarrier.Cargo;
            fleetCarrier.CarrierLockerAssets = frontierApiFleetCarrier.CarrierLockerAssets;
            fleetCarrier.CarrierLockerGoods = frontierApiFleetCarrier.CarrierLockerGoods;
            fleetCarrier.CarrierLockerData = frontierApiFleetCarrier.CarrierLockerData;
            fleetCarrier.commodityPurchaseOrders = frontierApiFleetCarrier.commodityPurchaseOrders;
            fleetCarrier.commoditySalesOrders = frontierApiFleetCarrier.commoditySalesOrders;
            fleetCarrier.microresourcePurchaseOrders = frontierApiFleetCarrier.microresourcePurchaseOrders;
            fleetCarrier.microresourceSalesOrders = frontierApiFleetCarrier.microresourceSalesOrders;

            // Information which might be newer, check timestamps prior to updating
            if (apiTimeStamp > journalTimeStamp)
            {
                fleetCarrier.name = frontierApiFleetCarrier.name;
                fleetCarrier.dockingAccess = frontierApiFleetCarrier.dockingAccess;
                fleetCarrier.notoriousAccess = frontierApiFleetCarrier.notoriousAccess;
                fleetCarrier.fuel = frontierApiFleetCarrier.fuel;
                fleetCarrier.state = frontierApiFleetCarrier.state;
                fleetCarrier.bankBalance = frontierApiFleetCarrier.bankBalance;
                fleetCarrier.bankReservedBalance = frontierApiFleetCarrier.bankReservedBalance;
                fleetCarrier.usedCapacity = frontierApiFleetCarrier.usedCapacity;
                fleetCarrier.freeCapacity = frontierApiFleetCarrier.freeCapacity;
            }

            return fleetCarrier;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }

    public class FrontierApiFleetCarrier : INotifyPropertyChanged
    {
        private FrontierApiProfileStation _market = new FrontierApiProfileStation();
        private string _name;
        private string _callsign;
        private string _currentStarSystem;
        private string _nextStarSystem;
        private int _fuel;
        private int _fuelInCargo;
        private string _state;
        private string _dockingAccess;
        private bool _notoriousAccess;
        private int _usedCapacity;
        private int _freeCapacity;
        private long _bankBalance;
        private ulong _bankReservedBalance;
        private JArray _cargo = new JArray();
        private JArray _carrierLockerAssets = new JArray();
        private JArray _carrierLockerGoods = new JArray();
        private JArray _carrierLockerData = new JArray();
        private JArray _commoditySalesOrders = new JArray();
        private JArray _commodityPurchaseOrders = new JArray();
        private JArray _microresourceSalesOrders = new JArray();
        private JArray _microresourcePurchaseOrders = new JArray();

        [PublicAPI]
        public string name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                _name = value;
                OnPropertyChanged();
            }
        }

        [PublicAPI]
        public string callsign
        {
            get => _callsign;
            set
            {
                if (value == _callsign) return;
                _callsign = value;
                OnPropertyChanged();
            }
        }

        [PublicAPI]
        public string currentStarSystem
        {
            get => _currentStarSystem;
            set
            {
                if (value == _currentStarSystem) return;
                _currentStarSystem = value;
                OnPropertyChanged();
            }
        }

        [PublicAPI]
        public string nextStarSystem
        {
            get => _nextStarSystem;
            set
            {
                if (value == _nextStarSystem) return;
                _nextStarSystem = value;
                OnPropertyChanged();
            }
        }

        [PublicAPI]
        public int fuel // Tritium Fuel Reserves
        {
            get => _fuel;
            set
            {
                if (value == _fuel) return;
                _fuel = value;
                OnPropertyChanged();
            }
        }

        [PublicAPI]
        public int fuelInCargo // Tritium Fuel carried as cargo
        {
            get => _fuelInCargo;
            set
            {
                if (value == _fuelInCargo) return;
                _fuelInCargo = value;
                OnPropertyChanged();
            }
        }

        public string state // one of "normalOperation", "debtState" (if services are offline due to lack of funds), or "pendingDecomission" 
        {
            get => _state;
            set
            {
                if (value == _state) return;
                _state = value;
                OnPropertyChanged();
            }
        }

        public string dockingAccess // one of "all", "squadronfriends", "friends", or "none"
        {
            get => _dockingAccess;
            set
            {
                if (value == _dockingAccess) return;
                _dockingAccess = value;
                OnPropertyChanged();
            }
        }

        public bool notoriousAccess
        {
            get => _notoriousAccess;
            set
            {
                if (value == _notoriousAccess) return;
                _notoriousAccess = value;
                OnPropertyChanged();
            }
        }

        // Capacity

        [PublicAPI]
        public int usedCapacity
        {
            get => _usedCapacity;
            set
            {
                if (value == _usedCapacity) return;
                _usedCapacity = value;
                OnPropertyChanged();
            }
        }

        [PublicAPI]
        public int freeCapacity
        {
            get => _freeCapacity;
            set
            {
                if (value == _freeCapacity) return;
                _freeCapacity = value;
                OnPropertyChanged();
            }
        }

        // Finances

        [PublicAPI]
        public long bankBalance // "CarrierBalance" in CarrierStats
        {
            get => _bankBalance;
            set
            {
                if (value == _bankBalance) return;
                _bankBalance = value;
                OnPropertyChanged();
            }
        }

        [PublicAPI]
        public ulong bankReservedBalance // "ReserveBalance" in CarrierStats
        {
            get => _bankReservedBalance;
            set
            {
                if (value == _bankReservedBalance) return;
                _bankReservedBalance = value;
                OnPropertyChanged();
            }
        }

        // Inventories

        public JArray Cargo // Current cargo inventory
        {
            get => _cargo;
            set
            {
                if (Equals(value, _cargo)) return;
                _cargo = value;
                OnPropertyChanged();
            }
        }

        public JArray CarrierLockerAssets // Current MicroResource Inventory of Assets
        {
            get => _carrierLockerAssets;
            set
            {
                if (Equals(value, _carrierLockerAssets)) return;
                _carrierLockerAssets = value;
                OnPropertyChanged();
            }
        }

        public JArray CarrierLockerGoods // Current MicroResource Inventory of Goods
        {
            get => _carrierLockerGoods;
            set
            {
                if (Equals(value, _carrierLockerGoods)) return;
                _carrierLockerGoods = value;
                OnPropertyChanged();
            }
        }

        public JArray CarrierLockerData // Current MicroResource Inventory of Data
        {
            get => _carrierLockerData;
            set
            {
                if (Equals(value, _carrierLockerData)) return;
                _carrierLockerData = value;
                OnPropertyChanged();
            }
        }

        // Market Buy/Sell Orders

        public JArray commoditySalesOrders
        {
            get => _commoditySalesOrders;
            set
            {
                if (Equals(value, _commoditySalesOrders)) return;
                _commoditySalesOrders = value;
                OnPropertyChanged();
            }
        }

        public JArray commodityPurchaseOrders
        {
            get => _commodityPurchaseOrders;
            set
            {
                if (Equals(value, _commodityPurchaseOrders)) return;
                _commodityPurchaseOrders = value;
                OnPropertyChanged();
            }
        }

        public JArray microresourceSalesOrders
        {
            get => _microresourceSalesOrders;
            set
            {
                if (Equals(value, _microresourceSalesOrders)) return;
                _microresourceSalesOrders = value;
                OnPropertyChanged();
            }
        }

        public JArray microresourcePurchaseOrders
        {
            get => _microresourcePurchaseOrders;
            set
            {
                if (Equals(value, _microresourcePurchaseOrders)) return;
                _microresourcePurchaseOrders = value;
                OnPropertyChanged();
            }
        }

        // Station properties

        public FrontierApiProfileStation Market
        {
            get => _market;
            set
            {
                if (_market == value) return;
                _market = value;
                OnPropertyChanged();
            }
        }

        // Metadata

        public JObject json { get; set; } // The raw data from the endpoint as a JObject

        public DateTime timestamp { get; set; } // When the raw data was obtained

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

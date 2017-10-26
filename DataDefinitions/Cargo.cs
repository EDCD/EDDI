using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Cargo defines a number of commodities carried along with some additional data
    /// </summary>
    public class Cargo : INotifyPropertyChanged
    {
        public Commodity commodity; // The commodity

        [JsonIgnore]
        // The number of items
        private int _amount;
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
        // How much we actually paid for it (per unit)
        private long _price;
        public long price
        {
            get
            {
                return _price;
            }
            set
            {
                if (_price != value)
                {
                    _price = value;
                    NotifyPropertyChanged("price");
                }
            }
        }

        [JsonIgnore]
        // If the cargo is stolen
        private bool _stolen;
        public bool stolen
        {
            get
            {
                return _stolen;
            }
            set
            {
                if (_stolen != value)
                {
                    _stolen = value;
                    NotifyPropertyChanged("stolen");
                }
            }
        }

        [JsonIgnore]
        // The mission ID to which the cargo relates
        private long? _missionid;
        public long? missionid
        {
            get
            {
                return _missionid;
            }
            set
            {
                if (_missionid != value)
                {
                    _missionid = value;
                    NotifyPropertyChanged("missionid");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

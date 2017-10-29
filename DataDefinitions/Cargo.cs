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
        // The commodity name
        private string _commodity;
        public string commodity
        {
            get
            {
                return _commodity;
            }
            set
            {
                if (_commodity != value)
                {
                    _commodity = value;
                    NotifyPropertyChanged("commodity");
                }
            }
        }

        // The commodity catagory
        private string _category;
        public string category
        {
            get
            {
                return _category;
            }
            set
            {
                if (_category != value)
                {
                    _category = value;
                    NotifyPropertyChanged("category");
                }
            }
        }

        // The number of items
        private int _total;
        public int total
        {
            get
            {
                return _total;
            }
            set
            {
                if (_total != value)
                {
                    _total = value;
                    NotifyPropertyChanged("total");
                }
            }
        }

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

        // If the cargo is stolen
        private int _stolen;
        public int stolen
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

        // The mission ID to which the cargo relates
        private long? _haulage;
        public long? haulage
        {
            get
            {
                return _haulage;
            }
            set
            {
                if (_haulage != value)
                {
                    _haulage = value;
                    NotifyPropertyChanged("missionid");
                }
            }
        }

        public List<HaulageAmount> haulageamounts { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

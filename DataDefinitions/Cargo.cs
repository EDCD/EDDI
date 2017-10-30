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

        // The number of stolen items
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

        // The number of items related to a mission
        private int _haulage;
        public int haulage
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
                    NotifyPropertyChanged("haulage");
                }
            }
        }

        public List<HaulageAmount> haulageamounts { get; set; }

        public Cargo()
        {
            haulageamounts = new List<HaulageAmount>();
        }

        public Cargo(string commodity, string category, long price, int total, int stolen, int haulage, List<HaulageAmount> haulageamounts)
        {
            this.commodity = commodity;
            this.category = category;
            this.price = price;
            this.total = total;
            this.stolen = stolen;
            this.haulage = haulage;
            haulageamounts = new List<HaulageAmount>();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

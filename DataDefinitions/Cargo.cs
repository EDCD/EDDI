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
        private string _name;
        public string name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged("name");
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

        // The number of collected/purchased items
        private int _other;
        public int other
        {
            get
            {
                return _other;
            }
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

        // How much we actually paid for it (per unit)
        public int price { get; set; }

        // The commodity catagory
        public string category { get; set; }

        public Commodity commodity { get; set; }

        public List<HaulageAmount> haulageamounts { get; set; }

        public Cargo()
        {
            Commodity commodity = new Commodity();
            haulageamounts = new List<HaulageAmount>();
        }

        public Cargo(string name, int total, int? price = null)
        {
            Commodity commodity = CommodityDefinitions.FromName(name);
            this.name = name;
            this.commodity = commodity;
            this.category = commodity.category;
            this.price = (price != null ? price ?? 0 : commodity.avgprice ?? 0);
            this.total = total;
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

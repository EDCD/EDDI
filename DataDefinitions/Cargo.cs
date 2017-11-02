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
        private int _price;
        public int price
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

        // The number of items
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

        public int amount { get; set; }

        public Commodity commodity { get; set; }

        public List<HaulageAmount> haulageamounts { get; set; }

        public Cargo()
        {
            Commodity commodity = new Commodity();
            haulageamounts = new List<HaulageAmount>();
        }

        public Cargo(string name, int amount)
        {
            this.name = name;
            this.commodity = CommodityDefinitions.FromName(name);
            this.category = commodity.category;
            this.price = commodity.avgprice ?? 0;
            this.amount = amount;
            haulageamounts = new List<HaulageAmount>();
        }

        public Cargo(Commodity commodity, int price, int amount)
        {
            this.commodity = commodity;
            this.name = commodity.name;
            this.category = commodity.category;
            this.price = price;
            this.amount = amount;
            haulageamounts = new List<HaulageAmount>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            this.amount = this.stolen + this.haulage + this.other;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

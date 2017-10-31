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
        public Commodity commodity { get; set; }

        public List<HaulageAmount> haulageamounts { get; set; }

        public Cargo()
        {
            Commodity commodity = new Commodity();
            haulageamounts = new List<HaulageAmount>();
        }

        public Cargo(string name, string category, Commodity commodity, long price, int amount, int stolen, int haulage, List<HaulageAmount> haulageamounts)
        {
            this.name = name;
            this.commodity = commodity;
            this.category = category;
            this.price = price;
            this.amount = amount;
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

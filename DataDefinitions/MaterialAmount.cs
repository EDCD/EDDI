using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiDataDefinitions
{
    public class MaterialAmount : INotifyPropertyChanged
    {
        public string material { get; private set; }

        [JsonIgnore]
        private int _amount;
        public int amount { get
            {
                return this._amount;
            }
            set
            {
                if (this._amount != value)
                {
                    this._amount = value;
                    this.NotifyPropertyChanged("amount");
                }
            }
        }

        [JsonIgnore]
        private int? _minimum;
        public int? minimum
        {
            get
            {
                return this._minimum;
            }
            set
            {
                if (this._minimum != value)
                {
                    this._minimum = value;
                    this.NotifyPropertyChanged("minimum");
                }
            }
        }

        [JsonIgnore]
        private int? _desired;
        public int? desired
        {
            get
            {
                return this._desired;
            }
            set
            {
                if (this._desired != value)
                {
                    this._desired = value;
                    this.NotifyPropertyChanged("desired");
                }
            }
        }

        [JsonIgnore]
        private int? _maximum;
        public int? maximum
        {
            get
            {
                return this._maximum;
            }
            set
            {
                if (this._maximum != value)
                {
                    this._maximum = value;
                    this.NotifyPropertyChanged("maximum");
                }
            }
        }

        public MaterialAmount(Material material, int amount)
        {
            this.material = material.name;
            this.amount = amount;
        }

        public MaterialAmount(Material material, int? minimum, int? desired, int? maximum)
        {
            this.material = material.name;
            this.amount = 0;
            this.minimum = minimum;
            this.desired = desired;
            this.maximum = maximum;
        }

        [JsonConstructor]
        public MaterialAmount(string material, int amount, int? minimum, int? desired, int? maximum)
        {
            this.material = material;
            this.amount = amount;
            this.minimum = minimum;
            this.desired = desired;
            this.maximum = maximum;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
    }
}

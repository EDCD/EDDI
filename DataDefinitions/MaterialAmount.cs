using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiDataDefinitions
{
    public class MaterialAmount
    {
        public string material { get; private set; }

        public int amount { get; set; }

        public int? minimum { get; private set; }

        public int? desired { get; private set; }

        public int? maximum { get; private set; }

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
    }
}

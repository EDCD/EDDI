using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Cargo defines a number of commodities carried along with some additional data
    /// </summary>
    public class Cargo
    {
        public Commodity commodity { get; set; } // The commodity
        public int amount; // The number of items
        public long price { get; set; } // How much we actually paid for it (per unit)
    }
}

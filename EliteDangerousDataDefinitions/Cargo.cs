using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousDataDefinitions
{
    /// <summary>
    /// Cargo defines a number of commodities carried along with some additional data
    /// </summary>
    public class Cargo
    {
        public Commodity Commodity { get; set; } // The commodity
        public int Quantity; // The number of items
        public long Cost { get; set; } // How much we actually paid for it (per unit)
    }
}

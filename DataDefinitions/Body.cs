using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A star or planet
    /// </summary>
    public class Body
    {
        /// <summary>The ID of this body in EDDB</summary>
        public long EDDBID { get; set; }

        /// <summary>The name of the body</summary>
        public string name { get; set;  }


        /// <summary>
        /// Convert gravity in m/s to g
        /// </summary>
        public static decimal ms2g(decimal gravity)
        {
            return gravity / (decimal)9.8;
        }
    }
}

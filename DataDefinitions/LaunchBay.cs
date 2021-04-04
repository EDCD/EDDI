using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    ///<summary>A launchbay is an internal slot within a ship housing SRV or fighter vehicles</summary>
    public class LaunchBay
    {
        // The name of the launchbay
        public string name { get; set; }

        /// <summary>The size of the launchbay</summary>
        [PublicAPI]
        public int size { get; set; }

        // The type of the launchbay ("SRV" or "Fighter")
        [PublicAPI]
        public string type { get; set; }

        /// <summary>The vehicles residing in the launchbay (can be null)</summary>
        [PublicAPI]
        public List<Vehicle> vehicles { get; set; }

        public LaunchBay()
        {
            vehicles = new List<Vehicle>();
        }
    }
}

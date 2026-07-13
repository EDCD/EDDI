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
        [PublicAPI( "the numeric size of the launchbay" )]
        public int size { get; set; }

        // The type of the launchbay ("SRV" or "Vessel")
        [PublicAPI( "the type of launchbay, 'SRV' or 'Vessel'" )]
        public string type { get; set; }

        /// <summary>The vehicles residing in the launchbay (can be null)</summary>
        [PublicAPI( "the vehicles within the launchbay (this is an array of vehicle objects)" )]
        public List<Vehicle> vehicles { get; set; } = [ ];
    }
}

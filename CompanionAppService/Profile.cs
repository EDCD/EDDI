using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiCompanionAppService
{
    /// <summary>
    /// Profile information returned by the companion app service
    /// </summary>
    public class Profile
    {
        /// <summary>The commander</summary>
        public Commander Cmdr { get; set; }

        /// <summary>The commander's current ship</summary>
        public Ship Ship { get; set; }

        /// <summary>The commander's stored ships</summary>
        public List<Ship> Shipyard { get; set; }

        /// <summary>The current starsystem</summary>
        public StarSystem CurrentStarSystem{ get; set; }

        /// <summary>The last station the commander docked at</summary>
        public Station LastStation { get; set; }

        public Profile()
        {
            Shipyard = new List<Ship>();
        }
    }
}

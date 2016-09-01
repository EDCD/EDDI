using EliteDangerousDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousCompanionAppService
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
        public List<Ship> StoredShips { get; set; }

        /// <summary>The current starsystem</summary>
        public StarSystem CurrentStarSystem{ get; set; }

        /// <summary>The name of the last station the commander docked at</summary>
        public string LastStation { get; set; }

        /// <summary>The modules available at the station the commander last docked at</summary>
        public List<Module> Outfitting;

        public Profile()
        {
            StoredShips = new List<Ship>();
            Outfitting = new List<Module>();
        }
    }
}

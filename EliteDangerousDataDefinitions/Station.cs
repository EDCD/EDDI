using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousDataDefinitions
{
    /// <summary>
    /// A starport, outpost or port
    /// </summary>
    public class Station
    {
        /// <summary>The name</summary>
        public String Name { get; set;  }

        /// <summary>The allegiance</summary>
        public String Allegiance { get; set; }

        /// <summary>How far this is from the star</summary>
        public long distanceFromStar { get; set; }
        /// <summary>Does this station have rearm facilities?</summary>
        public bool? HasRearm { get; set; }
        /// <summary>Does this station have refuel facilities?</summary>
        public bool? HasRefuel { get; set; }
        /// <summary>Does this station have a market?</summary>
        public bool? HasMarket { get; set; }
        /// <summary>Does this station have a black market?</summary>
        public bool? HasBlackMarket { get; set; }
        /// <summary>Does this station have outfitting?</summary>
        public bool? HasOutfitting { get; set; }
        /// <summary>Does this station have a shipyard?</summary>
        public bool? HasShipyard { get; set; }
        /// <summary>Does this station have repair facilities?</summary>
        public bool? HasRepair { get; set; }

        /// <summary>The model of the station</summary>
        public StationModel Model { get; set; }

        /// <summary>What is the largest ship that can land here?</summary>
        public ShipSize LargestShip { get; set;  }

        /// <summary>Is this station planetary?</summary>
        public bool isPlanetary() { return Model == StationModel.PlanetaryOutpost || Model == StationModel.PlanetaryPort || Model == StationModel.UnknownPlanetary; }
    }

    public enum StationModel
    {
        CoriolisStarport,
        OcellusStarport,
        OrbisStarport,
        CivilianOutpost,
        CommercialOutpost,
        IndustrialOutpost,
        MilitaryOutpost,
        MiningOutpost,
        ScientificOutpost,
        UnsanctionedOutpost,
        PlanetaryOutpost,
        PlanetaryPort,
        UnknownStarport,
        UnknownOutpost,
        UnknownPlanetary
    }
}

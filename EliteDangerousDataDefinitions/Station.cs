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
        /// <summary>The ID of this station in EDDB</summary>
        public long EDDBID { get; set; }

        /// <summary>The name</summary>
        public String Name { get; set;  }

        /// <summary>The government</summary>
        public String Government { get; set; }

        /// <summary>The faction</summary>
        public String Faction { get; set; }

        /// <summary>The allegiance</summary>
        public String Allegiance { get; set; }

        /// <summary>The state of the system</summary>
        public String State { get; set; }

        /// <summary>The economies of the station</summary>
        public List<String> Economies { get; set; }

        /// <summary>How far this is from the star</summary>
        public long? DistanceFromStar { get; set; }

        /// <summary>Does this station have refuel facilities?</summary>
        public bool? HasRefuel { get; set; }
        /// <summary>Does this station have rearm facilities?</summary>
        public bool? HasRearm { get; set; }
        /// <summary>Does this station have repair facilities?</summary>
        public bool? HasRepair { get; set; }
        /// <summary>Does this station have outfitting?</summary>
        public bool? HasOutfitting { get; set; }
        /// <summary>Does this station have a shipyard?</summary>
        public bool? HasShipyard { get; set; }
        /// <summary>Does this station have a market?</summary>
        public bool? HasMarket { get; set; }
        /// <summary>Does this station have a black market?</summary>
        public bool? HasBlackMarket { get; set; }

        /// <summary>The model of the station</summary>
        public StationModel Model { get; set; }

        /// <summary>What is the largest ship that can land here?</summary>
        public ShipSize LargestShip { get; set;  }

        /// <summary>Is this station a starport?</summary>
        public bool IsStarport() { return Model == StationModel.CoriolisStarport || Model == StationModel.OcellusStarport || Model == StationModel.OrbisStarport || Model == StationModel.UnknownStarport; }

        /// <summary>Is this station an outpost?</summary>
        public bool IsOutpost() { return Model == StationModel.CivilianOutpost|| Model == StationModel.CommercialOutpost || Model == StationModel.IndustrialOutpost || Model == StationModel.MilitaryOutpost || Model == StationModel.MiningOutpost || Model == StationModel.ScientificOutpost || Model == StationModel.UnsanctionedOutpost || Model == StationModel.UnknownOutpost; }

        /// <summary>Is this station a planetary outpost?</summary>
        public bool IsPlanetaryOutpost() { return Model == StationModel.PlanetaryOutpost; }

        /// <summary>Is this station a planetary  port?</summary>
        public bool IsPlanetaryPort() { return Model == StationModel.PlanetaryPort; }

        /// <summary>Is this station planetary?</summary>
        public bool IsPlanetary() { return Model == StationModel.PlanetaryOutpost || Model == StationModel.PlanetaryPort || Model == StationModel.UnknownPlanetary; }
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
        PlanetarySettlement,
        UnknownStarport,
        UnknownOutpost,
        UnknownPlanetary
    }
}

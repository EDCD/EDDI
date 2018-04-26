using System;
using System.Collections.Generic;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A starport, outpost or port
    /// </summary>
    public class Station
    {
        /// <summary>The ID of this station in EDDB</summary>
        public long EDDBID { get; set; }

        /// <summary>The name</summary>
        public string name { get; set;  }

        /// <summary>The government</summary>
        public string government { get; set; }

        /// <summary>The faction</summary>
        public string faction { get; set; }

        /// <summary>The allegiance</summary>
        public string allegiance { get; set; }

        /// <summary>The state of the system</summary>
        public string state { get; set; }

        /// <summary>The primary economy of the station</summary>
        public string primaryeconomy { get; set; }

        /// <summary>How far this is from the star</summary>
        public long? distancefromstar { get; set; }

        /// <summary>The system in which this station resides</summary>
        public string systemname { get; set; }

        /// <summary>Does this station have refuel facilities?</summary>
        public bool? hasrefuel { get; set; }
        /// <summary>Does this station have rearm facilities?</summary>
        public bool? hasrearm { get; set; }
        /// <summary>Does this station have repair facilities?</summary>
        public bool? hasrepair { get; set; }
        /// <summary>Does this station have outfitting?</summary>
        public bool? hasoutfitting { get; set; }
        /// <summary>Does this station have a shipyard?</summary>
        public bool? hasshipyard { get; set; }
        /// <summary>Does this station have a market?</summary>
        public bool? hasmarket { get; set; }
        /// <summary>Does this station have a black market?</summary>
        public bool? hasblackmarket { get; set; }

        /// <summary>The model of the station</summary>
        public string model { get; set; }

        private string LargestPad;
        /// <summary>What is the largest ship that can land here?</summary>
        public string largestpad
        {
            get { return LargestPad; }
            set
            {
                // Map old values from when we had an enum
                if (value == "0") LargestPad = "None";
                else if (value == "1") LargestPad = "Small";
                else if (value == "2") LargestPad = "Medium";
                else if (value == "3") LargestPad = "Large";
                else LargestPad = value;
            }
        }

        /// <summary>What are the economies at the station</summary>
        public List<CompanionAppEconomy> economies { get; set; }

        /// <summary>Which commodities are bought/sold by the station</summary>
        public List<CommodityMarketQuote> commodities { get; set; }

        /// <summary>Which commodities are prohibited at the station</summary>
        public List<String> prohibited { get; set; }

        /// <summary>Which modules are available for outfitting at the station</summary>
        public List<Module> outfitting { get; set; }

        /// <summary>Which ships are available for purchase at the station</summary>
        public List<Ship> shipyard { get; set; }

        // Admin - the last time the information present changed
        public long? updatedat;

        // Admin - the last time the market information present changed
        public long? commoditiesupdatedat;

        // Admin - the last time the outfitting information present changed
        public long? outfittingupdatedat;

        /// <summary>Is this station a starport?</summary>
        public bool IsStarport() { return model == null ? false : model.EndsWith("Starport"); }

        /// <summary>Is this station an outpost?</summary>
        public bool IsOutpost() { return model == null ? false : model.EndsWith("Outpost"); }

        /// <summary>Is this station a planetary outpost?</summary>
        public bool IsPlanetaryOutpost() { return model == "Planetary Outpost"; }

        /// <summary>Is this station a planetary  port?</summary>
        public bool IsPlanetaryPort() { return model == "Planetary Port" || model == "Planetary Engineer Base"; }

        /// <summary>Is this station planetary?</summary>
        public bool IsPlanetary() { return model == null ? false : model.Contains("Planetary"); }
    }
}

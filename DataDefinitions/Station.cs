using Newtonsoft.Json;
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
        public long? EDDBID { get; set; }

        /// <summary>The name</summary>
        public string name { get; set; }

        /// <summary>The controlling faction</summary>
        public Faction Faction { get; set; }
        /// <summary>The controlling faction's name</summary>
        public string faction => Faction.name;

        /// <summary>The controlling faction's government</summary>
        public string government => Faction?.Government?.localizedName;

        /// <summary>The controlling faction's allegiance</summary>
        public string allegiance => Faction?.Allegiance?.localizedName;

        /// <summary>The controlling faction's state within the system</summary>
        public string state => State?.localizedName;
        public State State { get; set; }

        /// <summary>The primary economy of the station</summary>
        public string primaryeconomy => economies[0];

        /// <summary>How far this is from the star, in light seconds</summary>
        public long? distancefromstar { get; set; }

        /// <summary>The system in which this station resides</summary>
        public string systemname { get; set; }

        /// <summary>Unique 64 bit id value for system</summary>
        public long? systemAddress { get; set; }

        /// <summary>The ID of this station's system in EDDB</summary>
        public long? systemEDDBID { get; set; }

        /// <summary>The ID of this station's body in EDDB</summary>
        public long? bodyEDDBID { get; set; }

        /// <summary>Unique 64 bit id value for station</summary>
        public long? marketId { get; set; }

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
        /// <summary>Does this station allow docking?</summary>
        public bool? hasdocking { get; set; }

        /// <summary>The model of the station</summary>
        public StationModel Model { get; set; } = StationModel.None;
        public string model => Model?.localizedName;

        /// <summary>What is the largest ship that can land here?</summary>
        public StationLargestPad LargestPad { get; set; } = StationLargestPad.None;
        public string largestpad => LargestPad?.localizedName;

        /// <summary>What are the economies at the station, with proportions for each</summary>
        public List<EconomyShare> economiesShares { get; set; }

        /// <summary>What are the economies at the station, without proportions (economy share / proportion is not given from EDDB)</summary>
        public List<string> economies
        {
            get
            {
                if (economiesShares != null)
                {
                    List<string> economiesFromShares = new List<string>();
                    foreach (EconomyShare economyShare in economiesShares)
                    {
                        economiesFromShares.Add(economyShare.economy.localizedName);
                    }
                    return economiesFromShares;
                }
                else
                {
                    List<string> localizedEconomies = new List<string>();
                    foreach (string economy in _economies)
                    {
                        string localEconomyName = Economy.FromName(economy).localizedName;
                        if (localEconomyName != null)
                        {
                            localizedEconomies.Add(localEconomyName);
                        }
                    }
                    return localizedEconomies;
                };
            }
            set { _economies = value; }
        }
        private List<string> _economies;

        /// <summary>Which commodities are bought/sold by the station</summary>
        public List<CommodityMarketQuote> commodities { get; set; }

        /// <summary>Which commodities are imported by the station</summary>
        public List<String> imported { get; set; }

        /// <summary>Which commodities are exported by the station</summary>
        public List<String> exported { get; set; }

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

        // Admin - the last time the shipyard information present changed
        public long? shipyardupdatedat;

        /// <summary>Is this station a starport?</summary>
        public bool IsStarport() { return model == null ? false : Model?.basename == "Starport"; }

        /// <summary>Is this station an outpost?</summary>
        public bool IsOutpost() { return model == null ? false : Model?.basename == "Outpost"; }

        /// <summary>Is this station planetary?</summary>
        public bool IsPlanetary() { return Model == null ? false : Model?.basename == "SurfaceStation"; }

        /// <summary>Is this station an (undockable) settlement?</summary>
        public bool IsPlanetarySettlement() { return Model?.basename == "SurfaceStation" && largestpad == null; }
    }

    /// <summary> Station's largest landing pad size </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StationLargestPad : ResourceBasedLocalizedEDName<StationLargestPad>
    {
        static StationLargestPad()
        {
            resourceManager = Properties.StationLargestPad.ResourceManager;
            resourceManager.IgnoreCase = true;

            None = new StationLargestPad("None");
            var Large = new StationLargestPad("Large");
            var Medium = new StationLargestPad("Medium");
            var Small = new StationLargestPad("Small");
        }

        public static readonly StationLargestPad None;

        // dummy used to ensure that the static constructor has run
        public StationLargestPad() : this("")
        { }

        private StationLargestPad(string edname) : base(edname, edname)
        { }

        public static StationLargestPad FromSize (string value)
        {
            // Map old values from when we had an enum and map abbreviated sizes
            string size = string.Empty;
            if (value == "0" || value == null) { size = "None"; }
            value?.ToLowerInvariant();
            if (value == "1" || value == "s") { size = "Small"; }
            if (value == "2" || value == "m") { size = "Medium"; }
            if (value == "3" || value == "l") { size = "Large"; }

            return FromName(size);
        }
    }
}

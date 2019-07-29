using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A starport, outpost or port
    /// </summary>
    public class Station
    {
        /// <summary>The ID of this station in EDDB</summary>
        public long? EDDBID { get; set; }

        /// <summary>The ID of this station in EDSM</summary>
        public long? EDSMID { get; set; }

        /// <summary>The name</summary>
        public string name { get; set; }

        /// <summary>The controlling faction</summary>
        public Faction Faction { get; set; } = new Faction();

        /// <summary>The controlling faction's name</summary>
        [JsonIgnore, Obsolete("Please use Faction instead")]
        public string faction => (Faction ?? new Faction()).name;

        /// <summary>The controlling faction's government</summary>
        [JsonIgnore, Obsolete("Please use Faction.Government instead")]
        public string government => (Faction?.Government ?? Government.None).localizedName;

        /// <summary>The controlling faction's allegiance</summary>
        [JsonIgnore, Obsolete("Please use Faction.Allegiance instead")]
        public string allegiance => (Faction?.Allegiance ?? Superpower.None).localizedName;

        /// <summary>The controlling faction's state within the system</summary>
        [JsonIgnore, Obsolete("Please use Faction.factionState instead")]
        public string state => (Faction?.presences.FirstOrDefault(p => p.systemName == systemname)?.FactionState ?? FactionState.None).localizedName;

        /// <summary>The primary economy of the station</summary>
        [JsonIgnore]
        public string primaryeconomy => (economyShares?.Count > 0 && economyShares[0] != null ? economyShares[0].economy : Economy.None).localizedName;

        /// <summary>The secondary economy of the station</summary>
        [JsonIgnore]
        public string secondaryeconomy => (economyShares?.Count > 1 && economyShares[1] != null ? economyShares[1].economy : Economy.None).localizedName;

        /// <summary>How far this is from the star, in light seconds</summary>
        public decimal? distancefromstar { get; set; }

        /// <summary>The system in which this station resides</summary>
        public string systemname { get; set; }

        /// <summary>Unique 64 bit id value for system</summary>
        public long? systemAddress { get; set; }

        /// <summary>Unique 64 bit id value for station</summary>
        public long? marketId { get; set; }

        /// <summary>A list of the services offered by this station</summary>
        public List<StationService> stationServices { get; set; } = new List<StationService>();

        /// <summary>A localized list of the services offered by this station</summary>
        public List<string> stationservices
        {
            get
            {
                List<string> services = new List<string>();
                foreach (StationService service in stationServices)
                {
                    if (service != null) { services.Add(service.localizedName); }
                }
                return services;
            }
        }

        /// <summary>Does this station have refuel facilities?</summary>
        [JsonIgnore]
        public bool? hasrefuel {
            get { return stationServices.Exists(s => s?.edname == "Refuel"); }
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Refuel")); } }
        }
        /// <summary>Does this station have rearm facilities?</summary>
        [JsonIgnore]
        public bool? hasrearm
        {
            get { return stationServices.Exists(s => s?.edname == "Rearm"); }
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Rearm")); } }
        }
        /// <summary>Does this station have repair facilities?</summary>
        [JsonIgnore]
        public bool? hasrepair
        {
            get { return stationServices.Exists(s => s?.edname == "Repair"); }
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Repair")); } }
        }
        /// <summary>Does this station have outfitting?</summary>
        [JsonIgnore]
        public bool? hasoutfitting
        {
            get { return stationServices.Exists(s => s?.edname == "Outfitting"); }
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Outfitting")); } }
        }
        /// <summary>Does this station have a shipyard?</summary>
        [JsonIgnore]
        public bool? hasshipyard
        {
            get { return stationServices.Exists(s => s?.edname == "Shipyard"); }
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Shipyard")); } }
        }
        /// <summary>Does this station have a market?</summary>
        [JsonIgnore]
        public bool? hasmarket
        {
            get { return stationServices.Exists(s => s?.edname == "Commodities"); }
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Commodities")); } }
        }
        /// <summary>Does this station have a black market?</summary>
        [JsonIgnore]
        public bool? hasblackmarket
        {
            get { return stationServices.Exists(s => s?.edname == "BlackMarket"); }
            set { if (value is true) { stationServices.Add(StationService.FromEDName("BlackMarket")); } }
        }
        /// <summary>Does this station allow docking?</summary>
        [JsonIgnore]
        public bool? hasdocking
        {
            get { return stationServices.Exists(s => s?.edname == "Dock"); }
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Dock")); } }
        }

        /// <summary>The model of the station</summary>
        [JsonIgnore, Obsolete("Please use Model instead")]
        public string model => (Model ?? StationModel.None).localizedName;
        public StationModel Model { get; set; } = StationModel.None;

        /// <summary>What is the largest ship that can land here?</summary>
        [JsonIgnore, Obsolete("Please use StationLargestPad instead")]
        public string largestpad => LargestPad.localizedName;
        // This field isn't always provided, so we derive it from the station model when it's not explicitly set.
        public Size LargestPad
        {
            get
            {
                if (_LargestPad != null)
                {
                    return _LargestPad ?? Size.None;
                }
                if (Model.edname == "None")
                {
                    return Size.None;
                }
                if (Model.edname == "Outpost")
                {
                    return Size.Medium;
                }
                return Size.Large;
            }
            set
            {
                _LargestPad = value ?? Size.None;
            }
        }
        private Size _LargestPad;

        public bool LandingPadCheck(Size shipSize)
        {
            if (LargestPad == Size.Large) { return true; }
            else if (LargestPad == Size.Medium)
            {
                if (shipSize == Size.Large) { return false; } else { return true; }
            }
            if (shipSize == Size.Small) { return true; }
            return false;
        }

        /// <summary>What are the economies at the station, with proportions for each</summary>
        public List<EconomyShare> economyShares
        {
            // Per FDev comments, the BGS does have multiple minor variations of each economy type,
            // e.g. a system can have two distinct economies rendered as follows "StationEconomies":  
            // [ { "Name": "$economy_Refinery;", "Proportion": 0.84 }, { "Name": "$economy_Refinery;", "Proportion": 0.16 }
            // We consolidate them here.
            get { return _economyShares ?? new List<EconomyShare>(2); }
            set
            {
                List<EconomyShare> updatedEconomyShares = new List<EconomyShare>();
                foreach (EconomyShare newEconomyShare in value)
                {
                    EconomyShare alreadyAddedEconomyShare = updatedEconomyShares.FirstOrDefault(e => e.economy == newEconomyShare.economy);
                    if (alreadyAddedEconomyShare != null)
                    {
                        updatedEconomyShares.Remove(alreadyAddedEconomyShare);
                        updatedEconomyShares.Add(new EconomyShare(newEconomyShare.economy, alreadyAddedEconomyShare.proportion + newEconomyShare.proportion));
                    }
                    else if (newEconomyShare != null)
                    {
                        updatedEconomyShares.Add(newEconomyShare);
                    }
                }
                _economyShares = updatedEconomyShares;
            }
        }
        [JsonIgnore]
        private List<EconomyShare> _economyShares;

        /// <summary>What are the localized economies at the stations</summary>
        [JsonIgnore]
        public List<string> economies
        {
            get
            {
                List<string> localizedEconomiesFromShares = new List<string>(2);
                foreach (EconomyShare economyShare in economyShares)
                {
                    localizedEconomiesFromShares.Add(economyShare.economy.localizedName);
                }
                return localizedEconomiesFromShares;
            }
        }

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
        public bool IsStarport() { return Model == null ? false : Model?.basename == "Starport"; }

        /// <summary>Is this station an outpost?</summary>
        public bool IsOutpost() { return Model == null ? false : Model?.basename == "Outpost"; }

        /// <summary>Is this station planetary?</summary>
        public bool IsPlanetary() { return Model == null ? false : Model?.basename == "SurfaceStation"; }

        /// <summary>Is this station an (undockable) settlement?</summary>
        public bool IsPlanetarySettlement() { return Model?.basename == "SurfaceStation" && hasdocking != true; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
    }
}

﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A starport, outpost or port
    /// </summary>
    public class Station : INotifyPropertyChanged
    {
        /// <summary>The ID of this station in EDSM</summary>
        public long? EDSMID { get; set; }

        /// <summary>The name</summary>
        [PublicAPI]
        public string name { get; set; }

        /// <summary>The controlling faction</summary>
        public Faction Faction
        {
            get => _faction;
            set { _faction = value; OnPropertyChanged();}
        }
        private Faction _faction = new Faction();

        /// <summary>The controlling faction's name</summary>
        [PublicAPI, JsonIgnore, Obsolete("Please use Faction instead")]
        public string faction => (Faction ?? new Faction()).name;

        /// <summary>The controlling faction's government</summary>
        [PublicAPI, JsonIgnore, Obsolete("Please use Faction.Government instead")]
        public string government => (Faction?.Government ?? Government.None).localizedName;

        /// <summary>The controlling faction's allegiance</summary>
        [PublicAPI, JsonIgnore, Obsolete("Please use Faction.Allegiance instead")]
        public string allegiance => (Faction?.Allegiance ?? Superpower.None).localizedName;

        /// <summary>The controlling faction's state within the system</summary>
        [PublicAPI, JsonIgnore, Obsolete("Please use Faction.factionState instead")]
        public string state => (Faction?.presences.FirstOrDefault(p => p.systemName == systemname)?.FactionState ?? FactionState.None).localizedName;

        /// <summary>The primary economy of the station</summary>
        [PublicAPI, JsonIgnore]
        public string primaryeconomy => (economyShares?.Count > 0 && economyShares[0] != null ? economyShares[0].economy : Economy.None).localizedName;

        /// <summary>The secondary economy of the station</summary>
        [PublicAPI, JsonIgnore]
        public string secondaryeconomy => (economyShares?.Count > 1 && economyShares[1] != null ? economyShares[1].economy : Economy.None).localizedName;

        /// <summary>How far this is from the star, in light seconds</summary>
        [PublicAPI]
        public decimal? distancefromstar { get; set; }

        /// <summary>The system in which this station resides</summary>
        [PublicAPI]
        public string systemname { get; set; }

        /// <summary>Unique 64 bit id value for system</summary>
        public ulong? systemAddress { get; set; }

        /// <summary>Unique 64 bit id value for station</summary>
        public long? marketId { get; set; }

        /// <summary>A list of the services offered by this station</summary>
        public List<StationService> stationServices
        {
            get => _stationServices;
            set { _stationServices = value; OnPropertyChanged();}
        }
        private List<StationService> _stationServices = new List<StationService>();

        /// <summary>A localized list of the services offered by this station</summary>
        [PublicAPI]
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
        [PublicAPI, JsonIgnore]
        public bool? hasrefuel
        {
            get { return stationServices.Exists(s => s?.edname == "Refuel"); }
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Refuel")); OnPropertyChanged();} }
        }

        /// <summary>Does this station have rearm facilities?</summary>
        [PublicAPI, JsonIgnore]
        public bool? hasrearm
        {
            get { return stationServices.Exists(s => s?.edname == "Rearm"); }
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Rearm")); OnPropertyChanged();} }
        }

        /// <summary>Does this station have repair facilities?</summary>
        [PublicAPI, JsonIgnore]
        public bool? hasrepair
        {
            get { return stationServices.Exists(s => s?.edname == "Repair"); }
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Repair")); OnPropertyChanged();} }
        }

        /// <summary>Does this station have outfitting?</summary>
        [PublicAPI, JsonIgnore]
        public bool? hasoutfitting
        {
            get { return stationServices.Exists(s => s?.edname == "Outfitting"); }
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Outfitting")); OnPropertyChanged();} }
        }

        /// <summary>Does this station have a shipyard?</summary>
        [PublicAPI, JsonIgnore]
        public bool? hasshipyard
        {
            get { return stationServices.Exists(s => s?.edname == "Shipyard"); }
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Shipyard")); OnPropertyChanged();} }
        }

        /// <summary>Does this station have a market?</summary>
        [PublicAPI, JsonIgnore]
        public bool? hasmarket
        {
            get { return stationServices.Exists(s => s?.edname == "Commodities"); }
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Commodities")); OnPropertyChanged();} }
        }

        /// <summary>Does this station have a black market?</summary>
        [PublicAPI, JsonIgnore]
        public bool? hasblackmarket
        {
            get { return stationServices.Exists(s => s?.edname == "BlackMarket"); }
            set { if (value is true) { stationServices.Add(StationService.FromEDName("BlackMarket")); OnPropertyChanged();} }
        }

        /// <summary>Does this station allow docking?</summary>
        [JsonIgnore]
        public bool? hasdocking
        {
            get { return stationServices.Exists(s => s?.edname == "Dock"); }
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Dock")); OnPropertyChanged();} }
        }

        /// <summary>The model of the station</summary>
        [PublicAPI, JsonIgnore, Obsolete("Please use Model instead")]
        public string model => (Model ?? StationModel.None).localizedName;

        public StationModel Model { get; set; } = StationModel.None;

        /// <summary>What is the largest ship that can land here?</summary>
        [PublicAPI, JsonIgnore, Obsolete("Please use LargestPad instead")]
        public string largestpad => LargestPad.localizedName;
        
        // This field isn't always provided, so we derive it from the station model when it's not explicitly set.
        public LandingPadSize LargestPad
        {
            get
            {
                if (_LargestPad != null)
                {
                    return _LargestPad ?? LandingPadSize.None;
                }
                if (Model?.edname == "None")
                {
                    return LandingPadSize.None;
                }
                if (Model?.edname == "Outpost")
                {
                    return LandingPadSize.Medium;
                }
                return LandingPadSize.Large;
            }
            set
            {
                _LargestPad = value ?? LandingPadSize.None;
            }
        }
        private LandingPadSize _LargestPad;

        public bool LandingPadCheck(LandingPadSize shipSize)
        {
            if (LargestPad == LandingPadSize.Large) { return true; }
            else if (LargestPad == LandingPadSize.Medium)
            {
                if (shipSize == LandingPadSize.Large) { return false; } else { return true; }
            }
            if (shipSize == LandingPadSize.Small) { return true; }
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
                OnPropertyChanged();
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
        [PublicAPI]
        public List<CommodityMarketQuote> commodities
        {
            get => _commodities;
            set { _commodities = value; OnPropertyChanged();}
        }
        private List<CommodityMarketQuote> _commodities;

        /// <summary>Which commodities are imported by the station</summary>
        [PublicAPI, JsonIgnore]
        public List<CommodityMarketQuote> imports => commodities
            ?.Where(c => c.demandbracket > 0)
            .OrderByDescending(c => c.demand)
            .ToList();

        /// <summary>Which commodities are exported by the station</summary>
        [PublicAPI, JsonIgnore]
        public List<CommodityMarketQuote> exports => commodities
            ?.Where(c => c.stockbracket > 0)
            .OrderByDescending(c => c.stock)
            .ToList();

        /// <summary>Which commodities are prohibited at the station</summary>
        [PublicAPI]
        public List<CommodityDefinition> prohibited
        {
            get => _prohibited;
            set { _prohibited = value; OnPropertyChanged();}
        }
        private List<CommodityDefinition> _prohibited;

        /// <summary>Which modules are available for outfitting at the station</summary>
        [PublicAPI]
        public List<Module> outfitting
        {
            get => _outfitting;
            set { _outfitting = value; OnPropertyChanged();}
        }
        private List<Module> _outfitting;

        /// <summary>Which ships are available for purchase at the station</summary>
        public List<Ship> shipyard
        {
            get => _shipyard;
            set { _shipyard = value; OnPropertyChanged();}
        }
        private List<Ship> _shipyard;

        // Admin - the last time the information present changed
        [PublicAPI]
        public long? updatedat;

        /// <summary>the last time the market information present changed</summary>
        [PublicAPI]
        public long? commoditiesupdatedat;

        // Admin - the last time the outfitting information present changed
        public long? outfittingupdatedat;

        // Admin - the last time the shipyard information present changed
        public long? shipyardupdatedat;

        /// <summary>Is this station a fleet carrier?</summary>
        public bool IsCarrier() { return Model == StationModel.FleetCarrier; }

        /// <summary>Is this station a mega ship?</summary>
        public bool IsMegaShip() { return 
            Model == StationModel.Megaship || 
            Model == StationModel.MegaShipCivilian; }
        
        /// <summary>Is this station a starport?</summary>
        public bool IsStarport() { return 
            !IsPlanetary() && 
            !IsCarrier() && 
            !IsMegaShip() && 
            !IsOutpost() &&
            Model != StationModel.None;
        }

        /// <summary>Is this station an outpost?</summary>
        public bool IsOutpost() { return 
            !IsPlanetary() && 
            Model == StationModel.Outpost; }

        /// <summary>Is this station planetary?</summary>
        public bool IsPlanetary() { return 
            Model == StationModel.SurfaceStation || 
            Model == StationModel.CraterOutpost || 
            Model == StationModel.CraterPort || 
            Model == StationModel.OnFootSettlement; }

        /// <summary>Is this station an (undockable) settlement?</summary>
        public bool IsPlanetarySettlement() { return
            Model == StationModel.SurfaceStation && 
            hasdocking != true; }

        #region Implement INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion
    }
}

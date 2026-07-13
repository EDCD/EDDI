using Newtonsoft.Json;
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
        /// <summary>The name</summary>
        [ PublicAPI( "the name of the station" ) ]
        public string name
        {
            get => !string.IsNullOrEmpty( localizedName ) ? localizedName : _name;
            set => _name = value;
        }

        private string _name;

        /// <summary>The localized name, if any</summary>
        [PublicAPI( "the localized name of the station, if any" )]
        public string localizedName { get; set; }

        /// <summary>The controlling faction</summary>
        [PublicAPI( "the faction that controls this station, as an object" )]
        public Faction Faction
        {
            get => _faction;
            set { _faction = value; OnPropertyChanged();}
        }
        private Faction _faction = new();

        /// <summary>The controlling faction's name</summary>
        [PublicAPI( "the name  of the faction that controls this station" ), JsonIgnore, Obsolete("Please use Faction instead")]
        public string faction => Faction?.name;

        /// <summary>The controlling faction's government</summary>
        [PublicAPI( "the type of government in this station (Democracy, Confederacy etc)" ), JsonIgnore, Obsolete("Please use Faction.Government instead")]
        public string government => (Faction?.Government ?? Government.None).localizedName;

        /// <summary>The controlling faction's allegiance</summary>
        [PublicAPI( "the superpower allegiance of the faction that controls this station (Federation, Empire etc)" ), JsonIgnore, Obsolete("Please use Faction.Allegiance instead")]
        public string allegiance => (Faction?.Allegiance ?? Superpower.None).localizedName;

        /// <summary>The controlling faction's state within the system</summary>
        [PublicAPI( "the state of the station (Boom, War, etc)" ), JsonIgnore, Obsolete("Please use Faction.factionState instead")]
        public string state => (Faction?.presences.FirstOrDefault(p => p.systemAddress == systemAddress )?.FactionState ?? FactionState.None).localizedName;

        /// <summary>The primary economy of the station</summary>
        [PublicAPI( "the primary economy in this station (High Technology, Agriculture, etc)" ), JsonIgnore]
        public string primaryeconomy => (economyShares.Count > 0 ? economyShares[0].economy : Economy.None).localizedName;

        /// <summary>The secondary economy of the station</summary>
        [PublicAPI( "the secondary economy in this station (if any)" ), JsonIgnore]
        public string secondaryeconomy => (economyShares.Count > 1 ? economyShares[1].economy : Economy.None).localizedName;

        /// <summary>How far this is from the star, in light seconds</summary>
        [PublicAPI( "the distance from the main star to this station (in light years)" )]
        public decimal? distancefromstar { get; set; }

        /// <summary>The system in which this station resides</summary>
        [PublicAPI( "the name of the system in which this station resides" )]
        public string systemname { get; set; }

        /// <summary>Unique 64 bit id value for system</summary>
        [PublicAPI( "the unique numeric ID for the system in which this station resides" )]
        public ulong? systemAddress { get; set; }

        /// <summary>Unique 64 bit id value for station</summary>
        [PublicAPI( "the unique numeric ID for this station" )]
        public long? marketId { get; set; }

        public StationState stationState
        {
            get => _stationState ?? StationState.NormalOperation; 
            set => _stationState = value;
        }
        private StationState _stationState;

        /// <summary>A list of the services offered by this station</summary>
        [PublicAPI( "a list of the station services available at this station, as objects" )]
        public List<StationService> stationServices
        {
            get => _stationServices ?? [ ];
            set { _stationServices = value; OnPropertyChanged();}
        }
        private List<StationService> _stationServices;

        /// <summary>A localized list of the services offered by this station</summary>
        [PublicAPI( "a localized list of the station services available at this station" )]
        public List<string> stationservices
        {
            get
            {
                var services = new List<string>();
                foreach (var service in stationServices)
                {
                    if (service != null) { services.Add(service.localizedName); }
                }
                return services;
            }
        }

        /// <summary>Does this station have refuel facilities?</summary>
        [PublicAPI( "true if this station has refuelling" ), JsonIgnore]
        public bool? hasrefuel
        {
            get => stationServices.Exists(s => s?.edname == "Refuel");
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Refuel")); OnPropertyChanged();} }
        }

        /// <summary>Does this station have rearm facilities?</summary>
        [PublicAPI( "true if this station has rearming" ), JsonIgnore]
        public bool? hasrearm
        {
            get => stationServices.Exists(s => s?.edname == "Rearm");
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Rearm")); OnPropertyChanged();} }
        }

        /// <summary>Does this station have repair facilities?</summary>
        [PublicAPI( "true if this station has repairs" ), JsonIgnore]
        public bool? hasrepair
        {
            get => stationServices.Exists(s => s?.edname == "Repair");
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Repair")); OnPropertyChanged();} }
        }

        /// <summary>Does this station have outfitting?</summary>
        [PublicAPI( "true if this station has outfitting" ), JsonIgnore]
        public bool? hasoutfitting
        {
            get => stationServices.Exists(s => s?.edname == "Outfitting");
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Outfitting")); OnPropertyChanged();} }
        }

        /// <summary>Does this station have a shipyard?</summary>
        [PublicAPI( "true if this station has a shipyard" ), JsonIgnore]
        public bool? hasshipyard
        {
            get => stationServices.Exists(s => s?.edname == "Shipyard");
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Shipyard")); OnPropertyChanged();} }
        }

        /// <summary>Does this station have a market?</summary>
        [PublicAPI( "true if this station has a market" ), JsonIgnore]
        public bool? hasmarket
        {
            get => stationServices.Exists(s => s?.edname == "Commodities");
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Commodities")); OnPropertyChanged();} }
        }

        /// <summary>Does this station have a black market?</summary>
        [PublicAPI( "true if this station has a black market" ), JsonIgnore]
        public bool? hasblackmarket
        {
            get => stationServices.Exists(s => s?.edname == "BlackMarket");
            set { if (value is true) { stationServices.Add(StationService.FromEDName("BlackMarket")); OnPropertyChanged();} }
        }

        /// <summary>Does this station allow docking?</summary>
        [JsonIgnore]
        public bool? hasdocking
        {
            get => stationServices.Exists(s => s?.edname == "Dock");
            set { if (value is true) { stationServices.Add(StationService.FromEDName("Dock")); OnPropertyChanged();} }
        }

        /// <summary>The model of the station</summary>
        [PublicAPI( "the model of the station (Orbis, Coriolis, etc)" ), JsonIgnore, Obsolete("Please use Model instead")]
        public string model => (Model ?? StationModel.None).localizedName;

        [ PublicAPI( "the model of the station (Orbis, Coriolis, etc), as an object" ) ]
        public StationModel Model
        {
            get => _model ?? StationModel.None;
            set
            {
                if ( value == StationModel.FleetCarrier )
                {
                    // Fleet carriers always have the same landing pad configuration
                    landingPads = new StationLandingPads( 4, 4, 8 );
                }
                _model = value;
            }
        }
        private StationModel _model;

        /// <summary>What is the largest ship that can land here?</summary>
        [PublicAPI( "the localized largest pad available at this station (None, Small, Medium, Large)" ), JsonIgnore, Obsolete("Please use LargestPad instead")]
        public string largestpad => landingPads.LargestPad().localizedName;

        [PublicAPI( "the landing pads available at this station, as objects" ), JsonIgnore ]
        public StationLandingPads landingPads { get; set; } = new();

        /// <summary>What are the economies at the station, with proportions for each</summary>
        [JsonIgnore, JetBrains.Annotations.NotNull, JetBrains.Annotations.ItemNotNull]
        public List<EconomyShare> economyShares
        {
            get =>  _economyShares ??= new List<EconomyShare>( 2 ) ;
            set
            {
                if (value.Count != value.Select(v => v.economy).Distinct().Count())
                {
                    // Per FDev comments, the BGS does have multiple minor variations of each economy type,
                    // e.g. a system can have two distinct economies rendered as follows "StationEconomies":  
                    // [ { "Name": "$economy_Refinery;", "Proportion": 0.84 }, { "Name": "$economy_Refinery;", "Proportion": 0.16 }
                    // We consolidate them here.
                    _economyShares = value.Select(v => new KeyValuePair<Economy, decimal>(v.economy, v.proportion))
                        .GroupBy(x => x.Key) // Group second economies of the same economy type
                        .ToDictionary(x => x.Key, x => x.Sum(y => y.Value)) // Sum proportions of economies of the same economy type
                        .Select(d => new EconomyShare(d.Key, d.Value)).ToList(); // Convert back to a list of EconomyShares
                }
                else
                {
                    _economyShares = value;
                }
                OnPropertyChanged();
            }
        }
        [JsonProperty(nameof(economyShares))]
        private List<EconomyShare> _economyShares;

        /// <summary>What are the localized economies at the stations</summary>
        [JsonIgnore]
        public List<string> economies
        {
            get
            {
                var localizedEconomiesFromShares = new List<string>(2);
                foreach (var economyShare in economyShares)
                {
                    localizedEconomiesFromShares.Add(economyShare.economy.localizedName);
                }
                return localizedEconomiesFromShares;
            }
        }

        /// <summary>Which commodities are bought/sold by the station</summary>
        [PublicAPI( "the commodities that are bought and sold by this station (array of Commodity objects)" ), JetBrains.Annotations.NotNull, JetBrains.Annotations.ItemNotNull]
        public List<CommodityMarketQuote> commodities
        {
            get =>  _commodities ??= [ ] ;
            set { _commodities = value; OnPropertyChanged();}
        }
        private List<CommodityMarketQuote> _commodities;

        /// <summary>Which commodities are imported by the station</summary>
        [PublicAPI( "the commodities that are bought by this station (array of Commodity objects)" ), JsonIgnore]
        public List<CommodityMarketQuote> imports => commodities
            .Where(c => c.demandbracket > 0)
            .OrderByDescending(c => c.demand)
            .ToList();

        /// <summary>Which commodities are exported by the station</summary>
        [PublicAPI( "the commodities that are sold by this station (array of Commodity objects)" ), JsonIgnore]
        public List<CommodityMarketQuote> exports => commodities
            .Where(c => c.stockbracket > 0)
            .OrderByDescending(c => c.stock)
            .ToList();

        /// <summary>Which commodities are prohibited at the station</summary>
        [PublicAPI( "the commodities prohibited by the station (array of Commodity objects, requires Frontier API access)" ), JetBrains.Annotations.NotNull, JetBrains.Annotations.ItemNotNull]
        public List<CommodityDefinition> prohibited
        {
            get =>  _prohibited ??= [ ] ;
            set { _prohibited = value; OnPropertyChanged();}
        }
        private List<CommodityDefinition> _prohibited;

        /// <summary>Which modules are available for outfitting at the station</summary>
        [PublicAPI( "the modules that are available for outfitting at this station (array of Module objects)" ), JetBrains.Annotations.NotNull, JetBrains.Annotations.ItemNotNull]
        public List<Module> outfitting
        {
            get =>  _outfitting ??= [ ] ;
            set { _outfitting = value; OnPropertyChanged();}
        }

        private List<Module> _outfitting;

        /// <summary>Which ships are available for purchase at the station</summary>
        [PublicAPI( "the ships that are available for purchase at this station (array of Ship objects)"  ), JetBrains.Annotations.NotNull, JetBrains.Annotations.ItemNotNull]
        public List<Ship> shipyard
        {
            get =>  _shipyard ??= [ ] ;
            set { _shipyard = value; OnPropertyChanged();}
        }
        private List<Ship> _shipyard;

        // Admin - the last time the information present changed
        [PublicAPI( "the timestamp at which the station information was last updated" )]
        public long? updatedat;

        /// <summary>the last time the market information present changed</summary>
        [PublicAPI( "the timestamp at which the station commodities information was last updated" )]
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

        #region MarketInformationUpdated
        [JsonIgnore] public bool marketUpdatedThisVisit { get; set; }
        [JsonIgnore] public bool outfittingUpdatedThisVisit { get; set; }
        [JsonIgnore] public bool shipyardUpdatedThisVisit { get; set; }
        #endregion

        #region Implement INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }
        #endregion
    }
}

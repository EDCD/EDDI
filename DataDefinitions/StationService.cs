using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    /// <summary> Station's largest landing pad size </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StationService : ResourceBasedLocalizedEDName<StationService>
    {
        static StationService()
        {
            resourceManager = Properties.StationService.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new StationService(edname);

            None = new StationService("None");
            ApexInterstellar = new StationService("ApexInterstellar");
            AutoDock = new StationService("AutoDock");
            Bartender = new StationService("Bartender");
            BlackMarket = new StationService("BlackMarket");
            CarrierCaptain = new StationService("Captain");
            CarrierFuel = new StationService("CarrierFuel");
            CarrierManagement = new StationService("CarrierManagement");
            CarrierVendor = new StationService("CarrierVendor");
            Commodities = new StationService("Commodities");
            Contacts = new StationService("Contacts");
            CrewLounge = new StationService("CrewLounge");
            Dock = new StationService("Dock");
            Exploration = new StationService("Exploration");
            Facilitator = new StationService("Facilitator");
            FlightController = new StationService("FlightController");
            FrontlineSolutions = new StationService("FrontlineSolutions");
            Initiatives = new StationService("Initiatives");
            Livery = new StationService("Livery");
            MaterialTrader = new StationService("MaterialTrader");
            Missions = new StationService("Missions");
            MissionsGenerated = new StationService("MissionsGenerated");
            ModulePacks = new StationService("ModulePacks");
            Outfitting = new StationService("Outfitting");
            OnDockMission = new StationService("OnDockMission");
            PioneerSupplies = new StationService("PioneerSupplies");
            PowerPlay = new StationService("Powerplay");
            Rearm = new StationService("Rearm");
            Refuel = new StationService("Refuel");
            Repair = new StationService("Repair");
            Research = new StationService("Research");
            SearchAndRescue = new StationService("SearchAndRescue");
            Shipyard = new StationService("Shipyard");
            Shop = new StationService("Shop");
            SocialSpace = new StationService("SocialSpace");
            StationOperations = new StationService("StationOperations");
            StationMenu = new StationService("StationMenu");
            TechBroker = new StationService("TechBroker");
            Tuning = new StationService("Tuning");
            VistaGenomics = new StationService("VistaGenomics");
            VoucherRedemption = new StationService("VoucherRedemption");
            Workshop = new StationService("Workshop");
        }

        public static readonly StationService None;
        public static readonly StationService ApexInterstellar;
        public static readonly StationService AutoDock;
        public static readonly StationService Bartender;
        public static readonly StationService BlackMarket;
        public static readonly StationService CarrierCaptain;
        public static readonly StationService CarrierFuel;
        public static readonly StationService CarrierManagement;
        public static readonly StationService CarrierVendor;
        public static readonly StationService Commodities;
        public static readonly StationService Contacts;
        public static readonly StationService CrewLounge;
        public static readonly StationService Dock;
        public static readonly StationService Exploration;
        public static readonly StationService Facilitator;
        public static readonly StationService FlightController;
        public static readonly StationService FrontlineSolutions;
        public static readonly StationService Initiatives;
        public static readonly StationService Livery;
        public static readonly StationService MaterialTrader;
        public static readonly StationService Missions;
        public static readonly StationService MissionsGenerated;
        public static readonly StationService ModulePacks;
        public static readonly StationService Outfitting;
        public static readonly StationService OnDockMission;
        public static readonly StationService PioneerSupplies;
        public static readonly StationService PowerPlay;
        public static readonly StationService Rearm;
        public static readonly StationService Refuel;
        public static readonly StationService Repair;
        public static readonly StationService Research;
        public static readonly StationService SearchAndRescue;
        public static readonly StationService Shipyard;
        public static readonly StationService Shop;
        public static readonly StationService SocialSpace;
        public static readonly StationService StationOperations;
        public static readonly StationService StationMenu;
        public static readonly StationService TechBroker;
        public static readonly StationService Tuning;
        public static readonly StationService VistaGenomics;
        public static readonly StationService VoucherRedemption;
        public static readonly StationService Workshop;


        // dummy used to ensure that the static constructor has run
        public StationService() : this("")
        { }

        private StationService(string edname) : base(edname, edname)
        { }

        public new static StationService FromEDName(string edname)
        {
            // In Elite Dangerous v3.7, "Workshop" is replaced by "Engineer" and "SearchAndRescue" is replaced by "SearchRescue"
            // Preserve the original edname for backwards compatibility.
            return ResourceBasedLocalizedEDName<StationService>.FromEDName(
                edname.ToLowerInvariant().Replace("engineer", "workshop").Replace("searchrescue", "searchandrescue")
                );
        }
    }
}

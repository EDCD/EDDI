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
        }

        public static readonly StationService None = new StationService("None");
        public static readonly StationService ApexInterstellar = new StationService("ApexInterstellar");
        public static readonly StationService AutoDock = new StationService("AutoDock");
        public static readonly StationService Bartender = new StationService("Bartender");
        public static readonly StationService BlackMarket = new StationService("BlackMarket");
        public static readonly StationService CarrierCaptain = new StationService("Captain");
        public static readonly StationService CarrierFuel = new StationService("CarrierFuel");
        public static readonly StationService CarrierManagement = new StationService("CarrierManagement");
        public static readonly StationService CarrierVendor = new StationService("CarrierVendor");
        public static readonly StationService ColonisationContribution = new StationService( "ColonisationContribution" );
        public static readonly StationService Commodities = new StationService("Commodities");
        public static readonly StationService Contacts = new StationService("Contacts");
        public static readonly StationService CrewLounge = new StationService("CrewLounge");
        public static readonly StationService Dock = new StationService("Dock");
        public static readonly StationService Exploration = new StationService("Exploration");
        public static readonly StationService Facilitator = new StationService("Facilitator");
        public static readonly StationService FleetCarrierFuel = new StationService( "FleetCarrierFuel" );
        public static readonly StationService FleetCarrierManagement = new StationService( "FleetCarrierManagement" );
        public static readonly StationService FlightController = new StationService("FlightController");
        public static readonly StationService FrontlineSolutions = new StationService("FrontlineSolutions");
        public static readonly StationService Initiatives = new StationService("Initiatives");
        public static readonly StationService Livery = new StationService("Livery");
        public static readonly StationService Market = new StationService("Market");
        public static readonly StationService MaterialTrader = new StationService("MaterialTrader");
        public static readonly StationService Missions = new StationService("Missions");
        public static readonly StationService MissionsGenerated = new StationService("MissionsGenerated");
        public static readonly StationService ModulePacks = new StationService("ModulePacks");
        public static readonly StationService Outfitting = new StationService("Outfitting");
        public static readonly StationService OnDockMission = new StationService("OnDockMission");
        public static readonly StationService PioneerSupplies = new StationService("PioneerSupplies");
        public static readonly StationService PowerPlay = new StationService("Powerplay");
        public static readonly StationService Rearm = new StationService("Rearm");
        public static readonly StationService Refinery = new StationService( "Refinery" );
        public static readonly StationService Refuel = new StationService("Refuel");
        public static readonly StationService RegisteringColonisation = new StationService( "RegisteringColonisation" );
        public static readonly StationService Repair = new StationService("Repair");
        public static readonly StationService Research = new StationService("Research");
        public static readonly StationService SearchAndRescue = new StationService("SearchAndRescue");
        public static readonly StationService Shipyard = new StationService("Shipyard");
        public static readonly StationService Shop = new StationService("Shop");
        public static readonly StationService SocialSpace = new StationService("SocialSpace");
        public static readonly StationService SquadronBank = new StationService( "SquadronBank" );
        public static readonly StationService StationOperations = new StationService("StationOperations");
        public static readonly StationService StationMenu = new StationService("StationMenu");
        public static readonly StationService TechBroker = new StationService("TechBroker");
        public static readonly StationService Tuning = new StationService("Tuning");
        public static readonly StationService VistaGenomics = new StationService("VistaGenomics");
        public static readonly StationService VoucherRedemption = new StationService("VoucherRedemption");
        public static readonly StationService Workshop = new StationService("Workshop");

        // dummy used to ensure that the static constructor has run
        public StationService() : this("")
        { }

        private StationService(string edname) : base(edname, edname)
        { }

        public static new StationService FromEDName(string edname)
        {
            // In Elite Dangerous v3.7, "Workshop" is replaced by "Engineer" and "SearchAndRescue" is replaced by "SearchRescue"
            // Preserve the original edname for backwards compatibility.
            return ResourceBasedLocalizedEDName<StationService>.FromEDName(
                edname.ToLowerInvariant().Replace("engineer", "workshop").Replace("searchrescue", "searchandrescue")
                );
        }

        public static new StationService FromName ( string name )
        {
            // Spansh localized names can vary slightly from the expected invariant names, normalize any differences here
            return ResourceBasedLocalizedEDName<StationService>.FromName(name
                .Replace( "Autodock", AutoDock.invariantName )
                .Replace( "Fleet Carrier Fuel", CarrierFuel.invariantName )
            );
        }
    }
}

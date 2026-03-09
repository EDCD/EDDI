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

        public static readonly StationService None = new("None");
        public static readonly StationService ApexInterstellar = new("ApexInterstellar");
        public static readonly StationService AutoDock = new("AutoDock");
        public static readonly StationService Bartender = new("Bartender");
        public static readonly StationService BlackMarket = new("BlackMarket");
        public static readonly StationService CarrierCaptain = new("Captain");
        public static readonly StationService CarrierFuel = new("CarrierFuel");
        public static readonly StationService CarrierManagement = new("CarrierManagement");
        public static readonly StationService CarrierVendor = new("CarrierVendor");
        public static readonly StationService ColonisationContribution = new( "ColonisationContribution" );
        public static readonly StationService Commodities = new("Commodities");
        public static readonly StationService Contacts = new("Contacts");
        public static readonly StationService CrewLounge = new("CrewLounge");
        public static readonly StationService Dock = new("Dock");
        public static readonly StationService Exploration = new("Exploration");
        public static readonly StationService Facilitator = new("Facilitator");
        public static readonly StationService FleetCarrierFuel = new( "FleetCarrierFuel" );
        public static readonly StationService FleetCarrierManagement = new( "FleetCarrierManagement" );
        public static readonly StationService FlightController = new("FlightController");
        public static readonly StationService FrontlineSolutions = new("FrontlineSolutions");
        public static readonly StationService Initiatives = new("Initiatives");
        public static readonly StationService Livery = new("Livery");
        public static readonly StationService Market = new("Market");
        public static readonly StationService MaterialTrader = new("MaterialTrader");
        public static readonly StationService Missions = new("Missions");
        public static readonly StationService MissionsGenerated = new("MissionsGenerated");
        public static readonly StationService ModulePacks = new("ModulePacks");
        public static readonly StationService Outfitting = new("Outfitting");
        public static readonly StationService OnDockMission = new("OnDockMission");
        public static readonly StationService PioneerSupplies = new("PioneerSupplies");
        public static readonly StationService PowerPlay = new("Powerplay");
        public static readonly StationService Rearm = new("Rearm");
        public static readonly StationService Refinery = new( "Refinery" );
        public static readonly StationService Refuel = new("Refuel");
        public static readonly StationService RegisteringColonisation = new( "RegisteringColonisation" );
        public static readonly StationService Repair = new("Repair");
        public static readonly StationService Research = new("Research");
        public static readonly StationService SearchAndRescue = new("SearchAndRescue");
        public static readonly StationService Shipyard = new("Shipyard");
        public static readonly StationService Shop = new("Shop");
        public static readonly StationService SocialSpace = new("SocialSpace");
        public static readonly StationService SquadronBank = new( "SquadronBank" );
        public static readonly StationService StationOperations = new("StationOperations");
        public static readonly StationService StationMenu = new("StationMenu");
        public static readonly StationService TechBroker = new("TechBroker");
        public static readonly StationService Tuning = new("Tuning");
        public static readonly StationService VistaGenomics = new("VistaGenomics");
        public static readonly StationService VoucherRedemption = new("VoucherRedemption");
        public static readonly StationService Workshop = new("Workshop");

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

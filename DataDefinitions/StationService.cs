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
            var ApexInterstellar = new StationService("ApexInterstellar");
            var AutoDock = new StationService("AutoDock");
            var Bartender = new StationService("Bartender");
            var BlackMarket = new StationService("BlackMarket");
            var CarrierCaptain = new StationService("Captain");
            var CarrierFuel = new StationService("CarrierFuel");
            var CarrierManagement = new StationService("CarrierManagement");
            var CarrierVendor = new StationService("CarrierVendor");
            var Commodities = new StationService("Commodities");
            var Contacts = new StationService("Contacts");
            var CrewLounge = new StationService("CrewLounge");
            var Dock = new StationService("Dock");
            var Exploration = new StationService("Exploration");
            var Facilitator = new StationService("Facilitator");
            var FlightController = new StationService("FlightController");
            var FrontlineSolutions = new StationService("FrontlineSolutions");
            var Initiatives = new StationService("Initiatives");
            var Livery = new StationService("Livery");
            var MaterialTrader = new StationService("MaterialTrader");
            var Missions = new StationService("Missions");
            var MissionsGenerated = new StationService("MissionsGenerated");
            var ModulePacks = new StationService("ModulePacks");
            var Outfitting = new StationService("Outfitting");
            var OnDockMission = new StationService("OnDockMission");
            var PioneerSupplies = new StationService("PioneerSupplies");
            var PowerPlay = new StationService("Powerplay");
            var Rearm = new StationService("Rearm");
            var Refuel = new StationService("Refuel");
            var Repair = new StationService("Repair");
            var Research = new StationService("Research");
            var SearchAndRescue = new StationService("SearchAndRescue");
            var Shipyard = new StationService("Shipyard");
            var Shop = new StationService("Shop");
            var SocialSpace = new StationService("SocialSpace");
            var StationOperations = new StationService("StationOperations");
            var StationMenu = new StationService("StationMenu");
            var TechBroker = new StationService("TechBroker");
            var Tuning = new StationService("Tuning");
            var VistaGenomics = new StationService("VistaGenomics");
            var VoucherRedemption = new StationService("VoucherRedemption");
            var Workshop = new StationService("Workshop");
        }

        public static readonly StationService None;

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

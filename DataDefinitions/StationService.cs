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

            var Dock = new StationService("Dock");
            var AutoDock = new StationService("AutoDock");
            var BlackMarket = new StationService("BlackMarket");
            var Commodities = new StationService("Commodities");
            var Contacts = new StationService("Contacts");
            var Exploration = new StationService("Exploration");
            var Initiatives = new StationService("Initiatives");
            var Missions = new StationService("Missions");
            var Outfitting = new StationService("Outfitting");
            var CrewLounge = new StationService("CrewLounge");
            var Rearm = new StationService("Rearm");
            var Refuel = new StationService("Refuel");
            var Repair = new StationService("Repair");
            var Shipyard = new StationService("Shipyard");
            var Tuning = new StationService("Tuning");
            var Workshop = new StationService("Workshop");
            var MissionsGenerated = new StationService("MissionsGenerated");
            var Facilitator = new StationService("Facilitator");
            var Research = new StationService("Research");
            var FlightController = new StationService("FlightController");
            var StationOperations = new StationService("StationOperations");
            var OnDockMission = new StationService("OnDockMission");
            var PowerPlay = new StationService("Powerplay");
            var SearchAndRescue = new StationService("SearchAndRescue");
            var TechBroker = new StationService("TechBroker");
            var MaterialTrader = new StationService("MaterialTrader");
            var StationMenu = new StationService("StationMenu");
        }

        public static readonly StationService None = new StationService("None");

        // dummy used to ensure that the static constructor has run
        public StationService() : this("")
        { }

        private StationService(string edname) : base(edname, edname)
        { }
    }
}

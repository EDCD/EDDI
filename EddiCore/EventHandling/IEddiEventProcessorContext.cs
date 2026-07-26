using EddiCore.GameState;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace EddiCore.EventHandling
{
    internal interface IEddiEventProcessorContext
    {
        IEddiGameState GameState { get; }
        DataProviderService DataProvider { get; }
        EddiEventPipeline EventPipeline { get; }

        StarSystem CurrentStarSystem { get; set; }
        StarSystem LastStarSystem { get; set; }
        StarSystem NextStarSystem { get; set; }
        StarSystem DestinationStarSystem { get; set; }
        Station CurrentStation { get; set; }
        Body CurrentStellarBody { get; set; }
        FleetCarrier FleetCarrier { get; set; }
        FleetCarrier SquadronCarrier { get; set; }
        string Environment { get; set; }
        string Vehicle { get; set; }
        bool inTelepresence { get; set; }
        bool inHorizons { get; set; }
        bool inOdyssey { get; set; }
        bool gameIsBeta { get; set; }
        ConcurrentDictionary<string, Event> lastEventOfType { get; }

        IEddiMonitor ObtainMonitor ( string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase );
        Task conditionallyRefreshStationProfileAsync ( string expectedSystemName, long expectedLastMarketID, bool forceUpdate = false, JObject profileJson = null );
        Task updateDestinationSystemAsync ( ulong? destinationSystemAddress, string destinationSystem = null );
        void SetGameVersionDetails ( string version, string build );
    }
}

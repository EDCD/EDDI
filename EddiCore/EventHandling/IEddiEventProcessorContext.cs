using EddiCore.GameState;
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
        IEddiGameStateMutator GameStateMutator { get; }
        DataProviderService DataProvider { get; }
        EddiEventPipeline EventPipeline { get; }
        OrganicSamplingTracker OrganicSamplingTracker { get; }
        ConcurrentDictionary<string, Event> lastEventOfType { get; }

        IEddiMonitor ObtainMonitor ( string invariantName, StringComparison stringComparison = StringComparison.InvariantCultureIgnoreCase );
        Task conditionallyRefreshStationProfileAsync ( string expectedSystemName, long expectedLastMarketID, bool forceUpdate = false, JObject profileJson = null );
        Task updateDestinationSystemAsync ( ulong? destinationSystemAddress, string destinationSystem = null );
    }
}

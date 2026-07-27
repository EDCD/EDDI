using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiCore.GameState;
using EddiDataDefinitions;
using EddiDataProviderService;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiNavigationService
{
    internal interface INavigationRuntimeContext
    {
        IEddiGameState GameState { get; }
        DataProviderService DataProvider { get; }
        NavigationMonitorConfiguration NavigationConfiguration { get; set; }
        MissionMonitorConfiguration MissionConfiguration { get; }
        void UpdateSearchSystem ( StarSystem system, decimal distanceLy );
        void UpdateSearchStation ( Station station );
        Task UpdateDestinationSystemAsync ( ulong? destinationSystemAddress, string destinationSystem = null );
        void UpdateDestinationDistance ( decimal distanceLy );
    }

    internal sealed class EddiNavigationRuntimeContext : INavigationRuntimeContext
    {
        public IEddiGameState GameState => EDDI.Instance.GameState;
        public DataProviderService DataProvider => EDDI.Instance.DataProvider;
        public NavigationMonitorConfiguration NavigationConfiguration
        {
            get => ConfigService.Instance.navigationMonitorConfiguration;
            set => ConfigService.Instance.navigationMonitorConfiguration = value;
        }
        public MissionMonitorConfiguration MissionConfiguration => ConfigService.Instance.missionMonitorConfiguration;
        public void UpdateSearchSystem ( StarSystem system, decimal distanceLy ) => EDDI.Instance.UpdateSearchSystem( system, distanceLy );
        public void UpdateSearchStation ( Station station ) => EDDI.Instance.UpdateSearchStation( station );
        public Task UpdateDestinationSystemAsync ( ulong? destinationSystemAddress, string destinationSystem = null ) =>
            EDDI.Instance.updateDestinationSystemAsync( destinationSystemAddress, destinationSystem );
        public void UpdateDestinationDistance ( decimal distanceLy ) => EDDI.Instance.UpdateDestinationDistance( distanceLy );
    }
}

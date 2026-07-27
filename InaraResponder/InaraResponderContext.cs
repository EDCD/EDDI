using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiCore.GameState;
using EddiDataDefinitions;
using EddiSpeechService;
using System.Threading.Tasks;

namespace EddiInaraResponder
{
    internal interface IInaraResponderContext
    {
        IEddiGameState GameState { get; }
        bool EddiIsBeta { get; }
        InaraConfiguration InaraConfiguration { get; set; }
        ShipMonitorConfiguration ShipMonitorConfiguration { get; }
        Task SayAsync ( Ship ship, string message );
    }

    internal sealed class EddiInaraResponderContext : IInaraResponderContext
    {
        public IEddiGameState GameState => EDDI.Instance.GameState;
        public bool EddiIsBeta => EDDI.EddiIsBeta();

        public InaraConfiguration InaraConfiguration
        {
            get => ConfigService.Instance.inaraConfiguration;
            set => ConfigService.Instance.inaraConfiguration = value;
        }

        public ShipMonitorConfiguration ShipMonitorConfiguration => ConfigService.Instance.shipMonitorConfiguration;

        public Task SayAsync ( Ship ship, string message )
        {
            return SpeechService.Instance.SayAsync( ship, message );
        }
    }
}

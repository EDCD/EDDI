using EddiConfigService.Configurations;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiInaraService
{
    public interface IInaraService
    {
        // Background Sync
        void Start(bool eddiIsBeta = false);
        void Stop();

        // API Event Queue Management
        void EnqueueAPIEvent(InaraAPIEvent inaraAPIEvent);
        Task<List<InaraResponse>> SendEventBatchAsync(List<InaraAPIEvent> events, InaraConfiguration inaraConfiguration = null);

        // Commander Profiles
        Task<InaraCmdr> GetCommanderProfileAsync(string cmdrName = null);
        Task<List<InaraCmdr>> GetCommanderProfilesAsync(IList<string> cmdrNames);

        // API Credentials
        bool checkAPIcredentialsOk ( InaraConfiguration inaraConfiguration );
    }
}

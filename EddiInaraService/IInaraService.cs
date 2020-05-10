using System.Collections.Generic;


namespace EddiInaraService
{
    public interface IInaraService
    {
        // Background Sync
        void Start(bool eddiIsBeta = false);
        void Stop();

        // API Event Queue Management
        void EnqueueAPIEvent(InaraAPIEvent inaraAPIEvent);
        List<InaraResponse> SendEventBatch(List<InaraAPIEvent> events, InaraConfiguration inaraConfiguration = null, bool eddiIsBeta = false);

        // Commander Profiles
        InaraCmdr GetCommanderProfile(string cmdrName);
        List<InaraCmdr> GetCommanderProfiles(IEnumerable<string> cmdrNames);
    }
}

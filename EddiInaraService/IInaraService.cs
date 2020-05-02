using System;
using System.Collections.Generic;


namespace EddiInaraService
{
    public interface IInaraService
    {
        // Variables
        DateTime lastSync { get; }

        // Background Sync
        void Start(bool eddiIsBeta = false);
        void Stop();

        // API Event Queue Management
        void EnqueueAPIEvent(InaraAPIEvent inaraAPIEvent);
        List<InaraResponse> SendEventBatch(ref List<InaraAPIEvent> events, bool sendEvenForBetaGame = false);

        // Commander Profiles
        InaraCmdr GetCommanderProfile(string cmdrName);
        List<InaraCmdr> GetCommanderProfiles(string[] cmdrNames);
    }
}

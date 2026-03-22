using EddiEddnResponder.Toolkit;
using System.Collections.Generic;

namespace EddiEddnResponder
{
    public class EDDNState
    {
        public readonly GameVersionAugmenter GameVersion = new();

        public readonly LocationAugmenter Location = new();

        public void GetStateInfo ( string edType, IDictionary<string, object> data )
        {
            // Attempt to obtain available game version data from the active event 
            GameVersion.GetVersionInfo( edType, data );

            // Attempt to obtain available location data from the active event 
            Location.GetLocationInfo( edType, data );
        }
    }
}
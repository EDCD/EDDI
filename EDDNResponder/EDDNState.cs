using EddiEddnResponder.Toolkit;
using System.Collections.Generic;

namespace EddiEddnResponder
{
    public class EDDNState
    {
        public readonly GameVersionAugmenter GameVersion;

        public readonly LocationAugmenter Location;

        public EDDNState()
        {
            GameVersion = new GameVersionAugmenter();
            Location = new LocationAugmenter();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="eddnState"></param>
        public EDDNState ( EDDNState eddnState )
        {
            GameVersion = eddnState.GameVersion;
            Location = eddnState.Location;
        }

        public void GetStateInfo ( string edType, IDictionary<string, object> data )
        {
            // Attempt to obtain available game version data from the active event 
            GameVersion.GetVersionInfo( edType, data );

            // Attempt to obtain available location data from the active event 
            Location.GetLocationInfo( edType, data );
        }
    }
}
using EddiEddnResponder.Toolkit;
using System.Collections.Generic;
using Utilities;

namespace EddiEddnResponder
{
    public class EDDNState
    {
        public readonly GameVersionAugmenter GameVersion = new();

        public readonly LocationAugmenter Location = new();

        // An absent mode preserves support for journals predating GameModeChange.
        internal string GameMode { get; private set; }
        internal bool InSpecialGameInstance => GameMode is "Operation" or "ProvingGrounds";

        internal bool UpdateGameMode ( string edType, IDictionary<string, object> data )
        {
            if ( edType != "GameModeChange" ) { return false; }

            var mode = JsonParsing.getString( data, "GameMode" );
            if ( mode is not ( "MainGame" or "Operation" or "ProvingGrounds" ) )
            {
                Logging.Warn( $"Ignoring unknown EDDN game mode '{mode}'." );
                return false;
            }
            if ( mode == GameMode ) { return false; }

            GameMode = mode;
            Location.Reset();
            return true;
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

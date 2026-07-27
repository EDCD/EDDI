using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiCore.GameState
{
    internal sealed class EddiGameStateService (
        EddiGameState gameState,
        Func<(decimal? x, decimal? y, decimal? z)> getHomeSystemCoordinates,
        Action<Ship> setCurrentShip,
        Action<string> sayLegacyGameVersionWarning,
        Action<System.Version, string, string> setStarMapGameVersion,
        System.Version minGameVersion ) : IEddiGameStateMutator
    {
        private string _gameVersionRaw;
        private string _gameBuild;

        public bool inTelepresence { get => gameState.inTelepresence; set => gameState.inTelepresence = value; }
        public bool inHorizons { get => gameState.inHorizons; set => gameState.inHorizons = value; }
        public bool inOdyssey { get => gameState.inOdyssey; set => gameState.inOdyssey = value; }
        public bool gameIsBeta { get => gameState.gameIsBeta; set => gameState.gameIsBeta = value; }
        public System.Version GameVersion { get => gameState.GameVersion; set => gameState.GameVersion = value; }

        public string GameVersionRaw
        {
            get => _gameVersionRaw;
            set
            {
                _gameVersionRaw = value;
                gameState.GameVersionRaw = value;
                SetGameVersion( value );
            }
        }

        public StarSystem DestinationStarSystem
        {
            get => gameState.DestinationStarSystem;
            set
            {
                gameState.DestinationStarSystem = value;
                SetSystemDistanceFromDestination( gameState.CurrentStarSystem );
            }
        }

        public decimal DestinationDistanceLy { get => gameState.DestinationDistanceLy; set => gameState.DestinationDistanceLy = value; }
        public string Environment { get => gameState.Environment; set => gameState.Environment = value; }

        public StarSystem CurrentStarSystem
        {
            get => gameState.CurrentStarSystem;
            set
            {
                SetSystemDistanceFromHome( value );
                SetSystemDistanceFromDestination( value );
                gameState.CurrentStarSystem = value;
            }
        }

        public StarSystem LastStarSystem
        {
            get => gameState.LastStarSystem;
            set
            {
                SetSystemDistanceFromHome( value );
                gameState.LastStarSystem = value;
            }
        }

        public StarSystem NextStarSystem
        {
            get => gameState.NextStarSystem;
            set
            {
                SetSystemDistanceFromHome( value );
                gameState.NextStarSystem = value;
            }
        }

        public Station CurrentStation { get => gameState.CurrentStation; set => gameState.CurrentStation = value; }
        public Body CurrentStellarBody { get => gameState.CurrentStellarBody; set => gameState.CurrentStellarBody = value; }
        public FleetCarrier FleetCarrier { get => gameState.FleetCarrier; set => gameState.FleetCarrier = value; }
        public FleetCarrier SquadronCarrier { get => gameState.SquadronCarrier; set => gameState.SquadronCarrier = value; }

        public Ship CurrentShip
        {
            get => gameState.CurrentShip;
            set
            {
                if ( Equals( value, gameState.CurrentShip ) ) { return; }
                setCurrentShip?.Invoke( value );
                gameState.CurrentShip = value;
            }
        }

        public string Vehicle { get => gameState.Vehicle; set => gameState.Vehicle = value; }
        public StarSystem SearchStarSystem { get => gameState.SearchStarSystem; set => gameState.SearchStarSystem = value; }
        public Station SearchStation { get => gameState.SearchStation; set => gameState.SearchStation = value; }
        public decimal SearchDistanceLy { get => gameState.SearchDistanceLy; set => gameState.SearchDistanceLy = value; }

        public void SetGameVersionDetails ( string version, string build )
        {
            _gameBuild = build;
            GameVersionRaw = version;
        }

        private void SetGameVersion ( string value )
        {
            try
            {
                if ( string.IsNullOrEmpty( value ) ) { return; }

                GameVersion = System.Version.TryParse( GeneratedRegex.SemanticVersionRegex().Match( value ).Value, out var versionResult )
                    ? versionResult
                    : null;

                if ( GameVersion != null && GameVersion < minGameVersion )
                {
                    const string msg = "Legacy game version detected. EDDI shall resume processing events after you return to the live galaxy.";
                    Logging.Warn( msg );
                    sayLegacyGameVersionWarning?.Invoke( msg );
                }

                setStarMapGameVersion?.Invoke( GameVersion, _gameVersionRaw, _gameBuild );
            }
            catch ( Exception e )
            {
                Logging.Error( "Failed to set game version", e );
            }
        }

        private void SetSystemDistanceFromHome ( StarSystem system )
        {
            var (homeX, homeY, homeZ) = getHomeSystemCoordinates();

            if ( system is null || homeX is null || homeY is null || homeZ is null ) { return; }

            system.distancefromhome = system.DistanceFromStarSystem( homeX, homeY, homeZ ) ?? 0;
            Logging.Debug( "Distance from home is " + system.distancefromhome );
        }

        private void SetSystemDistanceFromDestination ( StarSystem system )
        {
            if ( gameState.DestinationStarSystem is null || system is null )
            {
                DestinationDistanceLy = 0;
                return;
            }

            DestinationDistanceLy = system.DistanceFromStarSystem( gameState.DestinationStarSystem ) ?? 0;
            Logging.Debug( "Distance from destination system is " + DestinationDistanceLy );
        }
    }
}

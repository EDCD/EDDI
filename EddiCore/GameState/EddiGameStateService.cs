using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiCore.GameState
{
    internal sealed class EddiGameStateService : IEddiGameStateMutator
    {
        private readonly EddiGameState _gameState;
        private readonly Func<(decimal? x, decimal? y, decimal? z)> _getHomeSystemCoordinates;
        private readonly Action<Ship> _setCurrentShip;
        private readonly Action<string> _sayLegacyGameVersionWarning;
        private readonly Action<System.Version, string, string> _setStarMapGameVersion;
        private readonly System.Version _minGameVersion;
        private string _gameVersionRaw;
        private string _gameBuild;

        public EddiGameStateService (
            EddiGameState gameState,
            Func<(decimal? x, decimal? y, decimal? z)> getHomeSystemCoordinates,
            Action<Ship> setCurrentShip,
            Action<string> sayLegacyGameVersionWarning,
            Action<System.Version, string, string> setStarMapGameVersion,
            System.Version minGameVersion )
        {
            _gameState = gameState;
            _getHomeSystemCoordinates = getHomeSystemCoordinates;
            _setCurrentShip = setCurrentShip;
            _sayLegacyGameVersionWarning = sayLegacyGameVersionWarning;
            _setStarMapGameVersion = setStarMapGameVersion;
            _minGameVersion = minGameVersion;
        }

        public bool inTelepresence { get => _gameState.inTelepresence; set => _gameState.inTelepresence = value; }
        public bool inHorizons { get => _gameState.inHorizons; set => _gameState.inHorizons = value; }
        public bool inOdyssey { get => _gameState.inOdyssey; set => _gameState.inOdyssey = value; }
        public bool gameIsBeta { get => _gameState.gameIsBeta; set => _gameState.gameIsBeta = value; }
        public System.Version GameVersion { get => _gameState.GameVersion; set => _gameState.GameVersion = value; }

        public string GameVersionRaw
        {
            get => _gameVersionRaw;
            set
            {
                _gameVersionRaw = value;
                _gameState.GameVersionRaw = value;
                SetGameVersion( value );
            }
        }

        public StarSystem DestinationStarSystem
        {
            get => _gameState.DestinationStarSystem;
            set
            {
                _gameState.DestinationStarSystem = value;
                SetSystemDistanceFromDestination( _gameState.CurrentStarSystem );
            }
        }

        public decimal DestinationDistanceLy { get => _gameState.DestinationDistanceLy; set => _gameState.DestinationDistanceLy = value; }
        public string Environment { get => _gameState.Environment; set => _gameState.Environment = value; }

        public StarSystem CurrentStarSystem
        {
            get => _gameState.CurrentStarSystem;
            set
            {
                SetSystemDistanceFromHome( value );
                SetSystemDistanceFromDestination( value );
                _gameState.CurrentStarSystem = value;
            }
        }

        public StarSystem LastStarSystem
        {
            get => _gameState.LastStarSystem;
            set
            {
                SetSystemDistanceFromHome( value );
                _gameState.LastStarSystem = value;
            }
        }

        public StarSystem NextStarSystem
        {
            get => _gameState.NextStarSystem;
            set
            {
                SetSystemDistanceFromHome( value );
                _gameState.NextStarSystem = value;
            }
        }

        public Station CurrentStation { get => _gameState.CurrentStation; set => _gameState.CurrentStation = value; }
        public Body CurrentStellarBody { get => _gameState.CurrentStellarBody; set => _gameState.CurrentStellarBody = value; }
        public FleetCarrier FleetCarrier { get => _gameState.FleetCarrier; set => _gameState.FleetCarrier = value; }
        public FleetCarrier SquadronCarrier { get => _gameState.SquadronCarrier; set => _gameState.SquadronCarrier = value; }

        public Ship CurrentShip
        {
            get => _gameState.CurrentShip;
            set
            {
                if ( Equals( value, _gameState.CurrentShip ) ) { return; }
                _setCurrentShip?.Invoke( value );
                _gameState.CurrentShip = value;
            }
        }

        public string Vehicle { get => _gameState.Vehicle; set => _gameState.Vehicle = value; }
        public StarSystem SearchStarSystem { get => _gameState.SearchStarSystem; set => _gameState.SearchStarSystem = value; }
        public Station SearchStation { get => _gameState.SearchStation; set => _gameState.SearchStation = value; }
        public decimal SearchDistanceLy { get => _gameState.SearchDistanceLy; set => _gameState.SearchDistanceLy = value; }

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

                if ( GameVersion != null && GameVersion < _minGameVersion )
                {
                    const string msg = "Legacy game version detected. EDDI shall resume processing events after you return to the live galaxy.";
                    Logging.Warn( msg );
                    _sayLegacyGameVersionWarning?.Invoke( msg );
                }

                _setStarMapGameVersion?.Invoke( GameVersion, _gameVersionRaw, _gameBuild );
            }
            catch ( Exception e )
            {
                Logging.Error( "Failed to set game version", e );
            }
        }

        private void SetSystemDistanceFromHome ( StarSystem system )
        {
            var (homeX, homeY, homeZ) = _getHomeSystemCoordinates();

            if ( system is null || homeX is null || homeY is null || homeZ is null ) { return; }

            system.distancefromhome = system.DistanceFromStarSystem( homeX, homeY, homeZ ) ?? 0;
            Logging.Debug( "Distance from home is " + system.distancefromhome );
        }

        private void SetSystemDistanceFromDestination ( StarSystem system )
        {
            if ( _gameState.DestinationStarSystem is null || system is null )
            {
                DestinationDistanceLy = 0;
                return;
            }

            DestinationDistanceLy = system.DistanceFromStarSystem( _gameState.DestinationStarSystem ) ?? 0;
            Logging.Debug( "Distance from destination system is " + DestinationDistanceLy );
        }
    }
}

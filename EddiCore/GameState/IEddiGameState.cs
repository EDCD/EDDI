using EddiDataDefinitions;
using System.Collections.Generic;
using System.ComponentModel;

namespace EddiCore.GameState
{
    public interface IEddiGameState : INotifyPropertyChanged
    {
        bool inTelepresence { get; }
        bool inHorizons { get; }
        bool inOdyssey { get; }
        bool gameIsBeta { get; }
        System.Version GameVersion { get; }
        StarSystem DestinationStarSystem { get; }
        decimal DestinationDistanceLy { get; }
        string Environment { get; }
        StarSystem CurrentStarSystem { get; }
        StarSystem LastStarSystem { get; }
        StarSystem NextStarSystem { get; }
        StarSystem SearchStarSystem { get; }
        decimal SearchDistanceLy { get; }
        Station CurrentStation { get; }
        Station SearchStation { get; }
        Body CurrentStellarBody { get; }
        FleetCarrier FleetCarrier { get; }
        FleetCarrier SquadronCarrier { get; }
        Ship CurrentShip { get; }
        string Vehicle { get; }
        Dictionary<int, VesselDefinition> DeployedVessels { get; }
    }
}

using EddiDataDefinitions;

namespace EddiCore.GameState
{
    internal interface IEddiGameStateMutator
    {
        bool inTelepresence { get; set; }
        bool inHorizons { get; set; }
        bool inOdyssey { get; set; }
        bool gameIsBeta { get; set; }
        System.Version GameVersion { get; set; }
        string GameVersionRaw { get; set; }
        StarSystem DestinationStarSystem { get; set; }
        decimal DestinationDistanceLy { get; set; }
        string Environment { get; set; }
        StarSystem CurrentStarSystem { get; set; }
        StarSystem LastStarSystem { get; set; }
        StarSystem NextStarSystem { get; set; }
        Station CurrentStation { get; set; }
        Body CurrentStellarBody { get; set; }
        FleetCarrier FleetCarrier { get; set; }
        FleetCarrier SquadronCarrier { get; set; }
        Ship CurrentShip { get; set; }
        string Vehicle { get; set; }
        StarSystem SearchStarSystem { get; set; }
        Station SearchStation { get; set; }
        decimal SearchDistanceLy { get; set; }
        void SetGameVersionDetails ( string version, string build );
    }
}

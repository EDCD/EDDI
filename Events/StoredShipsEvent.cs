using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class StoredShipsEvent (
        DateTime timestamp,
        long marketId,
        string station,
        string system,
        List<Ship> shipyard )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Stored ships";
        public const string DESCRIPTION = "Triggered when the `Shipyard` screen is opened, providing a list of all stored ships";
        public const string SAMPLE = null;

        // Not intended to be user facing

        public string station { get; private set; } = station;

        public string system { get; private set; } = system;

        public List<Ship> shipyard { get; set; } = shipyard;

        public long marketId { get; private set; } = marketId;
    }
}
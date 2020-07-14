using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiShipMonitor
{
    public class StoredShipsEvent : Event
    {
        public const string NAME = "Stored ships";
        public const string DESCRIPTION = "Triggered when the `Shipyard` screen is opened, providing a list of all stored ships";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static StoredShipsEvent()
        {
        }

        // Not intended to be user facing

        [VoiceAttackIgnore]
        public long marketId { get; private set; }

        [VoiceAttackIgnore]
        public string station { get; private set; }

        [VoiceAttackIgnore]
        public string system { get; private set; }

        [VoiceAttackIgnore]
        public List<Ship> shipyard { get; set; }

        public StoredShipsEvent(DateTime timestamp, long marketId, string station, string system, List<Ship> shipyard) : base(timestamp, NAME)
        {
            this.marketId = marketId;
            this.station = station;
            this.system = system;
            this.shipyard = shipyard;
        }
    }
}
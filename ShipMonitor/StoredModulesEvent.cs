using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class StoredModulesEvent : Event
    {
        public const string NAME = "Stored modules";
        public const string DESCRIPTION = "Triggered when the `Outfitting` screen is opened, providing a list of all stored modules";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static StoredModulesEvent()
        {
        }

        public long marketId { get; private set; }
        public string station { get; private set; }
        public string system { get; private set; }
        public List<StoredModule> storedmodules { get; set; }

        public StoredModulesEvent(DateTime timestamp, long marketId, string station, string system, List<StoredModule> storedmodules) : base(timestamp, NAME)
        {
            this.marketId = marketId;
            this.station = station;
            this.system = system;
            this.storedmodules = storedmodules;
        }
    }
}
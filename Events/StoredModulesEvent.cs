using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class StoredModulesEvent (
        DateTime timestamp,
        long marketId,
        string station,
        string system,
        List<StoredModule> storedmodules )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Stored modules";
        public const string DESCRIPTION = "Triggered when the `Outfitting` screen is opened, providing a list of all stored modules";
        public const string SAMPLE = null;

        // not intended to be user facing

        public string station { get; private set; } = station;

        public string system { get; private set; } = system;

        public List<StoredModule> storedmodules { get; set; } = storedmodules;

        public long marketId { get; private set; } = marketId;
    }
}
using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ModuleInfoEvent ( DateTime timestamp, List<ModuleInfoItem> modules ) : Event( timestamp, NAME )
    {
        public const string NAME = "Module info";
        public const string DESCRIPTION = "Triggered when a ModulesInfo.json file is generated/updated";
        public const string SAMPLE = null;

        // Not intended to be user facing
        public List<ModuleInfoItem> Modules { get; } = modules;
    }
}
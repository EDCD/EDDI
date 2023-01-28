using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ModuleInfoEvent : Event
    {
        public const string NAME = "Module info";
        public const string DESCRIPTION = "Triggered when a ModulesInfo.json file is generated/updated";
        public const string SAMPLE = null;

        // Not intended to be user facing
        public List<ModuleInfoItem> Modules { get; }

        public ModuleInfoEvent(DateTime timestamp, List<ModuleInfoItem> Modules) : base(timestamp, NAME)
        {
            this.Modules = Modules;
        }
    }
}
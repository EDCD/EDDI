using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ModuleInfoEvent : Event
    {
        public const string NAME = "Module info";
        public const string DESCRIPTION = "Triggered when a ModulesInfo.json file is generated/updated";
        public const string SAMPLE = null;

        public ModuleInfoEvent(DateTime timestamp) : base(timestamp, NAME)
        { }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ModuleInfoEvent : Event
    {
        public const string NAME = "Module info";
        public const string DESCRIPTION = "Triggered when a ModulesInfo.json file is generated/updated";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModuleInfoEvent()
        {
        }

        public ModuleInfoEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}
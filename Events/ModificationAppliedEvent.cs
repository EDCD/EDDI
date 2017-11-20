using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ModificationAppliedEvent : Event
    {
        public const string NAME = "Modification applied";
        public const string DESCRIPTION = "Triggered when you apply a modification to a module";
        public const string SAMPLE = @"{ ""timestamp"":""2016-11-14T18:49:56Z"", ""event"":""EngineerApply"", ""Engineer"":""Broo Tarquin"", ""Blueprint"":""Weapon_RapidFire"", ""Level"":5 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModificationAppliedEvent()
        {
            VARIABLES.Add("engineer", "The name of the engineer applying the modification");
            VARIABLES.Add("blueprint", "The blueprint of the modification being applied");
            VARIABLES.Add("level", "The level of the modification being applied");
        }

        [JsonProperty("engineer")]
        public string engineer { get; private set; }

        [JsonProperty("blueprint")]
        public string blueprint { get; private set; }

        [JsonProperty("level")]
        public int level { get; private set; }

        public ModificationAppliedEvent(DateTime timestamp, string engineer, string blueprint, int level) : base(timestamp, NAME)
        {
            this.engineer = engineer;
            this.blueprint = blueprint;
            this.level = level;
        }
    }
}

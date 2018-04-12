using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ModuleSwappedEvent : Event
    {
        public const string NAME = "Module swapped";
        public const string DESCRIPTION = "Triggered when modules are swapped between slots on the ship";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleSwap\", \"FromSlot\":\"MediumHardpoint1\", \"ToSlot\":\"MediumHardpoint2\", \"FromItem\":\"hpt_pulselaser_fixed_medium\", \"ToItem\":\"hpt_multicannon_gimbal_medium\", \"Ship\":\"cobramkiii\", \"ShipID\":1 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModuleSwappedEvent()
        {
            VARIABLES.Add("ship", "The ship for which the module was swapped");
            VARIABLES.Add("shipid", "The ID of the ship for which the module was swapped");
            VARIABLES.Add("fromslot", "The slot from which the swap was initiated");
            VARIABLES.Add("frommodule", "The module (object) from which the swap was initiated");
            VARIABLES.Add("toslot", "The slot to which the swap was finalised");
            VARIABLES.Add("tomodule", "The module (object) to which the swap was finalised");
        }

        [JsonProperty("ship")]
        public string ship { get; private set; }

        [JsonProperty("shipid")]
        public int? shipid { get; private set; }

        [JsonProperty("fromslot")]
        public string fromslot { get; private set; }

        [JsonProperty("frommodule")]
        public Module frommodule { get; private set; }

        [JsonProperty("toslot")]
        public string toslot { get; private set; }

        [JsonProperty("tomodule")]
        public Module tomodule { get; private set; }

        public ModuleSwappedEvent(DateTime timestamp, string ship, int? shipid, string fromslot, Module frommodule, string toslot, Module tomodule) : base(timestamp, NAME)
        {
            this.ship = ShipDefinitions.FromEDModel(ship).model;
            this.shipid = shipid;
            this.fromslot = fromslot;
            this.frommodule = frommodule;
            this.toslot = toslot;
            this.tomodule = tomodule;
        }
    }
}

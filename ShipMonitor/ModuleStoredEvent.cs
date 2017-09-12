using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiShipMonitor
{
    public class ModuleStoredEvent : Event
    {
        public const string NAME = "Module stored";
        public const string DESCRIPTION = "Triggered when you store a module";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleStore\", \"Slot\":\"MediumHardpoint2\", \"Ship\":\"cobramkiii\", \"ShipID\":1, \"StoredItem\":\"hpt_pulselaser_fixed_medium\", \"EngineerModifications\":\"\", \"Cost\":500  }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModuleStoredEvent()
        {
            VARIABLES.Add("slot", "The outfitting slot");
            VARIABLES.Add("ship", "The ship from which the module was stored");
            VARIABLES.Add("shipid", "The ID of the ship from which the module was stored");
            VARIABLES.Add("item", "The item being stored");
            VARIABLES.Add("modifications", "The name of Engineer modifications (if any)");
            VARIABLES.Add("replacementitem", "The item being replaced (if a core module)");
            VARIABLES.Add("cost", "The cost of storage (if any)");

        }
        [JsonProperty("slot")]
        public string slot { get; private set; }

        [JsonProperty("ship")]
        public string ship { get; private set; }

        [JsonProperty("shipid")]
        public int? shipid { get; private set; }

        [JsonProperty("item")]
        public string item { get; private set; }

        [JsonProperty("modifications")]
        public string modifications { get; private set; }

        [JsonProperty("replacementitem")]
        public string swapoutitem { get; private set; }

        [JsonProperty("cost")]
        public long? cost { get; private set; }

        public ModuleStoredEvent(DateTime timestamp, string slot, string ship, int? shipid, string item, string modifications, string replacementitem, long? cost) : base(timestamp, NAME)
        {
            this.slot = slot;
            this.ship = ship;
            this.shipid = shipid;
            this.item = item;
            this.modifications = modifications;
            this.swapoutitem = swapoutitem;
            this.cost = cost;
        }
    }
}

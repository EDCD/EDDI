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
    public class ModuleRetrievedEvent : Event
    {
        public const string NAME = "Module retrieved";
        public const string DESCRIPTION = "Triggered when you fetch a previously stored module";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleRetrieve\", \"Slot\":\"MediumHardpoint2\", \"Ship\":\"cobramkiii\", \"ShipID\":1, \"RetrievedItem\":\"hpt_pulselaser_fixed_medium\", \"EngineerModifications\":\"\", \"SwapOutItem\":\"hpt_multicannon_gimbal_medium\", \"Cost\":500  }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModuleRetrievedEvent()
        {
            VARIABLES.Add("slot", "The outfitting slot");
            VARIABLES.Add("ship", "The ship for which the module was retrieved");
            VARIABLES.Add("shipid", "The ID of the ship for which the module was retrieved");
            VARIABLES.Add("item", "The item being retrieved");
            VARIABLES.Add("modifications", "The name of Engineer modifications, if any");
            VARIABLES.Add("swapoutitem", "The item being swapped out (if the slot was not empty)");
            VARIABLES.Add("cost", "The cost of retrieval");

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

        [JsonProperty("swapoutitem")]
        public string swapoutitem { get; private set; }

        [JsonProperty("cost")]
        public long? cost { get; private set; }

        public ModuleRetrievedEvent(DateTime timestamp, string slot, string ship, int? shipid, string item, string modifications, string swapoutitem, long? cost) : base(timestamp, NAME)
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

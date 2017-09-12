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
    public class ModuleSwappedEvent : Event
    {
        public const string NAME = "Module swapped";
        public const string DESCRIPTION = "Triggered when a module is moved to a different slot on the ship";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleSwap\", \"FromSlot\":\"MediumHardpoint1\", \"ToSlot\":\"MediumHardpoint2\", \"FromItem\":\"hpt_pulselaser_fixed_medium\", \"ToItem\":\"hpt_multicannon_gimbal_medium\", \"Ship\":\"cobramkiii\", \"ShipID\":1 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModuleSwappedEvent()
        {
            VARIABLES.Add("fromslot", "The slot from which the swap was initiated");
            VARIABLES.Add("toslot", "The slot to which the swap was finalised");
            VARIABLES.Add("fromitem", "The item from which the swap was initiated");
            VARIABLES.Add("toitem", "The item to which the swap was finalised");
            VARIABLES.Add("ship", "The ship for which the module was swapped");
            VARIABLES.Add("shipid", "The ID of the ship for which the module was swapped");
        }
        [JsonProperty("fromslot")]
        public string fromslot { get; private set; }

        [JsonProperty("toslot")]
        public string toslot { get; private set; }

        [JsonProperty("fromitem")]
        public string fromitem { get; private set; }

        [JsonProperty("toitem")]
        public string toitem { get; private set; }

        [JsonProperty("ship")]
        public string ship { get; private set; }

        [JsonProperty("shipid")]
        public int? shipid { get; private set; }

        public ModuleSwappedEvent(DateTime timestamp, string fromslot, string toslot, string fromitem, string toitem, string ship, int? shipid) : base(timestamp, NAME)
        {
            this.fromslot = fromslot;
            this.toslot = toslot;
            this.fromitem = fromitem;
            this.toitem = toitem;
            this.ship = ship;
            this.shipid = shipid;

        }
    }
}

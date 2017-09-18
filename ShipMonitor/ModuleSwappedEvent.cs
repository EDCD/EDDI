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
            VARIABLES.Add("frommodule", "The item from which the swap was initiated");
            VARIABLES.Add("toslot", "The slot to which the swap was finalised");
            VARIABLES.Add("tomodule", "The modlue to which the swap was finalised");
            VARIABLES.Add("ship", "The ship for which the module was swapped");
            VARIABLES.Add("shipid", "The ID of the ship for which the module was swapped");
        }

        public string fromslot { get; private set; }
        public Module frommodule { get; private set; }
        public string toslot { get; private set; }
        public Module tomodule { get; private set; }
        public string ship { get; private set; }
        public int shipid { get; private set; }

        public ModuleSwappedEvent(DateTime timestamp, string fromslot, Module frommodule, string toslot, Module tomodule, string ship, int shipid) : base(timestamp, NAME)
        {
            this.fromslot = fromslot;
            this.frommodule = frommodule;
            this.toslot = toslot;
            this.tomodule = tomodule;
            this.ship = ship;
            this.shipid = shipid;
        }
    }
}

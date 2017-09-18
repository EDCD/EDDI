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
            VARIABLES.Add("module", "The stored module");
            VARIABLES.Add("cost", "The cost of storage (if any)");
            VARIABLES.Add("replacement", "The replacement module (if a core module)");
            VARIABLES.Add("ship", "The ship from which the module was stored");
            VARIABLES.Add("shipid", "The ID of the ship from which the module was stored");
        }

        public string slot { get; private set; }
        public Module module { get; private set; }
        public long? cost { get; private set; }
        public Module replacement { get; private set; }
        public string ship { get; private set; }
        public int shipid { get; private set; }

        public ModuleStoredEvent(DateTime timestamp, string slot, Module module, long? cost, Module replacement, string ship, int shipid) : base(timestamp, NAME)
        {
            this.slot = slot;
            this.module = module;
            this.cost = cost;
            this.replacement = replacement;
            this.ship = ship;
            this.shipid = shipid;
        }
    }
}

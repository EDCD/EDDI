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
            VARIABLES.Add("module", "The retrieved module");
            VARIABLES.Add("cost", "The cost of retrieval");
            VARIABLES.Add("swapout", "The swapped out module");
            VARIABLES.Add("ship", "The ship for which the module was retrieved");
            VARIABLES.Add("shipid", "The ID of the ship for which the module was retrieved");
        }

        public string slot { get; private set; }
        public Module module { get; private set; }
        public long? cost { get; private set; }
        public Module swapout { get; private set; }
        public string ship { get; private set; }
        public int shipid { get; private set; }

        public ModuleRetrievedEvent(DateTime timestamp, string slot, Module module, long? cost, Module swapout, string ship, int shipid) : base(timestamp, NAME)
        {
            this.slot = slot;
            this.module = module;
            this.cost = cost;
            this.swapout = swapout;
            this.ship = ship;
            this.shipid = shipid;
        }
    }
}

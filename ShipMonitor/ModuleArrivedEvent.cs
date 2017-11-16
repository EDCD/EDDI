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
    public class ModuleArrivedEvent : Event
    {
        public const string NAME = "Module arrived";
        public const string DESCRIPTION = "Triggered when your transferred module is arriving at its destination";
        public const string SAMPLE = "{ \"timestamp\":\"2017-09-20T07:05:41Z\", \"event\":\"ModuleArrived\", \"StorageSlot\":25, \"StoredItem\":\"$hpt_cloudscanner_size0_class1_name;\", \"StoredItem_Localised\":\"Wake Scanner\", \"ServerId\":128662525, \"TransferCost\":322, \"TransferTime\":30, \"Ship\":\"diamondbackxl\", \"ShipID\":38 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModuleArrivedEvent()
        {
            VARIABLES.Add("ship", "The ship you were in when you requested the transfer");
            VARIABLES.Add("shipid", "The ID of the ship you were in when you requested the transfer");
            // VARIABLES.Add("storageslot", "The storage slot"); // This probably isn't useful to the end user
            // VARIABLES.Add("serverid", "The frontier ID of the item being sold"); // This probably isn't useful to the end user
            VARIABLES.Add("module", "The module (object) being transferred");
            VARIABLES.Add("transfercost", "The cost for the module transfer");
            VARIABLES.Add("transfertime", "The time elapsed during the transfer (in seconds)");
            VARIABLES.Add("system", "The system you were in when you requested the transfer");
            VARIABLES.Add("station", "The station you were in when you requested the transfer");

        }

        public string ship { get; private set; }
        public int? shipid { get; private set; }
        public int storageslot { get; private set; }
        public long serverid { get; private set; }
        public Module module { get; private set; }
        public long transfercost { get; private set; }
        public long? transfertime { get; private set; }
        public string system { get; private set; }
        public string station { get; private set; }

        public ModuleArrivedEvent(DateTime timestamp, string ship, int? shipid, int storageslot, long serverid, Module module, long transfercost, long? transfertime, string system, string station) : base(timestamp, NAME)
        {
            this.ship = ship;
            this.shipid = shipid;
            this.storageslot = storageslot;
            this.serverid = serverid;
            this.module = module;
            this.transfercost = transfercost;
            this.transfertime = transfertime;
            this.system = system;
            this.station = station;
        }
    }
}

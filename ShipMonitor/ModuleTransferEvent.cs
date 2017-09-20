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
    public class ModuleTransferEvent : Event
    {
        public const string NAME = "Module transfer";
        public const string DESCRIPTION = "Triggered when you transfer a module from storage at another station";
        // public const string SAMPLE = "{ \"timestamp\":\"2017-09-20T05:16:11Z\", \"event\":\"FetchRemoteModule\", \"StorageSlot\":50, \"StoredItem\":\"$hpt_chafflauncher_tiny_name;\", \"StoredItem_Localised\":\"Chaff\", \"ServerId\":128049513, \"TransferCost\":242, \"Ship\":\"diamondbackxl\", \"ShipID\":38 }";
        public const string SAMPLE = "{ \"timestamp\":\"2017-09-20T07:05:41Z\", \"event\":\"FetchRemoteModule\", \"StorageSlot\":25, \"StoredItem\":\"$hpt_cloudscanner_size0_class1_name;\", \"StoredItem_Localised\":\"Wake Scanner\", \"ServerId\":128662525, \"TransferCost\":322, \"Ship\":\"diamondbackxl\", \"ShipID\":38 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModuleTransferEvent()
        {
            VARIABLES.Add("ship", "The ship you are in when you request the transfer");
            VARIABLES.Add("shipid", "The ID of the ship you are in when you request the transfer");
            // VARIABLES.Add("storageslot", "The storage slot"); // This probably isn't useful to the end user
            // VARIABLES.Add("serverid", "The frontier ID of the item being sold"); // This probably isn't useful to the end user
            VARIABLES.Add("module", "The module (object) being transferred");
            VARIABLES.Add("transfercost", "The cost for the module transfer");
            VARIABLES.Add("transfertime", "The time until the module arrives (in seconds)");
        }

        public string ship { get; private set; }
        public int? shipid { get; private set; }
        public int storageslot { get; private set; }
        public long serverid { get; private set; }
        public Module module { get; private set; }
        public long transfercost { get; private set; }
        public long? transfertime { get; private set; }

        public ModuleTransferEvent(DateTime timestamp, string ship, int? shipid, int storageslot, long serverid, Module module, long transfercost, long? transfertime) : base(timestamp, NAME)
        {
            this.ship = ship;
            this.shipid = shipid;
            this.storageslot = storageslot;
            this.serverid = serverid;
            this.module = module;
            this.transfercost = transfercost;
            this.transfertime = transfertime;
        }
    }
}

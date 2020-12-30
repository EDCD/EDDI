using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ModuleArrivedEvent : Event
    {
        public const string NAME = "Module arrived";
        public const string DESCRIPTION = "Triggered when your transferred module is arriving at its destination";
        public static ModuleArrivedEvent SAMPLE = new ModuleArrivedEvent(DateTime.UtcNow, "Adder", 106, 25, 128662525, Module.FromEDName("$hpt_cloudscanner_size0_class1_name;"), 322, 30, "Lalande 32151", "Lee Gateway");

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModuleArrivedEvent()
        {
            VARIABLES.Add("ship", "The ship you were in when you requested the transfer");
            VARIABLES.Add("shipid", "The ID of the ship you were in when you requested the transfer");
            VARIABLES.Add("module", "The module (object) being transferred");
            VARIABLES.Add("transfercost", "The cost for the module transfer");
            VARIABLES.Add("transfertime", "The time elapsed during the transfer (in seconds)");
            VARIABLES.Add("system", "The system at which the module shall arrive");
            VARIABLES.Add("station", "The station at which the module shall arrive");
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
            this.ship = ShipDefinitions.FromEDModel(ship).model;
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

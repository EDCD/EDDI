using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiShipMonitor
{
    [PublicAPI]
    public class ModuleArrivedEvent : Event
    {
        public const string NAME = "Module arrived";
        public const string DESCRIPTION = "Triggered when your transferred module is arriving at its destination";
        public static ModuleArrivedEvent SAMPLE = new ModuleArrivedEvent(DateTime.UtcNow, "Adder", 106, 25, 128662525, Module.FromEDName("$hpt_cloudscanner_size0_class1_name;"), 322, 30, "Lalande 32151", "Lee Gateway");

        [PublicAPI("The ship you were in when you requested the transfer")]
        public string ship { get; private set; }

        [PublicAPI("The ID of the ship you were in when you requested the transfer")]
        public int? shipid { get; private set; }

        [PublicAPI("The module (object) being transferred")]
        public Module module { get; private set; }

        [PublicAPI("The cost for the module transfer")]
        public long transfercost { get; private set; }

        [PublicAPI("The time elapsed during the transfer (in seconds)")]
        public long? transfertime { get; private set; }

        [PublicAPI("The system at which the module shall arrive")]
        public string system { get; private set; }

        [PublicAPI("The station at which the module shall arrive")]
        public string station { get; private set; }

        // Not intended to be user facing
        public int storageslot { get; private set; }

        public long serverid { get; private set; }

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

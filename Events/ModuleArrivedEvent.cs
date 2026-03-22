using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ModuleArrivedEvent (
        DateTime timestamp,
        string ship,
        int? shipid,
        int storageslot,
        long serverid,
        Module module,
        long transfercost,
        long? transfertime,
        string system,
        string station )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Module arrived";
        public const string DESCRIPTION = "Triggered when your transferred module is arriving at its destination";
        public static readonly ModuleArrivedEvent SAMPLE = new(DateTime.UtcNow, "Adder", 106, 25, 128662525, Module.FromEDName("$hpt_cloudscanner_size0_class1_name;"), 322, 30, "Lalande 32151", "Lee Gateway");

        [PublicAPI("The ship you were in when you requested the transfer")]
        public string ship { get; private set; } = ShipDefinitions.FromEDModel(ship).model;

        [PublicAPI("The ID of the ship you were in when you requested the transfer")]
        public int? shipid { get; private set; } = shipid;

        [PublicAPI("The module (object) being transferred")]
        public Module module { get; private set; } = module;

        [PublicAPI("The cost for the module transfer")]
        public long transfercost { get; private set; } = transfercost;

        [PublicAPI("The time elapsed during the transfer (in seconds)")]
        public long? transfertime { get; private set; } = transfertime;

        [PublicAPI("The system at which the module shall arrive")]
        public string system { get; private set; } = system;

        [PublicAPI("The station at which the module shall arrive")]
        public string station { get; private set; } = station;

        // Not intended to be user facing
        public int storageslot { get; private set; } = storageslot;

        public long serverid { get; private set; } = serverid;
    }
}

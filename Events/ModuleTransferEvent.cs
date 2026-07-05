using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ModuleTransferEvent (
        DateTime timestamp,
        string ship,
        int? shipid,
        int storageslot,
        long serverid,
        Module module,
        long transfercost,
        long? transfertime )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Module transfer";
        public const string DESCRIPTION = "Triggered when you transfer a module from storage at another station";
        public const string SAMPLE = "{ \"timestamp\":\"2017-09-20T07:05:41Z\", \"event\":\"FetchRemoteModule\", \"StorageSlot\":25, \"StoredItem\":\"$hpt_cloudscanner_size0_class1_name;\", \"StoredItem_Localised\":\"Wake Scanner\", \"ServerId\":128662525, \"TransferCost\":322, \"TransferTime\":120, \"Ship\":\"diamondbackxl\", \"ShipID\":38 }";

        [PublicAPI("The ship you are in when you request the transfer")]
        public string ship { get; private set; } = ShipDefinitions.FromEDModel(ship).model;

        [PublicAPI("The ID of the ship you are in when you request the transfer")]
        public int? shipid { get; private set; } = shipid;

        [PublicAPI("The module (object) being transferred")]
        public Module module { get; private set; } = module;

        [PublicAPI("The cost for the module transfer")]
        public long transfercost { get; private set; } = transfercost;

        [PublicAPI("The time until the module arrives (in seconds)")]
        public long? transfertime { get; private set; } = transfertime;

        //Not intended to be user facing

        public int storageslot { get; private set; } = storageslot;

        public long serverid { get; private set; } = serverid;
    }
}

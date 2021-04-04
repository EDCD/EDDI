using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiShipMonitor
{
    public class ModuleRetrievedEvent : Event
    {
        public const string NAME = "Module retrieved";
        public const string DESCRIPTION = "Triggered when you fetch a previously stored module";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleRetrieve\", \"MarketID\": 128666762, \"Slot\":\"MediumHardpoint2\", \"Ship\":\"cobramkiii\", \"ShipID\":1, \"Hot\":true, \"RetrievedItem\":\"hpt_pulselaser_fixed_medium\", \"EngineerModifications\":\"\", \"SwapOutItem\":\"hpt_multicannon_gimbal_medium\", \"Cost\":500  }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModuleRetrievedEvent()
        {
            VARIABLES.Add("ship", "The ship for which the module was retrieved");
            VARIABLES.Add("shipid", "The ID of the ship for which the module was retrieved");
            VARIABLES.Add("slot", "The outfitting slot");
            VARIABLES.Add("module", "The module (object) retrieved from storage");
            VARIABLES.Add("cost", "The cost of retrieval");
            VARIABLES.Add("engineermodifications", "The name of the modification blueprint");
            VARIABLES.Add("swapoutmodule", "The module (object) swapped out (if the slot was not empty)");
        }

        [PublicAPI]
        public string ship => shipDefinition?.model;

        [PublicAPI]
        public int? shipid { get; private set; }

        [PublicAPI]
        public string slot { get; private set; }

        [PublicAPI]
        public Module module { get; private set; }

        [PublicAPI]
        public long? cost { get; private set; }

        [PublicAPI]
        public string engineermodifications { get; private set; }

        [PublicAPI]
        public Module swapoutmodule { get; private set; }

        // Not intended to be user facing

        public long marketId { get; private set; }

        public Ship shipDefinition { get; private set; }

        public ModuleRetrievedEvent(DateTime timestamp, string ship, int? shipid, string slot, Module module, long? cost, string engineermodifications, Module swapoutmodule, long marketId) : base(timestamp, NAME)
        {
            this.shipDefinition = ShipDefinitions.FromEDModel(ship);
            this.shipid = shipid;
            this.slot = slot;
            this.module = module;
            this.cost = cost;
            this.engineermodifications = engineermodifications;
            this.swapoutmodule = swapoutmodule;
            this.marketId = marketId;
        }
    }
}

﻿using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiShipMonitor
{
    public class ModuleStoredEvent : Event
    {
        public const string NAME = "Module stored";
        public const string DESCRIPTION = "Triggered when you store a module";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleStore\", \"MarketID\": 128666762, \"Slot\":\"MediumHardpoint2\", \"Ship\":\"cobramkiii\", \"ShipID\":1, \"Hot\":true, \"StoredItem\":\"hpt_pulselaser_fixed_medium\", \"EngineerModifications\":\"\", \"Cost\":500  }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModuleStoredEvent()
        {
            VARIABLES.Add("ship", "The ship from which the module was stored");
            VARIABLES.Add("shipid", "The ID of the ship from which the module was stored");
            VARIABLES.Add("slot", "The outfitting slot");
            VARIABLES.Add("module", "The module (object) being stored");
            VARIABLES.Add("cost", "The cost of storage (if any)");
            VARIABLES.Add("engineermodifications", "The name of the modification blueprint");
            VARIABLES.Add("replacementmodule", "The module (object) replacement (if a core module)");
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
        public Module replacementmodule { get; private set; }

        // Not intended to be user facing

        public long marketId { get; private set; }

        public Ship shipDefinition { get; private set; }

        public ModuleStoredEvent(DateTime timestamp, string ship, int? shipid, string slot, Module module, long? cost, string engineermodifications, Module replacementmodule, long marketId) : base(timestamp, NAME)
        {
            this.shipDefinition = ShipDefinitions.FromEDModel(ship);
            this.shipid = shipid;
            this.slot = slot;
            this.module = module;
            this.cost = cost;
            this.engineermodifications = engineermodifications;
            this.replacementmodule = replacementmodule;
            this.marketId = marketId;
        }
    }
}

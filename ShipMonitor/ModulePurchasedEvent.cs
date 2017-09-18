﻿using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiShipMonitor
{
    public class ModulePurchasedEvent : Event
    {
        public const string NAME = "Module purchased";
        public const string DESCRIPTION = "Triggered when you purchase a module in outfitting";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleBuy\", \"Slot\":\"MediumHardpoint2\", \"SellItem\":\"hpt_pulselaser_fixed_medium\", \"SellPrice\":0, \"BuyItem\":\"hpt_multicannon_gimbal_medium\", \"BuyPrice\":50018, \"Ship\":\"cobramkiii\", \"ShipID\":1  }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModulePurchasedEvent()
        {
            VARIABLES.Add("slot", "The outfitting slot");
            VARIABLES.Add("module", "The module being purchased");
            VARIABLES.Add("price", "The price of the module being purchased");
            VARIABLES.Add("ship", "The ship for which the module was purchased");
            VARIABLES.Add("shipid", "The ID of the ship for which the module was purchased");
        }

        public string slot { get; private set; }
        public Module module { get; private set; }
        public long price { get; private set; }
        public string ship { get; private set; }
        public int shipid { get; private set; }

        public ModulePurchasedEvent(DateTime timestamp, string slot, Module module, long price, string ship, int shipid) : base(timestamp, NAME)
        {
            this.slot = slot;
            this.module = module;
            this.price = price;
            this.ship = ship;
            this.shipid = shipid;
        }
    }
}

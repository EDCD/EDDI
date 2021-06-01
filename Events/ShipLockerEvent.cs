using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ShipLockerEvent : Event
    {
        public const string NAME = "Ship locker";
        public const string DESCRIPTION = "Triggered when you obtain the contents of your ship's micro-resource materials locker";
        public const string SAMPLE = "{ \"timestamp\":\"2021-05-21T23:59:20Z\", \"event\":\"ShipLockerMaterials\", \"Items\":[ { \"Name\":\"healthmonitor\", \"Name_Localised\":\"Health Monitor\", \"OwnerID\":0, \"Count\":2 }, { \"Name\":\"insight\", \"OwnerID\":0, \"Count\":2 }, { \"Name\":\"compactlibrary\", \"Name_Localised\":\"Compact Library\", \"OwnerID\":0, \"Count\":2 }, { \"Name\":\"infinity\", \"OwnerID\":0, \"Count\":3 }, { \"Name\":\"insightentertainmentsuite\", \"Name_Localised\":\"Insight Entertainment Suite\", \"OwnerID\":0, \"Count\":1 }, { \"Name\":\"degradedpowerregulator\", \"Name_Localised\":\"Degraded Power Regulator\", \"OwnerID\":0, \"Count\":1 } ], \"Components\":[ { \"Name\":\"electricalwiring\", \"Name_Localised\":\"Electrical Wiring\", \"OwnerID\":0, \"Count\":1 }, { \"Name\":\"microtransformer\", \"Name_Localised\":\"Micro Transformer\", \"OwnerID\":0, \"Count\":2 } ], \"Consumables\":[ { \"Name\":\"healthpack\", \"Name_Localised\":\"Medkit\", \"OwnerID\":0, \"Count\":30 }, { \"Name\":\"energycell\", \"Name_Localised\":\"Energy Cell\", \"OwnerID\":0, \"Count\":30 }, { \"Name\":\"amm_grenade_emp\", \"Name_Localised\":\"Shield Disruptor\", \"OwnerID\":0, \"Count\":30 }, { \"Name\":\"amm_grenade_frag\", \"Name_Localised\":\"Frag Grenade\", \"OwnerID\":0, \"Count\":30 }, { \"Name\":\"amm_grenade_shield\", \"Name_Localised\":\"Shield Projector\", \"OwnerID\":0, \"Count\":30 } ], \"Data\":[  ] }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipLockerEvent()
        {
            VARIABLES.Add("inventory", "The items stored in your ship's micro-resource materials locker (as a list of objects with `name`, `category`, and `amount` properties)");
        }

        [JsonProperty("inventory")]
        public List<MicroResourceAmount> inventory { get; }

        public ShipLockerEvent(DateTime timestamp, List<MicroResourceAmount> inventory) : base(timestamp, NAME)
        {
            this.inventory = inventory;
        }
    }
}
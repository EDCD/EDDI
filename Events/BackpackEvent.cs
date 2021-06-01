using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class BackpackEvent : Event
    {
        public const string NAME = "Backpack";
        public const string DESCRIPTION = "Lists the contents of your backpack when you disembark";
        public const string SAMPLE = "{ \"timestamp\":\"2021-05-03T13:31:45Z\", \"event\":\"BackPack\", \"Items\":[  ], \"Components\":[ { \"Name\":\"circuitswitch\", \"Name_Localised\":\"Circuit Switch\", \"OwnerID\":0, \"MissionID\":18446744073709551615, \"Count\":1 }, { \"Name\":\"encryptedmemorychip\", \"Name_Localised\":\"Encrypted Memory Chip\", \"OwnerID\":0, \"MissionID\":18446744073709551615, \"Count\":1 }, { \"Name\":\"epoxyadhesive\", \"Name_Localised\":\"Epoxy Adhesive\", \"OwnerID\":0, \"MissionID\":18446744073709551615, \"Count\":1 }, { \"Name\":\"memorychip\", \"Name_Localised\":\"Memory Chip\", \"OwnerID\":0, \"MissionID\":18446744073709551615, \"Count\":2 } ], \"Consumables\":[ { \"Name\":\"healthpack\", \"Name_Localised\":\"Medkit\", \"OwnerID\":0, \"MissionID\":18446744073709551615, \"Count\":3 }, { \"Name\":\"energycell\", \"Name_Localised\":\"Energy Cell\", \"OwnerID\":0, \"MissionID\":18446744073709551615, \"Count\":3 }, { \"Name\":\"amm_grenade_emp\", \"Name_Localised\":\"Shield Disruptor\", \"OwnerID\":0, \"MissionID\":18446744073709551615, \"Count\":1 }, { \"Name\":\"amm_grenade_frag\", \"Name_Localised\":\"Frag Grenade\", \"OwnerID\":0, \"MissionID\":18446744073709551615, \"Count\":2 }, { \"Name\":\"amm_grenade_shield\", \"Name_Localised\":\"Shield Projector\", \"OwnerID\":0, \"MissionID\":18446744073709551615, \"Count\":1 } ], \"Data\":[  ] }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BackpackEvent()
        {
            VARIABLES.Add("inventory", "The backpack items that you are carrying (as a list of objects with `name`, `category`, and `amount` properties)");
        }

        [JsonProperty("inventory")]
        public List<MicroResourceAmount> inventory { get; }

        public BackpackEvent(DateTime timestamp, List<MicroResourceAmount> inventory) : base(timestamp, NAME)
        {
            this.inventory = inventory;
        }
    }
}
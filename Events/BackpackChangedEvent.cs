using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class BackpackChangedEvent : Event
    {
        public const string NAME = "Backpack changed";
        public const string DESCRIPTION = "Triggered when there is any change to the contents of the suit backpack";
        public const string SAMPLE = "{ \"timestamp\":\"2021-05-22T05:46:01Z\", \"event\":\"BackpackChange\", \"Added\":[ { \"Name\":\"healthpack\", \"Name_Localised\":\"Medkit\", \"OwnerID\":0, \"Count\":1, \"Type\":\"Consumable\" } ] \"Removed\":[ { \"Name\":\"amm_grenade_frag\", \"Name_Localised\":\"Frag Grenade\", \"OwnerID\":0, \"Count\":1, \"Type\":\"Consumable\" } ] }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BackpackChangedEvent()
        {
            VARIABLES.Add("added", "The items added to your backpack (as a list of objects)");
            VARIABLES.Add("removed", "The items added to your backpack (as a list of objects)");
        }

        [PublicAPI]
        public List<MicroResourceAmount> added { get; }

        [PublicAPI]
        public List<MicroResourceAmount> removed { get; }

        public BackpackChangedEvent(DateTime timestamp, List<MicroResourceAmount> added, List<MicroResourceAmount> removed) : base(timestamp, NAME)
        {
            this.added = added;
            this.removed = removed;
        }
    }
}
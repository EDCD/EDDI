using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class ModificationCraftedEvent : Event
    {
        public const string NAME = "Modification crafted";
        public const string DESCRIPTION = "Triggered when you craft a modification to a module";
        public const string SAMPLE = @"{ ""timestamp"":""2016-11-14T18:49:29Z"", ""event"":""EngineerCraft"", ""Engineer"":""Broo Tarquin"", ""Blueprint"":""Weapon_RapidFire"", ""Level"":5, ""Ingredients"":{""powertransferconduits"":1, ""precipitatedalloys"":1, ""configurablecomponents"":1, ""technetium"":1 } }";
        //public const string SAMPLE = @"{ ""timestamp"":""2016-11-14T18:49:29Z"", ""event"":""EngineerCraft"", ""Engineer"":""Broo Tarquin"", ""Blueprint"":""Weapon_RapidFire"", ""Level"":5, ""Ingredients"":[{""Name"":""disruptedwakeechoes"", ""Count"":3 }, {""Name"":""chemicalprocessors"", ""Count"":2 }, {""Name"":""arsenic"", ""Count"":2} ] }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ModificationCraftedEvent()
        {
            VARIABLES.Add("engineer", "The name of the engineer crafting the modification");
            VARIABLES.Add("blueprint", "The blueprint being crafted");
            VARIABLES.Add("level", "The level of the blueprint being crafted");
            VARIABLES.Add("materials", "The materials and quantities used in the crafting (MaterialAmount object)");
            VARIABLES.Add("commodities", "The commodities and quantities used in the crafting (CommodityAmount object)");
        }

        public string engineer { get; private set; }

        public string blueprint{ get; private set; }

        public int level { get; private set; }

        public List<MaterialAmount> materials { get; private set; }

        public List<CommodityAmount> commodities { get; private set; }

        public ModificationCraftedEvent(DateTime timestamp, string engineer, string blueprint, int level, List<MaterialAmount> materials, List<CommodityAmount> commodities) : base(timestamp, NAME)
        {
            this.engineer = engineer;
            this.blueprint = blueprint;
            this.level = level;
            this.materials = materials;
            this.commodities = commodities;
        }
    }

    public class CommodityAmount
    {
        public string commodity { get; private set; }
        public int amount { get; private set; }

        public CommodityAmount(Commodity commodity, int amount)
        {
            this.commodity = commodity.name;
            this.amount = amount;
        }
    }
}

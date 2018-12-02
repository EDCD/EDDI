using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class EngineerContributedEvent : Event
    {
        public const string NAME = "Engineer contributed";
        public const string DESCRIPTION = "Triggered when contributing resources to an engineer in exchange for access";
        public const string SAMPLE = @"{ ""timestamp"":""2017-05-24T10:41:51Z"", ""event"":""EngineerContribution"", ""Engineer"":""Elvira Martuuk"", ""EngineerID"":300160, ""Type"":""Commodity"", ""Commodity"":""soontillrelics"", ""Quantity"":2, ""TotalQuantity"":3 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static EngineerContributedEvent()
        {
            VARIABLES.Add("engineer", "The name of the engineer with whom you have progressed");
            VARIABLES.Add("contributiontype", "The type of contribution (Commodity, Material)");
            VARIABLES.Add("contribution", "The resource contributed");
            VARIABLES.Add("category", "The category of the resource contributed");
            VARIABLES.Add("amount", "The amount contributed");
            VARIABLES.Add("total", "The current total contribution made to that engineer");
        }

        public string engineer => Engineer.name;

        public string contributiontype { get; private set; }

        public string contribution { get; private set; }

        public string category { get; private set; }

        public int amount { get; private set; }

        public int total { get; private set; }

        // Not intended to be user facing
        public Engineer Engineer { get; private set; }
        public MaterialAmount materialAmount { get; private set; }
        public CommodityAmount commodityAmount { get; private set; }

        public EngineerContributedEvent(DateTime timestamp, Engineer Engineer, CommodityAmount commodityAmount, MaterialAmount materialAmount, string contributionType, int amount, int total) : base(timestamp, NAME)
        {
            this.Engineer = Engineer;
            this.contributiontype = contributionType;
            this.commodityAmount = commodityAmount;
            this.materialAmount = materialAmount;
            this.amount = amount;
            this.total = total;
            getContributedResourceDefinition();
        }

        private void getContributedResourceDefinition()
        {
            if (contributiontype == "Commodity")
            {
                CommodityDefinition definition = commodityAmount?.commodityDefinition;
                contribution = definition?.localizedName;
                category = definition?.category?.localizedName;
            }
            else if (contributiontype == "Material")
            {
                Material definition = Material.FromEDName(materialAmount?.edname);
                contribution = definition?.localizedName;
                category = definition?.category?.localizedName;
            }
        }
    }
}

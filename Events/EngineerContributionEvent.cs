using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class EngineerContributedEvent : Event
    {
        public const string NAME = "Engineer contributed";
        public const string DESCRIPTION = "Triggered when contributing resources to an engineer in exchange for access";
        public const string SAMPLE = @"{ ""timestamp"":""2017-05-24T10:41:51Z"", ""event"":""EngineerContribution"", ""Engineer"":""Elvira Martuuk"", ""EngineerID"":300160, ""Type"":""Commodity"", ""Commodity"":""soontillrelics"", ""Quantity"":2, ""TotalQuantity"":3 }";

        [PublicAPI("The name of the engineer with whom you have progressed")]
        public string engineer => Engineer.name;

        [PublicAPI("The type of contribution (Commodity, Material)")]
        public string contributiontype { get; private set; }

        [PublicAPI("The resource contributed")]
        public string contribution { get; private set; }

        [PublicAPI("The category of the resource contributed")]
        public string category { get; private set; }

        [PublicAPI("The amount contributed")]
        public int amount { get; private set; }

        [PublicAPI("The current total contribution made to that engineer")]
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
                category = definition?.Category?.localizedName;
            }
            else if (contributiontype == "Material")
            {
                Material definition = Material.FromEDName(materialAmount?.edname);
                contribution = definition?.localizedName;
                category = definition?.Category?.localizedName;
            }
        }
    }
}

using EddiDataDefinitions;
using System;
using System.Collections.Generic;
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

        [PublicAPI("The type of contribution (Commodity, Materials)")]
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

        public EngineerContributedEvent(DateTime timestamp, Engineer Engineer, string contributionType, int amount, int total, CommodityAmount commodityAmount = null, MaterialAmount materialAmount = null) : base(timestamp, NAME)
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
                var definition = commodityAmount?.commodityDefinition;
                contribution = definition?.localizedName;
                category = definition?.Category?.localizedName;
            }
            else if (contributiontype == "Materials")
            {
                var definition = Material.FromEDName(materialAmount?.edname);
                contribution = definition?.localizedName;
                category = definition?.Category?.localizedName;
            }
        }

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var name = JsonParsing.getString(data, "Engineer");
            var engineerId = JsonParsing.getLong(data, "EngineerID");
            var engineer = Engineer.FromNameOrId(name, engineerId);

            var contributionType = JsonParsing.getString(data, "Type"); // (Commodity, materials, Credits, Bond, Bounty)
            var amount = JsonParsing.getInt(data, "Quantity");
            var total = JsonParsing.getInt(data, "TotalQuantity");
            switch ( contributionType )
            {
                case "Commodity":
                    {
                        var edname = JsonParsing.getString(data, "Commodity");
                        var commodityDef = CommodityDefinition.FromEDName( edname );
                        var commodity = new CommodityAmount(commodityDef, amount);
                        events.Add( new EngineerContributedEvent( timestamp, engineer, contributionType, amount, total, commodity, null ) { raw = line, fromLoad = fromLogLoad } );
                    }
                    break;
                case "Materials":
                    {
                        var edname = JsonParsing.getString(data, "Material");
                        var materialDef = Material.FromEDName( edname );
                        var material = new MaterialAmount(materialDef, amount);
                        events.Add( new EngineerContributedEvent( timestamp, engineer, contributionType, amount, total, null, material ) { raw = line, fromLoad = fromLogLoad } );
                    }
                    break;
                case "Credits":
                case "Bond":
                case "Bounty":
                    {
                        // We don't currently need to handle these types.
                    }
                    break;
            }
            return true;
        }
    }
}

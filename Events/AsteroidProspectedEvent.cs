using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class AsteroidProspectedEvent : Event
    {
        public const string NAME = "Asteroid prospected";
        public const string DESCRIPTION = "Triggered when using a prospecting drone";
        public const string SAMPLE = "{ \"timestamp\":\"2020-04-10T02:32:21Z\", \"event\":\"ProspectedAsteroid\", \"Materials\":[ { \"Name\":\"LowTemperatureDiamond\", \"Name_Localised\":\"Low Temperature Diamonds\", \"Proportion\":26.078022 }, { \"Name\":\"HydrogenPeroxide\", \"Name_Localised\":\"Hydrogen Peroxide\", \"Proportion\":10.189009 } ], \"MotherlodeMaterial\":\"Alexandrite\", \"Content\":\"$AsteroidMaterialContent_Low;\", \"Content_Localised\":\"Material Content: Low\", \"Remaining\":90.000000 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static AsteroidProspectedEvent()
        {
            VARIABLES.Add("commodities", "A list of `CommodityPresence` objects describing the contents of the asteroid (each with properties of `commodity` and `percentage`)");
            VARIABLES.Add("materialcontent", "The material content of the asteroid (high, medium, or low)");
            VARIABLES.Add("remaining", "The percentage of the asteroid's contents which remain available for surface mining");
            VARIABLES.Add("motherlode", "The asteroid's motherlode (if any)");
        }

        public List<CommodityPresence> commodities { get; private set; }

        public string materialcontent => materialContent.localizedName;

        public decimal remaining { get; private set; }

        public string motherlode => motherlodeCommodityDefinition != null ? motherlodeCommodityDefinition.localizedName : "";

        // Not intended to be public facing
        public AsteroidMaterialContent materialContent { get; private set; }

        public CommodityDefinition motherlodeCommodityDefinition { get; private set; }

        public AsteroidProspectedEvent(DateTime timestamp, List<CommodityPresence> commodities, AsteroidMaterialContent materialContent, decimal remaining, CommodityDefinition motherlodeCommodityDefinition) : base(timestamp, NAME)
        {
            this.commodities = commodities;
            this.materialContent = materialContent;
            this.remaining = remaining;
            this.motherlodeCommodityDefinition = motherlodeCommodityDefinition;
        }
    }
}
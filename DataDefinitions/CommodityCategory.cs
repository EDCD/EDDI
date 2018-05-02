
namespace EddiDataDefinitions
{
    public class CommodityCategory : ResourceBasedLocalizedEDName<CommodityCategory>
    {
        static CommodityCategory()
        {
            resourceManager = Properties.CommodityCategories.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new CommodityCategory(edname);

            Chemicals = new CommodityCategory("Chemicals");
            ConsumerItems = new CommodityCategory("ConsumerItems");
            Foods = new CommodityCategory("Foods");
            IndustrialMaterials = new CommodityCategory("IndustrialMaterials");
            Machinery = new CommodityCategory("Machinery");
            Medicines = new CommodityCategory("Medicines");
            Metals = new CommodityCategory("Metals");
            Minerals = new CommodityCategory("Minerals");
            Narcotics = new CommodityCategory("Narcotics");
            NonMarketable = new CommodityCategory("NonMarketable");
            Powerplay = new CommodityCategory("Powerplay");
            Salvage = new CommodityCategory("Salvage");
            Slaves = new CommodityCategory("Slaves");
            Technology = new CommodityCategory("Technology");
            Textiles = new CommodityCategory("Textiles");
            Unknown = new CommodityCategory("Unknown");
            Waste = new CommodityCategory("Waste");
            Weapons = new CommodityCategory("Weapons");
        }

        public static readonly CommodityCategory Chemicals;
        public static readonly CommodityCategory ConsumerItems;
        public static readonly CommodityCategory Foods;
        public static readonly CommodityCategory IndustrialMaterials;
        public static readonly CommodityCategory Machinery;
        public static readonly CommodityCategory Medicines;
        public static readonly CommodityCategory Metals;
        public static readonly CommodityCategory Minerals;
        public static readonly CommodityCategory Narcotics;
        public static readonly CommodityCategory NonMarketable;
        public static readonly CommodityCategory Powerplay;
        public static readonly CommodityCategory Salvage;
        public static readonly CommodityCategory Slaves;
        public static readonly CommodityCategory Technology;
        public static readonly CommodityCategory Textiles;
        public static readonly CommodityCategory Unknown;
        public static readonly CommodityCategory Waste;
        public static readonly CommodityCategory Weapons;

        // dummy used to ensure that the static constructor has run
        public CommodityCategory() : this("")
        { }

        private CommodityCategory(string edname) : base(edname, edname)
        { }
    }
}

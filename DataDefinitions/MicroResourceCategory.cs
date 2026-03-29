namespace EddiDataDefinitions
{
    public class MicroResourceCategory : ResourceBasedLocalizedEDName<MicroResourceCategory>
    {
        static MicroResourceCategory()
        {
            resourceManager = Properties.MicroResourceCategories.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new MicroResourceCategory(edname);
        }

        public static readonly MicroResourceCategory Components = new("Component");
        public static readonly MicroResourceCategory Consumables = new("Consumable");
        public static readonly MicroResourceCategory Data = new("Data");
        public static readonly MicroResourceCategory Items = new("Item");
        public static readonly MicroResourceCategory Unknown = new("Unknown");

        // dummy used to ensure that the static constructor has run
        public MicroResourceCategory() : this("")
        { }

        private MicroResourceCategory(string edname) : base(edname, edname)
        { }
    }
}

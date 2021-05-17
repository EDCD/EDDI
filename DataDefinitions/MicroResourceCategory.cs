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

        public static readonly MicroResourceCategory Component = new MicroResourceCategory("Component");
        public static readonly MicroResourceCategory Consumable = new MicroResourceCategory("Consumable");
        public static readonly MicroResourceCategory Data = new MicroResourceCategory("Data");
        public static readonly MicroResourceCategory Item = new MicroResourceCategory("Item");
        public static readonly MicroResourceCategory Unknown = new MicroResourceCategory("Unknown");

        // dummy used to ensure that the static constructor has run
        public MicroResourceCategory() : this("")
        { }

        private MicroResourceCategory(string edname) : base(edname, edname)
        { }
    }
}

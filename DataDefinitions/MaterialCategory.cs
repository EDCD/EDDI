namespace EddiDataDefinitions
{
    public class MaterialCategory : ResourceBasedLocalizedEDName<MaterialCategory>
    {
        static MaterialCategory()
        {
            resourceManager = Properties.MaterialCategories.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new MaterialCategory(edname);

            Data = new MaterialCategory("Data");
            Element = new MaterialCategory("Element");
            Encoded = new MaterialCategory("Encoded");
            Manufactured = new MaterialCategory("Manufactured");
            Unknown = new MaterialCategory("Unknown");
        }

        public static readonly MaterialCategory Data;
        public static readonly MaterialCategory Element;
        public static readonly MaterialCategory Encoded;
        public static readonly MaterialCategory Manufactured;
        public static readonly MaterialCategory Unknown;

        // dummy used to ensure that the static constructor has run
        public MaterialCategory() : this("")
        { }

        private MaterialCategory(string edname) : base(edname, edname)
        { }

        public new MaterialCategory FromEDName(string EDName)
        {
            string normalizedEDName = EDName.Replace("$MICRORESOURCE_CATEGORY_", "").Replace(";", "");
            return ResourceBasedLocalizedEDName<MaterialCategory>.FromEDName(normalizedEDName);
        }
    }
}

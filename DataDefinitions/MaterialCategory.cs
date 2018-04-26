using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Manufactured = new MaterialCategory("Manufactured");
            Unknown = new MaterialCategory("Unknown");
        }

        public static readonly MaterialCategory Data;
        public static readonly MaterialCategory Element;
        public static readonly MaterialCategory Manufactured;
        public static readonly MaterialCategory Unknown;

        // dummy used to ensure that the static constructor has run
        public MaterialCategory() : this("")
        { }

        private MaterialCategory(string edname) : base(edname, edname)
        { }

        new public MaterialCategory FromEDName(string edname)
        {
            string normalizedEDName = edname.Replace("$MICRORESOURCE_CATEGORY_", "").Replace(";", "");
            return ResourceBasedLocalizedEDName<MaterialCategory>.FromEDName(normalizedEDName);
        }
    }
}

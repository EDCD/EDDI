using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Ring compositions
    /// </summary>
    public class Composition: ResourceBasedLocalizedEDName<Composition>
    {
        static Composition()
        {
            resourceManager = Properties.Compositions.ResourceManager;
            resourceManager.IgnoreCase = false;
            
            var Icy = new Composition("Icy");
            var Rocky = new Composition("Rocky");
            var Metallic = new Composition("Metalic"); // sic
            var MetalRich = new Composition("MetalRich");
        }

        // dummy used to ensure that the static constructor has run
        public Composition() : this("")
        {}

        private Composition(string edname): base(edname, edname.Replace("eRingClass_", "").Replace("-", ""))
        {}

        new public static Composition FromEDName(string edname)
        {
            string normalizedEDName = edname.Replace("eRingClass_", "").Replace("-", "");
            return ResourceBasedLocalizedEDName<Composition>.FromEDName(normalizedEDName);
        }
    }
}

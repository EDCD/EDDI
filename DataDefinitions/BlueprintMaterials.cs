using Newtonsoft.Json;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Materials for a blueprint
    /// </summary>
    public class BlueprintMaterials
    {
        private static readonly Dictionary<string, BlueprintMaterials> BLUEPRINTMATERIALS = new Dictionary<string, BlueprintMaterials>();

        public List<MaterialAmount> materials { get; private set; }

        public BlueprintMaterials(List<MaterialAmount> materials)
        {
            this.materials = materials;
        }

        static BlueprintMaterials()
        {
            string data = Net.DownloadString(Constants.EDDI_SERVER_URL + "blueprintrequirements.json");
            if (data != null)
            {
                Dictionary<string, BlueprintMaterials> blueprints = JsonConvert.DeserializeObject<Dictionary<string, BlueprintMaterials>>(data);
                foreach (KeyValuePair<string, BlueprintMaterials> kv in blueprints)
                {
                    BLUEPRINTMATERIALS[kv.Key.ToLowerInvariant().Replace(" ", "")] = kv.Value;
                }
            }
        }

        public static BlueprintMaterials FromName(string name)
        {
            if (name == null)
            {
                return null;
            }

            string tidiedName = name.ToLowerInvariant().Replace(" ", "");
            BlueprintMaterials materials = null;
            BLUEPRINTMATERIALS.TryGetValue(tidiedName, out materials);
            return materials;
        }
    }
}

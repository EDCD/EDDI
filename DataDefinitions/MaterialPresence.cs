using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Presence of a material
    /// </summary>
    public class MaterialPresence
    {
        [JsonIgnore]
        public Material definition { get; private set; }

        // We merged this with MaterialPercentage (which is now gone) but old scripts used different keys for the material's name so put them both here
        public string material { get; private set; }
        // ....but we prefer 'material' so ignore this for JSON
        [JsonIgnore]
        public string name { get; private set; }

        [JsonIgnore]
        public Rarity rarity { get; private set; }

        public decimal percentage { get; private set; }

        public MaterialPresence(Material definition, decimal percentage)
        {
            this.definition = definition;
            this.name = definition.name;
            this.material = definition.name;
            this.rarity= definition.rarity;
            this.percentage = percentage;
        }

        [JsonConstructor]
        public MaterialPresence(string material, decimal percentage)
        {
            Material definition = Material.FromName(material);
            this.definition = definition;
            this.name = definition.name;
            this.material = definition.name;
            this.rarity = definition.rarity;
            this.percentage = percentage;
        }
    }
}

using Newtonsoft.Json;
using System;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Presence of a material
    /// </summary>
    public class MaterialPresence ( Material definition, decimal percentage )
    {
        [PublicAPI( "the material's localized name" ), JsonProperty("material")]
        public string name { get; private set; } = definition?.localizedName;

        [PublicAPI( "the material's localized category" )]
        public string category => definition?.category;

        [PublicAPI( "the material's localized rarity" )]
        public string rarity => definition?.Rarity.localizedName;   

        [PublicAPI( "the percentage of the material" )]
        public decimal percentage { get; private set; } = percentage;

        // Not intended to be user facing
        
        [JsonIgnore]
        public Material definition { get; private set; } = definition;

        [JsonIgnore, Obsolete("We merged this with MaterialPercentage (which is now gone) but old scripts used different keys for the material's name so put them both here")]
        public string material => name;

        [JsonConstructor]
        public MaterialPresence(string material, decimal percentage)
            : this(Material.FromName(material), percentage)
        { }
    }
}

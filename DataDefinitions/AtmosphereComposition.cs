using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace EddiDataDefinitions
{
    /// <summary> Atmosphere Composition </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AtmosphereComposition
    {
        static AtmosphereComposition()
        {
            resourceManager = Properties.AtmosphereComposition.ResourceManager;
            resourceManager.IgnoreCase = true;

            COMPOSITIONS.Add("water", "Water");
            COMPOSITIONS.Add("oxygen", "Oxygen");
            COMPOSITIONS.Add("carbondioxide", "Carbon dioxide");
            COMPOSITIONS.Add("sulphurdioxide", "Sulphur dioxide");
            COMPOSITIONS.Add("ammonia", "Ammonia");
            COMPOSITIONS.Add("methane", "Methane");
            COMPOSITIONS.Add("nitrogen", "Nitrogen");
            COMPOSITIONS.Add("hydrogen", "Hydrogen");
            COMPOSITIONS.Add("helium", "Helium");
            COMPOSITIONS.Add("neon", "Neon");
            COMPOSITIONS.Add("argon", "Argon");
            COMPOSITIONS.Add("silicates", "Silicates");
            COMPOSITIONS.Add("iron", "Iron");
        }

        public static readonly ResourceManager resourceManager;

        // Translation of composition of atmosphere 
        private static readonly IDictionary<string, string> COMPOSITIONS = new Dictionary<string, string>();

        [JsonProperty("composition")]
        public string edName { get; set; } // Nitrogen, Oxygen, etc.
        public string invariantName => GetInvariantString(edName);
        public string localizedName => GetLocalizedString(edName);
        [JsonIgnore, Obsolete("Please use localizedName or invariantName")]
        public string name => localizedName;

        [JsonProperty("share")]
        public decimal percent { get; set; } // Percent share of the atmosphere

        private string GetInvariantString(string name)
        {
            if (name == null) { return null; }
            name = name.Replace(" ", "_");
            return resourceManager.GetString(name, CultureInfo.InvariantCulture);
        }

        private string GetLocalizedString(string name)
        {
            if (name == null) { return null; }
            name = name.Replace(" ", "_");
            return resourceManager.GetString(name);
        }

        public AtmosphereComposition(string composition, decimal percent)
        {
            this.edName = composition;
            this.percent = percent;
        }
    }
}

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

    /// <summary> Atmosphere Composition </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AtmosphereClass : ResourceBasedLocalizedEDName<AtmosphereClass>
    {
        static AtmosphereClass()
        {
            resourceManager = Properties.AtmosphereClass.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new AtmosphereClass(edname);

            NoAtmosphere = new AtmosphereClass("NoAtmosphere");
            var SuitableForWaterBasedLife = new AtmosphereClass("SuitableForWaterBasedLife");
            var AmmoniaAndOxygen = new AtmosphereClass("AmmoniaAndOxygen");
            var Ammonia = new AtmosphereClass("Ammonia");
            var Water = new AtmosphereClass("Water");
            var CarbonDioxide = new AtmosphereClass("CarbonDioxide");
            var SulphurDioxide = new AtmosphereClass("SulphurDioxide");
            var Nitrogen = new AtmosphereClass("Nitrogen");
            var WaterRich = new AtmosphereClass("WaterRich");
            var MethaneRich = new AtmosphereClass("MethaneRich");
            var AmmoniaRich = new AtmosphereClass("AmmoniaRich");
            var CarbonDioxideRich = new AtmosphereClass("CarbonDioxideRich");
            var Methane = new AtmosphereClass("Methane");
            var Helium = new AtmosphereClass("Helium");
            var SilicateVapour = new AtmosphereClass("SilicateVapour");
            var MetallicVapour = new AtmosphereClass("MetallicVapour");
            var NeonRich = new AtmosphereClass("NeonRich");
            var ArgonRich = new AtmosphereClass("ArgonRich");
            var Neon = new AtmosphereClass("Neon");
            var Argon = new AtmosphereClass("Argon");
            var Oxygen = new AtmosphereClass("Oxygen");
            var EarthLike = new AtmosphereClass("EarthLike");
        }

        public static readonly AtmosphereClass NoAtmosphere;

        // dummy used to ensure that the static constructor has run
        public AtmosphereClass() : this("")
        { }

        private AtmosphereClass(string edname) : base(edname, edname
            .ToLowerInvariant()
            .Replace("thick", "")
            .Replace("thin", "")
            .Replace("hot", "")
            .Replace(" ", "")
            .Replace("-", ""))
        { }

        new public static AtmosphereClass FromEDName(string edname)
        {
            if (edname == null)
            {
                return null;
            }

            // Temperature and pressure are defined separately so we remove them from this string (if descriptors are present)
            string normalizedEDName = edname
            .ToLowerInvariant()
            .Replace("thick", "")
            .Replace("thin", "")
            .Replace("hot", "")
            .Replace(" ", "")
            .Replace("-", "");
            return ResourceBasedLocalizedEDName<AtmosphereClass>.FromEDName(normalizedEDName);
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace EddiDataDefinitions
{
    /// <summary> Atmosphere Class </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class AtmosphereClass : ResourceBasedLocalizedEDName<AtmosphereClass>
    {
        static AtmosphereClass()
        {
            resourceManager = Properties.AtmosphereClass.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new AtmosphereClass(edname);

            var Ammonia = new AtmosphereClass("Ammonia");
            var AmmoniaAndOxygen = new AtmosphereClass("AmmoniaAndOxygen");
            var AmmoniaRich = new AtmosphereClass("AmmoniaRich");
            var Argon = new AtmosphereClass("Argon");
            var ArgonRich = new AtmosphereClass("ArgonRich");
            var CarbonDioxide = new AtmosphereClass("CarbonDioxide");
            var CarbonDioxideRich = new AtmosphereClass("CarbonDioxideRich");
            var EarthLike = new AtmosphereClass("EarthLike");
            var Helium = new AtmosphereClass("Helium");
            var Methane = new AtmosphereClass("Methane");
            var MethaneRich = new AtmosphereClass("MethaneRich");
            var MetallicVapour = new AtmosphereClass("MetallicVapour");
            var Neon = new AtmosphereClass("Neon");
            var NeonRich = new AtmosphereClass("NeonRich");
            var Nitrogen = new AtmosphereClass("Nitrogen");
            var Oxygen = new AtmosphereClass("Oxygen");
            var SilicateVapour = new AtmosphereClass("SilicateVapour");
            var SuitableForWaterBasedLife = new AtmosphereClass("SuitableForWaterBasedLife");
            var SulphurDioxide = new AtmosphereClass("SulphurDioxide");
            var Water = new AtmosphereClass("Water");
            var WaterRich = new AtmosphereClass("WaterRich");

            // Synthetic name(s)
            var GasGiant = new AtmosphereClass("GasGiant");
        }

        public static readonly AtmosphereClass None = new AtmosphereClass("None");

        // dummy used to ensure that the static constructor has run
        public AtmosphereClass() : this("")
        { }

        private AtmosphereClass(string edname) : base(edname, edname
            .ToLowerInvariant()
            .Replace("thick ", "")
            .Replace("thin ", "")
            .Replace("hot ", "")
            .Replace(" ", "")
            .Replace("-", ""))
        { }

        new public static AtmosphereClass FromName(string name)
        {
            if (name == null)
            {
                return FromName("None");
            }

            // Temperature and pressure are defined separately so we remove them from this string (if descriptors are present)
            string normalizedName = name
            .ToLowerInvariant()
            .Replace("thick ", "")
            .Replace("thin ", "")
            .Replace("hot ", "");
            return ResourceBasedLocalizedEDName<AtmosphereClass>.FromName(normalizedName);
        }

        new public static AtmosphereClass FromEDName(string edname)
        {
            if (edname == null)
            {
                return FromEDName("None");
            }

            // Temperature and pressure are defined separately so we remove them from this string (if descriptors are present)
            string normalizedEDName = edname
            .ToLowerInvariant()
            .Replace("thick ", "")
            .Replace("thin ", "")
            .Replace("hot ", "")
            .Replace(" ", "")
            .Replace("-", "");
            return ResourceBasedLocalizedEDName<AtmosphereClass>.FromEDName(normalizedEDName);
        }
    }
}

using Newtonsoft.Json;

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

            None = new AtmosphereClass("None");
            Ammonia = new AtmosphereClass("Ammonia");
            AmmoniaOxygen = new AtmosphereClass("AmmoniaOxygen");
            AmmoniaAndOxygen = new AtmosphereClass("AmmoniaAndOxygen");
            AmmoniaRich = new AtmosphereClass("AmmoniaRich");
            Argon = new AtmosphereClass("Argon");
            ArgonRich = new AtmosphereClass("ArgonRich");
            CarbonDioxide = new AtmosphereClass("CarbonDioxide");
            CarbonDioxideRich = new AtmosphereClass("CarbonDioxideRich");
            EarthLike = new AtmosphereClass("EarthLike");
            Helium = new AtmosphereClass("Helium");
            Methane = new AtmosphereClass("Methane");
            MethaneRich = new AtmosphereClass("MethaneRich");
            MetallicVapour = new AtmosphereClass("MetallicVapour");
            Neon = new AtmosphereClass("Neon");
            NeonRich = new AtmosphereClass("NeonRich");
            Nitrogen = new AtmosphereClass("Nitrogen");
            Oxygen = new AtmosphereClass("Oxygen");
            SilicateVapour = new AtmosphereClass("SilicateVapour");
            SuitableForWaterBasedLife = new AtmosphereClass("SuitableForWaterBasedLife");
            SulphurDioxide = new AtmosphereClass("SulphurDioxide");
            Water = new AtmosphereClass("Water");
            WaterRich = new AtmosphereClass("WaterRich");

            // Synthetic name(s)
            GasGiant = new AtmosphereClass("GasGiant");
        }

        public static readonly AtmosphereClass None;
        public static readonly AtmosphereClass Ammonia;
        public static readonly AtmosphereClass AmmoniaOxygen;
        public static readonly AtmosphereClass AmmoniaAndOxygen;
        public static readonly AtmosphereClass AmmoniaRich;
        public static readonly AtmosphereClass Argon;
        public static readonly AtmosphereClass ArgonRich;
        public static readonly AtmosphereClass CarbonDioxide;
        public static readonly AtmosphereClass CarbonDioxideRich;
        public static readonly AtmosphereClass EarthLike;
        public static readonly AtmosphereClass Helium;
        public static readonly AtmosphereClass Methane;
        public static readonly AtmosphereClass MethaneRich;
        public static readonly AtmosphereClass MetallicVapour;
        public static readonly AtmosphereClass Neon;
        public static readonly AtmosphereClass NeonRich;
        public static readonly AtmosphereClass Nitrogen;
        public static readonly AtmosphereClass Oxygen;
        public static readonly AtmosphereClass SilicateVapour;
        public static readonly AtmosphereClass SuitableForWaterBasedLife;
        public static readonly AtmosphereClass SulphurDioxide;
        public static readonly AtmosphereClass Water;
        public static readonly AtmosphereClass WaterRich;
        public static readonly AtmosphereClass GasGiant;

        // dummy used to ensure that the static constructor has run
        public AtmosphereClass () : this("")
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

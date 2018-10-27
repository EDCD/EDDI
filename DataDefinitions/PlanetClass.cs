using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PlanetClass : ResourceBasedLocalizedEDName<PlanetClass>
    {
        static PlanetClass()
        {
            resourceManager = Properties.PlanetClass.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new PlanetClass(edname);

            var Ammonia = new PlanetClass("Ammonia");
            var EarthLike = new PlanetClass("EarthLike");
            var GasGiantWithAmmoniaBasedLife = new PlanetClass("GasGiantWithAmmoniaBasedLife");
            var GasGiantWithWaterBasedLife = new PlanetClass("GasGiantWithWaterBasedLife");
            var HeliumGasGiant = new PlanetClass("HeliumGasGiant");
            var HeliumRichGasGiant = new PlanetClass("HeliumRichGasGiant");
            var HighMetalContent = new PlanetClass("HighMetalContent");
            var Icy = new PlanetClass("Icy");
            var MetalRich = new PlanetClass("MetalRich");
            var Rock = new PlanetClass("Rocky");
            var RockyIce = new PlanetClass("RockyIce");
            var ClassIGasGiant = new PlanetClass("ClassIGasGiant");
            var ClassIIGasGiant = new PlanetClass("ClassIIGasGiant");
            var ClassIIIGasGiant = new PlanetClass("ClassIIIGasGiant");
            var ClassIVGasGiant = new PlanetClass("ClassIVGasGiant");
            var ClassVGasGiant = new PlanetClass("ClassVGasGiant");
            var WaterGiant = new PlanetClass("WaterGiant");
            var WaterGiantWithLife = new PlanetClass("WaterGiantWithLife");
            var Water = new PlanetClass("Water");
        }

        public static readonly PlanetClass None = new PlanetClass("None");

        // dummy used to ensure that the static constructor has run
        public PlanetClass() : this("")
        { }

        private PlanetClass(string edname) : base(edname, edname)
        { }

        new public static PlanetClass FromEDName(string edname)
        {
            if (edname == null)
            {
                return null;
            }

            string normalizedEDName = edname.Replace(" ", "").Replace("-", "");
            normalizedEDName = normalizedEDName.Replace("world", ""); // In some cases, EDDB uses "world" while the journal uses "body". Fix that here.
            normalizedEDName = normalizedEDName.Replace("body", ""); // In some cases, EDDB uses "world" while the journal uses "body". Fix that here.
            normalizedEDName = normalizedEDName.Replace("sudarsky", ""); // EDDB uses "class iv gas giant" while the journal uses "Sudarsky class IV gas giant". Fix that here.
            return ResourceBasedLocalizedEDName<PlanetClass>.FromEDName(normalizedEDName);
        }
    }
}

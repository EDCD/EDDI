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

            var AmmoniaWorld = new PlanetClass("AmmoniaWorld");
            var EarthLikeBody = new PlanetClass("EarthLikeBody");
            var GasGiantWithAmmoniaBasedLife = new PlanetClass("GasGiantWithAmmoniaBasedLife");
            var GasGiantWithWaterBasedLife = new PlanetClass("GasGiantWithWaterBasedLife");
            var HeliumGasGiant = new PlanetClass("HeliumGasGiant");
            var HeliumRichGasGiant = new PlanetClass("HeliumRichGasGiant");
            var HighMetalContentBody = new PlanetClass("HighMetalContentBody");
            var IcyBody = new PlanetClass("IcyBody");
            var MetalRichBody = new PlanetClass("MetalRichBody");
            var RockBody = new PlanetClass("RockyBody");
            var RockyIceBody = new PlanetClass("RockyIceBody");
            var SudarskyClassIGasGiant = new PlanetClass("SudarskyClassIGasGiant");
            var SudarskyClassIIGasGiant = new PlanetClass("SudarskyClassIIGasGiant");
            var SudarskyClassIIIGasGiant = new PlanetClass("SudarskyClassIIIGasGiant");
            var SudarskyClassIVGasGiant = new PlanetClass("SudarskyClassIVGasGiant");
            var SudarskyClassVGasGiant = new PlanetClass("SudarskyClassVGasGiant");
            var WaterGiant = new PlanetClass("WaterGiant");
            var WaterGiantWithLife = new PlanetClass("WaterGiantWithLife");
            var WaterWorld = new PlanetClass("WaterWorld");
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
            return ResourceBasedLocalizedEDName<PlanetClass>.FromEDName(normalizedEDName);
        }
    }
}

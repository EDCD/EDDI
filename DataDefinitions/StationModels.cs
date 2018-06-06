using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class StationModels : ResourceBasedLocalizedEDName<StationModels>
    {
        static StationModels()
        {
            resourceManager = Properties.StationModels.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new StationModels(edname);

            var AsteroidBase = new StationModels("AsteroidBase");
            var Bernal = new StationModels("Bernal");
            var Coriolis = new StationModels("Coriolis");
            var Megaship = new StationModels("Megaship");
            var Orbis = new StationModels("Orbis");
            var Outpost = new StationModels("Outpost");
            var SurfaceStation = new StationModels("SurfaceStation");
        }

        // dummy used to ensure that the static constructor has run
        public StationModels() : this("")
        {}

        private StationModels(string edname) : base(edname, edname)
        {}
    }
}

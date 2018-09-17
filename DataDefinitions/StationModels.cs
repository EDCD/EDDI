using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class StationModel : ResourceBasedLocalizedEDName<StationModel>
    {
        static StationModel()
        {
            resourceManager = Properties.StationModels.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new StationModel(edname);

            None = new StationModel("None");
            var AsteroidBase = new StationModel("AsteroidBase");
            var Bernal = new StationModel("Bernal");
            var Coriolis = new StationModel("Coriolis");
            var Megaship = new StationModel("Megaship");
            var Orbis = new StationModel("Orbis");
            var Outpost = new StationModel("Outpost");
            var SurfaceStation = new StationModel("SurfaceStation");
        }

        public static readonly StationModel None;

        // dummy used to ensure that the static constructor has run
        public StationModel() : this("")
        {}

        private StationModel(string edname) : base(edname, edname)
        {}
    }
}

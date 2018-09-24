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

        private StationModel(string edname) : base(edname, edname.Replace(" Starport", ""))
        {}

        new public static StationModel FromName(string from)
        {
            return ResourceBasedLocalizedEDName<StationModel>.FromName(eddbModel2journalModel(from));
        }

        private static string eddbModel2journalModel(string eddbModelName)
        {
            if (eddbModelName == null)
            {
                return null;
            }

            eddbModelName = eddbModelName.ToLowerInvariant();

            if (eddbModelName.StartsWith("planetary") || eddbModelName.EndsWith("planetary"))
            {
                return "surface station";
            }

            if (eddbModelName.EndsWith("starport"))
            {
                eddbModelName.Replace(" starport", "");

                // Ocellus starports are described by the journal as "Bernal" so we make the replacement here.
                eddbModelName.Replace("ocellus", "bernal");

                return eddbModelName;
            }

            if (eddbModelName.EndsWith("outpost"))
            {
                // Though EDDB provides details on the type of outpost (military, civilian, scientific, etc.), 
                // that info is not present in the journal. Strip it for consistent output between journal and EDDB data.
                return "outpost";
            }

            return eddbModelName;
        }
    }
}

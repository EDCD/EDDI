using System.Collections.Generic;

namespace EddiDataDefinitions
{
    public class StationModel : ResourceBasedLocalizedEDName<StationModel>
    {
        static StationModel()
        {
            resourceManager = Properties.StationModels.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new StationModel(edname);

            var AsteroidBase = new StationModel("AsteroidBase");
            var Bernal = new StationModel("Bernal");
            var Coriolis = new StationModel("Coriolis");
            var Megaship = new StationModel("Megaship");
            var MegaShipCivilian = new StationModel("MegaShipCivilian");
            var Orbis = new StationModel("Orbis");
            var Outpost = new StationModel("Outpost");
            var SurfaceStation = new StationModel("SurfaceStation");
            var CraterOutpost = new StationModel("CraterOutpost");
            var CraterPort = new StationModel("CraterPort");
            var OutpostScientific = new StationModel("OutpostScientific");
        }

        public static readonly StationModel None = new StationModel("None");

        // dummy used to ensure that the static constructor has run
        public StationModel() : this("")
        {}

        private StationModel(string edname) : base(edname, edname.Replace(" Starport", ""))
        {}

        new public static StationModel FromName(string from)
        {
            if (from == null)
            {
                return null;
            }

            // Translate from EDSM / EDDB station model names if these are present
            Dictionary<string, string> modelTranslations = new Dictionary<string, string>()
            {
                { "Coriolis Starport", "Coriolis" },
                // Ocellus starports are described by the journal as "Bernal"
                { "Ocellus Starport", "Bernal" }, 
                { "Orbis Starport", "Orbis" },
                // The journal doesn't provide details on the type of outpost (military, civilian, scientific, etc.)
                // Types are likely derived from the station primary economy, but mixing these into the model name does not add value.
                { "Outpost", "Outpost" },
                { "Civilian Outpost", "Outpost" },
                { "Commercial Outpost", "Outpost" },
                { "Industrial Outpost", "Outpost" },
                { "Military Outpost", "Outpost" },
                { "Mining Outpost", "Outpost" },
                { "Scientific Outpost", "Outpost" },
                { "Unknown Outpost", "Outpost" },
                // Planetary ports are assigned by EDSM via marketID, from a list given to EDSM by FDev long ago.
                // The journal doesn't distinguish between planetary ports and outposts. 
                { "Planetary Outpost", "SurfaceStation" },
                { "Planetary Port", "SurfaceStation" },
                // Planetary settlements are not dockable and require manual edits on ROSS to add to EDDB
                { "Planetary Settlement", "SurfaceStation" },
                { "Planetary Engineer Base", "SurfaceStation" },
                { "Unknown Planetary", "SurfaceStation" },
                { "Asteroid base", "AsteroidBase" },
                { "Mega ship", "Megaship" },
            };
            foreach (KeyValuePair<string, string> model in modelTranslations)
            {
                if (from == model.Key)
                {
                    return FromEDName(model.Value);
                }
            }

            return ResourceBasedLocalizedEDName<StationModel>.FromName(from);
        }
    }
}

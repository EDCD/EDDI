using System.Collections.Generic;

namespace EddiDataDefinitions
{
    public class StationModel : ResourceBasedLocalizedEDName<StationModel>
    {
        static StationModel()
        {
            // Note: The same station may use the model "Ocellus" for one event and "Bernal" for another. 

            resourceManager = Properties.StationModels.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new StationModel(edname);
        }

        public static readonly StationModel None = new StationModel("None");
        public static readonly StationModel AsteroidBase = new StationModel("AsteroidBase");
        public static readonly StationModel Bernal = new StationModel("Bernal");
        public static readonly StationModel Coriolis = new StationModel("Coriolis");
        public static readonly StationModel Megaship = new StationModel("Megaship");
        public static readonly StationModel MegaShipCivilian = new StationModel("MegaShipCivilian");
        public static readonly StationModel Ocellus = new StationModel("Ocellus");
        public static readonly StationModel Orbis = new StationModel("Orbis");
        public static readonly StationModel Outpost = new StationModel("Outpost");
        public static readonly StationModel SurfaceStation = new StationModel("SurfaceStation"); // No longer used in the player journal
        public static readonly StationModel CraterOutpost = new StationModel("CraterOutpost");
        public static readonly StationModel CraterPort = new StationModel("CraterPort");
        public static readonly StationModel OutpostScientific = new StationModel("OutpostScientific");
        public static readonly StationModel FleetCarrier = new StationModel("FleetCarrier");
        public static readonly StationModel OnFootSettlement = new StationModel("OnFootSettlement");

        // dummy used to ensure that the static constructor has run
        public StationModel() : this("")
        { }

        private StationModel(string edname) : base(edname, edname.Replace(" Starport", ""))
        { }

        new public static StationModel FromName(string from)
        {
            if (from == null)
            {
                return null;
            }

            // Translate from EDSM / EDDB station model names if these are present
            Dictionary<string, StationModel> modelTranslations = new Dictionary<string, StationModel>()
            {
                { "Coriolis Starport", Coriolis },
                { "Bernal Starport", Bernal }, // Ocellus starports are described by the journal as either "Bernal" or "Ocellus"
                { "Ocellus Starport", Ocellus },
                { "Orbis Starport", Orbis },
                // The journal doesn't provide details on the type of outpost (military, civilian, scientific, etc.)
                // Types are likely derived from the station primary economy, but mixing these into the model name does not add value.
                { "Civilian Outpost", Outpost },
                { "Commercial Outpost", Outpost },
                { "Industrial Outpost", Outpost },
                { "Military Outpost", Outpost },
                { "Mining Outpost", Outpost },
                { "Scientific Outpost", Outpost },
                { "Unknown Outpost", Outpost },
                { "Planetary Outpost", CraterOutpost },
                { "Planetary Port", CraterPort },
                { "Planetary Settlement", SurfaceStation }, // Planetary settlements are not dockable and require manual edits on ROSS to add to EDDB
                { "Planetary Engineer Base", CraterOutpost },
                { "Unknown Planetary", SurfaceStation },
                { "Asteroid base", AsteroidBase },
                { "Mega ship", Megaship },
                { "Odyssey Settlement", OnFootSettlement },
            };
            foreach (var model in modelTranslations)
            {
                if (from == model.Key)
                {
                    return model.Value;
                }
            }

            return ResourceBasedLocalizedEDName<StationModel>.FromName(from);
        }
    }
}

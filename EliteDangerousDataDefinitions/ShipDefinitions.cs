using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousDataDefinitions
{
    public class ShipDefinitions
    {
        private static Dictionary<long, Ship> ShipsByEliteID = new Dictionary<long, Ship>()
        {
            { 128049267, new Ship(128049267, "adder", "Zorgon Peterson", "Adder", ShipSize.Small) },
            { 128049363, new Ship(128049363, "anaconda", "Faulcon DeLacy", "Anaconda", ShipSize.Large) },
            { 128049303, new Ship(128049303, "asp", "Lakon Spaceways", "Asp Explorer", ShipSize.Medium) },
            { 128672276, new Ship(128672276, "asp_scout", "Lakon Spaceways", "Asp Scout", ShipSize.Medium) },
            { 111, new Ship(111, "belugaLiner", "Saud Kruger", "Beluga", ShipSize.Large) },
            { 128049279, new Ship(128049279, "cobramkiii", "Faulcon DeLacy", "Cobra Mk. III", ShipSize.Small) },
            { 128672262, new Ship(128672262, "cobramkiv", "Faulcon DeLacy", "Cobra Mk. IV", ShipSize.Small) },
            { 128671831, new Ship(128671831, "diamondbackxl", "Lakon Spaceways", "Diamondback Explorer", ShipSize.Small) },
            { 128671217, new Ship(128671217, "diamondback", "Lakon Spaceways", "Diamondback Scout", ShipSize.Small) },
            { 128049255, new Ship(128049255, "eagle", "Core Dynamics", "Eagle", ShipSize.Small) },
            { 128672145, new Ship(128672145, "federation_dropship_mkii", "Core Dynamics", "Federal Assault Ship", ShipSize.Medium) },
            { 128049369, new Ship(128049369, "federation_corvette", "Core Dynamics", "Federal Corvette", ShipSize.Large) },
            { 128049321, new Ship(128049321, "federation_dropship", "Core Dynamics", "Federal Dropship", ShipSize.Medium) },
            { 128672152, new Ship(128672152, "federation_gunship", "Core Dynamics", "Federal Gunship", ShipSize.Medium) },
            { 128049351, new Ship(128049351, "ferdelance", "Zorgon Peterson", "Fer-de-Lance", ShipSize.Medium) },
            { 128049315, new Ship(128049315, "empire_trader", "Gutamaya", "Imperial Clipper", ShipSize.Large) },
            { 128671223, new Ship(128671223, "empire_courier", "Gutamaya", "Imperial Courier", ShipSize.Small) },
            { 128049375, new Ship(128049375, "cutter", "Gutamaya", "Imperial Cutter", ShipSize.Large) },
            { 128672138, new Ship(128672138, "empire_eagle", "Gutamaya", "Imperial Eagle", ShipSize.Small) },
            { 128049261, new Ship(128049261, "hauler", "Zorgon Peterson", "Hauler", ShipSize.Small) },
            { 128672269, new Ship(128672269, "independant_trader", "Lakon Spaceways", "Keelback", ShipSize.Medium) },
            { 128049327, new Ship(128049327, "orca", "Saud Kruger", "Orca", ShipSize.Large) },
            { 128049339, new Ship(128049339, "python", "Faulcon DeLacy", "Python", ShipSize.Medium )},
            { 128049249, new Ship(128049249, "sidewinder", "Faulcon DeLacy", "Sidewinder", ShipSize.Small) },
            { 128049285, new Ship(128049285, "type6", "Lakon Spaceways", "Type-6 Transporter", ShipSize.Medium) },
            { 128049297, new Ship(128049297, "type7", "Lakon Spaceways", "Type-7 Transporter", ShipSize.Large) },
            { 128049333, new Ship(128049333, "type9", "Lakon Spaceways", "Type-9 Heavy", ShipSize.Large) },
            { 128049273, new Ship(128049273, "viper", "Faulcon DeLacy", "Viper Mk. III", ShipSize.Small) },
            { 128672255, new Ship(128672255, "Viper_MkIV", "Faulcon DeLacy", "Viper Mk. IV", ShipSize.Small) },
            { 128049309, new Ship(128049309, "vulture", "Core Dynamics", "Vulture", ShipSize.Small) },
        };

        public static List<string> ShipModels = ShipsByEliteID.Select(kp => kp.Value.model).ToList();

        private static Dictionary<string, Ship> ShipsByModel = ShipsByEliteID.ToDictionary(kp => kp.Value.model, kp => kp.Value);
        private static Dictionary<string, Ship> ShipsByEDModel = ShipsByEliteID.ToDictionary(kp => kp.Value.EDName, kp => kp.Value);

        /// <summary>Obtain details of a ship given its Elite ID</summary>
        public static Ship FromEliteID(long id)
        {
            Ship Ship = new Ship();
            Ship Template;
            if (ShipsByEliteID.TryGetValue(id, out Template))
            {
                Ship.EDID = Template.EDID;
                Ship.EDName = Template.EDName;
                Ship.manufacturer = Template.manufacturer;
                Ship.model = Template.model;
                Ship.size = Template.size;
            }
            // All ships default to 100% health
            Ship.health = 100;

            return Ship;
        }

        /// <summary>Obtain details of a ship given its model</summary>
        public static Ship FromModel(string model)
        {
            if (model == null)
            {
                return null;
            }

            Ship Ship = new Ship();
            Ship Template;
            if (ShipsByModel.TryGetValue(model, out Template))
            {
                Ship.EDID = Template.EDID;
                Ship.EDName = Template.EDName;
                Ship.manufacturer = Template.manufacturer;
                Ship.model = Template.model;
                Ship.size = Template.size;
            }
            else
            {
                Ship.model = model;
            }
            return Ship;
        }

        /// <summary>Obtain details of a ship given its Elite:Dangerous model</summary>
        public static Ship FromEDModel(string model)
        {
            if (model == null)
            {
                return null;
            }
            Ship Ship = new Ship();
            Ship Template;
            if (ShipsByEDModel.TryGetValue(model.ToLowerInvariant(), out Template))
            {
                Ship.EDID = Template.EDID;
                Ship.EDName = Template.EDName;
                Ship.manufacturer = Template.manufacturer;
                Ship.model = Template.model;
                Ship.size = Template.size;
            }
            else
            {
                Ship.model = model;
            }
            return Ship;
        }
    }
}

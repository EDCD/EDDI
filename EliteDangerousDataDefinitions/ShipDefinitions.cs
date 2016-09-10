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
            { 128049267, new Ship(128049267, "Adder", "Adder", ShipSize.Small) },
            { 128049363, new Ship(128049363, "Anaconda", "Anaconda", ShipSize.Large) },
            { 128049303, new Ship(128049303, "Asp", "Asp Explorer", ShipSize.Medium) },
            { 128672276, new Ship(128672276, "Asp_Scout", "Asp Scout", ShipSize.Medium) },
            { 128049279, new Ship(128049279, "CobraMkIII", "Cobra Mk. III", ShipSize.Small) },
            { 128672262, new Ship(128672262, "CobraMkIV", "Cobra Mk. IV", ShipSize.Small) },
            { 128671831, new Ship(128671831, "DiamondBackXL", "Diamondback Explorer", ShipSize.Small) },
            { 128671217, new Ship(128671217, "DiamondBack", "Diamondback Scout", ShipSize.Small) },
            { 128049255, new Ship(128049255, "Eagle", "Eagle", ShipSize.Small) },
            { 128672145, new Ship(128672145, "Federation_Dropship_MkII", "Federal Assault Ship", ShipSize.Medium) },
            { 128049369, new Ship(128049369, "Federation_Corvette", "Federal Corvette", ShipSize.Large) },
            { 128049321, new Ship(128049321, "Federation_Dropship", "Federal Dropship", ShipSize.Medium) },
            { 128672152, new Ship(128672152, "Federation_Gunship", "Federal Gunship", ShipSize.Medium) },
            { 128049351, new Ship(128049351, "FerDeLance", "Fer-de-Lance", ShipSize.Medium) },
            { 128049315, new Ship(128049315, "Empire_Trader", "Imperial Clipper", ShipSize.Large) },
            { 128671223, new Ship(128671223, "Empire_Courier", "Imperial Courier", ShipSize.Small) },
            { 128049375, new Ship(128049375, "Cutter", "Imperial Cutter", ShipSize.Large) },
            { 128672138, new Ship(128672138, "Empire_Eagle", "Imperial Eagle", ShipSize.Small) },
            { 128049261, new Ship(128049261, "Hauler", "Hauler", ShipSize.Small) },
            { 128672269, new Ship(128672269, "Independant_Trader", "Keelback", ShipSize.Medium) },
            { 128049327, new Ship(128049327, "Orca", "Orca", ShipSize.Large) },
            { 128049339, new Ship(128049339, "Python", "Python", ShipSize.Medium )},
            { 128049249, new Ship(128049249, "SideWinder", "Sidewinder", ShipSize.Small) },
            { 128049285, new Ship(128049285, "Type6", "Type-6 Transporter", ShipSize.Medium) },
            { 128049297, new Ship(128049297, "Type7", "Type-7 Transporter", ShipSize.Large) },
            { 128049333, new Ship(128049333, "Type9", "Type-9 Heavy", ShipSize.Large) },
            { 128049273, new Ship(128049273, "Viper", "Viper Mk. III", ShipSize.Small) },
            { 128672255, new Ship(128672255, "Viper_MkIV", "Viper Mk. IV", ShipSize.Small) },
            { 128049309, new Ship(128049309, "Vulture", "Vulture", ShipSize.Small) },
        };

        public static List<String> ShipModels = ShipsByEliteID.Select(kp => kp.Value.model).ToList();

        private static Dictionary<string, Ship> ShipsByModel = ShipsByEliteID.ToDictionary(kp => kp.Value.model, kp => kp.Value);

        /// <summary>Obtain details of a ship given its Elite ID</summary>
        public static Ship ShipFromEliteID(long id)
        {
            Ship Ship = new Ship();
            Ship Template;
            if (ShipsByEliteID.TryGetValue(id, out Template))
            {
                Ship.EDID = Template.EDID;
                Ship.EDName = Template.EDName;
                Ship.model = Template.model;
                Ship.size = Template.size;
            }
            // All ships default to 100% health
            Ship.Health = 100;

            return Ship;
        }

        /// <summary>Obtain details of a ship given its model</summary>
        public static Ship ShipFromModel(string model)
        {
            Ship Ship = new Ship();
            Ship Template;
            if (ShipsByModel.TryGetValue(model, out Template))
            {
                Ship.EDID = Template.EDID;
                Ship.EDName = Template.EDName;
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

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
            { 128049267, new Ship("Adder", ShipSize.Small) },
            { 128049363, new Ship("Anaconda", ShipSize.Large) },
            { 128049303, new Ship("Asp Explorer", ShipSize.Medium) },
            { 128672276, new Ship("Asp Scout", ShipSize.Medium) },
            { 128049279, new Ship("Cobra Mk. III", ShipSize.Small) },
            { 128672262, new Ship("Cobra Mk. IV", ShipSize.Small) },
            { 128671831, new Ship("Diamondback Explorer", ShipSize.Small) },
            { 128671217, new Ship("Diamondback Scout", ShipSize.Small) },
            { 128049255, new Ship("Eagle", ShipSize.Small) },
            { 128672145, new Ship("Federal Assault Ship", ShipSize.Medium) },
            { 128049369, new Ship("Federal Corvette", ShipSize.Large) },
            { 128049321, new Ship("Federal Dropship", ShipSize.Medium) },
            { 128672152, new Ship("Federal Gunship", ShipSize.Medium) },
            { 128049351, new Ship("Fer-de-Lance", ShipSize.Medium) },
            { 128049315, new Ship("Imperial Clipper", ShipSize.Large) },
            { 128671223, new Ship("Imperial Courier", ShipSize.Small) },
            { 128049375, new Ship("Imperial Cutter", ShipSize.Large) },
            { 128672138, new Ship("Imperial Eagle", ShipSize.Small) },
            { 128049261, new Ship("Hauler", ShipSize.Small) },
            { 128672269, new Ship("Keelback", ShipSize.Medium) },
            { 128049327, new Ship("Orca", ShipSize.Large) },
            { 128049339, new Ship("Python", ShipSize.Medium )},
            { 128049249, new Ship("Sidewinder", ShipSize.Small) },
            { 128049285, new Ship("Type-6 Transporter", ShipSize.Medium) },
            { 128049297, new Ship("Type-7 Transporter", ShipSize.Large) },
            { 128049333, new Ship("Type-9 Heavy", ShipSize.Large) },
            { 128049273, new Ship("Viper Mk. III", ShipSize.Small) },
            { 128672255, new Ship("Viper Mk. IV", ShipSize.Small) },
            { 128049309, new Ship("Vulture", ShipSize.Small) },
        };

        private static Dictionary<string, Ship> ShipsByModel = ShipsByEliteID.ToDictionary(kp => kp.Value.Model, kp => kp.Value);

        /// <summary>Obtain details of a ship given its Elite ID</summary>
        public static Ship ShipFromEliteID(long id)
        {
            Ship Ship = new Ship();
            Ship Template;
            if (ShipsByEliteID.TryGetValue(id, out Template))
            {
                Ship.Model = Template.Model;
                Ship.Size = Template.Size;
            }
            return Ship;
        }

        /// <summary>Obtain details of a ship given its model</summary>
        public static Ship ShipFromModel(string model)
        {
            Ship Ship = new Ship();
            Ship Template;
            if (ShipsByModel.TryGetValue(model, out Template))
            {
                Ship.Model = Template.Model;
                Ship.Size = Template.Size;
            }
            else
            {
                Ship.Model = model;
            }
            return Ship;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiDataDefinitions
{
    public class ShipDefinitions
    {
        private static Dictionary<long, Ship> ShipsByEliteID = new Dictionary<long, Ship>()
        {
            { 128049267, new Ship(128049267, "Adder", "Zorgon Peterson", null, "Adder", null, "Small", null) },
            { 128049363, new Ship(128049363, "Anaconda", "Faulcon DeLacy", null, "Anaconda", null, "Large", 5) },
            { 128049303, new Ship(128049303, "Asp", "Lakon Spaceways", new List<Translation> {new Translation("Lakon", "leɪkɒn") }, "Asp Explorer", null, "Medium", null) },
            { 128672276, new Ship(128672276, "Asp_Scout", "Lakon Spaceways", new List<Translation> {new Translation("Lakon", "leɪkɒn") }, "Asp Scout", null, "Medium", null) },
            { 128049345, new Ship(128049345, "BelugaLiner", "Saud Kruger", new List<Translation> {new Translation("Saud", "saʊd"), new Translation("Kruger", "ˈkruːɡə") }, "Beluga", new List<Translation> {new Translation("beluga", "bɪˈluːɡə") }, "Large", null) },
            { 128049279, new Ship(128049279, "CobraMkIII", "Faulcon DeLacy", null, "Cobra Mk. III", new List<Translation> {new Translation("cobra", "ˈkəʊbrə"), new Translation("Mk.", "mɑːk"), new Translation("III", "θriː") }, "Small", null) },
            { 128672262, new Ship(128672262, "CobraMkIV", "Faulcon DeLacy", null, "Cobra Mk. IV", new List<Translation> {new Translation("cobra", "ˈkəʊbrə"), new Translation("Mk.", "mɑːk"), new Translation("IV", "fɔː") }, "Small", null) },
            { 128671831, new Ship(128671831, "DiamondbackXL", "Lakon Spaceways", new List<Translation> {new Translation("Lakon", "leɪkɒn") }, "Diamondback Explorer", null, "Small", null) },
            { 128671217, new Ship(128671217, "Diamondback", "Lakon Spaceways", new List<Translation> {new Translation("Lakon", "leɪkɒn") }, "Diamondback Scout", null, "Small", null) },
            { 128049291, new Ship(128049291, "Dolphin", "Saud Kruger", new List<Translation> {new Translation("Saud", "saʊd"), new Translation("Kruger", "ˈkruːɡə") }, "Dolphin", null, "Small", null) },
            { 128049255, new Ship(128049255, "Eagle", "Core Dynamics", null, "Eagle", null, "Small", null) },
            { 128672145, new Ship(128672145, "Federation_Dropship_MkII", "Core Dynamics", null, "Federal Assault Ship", null, "Medium", 4) },
            { 128049369, new Ship(128049369, "Federation_Corvette", "Core Dynamics", null, "Federal Corvette", null, "Large", 5) },
            { 128049321, new Ship(128049321, "Federation_Dropship", "Core Dynamics", null, "Federal Dropship", null, "Medium", 4) },
            { 128672152, new Ship(128672152, "Federation_Gunship", "Core Dynamics", null, "Federal Gunship", null, "Medium", 4) },
            { 128049351, new Ship(128049351, "FerDeLance", "Zorgon Peterson", null, "Fer-de-Lance", new List<Translation> {new Translation("fer-de-lance", "ˌfɛədəˈlɑːns") }, "Medium", null) },
            { 128049315, new Ship(128049315, "Empire_Trader", "Gutamaya", new List<Translation> {new Translation("Gutamaya", "guːtəˈmaɪə") }, "Imperial Clipper", null, "Large", 5) },
            { 128671223, new Ship(128671223, "Empire_Courier", "Gutamaya", new List<Translation> {new Translation("Gutamaya", "guːtəˈmaɪə") }, "Imperial Courier", null, "Small", null) },
            { 128049375, new Ship(128049375, "Cutter", "Gutamaya", new List<Translation> {new Translation("Gutamaya", "guːtəˈmaɪə") }, "Imperial Cutter", null, "Large", 5) },
            { 128672138, new Ship(128672138, "Empire_Eagle", "Gutamaya", new List<Translation> {new Translation("Gutamaya", "guːtəˈmaɪə") }, "Imperial Eagle", null, "Small", 2) },
            { 128049261, new Ship(128049261, "Hauler", "Zorgon Peterson", null, "Hauler", null, "Small", null) },
            { 128672269, new Ship(128672269, "Independant_Trader", "Lakon Spaceways", new List<Translation> {new Translation("Lakon", "leɪkɒn") }, "Keelback", null, "Medium", null) },
            { 128049327, new Ship(128049327, "Orca", "Saud Kruger", new List<Translation> {new Translation("Saud", "saʊd"), new Translation("Kruger", "ˈkruːɡə") }, "Orca", null, "Large", null) },
            { 128049339, new Ship(128049339, "Python", "Faulcon DeLacy", null, "Python", null, "Medium", null)},
            { 128049249, new Ship(128049249, "Sidewinder", "Faulcon DeLacy", null, "Sidewinder", null, "Small", null) },
            { 128049285, new Ship(128049285, "Type6", "Lakon Spaceways", new List<Translation> {new Translation("Lakon", "leɪkɒn") }, "Type-6 Transporter", null, "Medium", null) },
            { 128049297, new Ship(128049297, "Type7", "Lakon Spaceways", new List<Translation> {new Translation("Lakon", "leɪkɒn") }, "Type-7 Transporter", null, "Large", null) },
            { 128049333, new Ship(128049333, "Type9", "Lakon Spaceways", new List<Translation> {new Translation("Lakon", "leɪkɒn") }, "Type-9 Heavy", null, "Large", null) },
            { 128049273, new Ship(128049273, "Viper", "Faulcon DeLacy", null, "Viper Mk. III", new List<Translation> {new Translation("viper", "ˈvaɪpə"), new Translation("Mk.", "mɑːk"), new Translation("III", "θriː") }, "Small", 3) },
            { 128672255, new Ship(128672255, "Viper_MkIV", "Faulcon DeLacy", null, "Viper Mk. IV", new List<Translation> {new Translation("viper", "ˈvaɪpə"), new Translation("Mk.", "mɑːk"), new Translation("IV", "fɔː") }, "Small", 3) },
            { 128049309, new Ship(128049309, "Vulture", "Core Dynamics", null, "Vulture", new List<Translation> { new Translation("vulture", "ˈvʌltʃə") }, "Small", 5) },

            // Speculative future ships (replace '9999999xx' with Frontier ID once known)
            { 999999901, new Ship(999999901, "Type10", "Lakon Spaceways", new List<Translation> {new Translation("Lakon", "leɪkɒn") }, "Type-10 Defender", null, "Large", null) },
            { 999999902, new Ship(999999902, "Panther", "Zorgon Peterson", null, "Panther Clipper", null, "Large", null) },
            { 999999903, new Ship(999999903, "AspMkII", "Lakon Spaceways", new List<Translation> {new Translation("Lakon", "leɪkɒn") }, "Asp Mk. II", null, "Medium", null) },
            { 999999904, new Ship(999999904, "Krait", "Faulcon DeLacy", null, "Krait", null, "Small", null) },
        };

        public static List<string> ShipModels = ShipsByEliteID.Select(kp => kp.Value.model).ToList();

        private static Dictionary<string, Ship> ShipsByModel = ShipsByEliteID.ToDictionary(kp => kp.Value.model.ToLowerInvariant(), kp => kp.Value);
        private static Dictionary<string, Ship> ShipsByEDModel = ShipsByEliteID.ToDictionary(kp => kp.Value.EDName.ToLowerInvariant().Replace(" ", "").Replace(".", "").Replace("_", ""), kp => kp.Value);

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
                Ship.phoneticmanufacturer = Template.phoneticmanufacturer;
                Ship.model = Template.model;
                Ship.phoneticmodel = Template.phoneticmodel;
                Ship.size = Template.size;
                Ship.militarysize = Template.militarysize;
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
            if (ShipsByModel.TryGetValue(model.ToLowerInvariant(), out Template))
            {
                Ship.EDID = Template.EDID;
                Ship.EDName = Template.EDName;
                Ship.manufacturer = Template.manufacturer;
                Ship.phoneticmanufacturer = Template.phoneticmanufacturer;
                Ship.model = Template.model;
                Ship.phoneticmodel = Template.phoneticmodel;
                Ship.size = Template.size;
                Ship.militarysize = Template.militarysize;
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
            if (ShipsByEDModel.TryGetValue(model.ToLowerInvariant().Replace(" ", "").Replace(".", "").Replace("_", ""), out Template))
            {
                Ship.EDID = Template.EDID;
                Ship.EDName = Template.EDName;
                Ship.manufacturer = Template.manufacturer;
                Ship.phoneticmanufacturer = Template.phoneticmanufacturer;
                Ship.model = Template.model;
                Ship.phoneticmodel = Template.phoneticmodel;
                Ship.size = Template.size;
                Ship.militarysize = Template.militarysize;
            }
            else
            {
                Ship.model = model;
            }
            return Ship;
        }
    }
}

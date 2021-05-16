﻿using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class ShipDefinitions
    {
        private static readonly Dictionary<long, Ship> ShipsByEliteID = new Dictionary<long, Ship>()
        {
            { 128049267, new Ship(128049267, "Adder", "Zorgon Peterson", "Adder", Properties.Ship.yourAdder, null, LandingPadSize.Small, null, 0.36M) },
            { 128049363, new Ship(128049363, "Anaconda", "Faulcon DeLacy", "Anaconda", Properties.Ship.yourAnaconda, null, LandingPadSize.Large, 5, 1.07M) },
            { 128049303, new Ship(128049303, "Asp", "Lakon Spaceways", "Asp Explorer", Properties.Ship.yourAspEx, null, LandingPadSize.Medium, null, 0.63M) },
            { 128672276, new Ship(128672276, "Asp_Scout", "Lakon Spaceways", "Asp Scout", Properties.Ship.yourAspS, null, LandingPadSize.Medium, null, 0.47M) },
            { 128049345, new Ship(128049345, "BelugaLiner", "Saud Kruger", "Beluga", Properties.Ship.yourBeluga, new List<Translation> {new Translation("beluga", "bɪˈluːɡə") }, LandingPadSize.Large, null, 0.81M) },
            { 128049279, new Ship(128049279, "CobraMkIII", "Faulcon DeLacy", "Cobra Mk. III", Properties.Ship.yourCobraMkIII, new List<Translation> {new Translation("cobra", "ˈkəʊbrə"), new Translation("Mark", "mɑːk"), new Translation("3", "θriː") }, LandingPadSize.Small, null, 0.49M) },
            { 128672262, new Ship(128672262, "CobraMkIV", "Faulcon DeLacy", "Cobra Mk. IV", Properties.Ship.yourCobraMkIV, new List<Translation> {new Translation("cobra", "ˈkəʊbrə"), new Translation("Mark", "mɑːk"), new Translation("4", "fɔː") }, LandingPadSize.Small, null, 0.51M) },
            { 128671831, new Ship(128671831, "DiamondbackXL", "Lakon Spaceways", "Diamondback Explorer", Properties.Ship.yourDBX, null, LandingPadSize.Small, null, 0.52M) },
            { 128671217, new Ship(128671217, "Diamondback", "Lakon Spaceways", "Diamondback Scout", Properties.Ship.yourDBS, null, LandingPadSize.Small, null, 0.49M) },
            { 128049291, new Ship(128049291, "Dolphin", "Saud Kruger", "Dolphin", Properties.Ship.yourDolphin, null, LandingPadSize.Small, null, 0.50M) },
            { 128049255, new Ship(128049255, "Eagle", "Core Dynamics", "Eagle", Properties.Ship.yourEagle, null, LandingPadSize.Small, 2, 0.34M) },
            { 128672145, new Ship(128672145, "Federation_Dropship_MkII", "Core Dynamics", "Federal Assault Ship", Properties.Ship.yourFedAssaultShip, null, LandingPadSize.Medium, 4, 0.72M) },
            { 128049369, new Ship(128049369, "Federation_Corvette", "Core Dynamics", "Federal Corvette", Properties.Ship.yourFedCorvette, null, LandingPadSize.Large, 5, 1.13M) },
            { 128049321, new Ship(128049321, "Federation_Dropship", "Core Dynamics", "Federal Dropship", Properties.Ship.yourFedDropship, null, LandingPadSize.Medium, 4, 0.83M) },
            { 128672152, new Ship(128672152, "Federation_Gunship", "Core Dynamics", "Federal Gunship", Properties.Ship.yourFedGunship, null, LandingPadSize.Medium, 4, 0.82M) },
            { 128049351, new Ship(128049351, "FerDeLance", "Zorgon Peterson", "Fer-de-Lance", Properties.Ship.yourFDL, new List<Translation> {new Translation("fer-de-lance", "ˌfɛədəˈlɑːns") }, LandingPadSize.Medium, null, 0.67M) },
            { 128049315, new Ship(128049315, "Empire_Trader", "Gutamaya", "Imperial Clipper", Properties.Ship.yourImpClipper, null, LandingPadSize.Large, 5, 0.74M) },
            { 128671223, new Ship(128671223, "Empire_Courier", "Gutamaya", "Imperial Courier", Properties.Ship.yourImpCourier, null, LandingPadSize.Small, null, 0.41M) },
            { 128049375, new Ship(128049375, "Cutter", "Gutamaya", "Imperial Cutter", Properties.Ship.yourImpCutter, null, LandingPadSize.Large, 5, 1.16M) },
            { 128672138, new Ship(128672138, "Empire_Eagle", "Gutamaya", "Imperial Eagle", Properties.Ship.yourImpEagle, null, LandingPadSize.Small, 2, 0.37M) },
            { 128049261, new Ship(128049261, "Hauler", "Zorgon Peterson", "Hauler", Properties.Ship.yourHauler, null, LandingPadSize.Small, null, 0.25M) },
            { 128672269, new Ship(128672269, "Independant_Trader", "Lakon Spaceways", "Keelback", Properties.Ship.yourKeelback, null, LandingPadSize.Medium, null, 0.39M) },
            { 128049327, new Ship(128049327, "Orca", "Saud Kruger", "Orca", Properties.Ship.yourOrca, null, LandingPadSize.Large, null, 0.79M) },
            { 128049339, new Ship(128049339, "Python", "Faulcon DeLacy", "Python", Properties.Ship.yourPython, null, LandingPadSize.Medium, null, 0.83M)},
            { 128049249, new Ship(128049249, "Sidewinder", "Faulcon DeLacy", "Sidewinder", Properties.Ship.yourSidewinder, null, LandingPadSize.Small, null, 0.3M) },
            { 128049285, new Ship(128049285, "Type6", "Lakon Spaceways", "Type-6 Transporter", Properties.Ship.yourType6, null, LandingPadSize.Medium, null, 0.39M) },
            { 128049297, new Ship(128049297, "Type7", "Lakon Spaceways", "Type-7 Transporter", Properties.Ship.yourType7, null, LandingPadSize.Large, null, 0.52M) },
            { 128049333, new Ship(128049333, "Type9", "Lakon Spaceways", "Type-9 Heavy", Properties.Ship.yourType9, null, LandingPadSize.Large, null, 0.77M) },
            { 128049273, new Ship(128049273, "Viper", "Faulcon DeLacy", "Viper Mk. III", Properties.Ship.yourViperMkIII, new List<Translation> {new Translation("viper", "ˈvaɪpə"), new Translation("Mark", "mɑːk"), new Translation("3", "θriː") }, LandingPadSize.Small, 3, 0.41M) },
            { 128672255, new Ship(128672255, "Viper_MkIV", "Faulcon DeLacy", "Viper Mk. IV", Properties.Ship.yourViperMkIV, new List<Translation> {new Translation("viper", "ˈvaɪpə"), new Translation("Mark", "mɑːk"), new Translation("4", "fɔː") }, LandingPadSize.Small, 3, 0.46M) },
            { 128049309, new Ship(128049309, "Vulture", "Core Dynamics", "Vulture", Properties.Ship.yourVulture, new List<Translation> { new Translation("vulture", "ˈvʌltʃə") }, LandingPadSize.Small, 5, 0.57M) },
            { 128785619, new Ship(128785619, "Type9_Military", "Lakon Spaceways", "Type-10 Defender", Properties.Ship.yourType10, null, LandingPadSize.Large, 5, 0.77M) },
            { 128816574, new Ship(128816574, "TypeX", "Lakon Spaceways", "Alliance Chieftain", Properties.Ship.yourAllChieftain, null, LandingPadSize.Medium, 4, 0.77M) },
            { 128816581, new Ship(128816581, "TypeX_2", "Lakon Spaceways", "Alliance Crusader", Properties.Ship.yourAllCrusader, null, LandingPadSize.Medium, 4, 0.77M) },
            { 128816588, new Ship(128816588, "TypeX_3", "Lakon Spaceways", "Alliance Challenger", Properties.Ship.yourAllChallenger, null, LandingPadSize.Medium, 4, 0.77M) },
            { 128816567, new Ship(128816567, "Krait_MkII", "Faulcon DeLacy", "Krait Mk. II", Properties.Ship.yourKraitMkII, new List<Translation>{new Translation("Krait", "ˈkreɪt"), new Translation("Mark", "mɑːk"), new Translation("2", "ˈtuː") }, LandingPadSize.Medium, null, 0.63M) },
            { 128839281, new Ship(128839281, "Krait_Light", "Faulcon DeLacy", "Krait Phantom", Properties.Ship.yourPhantom, new List<Translation>{new Translation("Krait", "ˈkreɪt"), new Translation("Phantom", "ˈfæntəm") }, LandingPadSize.Medium, null, 0.63M) },
            { 128915979, new Ship(128915979, "Mamba", "Zorgon Peterson", "Mamba", Properties.Ship.yourMamba, null, LandingPadSize.Medium, null, 0.5M) },
        };

        public static readonly SortedSet<string> ShipModels = new SortedSet<string>(ShipsByEliteID.Select(kp => kp.Value.model));

        private static readonly Dictionary<string, Ship> ShipsByModel = ShipsByEliteID.ToDictionary(kp => kp.Value.model.ToLowerInvariant(), kp => kp.Value);
        private static readonly Dictionary<string, Ship> ShipsByEDModel = ShipsByEliteID.ToDictionary(kp => kp.Value.EDName.ToLowerInvariant().Replace(" ", "").Replace(".", "").Replace("_", ""), kp => kp.Value);
        public static readonly Dictionary<string, List<Translation>> ManufacturerPhoneticNames = new Dictionary<string, List<Translation>>()
        {
            { "Core Dynamics", null },
            { "Faulcon DeLacy", null },
            { "Gutamaya", new List<Translation> {new Translation("Gutamaya", "guːtəˈmaɪə") }},
            { "Lakon Spaceways", new List<Translation> {new Translation("Lakon", "leɪkɒn"), new Translation("Spaceways", "speɪsweɪz") }},
            { "Saud Kruger", new List<Translation> {new Translation("Saud", "saʊd"), new Translation("Kruger", "ˈkruːɡə") }},
            { "Zorgon Peterson", null }
        };

        /// <summary>Obtain details of a ship given its Elite ID</summary>
        public static Ship FromEliteID(long id)
        {
            Ship Ship = new Ship();
            if (ShipsByEliteID.TryGetValue(id, out Ship Template))
            {
                Ship.EDID = Template.EDID;
                Ship.EDName = Template.EDName;
                Ship.manufacturer = Template.manufacturer;
                Ship.possessiveYour = Template.possessiveYour;
                Ship.model = Template.model;
                Ship.phoneticModel = Template.phoneticModel;
                Ship.Size = Template.Size;
                Ship.militarysize = Template.militarysize;
                Ship.activeFuelReservoirCapacity = Template.activeFuelReservoirCapacity;
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
            if (ShipsByModel.TryGetValue(model.ToLowerInvariant(), out Ship Template))
            {
                Ship.EDID = Template.EDID;
                Ship.EDName = Template.EDName;
                Ship.manufacturer = Template.manufacturer;
                Ship.possessiveYour = Template.possessiveYour;
                Ship.model = Template.model;
                Ship.phoneticModel = Template.phoneticModel;
                Ship.Size = Template.Size;
                Ship.militarysize = Template.militarysize;
                Ship.activeFuelReservoirCapacity = Template.activeFuelReservoirCapacity;
            }
            else
            {
                string edModel = null;
                EddbModelMap?.TryGetValue(model, out edModel);
                if (edModel != null)
                {
                    model = edModel;
                }
                Ship = FromEDModel(model, false);
            }
            return Ship;
        }

        /// <summary>Obtain details of a ship given its Elite:Dangerous model</summary>
        public static Ship FromEDModel(string edModel, bool createIfMissing = true)
        {
            if (edModel == null)
            {
                return null;
            }
            Ship Ship = new Ship();
            if (ShipsByEDModel.TryGetValue(edModel.ToLowerInvariant().Replace(" ", "").Replace(".", "").Replace("_", ""), out Ship Template))
            {
                Ship.EDID = Template.EDID;
                Ship.EDName = Template.EDName;
                Ship.manufacturer = Template.manufacturer;
                Ship.possessiveYour = Template.possessiveYour;
                Ship.model = Template.model;
                Ship.phoneticModel = Template.phoneticModel;
                Ship.Size = Template.Size;
                Ship.militarysize = Template.militarysize;
                Ship.activeFuelReservoirCapacity = Template.activeFuelReservoirCapacity;
                return Ship;
            }
            if (createIfMissing)
            {
                Ship.EDName = edModel;
                return Ship;
            }
            return null;
        }

        // EDDB model names may not match either EDDI's model name strings or FDev's EDName strings. 
        private static readonly Dictionary<string, string> EddbModelMap = new Dictionary<string, string>()
        {   
            // Map EDDB model names back to ednames here (format: EDDB model name, EDName)
            { "Adder", "Adder" },
            { "Anaconda", "Anaconda" },
            { "Asp Explorer", "Asp" },
            { "Asp Scout", "Asp_Scout" },
            { "Beluga Liner", "BelugaLiner" },
            { "Cobra Mk. III", "CobraMkIII" },
            { "Cobra MK IV", "CobraMkIV" },
            { "Diamondback Explorer", "DiamondbackXL" },
            { "Diamondback Scout", "Diamondback" },
            { "Dolphin", "Dolphin" },
            { "Eagle Mk. II", "Eagle" },
            { "Federal Assault Ship", "Federation_Dropship_MkII" },
            { "Federal Corvette", "Federation_Corvette" },
            { "Federal Dropship", "Federation_Dropship" },
            { "Federal Gunship", "Federation_Gunship" },
            { "Fer-de-Lance", "FerDeLance" },
            { "Imperial Clipper", "Empire_Trader" },
            { "Imperial Courier", "Empire_Courier" },
            { "Imperial Cutter", "Cutter" },
            { "Imperial Eagle", "Empire_Eagle" },
            { "Hauler", "Hauler" },
            { "Keelback", "Independant_Trader" },
            { "Orca", "Orca" },
            { "Python", "Python" },
            { "Sidewinder Mk. I", "Sidewinder" },
            { "Type-6 Transporter", "Type6" },
            { "Type-7 Transporter", "Type7" },
            { "Type-9 Heavy", "Type9" },
            { "Viper Mk III", "Viper" },
            { "Viper MK IV", "Viper_MkIV" },
            { "Vulture", "Vulture" },
            { "Type-10 Defender", "Type9_Military" },
            { "Alliance Chieftain", "TypeX" },
            { "Alliance Challenger", "TypeX_2" },
            { "Alliance Crusader", "TypeX_3" },
            { "Krait MkII", "Krait_MkII" },
            { "Krait Phantom", "Krait_Light" },
            { "Mamba", "Mamba" },
        };

        public static string SpokenManufacturer(string manufacturer)
        {
            var phoneticmanufacturer = ManufacturerPhoneticNames.FirstOrDefault(m => m.Key == manufacturer).Value;
            if (phoneticmanufacturer != null)
            {
                var result = "";
                foreach (Translation item in phoneticmanufacturer)
                {
                    result += "<phoneme alphabet=\"ipa\" ph=\"" + item.to + "\">" + item.from + "</phoneme> ";
                }
                return result;
            }
            // Model isn't in the dictionary
            return null;
        }
    }
}

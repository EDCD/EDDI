using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public static class ShipDefinitions
    {
        private static readonly List<Ship> AllOfThem = new List<Ship>()
        {
            new Ship( "Adder", ShipManufacturer.ZorgonPeterson, "Adder", nameof(Properties.Ship.yourAdder), null, LandingPadSize.Small, null, 0.36M),
            new Ship( "Anaconda", ShipManufacturer.FaulconDeLacy, "Anaconda", nameof(Properties.Ship.yourAnaconda), null, LandingPadSize.Large, 5, 1.07M),
            new Ship( "Asp", ShipManufacturer.LakonSpaceways, "Asp Explorer", nameof(Properties.Ship.yourAspEx), null, LandingPadSize.Medium, null, 0.63M),
            new Ship( "Asp_Scout", ShipManufacturer.LakonSpaceways, "Asp Scout", nameof(Properties.Ship.yourAspS), null, LandingPadSize.Medium, null, 0.47M),
            new Ship( "BelugaLiner", ShipManufacturer.SaudKruger, "Beluga", nameof(Properties.Ship.yourBeluga), new List<Translation> {new Translation("beluga", "bɪˈluːɡə") }, LandingPadSize.Large, null, 0.81M),
            new Ship( "CobraMkIII", ShipManufacturer.FaulconDeLacy, "Cobra Mk. III", nameof(Properties.Ship.yourCobraMkIII), new List<Translation> {new Translation("cobra", "ˈkəʊbrə"), new Translation("Mark", "mɑːk"), new Translation("3", "θriː") }, LandingPadSize.Small, null, 0.49M),
            new Ship( "CobraMkIV", ShipManufacturer.FaulconDeLacy, "Cobra Mk. IV", nameof(Properties.Ship.yourCobraMkIV), new List<Translation> {new Translation("cobra", "ˈkəʊbrə"), new Translation("Mark", "mɑːk"), new Translation("4", "fɔː") }, LandingPadSize.Small, null, 0.51M),
            new Ship( "DiamondbackXL", ShipManufacturer.LakonSpaceways, "Diamondback Explorer", nameof(Properties.Ship.yourDBX), null, LandingPadSize.Small, null, 0.52M),
            new Ship( "Diamondback", ShipManufacturer.LakonSpaceways, "Diamondback Scout", nameof(Properties.Ship.yourDBS), null, LandingPadSize.Small, null, 0.49M),
            new Ship( "Dolphin", ShipManufacturer.SaudKruger, "Dolphin", nameof(Properties.Ship.yourDolphin), null, LandingPadSize.Small, null, 0.50M),
            new Ship( "Eagle", ShipManufacturer.CoreDynamics, "Eagle", nameof(Properties.Ship.yourEagle), null, LandingPadSize.Small, 2, 0.34M),
            new Ship( "Federation_Dropship_MkII", ShipManufacturer.CoreDynamics, "Federal Assault Ship", nameof(Properties.Ship.yourFedAssaultShip), null, LandingPadSize.Medium, 4, 0.72M),
            new Ship( "Federation_Corvette", ShipManufacturer.CoreDynamics, "Federal Corvette", nameof(Properties.Ship.yourFedCorvette), null, LandingPadSize.Large, 5, 1.13M),
            new Ship( "Federation_Dropship", ShipManufacturer.CoreDynamics, "Federal Dropship", nameof(Properties.Ship.yourFedDropship), null, LandingPadSize.Medium, 4, 0.83M),
            new Ship( "Federation_Gunship", ShipManufacturer.CoreDynamics, "Federal Gunship", nameof(Properties.Ship.yourFedGunship), null, LandingPadSize.Medium, 4, 0.82M),
            new Ship( "FerDeLance", ShipManufacturer.ZorgonPeterson, "Fer-de-Lance", nameof(Properties.Ship.yourFDL), new List<Translation> {new Translation("Fer-de-Lance", "ˌfɛədəˈlɑːns") }, LandingPadSize.Medium, null, 0.67M),
            new Ship( "Empire_Trader", ShipManufacturer.Gutamaya, "Imperial Clipper", nameof(Properties.Ship.yourImpClipper), null, LandingPadSize.Large, 5, 0.74M),
            new Ship( "Empire_Courier", ShipManufacturer.Gutamaya, "Imperial Courier", nameof(Properties.Ship.yourImpCourier), null, LandingPadSize.Small, null, 0.41M),
            new Ship( "Cutter", ShipManufacturer.Gutamaya, "Imperial Cutter", nameof(Properties.Ship.yourImpCutter), null, LandingPadSize.Large, 5, 1.16M),
            new Ship( "Empire_Eagle", ShipManufacturer.Gutamaya, "Imperial Eagle", nameof(Properties.Ship.yourImpEagle), null, LandingPadSize.Small, 2, 0.37M),
            new Ship( "Hauler", ShipManufacturer.ZorgonPeterson, "Hauler", nameof(Properties.Ship.yourHauler), null, LandingPadSize.Small, null, 0.25M),
            new Ship( "Independant_Trader", ShipManufacturer.LakonSpaceways, "Keelback", nameof(Properties.Ship.yourKeelback), null, LandingPadSize.Medium, null, 0.39M),
            new Ship( "Orca", ShipManufacturer.SaudKruger, "Orca", nameof(Properties.Ship.yourOrca), null, LandingPadSize.Large, null, 0.79M),
            new Ship( "Python", ShipManufacturer.FaulconDeLacy, "Python", nameof(Properties.Ship.yourPython), null, LandingPadSize.Medium, null, 0.83M),
            new Ship( "Sidewinder", ShipManufacturer.FaulconDeLacy, "Sidewinder", nameof(Properties.Ship.yourSidewinder), null, LandingPadSize.Small, null, 0.3M),
            new Ship( "Type6", ShipManufacturer.LakonSpaceways, "Type-6 Transporter", nameof(Properties.Ship.yourType6), null, LandingPadSize.Medium, null, 0.39M),
            new Ship( "Type7", ShipManufacturer.LakonSpaceways, "Type-7 Transporter", nameof(Properties.Ship.yourType7), null, LandingPadSize.Large, null, 0.52M),
            new Ship( "Type9", ShipManufacturer.LakonSpaceways, "Type-9 Heavy", nameof(Properties.Ship.yourType9), null, LandingPadSize.Large, null, 0.77M),
            new Ship( "Viper", ShipManufacturer.FaulconDeLacy, "Viper Mk. III", nameof(Properties.Ship.yourViperMkIII), new List<Translation> {new Translation("Viper", "ˈvaɪpə"), new Translation("Mark", "mɑːk"), new Translation("3", "θriː") }, LandingPadSize.Small, 3, 0.41M),
            new Ship( "Viper_MkIV", ShipManufacturer.FaulconDeLacy, "Viper Mk. IV", nameof(Properties.Ship.yourViperMkIV), new List<Translation> {new Translation("Viper", "ˈvaɪpə"), new Translation("Mark", "mɑːk"), new Translation("4", "fɔː") }, LandingPadSize.Small, 3, 0.46M),
            new Ship( "Vulture", ShipManufacturer.CoreDynamics, "Vulture", nameof(Properties.Ship.yourVulture), new List<Translation> { new Translation("vulture", "ˈvʌltʃə") }, LandingPadSize.Small, 5, 0.57M),
            new Ship( "Type9_Military", ShipManufacturer.LakonSpaceways, "Type-10 Defender", nameof(Properties.Ship.yourType10), null, LandingPadSize.Large, 5, 0.77M),
            new Ship( "TypeX", ShipManufacturer.LakonSpaceways, "Alliance Chieftain", nameof(Properties.Ship.yourAllChieftain), null, LandingPadSize.Medium, 4, 0.77M),
            new Ship( "TypeX_2", ShipManufacturer.LakonSpaceways, "Alliance Crusader", nameof(Properties.Ship.yourAllCrusader), null, LandingPadSize.Medium, 4, 0.77M),
            new Ship( "TypeX_3", ShipManufacturer.LakonSpaceways, "Alliance Challenger", nameof(Properties.Ship.yourAllChallenger), null, LandingPadSize.Medium, 4, 0.77M),
            new Ship( "Krait_MkII", ShipManufacturer.FaulconDeLacy, "Krait Mk. II", nameof(Properties.Ship.yourKraitMkII), new List<Translation>{new Translation("Krait", "ˈkreɪt"), new Translation("Mark", "mɑːk"), new Translation("2", "ˈtuː") }, LandingPadSize.Medium, null, 0.63M),
            new Ship( "Krait_Light", ShipManufacturer.FaulconDeLacy, "Krait Phantom", nameof(Properties.Ship.yourPhantom), new List<Translation>{new Translation("Krait", "ˈkreɪt"), new Translation("Phantom", "ˈfæntəm") }, LandingPadSize.Medium, null, 0.63M),
            new Ship( "Mamba", ShipManufacturer.ZorgonPeterson, "Mamba", nameof(Properties.Ship.yourMamba), null, LandingPadSize.Medium, null, 0.5M),
            new Ship( "Python_NX", ShipManufacturer.FaulconDeLacy, "Python Mk. II", nameof(Properties.Ship.yourPython), new List<Translation>{new Translation("Python", "ˈpaɪθən" ), new Translation("Mark", "mɑːk"), new Translation("2", "ˈtuː") }, LandingPadSize.Medium, null, 0.83M),
            new Ship( "type8", ShipManufacturer.LakonSpaceways, "Type-8 Transporter", nameof(Properties.Ship.yourType8), null, LandingPadSize.Medium, null, 0.52M),
        };

        public static readonly SortedSet<string> ShipModels = new SortedSet<string>(AllOfThem.Select(ship => ship.model));
        private static readonly Dictionary<string, Ship> ShipsByModel = AllOfThem.ToDictionary( ship => ship.model.ToLowerInvariant(), ship => ship );
        private static readonly Dictionary<string, Ship> ShipsByEDModel = AllOfThem.ToDictionary( ship => ship.EDName.ToLowerInvariant().Replace(" ", "").Replace(".", "").Replace("_", ""), ship => ship );

        /// <summary>Obtain details of a ship given its model</summary>
        public static Ship FromModel(string model)
        {
            if ( model == null ) { return null; }

            var Ship = new Ship();
            if (ShipsByModel.TryGetValue(model.ToLowerInvariant(), out Ship Template))
            {
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
                Ship = FromEDModel(model, false);
            }
            return Ship;
        }

        /// <summary>Obtain details of a ship given its Elite:Dangerous model</summary>
        public static Ship FromEDModel(string edModel, bool createIfMissing = true)
        {
            if ( edModel == null ) { return null; }

            var Ship = new Ship();
            if (ShipsByEDModel.TryGetValue(edModel.ToLowerInvariant().Replace(" ", "").Replace(".", "").Replace("_", ""), out Ship Template))
            {
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
    }
}

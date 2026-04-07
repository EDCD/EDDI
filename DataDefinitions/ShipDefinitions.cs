using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public static class ShipDefinitions
    {
        private static readonly List<Ship> AllOfThem =
        [
            new( "Adder", Manufacturer.ZorgonPeterson, "Adder", nameof(Properties.Ship.yourAdder), null,
                LandingPadSize.Small, null, 0.36M ),
            new( "Anaconda", Manufacturer.FaulconDeLacy, "Anaconda", nameof(Properties.Ship.yourAnaconda),
                null, LandingPadSize.Large, 5, 1.07M ),
            new( "Asp", Manufacturer.LakonSpaceways, "Asp Explorer", nameof(Properties.Ship.yourAspEx), null,
                LandingPadSize.Medium, null, 0.63M ),
            new( "Asp_Scout", Manufacturer.LakonSpaceways, "Asp Scout", nameof(Properties.Ship.yourAspS), null,
                LandingPadSize.Medium, null, 0.47M ),
            new( "BelugaLiner", Manufacturer.SaudKruger, "Beluga", nameof(Properties.Ship.yourBeluga),
                [ new Translation( "beluga", "bɪˈluːɡə" ) ], LandingPadSize.Large, null, 0.81M ),
            new( "CobraMkIII", Manufacturer.FaulconDeLacy, "Cobra Mk. III",
                nameof(Properties.Ship.yourCobraMkIII),
                [
                    new Translation( "cobra", "ˈkəʊbrə" ),
                    new Translation( "Mark", "mɑːk" ),
                    new Translation( "3", "θriː" )
                ], LandingPadSize.Small, null, 0.49M ),
            new( "CobraMkIV", Manufacturer.FaulconDeLacy, "Cobra Mk. IV",
                nameof(Properties.Ship.yourCobraMkIV),
                [
                    new Translation( "cobra", "ˈkəʊbrə" ),
                    new Translation( "Mark", "mɑːk" ),
                    new Translation( "4", "fɔː" )
                ], LandingPadSize.Small, null, 0.51M ),
            new( "DiamondbackXL", Manufacturer.LakonSpaceways, "Diamondback Explorer",
                nameof(Properties.Ship.yourDBX), null, LandingPadSize.Small, null, 0.52M ),
            new( "Diamondback", Manufacturer.LakonSpaceways, "Diamondback Scout",
                nameof(Properties.Ship.yourDBS), null, LandingPadSize.Small, null, 0.49M ),
            new( "Dolphin", Manufacturer.SaudKruger, "Dolphin", nameof(Properties.Ship.yourDolphin), null,
                LandingPadSize.Small, null, 0.50M ),
            new( "Eagle", Manufacturer.CoreDynamics, "Eagle", nameof(Properties.Ship.yourEagle), null,
                LandingPadSize.Small, 2, 0.34M ),
            new( "Federation_Dropship_MkII", Manufacturer.CoreDynamics, "Federal Assault Ship",
                nameof(Properties.Ship.yourFedAssaultShip), null, LandingPadSize.Medium, 4, 0.72M ),
            new( "Federation_Corvette", Manufacturer.CoreDynamics, "Federal Corvette",
                nameof(Properties.Ship.yourFedCorvette), null, LandingPadSize.Large, 5, 1.13M ),
            new( "Federation_Dropship", Manufacturer.CoreDynamics, "Federal Dropship",
                nameof(Properties.Ship.yourFedDropship), null, LandingPadSize.Medium, 4, 0.83M ),
            new( "Federation_Gunship", Manufacturer.CoreDynamics, "Federal Gunship",
                nameof(Properties.Ship.yourFedGunship), null, LandingPadSize.Medium, 4, 0.82M ),
            new( "FerDeLance", Manufacturer.ZorgonPeterson, "Fer-de-Lance", nameof(Properties.Ship.yourFDL),
                [ new Translation( "Fer-de-Lance", "ˌfɛədəˈlɑːns" ) ], LandingPadSize.Medium,
                null, 0.67M ),
            new( "Empire_Trader", Manufacturer.Gutamaya, "Imperial Clipper",
                nameof(Properties.Ship.yourImpClipper), null, LandingPadSize.Large, 5, 0.74M ),
            new( "Empire_Courier", Manufacturer.Gutamaya, "Imperial Courier",
                nameof(Properties.Ship.yourImpCourier), null, LandingPadSize.Small, null, 0.41M ),
            new( "Cutter", Manufacturer.Gutamaya, "Imperial Cutter", nameof(Properties.Ship.yourImpCutter),
                null, LandingPadSize.Large, 5, 1.16M ),
            new( "Empire_Eagle", Manufacturer.Gutamaya, "Imperial Eagle", nameof(Properties.Ship.yourImpEagle),
                null, LandingPadSize.Small, 2, 0.37M ),
            new( "Hauler", Manufacturer.ZorgonPeterson, "Hauler", nameof(Properties.Ship.yourHauler), null,
                LandingPadSize.Small, null, 0.25M ),
            new( "Independant_Trader", Manufacturer.LakonSpaceways, "Keelback",
                nameof(Properties.Ship.yourKeelback), null, LandingPadSize.Medium, null, 0.39M ),
            new( "Orca", Manufacturer.SaudKruger, "Orca", nameof(Properties.Ship.yourOrca), null,
                LandingPadSize.Large, null, 0.79M ),
            new( "Python", Manufacturer.FaulconDeLacy, "Python", nameof(Properties.Ship.yourPython), null,
                LandingPadSize.Medium, null, 0.83M ),
            new( "Sidewinder", Manufacturer.FaulconDeLacy, "Sidewinder",
                nameof(Properties.Ship.yourSidewinder), null, LandingPadSize.Small, null, 0.3M ),
            new( "Type6", Manufacturer.LakonSpaceways, "Type-6 Transporter", nameof(Properties.Ship.yourType6),
                null, LandingPadSize.Medium, null, 0.39M ),
            new( "Type7", Manufacturer.LakonSpaceways, "Type-7 Transporter", nameof(Properties.Ship.yourType7),
                null, LandingPadSize.Large, null, 0.52M ),
            new( "Type9", Manufacturer.LakonSpaceways, "Type-9 Heavy", nameof(Properties.Ship.yourType9), null,
                LandingPadSize.Large, null, 0.77M ),
            new( "Viper", Manufacturer.FaulconDeLacy, "Viper Mk. III", nameof(Properties.Ship.yourViperMkIII),
            [
                new Translation( "Viper", "ˈvaɪpə" ),
                new Translation( "Mark", "mɑːk" ),
                new Translation( "3", "θriː" )
            ], LandingPadSize.Small, 3, 0.41M ),
            new( "Viper_MkIV", Manufacturer.FaulconDeLacy, "Viper Mk. IV",
                nameof(Properties.Ship.yourViperMkIV),
                [
                    new Translation( "Viper", "ˈvaɪpə" ),
                    new Translation( "Mark", "mɑːk" ),
                    new Translation( "4", "fɔː" )
                ], LandingPadSize.Small, 3, 0.46M ),
            new( "Vulture", Manufacturer.CoreDynamics, "Vulture", nameof(Properties.Ship.yourVulture),
                [ new Translation( "vulture", "ˈvʌltʃə" ) ], LandingPadSize.Small, 5, 0.57M ),
            new( "Type9_Military", Manufacturer.LakonSpaceways, "Type-10 Defender",
                nameof(Properties.Ship.yourType10), null, LandingPadSize.Large, 5, 0.77M ),
            new( "TypeX", Manufacturer.LakonSpaceways, "Alliance Chieftain",
                nameof(Properties.Ship.yourAllChieftain), null, LandingPadSize.Medium, 4, 0.77M ),
            new( "TypeX_2", Manufacturer.LakonSpaceways, "Alliance Crusader",
                nameof(Properties.Ship.yourAllCrusader), null, LandingPadSize.Medium, 4, 0.77M ),
            new( "TypeX_3", Manufacturer.LakonSpaceways, "Alliance Challenger",
                nameof(Properties.Ship.yourAllChallenger), null, LandingPadSize.Medium, 4, 0.77M ),
            new( "Krait_MkII", Manufacturer.FaulconDeLacy, "Krait Mk. II",
                nameof(Properties.Ship.yourKraitMkII),
                [
                    new Translation( "Krait", "ˈkreɪt" ),
                    new Translation( "Mark", "mɑːk" ),
                    new Translation( "2", "ˈtuː" )
                ], LandingPadSize.Medium, null, 0.63M ),
            new( "Krait_Light", Manufacturer.FaulconDeLacy, "Krait Phantom",
                nameof(Properties.Ship.yourPhantom),
                [ new Translation( "Krait", "ˈkreɪt" ), new Translation( "Phantom", "ˈfæntəm" ) ],
                LandingPadSize.Medium, null, 0.63M ),
            new( "Mamba", Manufacturer.ZorgonPeterson, "Mamba", nameof(Properties.Ship.yourMamba), null,
                LandingPadSize.Medium, null, 0.5M ),
            new( "Python_NX", Manufacturer.FaulconDeLacy, "Python Mk. II", nameof(Properties.Ship.yourPython),
            [
                new Translation( "Python", "ˈpaɪθən" ),
                new Translation( "Mark", "mɑːk" ),
                new Translation( "2", "ˈtuː" )
            ], LandingPadSize.Medium, null, 0.83M ),
            new( "type8", Manufacturer.LakonSpaceways, "Type-8 Transporter", nameof(Properties.Ship.yourType8),
                null, LandingPadSize.Medium, null, 0.52M ),
            new( "mandalay", Manufacturer.ZorgonPeterson, "Mandalay", nameof(Properties.Ship.yourMandalay),
                null, LandingPadSize.Medium, null, 0.50M ),
            new( "CobraMkV", Manufacturer.FaulconDeLacy, "Cobra Mk. V", nameof(Properties.Ship.yourCobraMkIV),
            [
                new Translation( "cobra", "ˈkəʊbrə" ),
                new Translation( "Mark", "mɑːk" ),
                new Translation( "5", "faɪv" )
            ], LandingPadSize.Small, null, 0.49M ),
            new( "Corsair", Manufacturer.Gutamaya, "Corsair", nameof(Properties.Ship.yourCorsair),
                [ new Translation( "corsair", "kɔːɹsɛəɹ" ) ], LandingPadSize.Medium, null,
                0.41M ),
            new( "PantherMkII", Manufacturer.ZorgonPeterson, "Panther Clipper Mk. II",
                nameof(Properties.Ship.yourPantherClipperMkII),
                [
                    new Translation( "Panther", "ˈpænθɚ" ),
                    new Translation( "Clipper", "ˈklɪpər" ),
                    new Translation( "Mark", "mɑːk" ),
                    new Translation( "2", "ˈtuː" )
                ], LandingPadSize.Large, null, 1.11M ),
            new( "LakonMiner", Manufacturer.LakonSpaceways, "Type-11 Prospector",
                nameof(Properties.Ship.yourType11), null, LandingPadSize.Medium, null, 0.60M ),
            new( "explorer_nx", Manufacturer.ZorgonPeterson, "Caspian Explorer",
                nameof(Properties.Ship.yourCaspianExplorer), null, LandingPadSize.Large, null, 1.14M ),
            new( "smallcombat01_nx", Manufacturer.CoreDynamics, "Kestrel Mk. II",
                nameof(Properties.Ship.yourKestrelMkII),
                [
                    new Translation( "Kestrel", "ˈkɛstɹəl" ),
                    new Translation( "Mark", "mɑːk" ),
                    new Translation( "2", "ˈtuː" )
                ], LandingPadSize.Small, 4, 0.61M )
        ];

        public static readonly SortedSet<string> ShipModels = new(AllOfThem.Select(ship => ship.model));
        private static readonly Dictionary<string, Ship> ShipsByModel = AllOfThem.ToDictionary( ship => ship.model.ToLowerInvariant(), ship => ship );
        private static readonly Dictionary<string, Ship> ShipsByEDModel = AllOfThem.ToDictionary( ship => ship.EDName.ToLowerInvariant().Replace(" ", "").Replace(".", "").Replace("_", ""), ship => ship );

        /// <summary>Obtain details of a ship given its model</summary>
        public static Ship FromModel(string model)
        {
            if ( model == null ) { return null; }

            var Ship = new Ship();
            if (ShipsByModel.TryGetValue(model.ToLowerInvariant(), out var Template))
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
            if (ShipsByEDModel.TryGetValue(edModel.ToLowerInvariant().Replace(" ", "").Replace(".", "").Replace("_", ""), out var Template))
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

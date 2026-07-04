using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    using static CommodityCategory;

    public class CommodityDefinition : ResourceBasedLocalizedEDName<CommodityDefinition>
    {
        private static readonly Dictionary<long, CommodityDefinition> CommoditiesByEliteID = [ ];

        static CommodityDefinition ()
        {
            resourceManager = Properties.Commodities.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new CommodityDefinition( 0, edname, CommodityCategory.Unknown );

            // 2xxxxxxxx & 3xxxxxxxx series Frontier IDs are placeholders, to use until an actual Frontier ID is identified
            // Check https://github.com/EDCD/FDevIDs (for any undefined FDevID's)
        }

        public static readonly CommodityDefinition Unknown = new(null, "Unknown", CommodityCategory.Unknown, 0, false, false);

        #region Chemicals

        public static readonly CommodityDefinition Water = new(128049166, "Water", Chemicals, 120, false);

        public static readonly CommodityDefinition HydrogenFuel = new(128049202, "HydrogenFuel", Chemicals, 110, false);

        public static readonly CommodityDefinition MineralOil = new(128049203, "MineralOil", Chemicals, 181, false);

        public static readonly CommodityDefinition Explosives = new(128049204, "Explosives", Chemicals, 261, false);

        public static readonly CommodityDefinition Pesticides = new(128049205, "Pesticides", Chemicals, 241, false);

        public static readonly CommodityDefinition DeltaPhoenicisPalms =
            new( 128667073, "DeltaPhoenicisPalms", Chemicals, 8188, true );

        public static readonly CommodityDefinition ToxandjiVirocide = new(128667074, "ToxandjiVirocide", Chemicals, 8275, true);

        public static readonly CommodityDefinition AnduligaFireWorks = new(128667673, "AnduligaFireWorks", Chemicals, 8519, true);

        public static readonly CommodityDefinition HIPOrganophosphates =
            new( 128667682, "HIPOrganophosphates", Chemicals, 8169, true );

        public static readonly CommodityDefinition KorroKungPellets = new(128667684, "KorroKungPellets", Chemicals, 8067, true);

        public static readonly CommodityDefinition SyntheticReagents =
            new( 128672303, "SyntheticReagents", Chemicals, 6675, false );

        public static readonly CommodityDefinition NerveAgents = new(128672304, "NerveAgents", Chemicals, 13526, false);

        public static readonly CommodityDefinition SurfaceStabilisers =
            new( 128672305, "SurfaceStabilisers", Chemicals, 467, false );

        public static readonly CommodityDefinition HydrogenPeroxide = new(128673850, "HydrogenPeroxide", Chemicals, 917, false);

        public static readonly CommodityDefinition LiquidOxygen = new(128673851, "LiquidOxygen", Chemicals, 263, false);

        public static readonly CommodityDefinition RockforthFertiliser =
            new( 128924333, "RockforthFertiliser", Chemicals, 8, false );

        public static readonly CommodityDefinition AgronomicTreatment =
            new( 128924334, "AgronomicTreatment", Chemicals, 3464, false );

        public static readonly CommodityDefinition Tritium = new(128961249, "Tritium", Chemicals, 41684, false);

        #endregion

        #region ConsumerItems

        public static readonly CommodityDefinition DomesticAppliances =
            new( 128049238, "DomesticAppliances", ConsumerItems, 487, false );

        public static readonly CommodityDefinition ConsumerTechnology =
            new( 128049240, "ConsumerTechnology", ConsumerItems, 6769, false );

        public static readonly CommodityDefinition Clothing = new(128049241, "Clothing", ConsumerItems, 285, false);

        public static readonly CommodityDefinition AlacarakmoSkinArt =
            new( 128667036, "AlacarakmoSkinArt", ConsumerItems, 8899, true );

        public static readonly CommodityDefinition EleuThermals = new(128667037, "EleuThermals", ConsumerItems, 8507, true);

        public static readonly CommodityDefinition EshuUmbrellas = new(128667038, "EshuUmbrellas", ConsumerItems, 9343, true);

        public static readonly CommodityDefinition KaretiiCouture = new(128667039, "KaretiiCouture", ConsumerItems, 11582, true);

        public static readonly CommodityDefinition NjangariSaddles = new(128667040, "NjangariSaddles", ConsumerItems, 8356, true);

        public static readonly CommodityDefinition KinagoInstruments =
            new( 128667045, "KinagoInstruments", ConsumerItems, 13030, true );

        public static readonly CommodityDefinition NgunaModernAntiques =
            new( 128667046, "NgunaModernAntiques", ConsumerItems, 8545, true );

        public static readonly CommodityDefinition RajukruStoves = new(128667047, "RajukruStoves", ConsumerItems, 8378, true);

        public static readonly CommodityDefinition TiolceWaste2PasteUnits =
            new( 128667048, "TiolceWaste2PasteUnits", ConsumerItems, 8710, true );

        public static readonly CommodityDefinition OphiuchiExinoArtefacts =
            new( 128667668, "OphiuchiExinoArtefacts", ConsumerItems, 10969, true );

        public static readonly CommodityDefinition HavasupaiDreamCatcher =
            new( 128667680, "HavasupaiDreamCatcher", ConsumerItems, 14639, true );

        public static readonly CommodityDefinition JaradharrePuzzleBox =
            new( 128667683, "JaradharrePuzzleBox", ConsumerItems, 16816, true );

        public static readonly CommodityDefinition UzumokuLowGWings =
            new( 128667696, "UzumokuLowGWings", ConsumerItems, 13845, true );

        public static readonly CommodityDefinition AltairianSkin = new(128667700, "AltairianSkin", ConsumerItems, 8432, true);

        public static readonly CommodityDefinition JotunMookah = new(128667702, "JotunMookah", ConsumerItems, 8780, true);

        public static readonly CommodityDefinition ZeesszeAntGlue = new(128667707, "ZeesszeAntGlue", ConsumerItems, 8161, true);

        public static readonly CommodityDefinition MomusBogSpaniel = new(128667713, "MomusBogSpaniel", ConsumerItems, 9184, true);

        public static readonly CommodityDefinition AlienEggs = new(128667717, "AlienEggs", ConsumerItems, 25067, true);

        public static readonly CommodityDefinition VidavantianLace =
            new( 128667719, "VidavantianLace", ConsumerItems, 12615, true );

        public static readonly CommodityDefinition JaquesQuinentianStill =
            new( 128668017, "JaquesQuinentianStill", ConsumerItems, 2108, true );

        public static readonly CommodityDefinition SoontillRelics = new(128668018, "SoontillRelics", ConsumerItems, 19885, true);

        public static readonly CommodityDefinition Advert1 = // Ultra-Compact Processor Prototypes
            new(128671119, "Advert1", ConsumerItems, 21542, true);

        public static readonly CommodityDefinition TheHuttonMug = new(128672121, "TheHuttonMug", ConsumerItems, 7986, true);

        public static readonly CommodityDefinition EvacuationShelter =
            new( 128672314, "EvacuationShelter", ConsumerItems, 343, false );

        public static readonly CommodityDefinition PersonalGifts = new(128672431, "PersonalGifts", ConsumerItems, 16535, false);

        public static readonly CommodityDefinition CrystallineSpheres =
            new( 128672432, "CrystallineSpheres", ConsumerItems, 12216, true );

        public static readonly CommodityDefinition SurvivalEquipment =
            new( 128682048, "SurvivalEquipment", ConsumerItems, 485, false );

        public static readonly CommodityDefinition ShansCharisOrchid =
            new( 128732551, "ShansCharisOrchid", ConsumerItems, 9043, true );

        public static readonly CommodityDefinition BuckyballBeerMats =
            new( 128748428, "BuckyballBeerMats", ConsumerItems, 7957, true );

        public static readonly CommodityDefinition Duradrives = new(128922524, "Duradrives", ConsumerItems, 19356, true);

        #endregion

        #region Foods

        public static readonly CommodityDefinition Algae = new(128049177, "Algae", Foods, 137, false);

        public static readonly CommodityDefinition FruitAndVegetables = new(128049178, "FruitAndVegetables", Foods, 312, false);

        public static readonly CommodityDefinition Grain = new(128049180, "Grain", Foods, 210, false);

        public static readonly CommodityDefinition Animalmeat = new(128049182, "Animalmeat", Foods, 1292, false);

        public static readonly CommodityDefinition Fish = new(128049183, "Fish", Foods, 406, false);

        public static readonly CommodityDefinition FoodCartridges = new(128049184, "FoodCartridges", Foods, 105, false);

        public static readonly CommodityDefinition SyntheticMeat = new(128049185, "SyntheticMeat", Foods, 271, false);

        public static readonly CommodityDefinition Tea = new(128049188, "Tea", Foods, 1467, false);

        public static readonly CommodityDefinition Coffee = new(128049189, "Coffee", Foods, 1279, false);

        public static readonly CommodityDefinition HIP10175BushMeat = new(128667019, "HIP10175BushMeat", Foods, 9382, true);

        public static readonly CommodityDefinition AlbinoQuechuaMammoth =
            new( 128667020, "AlbinoQuechuaMammoth", Foods, 9687, true );

        public static readonly CommodityDefinition UtgaroarMillenialEggs =
            new( 128667021, "UtgaroarMillenialEggs", Foods, 9163, true );

        public static readonly CommodityDefinition WitchhaulKobeBeef = new(128667022, "WitchhaulKobeBeef", Foods, 11085, true);

        public static readonly CommodityDefinition KarsukiLocusts = new(128667023, "KarsukiLocusts", Foods, 8543, true);

        public static readonly CommodityDefinition GiantIrukamaSnails = new(128667024, "GiantIrukamaSnails", Foods, 9174, true);

        public static readonly CommodityDefinition BaltahSineVacuumKrill =
            new( 128667025, "BaltahSineVacuumKrill", Foods, 8479, true );

        public static readonly CommodityDefinition CetiRabbits = new(128667026, "CetiRabbits", Foods, 9079, true);

        public static readonly CommodityDefinition AnyNaCoffee = new(128667041, "AnyNaCoffee", Foods, 9160, true);

        public static readonly CommodityDefinition CD75CatCoffee = new(128667042, "CD75CatCoffee", Foods, 9571, true);

        public static readonly CommodityDefinition GomanYauponCoffee = new(128667043, "GomanYauponCoffee", Foods, 8921, true);

        public static readonly CommodityDefinition ChiEridaniMarinePaste =
            new( 128667049, "ChiEridaniMarinePaste", Foods, 8450, true );

        public static readonly CommodityDefinition EsusekuCaviar = new(128667050, "EsusekuCaviar", Foods, 9625, true);

        public static readonly CommodityDefinition LiveHecateSeaWorms = new(128667051, "LiveHecateSeaWorms", Foods, 8737, true);

        public static readonly CommodityDefinition HelvetitjPearls = new(128667052, "HelvetitjPearls", Foods, 10450, true);

        public static readonly CommodityDefinition HIP41181Squid = new(128667053, "HIP41181Squid", Foods, 8497, true);

        public static readonly CommodityDefinition CoquimSpongiformVictuals =
            new( 128667054, "CoquimSpongiformVictuals", Foods, 8077, true );

        public static readonly CommodityDefinition AerialEdenApple = new(128667055, "AerialEdenApple", Foods, 8331, true);

        public static readonly CommodityDefinition NeritusBerries = new(128667056, "NeritusBerries", Foods, 8497, true);

        public static readonly CommodityDefinition OchoengChillies = new(128667057, "OchoengChillies", Foods, 8601, true);

        public static readonly CommodityDefinition DeuringasTruffles = new(128667058, "DeuringasTruffles", Foods, 9232, true);

        public static readonly CommodityDefinition HR7221Wheat = new(128667059, "HR7221Wheat", Foods, 8190, true);

        public static readonly CommodityDefinition JarouaRice = new(128667060, "JarouaRice", Foods, 8169, true);

        public static readonly CommodityDefinition SanumaMEAT = new(128667076, "SanumaMEAT", Foods, 8504, true);

        public static readonly CommodityDefinition EthgrezeTeaBuds = new(128667077, "EthgrezeTeaBuds", Foods, 10197, true);

        public static readonly CommodityDefinition CeremonialHeikeTea = new(128667078, "CeremonialHeikeTea", Foods, 9251, true);

        public static readonly CommodityDefinition TanmarkTranquilTea = new(128667079, "TanmarkTranquilTea", Foods, 9177, true);

        public static readonly CommodityDefinition BakedGreebles = new(128667669, "BakedGreebles", Foods, 8211, true);

        public static readonly CommodityDefinition CetiAepyornisEgg = new(128667670, "CetiAepyornisEgg", Foods, 9769, true);

        public static readonly CommodityDefinition HaidneBlackBrew = new(128667679, "HaidneBlackBrew", Foods, 8837, true);

        public static readonly CommodityDefinition LFTVoidExtractCoffee =
            new( 128667685, "LFTVoidExtractCoffee", Foods, 9554, true );

        public static readonly CommodityDefinition LTTHyperSweet = new(128667688, "LTTHyperSweet", Foods, 8054, true);

        public static readonly CommodityDefinition MechucosHighTea = new(128667689, "MechucosHighTea", Foods, 8846, true);

        public static readonly CommodityDefinition MokojingBeastFeast = new(128667691, "MokojingBeastFeast", Foods, 9788, true);

        public static readonly CommodityDefinition MukusubiiChitinOs = new(128667692, "MukusubiiChitinOs", Foods, 8359, true);

        public static readonly CommodityDefinition MulachiGiantFungus = new(128667693, "MulachiGiantFungus", Foods, 7957, true);

        public static readonly CommodityDefinition WheemeteWheatCakes = new(128667698, "WheemeteWheatCakes", Foods, 8081, true);

        public static readonly CommodityDefinition AroucaConventualSweets =
            new( 128667705, "AroucaConventualSweets", Foods, 8737, true );

        public static readonly CommodityDefinition OrrerianViciousBrew = new(128667711, "OrrerianViciousBrew", Foods, 8342, true);

        public static readonly CommodityDefinition UszaianTreeGrub = new(128667712, "UszaianTreeGrub", Foods, 8578, true);

        public static readonly CommodityDefinition DisoMaCorn = new(128667714, "DisoMaCorn", Foods, 8134, true);

        #endregion

        #region IndustrialMaterials

        public static readonly CommodityDefinition CeramicComposites = new( 128672302, "CeramicComposites", IndustrialMaterials, 232, false );

        public static readonly CommodityDefinition CMMComposite = new( 128673856, "CMMComposite", IndustrialMaterials, 3132, false );

        public static readonly CommodityDefinition CoolingHoses = new(128673857, "CoolingHoses", IndustrialMaterials, 403, false);

        public static readonly CommodityDefinition CuratedCommodity = new(129045961, "CuratedCommodity", IndustrialMaterials, 525, false);

        public static readonly CommodityDefinition InsulatingMembrane = new( 128673855, "InsulatingMembrane", IndustrialMaterials, 7837, false );

        public static readonly CommodityDefinition MedbStarlube = new(128667690, "MedbStarlube", IndustrialMaterials, 8191, true);

        public static readonly CommodityDefinition MetaAlloys = new( 128672701, "MetaAlloys", IndustrialMaterials, 88148, false, true );

        public static readonly CommodityDefinition NeofabricInsulation = new( 128673858, "NeofabricInsulation", IndustrialMaterials, 2769, false );

        public static readonly CommodityDefinition Polymers = new(128049197, "Polymers", IndustrialMaterials, 171, false);

        public static readonly CommodityDefinition Semiconductors = new( 128049199, "Semiconductors", IndustrialMaterials, 967, false );

        public static readonly CommodityDefinition Superconductors = new( 128049200, "Superconductors", IndustrialMaterials, 6609, false );

        #endregion

        #region Machinery

        public static readonly CommodityDefinition PowerGenerators = new(128049217, "PowerGenerators", Machinery, 458, false);

        public static readonly CommodityDefinition WaterPurifiers = new(128049218, "WaterPurifiers", Machinery, 258, false);

        public static readonly CommodityDefinition HeliostaticFurnaces =
            new( 128049220, "HeliostaticFurnaces", Machinery, 236, false );

        public static readonly CommodityDefinition MineralExtractors = new(128049221, "MineralExtractors", Machinery, 443, false);

        public static readonly CommodityDefinition CropHarvesters = new(128049222, "CropHarvesters", Machinery, 2021, false);

        public static readonly CommodityDefinition MarineSupplies = new(128049223, "MarineSupplies", Machinery, 3916, false);

        public static readonly CommodityDefinition AtmosphericExtractors =
            new( 128064028, "AtmosphericExtractors", Machinery, 357, false );

        public static readonly CommodityDefinition VolkhabBeeDrones = new(128667044, "VolkhabBeeDrones", Machinery, 10198, true);

        public static readonly CommodityDefinition WulpaHyperboreSystems =
            new( 128667067, "WulpaHyperboreSystems", Machinery, 8726, true );

        public static readonly CommodityDefinition NonEuclidianExotanks =
            new( 128667687, "NonEuclidianExotanks", Machinery, 8526, true );

        public static readonly CommodityDefinition GiantVerrix = new(128667703, "GiantVerrix", Machinery, 12496, true);

        public static readonly CommodityDefinition GeologicalEquipment =
            new( 128672307, "GeologicalEquipment", Machinery, 1661, false );

        public static readonly CommodityDefinition ThermalCoolingUnits =
            new( 128672308, "ThermalCoolingUnits", Machinery, 256, false );

        public static readonly CommodityDefinition BuildingFabricators =
            new( 128672309, "BuildingFabricators", Machinery, 980, false );

        public static readonly CommodityDefinition SkimerComponents = new(128672313, "SkimerComponents", Machinery, 859, false);

        public static readonly CommodityDefinition ArticulationMotors =
            new( 128673859, "ArticulationMotors", Machinery, 4997, false );

        public static readonly CommodityDefinition HNShockMount = new(128673860, "HNShockMount", Machinery, 406, false);

        public static readonly CommodityDefinition EmergencyPowerCells =
            new( 128673861, "EmergencyPowerCells", Machinery, 1011, false );

        public static readonly CommodityDefinition PowerConverter = new(128673862, "PowerConverter", Machinery, 246, false);

        public static readonly CommodityDefinition PowerGridAssembly =
            new( 128673863, "PowerGridAssembly", Machinery, 1684, false );

        public static readonly CommodityDefinition PowerTransferConduits =
            new( 128673864, "PowerTransferConduits", Machinery, 857, false );

        public static readonly CommodityDefinition RadiationBaffle = new(128673865, "RadiationBaffle", Machinery, 383, false);

        public static readonly CommodityDefinition ExhaustManifold = new(128673866, "ExhaustManifold", Machinery, 479, false);

        public static readonly CommodityDefinition ReinforcedMountingPlate =
            new( 128673867, "ReinforcedMountingPlate", Machinery, 1074, false );

        public static readonly CommodityDefinition HeatsinkInterlink = new(128673868, "HeatsinkInterlink", Machinery, 729, false);

        public static readonly CommodityDefinition MagneticEmitterCoil =
            new( 128673869, "MagneticEmitterCoil", Machinery, 199, false );

        public static readonly CommodityDefinition ModularTerminals = new(128673870, "ModularTerminals", Machinery, 695, false);

        #endregion

        #region Medicines

        public static readonly CommodityDefinition AgriculturalMedicines =
            new( 128049208, "AgriculturalMedicines", Medicines, 1038, false );

        public static readonly CommodityDefinition PerformanceEnhancers =
            new( 128049209, "PerformanceEnhancers", Medicines, 6816, false );

        public static readonly CommodityDefinition BasicMedicines = new(128049210, "BasicMedicines", Medicines, 279, false);

        public static readonly CommodityDefinition ProgenitorCells = new(128049669, "ProgenitorCells", Medicines, 6779, false);

        public static readonly CommodityDefinition CombatStabilisers =
            new( 128049670, "CombatStabilisers", Medicines, 3505, false );
        
        public static readonly CommodityDefinition KachiriginLeaches = new(128667027, "KachiriginLeaches", Medicines, 8227, true);
        
        public static readonly CommodityDefinition AganippeRush = new(128667068, "AganippeRush", Medicines, 14220, true);

        public static readonly CommodityDefinition TerraMaterBloodBores =
            new( 128667069, "TerraMaterBloodBores", Medicines, 13414, true );

        public static readonly CommodityDefinition WatersOfShintara = new(128667085, "WatersOfShintara", Medicines, 13711, true);

        public static readonly CommodityDefinition HonestyPills = new(128667686, "HonestyPills", Medicines, 8860, true);

        public static readonly CommodityDefinition VHerculisBodyRub = new(128667697, "VHerculisBodyRub", Medicines, 8010, true);

        public static readonly CommodityDefinition VegaSlimweed = new(128667699, "VegaSlimweed", Medicines, 9588, true);

        public static readonly CommodityDefinition TauriChimes = new(128667706, "TauriChimes", Medicines, 8549, true);

        public static readonly CommodityDefinition PantaaPrayerSticks =
            new( 128667708, "PantaaPrayerSticks", Medicines, 9177, true );

        public static readonly CommodityDefinition FujinTea = new(128667709, "FujinTea", Medicines, 8597, true);

        public static readonly CommodityDefinition AlyaBodilySoap = new(128667718, "AlyaBodilySoap", Medicines, 8218, true);

        public static readonly CommodityDefinition AdvancedMedicines =
            new( 128682046, "AdvancedMedicines", Medicines, 1259, false );

        public static readonly CommodityDefinition Nanomedicines = new(128913661, "Nanomedicines", Medicines, 9859, true);

        #endregion

        #region Metals

        public static readonly CommodityDefinition Platinum = new(128049152, "Platinum", Metals, 19279, false);

        public static readonly CommodityDefinition Palladium = new(128049153, "Palladium", Metals, 13298, false);

        public static readonly CommodityDefinition Gold = new(128049154, "Gold", Metals, 9401, false);

        public static readonly CommodityDefinition Silver = new(128049155, "Silver", Metals, 4775, false);

        public static readonly CommodityDefinition Cobalt = new(128049162, "Cobalt", Metals, 647, false);

        public static readonly CommodityDefinition Beryllium = new(128049168, "Beryllium", Metals, 8288, false);

        public static readonly CommodityDefinition Indium = new(128049169, "Indium", Metals, 5727, false);

        public static readonly CommodityDefinition Gallium = new(128049170, "Gallium", Metals, 5135, false);

        public static readonly CommodityDefinition Tantalum = new(128049171, "Tantalum", Metals, 3962, false);

        public static readonly CommodityDefinition Uranium = new(128049172, "Uranium", Metals, 2705, false);

        public static readonly CommodityDefinition Lithium = new(128049173, "Lithium", Metals, 1596, false);

        public static readonly CommodityDefinition Titanium = new(128049174, "Titanium", Metals, 1006, false);

        public static readonly CommodityDefinition Copper = new(128049175, "Copper", Metals, 481, false);

        public static readonly CommodityDefinition Aluminium = new(128049176, "Aluminium", Metals, 340, false);

        public static readonly CommodityDefinition Hafnium178 = new(128668549, "Hafnium178", Metals, 69098, false);

        public static readonly CommodityDefinition Osmium = new(128671118, "Osmium", Metals, 7591, false);

        public static readonly CommodityDefinition SothisCrystallineGold =
            new( 128672122, "SothisCrystallineGold", Metals, 19112, true );

        public static readonly CommodityDefinition Lanthanum = new(128672298, "Lanthanum", Metals, 8766, false);

        public static readonly CommodityDefinition Thallium = new(128672299, "Thallium", Metals, 3618, false);

        public static readonly CommodityDefinition Bismuth = new(128672300, "Bismuth", Metals, 2284, false);

        public static readonly CommodityDefinition Thorium = new(128672301, "Thorium", Metals, 11513, false);

        public static readonly CommodityDefinition Praseodymium = new(128673845, "Praseodymium", Metals, 7156, false);

        public static readonly CommodityDefinition Samarium = new(128673847, "Samarium", Metals, 6330, false);

        public static readonly CommodityDefinition PlatinumAloy = new(128793114, "PlatinumAloy", Metals, 18333, true);

        public static readonly CommodityDefinition Steel = new(129031238, "Steel", Metals, 4179, false);

        #endregion

        #region Minerals

        public static readonly CommodityDefinition Bertrandite = new(128049156, "Bertrandite", Minerals, 2374, false);

        public static readonly CommodityDefinition Indite = new(128049157, "Indite", Minerals, 2088, false);

        public static readonly CommodityDefinition Gallite = new(128049158, "Gallite", Minerals, 1819, false);

        public static readonly CommodityDefinition Coltan = new(128049159, "Coltan", Minerals, 1319, false);

        public static readonly CommodityDefinition Uraninite = new(128049160, "Uraninite", Minerals, 836, false);

        public static readonly CommodityDefinition Lepidolite = new(128049161, "Lepidolite", Minerals, 544, false);
        
        public static readonly CommodityDefinition Rutile = new(128049163, "Rutile", Minerals, 299, false);

        public static readonly CommodityDefinition Bauxite = new(128049165, "Bauxite", Minerals, 120, false);

        public static readonly CommodityDefinition CherbonesBloodCrystals =
            new( 128667675, "CherbonesBloodCrystals", Minerals, 16714, true );

        public static readonly CommodityDefinition NgadandariFireOpals =
            new( 128667694, "NgadandariFireOpals", Minerals, 19112, true );

        public static readonly CommodityDefinition Painite = new(128668550, "Painite", Minerals, 40508, false);

        public static readonly CommodityDefinition Cryolite = new(128672294, "Cryolite", Minerals, 2266, false);

        public static readonly CommodityDefinition Goslarite = new(128672295, "Goslarite", Minerals, 916, false);

        public static readonly CommodityDefinition Moissanite = new(128672296, "Moissanite", Minerals, 8273, false);

        public static readonly CommodityDefinition Pyrophyllite = new(128672297, "Pyrophyllite", Minerals, 1565, false);

        public static readonly CommodityDefinition Taaffeite = new(128672775, "Taaffeite", Minerals, 20696, false);

        public static readonly CommodityDefinition Jadeite = new(128672776, "Jadeite", Minerals, 13474, false);

        public static readonly CommodityDefinition Bromellite = new(128673846, "Bromellite", Minerals, 7062, false);

        public static readonly CommodityDefinition LowTemperatureDiamond =
            new( 128673848, "LowTemperatureDiamond", Minerals, 57445, false );

        public static readonly CommodityDefinition MethanolMonohydrateCrystals =
            new( 128673852, "MethanolMonohydrateCrystals", Minerals, 2282, false );

        public static readonly CommodityDefinition LithiumHydroxide = new(128673853, "LithiumHydroxide", Minerals, 5646, false);

        public static readonly CommodityDefinition MethaneClathrate = new(128673854, "MethaneClathrate", Minerals, 629, false);

        public static readonly CommodityDefinition Rhodplumsite = new(128924325, "Rhodplumsite", Minerals, 176791, false);

        public static readonly CommodityDefinition Serendibite = new(128924326, "Serendibite", Minerals, 172634, false);

        public static readonly CommodityDefinition Monazite = new(128924327, "Monazite", Minerals, 200925, false);

        public static readonly CommodityDefinition Musgravite = new(128924328, "Musgravite", Minerals, 198527, false);

        public static readonly CommodityDefinition Benitoite = new(128924329, "Benitoite", Minerals, 149325, false);

        public static readonly CommodityDefinition Grandidierite = new(128924330, "Grandidierite", Minerals, 197204, false);

        public static readonly CommodityDefinition Alexandrite = new(128924331, "Alexandrite", Minerals, 217192, false);

        public static readonly CommodityDefinition Opal = new(128924332, "Opal", Minerals, 135218, false);

        public static readonly CommodityDefinition Haematite = new(129031327, "Haematite", Minerals, 1791, false);

        #endregion

        #region Narcotics

        public static readonly CommodityDefinition BasicNarcotics = new(128049212, "BasicNarcotics", Narcotics, 9966, false);

        public static readonly CommodityDefinition Tobacco = new(128049213, "Tobacco", Narcotics, 5035, false);

        public static readonly CommodityDefinition Beer = new(128049214, "Beer", Narcotics, 186, false);

        public static readonly CommodityDefinition Wine = new(128049215, "Wine", Narcotics, 260, false);

        public static readonly CommodityDefinition Liquor = new(128049216, "Liquor", Narcotics, 587, false);

        public static readonly CommodityDefinition EraninPearlWhisky = new(128666746, "EraninPearlWhisky", Narcotics, 9040, true);

        public static readonly CommodityDefinition LavianBrandy = new(128666747, "LavianBrandy", Narcotics, 10365, true);

        public static readonly CommodityDefinition LyraeWeed = new(128667028, "LyraeWeed", Narcotics, 8937, true);

        public static readonly CommodityDefinition OnionHead = new(128667029, "OnionHead", Narcotics, 8437, true);

        public static readonly CommodityDefinition TarachTorSpice = new(128667030, "TarachTorSpice", Narcotics, 8642, true);

        public static readonly CommodityDefinition Wolf1301Fesh = new(128667031, "Wolf1301Fesh", Narcotics, 8399, true);

        public static readonly CommodityDefinition KonggaAle = new(128667034, "KonggaAle", Narcotics, 8310, true);

        public static readonly CommodityDefinition WuthieloKuFroth = new(128667035, "WuthieloKuFroth", Narcotics, 8194, true);

        public static readonly CommodityDefinition BastSnakeGin = new(128667065, "BastSnakeGin", Narcotics, 8659, true);

        public static readonly CommodityDefinition ThrutisCream = new(128667066, "ThrutisCream", Narcotics, 8550, true);

        public static readonly CommodityDefinition KamitraCigars = new(128667081, "KamitraCigars", Narcotics, 12282, true);

        public static readonly CommodityDefinition RusaniOldSmokey = new(128667082, "RusaniOldSmokey", Narcotics, 11994, true);

        public static readonly CommodityDefinition YasoKondiLeaf = new(128667083, "YasoKondiLeaf", Narcotics, 12171, true);

        public static readonly CommodityDefinition ChateauDeAegaeon = new(128667084, "ChateauDeAegaeon", Narcotics, 8791, true);

        public static readonly CommodityDefinition SaxonWine = new(128667671, "SaxonWine", Narcotics, 8983, true);

        public static readonly CommodityDefinition CentauriMegaGin = new(128667672, "CentauriMegaGin", Narcotics, 10217, true);

        public static readonly CommodityDefinition MotronaExperienceJelly =
            new( 128667676, "MotronaExperienceJelly", Narcotics, 13129, true );

        public static readonly CommodityDefinition GeawenDanceDust = new(128667677, "GeawenDanceDust", Narcotics, 8618, true);

        public static readonly CommodityDefinition GerasianGueuzeBeer =
            new( 128667678, "GerasianGueuzeBeer", Narcotics, 8215, true );

        public static readonly CommodityDefinition BurnhamBileDistillate =
            new( 128667681, "BurnhamBileDistillate", Narcotics, 8466, true );

        public static readonly CommodityDefinition PavonisEarGrubs = new(128667701, "PavonisEarGrubs", Narcotics, 8364, true);

        public static readonly CommodityDefinition IndiBourbon = new(128667704, "IndiBourbon", Narcotics, 8806, true);

        public static readonly CommodityDefinition LeestianEvilJuice = new(128667715, "LeestianEvilJuice", Narcotics, 8220, true);

        public static readonly CommodityDefinition BlueMilk = new(128667716, "BlueMilk", Narcotics, 10805, true);

        public static readonly CommodityDefinition TransgenicOnionHead =
            new( 128667760, "TransgenicOnionHead", Narcotics, 8472, true );

        public static readonly CommodityDefinition BootlegLiquor = new(128672306, "BootlegLiquor", Narcotics, 855, false);

        public static readonly CommodityDefinition OnionHeadA = new(128672812, "OnionHeadA", Narcotics, 8437, true);

        public static readonly CommodityDefinition OnionHeadB = new(128673069, "OnionHeadB", Narcotics, 8437, true);

        public static readonly CommodityDefinition AnimalEffigies = new(128727921, "AnimalEffigies", Narcotics, 8399, true);  // Crom Silver Fesh

        public static readonly CommodityDefinition HarmaSilverSeaRum = new(128793113, "HarmaSilverSeaRum", Narcotics, 9762, true);

        public static readonly CommodityDefinition ApaVietii = new(128958679, "ApaVietii", Narcotics, 10362, true);

        public static readonly CommodityDefinition OnionHeadC = new(128983059, "OnionHeadC", Narcotics, 5387, false);

        #endregion

        #region Nonmarketable

        public static readonly CommodityDefinition Drones = new(128066403, "Drones", NonMarketable, 101, false);

        #endregion

        #region Powerplay

        public static readonly CommodityDefinition AislingMediaMaterials =
            new( 128671289, "AislingMediaMaterials", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition AislingMediaResources =
            new( 128671290, "AislingMediaResources", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition AislingPromotionalMaterials =
            new( 128671291, "AislingPromotionalMaterials", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition AllianceTradeAgreements =
            new( 128671292, "AllianceTradeAgreements", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition AllianceLegaslativeContracts =
            new( 128671293, "AllianceLegaslativeContracts", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition AllianceLegaslativeRecords =
            new( 128671294, "AllianceLegaslativeRecords", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition LavignyCorruptionDossiers =
            new( 128671295, "LavignyCorruptionDossiers", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition LavignyFieldSupplies =
            new( 128671296, "LavignyFieldSupplies", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition LavignyGarisonSupplies =
            new( 128671297, "LavignyGarisonSupplies", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition RestrictedPackage = new(128671298, "RestrictedPackage", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition LiberalCampaignMaterials =
            new( 128671300, "LiberalCampaignMaterials", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition FederalAid = new(128671301, "FederalAid", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition FederalTradeContracts =
            new( 128671302, "FederalTradeContracts", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition LoanedArms = new(128671303, "LoanedArms", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition PatreusFieldSupplies =
            new( 128671304, "PatreusFieldSupplies", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition PatreusGarisonSupplies =
            new( 128671305, "PatreusGarisonSupplies", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition RestrictedIntel = new(128671306, "RestrictedIntel", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition RepublicanFieldSupplies =
            new( 128671307, "RepublicanFieldSupplies", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition RepublicanGarisonSupplies =
            new( 128671308, "RepublicanGarisonSupplies", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition SiriusFranchisePackage =
            new( 128671309, "SiriusFranchisePackage", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition SiriusCommercialContracts =
            new( 128671310, "SiriusCommercialContracts", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition SiriusIndustrialEquipment =
            new( 128671311, "SiriusIndustrialEquipment", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition TorvalCommercialContracts =
            new( 128671312, "TorvalCommercialContracts", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition ImperialPrisoner = new(128671313, "ImperialPrisoner", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition UtopianPublicity = new(128671314, "UtopianPublicity", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition UtopianFieldSupplies =
            new( 128671315, "UtopianFieldSupplies", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition UtopianDissident = new(128671316, "UtopianDissident", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition IllicitConsignment = new(128671317, "IllicitConsignment", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition UnmarkedWeapons = new(128671318, "UnmarkedWeapons", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition OnionheadSamples = new(128671319, "OnionheadSamples", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition CounterCultureSupport =
            new( 128671320, "CounterCultureSupport", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition MarkedSlaves = new(128671445, "MarkedSlaves", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition TorvalDeeds = new(128671446, "TorvalDeeds", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition OnionheadDerivatives =
            new( 128671447, "OnionheadDerivatives", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition OutOfDateGoods = new(128671450, "OutOfDateGoods", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition UndergroundSupport = new(128732548, "UndergroundSupport", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition GromCounterIntelligence =
            new( 128732549, "GromCounterIntelligence", Powerplay, 0, false);  // PowerPlay

        public static readonly CommodityDefinition GromWarTrophies = new(128732550, "GromWarTrophies", Powerplay, 0, false);  // PowerPlay

        // Items for which we do not have Elite IDs
        public static readonly CommodityDefinition KaineAidSupplies = new(null, "KaineAidSupplies", Powerplay, 0, false, false);

        public static readonly CommodityDefinition KaineMisinformation =
            new( null, "KaineMisinformation", Powerplay, 0, false, false );

        public static readonly CommodityDefinition KaineLobbyingMaterial =
            new( null, "KaineLobbyingMaterial", Powerplay, 0, false, false );

        public static readonly CommodityDefinition LavignyStrategicReports =
            new( null, "LavignyStrategicReports", Powerplay, 0, false, false );

        #endregion

        #region Salvage

        public static readonly CommodityDefinition USSCargoBlackBox = new(128666752, "USSCargoBlackBox", Salvage, 6995, false);

        public static readonly CommodityDefinition USSCargoTradeData = new(128666754, "USSCargoTradeData", Salvage, 2790, false);

        public static readonly CommodityDefinition USSCargoMilitaryPlans =
            new( 128666755, "USSCargoMilitaryPlans", Salvage, 9413, false );

        public static readonly CommodityDefinition USSCargoAncientArtefact =
            new( 128666756, "USSCargoAncientArtefact", Salvage, 8183, false );

        public static readonly CommodityDefinition USSCargoRareArtwork =
            new( 128666757, "USSCargoRareArtwork", Salvage, 7774, false );

        public static readonly CommodityDefinition USSCargoExperimentalChemicals =
            new( 128666758, "USSCargoExperimentalChemicals", Salvage, 3524, false );

        public static readonly CommodityDefinition USSCargoRebelTransmissions =
            new( 128666759, "USSCargoRebelTransmissions", Salvage, 4068, false );

        public static readonly CommodityDefinition USSCargoPrototypeTech =
            new( 128666760, "USSCargoPrototypeTech", Salvage, 10696, false );

        public static readonly CommodityDefinition USSCargoTechnicalBlueprints =
            new( 128666761, "USSCargoTechnicalBlueprints", Salvage, 6333, false );

        public static readonly CommodityDefinition UnknownArtifact =
            new( 128668547, "UnknownArtifact", Salvage, 290190, false, true );

        public static readonly CommodityDefinition AiRelics = new(128668548, "AiRelics", Salvage, 138613, false);

        public static readonly CommodityDefinition Antiquities = new(128668551, "Antiquities", Salvage, 115511, false);

        public static readonly CommodityDefinition MilitaryIntelligence =
            new( 128668552, "MilitaryIntelligence", Salvage, 55527, false );

        public static readonly CommodityDefinition SAP8CoreContainer = new(128671443, "SAP8CoreContainer", Salvage, 59196, false);

        public static readonly CommodityDefinition TrinketsOfFortune = new(128671444, "TrinketsOfFortune", Salvage, 1428, false);

        public static readonly CommodityDefinition WreckageComponents = new(128672123, "WreckageComponents", Salvage, 394, false);

        public static readonly CommodityDefinition EncriptedDataStorage =
            new( 128672124, "EncriptedDataStorage", Salvage, 806, false );

        public static readonly CommodityDefinition OccupiedCryoPod = new(128672125, "OccupiedCryoPod", Salvage, 4474, false);

        public static readonly CommodityDefinition PersonalEffects = new(128672126, "PersonalEffects", Salvage, 379, false);

        public static readonly CommodityDefinition ComercialSamples = new(128672127, "ComercialSamples", Salvage, 361, false);

        public static readonly CommodityDefinition TacticalData = new(128672128, "TacticalData", Salvage, 457, false);

        public static readonly CommodityDefinition AssaultPlans = new(128672129, "AssaultPlans", Salvage, 446, false);

        public static readonly CommodityDefinition EncryptedCorrespondence =
            new( 128672130, "EncryptedCorrespondence", Salvage, 372, false );

        public static readonly CommodityDefinition DiplomaticBag = new(128672131, "DiplomaticBag", Salvage, 572, false);

        public static readonly CommodityDefinition ScientificResearch = new(128672132, "ScientificResearch", Salvage, 635, false);

        public static readonly CommodityDefinition ScientificSamples = new(128672133, "ScientificSamples", Salvage, 772, false);

        public static readonly CommodityDefinition PoliticalPrisoner = new(128672134, "PoliticalPrisoner", Salvage, 5132, false);

        public static readonly CommodityDefinition Hostage = new(128672135, "Hostage", Salvage, 2427, false);

        public static readonly CommodityDefinition LargeExplorationDataCash =
            new( 128672136, "LargeExplorationDataCash", Salvage, 0, false );

        public static readonly CommodityDefinition SmallExplorationDataCash =
            new( 128672137, "SmallExplorationDataCash", Salvage, 0, false );

        public static readonly CommodityDefinition AntiqueJewellery = new(128672159, "AntiqueJewellery", Salvage, 0, false);

        public static readonly CommodityDefinition PreciousGems = new(128672160, "PreciousGems", Salvage, 109641, false);

        public static readonly CommodityDefinition EarthRelics = new(128672161, "EarthRelics", Salvage, 0, false);

        public static readonly CommodityDefinition GeneBank = new(128672162, "GeneBank", Salvage, 0, false);

        public static readonly CommodityDefinition TimeCapsule = new(128672163, "TimeCapsule", Salvage, 0, false);

        public static readonly CommodityDefinition GeologicalSamples = new(128672315, "GeologicalSamples", Salvage, 446, false);

        public static readonly CommodityDefinition UnstableDataCore = new(128672810, "UnstableDataCore", Salvage, 2427, false);

        public static readonly CommodityDefinition DamagedEscapePod = new(128672811, "DamagedEscapePod", Salvage, 11912, false);

        public static readonly CommodityDefinition UnknownArtifact2 =
            new( 128673876, "UnknownArtifact2", Salvage, 411003, false, true );

        public static readonly CommodityDefinition DataCore = new(128682049, "DataCore", Salvage, 2872, false);

        public static readonly CommodityDefinition GalacticTravelGuide =
            new( 128682050, "GalacticTravelGuide", Salvage, 332, false );

        public static readonly CommodityDefinition MysteriousIdol = new(128682051, "MysteriousIdol", Salvage, 15196, false);

        public static readonly CommodityDefinition ProhibitedResearchMaterials =
            new( 128682052, "ProhibitedResearchMaterials", Salvage, 46607, false );

        public static readonly CommodityDefinition AntimatterContainmentUnit =
            new( 128682053, "AntimatterContainmentUnit", Salvage, 26608, false );

        public static readonly CommodityDefinition SpacePioneerRelics =
            new( 128682054, "SpacePioneerRelics", Salvage, 7342, false );

        public static readonly CommodityDefinition FossilRemnants = new(128682055, "FossilRemnants", Salvage, 9927, false);

        public static readonly CommodityDefinition AncientRelic = new(128732183, "AncientRelic", Salvage, 24962, false);

        public static readonly CommodityDefinition AncientOrb = new(128732184, "AncientOrb", Salvage, 17415, false);

        public static readonly CommodityDefinition AncientCasket = new(128732185, "AncientCasket", Salvage, 16294, false);

        public static readonly CommodityDefinition AncientTablet = new(128732186, "AncientTablet", Salvage, 17415, false);

        public static readonly CommodityDefinition AncientUrn = new(128732187, "AncientUrn", Salvage, 14907, false);

        public static readonly CommodityDefinition AncientTotem = new(128732188, "AncientTotem", Salvage, 20437, false);

        public static readonly CommodityDefinition UnknownResin = new(128737287, "UnknownResin", Salvage, 18652, false, true);

        public static readonly CommodityDefinition UnknownBiologicalMatter =
            new( 128737288, "UnknownBiologicalMatter", Salvage, 25479, false, true );

        public static readonly CommodityDefinition UnknownTechnologySamples =
            new( 128737289, "UnknownTechnologySamples", Salvage, 22551, false, true );

        public static readonly CommodityDefinition UnknownArtifact3 =
            new( 128740752, "UnknownArtifact3", Salvage, 31350, false, true );

        public static readonly CommodityDefinition ThargoidHeart = new(128793127, "ThargoidHeart", Salvage, 0, false, true);

        public static readonly CommodityDefinition ThargoidTissueSampleType1 =
            new( 128793128, "ThargoidTissueSampleType1", Salvage, 14081, false, true );

        public static readonly CommodityDefinition ThargoidTissueSampleType2 =
            new( 128793129, "ThargoidTissueSampleType2", Salvage, 0, false, true );

        public static readonly CommodityDefinition ThargoidTissueSampleType3 =
            new( 128793130, "ThargoidTissueSampleType3", Salvage, 0, false, true );

        public static readonly CommodityDefinition ThargoidScoutTissueSample =
            new( 128824468, "ThargoidScoutTissueSample", Salvage, 15215, false, true );

        public static readonly CommodityDefinition AncientKey = new(128888499, "AncientKey", Salvage, 29931, false);

        public static readonly CommodityDefinition ThargoidTissueSampleType4 =
            new( 128902652, "ThargoidTissueSampleType4", Salvage, 0, false, true );

        public static readonly CommodityDefinition M_TissueSample_Fluid =
            new( 128922517, "M_TissueSample_Fluid", Salvage, 0, false );

        public static readonly CommodityDefinition M_TissueSample_Soft = new(128922518, "M_TissueSample_Soft", Salvage, 0, false);

        public static readonly CommodityDefinition M_TissueSample_Nerves =
            new( 128922519, "M_TissueSample_Nerves", Salvage, 0, false );

        public static readonly CommodityDefinition S_TissueSample_Cells =
            new( 128922520, "S_TissueSample_Cells", Salvage, 0, false );

        public static readonly CommodityDefinition S_TissueSample_Surface =
            new( 128922521, "S_TissueSample_Surface", Salvage, 0, false );

        public static readonly CommodityDefinition S_TissueSample_Core = new(128922522, "S_TissueSample_Core", Salvage, 0, false);

        public static readonly CommodityDefinition P_ParticulateSample = new(128922523, "P_ParticulateSample", Salvage, 0, false);

        public static readonly CommodityDefinition S9_TissueSample_Shell =
            new( 128922781, "S9_TissueSample_Shell", Salvage, 0, false );

        public static readonly CommodityDefinition M3_TissueSample_Membrane =
            new( 128922782, "M3_TissueSample_Membrane", Salvage, 0, false );

        public static readonly CommodityDefinition M3_TissueSample_Mycelium =
            new( 128922783, "M3_TissueSample_Mycelium", Salvage, 0, false );

        public static readonly CommodityDefinition M3_TissueSample_Spores =
            new( 128922784, "M3_TissueSample_Spores", Salvage, 0, false );

        public static readonly CommodityDefinition S6_TissueSample_Mesoglea =
            new( 128922785, "S6_TissueSample_Mesoglea", Salvage, 0, false );

        public static readonly CommodityDefinition S6_TissueSample_Cells =
            new( 128922786, "S6_TissueSample_Cells", Salvage, 0, false );

        public static readonly CommodityDefinition S6_TissueSample_Coenosarc =
            new( 128922787, "S6_TissueSample_Coenosarc", Salvage, 0, false );

        public static readonly CommodityDefinition AncientRelicTG = new(129015433, "AncientRelicTG", Salvage, 4798, false);

        public static readonly CommodityDefinition ThargoidTissueSampleType5 =
            new( 129019258, "ThargoidTissueSampleType5", Salvage, 98368, false, true );

        public static readonly CommodityDefinition ThargoidGeneratorTissueSample =
            new( 129019259, "ThargoidGeneratorTissueSample", Salvage, 67680, false, true );

        public static readonly CommodityDefinition UnocuppiedEscapePod =
            new( 129022087, "UnocuppiedEscapePod", Salvage, 3900, false );

        public static readonly CommodityDefinition UnknownMineral = new(129022408, "UnknownMineral", Salvage, 31986, false, true);

        public static readonly CommodityDefinition UnknownRefinedMineral =
            new( 129022409, "UnknownRefinedMineral", Salvage, 158421, false, true );

        // Items for which we do not have pricing
        public static readonly CommodityDefinition ThargoidTissueSampleType6 =
            new( 129022395, "ThargoidTissueSampleType6", Salvage, 0, false, true );

        public static readonly CommodityDefinition ThargoidTissueSampleType7 =
            new( 129022396, "ThargoidTissueSampleType7", Salvage, 0, false, true );

        public static readonly CommodityDefinition ThargoidTissueSampleType9a =
            new( 129022398, "ThargoidTissueSampleType9a", Salvage, 0, false, true );

        public static readonly CommodityDefinition ThargoidTissueSampleType9b =
            new( 129022399, "ThargoidTissueSampleType9b", Salvage, 0, false, true );

        public static readonly CommodityDefinition ThargoidTissueSampleType9c =
            new( 129022400, "ThargoidTissueSampleType9c", Salvage, 0, false, true );

        public static readonly CommodityDefinition ThargoidTissueSampleType10a =
            new( 129022402, "ThargoidTissueSampleType10a", Salvage, 0, false, true );

        public static readonly CommodityDefinition ThargoidTissueSampleType10b =
            new( 129022403, "ThargoidTissueSampleType10b", Salvage, 0, false, true );

        public static readonly CommodityDefinition ThargoidTissueSampleType10c =
            new( 129022404, "ThargoidTissueSampleType10c", Salvage, 0, false, true );

        public static readonly CommodityDefinition UnknownSack = new(129022405, "UnknownSack", Salvage, 0, false, true);

        public static readonly CommodityDefinition ThargoidPod = new(129022406, "ThargoidPod", Salvage, 0, false);

        public static readonly CommodityDefinition CoralSap = new(129022407, "CoralSap", Salvage, 0, false, true);

        public static readonly CommodityDefinition ThargoidTitanDriveComponent =
            new( 129030459, "ThargoidTitanDriveComponent", Salvage, 0, false, true );

        public static readonly CommodityDefinition ThargoidCystSpecimen =
            new( 129030460, "ThargoidCystSpecimen", Salvage, 0, false, true );

        public static readonly CommodityDefinition ThargoidBoneFragments =
            new( 129030461, "ThargoidBoneFragments", Salvage, 0, false, true );

        public static readonly CommodityDefinition ThargoidOrganSample =
            new( 129030462, "ThargoidOrganSample", Salvage, 0, false, true );

        #endregion

        #region Slaves

        public static readonly CommodityDefinition Slaves = new(128049243, "Slaves", CommodityCategory.Slaves, 10584, false);

        public static readonly CommodityDefinition ImperialSlaves =
            new( 128667728, "ImperialSlaves", CommodityCategory.Slaves, 15984, false );

        public static readonly CommodityDefinition MasterChefs =
            new( 128672316, "MasterChefs", CommodityCategory.Slaves, 20590, true );

        #endregion

        #region Technology

        public static readonly CommodityDefinition ComputerComponents =
            new( 128049225, "ComputerComponents", Technology, 513, false );

        public static readonly CommodityDefinition HazardousEnvironmentSuits =
            new( 128049226, "HazardousEnvironmentSuits", Technology, 340, false );

        public static readonly CommodityDefinition Robotics = new(128049227, "Robotics", Technology, 1856, false);

        public static readonly CommodityDefinition AutoFabricators = new(128049228, "AutoFabricators", Technology, 3734, false);

        public static readonly CommodityDefinition AnimalMonitors = new(128049229, "AnimalMonitors", Technology, 324, false);

        public static readonly CommodityDefinition AquaponicSystems = new(128049230, "AquaponicSystems", Technology, 314, false);

        public static readonly CommodityDefinition AdvancedCatalysers =
            new( 128049231, "AdvancedCatalysers", Technology, 2947, false );

        public static readonly CommodityDefinition TerrainEnrichmentSystems =
            new( 128049232, "TerrainEnrichmentSystems", Technology, 4887, false );

        public static readonly CommodityDefinition ResonatingSeparators =
            new( 128049671, "ResonatingSeparators", Technology, 5958, false );

        public static readonly CommodityDefinition BioReducingLichen =
            new( 128049672, "BioReducingLichen", Technology, 998, false );

        public static readonly CommodityDefinition XiheCompanions = new(128667075, "XiheCompanions", Technology, 11058, true);

        public static readonly CommodityDefinition AZCancriFormula42 =
            new( 128667080, "AZCancriFormula42", Technology, 12440, true );

        public static readonly CommodityDefinition MuTomImager = new(128672310, "MuTomImager", Technology, 6353, false);

        public static readonly CommodityDefinition StructuralRegulators =
            new( 128672311, "StructuralRegulators", Technology, 1791, false );

        public static readonly CommodityDefinition Nanobreakers = new(128673871, "Nanobreakers", Technology, 639, false);

        public static readonly CommodityDefinition TelemetrySuite = new(128673872, "TelemetrySuite", Technology, 2080, false);

        public static readonly CommodityDefinition MicroControllers = new(128673873, "MicroControllers", Technology, 3274, false);

        public static readonly CommodityDefinition IonDistributor = new(128673874, "IonDistributor", Technology, 1133, false);

        public static readonly CommodityDefinition DiagnosticSensor = new(128673875, "DiagnosticSensor", Technology, 4337, false);

        public static readonly CommodityDefinition MedicalDiagnosticEquipment =
            new( 128682047, "MedicalDiagnosticEquipment", Technology, 2848, false );

        public static readonly CommodityDefinition ClassifiedExperimentalEquipment =
            new( 129002574, "ClassifiedExperimentalEquipment", Technology, 0, true );

        #endregion

        #region Textiles

        public static readonly CommodityDefinition Leather = new(128049190, "Leather", Textiles, 205, false);

        public static readonly CommodityDefinition NaturalFabrics = new(128049191, "NaturalFabrics", Textiles, 439, false);

        public static readonly CommodityDefinition SyntheticFabrics = new(128049193, "SyntheticFabrics", Textiles, 211, false);

        public static readonly CommodityDefinition BelalansRayLeather =
            new( 128667061, "BelalansRayLeather", Textiles, 8519, true );

        public static readonly CommodityDefinition DamnaCarapaces = new(128667062, "DamnaCarapaces", Textiles, 8120, true);

        public static readonly CommodityDefinition RapaBaoSnakeSkins = new(128667063, "RapaBaoSnakeSkins", Textiles, 8285, true);

        public static readonly CommodityDefinition VanayequiRhinoFur = new(128667064, "VanayequiRhinoFur", Textiles, 8331, true);

        public static readonly CommodityDefinition BankiAmphibiousLeather =
            new( 128667674, "BankiAmphibiousLeather", Textiles, 8338, true );

        public static readonly CommodityDefinition TiegfriesSynthSilk =
            new( 128667695, "TiegfriesSynthSilk", Textiles, 8478, true );

        public static readonly CommodityDefinition ChameleonCloth = new(128667710, "ChameleonCloth", Textiles, 9071, true);

        public static readonly CommodityDefinition ConductiveFabrics = new(128682044, "ConductiveFabrics", Textiles, 507, false);

        public static readonly CommodityDefinition MilitaryGradeFabrics =
            new( 128682045, "MilitaryGradeFabrics", Textiles, 708, false );

        #endregion

        #region Waste

        public static readonly CommodityDefinition Biowaste = new(128049244, "Biowaste", Waste, 63, false);

        public static readonly CommodityDefinition ToxicWaste = new(128049245, "ToxicWaste", Waste, 287, false);

        public static readonly CommodityDefinition ChemicalWaste = new(128049246, "ChemicalWaste", Waste, 131, false);

        public static readonly CommodityDefinition Scrap = new(128049248, "Scrap", Waste, 48, false);

        #endregion

        #region Weapons

        public static readonly CommodityDefinition PersonalWeapons = new(128049233, "PersonalWeapons", Weapons, 4632, false);

        public static readonly CommodityDefinition BattleWeapons = new(128049234, "BattleWeapons", Weapons, 7259, false);

        public static readonly CommodityDefinition ReactiveArmour = new(128049235, "ReactiveArmour", Weapons, 2113, false);

        public static readonly CommodityDefinition NonLethalWeapons = new(128049236, "NonLethalWeapons", Weapons, 1837, false);

        public static readonly CommodityDefinition BorasetaniPathogenetics =
            new( 128667032, "BorasetaniPathogenetics", Weapons, 13679, true );

        public static readonly CommodityDefinition HIP118311Swarm = new(128667033, "HIP118311Swarm", Weapons, 13448, true);
        
        public static readonly CommodityDefinition HolvaDuellingBlades =
            new( 128667070, "HolvaDuellingBlades", Weapons, 12493, true );

        public static readonly CommodityDefinition KamorinHistoricWeapons =
            new( 128667071, "KamorinHistoricWeapons", Weapons, 9766, true );

        public static readonly CommodityDefinition GilyaSignatureWeapons =
            new( 128667072, "GilyaSignatureWeapons", Weapons, 13038, true );

        public static readonly CommodityDefinition Landmines = new(128672312, "Landmines", Weapons, 4602, false);

        #endregion

        [ Utilities.PublicAPI, JsonProperty( "category" ) ]
        public readonly CommodityCategory Category;

        public string category => Category.localizedName;

        [ Utilities.PublicAPI( "True if the commodity is a rare market commodity" ) ]
        public readonly bool rare;

        [ Utilities.PublicAPI( "True if the commodity is known to be corrosive" ) ]
        public readonly bool corrosive;

        // The average price of a commodity can change - thus this cannot be read only.
        // Instead, this value should be updated whenever revised data is received.
        [ Utilities.PublicAPI( "The latest known average market price for the commodity" ) ]
        public decimal avgprice { get; set; }

        // Not intended to be user facing

        public readonly long? EliteID;

        // dummy used to ensure that the static constructor has run
        public CommodityDefinition () : this( 0, "", CommodityCategory.Unknown )
        { }

        internal CommodityDefinition ( long? EliteID, string edname, CommodityCategory Category, int AveragePrice = 0,
            bool Rare = false, bool Corrosive = false ) : base( edname, edname )
        {
            this.EliteID = EliteID;
            this.Category = Category;
            this.avgprice = AveragePrice;
            this.rare = Rare;
            this.corrosive = Corrosive;
            if ( EliteID != null )
            {
                CommoditiesByEliteID[ (long)EliteID ] = this;
            }
        }

        public static CommodityDefinition CommodityDefinitionFromEliteID ( long id, string edName = null )
        {
            if ( CommoditiesByEliteID.TryGetValue( id, out var commodityDefinition ) )
            {
                return commodityDefinition;
            }

            Logging.Error( $"Unrecognized Commodity Definition EliteID {id} for {edName ?? "<Unknown> EDName"}" );
            return null;
        }

        private static string NormalizedName ( string rawName )
        {
            return rawName?.ToLowerInvariant()
                ?.Replace( "$", "" ) // Header for types from mining and mission events
                ?.Replace( "_name;", "" ) // Trailer for types from mining and mission events
                ?.Replace( "name;", "" );
        }

        public static CommodityDefinition FromNameOrEDName ( string name )
        {
            if ( string.IsNullOrEmpty( name ) ) { return null; }

            var normalizedName = NormalizedName( name );

            if ( ignoredCommodity( normalizedName ) )
            {
                return null;
            }

            // Correct ednames that we've gotten wrong sometime in the past
            normalizedName = correctedCommodityEdName( normalizedName );

            // Now try to fetch the commodity by either ED or real name
            CommodityDefinition result = null;
            if ( normalizedName != null )
            {
                result = FromName( normalizedName );
            }

            result ??= ResourceBasedLocalizedEDName<CommodityDefinition>.FromEDName( normalizedName );

            return result;
        }

        private static bool ignoredCommodity ( string name )
        {
            return name switch
            {
                "legal drugs" => true, // This is a commodity category but in some older sql databases it is listed as a commodity
                _ => false
            };
        }

        private static string correctedCommodityEdName ( string name )
        {
            switch ( name )
            {
                case "sanumadecorativemeat": { return "sanumameat"; }
                case "wolffesh": { return "wolf1301fesh"; }
                case "edenapplesofaerial": { return "aerialedenapple"; }
                case "uzumokulow-gwings": { return "uzumokulowgwings"; }
                default: { return name; }
            }
        }

        [CanBeNull]
        public static new CommodityDefinition FromEDName ( string rawName )
        {
            if ( string.IsNullOrEmpty( rawName ) ) { return null; }
            var edName = NormalizedName( rawName );
            return ResourceBasedLocalizedEDName<CommodityDefinition>.FromEDName( edName );
        }

        public static bool EDNameExists ( string edName )
        {
            if ( string.IsNullOrEmpty( edName ) ) { return false; }
            return AllOfThem.Any( v =>
                string.Equals( v.edname, tidiedEDName( edName ), StringComparison.InvariantCultureIgnoreCase ) );
        }

        private static string tidiedEDName ( string edName )
        {
            return edName?.ToLowerInvariant().Replace( "$", "" ).Replace( ";", "" ).Replace( "_name", "" );
        }
    }
}

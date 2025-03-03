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
        private static readonly Dictionary<long, CommodityDefinition> CommoditiesByEliteID =
            new Dictionary<long, CommodityDefinition>();

        static CommodityDefinition ()
        {
            resourceManager = Properties.Commodities.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = ( edname ) => new CommodityDefinition( 0, edname, CommodityCategory.Unknown );

            // 2xxxxxxxx & 3xxxxxxxx series Frontier IDs are placeholders, to use until an actual Frontier ID is identified
            // Check https://github.com/EDCD/FDevIDs (for any undefined FDevID's)
        }

        public static CommodityDefinition Unknown =
            new CommodityDefinition( null, "Unknown", CommodityCategory.Unknown, 0, false, false );

        #region Chemicals

        public static CommodityDefinition Water = 
            new CommodityDefinition( 128049166, "Water", Chemicals, 120, false );

        public static CommodityDefinition HydrogenFuel =
            new CommodityDefinition( 128049202, "HydrogenFuel", Chemicals, 110, false );

        public static CommodityDefinition MineralOil =
            new CommodityDefinition( 128049203, "MineralOil", Chemicals, 181, false );

        public static CommodityDefinition Explosives =
            new CommodityDefinition( 128049204, "Explosives", Chemicals, 261, false );

        public static CommodityDefinition Pesticides =
            new CommodityDefinition( 128049205, "Pesticides", Chemicals, 241, false );

        public static CommodityDefinition DeltaPhoenicisPalms =
            new CommodityDefinition( 128667073, "DeltaPhoenicisPalms", Chemicals, 8188, true );

        public static CommodityDefinition ToxandjiVirocide =
            new CommodityDefinition( 128667074, "ToxandjiVirocide", Chemicals, 8275, true );

        public static CommodityDefinition AnduligaFireWorks =
            new CommodityDefinition( 128667673, "AnduligaFireWorks", Chemicals, 8519, true );

        public static CommodityDefinition HIPOrganophosphates =
            new CommodityDefinition( 128667682, "HIPOrganophosphates", Chemicals, 8169, true );

        public static CommodityDefinition KorroKungPellets =
            new CommodityDefinition( 128667684, "KorroKungPellets", Chemicals, 8067, true );

        public static CommodityDefinition SyntheticReagents =
            new CommodityDefinition( 128672303, "SyntheticReagents", Chemicals, 6675, false );

        public static CommodityDefinition NerveAgents =
            new CommodityDefinition( 128672304, "NerveAgents", Chemicals, 13526, false );

        public static CommodityDefinition SurfaceStabilisers =
            new CommodityDefinition( 128672305, "SurfaceStabilisers", Chemicals, 467, false );

        public static CommodityDefinition HydrogenPeroxide =
            new CommodityDefinition( 128673850, "HydrogenPeroxide", Chemicals, 917, false );

        public static CommodityDefinition LiquidOxygen =
            new CommodityDefinition( 128673851, "LiquidOxygen", Chemicals, 263, false );

        public static CommodityDefinition RockforthFertiliser =
            new CommodityDefinition( 128924333, "RockforthFertiliser", Chemicals, 8, false );

        public static CommodityDefinition AgronomicTreatment =
            new CommodityDefinition( 128924334, "AgronomicTreatment", Chemicals, 3464, false );

        public static CommodityDefinition Tritium =
            new CommodityDefinition( 128961249, "Tritium", Chemicals, 41684, false );

        #endregion

        #region ConsumerItems

        public static CommodityDefinition DomesticAppliances =
            new CommodityDefinition( 128049238, "DomesticAppliances", ConsumerItems, 487, false );

        public static CommodityDefinition ConsumerTechnology =
            new CommodityDefinition( 128049240, "ConsumerTechnology", ConsumerItems, 6769, false );

        public static CommodityDefinition Clothing =
            new CommodityDefinition( 128049241, "Clothing", ConsumerItems, 285, false );

        public static CommodityDefinition AlacarakmoSkinArt =
            new CommodityDefinition( 128667036, "AlacarakmoSkinArt", ConsumerItems, 8899, true );

        public static CommodityDefinition EleuThermals =
            new CommodityDefinition( 128667037, "EleuThermals", ConsumerItems, 8507, true );

        public static CommodityDefinition EshuUmbrellas =
            new CommodityDefinition( 128667038, "EshuUmbrellas", ConsumerItems, 9343, true );

        public static CommodityDefinition KaretiiCouture =
            new CommodityDefinition( 128667039, "KaretiiCouture", ConsumerItems, 11582, true );

        public static CommodityDefinition NjangariSaddles =
            new CommodityDefinition( 128667040, "NjangariSaddles", ConsumerItems, 8356, true );

        public static CommodityDefinition KinagoInstruments =
            new CommodityDefinition( 128667045, "KinagoInstruments", ConsumerItems, 13030, true );

        public static CommodityDefinition NgunaModernAntiques =
            new CommodityDefinition( 128667046, "NgunaModernAntiques", ConsumerItems, 8545, true );

        public static CommodityDefinition RajukruStoves =
            new CommodityDefinition( 128667047, "RajukruStoves", ConsumerItems, 8378, true );

        public static CommodityDefinition TiolceWaste2PasteUnits =
            new CommodityDefinition( 128667048, "TiolceWaste2PasteUnits", ConsumerItems, 8710, true );

        public static CommodityDefinition OphiuchiExinoArtefacts =
            new CommodityDefinition( 128667668, "OphiuchiExinoArtefacts", ConsumerItems, 10969, true );

        public static CommodityDefinition HavasupaiDreamCatcher =
            new CommodityDefinition( 128667680, "HavasupaiDreamCatcher", ConsumerItems, 14639, true );

        public static CommodityDefinition JaradharrePuzzleBox =
            new CommodityDefinition( 128667683, "JaradharrePuzzleBox", ConsumerItems, 16816, true );

        public static CommodityDefinition UzumokuLowGWings =
            new CommodityDefinition( 128667696, "UzumokuLowGWings", ConsumerItems, 13845, true );

        public static CommodityDefinition AltairianSkin =
            new CommodityDefinition( 128667700, "AltairianSkin", ConsumerItems, 8432, true );

        public static CommodityDefinition JotunMookah =
            new CommodityDefinition( 128667702, "JotunMookah", ConsumerItems, 8780, true );

        public static CommodityDefinition ZeesszeAntGlue =
            new CommodityDefinition( 128667707, "ZeesszeAntGlue", ConsumerItems, 8161, true );

        public static CommodityDefinition MomusBogSpaniel =
            new CommodityDefinition( 128667713, "MomusBogSpaniel", ConsumerItems, 9184, true );

        public static CommodityDefinition AlienEggs =
            new CommodityDefinition( 128667717, "AlienEggs", ConsumerItems, 25067, true );

        public static CommodityDefinition VidavantianLace =
            new CommodityDefinition( 128667719, "VidavantianLace", ConsumerItems, 12615, true );

        public static CommodityDefinition JaquesQuinentianStill =
            new CommodityDefinition( 128668017, "JaquesQuinentianStill", ConsumerItems, 2108, true );

        public static CommodityDefinition SoontillRelics =
            new CommodityDefinition( 128668018, "SoontillRelics", ConsumerItems, 19885, true );

        public static CommodityDefinition Advert1 = // Ultra-Compact Processor Prototypes
            new CommodityDefinition( 128671119, "Advert1", ConsumerItems, 21542, true );

        public static CommodityDefinition TheHuttonMug =
            new CommodityDefinition( 128672121, "TheHuttonMug", ConsumerItems, 7986, true );

        public static CommodityDefinition EvacuationShelter =
            new CommodityDefinition( 128672314, "EvacuationShelter", ConsumerItems, 343, false );

        public static CommodityDefinition PersonalGifts =
            new CommodityDefinition( 128672431, "PersonalGifts", ConsumerItems, 16535, false );

        public static CommodityDefinition CrystallineSpheres =
            new CommodityDefinition( 128672432, "CrystallineSpheres", ConsumerItems, 12216, true );

        public static CommodityDefinition SurvivalEquipment =
            new CommodityDefinition( 128682048, "SurvivalEquipment", ConsumerItems, 485, false );

        public static CommodityDefinition ShansCharisOrchid =
            new CommodityDefinition( 128732551, "ShansCharisOrchid", ConsumerItems, 9043, true );

        public static CommodityDefinition BuckyballBeerMats =
            new CommodityDefinition( 128748428, "BuckyballBeerMats", ConsumerItems, 7957, true );

        public static CommodityDefinition Duradrives =
            new CommodityDefinition( 128922524, "Duradrives", ConsumerItems, 19356, true );

        #endregion

        #region Foods

        public static CommodityDefinition Algae = 
            new CommodityDefinition( 128049177, "Algae", Foods, 137, false );

        public static CommodityDefinition FruitAndVegetables =
            new CommodityDefinition( 128049178, "FruitAndVegetables", Foods, 312, false );

        public static CommodityDefinition Grain = 
            new CommodityDefinition( 128049180, "Grain", Foods, 210, false );

        public static CommodityDefinition Animalmeat =
            new CommodityDefinition( 128049182, "Animalmeat", Foods, 1292, false );

        public static CommodityDefinition Fish = 
            new CommodityDefinition( 128049183, "Fish", Foods, 406, false );

        public static CommodityDefinition FoodCartridges =
            new CommodityDefinition( 128049184, "FoodCartridges", Foods, 105, false );

        public static CommodityDefinition SyntheticMeat =
            new CommodityDefinition( 128049185, "SyntheticMeat", Foods, 271, false );

        public static CommodityDefinition Tea = 
            new CommodityDefinition( 128049188, "Tea", Foods, 1467, false );

        public static CommodityDefinition Coffee = 
            new CommodityDefinition( 128049189, "Coffee", Foods, 1279, false );

        public static CommodityDefinition HIP10175BushMeat =
            new CommodityDefinition( 128667019, "HIP10175BushMeat", Foods, 9382, true );

        public static CommodityDefinition AlbinoQuechuaMammoth =
            new CommodityDefinition( 128667020, "AlbinoQuechuaMammoth", Foods, 9687, true );

        public static CommodityDefinition UtgaroarMillenialEggs =
            new CommodityDefinition( 128667021, "UtgaroarMillenialEggs", Foods, 9163, true );

        public static CommodityDefinition WitchhaulKobeBeef =
            new CommodityDefinition( 128667022, "WitchhaulKobeBeef", Foods, 11085, true );

        public static CommodityDefinition KarsukiLocusts =
            new CommodityDefinition( 128667023, "KarsukiLocusts", Foods, 8543, true );

        public static CommodityDefinition GiantIrukamaSnails =
            new CommodityDefinition( 128667024, "GiantIrukamaSnails", Foods, 9174, true );

        public static CommodityDefinition BaltahSineVacuumKrill =
            new CommodityDefinition( 128667025, "BaltahSineVacuumKrill", Foods, 8479, true );

        public static CommodityDefinition CetiRabbits =
            new CommodityDefinition( 128667026, "CetiRabbits", Foods, 9079, true );

        public static CommodityDefinition AnyNaCoffee =
            new CommodityDefinition( 128667041, "AnyNaCoffee", Foods, 9160, true );

        public static CommodityDefinition CD75CatCoffee =
            new CommodityDefinition( 128667042, "CD75CatCoffee", Foods, 9571, true );

        public static CommodityDefinition GomanYauponCoffee =
            new CommodityDefinition( 128667043, "GomanYauponCoffee", Foods, 8921, true );

        public static CommodityDefinition ChiEridaniMarinePaste =
            new CommodityDefinition( 128667049, "ChiEridaniMarinePaste", Foods, 8450, true );

        public static CommodityDefinition EsusekuCaviar =
            new CommodityDefinition( 128667050, "EsusekuCaviar", Foods, 9625, true );

        public static CommodityDefinition LiveHecateSeaWorms =
            new CommodityDefinition( 128667051, "LiveHecateSeaWorms", Foods, 8737, true );

        public static CommodityDefinition HelvetitjPearls =
            new CommodityDefinition( 128667052, "HelvetitjPearls", Foods, 10450, true );

        public static CommodityDefinition HIP41181Squid =
            new CommodityDefinition( 128667053, "HIP41181Squid", Foods, 8497, true );

        public static CommodityDefinition CoquimSpongiformVictuals =
            new CommodityDefinition( 128667054, "CoquimSpongiformVictuals", Foods, 8077, true );

        public static CommodityDefinition AerialEdenApple =
            new CommodityDefinition( 128667055, "AerialEdenApple", Foods, 8331, true );

        public static CommodityDefinition NeritusBerries =
            new CommodityDefinition( 128667056, "NeritusBerries", Foods, 8497, true );

        public static CommodityDefinition OchoengChillies =
            new CommodityDefinition( 128667057, "OchoengChillies", Foods, 8601, true );

        public static CommodityDefinition DeuringasTruffles =
            new CommodityDefinition( 128667058, "DeuringasTruffles", Foods, 9232, true );

        public static CommodityDefinition HR7221Wheat =
            new CommodityDefinition( 128667059, "HR7221Wheat", Foods, 8190, true );

        public static CommodityDefinition JarouaRice =
            new CommodityDefinition( 128667060, "JarouaRice", Foods, 8169, true );

        public static CommodityDefinition SanumaMEAT =
            new CommodityDefinition( 128667076, "SanumaMEAT", Foods, 8504, true );

        public static CommodityDefinition EthgrezeTeaBuds =
            new CommodityDefinition( 128667077, "EthgrezeTeaBuds", Foods, 10197, true );

        public static CommodityDefinition CeremonialHeikeTea =
            new CommodityDefinition( 128667078, "CeremonialHeikeTea", Foods, 9251, true );

        public static CommodityDefinition TanmarkTranquilTea =
            new CommodityDefinition( 128667079, "TanmarkTranquilTea", Foods, 9177, true );

        public static CommodityDefinition BakedGreebles =
            new CommodityDefinition( 128667669, "BakedGreebles", Foods, 8211, true );

        public static CommodityDefinition CetiAepyornisEgg =
            new CommodityDefinition( 128667670, "CetiAepyornisEgg", Foods, 9769, true );

        public static CommodityDefinition HaidneBlackBrew =
            new CommodityDefinition( 128667679, "HaidneBlackBrew", Foods, 8837, true );

        public static CommodityDefinition LFTVoidExtractCoffee =
            new CommodityDefinition( 128667685, "LFTVoidExtractCoffee", Foods, 9554, true );

        public static CommodityDefinition LTTHyperSweet =
            new CommodityDefinition( 128667688, "LTTHyperSweet", Foods, 8054, true );

        public static CommodityDefinition MechucosHighTea =
            new CommodityDefinition( 128667689, "MechucosHighTea", Foods, 8846, true );

        public static CommodityDefinition MokojingBeastFeast =
            new CommodityDefinition( 128667691, "MokojingBeastFeast", Foods, 9788, true );

        public static CommodityDefinition MukusubiiChitinOs =
            new CommodityDefinition( 128667692, "MukusubiiChitinOs", Foods, 8359, true );

        public static CommodityDefinition MulachiGiantFungus =
            new CommodityDefinition( 128667693, "MulachiGiantFungus", Foods, 7957, true );

        public static CommodityDefinition WheemeteWheatCakes =
            new CommodityDefinition( 128667698, "WheemeteWheatCakes", Foods, 8081, true );

        public static CommodityDefinition AroucaConventualSweets =
            new CommodityDefinition( 128667705, "AroucaConventualSweets", Foods, 8737, true );

        public static CommodityDefinition OrrerianViciousBrew =
            new CommodityDefinition( 128667711, "OrrerianViciousBrew", Foods, 8342, true );

        public static CommodityDefinition UszaianTreeGrub =
            new CommodityDefinition( 128667712, "UszaianTreeGrub", Foods, 8578, true );

        public static CommodityDefinition DisoMaCorn =
            new CommodityDefinition( 128667714, "DisoMaCorn", Foods, 8134, true );

        #endregion

        #region IndustrialMaterials

        public static CommodityDefinition Polymers =
            new CommodityDefinition( 128049197, "Polymers", IndustrialMaterials, 171, false );

        public static CommodityDefinition Semiconductors =
            new CommodityDefinition( 128049199, "Semiconductors", IndustrialMaterials, 967, false );

        public static CommodityDefinition Superconductors =
            new CommodityDefinition( 128049200, "Superconductors", IndustrialMaterials, 6609, false );

        public static CommodityDefinition MedbStarlube =
            new CommodityDefinition( 128667690, "MedbStarlube", IndustrialMaterials, 8191, true );

        public static CommodityDefinition CeramicComposites =
            new CommodityDefinition( 128672302, "CeramicComposites", IndustrialMaterials, 232, false );

        public static CommodityDefinition MetaAlloys =
            new CommodityDefinition( 128672701, "MetaAlloys", IndustrialMaterials, 88148, false, true );

        public static CommodityDefinition InsulatingMembrane =
            new CommodityDefinition( 128673855, "InsulatingMembrane", IndustrialMaterials, 7837, false );

        public static CommodityDefinition CMMComposite =
            new CommodityDefinition( 128673856, "CMMComposite", IndustrialMaterials, 3132, false );

        public static CommodityDefinition CoolingHoses =
            new CommodityDefinition( 128673857, "CoolingHoses", IndustrialMaterials, 403, false );

        public static CommodityDefinition NeofabricInsulation =
            new CommodityDefinition( 128673858, "NeofabricInsulation", IndustrialMaterials, 2769, false );

        #endregion

        #region Machinery

        public static CommodityDefinition PowerGenerators =
            new CommodityDefinition( 128049217, "PowerGenerators", Machinery, 458, false );

        public static CommodityDefinition WaterPurifiers =
            new CommodityDefinition( 128049218, "WaterPurifiers", Machinery, 258, false );

        public static CommodityDefinition HeliostaticFurnaces =
            new CommodityDefinition( 128049220, "HeliostaticFurnaces", Machinery, 236, false );

        public static CommodityDefinition MineralExtractors =
            new CommodityDefinition( 128049221, "MineralExtractors", Machinery, 443, false );

        public static CommodityDefinition CropHarvesters =
            new CommodityDefinition( 128049222, "CropHarvesters", Machinery, 2021, false );

        public static CommodityDefinition MarineSupplies =
            new CommodityDefinition( 128049223, "MarineSupplies", Machinery, 3916, false );

        public static CommodityDefinition AtmosphericExtractors =
            new CommodityDefinition( 128064028, "AtmosphericExtractors", Machinery, 357, false );

        public static CommodityDefinition VolkhabBeeDrones =
            new CommodityDefinition( 128667044, "VolkhabBeeDrones", Machinery, 10198, true );

        public static CommodityDefinition WulpaHyperboreSystems =
            new CommodityDefinition( 128667067, "WulpaHyperboreSystems", Machinery, 8726, true );

        public static CommodityDefinition NonEuclidianExotanks =
            new CommodityDefinition( 128667687, "NonEuclidianExotanks", Machinery, 8526, true );

        public static CommodityDefinition GiantVerrix =
            new CommodityDefinition( 128667703, "GiantVerrix", Machinery, 12496, true );

        public static CommodityDefinition GeologicalEquipment =
            new CommodityDefinition( 128672307, "GeologicalEquipment", Machinery, 1661, false );

        public static CommodityDefinition ThermalCoolingUnits =
            new CommodityDefinition( 128672308, "ThermalCoolingUnits", Machinery, 256, false );

        public static CommodityDefinition BuildingFabricators =
            new CommodityDefinition( 128672309, "BuildingFabricators", Machinery, 980, false );

        public static CommodityDefinition SkimerComponents =
            new CommodityDefinition( 128672313, "SkimerComponents", Machinery, 859, false );

        public static CommodityDefinition ArticulationMotors =
            new CommodityDefinition( 128673859, "ArticulationMotors", Machinery, 4997, false );

        public static CommodityDefinition HNShockMount =
            new CommodityDefinition( 128673860, "HNShockMount", Machinery, 406, false );

        public static CommodityDefinition EmergencyPowerCells =
            new CommodityDefinition( 128673861, "EmergencyPowerCells", Machinery, 1011, false );

        public static CommodityDefinition PowerConverter =
            new CommodityDefinition( 128673862, "PowerConverter", Machinery, 246, false );

        public static CommodityDefinition PowerGridAssembly =
            new CommodityDefinition( 128673863, "PowerGridAssembly", Machinery, 1684, false );

        public static CommodityDefinition PowerTransferConduits =
            new CommodityDefinition( 128673864, "PowerTransferConduits", Machinery, 857, false );

        public static CommodityDefinition RadiationBaffle =
            new CommodityDefinition( 128673865, "RadiationBaffle", Machinery, 383, false );

        public static CommodityDefinition ExhaustManifold =
            new CommodityDefinition( 128673866, "ExhaustManifold", Machinery, 479, false );

        public static CommodityDefinition ReinforcedMountingPlate =
            new CommodityDefinition( 128673867, "ReinforcedMountingPlate", Machinery, 1074, false );

        public static CommodityDefinition HeatsinkInterlink =
            new CommodityDefinition( 128673868, "HeatsinkInterlink", Machinery, 729, false );

        public static CommodityDefinition MagneticEmitterCoil =
            new CommodityDefinition( 128673869, "MagneticEmitterCoil", Machinery, 199, false );

        public static CommodityDefinition ModularTerminals =
            new CommodityDefinition( 128673870, "ModularTerminals", Machinery, 695, false );

        #endregion

        #region Medicines

        public static CommodityDefinition AgriculturalMedicines =
            new CommodityDefinition( 128049208, "AgriculturalMedicines", Medicines, 1038, false );

        public static CommodityDefinition PerformanceEnhancers =
            new CommodityDefinition( 128049209, "PerformanceEnhancers", Medicines, 6816, false );

        public static CommodityDefinition BasicMedicines =
            new CommodityDefinition( 128049210, "BasicMedicines", Medicines, 279, false );

        public static CommodityDefinition ProgenitorCells =
            new CommodityDefinition( 128049669, "ProgenitorCells", Medicines, 6779, false );

        public static CommodityDefinition CombatStabilisers =
            new CommodityDefinition( 128049670, "CombatStabilisers", Medicines, 3505, false );
        
        public static CommodityDefinition KachiriginLeaches =
            new CommodityDefinition( 128667027, "KachiriginLeaches", Medicines, 8227, true );
        
        public static CommodityDefinition AganippeRush =
            new CommodityDefinition( 128667068, "AganippeRush", Medicines, 14220, true );

        public static CommodityDefinition TerraMaterBloodBores =
            new CommodityDefinition( 128667069, "TerraMaterBloodBores", Medicines, 13414, true );

        public static CommodityDefinition WatersOfShintara =
            new CommodityDefinition( 128667085, "WatersOfShintara", Medicines, 13711, true );

        public static CommodityDefinition HonestyPills =
            new CommodityDefinition( 128667686, "HonestyPills", Medicines, 8860, true );

        public static CommodityDefinition VHerculisBodyRub =
            new CommodityDefinition( 128667697, "VHerculisBodyRub", Medicines, 8010, true );

        public static CommodityDefinition VegaSlimweed =
            new CommodityDefinition( 128667699, "VegaSlimweed", Medicines, 9588, true );

        public static CommodityDefinition TauriChimes =
            new CommodityDefinition( 128667706, "TauriChimes", Medicines, 8549, true );

        public static CommodityDefinition PantaaPrayerSticks =
            new CommodityDefinition( 128667708, "PantaaPrayerSticks", Medicines, 9177, true );

        public static CommodityDefinition FujinTea =
            new CommodityDefinition( 128667709, "FujinTea", Medicines, 8597, true );

        public static CommodityDefinition AlyaBodilySoap =
            new CommodityDefinition( 128667718, "AlyaBodilySoap", Medicines, 8218, true );

        public static CommodityDefinition AdvancedMedicines =
            new CommodityDefinition( 128682046, "AdvancedMedicines", Medicines, 1259, false );

        public static CommodityDefinition Nanomedicines =
            new CommodityDefinition( 128913661, "Nanomedicines", Medicines, 9859, true );

        #endregion

        #region Metals

        public static CommodityDefinition Platinum =
            new CommodityDefinition( 128049152, "Platinum", Metals, 19279, false );

        public static CommodityDefinition Palladium =
            new CommodityDefinition( 128049153, "Palladium", Metals, 13298, false );

        public static CommodityDefinition Gold = 
            new CommodityDefinition( 128049154, "Gold", Metals, 9401, false );

        public static CommodityDefinition Silver = 
            new CommodityDefinition( 128049155, "Silver", Metals, 4775, false );

        public static CommodityDefinition Cobalt = 
            new CommodityDefinition( 128049162, "Cobalt", Metals, 647, false );

        public static CommodityDefinition Beryllium =
            new CommodityDefinition( 128049168, "Beryllium", Metals, 8288, false );

        public static CommodityDefinition Indium = 
            new CommodityDefinition( 128049169, "Indium", Metals, 5727, false );

        public static CommodityDefinition Gallium =
            new CommodityDefinition( 128049170, "Gallium", Metals, 5135, false );

        public static CommodityDefinition Tantalum =
            new CommodityDefinition( 128049171, "Tantalum", Metals, 3962, false );

        public static CommodityDefinition Uranium =
            new CommodityDefinition( 128049172, "Uranium", Metals, 2705, false );

        public static CommodityDefinition Lithium =
            new CommodityDefinition( 128049173, "Lithium", Metals, 1596, false );

        public static CommodityDefinition Titanium =
            new CommodityDefinition( 128049174, "Titanium", Metals, 1006, false );

        public static CommodityDefinition Copper = 
            new CommodityDefinition( 128049175, "Copper", Metals, 481, false );

        public static CommodityDefinition Aluminium =
            new CommodityDefinition( 128049176, "Aluminium", Metals, 340, false );

        public static CommodityDefinition Hafnium178 =
            new CommodityDefinition( 128668549, "Hafnium178", Metals, 69098, false );

        public static CommodityDefinition Osmium =
            new CommodityDefinition( 128671118, "Osmium", Metals, 7591, false );

        public static CommodityDefinition SothisCrystallineGold =
            new CommodityDefinition( 128672122, "SothisCrystallineGold", Metals, 19112, true );

        public static CommodityDefinition Lanthanum =
            new CommodityDefinition( 128672298, "Lanthanum", Metals, 8766, false );

        public static CommodityDefinition Thallium =
            new CommodityDefinition( 128672299, "Thallium", Metals, 3618, false );

        public static CommodityDefinition Bismuth =
            new CommodityDefinition( 128672300, "Bismuth", Metals, 2284, false );

        public static CommodityDefinition Thorium =
            new CommodityDefinition( 128672301, "Thorium", Metals, 11513, false );

        public static CommodityDefinition Praseodymium =
            new CommodityDefinition( 128673845, "Praseodymium", Metals, 7156, false );

        public static CommodityDefinition Samarium =
            new CommodityDefinition( 128673847, "Samarium", Metals, 6330, false );

        public static CommodityDefinition PlatinumAloy =
            new CommodityDefinition( 128793114, "PlatinumAloy", Metals, 18333, true );

        public static CommodityDefinition Steel =
            new CommodityDefinition( 129031238, "Steel", Metals, 4179, false );

        #endregion

        #region Minerals

        public static CommodityDefinition Bertrandite =
            new CommodityDefinition( 128049156, "Bertrandite", Minerals, 2374, false );

        public static CommodityDefinition Indite =
            new CommodityDefinition( 128049157, "Indite", Minerals, 2088, false );

        public static CommodityDefinition Gallite =
            new CommodityDefinition( 128049158, "Gallite", Minerals, 1819, false );

        public static CommodityDefinition Coltan =
            new CommodityDefinition( 128049159, "Coltan", Minerals, 1319, false );

        public static CommodityDefinition Uraninite =
            new CommodityDefinition( 128049160, "Uraninite", Minerals, 836, false );

        public static CommodityDefinition Lepidolite =
            new CommodityDefinition( 128049161, "Lepidolite", Minerals, 544, false );
        
        public static CommodityDefinition Rutile =
            new CommodityDefinition( 128049163, "Rutile", Minerals, 299, false );

        public static CommodityDefinition Bauxite =
            new CommodityDefinition( 128049165, "Bauxite", Minerals, 120, false );

        public static CommodityDefinition CherbonesBloodCrystals =
            new CommodityDefinition( 128667675, "CherbonesBloodCrystals", Minerals, 16714, true );

        public static CommodityDefinition NgadandariFireOpals =
            new CommodityDefinition( 128667694, "NgadandariFireOpals", Minerals, 19112, true );

        public static CommodityDefinition Painite =
            new CommodityDefinition( 128668550, "Painite", Minerals, 40508, false );

        public static CommodityDefinition Cryolite =
            new CommodityDefinition( 128672294, "Cryolite", Minerals, 2266, false );

        public static CommodityDefinition Goslarite =
            new CommodityDefinition( 128672295, "Goslarite", Minerals, 916, false );

        public static CommodityDefinition Moissanite =
            new CommodityDefinition( 128672296, "Moissanite", Minerals, 8273, false );

        public static CommodityDefinition Pyrophyllite =
            new CommodityDefinition( 128672297, "Pyrophyllite", Minerals, 1565, false );

        public static CommodityDefinition Taaffeite =
            new CommodityDefinition( 128672775, "Taaffeite", Minerals, 20696, false );

        public static CommodityDefinition Jadeite =
            new CommodityDefinition( 128672776, "Jadeite", Minerals, 13474, false );

        public static CommodityDefinition Bromellite =
            new CommodityDefinition( 128673846, "Bromellite", Minerals, 7062, false );

        public static CommodityDefinition LowTemperatureDiamond =
            new CommodityDefinition( 128673848, "LowTemperatureDiamond", Minerals, 57445, false );

        public static CommodityDefinition MethanolMonohydrateCrystals =
            new CommodityDefinition( 128673852, "MethanolMonohydrateCrystals", Minerals, 2282, false );

        public static CommodityDefinition LithiumHydroxide =
            new CommodityDefinition( 128673853, "LithiumHydroxide", Minerals, 5646, false );

        public static CommodityDefinition MethaneClathrate =
            new CommodityDefinition( 128673854, "MethaneClathrate", Minerals, 629, false );

        public static CommodityDefinition Rhodplumsite =
            new CommodityDefinition( 128924325, "Rhodplumsite", Minerals, 176791, false );

        public static CommodityDefinition Serendibite =
            new CommodityDefinition( 128924326, "Serendibite", Minerals, 172634, false );

        public static CommodityDefinition Monazite =
            new CommodityDefinition( 128924327, "Monazite", Minerals, 200925, false );

        public static CommodityDefinition Musgravite =
            new CommodityDefinition( 128924328, "Musgravite", Minerals, 198527, false );

        public static CommodityDefinition Benitoite =
            new CommodityDefinition( 128924329, "Benitoite", Minerals, 149325, false );

        public static CommodityDefinition Grandidierite =
            new CommodityDefinition( 128924330, "Grandidierite", Minerals, 197204, false );

        public static CommodityDefinition Alexandrite =
            new CommodityDefinition( 128924331, "Alexandrite", Minerals, 217192, false );

        public static CommodityDefinition Opal = 
            new CommodityDefinition( 128924332, "Opal", Minerals, 135218, false );

        public static CommodityDefinition Haematite =
            new CommodityDefinition( 129031327, "Haematite", Minerals, 1791, false );

        #endregion

        #region Narcotics

        public static CommodityDefinition BasicNarcotics =
            new CommodityDefinition( 128049212, "BasicNarcotics", Narcotics, 9966, false );

        public static CommodityDefinition Tobacco =
            new CommodityDefinition( 128049213, "Tobacco", Narcotics, 5035, false );

        public static CommodityDefinition Beer =
            new CommodityDefinition( 128049214, "Beer", Narcotics, 186, false );

        public static CommodityDefinition Wine =
            new CommodityDefinition( 128049215, "Wine", Narcotics, 260, false );

        public static CommodityDefinition Liquor =
            new CommodityDefinition( 128049216, "Liquor", Narcotics, 587, false );

        public static CommodityDefinition EraninPearlWhisky =
            new CommodityDefinition( 128666746, "EraninPearlWhisky", Narcotics, 9040, true );

        public static CommodityDefinition LavianBrandy =
            new CommodityDefinition( 128666747, "LavianBrandy", Narcotics, 10365, true );

        public static CommodityDefinition LyraeWeed =
            new CommodityDefinition( 128667028, "LyraeWeed", Narcotics, 8937, true );

        public static CommodityDefinition OnionHead =
            new CommodityDefinition( 128667029, "OnionHead", Narcotics, 8437, true );

        public static CommodityDefinition TarachTorSpice =
            new CommodityDefinition( 128667030, "TarachTorSpice", Narcotics, 8642, true );

        public static CommodityDefinition Wolf1301Fesh =
            new CommodityDefinition( 128667031, "Wolf1301Fesh", Narcotics, 8399, true );

        public static CommodityDefinition KonggaAle =
            new CommodityDefinition( 128667034, "KonggaAle", Narcotics, 8310, true );

        public static CommodityDefinition WuthieloKuFroth =
            new CommodityDefinition( 128667035, "WuthieloKuFroth", Narcotics, 8194, true );

        public static CommodityDefinition BastSnakeGin =
            new CommodityDefinition( 128667065, "BastSnakeGin", Narcotics, 8659, true );

        public static CommodityDefinition ThrutisCream =
            new CommodityDefinition( 128667066, "ThrutisCream", Narcotics, 8550, true );

        public static CommodityDefinition KamitraCigars =
            new CommodityDefinition( 128667081, "KamitraCigars", Narcotics, 12282, true );

        public static CommodityDefinition RusaniOldSmokey =
            new CommodityDefinition( 128667082, "RusaniOldSmokey", Narcotics, 11994, true );

        public static CommodityDefinition YasoKondiLeaf =
            new CommodityDefinition( 128667083, "YasoKondiLeaf", Narcotics, 12171, true );

        public static CommodityDefinition ChateauDeAegaeon =
            new CommodityDefinition( 128667084, "ChateauDeAegaeon", Narcotics, 8791, true );

        public static CommodityDefinition SaxonWine =
            new CommodityDefinition( 128667671, "SaxonWine", Narcotics, 8983, true );

        public static CommodityDefinition CentauriMegaGin =
            new CommodityDefinition( 128667672, "CentauriMegaGin", Narcotics, 10217, true );

        public static CommodityDefinition MotronaExperienceJelly =
            new CommodityDefinition( 128667676, "MotronaExperienceJelly", Narcotics, 13129, true );

        public static CommodityDefinition GeawenDanceDust =
            new CommodityDefinition( 128667677, "GeawenDanceDust", Narcotics, 8618, true );

        public static CommodityDefinition GerasianGueuzeBeer =
            new CommodityDefinition( 128667678, "GerasianGueuzeBeer", Narcotics, 8215, true );

        public static CommodityDefinition BurnhamBileDistillate =
            new CommodityDefinition( 128667681, "BurnhamBileDistillate", Narcotics, 8466, true );

        public static CommodityDefinition PavonisEarGrubs =
            new CommodityDefinition( 128667701, "PavonisEarGrubs", Narcotics, 8364, true );

        public static CommodityDefinition IndiBourbon =
            new CommodityDefinition( 128667704, "IndiBourbon", Narcotics, 8806, true );

        public static CommodityDefinition LeestianEvilJuice =
            new CommodityDefinition( 128667715, "LeestianEvilJuice", Narcotics, 8220, true );

        public static CommodityDefinition BlueMilk =
            new CommodityDefinition( 128667716, "BlueMilk", Narcotics, 10805, true );

        public static CommodityDefinition TransgenicOnionHead =
            new CommodityDefinition( 128667760, "TransgenicOnionHead", Narcotics, 8472, true );

        public static CommodityDefinition BootlegLiquor =
            new CommodityDefinition( 128672306, "BootlegLiquor", Narcotics, 855, false );

        public static CommodityDefinition OnionHeadA =
            new CommodityDefinition( 128672812, "OnionHeadA", Narcotics, 8437, true );

        public static CommodityDefinition OnionHeadB =
            new CommodityDefinition( 128673069, "OnionHeadB", Narcotics, 8437, true );

        public static CommodityDefinition AnimalEffigies =
            new CommodityDefinition( 128727921, "AnimalEffigies", Narcotics, 8399, true);  // Crom Silver Fesh

        public static CommodityDefinition HarmaSilverSeaRum =
            new CommodityDefinition( 128793113, "HarmaSilverSeaRum", Narcotics, 9762, true );

        public static CommodityDefinition ApaVietii =
            new CommodityDefinition( 128958679, "ApaVietii", Narcotics, 10362, true );

        public static CommodityDefinition OnionHeadC =
            new CommodityDefinition( 128983059, "OnionHeadC", Narcotics, 5387, false );

        #endregion

        #region Nonmarketable

        public static CommodityDefinition Drones =
            new CommodityDefinition( 128066403, "Drones", NonMarketable, 101, false );

        #endregion

        #region Powerplay

        public static CommodityDefinition AislingMediaMaterials =
            new CommodityDefinition( 128671289, "AislingMediaMaterials", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition AislingMediaResources =
            new CommodityDefinition( 128671290, "AislingMediaResources", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition AislingPromotionalMaterials =
            new CommodityDefinition( 128671291, "AislingPromotionalMaterials", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition AllianceTradeAgreements =
            new CommodityDefinition( 128671292, "AllianceTradeAgreements", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition AllianceLegaslativeContracts =
            new CommodityDefinition( 128671293, "AllianceLegaslativeContracts", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition AllianceLegaslativeRecords =
            new CommodityDefinition( 128671294, "AllianceLegaslativeRecords", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition LavignyCorruptionDossiers =
            new CommodityDefinition( 128671295, "LavignyCorruptionDossiers", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition LavignyFieldSupplies =
            new CommodityDefinition( 128671296, "LavignyFieldSupplies", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition LavignyGarisonSupplies =
            new CommodityDefinition( 128671297, "LavignyGarisonSupplies", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition RestrictedPackage =
            new CommodityDefinition( 128671298, "RestrictedPackage", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition LiberalCampaignMaterials =
            new CommodityDefinition( 128671300, "LiberalCampaignMaterials", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition FederalAid =
            new CommodityDefinition( 128671301, "FederalAid", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition FederalTradeContracts =
            new CommodityDefinition( 128671302, "FederalTradeContracts", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition LoanedArms =
            new CommodityDefinition( 128671303, "LoanedArms", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition PatreusFieldSupplies =
            new CommodityDefinition( 128671304, "PatreusFieldSupplies", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition PatreusGarisonSupplies =
            new CommodityDefinition( 128671305, "PatreusGarisonSupplies", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition RestrictedIntel =
            new CommodityDefinition( 128671306, "RestrictedIntel", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition RepublicanFieldSupplies =
            new CommodityDefinition( 128671307, "RepublicanFieldSupplies", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition RepublicanGarisonSupplies =
            new CommodityDefinition( 128671308, "RepublicanGarisonSupplies", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition SiriusFranchisePackage =
            new CommodityDefinition( 128671309, "SiriusFranchisePackage", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition SiriusCommercialContracts =
            new CommodityDefinition( 128671310, "SiriusCommercialContracts", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition SiriusIndustrialEquipment =
            new CommodityDefinition( 128671311, "SiriusIndustrialEquipment", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition TorvalCommercialContracts =
            new CommodityDefinition( 128671312, "TorvalCommercialContracts", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition ImperialPrisoner =
            new CommodityDefinition( 128671313, "ImperialPrisoner", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition UtopianPublicity =
            new CommodityDefinition( 128671314, "UtopianPublicity", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition UtopianFieldSupplies =
            new CommodityDefinition( 128671315, "UtopianFieldSupplies", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition UtopianDissident =
            new CommodityDefinition( 128671316, "UtopianDissident", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition IllicitConsignment =
            new CommodityDefinition( 128671317, "IllicitConsignment", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition UnmarkedWeapons =
            new CommodityDefinition( 128671318, "UnmarkedWeapons", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition OnionheadSamples =
            new CommodityDefinition( 128671319, "OnionheadSamples", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition CounterCultureSupport =
            new CommodityDefinition( 128671320, "CounterCultureSupport", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition MarkedSlaves =
            new CommodityDefinition( 128671445, "MarkedSlaves", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition TorvalDeeds =
            new CommodityDefinition( 128671446, "TorvalDeeds", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition OnionheadDerivatives =
            new CommodityDefinition( 128671447, "OnionheadDerivatives", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition OutOfDateGoods =
            new CommodityDefinition( 128671450, "OutOfDateGoods", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition UndergroundSupport =
            new CommodityDefinition( 128732548, "UndergroundSupport", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition GromCounterIntelligence =
            new CommodityDefinition( 128732549, "GromCounterIntelligence", Powerplay, 0, false);  // PowerPlay

        public static CommodityDefinition GromWarTrophies =
            new CommodityDefinition( 128732550, "GromWarTrophies", Powerplay, 0, false);  // PowerPlay

        // Items for which we do not have Elite IDs
        public static CommodityDefinition KaineAidSupplies =
            new CommodityDefinition( null, "KaineAidSupplies", Powerplay, 0, false, false );

        public static CommodityDefinition KaineMisinformation =
            new CommodityDefinition( null, "KaineMisinformation", Powerplay, 0, false, false );

        public static CommodityDefinition KaineLobbyingMaterial =
            new CommodityDefinition( null, "KaineLobbyingMaterial", Powerplay, 0, false, false );

        public static CommodityDefinition LavignyStrategicReports =
            new CommodityDefinition( null, "LavignyStrategicReports", Powerplay, 0, false, false );

        #endregion

        #region Salvage

        public static CommodityDefinition USSCargoBlackBox =
            new CommodityDefinition( 128666752, "USSCargoBlackBox", Salvage, 6995, false );

        public static CommodityDefinition USSCargoTradeData =
            new CommodityDefinition( 128666754, "USSCargoTradeData", Salvage, 2790, false );

        public static CommodityDefinition USSCargoMilitaryPlans =
            new CommodityDefinition( 128666755, "USSCargoMilitaryPlans", Salvage, 9413, false );

        public static CommodityDefinition USSCargoAncientArtefact =
            new CommodityDefinition( 128666756, "USSCargoAncientArtefact", Salvage, 8183, false );

        public static CommodityDefinition USSCargoRareArtwork =
            new CommodityDefinition( 128666757, "USSCargoRareArtwork", Salvage, 7774, false );

        public static CommodityDefinition USSCargoExperimentalChemicals =
            new CommodityDefinition( 128666758, "USSCargoExperimentalChemicals", Salvage, 3524, false );

        public static CommodityDefinition USSCargoRebelTransmissions =
            new CommodityDefinition( 128666759, "USSCargoRebelTransmissions", Salvage, 4068, false );

        public static CommodityDefinition USSCargoPrototypeTech =
            new CommodityDefinition( 128666760, "USSCargoPrototypeTech", Salvage, 10696, false );

        public static CommodityDefinition USSCargoTechnicalBlueprints =
            new CommodityDefinition( 128666761, "USSCargoTechnicalBlueprints", Salvage, 6333, false );

        public static CommodityDefinition UnknownArtifact =
            new CommodityDefinition( 128668547, "UnknownArtifact", Salvage, 290190, false, true );

        public static CommodityDefinition AiRelics =
            new CommodityDefinition( 128668548, "AiRelics", Salvage, 138613, false );

        public static CommodityDefinition Antiquities =
            new CommodityDefinition( 128668551, "Antiquities", Salvage, 115511, false );

        public static CommodityDefinition MilitaryIntelligence =
            new CommodityDefinition( 128668552, "MilitaryIntelligence", Salvage, 55527, false );

        public static CommodityDefinition SAP8CoreContainer =
            new CommodityDefinition( 128671443, "SAP8CoreContainer", Salvage, 59196, false );

        public static CommodityDefinition TrinketsOfFortune =
            new CommodityDefinition( 128671444, "TrinketsOfFortune", Salvage, 1428, false );

        public static CommodityDefinition WreckageComponents =
            new CommodityDefinition( 128672123, "WreckageComponents", Salvage, 394, false );

        public static CommodityDefinition EncriptedDataStorage =
            new CommodityDefinition( 128672124, "EncriptedDataStorage", Salvage, 806, false );

        public static CommodityDefinition OccupiedCryoPod =
            new CommodityDefinition( 128672125, "OccupiedCryoPod", Salvage, 4474, false );

        public static CommodityDefinition PersonalEffects =
            new CommodityDefinition( 128672126, "PersonalEffects", Salvage, 379, false );

        public static CommodityDefinition ComercialSamples =
            new CommodityDefinition( 128672127, "ComercialSamples", Salvage, 361, false );

        public static CommodityDefinition TacticalData =
            new CommodityDefinition( 128672128, "TacticalData", Salvage, 457, false );

        public static CommodityDefinition AssaultPlans =
            new CommodityDefinition( 128672129, "AssaultPlans", Salvage, 446, false );

        public static CommodityDefinition EncryptedCorrespondence =
            new CommodityDefinition( 128672130, "EncryptedCorrespondence", Salvage, 372, false );

        public static CommodityDefinition DiplomaticBag =
            new CommodityDefinition( 128672131, "DiplomaticBag", Salvage, 572, false );

        public static CommodityDefinition ScientificResearch =
            new CommodityDefinition( 128672132, "ScientificResearch", Salvage, 635, false );

        public static CommodityDefinition ScientificSamples =
            new CommodityDefinition( 128672133, "ScientificSamples", Salvage, 772, false );

        public static CommodityDefinition PoliticalPrisoner =
            new CommodityDefinition( 128672134, "PoliticalPrisoner", Salvage, 5132, false );

        public static CommodityDefinition Hostage =
            new CommodityDefinition( 128672135, "Hostage", Salvage, 2427, false );

        public static CommodityDefinition LargeExplorationDataCash =
            new CommodityDefinition( 128672136, "LargeExplorationDataCash", Salvage, 0, false );

        public static CommodityDefinition SmallExplorationDataCash =
            new CommodityDefinition( 128672137, "SmallExplorationDataCash", Salvage, 0, false );

        public static CommodityDefinition AntiqueJewellery =
            new CommodityDefinition( 128672159, "AntiqueJewellery", Salvage, 0, false );

        public static CommodityDefinition PreciousGems =
            new CommodityDefinition( 128672160, "PreciousGems", Salvage, 109641, false );

        public static CommodityDefinition EarthRelics =
            new CommodityDefinition( 128672161, "EarthRelics", Salvage, 0, false );

        public static CommodityDefinition GeneBank =
            new CommodityDefinition( 128672162, "GeneBank", Salvage, 0, false );

        public static CommodityDefinition TimeCapsule =
            new CommodityDefinition( 128672163, "TimeCapsule", Salvage, 0, false );

        public static CommodityDefinition GeologicalSamples =
            new CommodityDefinition( 128672315, "GeologicalSamples", Salvage, 446, false );

        public static CommodityDefinition UnstableDataCore =
            new CommodityDefinition( 128672810, "UnstableDataCore", Salvage, 2427, false );

        public static CommodityDefinition DamagedEscapePod =
            new CommodityDefinition( 128672811, "DamagedEscapePod", Salvage, 11912, false );

        public static CommodityDefinition UnknownArtifact2 =
            new CommodityDefinition( 128673876, "UnknownArtifact2", Salvage, 411003, false, true );

        public static CommodityDefinition DataCore =
            new CommodityDefinition( 128682049, "DataCore", Salvage, 2872, false );

        public static CommodityDefinition GalacticTravelGuide =
            new CommodityDefinition( 128682050, "GalacticTravelGuide", Salvage, 332, false );

        public static CommodityDefinition MysteriousIdol =
            new CommodityDefinition( 128682051, "MysteriousIdol", Salvage, 15196, false );

        public static CommodityDefinition ProhibitedResearchMaterials =
            new CommodityDefinition( 128682052, "ProhibitedResearchMaterials", Salvage, 46607, false );

        public static CommodityDefinition AntimatterContainmentUnit =
            new CommodityDefinition( 128682053, "AntimatterContainmentUnit", Salvage, 26608, false );

        public static CommodityDefinition SpacePioneerRelics =
            new CommodityDefinition( 128682054, "SpacePioneerRelics", Salvage, 7342, false );

        public static CommodityDefinition FossilRemnants =
            new CommodityDefinition( 128682055, "FossilRemnants", Salvage, 9927, false );

        public static CommodityDefinition AncientRelic =
            new CommodityDefinition( 128732183, "AncientRelic", Salvage, 24962, false );

        public static CommodityDefinition AncientOrb =
            new CommodityDefinition( 128732184, "AncientOrb", Salvage, 17415, false );

        public static CommodityDefinition AncientCasket =
            new CommodityDefinition( 128732185, "AncientCasket", Salvage, 16294, false );

        public static CommodityDefinition AncientTablet =
            new CommodityDefinition( 128732186, "AncientTablet", Salvage, 17415, false );

        public static CommodityDefinition AncientUrn =
            new CommodityDefinition( 128732187, "AncientUrn", Salvage, 14907, false );

        public static CommodityDefinition AncientTotem =
            new CommodityDefinition( 128732188, "AncientTotem", Salvage, 20437, false );

        public static CommodityDefinition UnknownResin =
            new CommodityDefinition( 128737287, "UnknownResin", Salvage, 18652, false, true );

        public static CommodityDefinition UnknownBiologicalMatter =
            new CommodityDefinition( 128737288, "UnknownBiologicalMatter", Salvage, 25479, false, true );

        public static CommodityDefinition UnknownTechnologySamples =
            new CommodityDefinition( 128737289, "UnknownTechnologySamples", Salvage, 22551, false, true );

        public static CommodityDefinition UnknownArtifact3 =
            new CommodityDefinition( 128740752, "UnknownArtifact3", Salvage, 31350, false, true );

        public static CommodityDefinition ThargoidHeart =
            new CommodityDefinition( 128793127, "ThargoidHeart", Salvage, 0, false, true );

        public static CommodityDefinition ThargoidTissueSampleType1 =
            new CommodityDefinition( 128793128, "ThargoidTissueSampleType1", Salvage, 14081, false, true );

        public static CommodityDefinition ThargoidTissueSampleType2 =
            new CommodityDefinition( 128793129, "ThargoidTissueSampleType2", Salvage, 0, false, true );

        public static CommodityDefinition ThargoidTissueSampleType3 =
            new CommodityDefinition( 128793130, "ThargoidTissueSampleType3", Salvage, 0, false, true );

        public static CommodityDefinition ThargoidScoutTissueSample =
            new CommodityDefinition( 128824468, "ThargoidScoutTissueSample", Salvage, 15215, false, true );

        public static CommodityDefinition AncientKey =
            new CommodityDefinition( 128888499, "AncientKey", Salvage, 29931, false );

        public static CommodityDefinition ThargoidTissueSampleType4 =
            new CommodityDefinition( 128902652, "ThargoidTissueSampleType4", Salvage, 0, false, true );

        public static CommodityDefinition M_TissueSample_Fluid =
            new CommodityDefinition( 128922517, "M_TissueSample_Fluid", Salvage, 0, false );

        public static CommodityDefinition M_TissueSample_Soft =
            new CommodityDefinition( 128922518, "M_TissueSample_Soft", Salvage, 0, false );

        public static CommodityDefinition M_TissueSample_Nerves =
            new CommodityDefinition( 128922519, "M_TissueSample_Nerves", Salvage, 0, false );

        public static CommodityDefinition S_TissueSample_Cells =
            new CommodityDefinition( 128922520, "S_TissueSample_Cells", Salvage, 0, false );

        public static CommodityDefinition S_TissueSample_Surface =
            new CommodityDefinition( 128922521, "S_TissueSample_Surface", Salvage, 0, false );

        public static CommodityDefinition S_TissueSample_Core =
            new CommodityDefinition( 128922522, "S_TissueSample_Core", Salvage, 0, false );

        public static CommodityDefinition P_ParticulateSample =
            new CommodityDefinition( 128922523, "P_ParticulateSample", Salvage, 0, false );

        public static CommodityDefinition S9_TissueSample_Shell =
            new CommodityDefinition( 128922781, "S9_TissueSample_Shell", Salvage, 0, false );

        public static CommodityDefinition M3_TissueSample_Membrane =
            new CommodityDefinition( 128922782, "M3_TissueSample_Membrane", Salvage, 0, false );

        public static CommodityDefinition M3_TissueSample_Mycelium =
            new CommodityDefinition( 128922783, "M3_TissueSample_Mycelium", Salvage, 0, false );

        public static CommodityDefinition M3_TissueSample_Spores =
            new CommodityDefinition( 128922784, "M3_TissueSample_Spores", Salvage, 0, false );

        public static CommodityDefinition S6_TissueSample_Mesoglea =
            new CommodityDefinition( 128922785, "S6_TissueSample_Mesoglea", Salvage, 0, false );

        public static CommodityDefinition S6_TissueSample_Cells =
            new CommodityDefinition( 128922786, "S6_TissueSample_Cells", Salvage, 0, false );

        public static CommodityDefinition S6_TissueSample_Coenosarc =
            new CommodityDefinition( 128922787, "S6_TissueSample_Coenosarc", Salvage, 0, false );

        public static CommodityDefinition AncientRelicTG =
            new CommodityDefinition( 129015433, "AncientRelicTG", Salvage, 4798, false );

        public static CommodityDefinition ThargoidTissueSampleType5 =
            new CommodityDefinition( 129019258, "ThargoidTissueSampleType5", Salvage, 98368, false, true );

        public static CommodityDefinition ThargoidGeneratorTissueSample =
            new CommodityDefinition( 129019259, "ThargoidGeneratorTissueSample", Salvage, 67680, false, true );

        public static CommodityDefinition UnocuppiedEscapePod =
            new CommodityDefinition( 129022087, "UnocuppiedEscapePod", Salvage, 3900, false );

        public static CommodityDefinition UnknownMineral =
            new CommodityDefinition( 129022408, "UnknownMineral", Salvage, 31986, false, true );

        public static CommodityDefinition UnknownRefinedMineral =
            new CommodityDefinition( 129022409, "UnknownRefinedMineral", Salvage, 158421, false, true );

        // Items for which we do not have pricing
        public static CommodityDefinition ThargoidTissueSampleType6 =
            new CommodityDefinition( 129022395, "ThargoidTissueSampleType6", Salvage, 0, false, true );

        public static CommodityDefinition ThargoidTissueSampleType7 =
            new CommodityDefinition( 129022396, "ThargoidTissueSampleType7", Salvage, 0, false, true );

        public static CommodityDefinition ThargoidTissueSampleType9a =
            new CommodityDefinition( 129022398, "ThargoidTissueSampleType9a", Salvage, 0, false, true );

        public static CommodityDefinition ThargoidTissueSampleType9b =
            new CommodityDefinition( 129022399, "ThargoidTissueSampleType9b", Salvage, 0, false, true );

        public static CommodityDefinition ThargoidTissueSampleType9c =
            new CommodityDefinition( 129022400, "ThargoidTissueSampleType9c", Salvage, 0, false, true );

        public static CommodityDefinition ThargoidTissueSampleType10a =
            new CommodityDefinition( 129022402, "ThargoidTissueSampleType10a", Salvage, 0, false, true );

        public static CommodityDefinition ThargoidTissueSampleType10b =
            new CommodityDefinition( 129022403, "ThargoidTissueSampleType10b", Salvage, 0, false, true );

        public static CommodityDefinition ThargoidTissueSampleType10c =
            new CommodityDefinition( 129022404, "ThargoidTissueSampleType10c", Salvage, 0, false, true );

        public static CommodityDefinition UnknownSack =
            new CommodityDefinition( 129022405, "UnknownSack", Salvage, 0, false, true );

        public static CommodityDefinition ThargoidPod =
            new CommodityDefinition( 129022406, "ThargoidPod", Salvage, 0, false );

        public static CommodityDefinition CoralSap =
            new CommodityDefinition( 129022407, "CoralSap", Salvage, 0, false, true );

        public static CommodityDefinition ThargoidTitanDriveComponent =
            new CommodityDefinition( 129030459, "ThargoidTitanDriveComponent", Salvage, 0, false, true );

        public static CommodityDefinition ThargoidCystSpecimen =
            new CommodityDefinition( 129030460, "ThargoidCystSpecimen", Salvage, 0, false, true );

        public static CommodityDefinition ThargoidBoneFragments =
            new CommodityDefinition( 129030461, "ThargoidBoneFragments", Salvage, 0, false, true );

        public static CommodityDefinition ThargoidOrganSample =
            new CommodityDefinition( 129030462, "ThargoidOrganSample", Salvage, 0, false, true );

        #endregion

        #region Slaves

        public static CommodityDefinition Slaves =
            new CommodityDefinition( 128049243, "Slaves", CommodityCategory.Slaves, 10584, false );

        public static CommodityDefinition ImperialSlaves =
            new CommodityDefinition( 128667728, "ImperialSlaves", CommodityCategory.Slaves, 15984, false );

        public static CommodityDefinition MasterChefs =
            new CommodityDefinition( 128672316, "MasterChefs", CommodityCategory.Slaves, 20590, true );

        #endregion

        #region Technology

        public static CommodityDefinition ComputerComponents =
            new CommodityDefinition( 128049225, "ComputerComponents", Technology, 513, false );

        public static CommodityDefinition HazardousEnvironmentSuits =
            new CommodityDefinition( 128049226, "HazardousEnvironmentSuits", Technology, 340, false );

        public static CommodityDefinition Robotics =
            new CommodityDefinition( 128049227, "Robotics", Technology, 1856, false );

        public static CommodityDefinition AutoFabricators =
            new CommodityDefinition( 128049228, "AutoFabricators", Technology, 3734, false );

        public static CommodityDefinition AnimalMonitors =
            new CommodityDefinition( 128049229, "AnimalMonitors", Technology, 324, false );

        public static CommodityDefinition AquaponicSystems =
            new CommodityDefinition( 128049230, "AquaponicSystems", Technology, 314, false );

        public static CommodityDefinition AdvancedCatalysers =
            new CommodityDefinition( 128049231, "AdvancedCatalysers", Technology, 2947, false );

        public static CommodityDefinition TerrainEnrichmentSystems =
            new CommodityDefinition( 128049232, "TerrainEnrichmentSystems", Technology, 4887, false );

        public static CommodityDefinition ResonatingSeparators =
            new CommodityDefinition( 128049671, "ResonatingSeparators", Technology, 5958, false );

        public static CommodityDefinition BioReducingLichen =
            new CommodityDefinition( 128049672, "BioReducingLichen", Technology, 998, false );

        public static CommodityDefinition XiheCompanions =
            new CommodityDefinition( 128667075, "XiheCompanions", Technology, 11058, true );

        public static CommodityDefinition AZCancriFormula42 =
            new CommodityDefinition( 128667080, "AZCancriFormula42", Technology, 12440, true );

        public static CommodityDefinition MuTomImager =
            new CommodityDefinition( 128672310, "MuTomImager", Technology, 6353, false );

        public static CommodityDefinition StructuralRegulators =
            new CommodityDefinition( 128672311, "StructuralRegulators", Technology, 1791, false );

        public static CommodityDefinition Nanobreakers =
            new CommodityDefinition( 128673871, "Nanobreakers", Technology, 639, false );

        public static CommodityDefinition TelemetrySuite =
            new CommodityDefinition( 128673872, "TelemetrySuite", Technology, 2080, false );

        public static CommodityDefinition MicroControllers =
            new CommodityDefinition( 128673873, "MicroControllers", Technology, 3274, false );

        public static CommodityDefinition IonDistributor =
            new CommodityDefinition( 128673874, "IonDistributor", Technology, 1133, false );

        public static CommodityDefinition DiagnosticSensor =
            new CommodityDefinition( 128673875, "DiagnosticSensor", Technology, 4337, false );

        public static CommodityDefinition MedicalDiagnosticEquipment =
            new CommodityDefinition( 128682047, "MedicalDiagnosticEquipment", Technology, 2848, false );

        public static CommodityDefinition ClassifiedExperimentalEquipment =
            new CommodityDefinition( 129002574, "ClassifiedExperimentalEquipment", Technology, 0, true );

        #endregion

        #region Textiles

        public static CommodityDefinition Leather =
            new CommodityDefinition( 128049190, "Leather", Textiles, 205, false );

        public static CommodityDefinition NaturalFabrics =
            new CommodityDefinition( 128049191, "NaturalFabrics", Textiles, 439, false );

        public static CommodityDefinition SyntheticFabrics =
            new CommodityDefinition( 128049193, "SyntheticFabrics", Textiles, 211, false );

        public static CommodityDefinition BelalansRayLeather =
            new CommodityDefinition( 128667061, "BelalansRayLeather", Textiles, 8519, true );

        public static CommodityDefinition DamnaCarapaces =
            new CommodityDefinition( 128667062, "DamnaCarapaces", Textiles, 8120, true );

        public static CommodityDefinition RapaBaoSnakeSkins =
            new CommodityDefinition( 128667063, "RapaBaoSnakeSkins", Textiles, 8285, true );

        public static CommodityDefinition VanayequiRhinoFur =
            new CommodityDefinition( 128667064, "VanayequiRhinoFur", Textiles, 8331, true );

        public static CommodityDefinition BankiAmphibiousLeather =
            new CommodityDefinition( 128667674, "BankiAmphibiousLeather", Textiles, 8338, true );

        public static CommodityDefinition TiegfriesSynthSilk =
            new CommodityDefinition( 128667695, "TiegfriesSynthSilk", Textiles, 8478, true );

        public static CommodityDefinition ChameleonCloth =
            new CommodityDefinition( 128667710, "ChameleonCloth", Textiles, 9071, true );

        public static CommodityDefinition ConductiveFabrics =
            new CommodityDefinition( 128682044, "ConductiveFabrics", Textiles, 507, false );

        public static CommodityDefinition MilitaryGradeFabrics =
            new CommodityDefinition( 128682045, "MilitaryGradeFabrics", Textiles, 708, false );

        #endregion

        #region Waste

        public static CommodityDefinition Biowaste =
            new CommodityDefinition( 128049244, "Biowaste", Waste, 63, false );

        public static CommodityDefinition ToxicWaste =
            new CommodityDefinition( 128049245, "ToxicWaste", Waste, 287, false );

        public static CommodityDefinition ChemicalWaste =
            new CommodityDefinition( 128049246, "ChemicalWaste", Waste, 131, false );

        public static CommodityDefinition Scrap = 
            new CommodityDefinition( 128049248, "Scrap", Waste, 48, false );

        #endregion

        #region Weapons

        public static CommodityDefinition PersonalWeapons =
            new CommodityDefinition( 128049233, "PersonalWeapons", Weapons, 4632, false );

        public static CommodityDefinition BattleWeapons =
            new CommodityDefinition( 128049234, "BattleWeapons", Weapons, 7259, false );

        public static CommodityDefinition ReactiveArmour =
            new CommodityDefinition( 128049235, "ReactiveArmour", Weapons, 2113, false );

        public static CommodityDefinition NonLethalWeapons =
            new CommodityDefinition( 128049236, "NonLethalWeapons", Weapons, 1837, false );

        public static CommodityDefinition BorasetaniPathogenetics =
            new CommodityDefinition( 128667032, "BorasetaniPathogenetics", Weapons, 13679, true );

        public static CommodityDefinition HIP118311Swarm =
            new CommodityDefinition( 128667033, "HIP118311Swarm", Weapons, 13448, true );
        
        public static CommodityDefinition HolvaDuellingBlades =
            new CommodityDefinition( 128667070, "HolvaDuellingBlades", Weapons, 12493, true );

        public static CommodityDefinition KamorinHistoricWeapons =
            new CommodityDefinition( 128667071, "KamorinHistoricWeapons", Weapons, 9766, true );

        public static CommodityDefinition GilyaSignatureWeapons =
            new CommodityDefinition( 128667072, "GilyaSignatureWeapons", Weapons, 13038, true );

        public static CommodityDefinition Landmines =
            new CommodityDefinition( 128672312, "Landmines", Weapons, 4602, false );

        #endregion

        [ PublicAPI, JsonProperty( "category" ) ]
        public readonly CommodityCategory Category;

        public string category => Category.localizedName;

        [ PublicAPI( "True if the commodity is a rare market commodity" ) ]
        public readonly bool rare;

        [ PublicAPI( "True if the commodity is known to be corrosive" ) ]
        public readonly bool corrosive;

        // The average price of a commodity can change - thus this cannot be read only.
        // Instead, this value should be updated whenever revised data is received.
        [ PublicAPI( "The latest known average market price for the commodity" ) ]
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
            if ( CommoditiesByEliteID.TryGetValue( id, out CommodityDefinition commodityDefinition ) )
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

            string normalizedName = NormalizedName( name );

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

            if ( result == null )
            {
                result = ResourceBasedLocalizedEDName<CommodityDefinition>.FromEDName( normalizedName );
            }

            return result;
        }

        private static bool ignoredCommodity ( string name )
        {
            switch ( name )
            {
                case "legal drugs"
                    : // This is a commodity category but in some older sql databases it is listed as a commodity
                    return true;
                default:
                    return false;
            }
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

        new public static CommodityDefinition FromEDName ( string rawName )
        {
            if ( string.IsNullOrEmpty( rawName ) ) { return null; }

            string edName = NormalizedName( rawName );
            return ResourceBasedLocalizedEDName<CommodityDefinition>.FromEDName( edName );
        }

        public static bool EDNameExists ( string edName )
        {
            if ( string.IsNullOrEmpty( edName ) ) { return false; }

            return AllOfThem.Any( v =>
                string.Equals( v.edname, titiedEDName( edName ), StringComparison.InvariantCultureIgnoreCase ) );
        }

        private static string titiedEDName ( string edName )
        {
            return edName?.ToLowerInvariant().Replace( "$", "" ).Replace( ";", "" ).Replace( "_name", "" );
        }
    }
}

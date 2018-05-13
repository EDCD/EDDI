using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    using static CommodityCategory;

    public class CommodityDefinition : ResourceBasedLocalizedEDName<CommodityDefinition>
    {
        static CommodityDefinition()
        {
            resourceManager = Properties.Commodities.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new CommodityDefinition(0, 0, edname, Unknown);
            CommoditiesByEliteID = new Dictionary<long, CommodityDefinition>();

            // 2xxxxxxxx & 3xxxxxxxx series Frontier IDs are placeholders, to use until an actual Frontier ID is identified
            // Check https://eddb.io/archive/v5/commodities.json (for any undefined EDDBID's or undefined FDevID's) and https://github.com/EDCD/FDevIDs (for any undefined FDevID's)
            var _ = new List<CommodityDefinition>
            {
                new CommodityDefinition(128049204, 1, "Explosives", Chemicals, 261, false),
                new CommodityDefinition(128049202, 2, "HydrogenFuel", Chemicals, 110, false),
                new CommodityDefinition(128049203, 3, "MineralOil", Chemicals, 181, false),
                new CommodityDefinition(128049205, 4, "Pesticides", Chemicals, 241, false),
                new CommodityDefinition(128049241, 5, "Clothing", ConsumerItems, 285, false),
                new CommodityDefinition(128049240, 6, "ConsumerTechnology", ConsumerItems, 6769, false),
                new CommodityDefinition(128049238, 7, "DomesticAppliances", ConsumerItems, 487, false),
                new CommodityDefinition(128049214, 8, "Beer", Narcotics, 186, false),
                new CommodityDefinition(128049216, 9, "Liquor", Narcotics, 587, false),
                new CommodityDefinition(128049212, 10, "BasicNarcotics", Narcotics, 9966, false),
                new CommodityDefinition(128049213, 11, "Tobacco", Narcotics, 5035, false),
                new CommodityDefinition(128049215, 12, "Wine", Narcotics, 260, false),
                new CommodityDefinition(128049177, 13, "Algae", Foods, 137, false),
                new CommodityDefinition(128049182, 14, "Animalmeat", Foods, 1292, false),
                new CommodityDefinition(128049189, 15, "Coffee", Foods, 1279, false),
                new CommodityDefinition(128049183, 16, "Fish", Foods, 406, false),
                new CommodityDefinition(128049184, 17, "FoodCartridges", Foods, 105, false),
                new CommodityDefinition(128049178, 18, "FruitAndVegetables", Foods, 312, false),
                new CommodityDefinition(128049180, 19, "Grain", Foods, 210, false),
                new CommodityDefinition(128049185, 20, "SyntheticMeat", Foods, 271, false),
                new CommodityDefinition(128049188, 21, "Tea", Foods, 1467, false),
                new CommodityDefinition(128049197, 22, "Polymers", IndustrialMaterials, 171, false),
                new CommodityDefinition(128049199, 23, "Semiconductors", IndustrialMaterials, 967, false),
                new CommodityDefinition(128049200, 24, "Superconductors", IndustrialMaterials, 6609, false),
                new CommodityDefinition(128064028, 25, "AtmosphericExtractors", Machinery, 357, false),
                new CommodityDefinition(128049222, 26, "CropHarvesters", Machinery, 2021, false),
                new CommodityDefinition(128049223, 27, "MarineSupplies", Machinery, 3916, false),
                new CommodityDefinition(128049220, 28, "HeliostaticFurnaces", Machinery, 236, false),
                new CommodityDefinition(128049221, 29, "MineralExtractors", Machinery, 443, false),
                new CommodityDefinition(128049217, 30, "PowerGenerators", Machinery, 458, false),
                new CommodityDefinition(128049218, 31, "WaterPurifiers", Machinery, 258, false),
                new CommodityDefinition(128049208, 32, "AgriculturalMedicines", Medicines, 1038, false),
                new CommodityDefinition(128049210, 33, "BasicMedicines", Medicines, 279, false),
                new CommodityDefinition(128049670, 34, "CombatStabilisers", Medicines, 3505, false),
                new CommodityDefinition(128049209, 35, "PerformanceEnhancers", Medicines, 6816, false),
                new CommodityDefinition(128049669, 36, "ProgenitorCells", Medicines, 6779, false),
                new CommodityDefinition(128049176, 37, "Aluminium", Metals, 340, false),
                new CommodityDefinition(128049168, 38, "Beryllium", Metals, 8288, false),
                new CommodityDefinition(128049162, 39, "Cobalt", Metals, 647, false),
                new CommodityDefinition(128049175, 40, "Copper", Metals, 481, false),
                new CommodityDefinition(128049170, 41, "Gallium", Metals, 5135, false),
                new CommodityDefinition(128049154, 42, "Gold", Metals, 9401, false),
                new CommodityDefinition(128049169, 43, "Indium", Metals, 5727, false),
                new CommodityDefinition(128049173, 44, "Lithium", Metals, 1596, false),
                new CommodityDefinition(128049153, 45, "Palladium", Metals, 13298, false),
                new CommodityDefinition(128049152, 46, "Platinum", Metals, 19279, false),
                new CommodityDefinition(128049155, 47, "Silver", Metals, 4775, false),
                new CommodityDefinition(128049171, 48, "Tantalum", Metals, 3962, false),
                new CommodityDefinition(128049174, 49, "Titanium", Metals, 1006, false),
                new CommodityDefinition(128049172, 50, "Uranium", Metals, 2705, false),
                new CommodityDefinition(128049165, 51, "Bauxite", Minerals, 120, false),
                new CommodityDefinition(128049156, 52, "Bertrandite", Minerals, 2374, false),
                new CommodityDefinition(128049159, 53, "Coltan", Minerals, 1319, false),
                new CommodityDefinition(128049158, 54, "Gallite", Minerals, 1819, false),
                new CommodityDefinition(128049157, 55, "Indite", Minerals, 2088, false),
                new CommodityDefinition(128049161, 56, "Lepidolite", Minerals, 544, false),
                new CommodityDefinition(128049163, 57, "Rutile", Minerals, 299, false),
                new CommodityDefinition(128049160, 58, "Uraninite", Minerals, 836, false),
                new CommodityDefinition(128667728, 59, "ImperialSlaves", Slaves, 15984, false),
                new CommodityDefinition(128049243, 60, "Slaves", Slaves, 10584, false),
                new CommodityDefinition(128049231, 61, "AdvancedCatalysers", Technology, 2947, false),
                new CommodityDefinition(128049229, 62, "AnimalMonitors", Technology, 324, false),
                new CommodityDefinition(128049230, 63, "AquaponicSystems", Technology, 314, false),
                new CommodityDefinition(128049228, 64, "AutoFabricators", Technology, 3734, false),
                new CommodityDefinition(128049672, 65, "BioReducingLichen", Technology, 998, false),
                new CommodityDefinition(128049225, 66, "ComputerComponents", Technology, 513, false),
                new CommodityDefinition(128049226, 67, "HazardousEnvironmentSuits", Technology, 340, false),
                new CommodityDefinition(128049232, 68, "TerrainEnrichmentSystems", Technology, 4887, false),
                new CommodityDefinition(128049671, 69, "ResonatingSeparators", Technology, 5958, false),
                new CommodityDefinition(128049227, 70, "Robotics", Technology, 1856, false),
                new CommodityDefinition(128049190, 72, "Leather", Textiles, 205, false),
                new CommodityDefinition(128049191, 73, "NaturalFabrics", Textiles, 439, false),
                new CommodityDefinition(128049193, 74, "SyntheticFabrics", Textiles, 211, false),
                new CommodityDefinition(128049244, 75, "Biowaste", Waste, 63, false),
                new CommodityDefinition(128049246, 76, "ChemicalWaste", Waste, 131, false),
                new CommodityDefinition(128049248, 77, "Scrap", Waste, 48, false),
                new CommodityDefinition(128049236, 78, "NonLethalWeapons", Weapons, 1837, false),
                new CommodityDefinition(128049233, 79, "PersonalWeapons", Weapons, 4632, false),
                new CommodityDefinition(128049235, 80, "ReactiveArmour", Weapons, 2113, false),
                new CommodityDefinition(128049234, 81, "BattleWeapons", Weapons, 7259, false),
                new CommodityDefinition(128049245, 82, "ToxicWaste", Waste, 287, false),
                new CommodityDefinition(128668550, 83, "Painite", Minerals, 40508, false),
                new CommodityDefinition(128066403, 84, "Drones", NonMarketable, 101, false),
                new CommodityDefinition(128666746, 85, "EraninPearlWhisky", Narcotics, 9040, true),
                new CommodityDefinition(128667071, 86, "KamorinHistoricWeapons", Weapons, 9766, true),
                new CommodityDefinition(128667760, 87, "TransgenicOnionHead", Narcotics, 8472, true),
                new CommodityDefinition(128667676, 88, "MotronaExperienceJelly", Narcotics, 13129, true),
                new CommodityDefinition(128667029, 89, "OnionHead", Narcotics, 8437, true),
                new CommodityDefinition(128667082, 90, "RusaniOldSmokey", Narcotics, 11994, true),
                new CommodityDefinition(128667030, 91, "TarachTorSpice", Narcotics, 8642, true),
                new CommodityDefinition(128667069, 92, "TerraMaterBloodBores", Medicines, 13414, true),
                new CommodityDefinition(128667031, 93, "WolfFesh", Narcotics, 8399, true),
                new CommodityDefinition(128667035, 94, "WuthieloKuFroth", Narcotics, 8194, true),
                new CommodityDefinition(128668548, 95, "AiRelics", Salvage, 138613, false),
                new CommodityDefinition(128668551, 96, "Antiquities", Salvage, 115511, false),
                new CommodityDefinition(128671118, 97, "Osmium", Metals, 7591, false),
                new CommodityDefinition(128671443, 98, "SAP8CoreContainer", Salvage, 59196, false),
                new CommodityDefinition(128671444, 99, "TrinketsOfFortune", Salvage, 1428, false),
                new CommodityDefinition(128666754, 100, "USSCargoTradeData", Salvage, 2790, false),
                new CommodityDefinition(128672308, 101, "ThermalCoolingUnits", Machinery, 256, false),
                new CommodityDefinition(128672313, 102, "SkimerComponents", Machinery, 859, false),
                new CommodityDefinition(128672307, 103, "GeologicalEquipment", Machinery, 1661, false),
                new CommodityDefinition(128672311, 104, "StructuralRegulators", Technology, 1791, false),
                new CommodityDefinition(128672297, 105, "Pyrophyllite", Minerals, 1565, false),
                new CommodityDefinition(128672296, 106, "Moissanite", Minerals, 8273, false),
                new CommodityDefinition(128672295, 107, "Goslarite", Minerals, 916, false),
                new CommodityDefinition(128672294, 108, "Cryolite", Minerals, 2266, false),
                new CommodityDefinition(128672301, 109, "Thorium", Metals, 11513, false),
                new CommodityDefinition(128672299, 110, "Thallium", Metals, 3618, false),
                new CommodityDefinition(128672298, 111, "Lanthanum", Metals, 8766, false),
                new CommodityDefinition(128672300, 112, "Bismuth", Metals, 2284, false),
                new CommodityDefinition(128672306, 113, "BootlegLiquor", Narcotics, 855, false),
                new CommodityDefinition(128672701, 114, "MetaAlloys", IndustrialMaterials, 88148, false),
                new CommodityDefinition(128672302, 115, "CeramicComposites", IndustrialMaterials, 232, false),
                new CommodityDefinition(128672314, 116, "EvacuationShelter", ConsumerItems, 343, false),
                new CommodityDefinition(128672303, 117, "SyntheticReagents", Chemicals, 6675, false),
                new CommodityDefinition(128672305, 118, "SurfaceStabilisers", Chemicals, 467, false),
                new CommodityDefinition(128672309, 119, "BuildingFabricators", Machinery, 980, false),
                new CommodityDefinition(128672312, 121, "Landmines", Weapons, 4602, false),
                new CommodityDefinition(128672304, 122, "NerveAgents", Chemicals, 13526, false),
                new CommodityDefinition(128672310, 124, "MuTomImager", Technology, 6353, false),
                new CommodityDefinition(128666747, 125, "LavianBrandy", Narcotics, 10365, true),
                new CommodityDefinition(128666752, 126, "USSCargoBlackBox", Salvage, 6995, false),
                new CommodityDefinition(128666755, 127, "USSCargoMilitaryPlans", Salvage, 9413, false),
                new CommodityDefinition(128666756, 128, "USSCargoAncientArtefact", Salvage, 8183, false),
                new CommodityDefinition(128666757, 129, "USSCargoRareArtwork", Salvage, 7774, false),
                new CommodityDefinition(128666758, 130, "USSCargoExperimentalChemicals", Salvage, 3524, false),
                new CommodityDefinition(128666759, 131, "USSCargoRebelTransmissions", Salvage, 4068, false),
                new CommodityDefinition(128666760, 132, "USSCargoPrototypeTech", Salvage, 10696, false),
                new CommodityDefinition(128666761, 133, "USSCargoTechnicalBlueprints", Salvage, 6333, false),
                new CommodityDefinition(128667019, 134, "HIP10175BushMeat", Foods, 9382, true),
                new CommodityDefinition(128667020, 135, "AlbinoQuechuaMammoth", Foods, 9687, true),
                new CommodityDefinition(128667021, 136, "UtgaroarMillenialEggs", Foods, 9163, true),
                new CommodityDefinition(128667022, 137, "WitchhaulKobeBeef", Foods, 11085, true),
                new CommodityDefinition(128667023, 138, "KarsukiLocusts", Foods, 8543, true),
                new CommodityDefinition(128667024, 139, "GiantIrukamaSnails", Foods, 9174, true),
                new CommodityDefinition(128667025, 140, "BaltahSineVacuumKrill", Foods, 8479, true),
                new CommodityDefinition(128667026, 141, "CetiRabbits", Foods, 9079, true),
                new CommodityDefinition(128667027, 142, "KachiriginFilterLeeches", Medicines, 8227, true),
                new CommodityDefinition(128667028, 143, "LyraeWeed", Narcotics, 8937, true),
                new CommodityDefinition(128667032, 144, "BorasetaniPathogenetics", Weapons, 13679, true),
                new CommodityDefinition(128667033, 145, "HIP118311Swarm", Weapons, 13448, true),
                new CommodityDefinition(128667034, 146, "KonggaAle", Narcotics, 8310, true),
                new CommodityDefinition(128667036, 147, "AlacarakmoSkinArt", ConsumerItems, 8899, true),
                new CommodityDefinition(128667037, 148, "EleuThermals", ConsumerItems, 8507, true),
                new CommodityDefinition(128667038, 149, "EshuUmbrellas", ConsumerItems, 9343, true),
                new CommodityDefinition(128667039, 150, "KaretiiCouture", ConsumerItems, 11582, true),
                new CommodityDefinition(128667040, 151, "NjangariSaddles", ConsumerItems, 8356, true),
                new CommodityDefinition(128667041, 152, "AnyNaCoffee", Foods, 9160, true),
                new CommodityDefinition(128667042, 153, "CD-75KittenBrandCoffee", Foods, 9571, true),
                new CommodityDefinition(128667043, 154, "GomanYauponCoffee", Foods, 8921, true),
                new CommodityDefinition(128667044, 155, "VolkhabBeeDrones", Machinery, 10198, true),
                new CommodityDefinition(128667045, 156, "KinagoViolins", ConsumerItems, 13030, true),
                new CommodityDefinition(128667046, 157, "NgunaModernAntiques", ConsumerItems, 8545, true),
                new CommodityDefinition(128667047, 158, "RajukruMulti-Stoves", ConsumerItems, 8378, true),
                new CommodityDefinition(128667048, 159, "TiolceWaste2PasteUnits", ConsumerItems, 8710, true),
                new CommodityDefinition(128667049, 160, "ChiEridaniMarinePaste", Foods, 8450, true),
                new CommodityDefinition(128667050, 161, "EsusekuCaviar", Foods, 9625, true),
                new CommodityDefinition(128667051, 162, "LiveHecateSeaWorms", Foods, 8737, true),
                new CommodityDefinition(128667052, 163, "HelvetitjPearls", Foods, 10450, true),
                new CommodityDefinition(128667053, 164, "HIPProto-Squid", Foods, 8497, true),
                new CommodityDefinition(128667054, 165, "CoquimSpongiformVictuals", Foods, 8077, true),
                new CommodityDefinition(128667055, 166, "EdenApplesOfAerial", Foods, 8331, true),
                new CommodityDefinition(128667056, 167, "NeritusBerries", Foods, 8497, true),
                new CommodityDefinition(128667057, 168, "OchoengChillies", Foods, 8601, true),
                new CommodityDefinition(128667058, 169, "DeuringasTruffles", Foods, 9232, true),
                new CommodityDefinition(128667059, 170, "HR7221Wheat", Foods, 8190, true),
                new CommodityDefinition(128667060, 171, "JarouaRice", Foods, 8169, true),
                new CommodityDefinition(128667061, 172, "BelalansRayLeather", Textiles, 8519, true),
                new CommodityDefinition(128667062, 173, "DamnaCarapaces", Textiles, 8120, true),
                new CommodityDefinition(128667063, 174, "RapaBaoSnakeSkins", Textiles, 8285, true),
                new CommodityDefinition(128667064, 175, "VanayequiCeratomorphaFur", Textiles, 8331, true),
                new CommodityDefinition(128667065, 176, "BastSnakeGin", Narcotics, 8659, true),
                new CommodityDefinition(128667066, 177, "ThrutisCream", Narcotics, 8550, true),
                new CommodityDefinition(128667067, 178, "WulpaHyperboreSystems", Machinery, 8726, true),
                new CommodityDefinition(128667068, 179, "AganippeRush", Medicines, 14220, true),
                new CommodityDefinition(128667070, 180, "HolvaDuellingBlades", Weapons, 12493, true),
                new CommodityDefinition(128667072, 181, "GilyaSignatureWeapons", Weapons, 13038, true),
                new CommodityDefinition(128667073, 182, "DeltaPhoenicisPalms", Chemicals, 8188, true),
                new CommodityDefinition(128667074, 183, "ToxandjiVirocide", Chemicals, 8275, true),
                new CommodityDefinition(128667075, 184, "xihecompanions", Technology, 11058, true),
                new CommodityDefinition(128667076, 185, "SanumaDecorativeMeat", Foods, 8504, true),
                new CommodityDefinition(128667077, 186, "EthgrezeTeaBuds", Foods, 10197, true),
                new CommodityDefinition(128667078, 187, "CeremonialHeikeTea", Foods, 9251, true),
                new CommodityDefinition(128667079, 188, "TanmarkTranquilTea", Foods, 9177, true),
                new CommodityDefinition(128667080, 189, "AzCancriFormula42", Technology, 12440, true),
                new CommodityDefinition(128667081, 190, "KamitraCigars", Narcotics, 12282, true),
                new CommodityDefinition(128667083, 191, "YasoKondiLeaf", Narcotics, 12171, true),
                new CommodityDefinition(128667084, 192, "ChateauDeAegaeon", Narcotics, 8791, true),
                new CommodityDefinition(128667085, 193, "WatersOfShintara", Medicines, 13711, true),
                new CommodityDefinition(128667668, 194, "OphiuchExinoArtefacts", ConsumerItems, 10969, true),
                new CommodityDefinition(128667670, 195, "CetiAepyornisEgg", Foods, 9769, true),
                new CommodityDefinition(128667671, 196, "SaxonWine", Narcotics, 8983, true),
                new CommodityDefinition(128667672, 197, "CentauriMegaGin", Narcotics, 10217, true),
                new CommodityDefinition(128667673, 198, "AnduligaFireWorks", Chemicals, 8519, true),
                new CommodityDefinition(128667674, 199, "BankiAmphibiousLeather", Textiles, 8338, true),
                new CommodityDefinition(128667675, 200, "CherbonesBloodCrystals", Minerals, 16714, true),
                new CommodityDefinition(128667677, 201, "GeawenDanceDust", Narcotics, 8618, true),
                new CommodityDefinition(128667678, 202, "GerasianGueuzeBeer", Narcotics, 8215, true),
                new CommodityDefinition(128667679, 203, "HaidneBlackBrew", Foods, 8837, true),
                new CommodityDefinition(128667680, 204, "HavasupaiDreamCatcher", ConsumerItems, 14639, true),
                new CommodityDefinition(128667681, 205, "BurnhamBileDistillate", Narcotics, 8466, true),
                new CommodityDefinition(128667682, 206, "HIPOrganophosphates", Chemicals, 8169, true),
                new CommodityDefinition(128667683, 207, "JaradharrePuzzleBox", ConsumerItems, 16816, true),
                new CommodityDefinition(128667684, 208, "KoroKungPellets", Chemicals, 8067, true),
                new CommodityDefinition(128667685, 209, "VoidExtractCoffee", Foods, 9554, true),
                new CommodityDefinition(128667686, 210, "HonestyPills", Medicines, 8860, true),
                new CommodityDefinition(128667687, 211, "NonEuclidianExotanks", Machinery, 8526, true),
                new CommodityDefinition(128667688, 212, "LTTHypersweet", Foods, 8054, true),
                new CommodityDefinition(128667689, 213, "MechucosHighTea", Foods, 8846, true),
                new CommodityDefinition(128667690, 214, "MedbStarlube", IndustrialMaterials, 8191, true),
                new CommodityDefinition(128667691, 215, "MokojingBeastFeast", Foods, 9788, true),
                new CommodityDefinition(128667692, 216, "MukusubiiChitin-Os", Foods, 8359, true),
                new CommodityDefinition(128667693, 217, "MulachiGiantFungus", Foods, 7957, true),
                new CommodityDefinition(128667694, 218, "NgadandariFireOpals", Minerals, 19112, true),
                new CommodityDefinition(128667695, 219, "TiegfriesSynthSilk", Textiles, 8478, true),
                new CommodityDefinition(128667696, 220, "UzumokuLow-GWings", ConsumerItems, 13845, true),
                new CommodityDefinition(128667697, 221, "VHerculisBodyRub", Medicines, 8010, true),
                new CommodityDefinition(128667698, 222, "WheemeteWheatCakes", Foods, 8081, true),
                new CommodityDefinition(128667699, 223, "VegaSlimweed", Medicines, 9588, true),
                new CommodityDefinition(128667700, 224, "AltairianSkin", ConsumerItems, 8432, true),
                new CommodityDefinition(128667701, 225, "PavonisEarGrubs", Narcotics, 8364, true),
                new CommodityDefinition(128667702, 226, "JotunMookah", ConsumerItems, 8780, true),
                new CommodityDefinition(128667703, 227, "GiantVerrix", Machinery, 12496, true),
                new CommodityDefinition(128667704, 228, "IndiBourbon", Narcotics, 8806, true),
                new CommodityDefinition(128667705, 229, "AroucaConventualSweets", Foods, 8737, true),
                new CommodityDefinition(128667706, 230, "TauriChimes", Medicines, 8549, true),
                new CommodityDefinition(128667707, 231, "ZeesszeAntGrubGlue", ConsumerItems, 8161, true),
                new CommodityDefinition(128667708, 232, "PantaaPrayerSticks", Medicines, 9177, true),
                new CommodityDefinition(128667709, 233, "FujinTea", Medicines, 8597, true),
                new CommodityDefinition(128667710, 234, "ChameleonCloth", Textiles, 9071, true),
                new CommodityDefinition(128667711, 235, "OrrerianViciousBrew", Foods, 8342, true),
                new CommodityDefinition(128667712, 236, "UszaianTreeGrub", Foods, 8578, true),
                new CommodityDefinition(128667713, 237, "MomusBogSpaniel", ConsumerItems, 9184, true),
                new CommodityDefinition(128667714, 238, "DisoMaCorn", Foods, 8134, true),
                new CommodityDefinition(128667715, 239, "LeestianEvilJuice", Narcotics, 8220, true),
                new CommodityDefinition(128667716, 240, "BlueMilk", Narcotics, 10805, true),
                new CommodityDefinition(128667717, 241, "alieneggs", ConsumerItems, 25067, true),
                new CommodityDefinition(128667718, 242, "AlyaBodySoap", Medicines, 8218, true),
                new CommodityDefinition(128667719, 243, "VidavantianLace", ConsumerItems, 12615, true),
                new CommodityDefinition(128668017, 244, "JaquesQuinentianStill", ConsumerItems, 2108, true),
                new CommodityDefinition(128668018, 245, "SoontillRelics", ConsumerItems, 19885, true),
                new CommodityDefinition(128668547, 246, "UnknownArtifact", Salvage, 290190, false),
                new CommodityDefinition(128668549, 247, "Hafnium178", Metals, 69098, false),
                new CommodityDefinition(128668552, 248, "MilitaryIntelligence", Salvage, 55527, false),
                new CommodityDefinition(128672121, 249, "TheHuttonMug", ConsumerItems, 7986, true),
                new CommodityDefinition(128672122, 250, "SothisCrystallineGold", Metals, 19112, true),
                new CommodityDefinition(128672123, 251, "WreckageComponents", Salvage, 394, false),
                new CommodityDefinition(128672124, 252, "EncriptedDataStorage", Salvage, 806, false),
                new CommodityDefinition(128672126, 253, "PersonalEffects", Salvage, 379, false),
                new CommodityDefinition(128672127, 254, "ComercialSamples", Salvage, 361, false),
                new CommodityDefinition(128672128, 255, "TacticalData", Salvage, 457, false),
                new CommodityDefinition(128672129, 256, "AssaultPlans", Salvage, 446, false),
                new CommodityDefinition(128672130, 257, "EncryptedCorrespondence", Salvage, 372, false),
                new CommodityDefinition(128672131, 258, "DiplomaticBag", Salvage, 572, false),
                new CommodityDefinition(128672132, 259, "ScientificResearch", Salvage, 635, false),
                new CommodityDefinition(128672133, 260, "ScientificSamples", Salvage, 772, false),
                new CommodityDefinition(128672134, 261, "PoliticalPrisoner", Salvage, 5132, false),
                new CommodityDefinition(128672135, 262, "Hostage", Salvage, 2427, false),
                new CommodityDefinition(128672315, 263, "GeologicalSamples", Salvage, 446, false),
                new CommodityDefinition(128672316, 264, "MasterChefs", Slaves, 20590, true),
                new CommodityDefinition(128672432, 265, "CrystallineSpheres", ConsumerItems, 12216, true),
                new CommodityDefinition(128672775, 266, "Taaffeite", Minerals, 20696, false),
                new CommodityDefinition(128672776, 267, "Jadeite", Minerals, 13474, false),
                new CommodityDefinition(128672810, 268, "UnstableDataCore", Salvage, 2427, false),
                new CommodityDefinition(128672812, 269, "OnionheadAlphaStrain", Narcotics, 8437, true),
                new CommodityDefinition(128672125, 270, "OccupiedCryoPod", Salvage, 4474, false),
                new CommodityDefinition(128049166, 271, "Water", Chemicals, 120, false),
                new CommodityDefinition(128673069, 272, "OnionheadBetaStrain", Narcotics, 8437, true),
                new CommodityDefinition(128673845, 273, "Praseodymium", Metals, 7156, false),
                new CommodityDefinition(128673846, 274, "Bromellite", Minerals, 7062, false),
                new CommodityDefinition(128673847, 275, "Samarium", Metals, 6330, false),
                new CommodityDefinition(128673848, 276, "LowTemperatureDiamond", Minerals, 57445, false),
                new CommodityDefinition(128673850, 277, "HydrogenPeroxide", Chemicals, 917, false),
                new CommodityDefinition(128673851, 278, "LiquidOxygen", Chemicals, 263, false),
                new CommodityDefinition(128673852, 279, "MethanolMonohydrateCrystals", Minerals, 2282, false),
                new CommodityDefinition(128673853, 280, "LithiumHydroxide", Minerals, 5646, false),
                new CommodityDefinition(128673854, 281, "MethaneClathrate", Minerals, 629, false),
                new CommodityDefinition(128673855, 282, "InsulatingMembrane", IndustrialMaterials, 7837, false),
                new CommodityDefinition(128673856, 283, "CMMComposite", IndustrialMaterials, 3132, false),
                new CommodityDefinition(128673857, 284, "CoolingHoses", IndustrialMaterials, 403, false),
                new CommodityDefinition(128673858, 285, "NeofabricInsulation", IndustrialMaterials, 2769, false),
                new CommodityDefinition(128673859, 286, "ArticulationMotors", Machinery, 4997, false),
                new CommodityDefinition(128673860, 287, "HNShockMount", Machinery, 406, false),
                new CommodityDefinition(128673861, 288, "EmergencyPowerCells", Machinery, 1011, false),
                new CommodityDefinition(128673862, 289, "PowerConverter", Machinery, 246, false),
                new CommodityDefinition(128673863, 290, "PowerGridAssembly", Machinery, 1684, false),
                new CommodityDefinition(128673864, 291, "PowerTransferConduits", Machinery, 857, false),
                new CommodityDefinition(128673865, 292, "RadiationBaffle", Machinery, 383, false),
                new CommodityDefinition(128673866, 293, "ExhaustManifold", Machinery, 479, false),
                new CommodityDefinition(128673867, 294, "ReinforcedMountingPlate", Machinery, 1074, false),
                new CommodityDefinition(128673868, 295, "HeatsinkInterlink", Machinery, 729, false),
                new CommodityDefinition(128673869, 296, "MagneticEmitterCoil", Machinery, 199, false),
                new CommodityDefinition(128673870, 297, "ModularTerminals", Machinery, 695, false),
                new CommodityDefinition(128673871, 298, "Nanobreakers", Technology, 639, false),
                new CommodityDefinition(128673872, 299, "TelemetrySuite", Technology, 2080, false),
                new CommodityDefinition(128673873, 300, "MicroControllers", Technology, 3274, false),
                new CommodityDefinition(128673874, 301, "IonDistributor", Technology, 1133, false),
                new CommodityDefinition(128673875, 302, "DiagnosticSensor", Technology, 4337, false),
                new CommodityDefinition(128682044, 303, "ConductiveFabrics", Textiles, 507, false),
                new CommodityDefinition(128682045, 304, "MilitaryGradeFabrics", Textiles, 708, false),
                new CommodityDefinition(128682046, 305, "AdvancedMedicines", Medicines, 1259, false),
                new CommodityDefinition(128682047, 306, "MedicalDiagnosticEquipment", Technology, 2848, false),
                new CommodityDefinition(128682048, 307, "SurvivalEquipment", ConsumerItems, 485, false),
                new CommodityDefinition(128682049, 308, "DataCore", Salvage, 2872, false),
                new CommodityDefinition(128682050, 309, "GalacticTravelGuide", Salvage, 332, false),
                new CommodityDefinition(128682051, 310, "MysteriousIdol", Salvage, 15196, false),
                new CommodityDefinition(128682052, 311, "ProhibitedResearchMaterials", Salvage, 46607, false),
                new CommodityDefinition(128682053, 312, "AntimatterContainmentUnit", Salvage, 26608, false),
                new CommodityDefinition(128682054, 313, "SpacePioneerRelics", Salvage, 7342, false),
                new CommodityDefinition(128682055, 314, "FossilRemnants", Salvage, 9927, false),
                new CommodityDefinition(128672159, 10016, "AntiqueJewellery", Salvage, 0, false),
                new CommodityDefinition(128672162, 10018, "GeneBank", Salvage, 0, false),
                new CommodityDefinition(128672163, 10019, "TimeCapsule", Salvage, 0, false),
                new CommodityDefinition(128673876, 315, "UnknownArtifact2", Salvage, 411003, false),
                new CommodityDefinition(128672160, 316, "PreciousGems", Salvage, 109641, false),
                new CommodityDefinition(128740752, 317, "UnknownArtifact3", Salvage, 31350, false),
                new CommodityDefinition(128737288, 318, "UnknownBiologicalMatter", Salvage, 25479, false),
                new CommodityDefinition(128737287, 319, "UnknownResin", Salvage, 18652, false),
                new CommodityDefinition(128737289, 320, "UnknownTechnologySamples", Salvage, 22551, false),
                new CommodityDefinition(128732183, 321, "AncientRelic", Salvage, 0, false),
                new CommodityDefinition(128732184, 322, "AncientOrb", Salvage, 0, false),
                new CommodityDefinition(128732185, 323, "AncientCasket", Salvage, 0, false),
                new CommodityDefinition(128732186, 324, "AncientTablet", Salvage, 0, false),
                new CommodityDefinition(128732187, 325, "AncientUrn", Salvage, 0, false),
                new CommodityDefinition(128732188, 326, "AncientTotem", Salvage, 0, false),
                new CommodityDefinition(128793127, 10022, "ThargoidHeart", Salvage, 0, false),
                new CommodityDefinition(128793128, 10020, "ThargoidTissueSampleType1", Salvage, 14081, false),
                new CommodityDefinition(128793129, 10023, "ThargoidTissueSampleType2", Salvage, 0, false),
                new CommodityDefinition(128793130, 10024, "ThargoidTissueSampleType3", Salvage, 0, false),
                new CommodityDefinition(128672137, 327, "SmallExplorationDataCash", Salvage, 0, false),
                new CommodityDefinition(128672136, 328, "LargeExplorationDataCash", Salvage, 0, false),
                new CommodityDefinition(128672811, 329, "DamagedEscapePod", Salvage, 11912, false),
                new CommodityDefinition(128672161, 330, "EarthRelics", Salvage, 0, false),
                new CommodityDefinition(128824468, 331, "ThargoidScoutTissueSample", Salvage, 15215, false),

                // Items for which we do not have EDDB IDs
                new CommodityDefinition(200000000, 10000, "aislingpromotionalmaterials", Powerplay, 0, false),
                new CommodityDefinition(200000001, 10001, "alliancelegaslativerecords", Powerplay, 0, false),
                new CommodityDefinition(200000002, 10002, "torvaldeeds", Powerplay, 0, false),
                new CommodityDefinition(200000003, 10003, "aislingmediamaterials", Powerplay, 0, false),
                new CommodityDefinition(200000004, 10004, "torvalcommercialcontracts", Powerplay, 0, false),
                new CommodityDefinition(200000012, 10012, "siriuscommercialcontracts", Powerplay, 0, false),
                new CommodityDefinition(200000013, 10013, "siriusindustrialequipment", Powerplay, 0, false),
                new CommodityDefinition(200000014, 10014, "siriusfranchisepackage", Powerplay, 0, false),
                new CommodityDefinition(200000015, 10015, "republicangarisonsupplies", Powerplay, 0, false),
                new CommodityDefinition(200000016, 10016, "lavignygarisonsupplies", Powerplay, 0, false),
                new CommodityDefinition(200000017, 10017, "RajukruStoves", ConsumerItems, 8378, true),
                new CommodityDefinition(200000018, 10018, "KinagoInstruments", ConsumerItems, 13030, true),
                new CommodityDefinition(200000019, 10019, "KachiriginLeaches", Medicines, 8227, true),
                new CommodityDefinition(200000020, 10020, "VanayequiRhinoFur", Textiles, 8331, true),
                new CommodityDefinition(200000021, 10021, "ZeesszeAntGlue", ConsumerItems, 8161, true),
                new CommodityDefinition(200000022, 10022, "imperialprisoner", Salvage, 0, false),
                new CommodityDefinition(200000023, 10023, "undergroundsupport", Powerplay, 0, false),
            };
        }
        private static Dictionary<long, CommodityDefinition> CommoditiesByEliteID;

        public readonly long EliteID;
        public readonly long EDDBID;
        public readonly CommodityCategory category;
        public readonly int avgprice;
        public readonly bool rare;

        // dummy used to ensure that the static constructor has run
        public CommodityDefinition() : this(0, 0, "", Unknown)
        {}

        private CommodityDefinition(long EliteID, long EDDBID, string edname, CommodityCategory Category, int AveragePrice = 0, bool Rare = false) : base(edname, edname)
        {
            this.EDDBID = EDDBID;
            this.category = Category;
            this.avgprice = AveragePrice;
            this.rare = Rare;
            CommoditiesByEliteID[EliteID] = this;
        }

        public static CommodityDefinition CommodityDefinitionFromEliteID(long id)
        {
            try
            {
                return CommoditiesByEliteID[id];
            }
            catch(KeyNotFoundException)
            {
                Logging.Report($"Unrecognized Commodity Definition EliteID {id}");
                throw;
            }
        }
        
        private static string NormalizedName(string rawName)
        {
            return rawName?.ToLowerInvariant()
                ?.Replace("$", "") // Header for types from mining and mission events
                ?.Replace("_name;", "") // Trailer for types from mining and mission events
                ?.Replace(" name;", "");
        }
        
        new public static CommodityDefinition FromName(string name)
        {
            if (name == null)
            {
                return null;
            }

            string normalizedName = NormalizedName(name);

            if (ignoredCommodity(normalizedName))
            {
                return null;
            }
            
            // Now try to fetch the commodity by either ED or real name
            CommodityDefinition result = null;
            if (normalizedName != null)
            {
                result = ResourceBasedLocalizedEDName<CommodityDefinition>.FromName(normalizedName);
            }
            if (result == null)
            {
                result = ResourceBasedLocalizedEDName<CommodityDefinition>.FromEDName(normalizedName);
            }
            return result;
        }

        private static bool ignoredCommodity(string name)
        {
            switch (name)
            {
                case "legal drugs": // This is a commodity category but in some older sql databases it is listed as a commodity
                    return true;
                default:
                    return false;
            }
        }

        new public static CommodityDefinition FromEDName(string rawName)
        {
            string edName = NormalizedName(rawName); 
            return ResourceBasedLocalizedEDName<CommodityDefinition>.FromEDName(edName);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiDataDefinitions
{
    public class CommodityDefinitions
    {
        // Mapping from Elite internal names to common names
        private static Dictionary<string, string> nameMapping = new Dictionary<string, string>
        {
            {"agriculturalmedicines", "agrimedicines"},
            {"atmosphericextractors", "atmosphericprocessors"},
            {"basicnarcotics", "narcotics"},
            {"hazardousenvironmentsuits", "hesuits"},
            {"marinesupplies", "marineequipment"},
            //{"mutomimager", "muonimager"},
            {"skimercomponents", "skimmercomponents"},
            {"terrainenrichmentsystems", "landenrichmentsystems"},
            {"trinketsoffortune", "trinketsofhiddenfortune"},
            //{"unknownartifact", "unknownartefact"},
            //{"usscargoancientartefact", "ancientartefact"},
            //{"usscargoexperimentalchemicals", "experimentalchemicals"},
            //{"usscargomilitaryplans", "militaryplans"},
            //{"usscargoprototypetech", "prototypetech"},
            //{"usscargorebeltransmissions", "rebeltransmissions"},
            //{"usscargotechnicalblueprints", "technicalblueprints"},
            //{"usscargotradedata", "tradedata"},
            {"comercialsamples", "commercialsamples"},
        };

        private static Dictionary<long, Commodity> CommoditiesByEliteID = new Dictionary<long, Commodity>
        // 2xxxxxxxx & 3xxxxxxxx series Frontier IDs are placeholders, to use until an actual Frontier ID is identified
        // Check https://eddb.io/archive/v5/commodities.json for any undefined EDDBID's
        {
            {128049204, new Commodity(1, "Explosives", "Explosives", "Chemicals", 261, false) },
            {128049202, new Commodity(2, "HydrogenFuel", "Hydrogen Fuel", "Chemicals", 110, false) },
            {128049203, new Commodity(3, "MineralOil", "Mineral Oil", "Chemicals", 181, false) },
            {128049205, new Commodity(4, "Pesticides", "Pesticides", "Chemicals", 241, false) },
            {128049241, new Commodity(5, "Clothing", "Clothing", "Consumer Items", 285, false) },
            {128049240, new Commodity(6, "ConsumerTechnology", "Consumer Technology", "Consumer Items", 6769, false) },
            {128049238, new Commodity(7, "DomesticAppliances", "Domestic Appliances", "Consumer Items", 487, false) },
            {128049214, new Commodity(8, "Beer", "Beer", "Narcotics", 186, false) },
            {128049216, new Commodity(9, "Liquor", "Liquor", "Narcotics", 587, false) },
            {128049212, new Commodity(10, "BasicNarcotics", "Narcotics", "Narcotics", 9966, false) },
            {128049213, new Commodity(11, "Tobacco", "Tobacco", "Narcotics", 5035, false) },
            {128049215, new Commodity(12, "Wine", "Wine", "Narcotics", 260, false) },
            {128049177, new Commodity(13, "Algae", "Algae", "Foods", 137, false) },
            {128049182, new Commodity(14, "Animalmeat", "Animal Meat", "Foods", 1292, false) },
            {128049189, new Commodity(15, "Coffee", "Coffee", "Foods", 1279, false) },
            {128049183, new Commodity(16, "Fish", "Fish", "Foods", 406, false) },
            {128049184, new Commodity(17, "FoodCartridges", "Food Cartridges", "Foods", 105, false) },
            {128049178, new Commodity(18, "FruitAndVegetables", "Fruit and Vegetables", "Foods", 312, false) },
            {128049180, new Commodity(19, "Grain", "Grain", "Foods", 210, false) },
            {128049185, new Commodity(20, "SyntheticMeat", "Synthetic Meat", "Foods", 271, false) },
            {128049188, new Commodity(21, "Tea", "Tea", "Foods", 1467, false) },
            {128049197, new Commodity(22, "Polymers", "Polymers", "Industrial Materials", 171, false) },
            {128049199, new Commodity(23, "Semiconductors", "Semiconductors", "Industrial Materials", 967, false) },
            {128049200, new Commodity(24, "Superconductors", "Superconductors", "Industrial Materials", 6609, false) },
            {128064028, new Commodity(25, "AtmosphericExtractors", "Atmospheric Processors", "Machinery", 357, false) },
            {128049222, new Commodity(26, "CropHarvesters", "Crop Harvesters", "Machinery", 2021, false) },
            {128049223, new Commodity(27, "MarineSupplies", "Marine Equipment", "Machinery", 3916, false) },
            {128049220, new Commodity(28, "HeliostaticFurnaces", "Microbial Furnaces", "Machinery", 236, false) },
            {128049221, new Commodity(29, "MineralExtractors", "Mineral Extractors", "Machinery", 443, false) },
            {128049217, new Commodity(30, "PowerGenerators", "Power Generators", "Machinery", 458, false) },
            {128049218, new Commodity(31, "WaterPurifiers", "Water Purifiers", "Machinery", 258, false) },
            {128049208, new Commodity(32, "AgriculturalMedicines", "Agri-Medicines", "Medicines", 1038, false) },
            {128049210, new Commodity(33, "BasicMedicines", "Basic Medicines", "Medicines", 279, false) },
            {128049670, new Commodity(34, "CombatStabilisers", "Combat Stabilisers", "Medicines", 3505, false) },
            {128049209, new Commodity(35, "PerformanceEnhancers", "Performance Enhancers", "Medicines", 6816, false) },
            {128049669, new Commodity(36, "ProgenitorCells", "Progenitor Cells", "Medicines", 6779, false) },
            {128049176, new Commodity(37, "Aluminium", "Aluminium", "Metals", 340, false) },
            {128049168, new Commodity(38, "Beryllium", "Beryllium", "Metals", 8288, false) },
            {128049162, new Commodity(39, "Cobalt", "Cobalt", "Metals", 647, false) },
            {128049175, new Commodity(40, "Copper", "Copper", "Metals", 481, false) },
            {128049170, new Commodity(41, "Gallium", "Gallium", "Metals", 5135, false) },
            {128049154, new Commodity(42, "Gold", "Gold", "Metals", 9401, false) },
            {128049169, new Commodity(43, "Indium", "Indium", "Metals", 5727, false) },
            {128049173, new Commodity(44, "Lithium", "Lithium", "Metals", 1596, false) },
            {128049153, new Commodity(45, "Palladium", "Palladium", "Metals", 13298, false) },
            {128049152, new Commodity(46, "Platinum", "Platinum", "Metals", 19279, false) },
            {128049155, new Commodity(47, "Silver", "Silver", "Metals", 4775, false) },
            {128049171, new Commodity(48, "Tantalum", "Tantalum", "Metals", 3962, false) },
            {128049174, new Commodity(49, "Titanium", "Titanium", "Metals", 1006, false) },
            {128049172, new Commodity(50, "Uranium", "Uranium", "Metals", 2705, false) },
            {128049165, new Commodity(51, "Bauxite", "Bauxite", "Minerals", 120, false) },
            {128049156, new Commodity(52, "Bertrandite", "Bertrandite", "Minerals", 2374, false) },
            {128049159, new Commodity(53, "Coltan", "Coltan", "Minerals", 1319, false) },
            {128049158, new Commodity(54, "Gallite", "Gallite", "Minerals", 1819, false) },
            {128049157, new Commodity(55, "Indite", "Indite", "Minerals", 2088, false) },
            {128049161, new Commodity(56, "Lepidolite", "Lepidolite", "Minerals", 544, false) },
            {128049163, new Commodity(57, "Rutile", "Rutile", "Minerals", 299, false) },
            {128049160, new Commodity(58, "Uraninite", "Uraninite", "Minerals", 836, false) },
            {128667728, new Commodity(59, "ImperialSlaves", "Imperial Slaves", "Slaves", 15984, false) },
            {128049243, new Commodity(60, "Slaves", "Slaves", "Slaves", 10584, false) },
            {128049231, new Commodity(61, "AdvancedCatalysers", "Advanced Catalysers", "Technology", 2947, false) },
            {128049229, new Commodity(62, "AnimalMonitors", "Animal Monitors", "Technology", 324, false) },
            {128049230, new Commodity(63, "AquaponicSystems", "Aquaponic Systems", "Technology", 314, false) },
            {128049228, new Commodity(64, "AutoFabricators", "Auto-Fabricators", "Technology", 3734, false) },
            {128049672, new Commodity(65, "BioReducingLichen", "Bioreducing Lichen", "Technology", 998, false) },
            {128049225, new Commodity(66, "ComputerComponents", "Computer Components", "Technology", 513, false) },
            {128049226, new Commodity(67, "HazardousEnvironmentSuits", "H.E. Suits", "Technology", 340, false) },
            {128049232, new Commodity(68, "TerrainEnrichmentSystems", "Land Enrichment Systems", "Technology", 4887, false) },
            {128049671, new Commodity(69, "ResonatingSeparators", "Resonating Separators", "Technology", 5958, false) },
            {128049227, new Commodity(70, "Robotics", "Robotics", "Technology", 1856, false) },
            {128049190, new Commodity(72, "Leather", "Leather", "Textiles", 205, false) },
            {128049191, new Commodity(73, "NaturalFabrics", "Natural Fabrics", "Textiles", 439, false) },
            {128049193, new Commodity(74, "SyntheticFabrics", "Synthetic Fabrics", "Textiles", 211, false) },
            {128049244, new Commodity(75, "Biowaste", "Biowaste", "Waste", 63, false) },
            {128049246, new Commodity(76, "ChemicalWaste", "Chemical Waste", "Waste", 131, false) },
            {128049248, new Commodity(77, "Scrap", "Scrap", "Waste", 48, false) },
            {128049236, new Commodity(78, "NonLethalWeapons", "Non-Lethal Weapons", "Weapons", 1837, false) },
            {128049233, new Commodity(79, "PersonalWeapons", "Personal Weapons", "Weapons", 4632, false) },
            {128049235, new Commodity(80, "ReactiveArmour", "Reactive Armour", "Weapons", 2113, false) },
            {128049234, new Commodity(81, "BattleWeapons", "Battle Weapons", "Weapons", 7259, false) },
            {128049245, new Commodity(82, "ToxicWaste", "Toxic Waste", "Waste", 287, false) },
            {128668550, new Commodity(83, "Painite", "Painite", "Minerals", 40508, false) },
            {128066403, new Commodity(84, "Drones", "Limpet", "NonMarketable", 101, false) },
            {128666746, new Commodity(85, "EraninPearlWhisky", "Eranin Pearl Whisky", "Narcotics", 9040, true) },
            {128667071, new Commodity(86, "KamorinHistoricWeapons", "Kamorin Historic Weapons", "Weapons", 9766, true) },
            {128667760, new Commodity(87, "LucanOnionHead", "Lucan Onion Head", "Narcotics", 8472, true) },
            {128667676, new Commodity(88, "MotronaExperienceJelly", "Motrona Experience Jelly", "Narcotics", 13129, true) },
            {128667029, new Commodity(89, "OnionHead", "Onion Head", "Narcotics", 8437, true) },
            {128667082, new Commodity(90, "RusaniOldSmokey", "Rusani Old Smokey", "Narcotics", 11994, true) },
            {128667030, new Commodity(91, "TarachSpice", "Tarach Spice", "Narcotics", 8642, true) },
            {128667069, new Commodity(92, "TerraMaterBloodBores", "Terra Mater Blood Bores", "Medicines", 13414, true) },
            {128667031, new Commodity(93, "WolfFesh", "Wolf Fesh", "Narcotics", 8399, true) },
            {128667035, new Commodity(94, "WuthieloKuFroth", "Wuthielo Ku Froth", "Narcotics", 8194, true) },
            {128668548, new Commodity(95, "AiRelics", "Ai Relics", "Salvage", 138613, false) },
            {128668551, new Commodity(96, "Antiquities", "Antiquities", "Salvage", 115511, false) },
            {128671118, new Commodity(97, "Osmium", "Osmium", "Metals", 7591, false) },
            {128671443, new Commodity(98, "SAP8CoreContainer", "Sap 8 Core Container", "Salvage", 59196, false) },
            {128671444, new Commodity(99, "TrinketsOfFortune", "Trinkets Of Hidden Fortune", "Salvage", 1428, false) },
            {128666754, new Commodity(100, "USSCargoTradeData", "Trade Data", "Salvage", 2790, false) },
            {128672308, new Commodity(101, "ThermalCoolingUnits", "Thermal Cooling Units", "Machinery", 256, false) },
            {128672313, new Commodity(102, "SkimerComponents", "Skimmer Components", "Machinery", 859, false) },
            {128672307, new Commodity(103, "GeologicalEquipment", "Geological Equipment", "Machinery", 1661, false) },
            {128672311, new Commodity(104, "StructuralRegulators", "Structural Regulators", "Technology", 1791, false) },
            {128672297, new Commodity(105, "Pyrophyllite", "Pyrophyllite", "Minerals", 1565, false) },
            {128672296, new Commodity(106, "Moissanite", "Moissanite", "Minerals", 8273, false) },
            {128672295, new Commodity(107, "Goslarite", "Goslarite", "Minerals", 916, false) },
            {128672294, new Commodity(108, "Cryolite", "Cryolite", "Minerals", 2266, false) },
            {128672301, new Commodity(109, "Thorium", "Thorium", "Metals", 11513, false) },
            {128672299, new Commodity(110, "Thallium", "Thallium", "Metals", 3618, false) },
            {128672298, new Commodity(111, "Lanthanum", "Lanthanum", "Metals", 8766, false) },
            {128672300, new Commodity(112, "Bismuth", "Bismuth", "Metals", 2284, false) },
            {128672306, new Commodity(113, "BootlegLiquor", "Bootleg Liquor", "Narcotics", 855, false) },
            {128672701, new Commodity(114, "MetaAlloys", "Meta-Alloys", "Industrial Materials", 88148, false) },
            {128672302, new Commodity(115, "CeramicComposites", "Ceramic Composites", "Industrial Materials", 232, false) },
            {128672314, new Commodity(116, "EvacuationShelter", "Evacuation Shelter", "Consumer Items", 343, false) },
            {128672303, new Commodity(117, "SyntheticReagents", "Synthetic Reagents", "Chemicals", 6675, false) },
            {128672305, new Commodity(118, "SurfaceStabilisers", "Surface Stabilisers", "Chemicals", 467, false) },
            {128672309, new Commodity(119, "BuildingFabricators", "Building Fabricators", "Machinery", 980, false) },
            {128672312, new Commodity(121, "Landmines", "Landmines", "Weapons", 4602, false) },
            {128672304, new Commodity(122, "NerveAgents", "Nerve Agents", "Chemicals", 13526, false) },
            {128672310, new Commodity(124, "MuTomImager", "Muon Imager", "Technology", 6353, false) },
            {128666747, new Commodity(125, "LavianBrandy", "Lavian Brandy", "Narcotics", 10365, true) },
            {128666752, new Commodity(126, "USSCargoBlackBox", "Black Box", "Salvage", 6995, false) },
            {128666755, new Commodity(127, "USSCargoMilitaryPlans", "Military Plans", "Salvage", 9413, false) },
            {128666756, new Commodity(128, "USSCargoAncientArtefact", "Ancient Artefact", "Salvage", 8183, false) },
            {128666757, new Commodity(129, "USSCargoRareArtwork", "Rare Artwork", "Salvage", 7774, false) },
            {128666758, new Commodity(130, "USSCargoExperimentalChemicals", "Experimental Chemicals", "Salvage", 3524, false) },
            {128666759, new Commodity(131, "USSCargoRebelTransmissions", "Rebel Transmissions", "Salvage", 4068, false) },
            {128666760, new Commodity(132, "USSCargoPrototypeTech", "Prototype Tech", "Salvage", 10696, false) },
            {128666761, new Commodity(133, "USSCargoTechnicalBlueprints", "Technical Blueprints", "Salvage", 6333, false) },
            {128667019, new Commodity(134, "HIP10175BushMeat", "HIP 10175 Bush Meat", "Foods", 9382, true) },
            {128667020, new Commodity(135, "AlbinoQuechuaMammoth", "Albino Quechua Mammoth", "Foods", 9687, true) },
            {128667021, new Commodity(136, "UtgaroarMillennialEggs", "Utgaroar Millennial Eggs", "Foods", 9163, true) },
            {128667022, new Commodity(137, "WitchhaulKobeBeef", "Witchhaul Kobe Beef", "Foods", 11085, true) },
            {128667023, new Commodity(138, "KarsukiLocusts", "Karsuki Locusts", "Foods", 8543, true) },
            {128667024, new Commodity(139, "GiantIrukamaSnails", "Giant Irukama Snails", "Foods", 9174, true) },
            {128667025, new Commodity(140, "BaltahSineVacuumKrill",  "Baltah Sine Vacuum Krill", "Foods", 8479, true) },
            {128667026, new Commodity(141, "CetiRabbits", "Ceti Rabbits", "Foods", 9079, true) },
            {128667027, new Commodity(142, "KachiriginFilterLeeches", "Kachirigin Filter Leeches", "Medicines", 8227, true) },
            {128667028, new Commodity(143, "LyraeWeed", "Lyrae Weed", "Narcotics", 8937, true) },
            {128667032, new Commodity(144, "BorasetaniPathogenetics", "Borasetani Pathogenetics", "Weapons", 13679, true) },
            {128667033, new Commodity(145, "HIP118311Swarm", "HIP 118311 Swarm", "Weapons", 13448, true) },
            {128667034, new Commodity(146, "KonggaAle", "Kongga Ale", "Narcotics", 8310, true) },
            {128667036, new Commodity(147, "AlacarakmoSkinArt", "Alacarakmo Skin Art", "Consumer Items", 8899, true) },
            {128667037, new Commodity(148, "EleuThermals", "Eleu Thermals", "Consumer Items", 8507, true) },
            {128667038, new Commodity(149, "EshuUmbrellas", "Eshu Umbrellas", "Consumer Items", 9343, true) },
            {128667039, new Commodity(150, "KaretiiCouture", "Karetii Couture", "Consumer Items", 11582, true) },
            {128667040, new Commodity(151, "NjangariSaddles", "Njangari Saddles", "Consumer Items", 8356, true) },
            {128667041, new Commodity(152, "AnyNaCoffee", "Any Na Coffee", "Foods", 9160, true) },
            {128667042, new Commodity(153, "CD-75KittenBrandCoffee", "CD-75 Kitten Brand Coffee", "Foods", 9571, true) },
            {128667043, new Commodity(154, "GomanYauponCoffee", "Goman Yaupon Coffee", "Foods", 8921, true) },
            {128667044, new Commodity(155, "VolkhabBeeDrones", "Volkhab Bee Drones", "Machinery", 10198, true) },
            {128667045, new Commodity(156, "KinagoViolins", "Kinago Violins", "Consumer Items", 13030, true) },
            {128667046, new Commodity(157, "NgunaModernAntiques", "Nguna Modern Antiques", "Consumer Items", 8545, true) },
            {128667047, new Commodity(158, "RajukruMulti-Stoves", "Rajukru Multi-Stoves", "Consumer Items", 8378, true) },
            {128667048, new Commodity(159, "TiolceWaste2PasteUnits", "Tiolce Waste2Paste Units", "Consumer Items", 8710, true) },
            {128667049, new Commodity(160, "ChiEridaniMarinePaste", "Chi Eridani Marine Paste", "Foods", 8450, true) },
            {128667050, new Commodity(161, "EsusekuCaviar", "Esuseku Caviar", "Foods", 9625, true) },
            {128667051, new Commodity(162, "LiveHecateSeaWorms", "Live Hecate Sea Worms", "Foods", 8737, true) },
            {128667052, new Commodity(163, "HelvetitjPearls", "Helvetitj Pearls", "Foods", 10450, true) },
            {128667053, new Commodity(164, "HIPProto-Squid", "HIP Proto-Squid", "Foods", 8497, true) },
            {128667054, new Commodity(165, "CoquimSpongiformVictuals", "Coquim Spongiform Victuals", "Foods", 8077, true) },
            {128667055, new Commodity(166, "EdenApplesOfAerial", "Eden Apples Of Aerial", "Foods", 8331, true) },
            {128667056, new Commodity(167, "NeritusBerries", "Neritus Berries", "Foods", 8497, true) },
            {128667057, new Commodity(168, "OchoengChillies", "Ochoeng Chillies", "Foods", 8601, true) },
            {128667058, new Commodity(169, "DeuringasTruffles", "Deuringas Truffles", "Foods", 9232, true) },
            {128667059, new Commodity(170, "HR7221Wheat", "HR 7221 Wheat", "Foods", 8190, true) },
            {128667060, new Commodity(171, "JarouaRice", "Jaroua Rice", "Foods", 8169, true) },
            {128667061, new Commodity(172, "BelalansRayLeather", "Belalans Ray Leather", "Textiles", 8519, true) },
            {128667062, new Commodity(173, "DamnaCarapaces", "Damna Carapaces", "Textiles", 8120, true) },
            {128667063, new Commodity(174, "RapaBaoSnakeSkins", "Rapa Bao Snake Skins", "Textiles", 8285, true) },
            {128667064, new Commodity(175, "VanayequiCeratomorphaFur", "Vanayequi Ceratomorpha Fur", "Textiles", 8331, true) },
            {128667065, new Commodity(176, "BastSnakeGin", "Bast Snake Gin", "Narcotics", 8659, true) },
            {128667066, new Commodity(177, "ThrutisCream", "Thrutis Cream", "Narcotics", 8550, true) },
            {128667067, new Commodity(178, "WulpaHyperboreSystems", "Wulpa Hyperbore Systems", "Machinery", 8726, true) },
            {128667068, new Commodity(179, "AganippeRush", "Aganippe Rush", "Medicines", 14220, true) },
            {128667070, new Commodity(180, "HolvaDuellingBlades", "Holva Duelling Blades", "Weapons", 12493, true) },
            {128667072, new Commodity(181, "GilyaSignatureWeapons", "Gilya Signature Weapons", "Weapons", 13038, true) },
            {128667073, new Commodity(182, "DeltaPhoenicisPalms", "Delta Phoenicis Palms", "Chemicals", 8188, true) },
            {128667074, new Commodity(183, "ToxandjiVirocide", "Toxandji Virocide", "Chemicals", 8275, true) },
            {128667075, new Commodity(184, "xihecompanions", "Xihe Biomorphic Companions", "Technology", 11058, true) },
            {128667076, new Commodity(185, "SanumaDecorativeMeat", "Sanuma Decorative Meat", "Foods", 8504, true) },
            {128667077, new Commodity(186, "EthgrezeTeaBuds", "Ethgreze Tea Buds", "Foods", 10197, true) },
            {128667078, new Commodity(187, "CeremonialHeikeTea", "Ceremonial Heike Tea", "Foods", 9251, true) },
            {128667079, new Commodity(188, "TanmarkTranquilTea", "Tanmark Tranquil Tea", "Foods", 9177, true) },
            {128667080, new Commodity(189, "AzCancriFormula42", "Az Cancri Formula 42", "Technology", 12440, true) },
            {128667081, new Commodity(190, "KamitraCigars", "Kamitra Cigars", "Narcotics", 12282, true) },
            {128667083, new Commodity(191, "YasoKondiLeaf", "Yaso Kondi Leaf", "Narcotics", 12171, true) },
            {128667084, new Commodity(192, "ChateauDeAegaeon", "Chateau De Aegaeon", "Narcotics", 8791, true) },
            {128667085, new Commodity(193, "WatersOfShintara", "Waters Of Shintara", "Medicines", 13711, true) },
            {128667668, new Commodity(194, "OphiuchExinoArtefacts", "Ophiuch Exino Artefacts", "Consumer Items", 10969, true) },
            {128667670, new Commodity(195, "AepyornisEgg", "Aepyornis Egg", "Foods", 9769, true) },
            {128667671, new Commodity(196, "SaxonWine",  "Saxon Wine", "Narcotics", 8983, true) },
            {128667672, new Commodity(197, "CentauriMegaGin", "Centauri Mega Gin", "Narcotics", 10217, true) },
            {128667673, new Commodity(198, "AnduligaFireWorks", "Anduliga Fire Works", "Chemicals", 8519, true) },
            {128667674, new Commodity(199, "BankiAmphibiousLeather", "Banki Amphibious Leather", "Textiles", 8338, true) },
            {128667675, new Commodity(200, "CherbonesBloodCrystals", "Cherbones Blood Crystals", "Minerals", 16714, true) },
            {128667677, new Commodity(201, "GeawenDanceDust", "Geawen Dance Dust", "Narcotics", 8618, true) },
            {128667678, new Commodity(202, "GerasianGueuzeBeer", "Gerasian Gueuze Beer", "Narcotics", 8215, true) },
            {128667679, new Commodity(203, "HaidneBlackBrew", "Haidne Black Brew", "Foods", 8837, true) },
            {128667680, new Commodity(204, "HavasupaiDreamCatcher", "Havasupai Dream Catcher", "Consumer Items", 14639, true) },
            {128667681, new Commodity(205, "BurnhamBileDistillate", "Burnham Bile Distillate", "Narcotics", 8466, true) },
            {128667682, new Commodity(206, "HIPOrganophosphates", "HIP Organophosphates", "Chemicals", 8169, true) },
            {128667683, new Commodity(207, "JaradharrePuzzleBox", "Jaradharre Puzzle Box", "Consumer Items", 16816, true) },
            {128667684, new Commodity(208, "KoroKungPellets", "Koro Kung Pellets", "Chemicals", 8067, true) },
            {128667685, new Commodity(209, "VoidExtractCoffee", "Void Extract Coffee", "Foods", 9554, true) },
            {128667686, new Commodity(210, "HonestyPills", "Honesty Pills", "Medicines", 8860, true) },
            {128667687, new Commodity(211, "NonEuclidianExotanks", "Non Euclidian Exotanks", "Machinery", 8526, true) },
            {128667688, new Commodity(212, "LTTHypersweet", "LTT Hypersweet", "Foods", 8054, true) },
            {128667689, new Commodity(213, "MechucosHighTea", "Mechucos High Tea", "Foods", 8846, true) },
            {128667690, new Commodity(214, "MedbStarlube", "Medb Starlube", "Industrial Materials", 8191, true) },
            {128667691, new Commodity(215, "MokojingBeastFeast", "Mokojing Beast Feast", "Foods", 9788, true) },
            {128667692, new Commodity(216, "MukusubiiChitin-Os", "Mukusubii Chitin-Os", "Foods", 8359, true) },
            {128667693, new Commodity(217, "MulachiGiantFungus", "Mulachi Giant Fungus", "Foods", 7957, true) },
            {128667694, new Commodity(218, "NgadandariFireOpals", "Ngadandari Fire Opals", "Minerals", 19112, true) },
            {128667695, new Commodity(219, "TiegfriesSynthSilk", "Tiegfries Synth Silk", "Textiles", 8478, true) },
            {128667696, new Commodity(220, "UzumokuLow-GWings", "Uzumoku Low-G Wings", "Consumer Items", 13845, true) },
            {128667697, new Commodity(221, "VHerculisBodyRub", "V Herculis Body Rub", "Medicines", 8010, true) },
            {128667698, new Commodity(222, "WheemeteWheatCakes", "Wheemete Wheat Cakes", "Foods", 8081, true) },
            {128667699, new Commodity(223, "VegaSlimweed", "Vega Slimweed", "Medicines", 9588, true) },
            {128667700, new Commodity(224, "AltairianSkin", "Altairian Skin", "Consumer Items", 8432, true) },
            {128667701, new Commodity(225, "PavonisEarGrubs", "Pavonis Ear Grubs", "Narcotics", 8364, true) },
            {128667702, new Commodity(226, "JotunMookah", "Jotun Mookah", "Consumer Items", 8780, true) },
            {128667703, new Commodity(227, "GiantVerrix", "Giant Verrix", "Machinery", 12496, true) },
            {128667704, new Commodity(228, "IndiBourbon", "Indi Bourbon", "Narcotics", 8806, true) },
            {128667705, new Commodity(229, "AroucaConventualSweets", "Arouca Conventual Sweets", "Foods", 8737, true) },
            {128667706, new Commodity(230, "TauriChimes", "Tauri Chimes", "Medicines", 8549, true) },
            {128667707, new Commodity(231, "ZeesszeAntGrubGlue", "Zeessze Ant Grub Glue", "Consumer Items", 8161, true) },
            {128667708, new Commodity(232, "PantaaPrayerSticks", "Pantaa Prayer Sticks", "Medicines", 9177, true) },
            {128667709, new Commodity(233, "FujinTea", "Fujin Tea", "Medicines", 8597, true) },
            {128667710, new Commodity(234, "ChameleonCloth", "Chameleon Cloth", "Textiles", 9071, true) },
            {128667711, new Commodity(235, "OrrerianViciousBrew", "Orrerian Vicious Brew", "Foods", 8342, true) },
            {128667712, new Commodity(236, "UszaianTreeGrub", "Uszaian Tree Grub", "Foods", 8578, true) },
            {128667713, new Commodity(237, "MomusBogSpaniel", "Momus Bog Spaniel", "Consumer Items", 9184, true) },
            {128667714, new Commodity(238, "DisoMaCorn", "Diso Ma Corn", "Foods", 8134, true) },
            {128667715, new Commodity(239, "LeestianEvilJuice", "Leestian Evil Juice", "Narcotics", 8220, true) },
            {128667716, new Commodity(240, "bluemilk", "Azure Milk", "Narcotics", 10805, true) },
            {128667717, new Commodity(241, "alieneggs", "Leathery Eggs", "Consumer Items", 25067, true) },
            {128667718, new Commodity(242, "AlyaBodySoap", "Alya Body Soap", "Medicines", 8218, true) },
            {128667719, new Commodity(243, "VidavantianLace", "Vidavantian Lace", "Consumer Items", 12615, true) },
            {128668017, new Commodity(244, "JaquesQuinentianStill", "Jaques Quinentian Still", "Consumer Items", 2108, true) },
            {128668018, new Commodity(245, "SoontillRelics", "Soontill Relics", "Consumer Items", 19885, true) },
            {128668547, new Commodity(246, "UnknownArtifact", "Thargoid Sensor", "Salvage", 290190, false) },
            {128668549, new Commodity(247, "Hafnium178", "Hafnium 178", "Metals", 69098, false) },
            {128668552, new Commodity(248, "MilitaryIntelligence", "Military Intelligence", "Salvage", 55527, false) },
            {128672121, new Commodity(249, "TheHuttonMug", "The Hutton Mug", "Consumer Items", 7986, true) },
            {128672122, new Commodity(250, "SothisCrystallineGold", "Sothis Crystalline Gold", "metals", 19112, true) },
            {128672123, new Commodity(251, "WreckageComponents", "Salvageable Wreckage", "Salvage", 394, false) },
            {128672124, new Commodity(252, "EncriptedDataStorage", "Encrypted Data Storage", "Salvage", 806, false) },
            {128672126, new Commodity(253, "PersonalEffects", "Personal Effects", "Salvage", 379, false) },
            {128672127, new Commodity(254, "ComercialSamples", "Commercial Samples", "Salvage", 361, false) },
            {128672128, new Commodity(255, "TacticalData", "Tactical Data", "Salvage", 457, false) },
            {128672129, new Commodity(256, "AssaultPlans", "Assault Plans", "Salvage", 446, false) },
            {128672130, new Commodity(257, "EncryptedCorrespondence", "Encrypted Correspondence", "Salvage", 372, false) },
            {128672131, new Commodity(258, "DiplomaticBag", "Diplomatic Bag", "Salvage", 572, false) },
            {128672132, new Commodity(259, "ScientificResearch", "Scientific Research", "Salvage", 635, false) },
            {128672133, new Commodity(260, "ScientificSamples", "Scientific Samples", "Salvage", 772, false) },
            {128672134, new Commodity(261, "PoliticalPrisoner", "Political Prisoner", "Salvage", 5132, false) },
            {128672135, new Commodity(262, "Hostage", "Hostage", "Salvage", 2427, false) },
            {128672315, new Commodity(263, "GeologicalSamples", "Geological Samples", "Salvage", 446, false) },
            {128672316, new Commodity(264, "MasterChefs", "Master Chefs","Slaves", 20590, true) },
            {128672432, new Commodity(265, "CrystallineSpheres", "Crystalline Spheres", "Consumer Items", 12216, true) },
            {128672775, new Commodity(266, "Taaffeite", "Taaffeite", "Minerals", 20696, false) },
            {128672776, new Commodity(267, "Jadeite", "Jadeite", "Minerals", 13474, false) },
            {128672810, new Commodity(268, "UnstableDataCore", "Unstable Data Core", "Salvage", 2427, false) },
            {128672812, new Commodity(269, "OnionheadAlphaStrain", "Onionhead Alpha Strain", "Narcotics", 8437, true) },
            {128672125, new Commodity(270, "OccupiedCryoPod", "Occupied Escape Pod", "Salvage", 4474, false) },
            {128049166, new Commodity(271, "Water", "Water", "Chemicals", 120, false) },
            {128673069, new Commodity(272, "OnionheadBetaStrain", "Onionhead Beta Strain", "Narcotics", 8437, true) },
            {128673845, new Commodity(273, "Praseodymium", "Praseodymium", "Metals", 7156, false) },
            {128673846, new Commodity(274, "Bromellite", "Bromellite", "Minerals", 7062, false) },
            {128673847, new Commodity(275, "Samarium", "Samarium", "Metals", 6330, false) },
            {128673848, new Commodity(276, "LowTemperatureDiamond", "Low Temperature Diamonds", "Minerals", 57445, false) },
            {128673850, new Commodity(277, "HydrogenPeroxide", "Hydrogen Peroxide", "Chemicals", 917, false) },
            {128673851, new Commodity(278, "LiquidOxygen", "Liquid oxygen", "Chemicals", 263, false) },
            {128673852, new Commodity(279, "MethanolMonohydrateCrystals", "Methanol Monohydrate Crystals", "Minerals", 2282, false) },
            {128673853, new Commodity(280, "LithiumHydroxide", "Lithium Hydroxide", "Minerals", 5646, false) },
            {128673854, new Commodity(281, "MethaneClathrate", "Methane Clathrate", "Minerals", 629, false) },
            {128673855, new Commodity(282, "InsulatingMembrane", "Insulating Membrane", "Industrial Materials", 7837, false) },
            {128673856, new Commodity(283, "CMMComposite", "CMM Composite", "Industrial Materials", 3132, false) },
            {128673857, new Commodity(284, "CoolingHoses", "Micro-weave Cooling Hoses", "Industrial Materials", 403, false) },
            {128673858, new Commodity(285, "NeofabricInsulation", "Neofabric Insulation", "Industrial Materials", 2769, false) },
            {128673859, new Commodity(286, "ArticulationMotors", "Articulation Motors", "Machinery", 4997, false) },
            {128673860, new Commodity(287, "HNShockMount", "HN Shock Mount", "Machinery", 406, false) },
            {128673861, new Commodity(288, "EmergencyPowerCells", "Emergency Power Cells", "Machinery", 1011, false) },
            {128673862, new Commodity(289, "PowerConverter", "Power Converter", "Machinery", 246, false) },
            {128673863, new Commodity(290, "PowerGridAssembly", "Energy Grid Assembly", "Machinery", 1684, false) },
            {128673864, new Commodity(291, "PowerTransferConduits", "Power Transfer Conduits", "Machinery", 857, false) },
            {128673865, new Commodity(292, "RadiationBaffle", "Radiation Baffle", "Machinery", 383, false) },
            {128673866, new Commodity(293, "ExhaustManifold", "Exhaust Manifold", "Machinery", 479, false) },
            {128673867, new Commodity(294, "ReinforcedMountingPlate", "Reinforced Mounting Plate", "Machinery", 1074, false) },
            {128673868, new Commodity(295, "HeatsinkInterlink", "Heatsink Interlink", "Machinery", 729, false) },
            {128673869, new Commodity(296, "MagneticEmitterCoil", "Magnetic Emitter Coil", "Machinery", 199, false) },
            {128673870, new Commodity(297, "ModularTerminals", "Modular Terminals", "Machinery", 695, false) },
            {128673871, new Commodity(298, "Nanobreakers", "Nanobreakers", "Technology", 639, false) },
            {128673872, new Commodity(299, "TelemetrySuite", "Telemetry Suite", "Technology", 2080, false) },
            {128673873, new Commodity(300, "MicroControllers", "Micro Controllers", "Technology", 3274, false) },
            {128673874, new Commodity(301, "IonDistributor", "Ion Distributor", "Technology", 1133, false) },
            {128673875, new Commodity(302, "DiagnosticSensor", "Hardware Diagnostic Sensor", "Technology", 4337, false) },
            {128682044, new Commodity(303, "ConductiveFabrics", "Conductive Fabrics", "Textiles", 507, false) },
            {128682045, new Commodity(304, "MilitaryGradeFabrics", "Military Grade Fabrics", "Textiles", 708, false) },
            {128682046, new Commodity(305, "AdvancedMedicines", "Advanced Medicines", "Medicines", 1259, false) },
            {128682047, new Commodity(306, "MedicalDiagnosticEquipment", "Medical Diagnostic Equipment", "Technology", 2848, false) },
            {128682048, new Commodity(307, "SurvivalEquipment", "Survival Equipment", "Consumer Items", 485, false) },
            {128682049, new Commodity(308, "DataCore", "Data Core", "Salvage", 2872, false) },
            {128682050, new Commodity(309, "GalacticTravelGuide", "Galactic Travel Guide", "Salvage", 332, false) },
            {128682051, new Commodity(310, "MysteriousIdol", "Mysterious Idol", "Salvage", 15196, false) },
            {128682052, new Commodity(311, "ProhibitedResearchMaterials", "Prohibited Research Materials", "Salvage", 46607, false) },
            {128682053, new Commodity(312, "AntimatterContainmentUnit", "Antimatter Containment Unit", "Salvage", 26608, false) },
            {128682054, new Commodity(313, "SpacePioneerRelics", "Space Pioneer Relics", "Salvage", 7342, false) },
            {128682055, new Commodity(314, "FossilRemnants", "Fossil Remnants", "Salvage", 9927, false) },
            {128673876, new Commodity(315, "UnknownArtifact2", "Thargoid Probe", "Salvage", 411003, false) },
            {128672160, new Commodity(316, "PreciousGems", "Precious Gems", "Salvage", 109641, false) },
            {128740752, new Commodity(317, "UnknownArtifact3", "Thargoid Link", "Salvage", 31350, false) },
            {128737288, new Commodity(318, "UnknownBiologicalMatter", "Thargoid Biological Matter", "Salvage", 25479, false) },
            {128737287, new Commodity(319, "UnknownResin", "Thargoid Resin", "Salvage", 18652, false) },
            {128737289, new Commodity(320, "UnknownTechnologySamples", "Thargoid Technology Samples", "Salvage", 22551, false) },
            {128732183, new Commodity(321, "AncientRelic", "Ancient Relic", "Salvage", 0, false) },
            {128732184, new Commodity(322, "AncientOrb", "Ancient Orb", "Salvage", 0, false) },
            {128732185, new Commodity(323, "AncientCasket", "Ancient Casket", "Salvage", 0, false) },
            {128732186, new Commodity(324, "AncientTablet", "Ancient Tablet", "Salvage", 0, false) },
            {128732187, new Commodity(325, "AncientUrn", "Ancient Urn", "Salvage", 0, false) },
            {128732188, new Commodity(326, "AncientTotem", "Ancient Totem", "Salvage", 0, false) },
            {128672137, new Commodity(327, "SmallExplorationDataCash", "Small Survey Data Cache", "Salvage", 0, false) },
            {128672136, new Commodity(328, "LargeExplorationDataCash", "Large Survey Data Cache", "Salvage", 0, false) },
            {128672811, new Commodity(329, "DamagedEscapePod", "Damaged Escape Pod", "Salvage", 11912, false) },
            {128672161, new Commodity(330, "EarthRelics", "Earth Relics", "Salvage", 0, false) },

            // Items for which we do not have EDDB IDs
            {200000012, new Commodity(10012, "siriuscommercialcontracts", "Sirius Commerical Contracts", "Powerplay", 0, false) },
            {200000013, new Commodity(10013, "siriusindustrialequipment", "Sirius Inustrial Equipment", "Powerplay", 0, false) },
            {200000014, new Commodity(10014, "siriusfranchisepackage", "Sirius Franchise Package", "Powerplay", 0, false) },
            {200000015, new Commodity(10015, "republicangarisonsupplies", "Republic Garrison Supplies", "Powerplay", 0, false) },
            {128672159, new Commodity(10016, "AntiqueJewellery", "Antique Jewellery", "Salvage", 0, false) },
            {128672162, new Commodity(10018, "GeneBank", "Gene Bank", "Salvage", 0, false) },
            {128672163, new Commodity(10019, "TimeCapsule", "Time Capsule", "Salvage", 0, false) },
            {200000016, new Commodity(10020, "ThargoidTissueSampleType1", "Thargoid Tissue Sample", "Salvage", 14081, false) },
        };

        // Builds dictionaries for Name & EDName, converting to lower case and removing all spaces, dashes, and dots from the names
        private static Dictionary<string, Commodity> CommoditiesByName = CommoditiesByEliteID.ToDictionary(kp => kp.Value.name.ToLowerInvariant().Replace(" ", "").Replace(".", "").Replace("-", ""), kp => kp.Value);
        private static Dictionary<string, Commodity> CommoditiesByEDName = CommoditiesByEliteID.ToDictionary(kp => kp.Value.EDName.ToLowerInvariant().Replace(" ", "").Replace(".", "").Replace("-", ""), kp => kp.Value);

        public static Commodity CommodityFromEliteID(long id)
        {
            Commodity Commodity = new Commodity();

            Commodity Template;
            if (CommoditiesByEliteID.TryGetValue(id, out Template))
            {
                Commodity.EDDBID = Template.EDDBID;
                Commodity.EDName = Template.EDName;
                Commodity.name = Template.name;
                Commodity.category = Template.category;
                Commodity.rare = Template.rare;
                Commodity.avgprice = Template.avgprice;
            }
            return Commodity;
        }

        public static Commodity FromName(string name)
        {
            if (name == null)
            {
                return null;
            }

            Commodity Commodity = new Commodity();

            string cleanedName = name.ToLowerInvariant()
                .Replace("$", "") // Header for types from mining and mission events
                .Replace("_name;", "") // Trailer for types from mining and mission events
                ;

            // Map from ED name to sensible name
            string edName;
            nameMapping.TryGetValue(cleanedName, out edName);

            // Now try to fetch the commodity by either ED or real name
            Commodity Template = null;
            bool found = false;

            if (edName != null)
            {
                found = CommoditiesByEDName.TryGetValue(edName, out Template);
                if (!found)
                {
                    found = CommoditiesByName.TryGetValue(edName, out Template);
                }
            }
            if (!found)
            {
                found = CommoditiesByEDName.TryGetValue(cleanedName, out Template);
            }
            if (!found)
            {
                found = CommoditiesByName.TryGetValue(cleanedName, out Template);
            }
            if (found)
            {
                Commodity.EDDBID = Template?.EDDBID ?? Commodity.EDDBID;
                Commodity.EDName = Template?.EDName ?? Commodity.EDName;
                Commodity.name = Template?.name ?? Commodity.name;
                Commodity.category = Template?.category ?? Commodity.category;
                Commodity.avgprice = Template?.avgprice ?? Commodity.avgprice;
                Commodity.rare = Template?.rare ?? Commodity.rare;
            }
            else
            {
                Logging.Report("Unknown commodity", @"{""name"":""" + name + @"""}");

                // Put some basic information in place
                Commodity.EDName = name;
                Commodity.name = Regex.Replace(name.Replace("$", "").Replace("_Name;", ""), "([a-z])([A-Z])", "$1 $2");
            }
            return Commodity;
        }
    }
}


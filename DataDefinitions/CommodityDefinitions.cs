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
            {"heliostaticfurnaces", "microbialfurnaces"},
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
        {
            {128049204, new Commodity(1, "Explosives", "Chemicals", 261, false) },
            {128049202, new Commodity(2, "Hydrogen Fuel", "Chemicals", 110, false) },
            {128049203, new Commodity(3, "Mineral Oil", "Chemicals", 181, false) },
            {128049205, new Commodity(4, "Pesticides", "Chemicals", 241, false) },
            {128049241, new Commodity(5, "Clothing", "Consumer Items", 285, false) },
            {128049240, new Commodity(6, "Consumer Technology", "Consumer Items", 6769, false) },
            {128049238, new Commodity(7, "Domestic Appliances", "Consumer Items", 487, false) },
            {128049214, new Commodity(8, "Beer", "Legal Drugs", 186, false) },
            {128049216, new Commodity(9, "Liquor", "Legal Drugs", 587, false) },
            {128049212, new Commodity(10, "Basic Narcotics", "Narcotics", "Legal Drugs", 9966, false) },
            {128049213, new Commodity(11, "Tobacco", "Legal Drugs", 5035, false) },
            {128049215, new Commodity(12, "Wine", "Legal Drugs", 260, false) },
            {128049177, new Commodity(13, "Algae", "Foods", 137, false) },
            {128049182, new Commodity(14, "Animalmeat", "Animal Meat", "Foods", 1292, false) },
            {128049189, new Commodity(15, "Coffee", "Foods", 1279, false) },
            {128049183, new Commodity(16, "Fish", "Foods", 406, false) },
            {128049184, new Commodity(17, "Food Cartridges", "Foods", 105, false) },
            {128049178, new Commodity(18, "Fruit and Vegetables", "Foods", 312, false) },
            {128049180, new Commodity(19, "Grain", "Foods", 210, false) },
            {128049185, new Commodity(20, "Synthetic Meat", "Foods", 271, false) },
            {128049188, new Commodity(21, "Tea", "Foods", 1467, false) },
            {128049197, new Commodity(22, "Polymers", "Industrial Materials", 171, false) },
            {128049199, new Commodity(23, "Semiconductors", "Industrial Materials", 967, false) },
            {128049200, new Commodity(24, "Superconductors", "Industrial Materials", 6609, false) },
            {128064028, new Commodity(25, "Atmospheric Extractors", "Atmospheric Processors", "Machinery", 357, false) },
            {128049222, new Commodity(26, "Crop Harvesters", "Machinery", 2021, false) },
            {128049223, new Commodity(27, "Marine Supplies", "Marine Equipment", "Machinery", 3916, false) },
            {128049220, new Commodity(28, "Heliostatic Furnaces", "Microbial Furnaces", "Machinery", 236, false) },
            {128049221, new Commodity(29, "Mineral Extractors", "Machinery", 443, false) },
            {128049217, new Commodity(30, "Power Generators", "Machinery", 458, false) },
            {128049218, new Commodity(31, "Water Purifiers", "Machinery", 258, false) },
            {128049208, new Commodity(32, "Agricultural Medicines", "Agri-Medicines", "Medicines", 1038, false) },
            {128049210, new Commodity(33, "Basic Medicines", "Medicines", 279, false) },
            {128049670, new Commodity(34, "Combat Stabilisers", "Medicines", 3505, false) },
            {128049209, new Commodity(35, "Performance Enhancers", "Medicines", 6816, false) },
            {128049669, new Commodity(36, "Progenitor Cells", "Medicines", 6779, false) },
            {128049176, new Commodity(37, "Aluminium", "Metals", 340, false) },
            {128049168, new Commodity(38, "Beryllium", "Metals", 8288, false) },
            {128049162, new Commodity(39, "Cobalt", "Metals", 647, false) },
            {128049175, new Commodity(40, "Copper", "Metals", 481, false) },
            {128049170, new Commodity(41, "Gallium", "Metals", 5135, false) },
            {128049154, new Commodity(42, "Gold", "Metals", 9401, false) },
            {128049169, new Commodity(43, "Indium", "Metals", 5727, false) },
            {128049173, new Commodity(44, "Lithium", "Metals", 1596, false) },
            {128049153, new Commodity(45, "Palladium", "Metals", 13298, false) },
            {128049152, new Commodity(46, "Platinum", "Metals", 19279, false) },
            {128049155, new Commodity(47, "Silver", "Metals", 4775, false) },
            {128049171, new Commodity(48, "Tantalum", "Metals", 3962, false) },
            {128049174, new Commodity(49, "Titanium", "Metals", 1006, false) },
            {128049172, new Commodity(50, "Uranium", "Metals", 2705, false) },
            {128049165, new Commodity(51, "Bauxite", "Minerals", 120, false) },
            {128049156, new Commodity(52, "Bertrandite", "Minerals", 2374, false) },
            {128049159, new Commodity(53, "Coltan", "Minerals", 1319, false) },
            {128049158, new Commodity(54, "Gallite", "Minerals", 1819, false) },
            {128049157, new Commodity(55, "Indite", "Minerals", 2088, false) },
            {128049161, new Commodity(56, "Lepidolite", "Minerals", 544, false) },
            {128049163, new Commodity(57, "Rutile", "Minerals", 299, false) },
            {128049160, new Commodity(58, "Uraninite", "Minerals", 836, false) },
            {128667728, new Commodity(59, "Imperial Slaves", "Slavery", 15984, false) },
            {128049243, new Commodity(60, "Slaves", "Slavery", 10584, false) },
            {128049231, new Commodity(61, "Advanced Catalysers", "Technology", 2947, false) },
            {128049229, new Commodity(62, "Animal Monitors", "Technology", 324, false) },
            {128049230, new Commodity(63, "Aquaponic Systems", "Technology", 314, false) },
            {128049228, new Commodity(64, "Auto Fabricators", "Auto-Fabricators", "Technology", 3734, false) },
            {128049672, new Commodity(65, "Bio Reducing Lichen", "Bioreducing Lichen", "Technology", 998, false) },
            {128049225, new Commodity(66, "Computer Components", "Technology", 513, false) },
            {128049226, new Commodity(67, "Hazardous Environment Suits", "H.E. Suits", "Technology", 340, false) },
            {128049232, new Commodity(68, "Terrain Enrichment Systems", "Land Enrichment Systems", "Technology", 4887, false) },
            {128049671, new Commodity(69, "Resonating Separators", "Technology", 5958, false) },
            {128049227, new Commodity(70, "Robotics", "Technology", 1856, false) },
            {128049190, new Commodity(72, "Leather", "Textiles", 205, false) },
            {128049191, new Commodity(73, "Natural Fabrics", "Textiles", 439, false) },
            {128049193, new Commodity(74, "Synthetic Fabrics", "Textiles", 211, false) },
            {128049244, new Commodity(75, "Biowaste", "Waste", 63, false) },
            {128049246, new Commodity(76, "Chemical Waste", "Waste", 131, false) },
            {128049248, new Commodity(77, "Scrap", "Waste", 48, false) },
            {128049236, new Commodity(78, "Non Lethal Weapons", "Non-lethal Weapons", "Weapons", 1837, false) },
            {128049233, new Commodity(79, "Personal Weapons", "Weapons", 4632, false) },
            {128049235, new Commodity(80, "Reactive Armour", "Weapons", 2113, false) },
            {128049234, new Commodity(81, "Battle Weapons", "Weapons", 7259, false) },
            {200000009, new Commodity(82, "Toxic Waste", "Waste", 287, false) },
            {128668550, new Commodity(83, "Painite", "Minerals", 40508, false) },
            {128066403, new Commodity(84, "drones", "Limpet", "NonMarketable", 101, false) },
            {300000001, new Commodity(85, "Eranin Pearl Whiskey", "Legal Drugs", 9040, true) },
            {300000002, new Commodity(86, "Kamorin Historic Weapons", "Weapons", 9766, true) },
            {300000003, new Commodity(87, "Lucan Onion Head", "Legal Drugs", 8472, true) },
            {300000004, new Commodity(88, "Motrona Experience Jelly", "Legal Drugs", 13129, true) },
            {300000005, new Commodity(89, "Onion Head", "Legal Drugs", 8437, true) },
            {300000006, new Commodity(90, "Rusani Old Smokey", "Legal Drugs", 11994, true) },
            {300000007, new Commodity(91, "Tarach Spice", "Legal Drugs", 8642, true) },
            {300000008, new Commodity(92, "Terra Mater Blood Bores", "Medicines", 13414, true) },
            {300000009, new Commodity(93, "Wolf Fesh", "Legal Drugs", 8399, true) },
            {300000010, new Commodity(94, "Wuthielo Ku Froth", "Legal Drugs", 8194, true) },
            {128668548, new Commodity(95, "Ai Relics", "Ai Relics", "Salvage", 138613, false) },
            {128668551, new Commodity(96, "Antiquities", "Salvage", 115511, false) },
            {128671118, new Commodity(97, "Osmium", "Metals", 7591, false) },
            {128671443, new Commodity(98, "S A P8 Core Container", "Sap 8 Core Container", "Salvage", 59196, false) },
            {128671444, new Commodity(99, "Trinkets Of Fortune", "Trinkets Of Hidden Fortune", "Salvage", 1428, false) },
            {128666754, new Commodity(100, "U S S Cargo Trade Data", "Trade Data", "Salvage", 2790, false) },
            {128672308, new Commodity(101, "Thermal Cooling Units", "Machinery", 256, false) },
            {128672313, new Commodity(102, "Skimer Components", "Skimmer Components", "Machinery", 859, false) },
            {128672307, new Commodity(103, "Geological Equipment", "Machinery", 1661, false) },
            {128672311, new Commodity(104, "Structural Regulators", "Technology", 1791, false) },
            {128672297, new Commodity(105, "Pyrophyllite", "Minerals", 1565, false) },
            {128672296, new Commodity(106, "Moissanite", "Minerals", 8273, false) },
            {128672295, new Commodity(107, "Goslarite", "Minerals", 916, false) },
            {128672294, new Commodity(108, "Cryolite", "Minerals", 2266, false) },
            {128672301, new Commodity(109, "Thorium", "Metals", 11513, false) },
            {128672299, new Commodity(110, "Thallium", "Metals", 3618, false) },
            {128672298, new Commodity(111, "Lanthanum", "Metals", 8766, false) },
            {128672300, new Commodity(112, "Bismuth", "Metals", 2284, false) },
            {128672306, new Commodity(113, "Bootleg Liquor", "Legal Drugs", 855, false) },
            {128672701, new Commodity(114, "Meta Alloys", "Meta-Alloys", "Industrial Materials", 88148, false) },
            {128672302, new Commodity(115, "Ceramic Composites", "Industrial Materials", 232, false) },
            {128672314, new Commodity(116, "Evacuation Shelter", "Consumer Items", 343, false) },
            {128672303, new Commodity(117, "Synthetic Reagents", "Chemicals", 6675, false) },
            {128672305, new Commodity(118, "Surface Stabilisers", "Chemicals", 467, false) },
            {128672309, new Commodity(119, "Building Fabricators", "Machinery", 980, false) },
            {128672312, new Commodity(121, "Landmines", "Weapons", 4602, false) },
            {128672304, new Commodity(122, "Nerve Agents", "Chemicals", 13526, false) },
            {200000008, new Commodity(123, "Occupied CryoPod", "Salvage", 5132, false) },
            {128672310, new Commodity(124, "mutomimager", "Muon Imager", "Technology", 6353, false) },
            {310000001, new Commodity(125, "Lavian Brandy", "Legal Drugs", 10365, true) },
            {310000002, new Commodity(126, "usscargoblackbox", "Black Box", "Salvage", 6995, false) },
            {128666755, new Commodity(127, "usscargomilitaryplans", "Military Plans", "Salvage", 9413, false) },
            {128666756, new Commodity(128, "usscargoancientartefact", "Ancient Artefact", "Salvage", 8183, false) },
            {300000012, new Commodity(129, "usscargorareartwork", "Rare Artwork", "Salvage", 7774, false) },
            {128666758, new Commodity(130, "usscargoexperimentalchemicals", "Experimental Chemicals", "Salvage", 3524, false) },
            {128666759, new Commodity(131, "usscargorebeltransmissions", "Rebel Transmissions", "Salvage", 4068, false) },
            {128666760, new Commodity(132, "usscargoprototypetech", "Prototype Tech", "Salvage", 10696, false) },
            {128666761, new Commodity(133, "usscargotechnicalblueprints", "Technical Blueprints", "Salvage", 6333, false) },
            {300000013, new Commodity(134, "HIP 10175 Bush Meat", "Foods", 9382, true) },
            {300000014, new Commodity(135, "Albino Quechua Mammoth", "Foods", 9687, true) },
            {300000015, new Commodity(136, "Utgaroar Millennial Eggs", "Foods", 9163, true) },
            {300000016, new Commodity(137, "Witchhaul Kobe Beef", "Foods", 11085, true) },
            {300000017, new Commodity(138, "Karsuki Locusts", "Foods", 8543, true) },
            {300000018, new Commodity(139, "Giant Irukama Snails", "Foods", 9174, true) },
            {300000019, new Commodity(140, "Baltah Sine Vacuum Krill", "Foods", 8479, true) },
            {300000020, new Commodity(141, "Ceti Rabbits", "Foods", 9079, true) },
            {300000021, new Commodity(142, "Kachirigin Filter Leeches", "Medicines", 8227, true) },
            {300000022, new Commodity(143, "Lyrae Weed", "Legal Drugs", 8937, true) },
            {300000023, new Commodity(144, "Borasetani Pathogenetics", "Weapons", 13679, true) },
            {300000024, new Commodity(145, "HIP 118311 Swarm", "Weapons", 13448, true) },
            {300000025, new Commodity(146, "Kongga Ale", "Legal Drugs", 8310, true) },
            {300000026, new Commodity(147, "Alacarakmo Skin Art", "Consumer Items", 8899, true) },
            {300000027, new Commodity(148, "Eleu Thermals", "Consumer Items", 8507, true) },
            {300000028, new Commodity(149, "Eshu Umbrellas", "Consumer Items", 9343, true) },
            {300000029, new Commodity(150, "Karetii Couture", "Consumer Items", 11582, true) },
            {300000030, new Commodity(151, "Njangari Saddles", "Consumer Items", 8356, true) },
            {300000031, new Commodity(152, "Any Na Coffee", "Foods", 9160, true) },
            {300000032, new Commodity(153, "CD-75 Kitten Brand Coffee", "Foods", 9571, true) },
            {300000033, new Commodity(154, "Goman Yaupon Coffee", "Foods", 8921, true) },
            {300000034, new Commodity(155, "Volkhab Bee Drones", "Machinery", 10198, true) },
            {300000035, new Commodity(156, "Kinago Violins", "Consumer Items", 13030, true) },
            {300000036, new Commodity(157, "Nguna Modern Antiques", "Consumer Items", 8545, true) },
            {300000037, new Commodity(158, "Rajukru Multi-Stoves", "Consumer Items", 8378, true) },
            {300000038, new Commodity(159, "Tiolce Waste2Paste Units", "Consumer Items", 8710, true) },
            {300000039, new Commodity(160, "Chi Eridani Marine Paste", "Foods", 8450, true) },
            {300000040, new Commodity(161, "Esuseku Caviar", "Foods", 9625, true) },
            {300000041, new Commodity(162, "Live Hecate Sea Worms", "Foods", 8737, true) },
            {300000042, new Commodity(163, "Helvetitj Pearls", "Foods", 10450, true) },
            {300000043, new Commodity(164, "HIP Proto-Squid", "Foods", 8497, true) },
            {300000044, new Commodity(165, "Coquim Spongiform Victuals", "Foods", 8077, true) },
            {300000045, new Commodity(166, "Eden Apples Of Aerial", "Foods", 8331, true) },
            {300000046, new Commodity(167, "Neritus Berries", "Foods", 8497, true) },
            {300000047, new Commodity(168, "Ochoeng Chillies", "Foods", 8601, true) },
            {300000048, new Commodity(169, "Deuringas Truffles", "Foods", 9232, true) },
            {300000049, new Commodity(170, "HR 7221 Wheat", "Foods", 8190, true) },
            {300000050, new Commodity(171, "Jaroua Rice", "Foods", 8169, true) },
            {300000051, new Commodity(172, "Belalans Ray Leather", "Textiles", 8519, true) },
            {300000052, new Commodity(173, "Damna Carapaces", "Textiles", 8120, true) },
            {300000053, new Commodity(174, "Rapa Bao Snake Skins", "Textiles", 8285, true) },
            {300000054, new Commodity(175, "Vanayequi Ceratomorpha Fur", "Textiles", 8331, true) },
            {300000055, new Commodity(176, "Bast Snake Gin", "Legal Drugs", 8659, true) },
            {300000056, new Commodity(177, "Thrutis Cream", "Legal Drugs", 8550, true) },
            {300000057, new Commodity(178, "Wulpa Hyperbore Systems", "Machinery", 8726, true) },
            {300000058, new Commodity(179, "Aganippe Rush", "Medicines", 14220, true) },
            {300000059, new Commodity(180, "Holva Duelling Blades", "Weapons", 12493, true) },
            {300000060, new Commodity(181, "Gilya Signature Weapons", "Weapons", 13038, true) },
            {300000061, new Commodity(182, "Delta Phoenicis Palms", "Chemicals", 8188, true) },
            {300000062, new Commodity(183, "Toxandji Virocide", "Chemicals", 8275, true) },
            {300000063, new Commodity(184, "xihecompanions", "Xihe Biomorphic Companions", "Technology", 11058, true) },
            {300000064, new Commodity(185, "Sanuma Decorative Meat", "Foods", 8504, true) },
            {300000065, new Commodity(186, "Ethgreze Tea Buds", "Foods", 10197, true) },
            {300000066, new Commodity(187, "Ceremonial Heike Tea", "Foods", 9251, true) },
            {300000067, new Commodity(188, "Tanmark Tranquil Tea", "Foods", 9177, true) },
            {300000068, new Commodity(189, "Az Cancri Formula 42", "Technology", 12440, true) },
            {300000069, new Commodity(190, "Kamitra Cigars", "Legal Drugs", 12282, true) },
            {300000070, new Commodity(191, "Yaso Kondi Leaf", "Legal Drugs", 12171, true) },
            {300000071, new Commodity(192, "Chateau De Aegaeon", "Legal Drugs", 8791, true) },
            {300000072, new Commodity(193, "Waters Of Shintara", "Medicines", 13711, true) },
            {300000073, new Commodity(194, "Ophiuch Exino Artefacts", "Consumer Items", 10969, true) },
            {300000074, new Commodity(195, "Aepyornis Egg", "Foods", 9769, true) },
            {300000075, new Commodity(196, "Saxon Wine", "Legal Drugs", 8983, true) },
            {300000076, new Commodity(197, "Centauri Mega Gin", "Legal Drugs", 10217, true) },
            {300000077, new Commodity(198, "Anduliga Fire Works", "Chemicals", 8519, true) },
            {300000078, new Commodity(199, "Banki Amphibious Leather", "Textiles", 8338, true) },
            {300000079, new Commodity(200, "Cherbones Blood Crystals", "Minerals", 16714, true) },
            {300000080, new Commodity(201, "Geawen Dance Dust", "Legal Drugs", 8618, true) },
            {300000081, new Commodity(202, "Gerasian Gueuze Beer", "Legal Drugs", 8215, true) },
            {300000082, new Commodity(203, "Haidne Black Brew", "Foods", 8837, true) },
            {300000083, new Commodity(204, "Havasupai Dream Catcher", "Consumer Items", 14639, true) },
            {300000084, new Commodity(205, "Burnham Bile Distillate", "Legal Drugs", 8466, true) },
            {300000085, new Commodity(206, "HIP Organophosphates", "Chemicals", 8169, true) },
            {300000086, new Commodity(207, "Jaradharre Puzzle Box", "Consumer Items", 16816, true) },
            {300000087, new Commodity(208, "Koro Kung Pellets", "Chemicals", 8067, true) },
            {300000088, new Commodity(209, "Void Extract Coffee", "Foods", 9554, true) },
            {300000089, new Commodity(210, "Honesty Pills", "Medicines", 8860, true) },
            {300000090, new Commodity(211, "Non Euclidian Exotanks", "Machinery", 8526, true) },
            {300000091, new Commodity(212, "LTT Hypersweet", "Foods", 8054, true) },
            {300000092, new Commodity(213, "Mechucos High Tea", "Foods", 8846, true) },
            {300000093, new Commodity(214, "Medb Starlube", "Industrial Materials", 8191, true) },
            {300000094, new Commodity(215, "Mokojing Beast Feast", "Foods", 9788, true) },
            {300000095, new Commodity(216, "Mukusubii Chitin-Os", "Foods", 8359, true) },
            {300000096, new Commodity(217, "Mulachi Giant Fungus", "Foods", 7957, true) },
            {300000097, new Commodity(218, "Ngadandari Fire Opals", "Minerals", 19112, true) },
            {300000098, new Commodity(219, "Tiegfries Synth Silk", "Textiles", 8478, true) },
            {300000099, new Commodity(220, "Uzumoku Low-G Wings", "Consumer Items", 13845, true) },
            {300000100, new Commodity(221, "V Herculis Body Rub", "Medicines", 8010, true) },
            {300000101, new Commodity(222, "Wheemete Wheat Cakes", "Foods", 8081, true) },
            {300000102, new Commodity(223, "Vega Slimweed", "Medicines", 9588, true) },
            {300000103, new Commodity(224, "Altairian Skin", "Consumer Items", 8432, true) },
            {300000104, new Commodity(225, "Pavonis Ear Grubs", "Legal Drugs", 8364, true) },
            {300000105, new Commodity(226, "Jotun Mookah", "Consumer Items", 8780, true) },
            {300000106, new Commodity(227, "Giant Verrix", "Machinery", 12496, true) },
            {300000107, new Commodity(228, "Indi Bourbon", "Legal Drugs", 8806, true) },
            {300000108, new Commodity(229, "Arouca Conventual Sweets", "Foods", 8737, true) },
            {300000109, new Commodity(230, "Tauri Chimes", "Medicines", 8549, true) },
            {300000110, new Commodity(231, "Zeessze Ant Grub Glue", "Consumer Items", 8161, true) },
            {300000111, new Commodity(232, "Pantaa Prayer Sticks", "Medicines", 9177, true) },
            {300000112, new Commodity(233, "Fujin Tea", "Medicines", 8597, true) },
            {300000113, new Commodity(234, "Chameleon Cloth", "Textiles", 9071, true) },
            {300000114, new Commodity(235, "Orrerian Vicious Brew", "Foods", 8342, true) },
            {300000115, new Commodity(236, "Uszaian Tree Grub", "Foods", 8578, true) },
            {300000116, new Commodity(237, "Momus Bog Spaniel", "Consumer Items", 9184, true) },
            {300000117, new Commodity(238, "Diso Ma Corn", "Foods", 8134, true) },
            {300000118, new Commodity(239, "Leestian Evil Juice", "Legal Drugs", 8220, true) },
            {300000119, new Commodity(240, "bluemilk", "Azure Milk", "Legal Drugs", 10805, true) },
            {300000120, new Commodity(241, "alieneggs", "Leathery Eggs", "Consumer Items", 25067, true) },
            {300000121, new Commodity(242, "Alya Body Soap", "Medicines", 8218, true) },
            {300000122, new Commodity(243, "Vidavantian Lace", "Consumer Items", 12615, true) },
            {300000123, new Commodity(244, "Jaques Quinentian Still", "consumer Items", 2108, true) },
            {300000124, new Commodity(245, "Soontill Relics", "Consumer Items", 19885, true) },
            {128668547, new Commodity(246, "unknownartifact", "Thargoid Sensor", "Salvage", 290190, false) },
            {128668549, new Commodity(247, "Hafnium178", "Hafnium 178", "Metals", 69098, false) },
            {128668552, new Commodity(248, "Military Intelligence", "Salvage", 55527, false) },
            {300000128, new Commodity(249, "The Hutton Mug", "Consumer Items", 7986, true) },
            {300000129, new Commodity(250, "Sothis Crystalline Gold", "metals", 19112, true) },
            {300000130, new Commodity(251, "wreckagecomponents", "Salvageable Wreckage", "Salvage", 394, false) },
            {300000131, new Commodity(252, "encripteddatastorage", "Encrypted Data Storage", "Salvage", 806, false) },
            {300000132, new Commodity(253, "Personal Effects", "Salvage", 379, false) },
            {300000133, new Commodity(254, "Commercial Samples", "Salvage", 361, false) },
            {300000134, new Commodity(255, "Tactical Data", "Salvage", 457, false) },
            {300000135, new Commodity(256, "Assault Plans", "Salvage", 446, false) },
            {300000136, new Commodity(257, "Encrypted Correspondence", "Salvage", 372, false) },
            {300000137, new Commodity(258, "Diplomatic Bag", "Salvage", 572, false) },
            {300000138, new Commodity(259, "Scientific Research", "Salvage", 635, false) },
            {300000139, new Commodity(260, "Scientific Samples", "Salvage", 772, false) },
            {300000140, new Commodity(261, "Political Prisoner", "Salvage", 5132, false) },
            {300000141, new Commodity(262, "Hostage", "Salvage", 2427, false) },
            {300000142, new Commodity(263, "Geological Samples", "Salvage", 446, false) },
            {300000143, new Commodity(264, "Master Chefs", "Slavery", 20590, true) },
            {300000144, new Commodity(265, "Crystalline Spheres", "Consumer Items", 12216, true) },
            {300000145, new Commodity(266, "Taaffeite", "Minerals", 20696, false) },
            {300000146, new Commodity(267, "Jadeite", "Minerals", 13474, false) },
            {300000147, new Commodity(268, "Unstable Data Core", "Salvage", 2427, false) },
            {300000148, new Commodity(269, "Onionhead Alpha Strain", "Legal Drugs", 8437, true) },
            {300000149, new Commodity(270, "damagedescapepod", "Occupied Escape Pod", "Salvage", 4474, false) },
            {128049166, new Commodity(271, "Water", "Chemicals", 120, false) },
            {300000150, new Commodity(272, "Onionhead Beta Strain", "Legal Drugs", 8437, true) },
            {128673845, new Commodity(273, "Praseodymium", "Metals", 7156, false) },
            {128673846, new Commodity(274, "Bromellite", "Minerals", 7062, false) },
            {128673847, new Commodity(275, "Samarium", "Metals", 6330, false) },
            {128673848, new Commodity(276, "Low Temperature Diamond", "Minerals", 57445, false) },
            {128673850, new Commodity(277, "Hydrogen Peroxide", "Chemicals", 917, false) },
            {128673851, new Commodity(278, "Liquid Oxygen", "Chemicals", 263, false) },
            {128673852, new Commodity(279, "methanolmonohydratecrystals", "Methanol Monohydrate", "Minerals", 2282, false) },
            {128673853, new Commodity(280, "Lithium Hydroxide", "Minerals", 5646, false) },
            {128673854, new Commodity(281, "Methane Clathrate", "Minerals", 629, false) },
            {128673855, new Commodity(282, "Insulating Membrane", "Industrial Materials", 7837, false) },
            {128673856, new Commodity(283, "C M M Composite", "CMM Composite", "Industrial Materials", 3132, false) },
            {128673857, new Commodity(284, "Cooling Hoses", "Micro-Weave Cooling Hoses", "Industrial Materials", 403, false) },
            {128673858, new Commodity(285, "Neofabric Insulation", "Industrial Materials", 2769, false) },
            {128673859, new Commodity(286, "Articulation Motors", "Machinery", 4997, false) },
            {128673860, new Commodity(287, "H N Shock Mount", "HN Shock Mount", "Machinery", 406, false) },
            {128673861, new Commodity(288, "Emergency Power Cells", "Machinery", 1011, false) },
            {128673862, new Commodity(289, "Power Converter", "Machinery", 246, false) },
            {128673863, new Commodity(290, "powergridassembly", "Energy Grid Assembly", "Machinery", 1684, false) },
            {128673864, new Commodity(291, "Power Transfer Conduits", "Machinery", 857, false) },
            {128673865, new Commodity(292, "Radiation Baffle", "Machinery", 383, false) },
            {128673866, new Commodity(293, "Exhaust Manifold", "Machinery", 479, false) },
            {128673867, new Commodity(294, "Reinforced Mounting Plate", "Machinery", 1074, false) },
            {128673868, new Commodity(295, "Heatsink Interlink", "Machinery", 729, false) },
            {128673869, new Commodity(296, "Magnetic Emitter Coil", "Machinery", 199, false) },
            {128673870, new Commodity(297, "Modular Terminals", "Machinery", 695, false) },
            {128673871, new Commodity(298, "Nanobreakers", "Technology", 639, false) },
            {128673872, new Commodity(299, "Telemetry Suite", "Technology", 2080, false) },
            {128673873, new Commodity(300, "Micro Controllers", "Technology", 3274, false) },
            {128673874, new Commodity(301, "Ion Distributor", "Technology", 1133, false) },
            {128673875, new Commodity(302, "Hardware Diagnostic Sensor", "Technology", 4337, false) },
            {128682044, new Commodity(303, "Conductive Fabrics", "Textiles", 507, false) },
            {128682045, new Commodity(304, "Military Grade Fabrics", "Textiles", 708, false) },
            {128682046, new Commodity(305, "Advanced Medicines", "Medicines", 1259, false) },
            {128682047, new Commodity(306, "Medical Diagnostic Equipment", "Technology", 2848, false) },
            {128682048, new Commodity(307, "Survival Equipment", "Consumer Items", 485, false) },
            {300000151, new Commodity(308, "Data Core", "Salvage", 2872, false) },
            {300000152, new Commodity(309, "Galactic Travel Guide", "Salvage", 332, false) },
            {300000153, new Commodity(310, "Mysterious Idol", "Salvage", 15196, false) },
            {300000154, new Commodity(311, "Prohibited Research Materials", "Salvage", 46607, false) },
            {300000155, new Commodity(312, "Antimatter Containment Unit", "Salvage", 26608, false) },
            {300000156, new Commodity(313, "Space Pioneer Relics", "Salvage", 7342, false) },
            {300000157, new Commodity(314, "Fossil Remnants", "Salvage", 9927, false) },
            {300000158, new Commodity(315, "Thargoid Probe", "Salvage", 411003, false) },
            {300000159, new Commodity(316, "Precious Gems", "Salvage", 109641, false) },
            {300000160, new Commodity(317, "Thargoid Link", "Salvage", 31350, false) },
            {300000161, new Commodity(318, "Thargoid Biological Matter", "Salvage", 25479, false) },
            {300000162, new Commodity(319, "Thargoid Resin", "Salvage", 18652, false) },
            {300000163, new Commodity(320, "Thargoid Technology Samples", "Salvage", 22551, false) },
            // Items for which we do not have EDDB IDs
            {200000001, new Commodity(10001, "Ancient Orb", "Ancient Artifacts", 0, false) },
            {200000002, new Commodity(10002, "Ancient Urn", "Ancient Artifacts", 0, false) },
            {200000003, new Commodity(10003, "Ancient Tablet", "Ancient Artifacts", 0, false) },
            {200000004, new Commodity(10004, "Ancient Casket", "Ancient Artifacts", 0, false) },
            {200000005, new Commodity(10005, "Ancient Relic", "Ancient Artifacts", 0, false) },
            {200000006, new Commodity(10006, "Ancient Totem", "Ancient Artifacts", 0, false) },
            {200000007, new Commodity(10007, "smallexplorationdatacash", "Small Exploration Data Cache", "Salvage", 0, false) },
            {200000010, new Commodity(10010, "largeexplorationdatacash", "Large Exploration Data Cache", "Salvage", 0, false) },
            {200000011, new Commodity(10011, "unknownartifact2", "Unknown Artefact (2)", "Salvage", 0, false) },
            {200000012, new Commodity(10012, "siriuscommercialcontracts", "Sirius Commerical Contracts", "Powerplay", 0, false) },
            {200000013, new Commodity(10013, "siriusindustrialequipment", "Sirius Inustrial Equipment", "Powerplay", 0, false) },
            {200000014, new Commodity(10014, "siriusfranchisepackage", "Sirius Franchise Package", "Powerplay", 0, false) },
            {200000015, new Commodity(10015, "republicangarisonsupplies", "Repiblic Garrison Supplies", "Powerplay", 0, false) },
        };

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
                Commodity.rare = Template?.rare ?? Commodity.rare;
                Commodity.avgprice = Template?.avgprice ?? Commodity.avgprice;
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


using System.Collections.Generic;
using System.Linq;

namespace EliteDangerousDataDefinitions
{
    public class CommodityDefinitions
    {
        // Names that are totally different between cargo and commodity
        private static Dictionary<string, string> cargoNamesMapping = new Dictionary<string, string>
        {
            {"drones", "limpet" },
        };

        // Mapping from Elite internal names to common names
        private static Dictionary<string, string> nameMapping = new Dictionary<string, string>
        {
            {"Agricultural Medicines", "Agri-Medicines"},
            { "Ai Relics", "AI Relics"},
            { "Atmospheric Extractors", "Atmospheric Processors"},
            {"Auto Fabricators", "Auto-Fabricators"},
            { "Basic Narcotics", "Narcotics"},
            {"Bio Reducing Lichen", "Bioreducing Lichen"},
            { "C M M Composite", "CMM Composite"},
            { "Drones", "Limpet"},
            { "H N Shock Mount", "HN Shock Mount"},
            {"Hafnium178", "Hafnium 178"},
            {"Hazardous Environment Suits", "H.E. Suits"},
            {"Heliostatic Furnaces", "Microbial Furnaces"},
            {"Marine Supplies", "Marine Equipment"},
            {"Meta Alloys", "Meta-Alloys"},
            {"Mu Tom Imager", "Muon Imager"},
            {"Non Lethal Weapons", "Non-Lethal Weapons"},
            {"S A P8 Core Container", "SAP 8 Core Container"},
            {"Skimer Components", "Skimmer Components"},
            {"Terrain Enrichment Systems", "Land Enrichment Systems"},
            {"Trinkets Of Fortune", "Trinkets Of Hidden Fortune"},
            {"Unknown Artifact", "Unknown Artefact"},
            {"U S S Cargo Ancient Artefact", "Ancient Artefact"},
            {"U S S Cargo Experimental Chemicals", "Experimental Chemicals"},
            {"U S S Cargo Military Plans", "Military Plans"},
            {"U S S Cargo Prototype Tech", "Prototype Tech"},
            {"U S S Cargo Rebel Transmissions", "Rebel Transmissions"},
            {"U S S Cargo Technical Blueprints", "Technical Blueprints"},
            {"U S S Cargo Trade Data", "Trade Data"},

            { "Comercial Samples", "Commercial Samples"},
            { "Encripted Data Storage", "Encrypted Data Storage"},
            {"Wreckage Components", "Salvageable Wreckage"},
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
            {128049182, new Commodity(14, "Animal Meat", "Foods", 1292, false) },
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
            {128668550, new Commodity(83, "Painite", "Minerals", 40508, false) },
            {128066403, new Commodity(84, "Drones", "Limpet", "Unknown", 101, false) },
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
            {128672310, new Commodity(124, "Mu Tom Imager", "Muon Imager", "Technology", 6353, false) },
            {128666755, new Commodity(127, "U S S Cargo Military Plans", "Military Plans", "Salvage", 9413, false) },
            {128666756, new Commodity(128, "U S S Cargo Ancient Artefact", "Ancient Artefact", "Salvage", 8183, false) },
            {128666758, new Commodity(130, "U S S Cargo Experimental Chemicals", "Experimental Chemicals", "Salvage", 3524, false) },
            {128666759, new Commodity(131, "U S S Cargo Rebel Transmissions", "Rebel Transmissions", "Salvage", 4068, false) },
            {128666760, new Commodity(132, "U S S Cargo Prototype Tech", "Prototype Tech", "Salvage", 10696, false) },
            {128666761, new Commodity(133, "U S S Cargo Technical Blueprints", "Technical Blueprints", "Salvage", 6333, false) },
            {128668547, new Commodity(246, "Unknown Artifact", "Unknown Artefact", "Salvage", 290190, false) },
            {128668549, new Commodity(247, "Hafnium178", "Hafnium 178", "Metals", 69098, false) },
            {128668552, new Commodity(248, "Military Intelligence", "Salvage", 55527, false) },
            {128049166, new Commodity(271, "Water", "Chemicals", 120, false) },
            {128673845, new Commodity(273, "Praseodymium", "Metals", 7156, false) },
            {128673846, new Commodity(274, "Bromellite", "Minerals", 7062, false) },
            {128673847, new Commodity(275, "Samarium", "Metals", 6330, false) },
            {128673848, new Commodity(276, "Low Temperature Diamond", "Minerals", 57445, false) },
            {128673850, new Commodity(277, "Hydrogen Peroxide", "Chemicals", 917, false) },
            {128673851, new Commodity(278, "Liquid Oxygen", "Chemicals", 263, false) },
            {128673852, new Commodity(279, "Methanol Monohydrate", "Minerals", 2282, false) },
            {128673853, new Commodity(280, "Lithium Hydroxide", "Minerals", 5646, false) },
            {128673854, new Commodity(281, "Methane Clathrate", "Minerals", 629, false) },
            {128673855, new Commodity(282, "Insulating Membrane", "Industrial Materials", 7837, false) },
            {128673856, new Commodity(283, "C M M Composite", "CMM Composite", "Industrial Materials", 3132, false) },
            {128673859, new Commodity(286, "Articulation Motors", "Machinery", 4997, false) },
            {128673860, new Commodity(287, "H N Shock Mount", "HN Shock Mount", "Machinery", 406, false) },
            {128673861, new Commodity(288, "Emergency Power Cells", "Machinery", 1011, false) },
            {128673862, new Commodity(289, "Power Converter", "Machinery", 246, false) },
            {128673863, new Commodity(290, "Energy Grid Assembly", "Machinery", 1684, false) },
            {128673864, new Commodity(291, "Power Transfer Conduits", "Machinery", 857, false) },
            {128673865, new Commodity(292, "Radiation Baffle", "Machinery", 383, false) },
            {128673866, new Commodity(293, "Exhaust Manifold", "Machinery", 479, false) },
            {128673867, new Commodity(294, "Reinforced Mounting Plate", "Machinery", 1074, false) },
            {128673868, new Commodity(295, "Heatsink Interlink", "Machinery", 729, false) },
            {128673869, new Commodity(296, "Magnetic Emitter Coil", "Machinery", 199, false) },
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
        };
        private static Dictionary<string, Commodity> CommoditiesByCargoName = CommoditiesByEliteID.ToDictionary(kp => kp.Value.Name.ToLower().Replace(" ", ""), kp => kp.Value);

        public static Commodity CommodityFromEliteID(long id)
        {
            Commodity Commodity = new Commodity();

            Commodity Template;
            if (CommoditiesByEliteID.TryGetValue(id, out Template))
            {
                Commodity.EDDBID = Template.EDDBID;
                Commodity.EDName = Template.EDName;
                Commodity.Name = Template.Name;
                Commodity.Category = Template.Category;
                Commodity.Rare = Template.Rare;
                Commodity.AveragePrice = Template.AveragePrice;
            }
            return Commodity;
        }

        public static Commodity CommodityFromCargoName(string name)
        {
            if (name == null)
            {
                return null;
            }

            Commodity Commodity = new Commodity();

            // First try to map from cargo name to the commodity name
            string cargoName;
            cargoNamesMapping.TryGetValue(name, out cargoName);
            if (cargoName == null) { cargoName = name; }

            // Now try to fetch the commodity by name
            Commodity Template;
            if (CommoditiesByCargoName.TryGetValue(cargoName, out Template))
            {
                Commodity.EDDBID = Template.EDDBID;
                Commodity.EDName = Template.EDName;
                Commodity.Name = Template.Name;
                Commodity.Category = Template.Category;
                Commodity.Rare = Template.Rare;
                Commodity.AveragePrice = Template.AveragePrice;
            }
            return Commodity;
        }
    }
}


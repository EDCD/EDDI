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
            {"Ai Relics", "AI Relics"},
            {"Atmospheric Extractors", "Atmospheric Processors"},
            {"Auto Fabricators", "Auto-Fabricators"},
            {"Basic Narcotics", "Narcotics"},
            {"Bio Reducing Lichen", "Bioreducing Lichen"},
            {"Comercial Samples", "Commercial Samples"},
            {"Drones", "Limpet"},
            {"Encripted Data Storage", "Encrypted Data Storage"},
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
            {"Wreckage Components", "Salvageable Wreckage"},
        };

        private static Dictionary<long, Commodity> CommoditiesByEliteID = new Dictionary<long, Commodity>
        {
            {128049204, new Commodity(1, "Explosives", "Chemicals", 378, false) },
            {128049202, new Commodity(2, "Hydrogen Fuel", "Chemicals", 147, false) },
            {128049203, new Commodity(3, "Mineral Oil", "Chemicals", 259, false) },
            {128049205, new Commodity(4, "Pesticides", "Chemicals", 292, false) },
            {128049241, new Commodity(5, "Clothing", "Consumer Items", 395, false) },
            {128049240, new Commodity(6, "Consumer Technology", "Consumer Items", 7031, false) },
            {128049238, new Commodity(7, "Domestic Appliances", "Consumer Items", 631, false) },
            {128049214, new Commodity(8, "Beer", "Legal Drugs", 240, false) },
            {128049216, new Commodity(9, "Liquor", "Legal Drugs", 738, false) },
            {128049212, new Commodity(10, "Narcotics", "Legal Drugs", 9742, false) },
            {128049213, new Commodity(11, "Tobacco", "Legal Drugs", 5088, false) },
            {128049215, new Commodity(12, "Wine", "Legal Drugs", 324, false) },
            {128049177, new Commodity(13, "Algae", "Foods", 200, false) },
            {128049182, new Commodity(14, "Animal Meat", "Foods", 1460, false) },
            {128049189, new Commodity(15, "Coffee", "Foods", 1460, false) },
            {128049183, new Commodity(16, "Fish", "Foods", 493, false) },
            {128049184, new Commodity(17, "Food Cartridges", "Foods", 206, false) },
            {128049178, new Commodity(18, "Fruit and Vegetables", "Foods", 395, false) },
            {128049180, new Commodity(19, "Grain", "Foods", 275, false) },
            {128049185, new Commodity(20, "Synthetic Meat", "Foods", 324, false) },
            {128049188, new Commodity(21, "Tea", "Foods", 1646, false) },
            {128049197, new Commodity(22, "Polymers", "Industrial Materials", 216, false) },
            {128049199, new Commodity(23, "Semiconductors", "Industrial Materials", 1029, false) },
            {128049200, new Commodity(24, "Superconductors", "Industrial Materials", 7031, false) },
            {128064028, new Commodity(25, "Atmospheric Processors", "Machinery", 493, false) },
            {128049222, new Commodity(26, "Crop Harvesters", "Machinery", 2378, false) },
            {128049223, new Commodity(27, "Marine Equipment", "Machinery", 4474, false) },
            {128049220, new Commodity(28, "Microbial Furnaces", "Machinery", 266, false) },
            {128049221, new Commodity(29, "Mineral Extractors", "Machinery", 700, false) },
            {128049217, new Commodity(30, "Power Generators", "Machinery", 631, false) },
            {128049218, new Commodity(31, "Water Purifiers", "Machinery", 378, false) },
            {128049208, new Commodity(32, "Agri-Medicines", "Medicines", 1154, false) },
            {128049210, new Commodity(33, "Basic Medicines", "Medicines", 395, false) },
            {128049670, new Commodity(34, "Combat Stabilisers", "Medicines", 3055, false) },
            {128049209, new Commodity(35, "Performance Enhancers", "Medicines", 7031, false) },
            {128049669, new Commodity(36, "Progenitor Cells", "Medicines", 7031, false) },
            {128049176, new Commodity(37, "Aluminium", "Metals", 412, false) },
            {128049168, new Commodity(38, "Beryllium", "Metals", 8549, false) },
            {128049162, new Commodity(39, "Cobalt", "Metals", 823, false) },
            {128049175, new Commodity(40, "Copper", "Metals", 570, false) },
            {128049170, new Commodity(41, "Gallium", "Metals", 5426, false) },
            {128049154, new Commodity(42, "Gold", "Metals", 9742, false) },
            {128049169, new Commodity(43, "Indium", "Metals", 6175, false) },
            {128049173, new Commodity(44, "Lithium", "Metals", 1749, false) },
            {128049153, new Commodity(45, "Palladium", "Metals", 13527, false) },
            {128049152, new Commodity(46, "Platinum", "Metals", 18814, false) },
            {128049155, new Commodity(47, "Silver", "Metals", 5088, false) },
            {128049171, new Commodity(48, "Tantalum", "Metals", 4196, false) },
            {128049174, new Commodity(49, "Titanium", "Metals", 1154, false) },
            {128049172, new Commodity(50, "Uranium", "Metals", 2869, false) },
            {128049165, new Commodity(51, "Bauxite", "Minerals", 216, false) },
            {128049156, new Commodity(52, "Bertrandite", "Minerals", 2694, false) },
            {128049159, new Commodity(53, "Coltan", "Minerals", 1550, false) },
            {128049158, new Commodity(54, "Gallite", "Minerals", 2101, false) },
            {128049157, new Commodity(55, "Indite", "Minerals", 2378, false) },
            {128049161, new Commodity(56, "Lepidolite", "Minerals", 700, false) },
            {128049163, new Commodity(57, "Rutile", "Minerals", 412, false) },
            {128049160, new Commodity(58, "Uraninite", "Minerals", 1029, false) },
            {128667728, new Commodity(59, "Imperial Slaves", "Slavery", 16485, false) },
            {128049243, new Commodity(60, "Slaves", "Slavery", 11107, false) },
            {128049231, new Commodity(61, "Advanced Catalysers", "Technology", 3055, false) },
            {128049229, new Commodity(62, "Animal Monitors", "Technology", 378, false) },
            {128049230, new Commodity(63, "Aquaponic Systems", "Technology", 349, false) },
            {128049228, new Commodity(64, "Auto-Fabricators", "Technology", 3937, false) },
            {128049672, new Commodity(65, "Bioreducing Lichen", "Technology", 1089, false) },
            {128049225, new Commodity(66, "Computer Components", "Technology", 631, false) },
            {128049226, new Commodity(67, "H.E. Suits", "Technology", 349, false) },
            {128049232, new Commodity(68, "Land Enrichment Systems", "Technology", 5088, false) },
            {128049671, new Commodity(69, "Resonating Separators", "Technology", 6175, false) },
            {128049227, new Commodity(70, "Robotics", "Technology", 1976, false) },
            {128049190, new Commodity(72, "Leather", "Textiles", 240, false) },
            {128049191, new Commodity(73, "Natural Fabrics", "Textiles", 493, false) },
            {128049193, new Commodity(74, "Synthetic Fabrics", "Textiles", 252, false) },
            {128049244, new Commodity(75, "Biowaste", "Waste", 74, false) },
            {128049246, new Commodity(76, "Chemical Waste", "Waste", 79, false) },
            {128049248, new Commodity(77, "Scrap", "Waste", 96, false) },
            {128049236, new Commodity(78, "Non-lethal Weapons", "Weapons", 1976, false) },
            {128049233, new Commodity(79, "Personal Weapons", "Weapons", 4474, false) },
            {128049235, new Commodity(80, "Reactive Armour", "Weapons", 2235, false) },
            {128049234, new Commodity(81, "Battle Weapons", "Weapons", 7031, false) },
            {128668550, new Commodity(83, "Painite", "Minerals", 33000, false) },
            {128066403, new Commodity(84, "Limpet", "Unknown", 100, false) },
            {128668548, new Commodity(95, "Ai Relics", "Salvage", 132000, false) },
            {128668551, new Commodity(96, "Antiquities", "Salvage", 110000, false) },
            {128671118, new Commodity(97, "Osmium", "Metals", 7031, false) },
            {128671443, new Commodity(98, "Sap 8 Core Container", "Salvage", 55000, false) },
            {128671444, new Commodity(99, "Trinkets Of Hidden Fortune", "Salvage", 880, false) },
            {128666754, new Commodity(100, "Trade Data", "Salvage", 1450, false) },
            {128672308, new Commodity(101, "Thermal Cooling Units", "Machinery", 378, false) },
            {128672313, new Commodity(102, "Skimmer Components", "Machinery", 1089, false) },
            {128672307, new Commodity(103, "Geological Equipment", "Machinery", 1976, false) },
            {128672311, new Commodity(104, "Structural Regulators", "Technology", 1976, false) },
            {128672297, new Commodity(105, "Pyrophyllite", "Minerals", 1646, false) },
            {128672295, new Commodity(107, "Goslarite", "Minerals", 972, false) },
            {128672294, new Commodity(108, "Cryolite", "Minerals", 2378, false) },
            {128672301, new Commodity(109, "Thorium", "Metals", 11860, false) },
            {128672299, new Commodity(110, "Thallium", "Metals", 3937, false) },
            {128672298, new Commodity(111, "Lanthanum", "Metals", 9125, false) },
            {128672300, new Commodity(112, "Bismuth", "Metals", 2531, false) },
            {128672306, new Commodity(113, "Bootleg Liquor", "Legal Drugs", 408, false) },
            {128672701, new Commodity(114, "Meta-Alloys", "Industrial Materials", 83530, false) },
            {128672302, new Commodity(115, "Ceramic Composites", "Industrial Materials", 259, false) },
            {128672314, new Commodity(116, "Evacuation Shelter", "Consumer Items", 395, false) },
            {128672303, new Commodity(117, "Synthetic Reagents", "Chemicals", 7031, false) },
            {128672305, new Commodity(118, "Surface Stabilisers", "Chemicals", 631, false) },
            {128672309, new Commodity(119, "Building Fabricators", "Machinery", 1223, false) },
            {128672312, new Commodity(121, "Landmines", "Weapons", 4474, false) },
            {128672304, new Commodity(122, "Nerve Agents", "Chemicals", 13527, false) },
            {128672310, new Commodity(124, "Muon Imager", "Technology", 6589, false) },
            {128666755, new Commodity(127, "Military Plans", "Salvage", 5400, false) },
            {128666756, new Commodity(128, "Ancient Artefact", "Salvage", 5000, false) },
            {128666758, new Commodity(130, "Experimental Chemicals", "Salvage", 2250, false) },
            {128666759, new Commodity(131, "Rebel Transmissions", "Salvage", 2100, false) },
            {128666760, new Commodity(132, "Prototype Tech", "Salvage", 6100, false) },
            {128666761, new Commodity(133, "Technical Blueprints", "Salvage", 3750, false) },
            {128668547, new Commodity(246, "Unknown Artefact", "Salvage", 330000, false) },
            {128668549, new Commodity(247, "Hafnium 178", "Metals", 66000, false) },
            {128668552, new Commodity(248, "Military Intelligence", "Salvage", 52800, false) },
        };
        private static Dictionary<string, Commodity> CommoditiesByCargoName = CommoditiesByEliteID.ToDictionary(kp => kp.Value.Name.ToLower().Replace(" ", ""), kp => kp.Value);

        public static Commodity CommodityFromEliteID(long id)
        {
            Commodity Commodity = new Commodity();

            Commodity Template;
            if (CommoditiesByEliteID.TryGetValue(id, out Template))
            {
                Commodity.EDDBID = Template.EDDBID;
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
                Commodity.Name = Template.Name;
                Commodity.Category = Template.Category;
                Commodity.Rare = Template.Rare;
                Commodity.AveragePrice = Template.AveragePrice;
            }
            return Commodity;
        }
    }
}


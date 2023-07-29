using JetBrains.Annotations;
using MathNet.Numerics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using Utilities;

namespace EddiDataDefinitions
{

    public class OrganicSpeciesBase
    {
        public string name;
        public int value;
        public string description;
        public string conditions;

        public OrganicSpeciesBase ()
        {
            this.name = "";
            this.value = 0;
            this.description = "";
            this.conditions = "";
        }
    };

    public class OrganicSpecies : OrganicSpeciesBase
    {
        public OrganicSpecies ()
        {
            this.name = "Invalid";
            this.value = 0;
            this.description = "Invalid species";
            this.conditions = "";
        }

        public OrganicSpecies ( string species, int value, string desc, string conditions )
        {
            this.name = species;
            this.value = value;
            this.description = desc;
            this.conditions = conditions;
        }
    };

    public class OrganicGenusBase
    {
        public string name;
        public int distance;
        public string description;

        public OrganicGenusBase()
        {
            this.name = "";
            this.distance = 0;
            this.description = "";
        }
    };

    public class OrganicGenus : OrganicGenusBase
    {
        public Dictionary<string, OrganicSpecies> species = new Dictionary<string, OrganicSpecies>();

        public OrganicGenus ( string genus, int distance, string desc )
        {
            this.name = genus;
            this.distance = distance;
            this.description = desc;
        }

        public void Add ( string species, int distance, string desc, string conditions )
        {
            OrganicSpecies myData = new OrganicSpecies(species, distance, desc, conditions);
            this.species.Add( species, myData );
        }
    };

    public class OrganicData
    {
        public OrganicGenusBase genus;
        public OrganicSpeciesBase species;
    }

    public class OrganicInfo
    {

        // Exobiology
        private static OrganicGenus Aleoida           = new OrganicGenus("Aleoida",             150, "These are extremely hardy photosynthetic organisms that thrive in arid environments. " +
                                                                                                        "Thick, waxy leaf structures protect them from extreme surroundings. When " +
                                                                                                        "gaseous exchange becomes unfavourable.the leaves can completely shut off " +
                                                                                                        "the organism from the atmosphere causing a state of hibernation. The " +
                                                                                                        "pointed leaves create precipitation slopes, which draw liquids to the heart " +
                                                                                                        "of the organism.Here they are absorbed through a series of specialised cells, " +
                                                                                                        "and stored in the root structure until needed.");
        private static OrganicGenus Amphora           = new OrganicGenus("Amphora",             100, "These organic structures take their name from a type of container dating from " +
                                                                                                        "Earth’s Neolithic period.");
        private static OrganicGenus Anemone           = new OrganicGenus("Anemone",             100, "Despite their name, these organic structures more closely resemble the shells " +
                                                                                                        "of sea urchins.These organic structures can tolerate a wide range of temperatures " +
                                                                                                        "and are able to absorb energy from nearby stars.");
        private static OrganicGenus Bacterium         = new OrganicGenus("Bacterium",           500, "These are true unicellular extremophiles capable of living in the full spectrum of temperatures, " +
                                                                                                        "chemical soups and atmospheres. They form a kaleidoscopic range of patterns and " +
                                                                                                        "colours based on their method of metabolism.They derive energy from photosynthetic " +
                                                                                                        "chemosynthetic or thermosynthetic processes.These are believed to be the precursors " +
                                                                                                        "for many life forms, and are often found in conjunction with other species. Links " +
                                                                                                        "between the biochemistry of bacterial colonies and neighbouring organisms are " +
                                                                                                        "likely but as yet unproven.");
        private static OrganicGenus BarkMounds        = new OrganicGenus("Bark Mounds",         100, "These organic structures survive by absorbing elements from nova and supernova, " +
                                                                                                        "with the dense outer layer protecting them from the severest radiation.");
        private static OrganicGenus BrainTree         = new OrganicGenus("Brain Tree",          100, "These organic structures are so called because of the vaguely brain-like growths " +
                                                                                                        "on the ends of their branches. These resilient organic structures absorb minerals " +
                                                                                                        "via their subsurface roots and energy via their outer skin.");
        private static OrganicGenus Cactoida          = new OrganicGenus("Cactoida",            300, "These are photosynthetic organisms that have adapted to extreme conditions by reducing their " +
                                                                                                        "surface area to volume ratio, thereby protecting more sensitive tissues from " +
                                                                                                        "exposure.The outer layer is formed from specialised light-transmitting cells. " +
                                                                                                        "These are filled with an insulating hydrophobic layer, which helps to maintain " +
                                                                                                        "core temperature and liquid retention.Deep, extensive root structures hold the " +
                                                                                                        "organisms in place, and facilitate the extraction of trace minerals.Some cactoida " +
                                                                                                        "species feature explosive seed distribution as a method of reproduction.");
        private static OrganicGenus Clypeus           = new OrganicGenus("Clypeus",             150, "These are extremophile organisms that have evolved to create hard shield structures, primarily " +
                                                                                                        "to protect against stellar radiation. They also collect and condense traces of " +
                                                                                                        "liquid material in the atmosphere, allowing the organisms to flourish in extremely " +
                                                                                                        "arid environments.The shields are typically synthesised from surrounding inorganic " +
                                                                                                        "material.which will frequently define their shape and colouration.");
        private static OrganicGenus Concha            = new OrganicGenus("Concha",              150, "These are highly specialised complex extremophiles that have developed protective and sturdy " +
                                                                                                        "motile shell structures.These open and close based on the suitability of the " +
                                                                                                        "current environmental conditions.The shells are an organic structure with an " +
                                                                                                        "excreted inorganic insulated and sometimes reﬂective casing that help the organism " +
                                                                                                        "maintain homeostasis.The internal organisms, which are remarkably tough in their " +
                                                                                                        "own right, are only exposed for metabolic and reproductive purposes.");
        private static OrganicGenus CrystallineShards = new OrganicGenus("Crystalline Shards",  100,  "These crystalline structures are created by large colonies of microorganisms.");
        private static OrganicGenus Electricae        = new OrganicGenus("Electricae",          1000, "These are organisms found exclusively on extremely cold ice worlds in the vicinity of frozen " +
                                                                                                        "lakes.The visible tips can be observed protruding from the ice, often near fissures " +
                                                                                                        "where it is thinnest.The bulk of the organisms extend down through the ice into " +
                                                                                                        "subsurface melt potentially for several kilometres. Electricae are superconductive " +
                                                                                                        "in nature.utilising the thermal circulation of the surrounding fluid to drive an " +
                                                                                                        "electrochemical process.This is probably why they are limited to planets with " +
                                                                                                        "atmospheres dominated by noble gases.The surface structure exists to provide a " +
                                                                                                        "connection to the atmosphere, which in turn creates a point of electrical potential " +
                                                                                                        "difference. A by-product of this is the bioluminescent display that runs through " +
                                                                                                        "the organism. Although never witnessed, reproduction presumably occurs below the " +
                                                                                                        "surface by some unidentified process.");
        private static OrganicGenus Fonticulua         = new OrganicGenus("Fonticula",          500,  "These are photosynthetic colony organisms found exclusively on ice worlds, where they have " +
                                                                                                        "embraced the surrounding frozen material as a form of protection. As the fonticulus " +
                                                                                                        "develop they melt ice from around them, absorbing the liquid through tiny cellular " +
                                                                                                        "pores and passing it to the colony’s reproductive edge.Here the liquid is excreted " +
                                                                                                        "and immediately refreezes, creating hard translucent exoskeletons that provide " +
                                                                                                        "protection for the organisms. Frond structures create a wide flat space that expose " +
                                                                                                        "internal photosynthetic cells to as much light as possible. Reproduction appears to " +
                                                                                                        "occur by colony division, most likely when a shard of the structure collapses under " +
                                                                                                        "its own weight and the smaller shard creates a new colony.");
        private static OrganicGenus Frutexa           = new OrganicGenus("Frutexa",             150, "These are robust woody plants with deep rooting structures to gather liquids and hold themselves " +
                                                                                                        "in place where the ground may be unstable. They are photosynthetic organisms with " +
                                                                                                        "specialised photoreceptors that work even in low light conditions. As such they are " +
                                                                                                        "highly successful, and are widespread and diverse in nature. Their small leaf " +
                                                                                                        "structures protect them against extremely low temperatures and liquid loss in " +
                                                                                                        "higher temperatures.");
        private static OrganicGenus Fumerola          = new OrganicGenus("Fumerola",            100, "These are extremophile organisms located in regions with active fumaroles. Their metabolism is " +
                                                                                                        "driven exclusively through chemosynthetic and thermosynthetic mechanisms based on " +
                                                                                                        "nearby volcanic activity. Proximity to volcanic heat allows them to survive in " +
                                                                                                        "environments where the ambient temperature is naturally too cold for them.They " +
                                                                                                        "frequently incorporate minerals from the fumaroles’ ejecta, meaning they can appear " +
                                                                                                        "inorganic at first glance and may sport exotic colours.");
        private static OrganicGenus Fungoida          = new OrganicGenus("Fungoida",            300, "These are organisms that live deep inside a planetary substrate. They share similar morphology to " +
                                                                                                        "fungi but are not saprophytic, instead their mycelial body drives its metabolism " +
                                                                                                        "through chemosynthetic and thermosynthetic processes. These are facilitated by the " +
                                                                                                        "substrate which also protects the organism from environmental extremes. The exposed " +
                                                                                                        "aspects of the organisms are primarily involved in reproduction.This is frequently " +
                                                                                                        "through spore ejection, but certain species also support gaseous exchange with the " +
                                                                                                        "atmosphere. Some fungoicla exhibit bioluminescent behaviours as part of a metabolic " +
                                                                                                        "process involved in the breakdown of accumulated toxins.");
        private static OrganicGenus Osseus            = new OrganicGenus("Osseus",              800, "These are slow-growing organisms that can be found exclusively on rocky areas of planets. They " +
                                                                                                        "are defined by a symbiotic relationship that has evolved between two unicellular " +
                                                                                                        "organisms. which are now inseparable. One cell type is solely responsible for energy " +
                                                                                                        "production by either photosynthetic.chemosynthetic or thermosynthetic processes. " +
                                                                                                        "The symbiotic cells harvest some of this energy, and in turn deposit a hard rock-like " +
                                                                                                        "substance extracted from the local geology to create a rigid endoskeleton. This " +
                                                                                                        "structure provides a solid base for the organism to exist. It features complex folds " +
                                                                                                        "that help increase available surface area for metabolic interactions. Osseus have " +
                                                                                                        "been observed to create callus-like cell coverings, and withdraw themselves into " +
                                                                                                        "the endoskeleton for protection.");
        private static OrganicGenus Recepta           = new OrganicGenus("Recepta",             150, "These are extremophiles that are found exclusively on planets wlth atmospheres dominated by " +
                                                                                                        "sulphur dioxide.Using a combination of inorganic and hydrocarbon materials, they " +
                                                                                                        "build a shielding bubble.This allows them to create an isolated biome with regulated " +
                                                                                                        "temperature and chemical composition. Growth is a difﬁcult and complex process " +
                                                                                                        "that requires careful melting, regrowing and freezing of the external shell so that " +
                                                                                                        "the internal organism can develop.This is a gradual process meaning that larger " +
                                                                                                        "recepta are of significant age.Reproduction is also similarly complex and revolves " +
                                                                                                        "around a budding process, which creates a smaller version of the adult.Once detached, " +
                                                                                                        "it can roll under the action of gravity and air currents before coming to rest, where " +
                                                                                                        "it will deploy a holdfast marking its final position.");
        private static OrganicGenus SinuousTuber      = new OrganicGenus("Sinuous Tuber",       100, "These organic structures are distinguished by their tubular shape and vivid colouration. " +
                                                                                                        "These organic structures are merely the above-ground portion of a much larger " +
                                                                                                        "subterranean organism.");
        private static OrganicGenus Stratum           = new OrganicGenus("Stratum",             500, "These are Low-lying photosynthetic organisms that bond tightly to the surface of rocks.The body " +
                                                                                                        "of the organism may be embedded in the rock subsurface to provide protection from " +
                                                                                                        "the elements, leaving the tough photosynthetic proto-leaves exposed. Their simple " +
                                                                                                        "proto-evolutionary nature means that they are a common sight on rocky worlds. " +
                                                                                                        "Colouration is driven by a mixture of the mineral content of the attached rock and " +
                                                                                                        "the absorption spectral of the nearby stellar body.");
        private static OrganicGenus Tubus             = new OrganicGenus("Tubus",               800, "These are highly specialised organisms that can grow to extreme heights, which peak when in a " +
                                                                                                        "mature state.The organisms’ height and ability to survive is largely constrained by " +
                                                                                                        "gravity. The lack of any real atmospheric weather has not put evolutionaly pressure " +
                                                                                                        "on the towers to be strong, so they form tall but thin structures which cannot be " +
                                                                                                        "supported in high-gravity environments. The tower is formed from wrapped leaf-like " +
                                                                                                        "structures, which create a regulated chimney void. This is used by the organism to " +
                                                                                                        "create and maintain an artificial internal atmosphere, where discrete chemical " +
                                                                                                        "processes can be undertaken along the length of the tower. Waste gases and other " +
                                                                                                        "material may he released from the top of the tower.Some species feature external " +
                                                                                                        "rings that can be used to gauge an organism’s age.");
        private static OrganicGenus Tussock           = new OrganicGenus("Tussock",             200, "These are robust photosynthetic plants similar in appearance to clump grasses. They have " +
                                                                                                        "a shallow but complex root structure, which requires a solid surface to produce " +
                                                                                                        "structural stability. Clumps may form through tillering or wider dispersal through " +
                                                                                                        "a variety of seeding mechanisms. Some species have adapted cellular structures " +
                                                                                                        "containing high sugar concentrations to protect against freezing damage.");

        // Other Organics
        //  - Sample distance not used
        private static OrganicGenus MineralSpheres    = new OrganicGenus( "Mineral Spheres",    0, "These mineral structures are created by large colonies of ancient microorganisms." );
        private static OrganicGenus MetallicCrystals  = new OrganicGenus( "Metallic Crystals",  0, "These crystalline structures are created by huge numbers of primordial microorganisms." );
        private static OrganicGenus SilicateCrystals  = new OrganicGenus( "Silicate Crystals",  0, "These dense crystalline structures are created by large colonies of ancient microorganisms." );
        private static OrganicGenus IceCrystals       = new OrganicGenus( "Ice Crystals",       0, "These crystalline structures are created by microorganisms believed to be some of the oldest life forms in the galaxy." );
        private static OrganicGenus ReelMolluscs      = new OrganicGenus( "Reel Molluscs",      0, "This organism is protected by a reel-shaped husk, allowing it to survive for millennia in the vacuum of space." );
        private static OrganicGenus GlobeMolluscs     = new OrganicGenus( "Globe Molluscs",     0, "These organisms are so called because of their spherical shell. Despite being animals, they generate chemical energy through a process similar to photosynthesis, using their tentacles to absorb starlight." );
        private static OrganicGenus BellMolluscs      = new OrganicGenus( "Bell Molluscs",      0, "A bell-shaped organism with distinctive linear patterns on its underside, which feeds off starlight converted into chemical energy." );
        private static OrganicGenus UmbrellaMolluscs  = new OrganicGenus( "Umbrella Molluscs",  0, "This organism is protected by a umbrella-shaped husk, allowing it to survive for millennia in the vacuum of space." );
        private static OrganicGenus GourdMollusc      = new OrganicGenus( "Gourd Mollusc",      0, "A gourd-shaped organism that feeds off starlight converted into chemical energy." );
        private static OrganicGenus TorusMolluscs     = new OrganicGenus( "Torus Molluscs",     0, "A ring-shaped organism with a twin set of tentacles, which feeds off starlight converted into chemical energy." );
        private static OrganicGenus BulbMolluscs      = new OrganicGenus( "Bulb Molluscs",      0, "This organism is protected by a bulb-shaped husk, allowing it to survive for millennia in the vacuum of space." );
        private static OrganicGenus ParasolMolluscs   = new OrganicGenus( "Parasol Molluscs",   0, "This organism is protected by a parasol-shaped husk, allowing it to survive for millennia in the vacuum of space." );
        private static OrganicGenus SquidMolluscs     = new OrganicGenus( "Squid Molluscs",     0, "A squid-shaped organism with tentacles and an extended head, which feeds off starlight converted into chemical energy." );
        private static OrganicGenus BulletMolluscs    = new OrganicGenus( "Bullet Molluscs",    0, "A bullet-shaped orgamism that feeds off starlight converted into chemical energy." );
        private static OrganicGenus CapsuleMolluscs   = new OrganicGenus( "Capsule Molluscs",   0, "This organism is protected by a capsule-shaped husk, allowing it to survive for millennia in the vacuum of space." );
        private static OrganicGenus CollaredPod       = new OrganicGenus( "Collared Pod",       0, "These organisms absorb energy through their distinctive collar." );
        private static OrganicGenus StolonPod         = new OrganicGenus( "Stolon Pod",         0, "These seed pods are colloquially known as space oysters due to the pearlescent object sometimes found at the centre of the pod." );
        private static OrganicGenus StolonTree        = new OrganicGenus( "Stolon Tree",        0, "These organic structures are incredibly long lived, enduring for millennia in the vacuum of space." );
        private static OrganicGenus AsterPods         = new OrganicGenus( "Aster Pods",         0, "These robust seed pods will drift through space for thousands of years before finding a suitable place to release their seeds and spores." );
        private static OrganicGenus ChalicePods       = new OrganicGenus( "Chalice Pods",       0, "These robust seed pods are able to survive indefinitely in the vacuum of space." );
        private static OrganicGenus PedunclePods      = new OrganicGenus( "Peduncle Pods",      0, "These robust seed pods contain both seeds and spores." );
        private static OrganicGenus RhizomePods       = new OrganicGenus( "Rhizome Pods",       0, "The parent organism of these seed pods has not been identified, and it is possible that they are entirely self-contained." );
        private static OrganicGenus QuadripartitePods = new OrganicGenus( "Quadripartite Pods", 0, "These seed pods have a thick husk that protects the fleshy interior from the vacuum of space." );
        private static OrganicGenus OctahedralPods    = new OrganicGenus( "Octahedral Pods",    0, "These seed pods are distinguished by their leathery exterior and colourful bioluminescence." );
        private static OrganicGenus AsterTrees        = new OrganicGenus( "Aster Trees",        0, "These organic structures are not trees in the traditional sense, but long-lived and extremely hardy space-based organisms." );
        private static OrganicGenus PeduncleTrees     = new OrganicGenus( "Peduncle Trees",     0, "These organic structures are able to survive and propagate in the vacuum of space." );
        private static OrganicGenus GyreTrees         = new OrganicGenus( "Gyre Trees",         0, "These organisms are characterised by their long, slender limbs, which often terminate in a large seed pod." );
        private static OrganicGenus GyrePods          = new OrganicGenus( "Gyre Pods",          0, "These seed pods are unusual in that they are covered in a fine layer of living tissue." );
        private static OrganicGenus VoidHearts        = new OrganicGenus( "Void Hearts",        0, "These organic structures, sometimes called barbed knots, are noted for periodically emitting tremendous bursts of heat and light." );
        private static OrganicGenus CalcitePlates     = new OrganicGenus( "Calcite Plates",     0, "These calcium-carbonate structures are created by vast numbers of tiny polyp-like creatures." );
        private static OrganicGenus ThargoidBarnacles = new OrganicGenus( "Thargoid Barnacles", 0, "" );


        // For easier reverse lookups
        public static Dictionary<string, string> Species = new Dictionary<string, string>();

        static OrganicInfo ()
        {
            // Exobiology
            Aleoida.Add( "Aleoida Arcus", 7252500, "This aleoida species has upright clumps of long serrated leaves, which can open up to expose a reproductive organ containing tiny round seeds.", "This species requires a planet with a maximum gravity of 0.27, a temperature range of 175 to 180 kelvin, and Carbon Dioxide atmosphere." );
            Aleoida.Add( "Aleoida Coronamus", 6284600, "This interleaved crown of mottled leaves can grow to head height, with explosive seed pods emerging on long protruding stalks.", "This species requires a planet with a maximum gravity of 0.27, a temperature range of 180 to 190 kelvin, and Carbon Dioxide atmosphere." );
            Aleoida.Add( "Aleoida Gravis", 12934900, "These aleoida’s wide flat leaves on a heavy bark base can reach huge sizes. and sprout a dome-shaped reproductive organ at their peak.", "This species requires a planet with a maximum gravity of 0.27, a temperature range of 190 to 195 kelvin, and Carbon Dioxide atmosphere." );
            Aleoida.Add( "Aleoida Laminiae", 3385200, "These aleoida have a circle of upturned leaves marked with patterns, surrounding a bright fleshy pod with darker markings which matures in their centre.", "This species requires a planet with a maximum gravity of 0.27, and Ammonia atmosphere." );
            Aleoida.Add( "Aleoida Spica", 3385200, "An aleoida species with long spiky leaves that can reach over two metres high surrounding a single reproductive organ on a long central stalk.", "This species requires a planet with a maximum gravity of 0.27, and Ammonia atmosphere." );
            Amphora.Add( "Amphora Plant", 1628800, "", "This species requires a Metal Rich planet with an A type star, and No atmosphere. Additionally An Earth-Like, Ammonia, water giant or Gas Giant with water or ammonia based life must be present in system.." );
            Anemone.Add( "Blatteum Bioluminescent Anemone", 1499900, "", "This species requires a planet with an O, B and sometimes A type star, and Sulfur Dioxide or No atmosphere." );
            Anemone.Add( "Croceum Anemone", 1499900, "", "This species requires a planet with an O, B and sometimes A type star, and Sulfur Dioxide or No atmosphere." );
            Anemone.Add( "Luteolum Anemone", 1499900, "", "This species requires a planet with an O, B and sometimes A type star, and Sulfur Dioxide or No atmosphere." );
            Anemone.Add( "Prasinum Bioluminescent Anemone", 1499900, "", "This species requires a planet with an O, B and sometimes A type star, and Sulfur Dioxide or No atmosphere." );
            Anemone.Add( "Puniceum Anemone", 1499900, "", "This species requires a planet with an O, B and sometimes A type star, and Sulfur Dioxide or No atmosphere." );
            Anemone.Add( "Roseum Anemone", 1499900, "", "This species requires a planet with an O, B and sometimes A type star, and Sulfur Dioxide or No atmosphere." );
            Anemone.Add( "Roseum Bioluminescent Anemone", 1499900, "", "This species requires a planet with an O, B and sometimes A type star, and Sulfur Dioxide or No atmosphere." );
            Anemone.Add( "Rubeum Bioluminescent Anemone", 1499900, "", "This species requires a planet with an  type star, and Sulfur Dioxide or No atmosphere." );
            Bacterium.Add( "Bacterium Acies", 1000000, "A bacterial species that converts energy from neon-based atmospheres, creating looping whirls of bright colour.", "This species requires a planet with a Neon or Neon-rich atmosphere." );
            Bacterium.Add( "Bacterium Alcyoneum", 1658500, "A bacterial species found in ammonia-based atmospheres that lives in sunlight. A colony’s appearance resembles an intricate maze.", "This species requires a planet with a Ammonia atmosphere." );
            Bacterium.Add( "Bacterium Aurasus", 1000000, "These bacteria thrive on sunlight in atmospheres rich with carbon clioxicle. They cause blanket coloration across a planetary surface.", "This species requires a planet with a Carbon Dioxide atmosphere." );
            Bacterium.Add( "Bacterium Bullaris", 1152500, "This species of bacteria thrives on atmospheric methane, appearing as a network of linked bubble paiterns.", "This species requires a planet with a Methane or Methane-rich atmosphere." );
            Bacterium.Add( "Bacterium Cerbrus", 1689800, "A sunlight-converting bacterial species on worlds with atmospheres dominated by water and sulphur dioxide. Their colonies resemble a brain-shaped mass of smaller connected cells.", "This species requires a planet with a Water or Sulfur Dioxide atmosphere." );
            Bacterium.Add( "Bacterium Informem", 8418000, "These bacteria can be found in nitrogen atmospheres, and form a shapeless mass across the surface.", "This species requires a planet with a Nitrogen atmosphere." );
            Bacterium.Add( "Bacterium Nebulus", 5289900, "A bacterial species that survives exclusively on atmospheric helium. They are distinguished by a radial pattern extending outward from the colony’s centre.", "This species requires a planet with a Helium atmosphere." );
            Bacterium.Add( "Bacterium Omentum", 4638900, "These bacteria convert geothermal heat from nitrogen-based volcanic sites into energy. They appear as long interlinked strands across the surface.", "This species requires a planet with a Neon or Neon-rich atmosphere and Nitrogen or Ammonia volcanism." );
            Bacterium.Add( "Bacterium Scopulum", 4934500, "These bacteria thrive on the heat generated by carbon-based volcanic activity and appear as long swirling ridges on the surface.", "This species requires a planet with a Neon or Neon-rich atmosphere and Carbon or Methane volcanism." );
            Bacterium.Add( "Bacterium Tela", 1949000, "These bacteria appear as an intricate web pattern. They thrive in proximity to helium-based, iron-based and silicate-based volcanic sites.", "This species requires a planet with Helium, Iron, Silicate or Ammonia volcanism." );
            Bacterium.Add( "Bacterium Verrata", 3897000, "These bacteria appear as an intricate web pattern. They thrive in proximity to helium-based, iron-based and silicate-based volcanic sites.", "This species requires a planet with a Neon or Neon-rich atmosphere and Water volcanism." );
            Bacterium.Add( "Bacterium Vesicula", 1000000, "These bacteria survive on worlds with argon—based atmospheres, and appear as a collection of tight loops on the ground.", "This species requires a planet with a Argon atmosphere." );
            Bacterium.Add( "Bacterium Volu", 7774700, "A bacterial species dependent upon oxygen atmospheres, which creates random swirling patterns across the ground.", "This species requires a planet with a Oxygen atmosphere." );
            BarkMounds.Add( "Bark Mounds", 1471900, "", "This species requires a planet with a No Atmosphere atmosphere, and In or <150 light years from a nebula." );
            BrainTree.Add( "Aureum Brain Tree", 1593700, "", "This species requires a planet with a No atmosphere and Any volcanism, and near system with Guardian ruins." );
            BrainTree.Add( "Gypseeum Brain Tree", 1593700, "", "This species requires a planet with a No atmosphere and Any volcanism, and near system with Guardian ruins." );
            BrainTree.Add( "Lindigoticum Brain Tree", 1593700, "", "This species requires a planet with a No atmosphere and Any volcanism, and near system with Guardian ruins." );
            BrainTree.Add( "Lividum Brain Tree", 1593700, "", "This species requires a planet with a No atmosphere and Any volcanism, and near system with Guardian ruins." );
            BrainTree.Add( "Ostrinum Brain Tree", 1593700, "", "This species requires a planet with a No atmosphere and Any volcanism, and near system with Guardian ruins." );
            BrainTree.Add( "Puniceum Brain Tree", 1593700, "", "This species requires a planet with a No atmosphere and Any volcanism, and near system with Guardian ruins." );
            BrainTree.Add( "Roseum Brain Tree", 1593700, "", "This species requires a planet with a No atmosphere and Any volcanism, and near system with Guardian ruins." );
            BrainTree.Add( "Viride Brain Tree", 1593700, "", "This species requires a planet with a No atmosphere and Any volcanism, and near system with Guardian ruins." );
            Cactoida.Add( "Cactoida Cortexum", 3667600, "A species of cactoid that can reach over three metres in height. They are composed of multiple growths that sprout sealed pods at their peaks, which open up to distribute seeds.", "This species requires a planet with a maximum gravity of 0.27, a temperature range of 180 to 195 kelvin, and Carbon Dioxide atmosphere." );
            Cactoida.Add( "Cactoida Lapis", 2483600, "This cactoid species appears as a squat growth with a latficecl upper surface, which eventually produces a cluster of seed pods.", "This species requires a planet with a maximum gravity of 0.27, and Ammonia atmosphere. Unofficially, additional sources say  this is also known as Tetonus aymericus." );
            Cactoida.Add( "Cactoida Peperatis", 2483600, "A cactoid species appearing as a swollen five-sided growth, reaching over two metres high and topped with an intersected crown.", "This species requires a planet with a maximum gravity of 0.27, and Ammonia atmosphere." );
            Cactoida.Add( "Cactoida Pullulanta", 3667600, "This species of cactoid has a globular base, from which extend vertical cylinders that can reach over four metres. Rounded pods grow in clusters along the cylinders, which break open to scatter seeds.", "This species requires a planet with a maximum gravity of 0.27, and a temperature range of 180 to 195." );
            Cactoida.Add( "Cactoida Vermis", 16202800, "These cactoids appear as a tall collection of cylinders linked by an undulating membrane and topped with a spiky crown. They often have a spiny life-form attached that is thought to form a symbiotic relationship with the larger organism, although the nature of this is not understood.", "This species requires a planet with a maximum gravity of 0.27, and Water atmosphere." );
            Clypeus.Add( "Clypeus Lacrimam", 8418000, "A species of clypeus that grows a broad, tear-shaped shield to protect the sensitive organism from extreme sunlight. The shield’s ridges help to direct Water droplets down into the soil.", "This species requires a Rocky or High Metal Content planet with a maximum gravity of 0.27, a temperature greater than 190 kelvin, and Water or Carbon Dioxide atmosphere." );
            Clypeus.Add( "Clypeus Margaritus", 11873200, "This clypeus species produces a curved shield that resembles a large pearl in shape and texture. Up to three central organisms grow within it upon a supporting bed of leaves.", "This species requires a Rocky or High Metal Content planet with a maximum gravity of 0.27, a temperature greater than 190 kelvin, and Water or Carbon Dioxide atmosphere." );
            Clypeus.Add( "Clypeus Speculumi", 16202800, "A clypeus species that grows an angular shield with a mirrored exterior to protect the spiky organisms. This species can be found on planets orbiting their parent star at a distance of 5 AU or greater.", "This species requires a Rocky or High Metal Content planet with a maximum gravity of 0.27, a temperature greater than 190 kelvin, and Water or Carbon Dioxide atmosphere." );
            Concha.Add( "Concha Aureolas", 7774700, "These concha are found on worlds with nitrogen-based atmospheres. Their rounded rock-like structure splits part to extend long stalks topped with loops.", "This species requires a planet with a maximum gravity of 0.27, and Ammonia atmosphere." );
            Concha.Add( "Concha Biconcavis", 19010800, "This concha species resembles a ridged, bisected egg until they crack in half, allowing a thin stalk to sprout from its fleshy insides. This is covered with cloughnut—shaped pods that create locations for chemical exchange.", "This species requires a planet with a maximum gravity of 0.27, and Nitrogen atmosphere." );
            Concha.Add( "Concha Labiata", 2352400, "A concha species that thrives in atmospheres rich with carbon dioxide. The lip-like upper opening cracks apart to allow a vertical growth of spiky leaves and bright seeds to stretch upward.", "This species requires a planet with a maximum gravity of 0.27, and Carbon Dioxide atmosphere." );
            Concha.Add( "Concha Renibus", 4572400, "A species of concha that relies on heat sources to survive. As the bisected growth increases in size. it sprouts a single stalk topped with an array of luminous fronds that facilitate metabolism.", "This species requires a planet with a maximum gravity of 0.27, a temperature range of 180 to 195 kelvin, and Water or Carbon Dioxide atmosphere." );
            CrystallineShards.Add( "Crystalline Shards", 1628800, "", "This species requires a planet with an A or Neutron type star, a temperature range of 0 to 273 kelvin, and No atmosphere. Additionally An Earth-Like, Ammonia, water giant or Gas Giant with water or ammonia based life must be present in system and must be >12000 light seconds from the nearest star." );
            Electricae.Add( "Electricae Pluma", 6284600, "A species of electricae that extends a tip of four connected loops above the ice, each covered with brightly luminous fronds. This species is typically found on planets orbiting bright white stars.", "This species requires a Icy planet with a maximum gravity of 0.27, an A or Neutron type star, and Helium, Neon or Argon atmosphere." );
            Electricae.Add( "Electricae Radialem", 6284600, "These electricae species protrude bioluminescent stalks that radiate out in all directions. It is thought that this species may have an unspecified link with the proximity of nebulae to its host planet.", "This species requires a Icy planet with a maximum gravity of 0.27, an A or Neutron type star, and Helium, Neon or Argon atmosphere." );
            Fonticulua.Add( "Fonticulua Campestris", 1000000, "These fonticulua thrive in argon atmospheres, and can reach four metres in height. They feature huge leaf-like structures to capture sunlight for conversion to energy.", "This species requires a Icy or Rocky planet with a maximum gravity of 0.27, and Argon atmosphere." );
            Fonticulua.Add( "Fonticulua Digitos", 1804100, "A fonticulua species that thrives in methane—based atmospheres, sprouting a cluster of cylindrical tubes directly from the ice.", "This species requires a Icy or Rocky planet with a maximum gravity of 0.27, and Methane or Methane-rich atmosphere." );
            Fonticulua.Add( "Fonticulua Fluctus", 20000000, "A species of fonticulua that exists on worlds with oxygen atmospheres. They produce coiling wave-shaped structures which tilt toward sunlight.", "This species requires a Icy or Rocky planet with a maximum gravity of 0.27, and Oxygen atmosphere." );
            Fonticulua.Add( "Fonticulua Lapida", 3111000, "A fonticulua species that exists in atmospheres with a heavy concentration of nitrogen. Growing up along the main stalk are bright gem-like pods. which can break off and create new colonies.", "This species requires a Icy or Rocky planet with a maximum gravity of 0.27, and Nitrogen atmosphere." );
            Fonticulua.Add( "Fonticulua Segmentatus", 19010800, "A species of fonticulua found in atmospheres dominated by neon, appearing as a pyramid—shaped cluster of frilled sections.", "This species requires a Icy or Rocky planet with a maximum gravity of 0.27, and Neon or Neon-rich atmosphere." );
            Fonticulua.Add( "Fonticulua Upupam", 5727600, "This fonticulua species can he found on ice worlds with argon-rich atmospheres. They produce broad hoop-shaped structures to better reflect weak sunlight onto themselves for photosynthesis.", "This species requires a Icy or Rocky planet with a maximum gravity of 0.27, and Argon-Rich atmosphere." );
            Frutexa.Add( "Frutexa Acus", 7774700, "This frutexa species has vivid colouration when young that alters as it matures, its upper branches produce lines of small pea-like seed pods.", "This species requires a Rocky planet with a maximum gravity of 0.27, a temperature less than 195 kelvin, and Carbon Dioxide atmosphere." );
            Frutexa.Add( "Frutexa Collum", 1639800, "A species of frutexa characterised by its spiky lower branches surrounding a thick central column, which is clotted with spores and with a dark crown.", "This species requires a Rocky planet with a maximum gravity of 0.27, and Sulfur Dioxide atmosphere." );
            Frutexa.Add( "Frutexa Fera", 1632500, "This species of frutexa combines broad branches with long thin stalks, along which grow clusters of lightweight seed pods that are scaitered by light winds.", "This species requires a Rocky planet with a maximum gravity of 0.27, a temperature less than 195 kelvin, and Carbon Dioxide atmosphere." );
            Frutexa.Add( "Frutexa Flabellum", 1808900, "A species of frutexa that appears as a bush of leaves with a similar texture to seaweed. Seeds are extended on long stalks and protected by a cage formation until ready to germinate.", "This species requires a Rocky planet with a maximum gravity of 0.27, and Ammonia atmosphere." );
            Frutexa.Add( "Frutexa Flammasis", 10326000, "A frutexa species that gives the appearance of flames, with vivid upright fronds extended from multiple stalks. The fronds are dotted with disc-sha ped spores that are distributed by the wind.", "This species requires a Rocky planet with a maximum gravity of 0.27, and Ammonia atmosphere." );
            Frutexa.Add( "Frutexa Metallicum", 1632500, "This species of frutexa has an almost metallic shine to its small leaves. Along its upper branches grow spherical spores, which each have a star-shaped opening to increase germination.", "This species requires a High Metal Content planet with a maximum gravity of 0.27, a temperature less than 195 kelvin, and Carbon Dioxide or Ammonia atmosphere." );
            Frutexa.Add( "Frutexa Sponsae", 5988000, "A frutexa species that produces clusters of upright intertwining branches, which are crowned with bright seed sacks.", "This species requires a Rocky planet with a maximum gravity of 0.27, and Water atmosphere." );
            Fumerola.Add( "Fumerola Aquatis", 6284600, "A species of fumerola that can be found near sites of water-based volcanic activity. They appear as small dark clusters with ridged folds that trap heat within.", "This species requires a Icy or Rocky Ice planet with a maximum gravity of 0.27, Any atmosphere, and Water volcanism." );
            Fumerola.Add( "Fumerola Carbosis", 6284600, "A fumerola species found near sites of carbon—based volcanism, appearing as a thin upright tube. An inner organism protrudes from an opening at its peak to increase heat absorption.", "This species requires a Icy or Rocky Ice planet with a maximum gravity of 0.27, Any atmosphere, and Carbon or Methane volcanism." );
            Fumerola.Add( "Fumerola Extremus", 16202800, "An exception among its kin. this fumerola species seems to have an arbitrary preference of specific volcanism types which have yet to be explicitly linked in any way. They appear as long vertical stalks with smaller fronds that can stretch out.", "This species requires a Rocky or High Metal Content planet with a maximum gravity of 0.27, Any atmosphere, and Silicate, Iron or Rocky volcanism." );
            Fumerola.Add( "Fumerola Nitris", 7500900, "This species of fumerola prefers nitrogen-based volcanism. They produce an ovoid organism with dotted markings, which sits on top of a thin stalk.", "This species requires a Icy or Rocky Ice planet with a maximum gravity of 0.27, Any atmosphere, and Nitrogen or Ammonia volcanism." );
            Fungoida.Add( "Fungoida Bullarum", 3703200, "A fungoida that features clusters of mottled bubble-shaped growths atop a central stalk. These contain spores that can be exposed to the winds to facilitate distribution.", "This species requires a planet with a maximum gravity of 0.27, and Argon or Argon-rich atmosphere." );
            Fungoida.Add( "Fungoida Gelata", 3330300, "This fungoida species resembles an upturned jellyfish, emerging from a solid base buried within the substrate. The exposed part is dominated by fleshy reproductive organisms that shed organic tissue. This tissue can float on the light breeze and form a new organism if it lands in the right location.", "This species requires a planet with a maximum gravity of 0.27, and Water or Carbon Dioxide atmosphere. Additionally, Carbon Dioxide atmospheres requires a temperature range of 180 to 195 kelvin." );
            Fungoida.Add( "Fungoida Setisis", 1670100, "This fungoida species produces vertical clusters interspersed with spore pods atop thin stalks. allowing them to break off and scatter to reproduce elsewhere.", "This species requires a planet with a maximum gravity of 0.27, and Ammonia or Methane atmosphere." );
            Fungoida.Add( "Fungoida Stabitis", 2680300, "A species of fungoida that thrives on geothermal energy. and can produce two-metre high tower structures composed of tightly clustered cylinders.", "This species requires a planet with a maximum gravity of 0.27, and Water or Carbon Dioxide atmosphere." );
            Osseus.Add( "Osseus Cornibus", 1483000, "An osseus species that produces a stacked series of spiral structures up to about three metres. These ridged features are upturned to better absorb sunlight for photosynthesis.", "This species requires a Rocky or High Metal Content planet with a maximum gravity of 0.27, a temperature range of 180 to 195 kelvin, and Carbon Dioxide atmosphere." );
            Osseus.Add( "Osseus Discus", 12934900, "An osseus that appears as half-buried discs with radial patterns, which may resemble natural rook formations from a distance. They absorb geothermal energy from below the surface as well as available heat sources above ground.", "This species requires a Rocky or High Metal Content planet with a maximum gravity of 0.27, and Water atmosphere." );
            Osseus.Add( "Osseus Fractus", 4027800, "This osseus species can grow to over six metres across. They produce wide ridged frills for metabolic interactions including aosorbing sunlight for energy production.", "This species requires a planet with a maximum gravity of 0.27, a temperature range of 180 to 195 kelvin, and Carbon Dioxide atmosphere." );
            Osseus.Add( "Osseus Pellebantus", 9739000, "A species of osseus with a single broad stalk from which extend wide circular structures, with the largest plate capping the top to maximise sunlight absorption.", "This species requires a planet with a maximum gravity of 0.27, a temperature range of 180 to 195 kelvin, and Carbon Dioxide atmosphere." );
            Osseus.Add( "Osseus Pumice", 3156300, "This osseus species grows a single thick stalk from which emerges a wide, broadly circular, pitted endoskeleton. This structure is designed to dramatically increase the surface area to volume ofthe organism, facilitating chemical capture and chemosynthesis on its catalytically active surface.", "This species requires a Rocky, High Metal Content or Rocky Ice planet with a maximum gravity of 0.27, and Argon, Methane or Nitrogen atmosphere." );
            Osseus.Add( "Osseus Spiralis", 2404700, "A species of osseus that produces coiling spiral structures up to six metres wide. There are ridged folds on their upturned surfaces designed to capture sunlight.", "This species requires a Rocky or High Metal Content planet with a maximum gravity of 0.27, and Ammonia atmosphere." );
            Recepta.Add( "Recepta Conditivus", 14313700, "A recepta species where the body of the organism is suspended above ground inside a sphere-shaped translucent membrane. This is filled with chemical-rich ﬂuid that both protects the organism and provides the chemical soup needed for metabolism. Chemical exchange is controlled actively through the membrane and passively through the extensive root structure.", "This species requires a Icy or Rocky Ice planet with a maximum gravity of 0.27, and Sulfur Dioxide atmosphere." );
            Recepta.Add( "Recepta Deltahedronix", 16202800, "This species of recepta produces a thick lattice of trunks in a deltahedron shape. This grows around and above the globular central organism, and helps to capture, retain and focus geothermal heat for thermosynthesis.", "This species requires a Rocky or High Metal Content planet with a maximum gravity of 0.27, and Sulfur Dioxide atmosphere." );
            Recepta.Add( "Recepta Umbrux", 12934900, "A recepta species that grows a thick latticed structure for protection. A fine translucent membrane stretches between its gaps, allowing sunlight to penetrate and reach the inner organism for photosynthesis.", "This species requires a planet with a maximum gravity of 0.27, and Sulfur Dioxide atmosphere." );
            SinuousTuber.Add( "Albidum Sinuous Tubers", 1514500, "", "This species requires a planet with a No atmosphere and Any volcanism, and Seemingly more common near galactic core." );
            SinuousTuber.Add( "Blatteum Sinuous Tubers", 1514500, "", "This species requires a planet with a No atmosphere and Any volcanism, and Seemingly more common near galactic core." );
            SinuousTuber.Add( "Caeruleum Sinuous Tubers", 1514500, "", "This species requires a planet with a No atmosphere and Any volcanism, and Seemingly more common near galactic core." );
            SinuousTuber.Add( "Lindigoticum Sinuous Tubers", 1514500, "", "This species requires a planet with a No atmosphere and Any volcanism, and Seemingly more common near galactic core." );
            SinuousTuber.Add( "Prasinum Sinuous Tubers", 1514500, "", "This species requires a planet with a No atmosphere and Any volcanism, and Seemingly more common near galactic core." );
            SinuousTuber.Add( "Roseum Sinuous Tubers", 1514500, "", "This species requires a planet with a No atmosphere and Any volcanism, and Seemingly more common near galactic core." );
            SinuousTuber.Add( "Violaceum Sinuous Tubers", 1514500, "", "This species requires a planet with a No atmosphere and Any volcanism, and Seemingly more common near galactic core." );
            SinuousTuber.Add( "Viride Sinuous Tubers", 1514500, "", "This species requires a planet with a No atmosphere and Any volcanism, and Seemingly more common near galactic core." );
            Stratum.Add( "Stratum Araneamus", 2448900, "A stratum species that has a vaguely octopoid shape. Their pale semi-translucent upper domes can reveal colourful inner organisms, which contrast with their darker outstretched tentacles.", "This species requires a Rocky planet with a temperature greater than 165 kelvin, and Sulfur Dioxide atmosphere." );
            Stratum.Add( "Stratum Cucumisis", 16202800, "A species of stratum that displays fleshy ovoid shapes that are connected in a narrow pattern across the ground. These are covered with streaks of round photosynthetic cells that absorb sunlight.", "This species requires a Rocky planet with a temperature greater than 190 kelvin, and Sulfur Dioxide or Carbon Dioxide atmosphere." );
            Stratum.Add( "Stratum Excutitus", 2448900, "This stratum species appears as a mixture of tight concentric ring patterns and mottled proto-leaves in a mixture of dark hues.", "This species requires a Rocky planet with a temperature range of 165 to 190 kelvin, and Sulfur Dioxide or Carbon Dioxide atmosphere." );
            Stratum.Add( "Stratum Frigus", 2637500, "This species of stratum forms broad interconnected ring structures, which are composed of narrow ridges to capture sunlight.", "This species requires a Rocky planet with a temperature greater than 190 kelvin, and Sulfur Dioxide or Carbon Dioxide atmosphere." );
            Stratum.Add( "Stratum Laminamus", 2788300, "This particular stratum species gives the appearance of overlapping rock plateaus, each with narrow bands of colouration.", "This species requires a Rocky planet with a temperature greater than 165 kelvin, and Ammonia atmosphere." );
            Stratum.Add( "Stratum Limaxus", 1362000, "This species of stratum appears as a series of unconnected ovoid sha pes across the ground, which are the protruding tips of the larger subterranean organism.", "This species requires a Rocky planet with a temperature range of 165 to 190 kelvin, and Sulfur Dioxide or Carbon Dioxide atmosphere." );
            Stratum.Add( "Stratum Paleas", 1362000, "A stratum that blends thick overlapping vines with irregular growths. with varying colours appearing in bands or streaks.  ", "This species requires a Rocky planet with a temperature greater than 165 kelvin, and Ammonia, Water or Carbon Dioxide atmosphere." );
            Stratum.Add( "Stratum Tectonicas", 19010800, "A stratum species with a thick rock-like outer shell, covered with an irregular lattice of brighter cells that absorb sunlight for photosynthesis.", "This species requires a High Metal Content planet with a temperature greater than 165 kelvin, and Any Thin atmosphere." );
            Tubus.Add( "Tubus Cavas", 11873200, "A tubus species that extends pale vertical stalks composed of rigid modules. Colourful fronds frequently appear in the gaps between segments and aid with controlling gaseous exchange.", "This species requires a Rocky planet with a maximum gravity of 0.15, a temperature range of 160 to 190 kelvin, and Carbon Dioxide atmosphere." );
            Tubus.Add( "Tubus Compagibus", 7774700, "A tubus species with narrow pale segments and fronds growing between each module. A wide crown of leaves at the peak hold spores on their undersides, to germinate across a wide area.", "This species requires a Rocky planet with a maximum gravity of 0.15, a temperature range of 160 to 190 kelvin, and Carbon Dioxide atmosphere." );
            Tubus.Add( "Tubus Conifer", 2415500, "A tubus species formed from hollow vertical cylinders that can reach heights of six metres. Mature specimens are capped with a downtu rned crown that can distribute seeds on the wind across a wide area.", "This species requires a Rocky planet with a maximum gravity of 0.15, a temperature range of 160 to 190 kelvin, and Carbon Dioxide atmosphere." );
            Tubus.Add( "Tubus Rosarium", 2637500, "This tubus species is composed of squat tubes growing into a vertical spire. The upper pods of mature specimens produce explosive seed pods on their outer skin.", "This species requires a Rocky planet with a maximum gravity of 0.15, a temperature greater than 160 kelvin, and Ammonia atmosphere." );
            Tubus.Add( "Tubus Sororibus", 5727600, "This species of tubus grows a cluster of hollow stalks composed of rigid segments. Over time these become capped with a growth that flowers and produces seeds.", "This species requires a High Metal Content planet with a maximum gravity of 0.15, a temperature range of 160 to 190 kelvin, and Carbon Dioxide or Ammonia atmosphere." );
            Tussock.Add( "Tussock Albata", 3252500, "A tussock species characterised by leaves with a distinctive striped pattern that are bisected like a snake’s tongue. Mature versions also sprout smaller leaves which produce spores.", "This species requires a Rocky planet with a maximum gravity of 0.27, a temperature range of 175 to 180 kelvin, and Carbon Dioxide atmosphere." );
            Tussock.Add( "Tussock Capillum", 7025800, "This tussock species is a squat cluster of leaves resembling thick matted hair. From the top of these sprout thick pods that carw a number of round beans.", "This species requires a Rocky planet with a maximum gravity of 0.27, and Argon or Methane atmosphere." );
            Tussock.Add( "Tussock Caputus", 3472400, "A tussock species with leaves that have a thick segmented lower half and a willowy upper half. Mature versions produce separate stalks that carry ovoid organisms clotted with spores.", "This species requires a Rocky planet with a maximum gravity of 0.27, a temperature range of 180 to 190 kelvin, and Carbon Dioxide atmosphere." );
            Tussock.Add( "Tussock Catena", 1766600, "This species of tussock has very thin stalks carrying twin sets of seed sacks along their entire length, resembling links on a chain.", "This species requires a Rocky planet with a maximum gravity of 0.27, and Ammonia atmosphere." );
            Tussock.Add( "Tussock Cultro", 1766600, "A tussock species with tall sharp reeds reaching about two metres, characterised by narrow markings along their length.", "This species requires a Rocky planet with a maximum gravity of 0.27, and Ammonia atmosphere." );
            Tussock.Add( "Tussock Divisa", 1766600, "This tussock species blends thick segmented lower growths with longer. narrower leaves. Mature versions have pale spores along the upper branches.", "This species requires a Rocky planet with a maximum gravity of 0.27, and Ammonia atmosphere." );
            Tussock.Add( "Tussock Ignis", 1849000, "This tussock species produces thick intertwined leaves, above which sprout narrow stems crowned with seed pods.", "This species requires a Rocky planet with a maximum gravity of 0.27, a temperature range of 160 to 170 kelvin, and Carbon Dioxide atmosphere." );
            Tussock.Add( "Tussock Pennata", 5853800, "A tussock species that extends large seed pods on thin stems above a cluster of bright leaves.", "This species requires a Rocky planet with a maximum gravity of 0.27, a temperature range of 145 to 155 kelvin, and Carbon Dioxide atmosphere." );
            Tussock.Add( "Tussock Pennatis", 1000000, "A tussock species with feather-shaped growths surrounding a single segmented stem which when mature is crowned with colourful seeds.", "This species requires a Rocky planet with a maximum gravity of 0.27, a temperature less than 195 kelvin, and Carbon Dioxide atmosphere." );
            Tussock.Add( "Tussock Propagito", 1000000, "A species of tussock that sprouts tapering leaves, with tips covered with colourful seed pods.", "This species requires a Rocky planet with a maximum gravity of 0.27, a temperature less than 195 kelvin, and Carbon Dioxide atmosphere." );
            Tussock.Add( "Tussock Serrati", 4447100, "This tussock species sprouts serrated leaves around thick stalks that produce dark seed pods.", "This species requires a Rocky planet with a maximum gravity of 0.27, a temperature range of 170 to 175 kelvin, and Carbon Dioxide atmosphere." );
            Tussock.Add( "Tussock Stigmasis", 19010800, "This tussock species resembles a patch of tough. wiry grasses. Taller stalks carrying disc-shaped seed pods rise above the main organism when mature.", "This species requires a Rocky planet with a maximum gravity of 0.27, and Sulfur Dioxide atmosphere." );
            Tussock.Add( "Tussock Triticum", 7774700, "A species of tussock with thin tough leaves marked with dark stripes. From these sprout taller stalks with small leaves, from which seeds are released to the winds.", "This species requires a Rocky planet with a maximum gravity of 0.27, a temperature range of 190 to 195 kelvin, and Carbon Dioxide atmosphere." );
            Tussock.Add( "Tussock Ventusa", 3227700, "A species of tussock that blends tough lower stalks with taller willowy reeds, which produce small pale spores.", "This species requires a Rocky planet with a maximum gravity of 0.27, a temperature range of 155 to 160 kelvin, and Carbon Dioxide atmosphere." );
            Tussock.Add( "Tussock Virgam", 14313700, "A species of tussock with thin reeds clustered around a central stalk. which is eventually crowned with spores.", "This species requires a Rocky planet with a maximum gravity of 0.27, and Water atmosphere." );

            // Other Organics
            //  - Value is for a new codex entry voucher
            MineralSpheres.Add( "Solid Mineral Spheres", 50000, "", "" );
            MineralSpheres.Add( "Lattice Mineral Spheres", 50000, "", "" );
            MetallicCrystals.Add( "Prasinum Metallic Crystals", 50000, "", "" );
            MetallicCrystals.Add( "Purpureum Metallic Crystals", 50000, "", "" );
            MetallicCrystals.Add( "Rubeum Metallic Crystals", 50000, "", "" );
            MetallicCrystals.Add( "Flavum Metallic Crystals", 50000, "", "" );
            SilicateCrystals.Add( "Lindigoticum Silicate Crystals", 50000, "", "" );
            SilicateCrystals.Add( "Prasinum Silicate Crystals", 50000, "", "" );
            SilicateCrystals.Add( "Roseum Silicate Crystals", 50000, "", "" );
            SilicateCrystals.Add( "Purpureum Silicate Crystals", 50000, "", "" );
            SilicateCrystals.Add( "Albidium Silicate Crystals", 50000, "", "" );
            SilicateCrystals.Add( "Rubeum Silicate Crystals", 50000, "", "" );
            SilicateCrystals.Add( "Flavum Silicate Crystals", 50000, "", "" );
            IceCrystals.Add( "Lindigoticum Ice Crystals", 50000, "", "" );
            IceCrystals.Add( "Prasinum Ice Crystals", 50000, "", "" );
            IceCrystals.Add( "Roseum Ice Crystals", 50000, "", "" );
            IceCrystals.Add( "Purpureum Ice Crystals", 50000, "", "" );
            IceCrystals.Add( "Rubeum Ice Crystals", 50000, "", "" );
            IceCrystals.Add( "Albidium Ice Crystals", 50000, "", "" );
            IceCrystals.Add( "Flavum Ice Crystals", 50000, "", "" );
            ReelMolluscs.Add( "Luteolum Reel Mollusc", 50000, "", "" );
            ReelMolluscs.Add( "Lindigoticum Reel Mollusc", 50000, "", "" );
            ReelMolluscs.Add( "Viride Reel Mollusc", 50000, "", "" );
            GlobeMolluscs.Add( "Niveum Globe Molluscs", 50000, "", "" );
            BellMolluscs.Add( "Albens Bell Mollusc", 50000, "", "" );
            BellMolluscs.Add( "Blatteum Bell Mollusc", 50000, "", "" );
            BellMolluscs.Add( "Lindigoticum Bell Mollusc", 50000, "", "" );
            UmbrellaMolluscs.Add( "Luteolum Umbrella Mollusc", 50000, "", "" );
            UmbrellaMolluscs.Add( "Lindigoticum Umbrella Mollusc", 50000, "", "" );
            UmbrellaMolluscs.Add( "Viride Umbrella Mollusc", 50000, "", "" );
            GourdMollusc.Add( "Albulum Gourd Mollusc", 50000, "", "" );
            GourdMollusc.Add( "Caeruleum Gourd Mollusc", 50000, "", "" );
            GourdMollusc.Add( "Viride Gourd Mollusc", 50000, "", "" );
            GourdMollusc.Add( "Phoeniceum Gourd Mollusc", 50000, "", "" );
            GourdMollusc.Add( "Purpureum Gourd Mollusc", 50000, "", "" );
            GourdMollusc.Add( "Rufum Gourd Mollusc", 50000, "", "" );
            GourdMollusc.Add( "Croceum Gourd Mollusc", 50000, "", "" );
            TorusMolluscs.Add( "Caeruleum Torus Mollusc", 50000, "", "" );
            TorusMolluscs.Add( "Rubellum Torus Mollusc", 50000, "", "" );
            BulbMolluscs.Add( "Luteolum Bulb Mollusc", 50000, "", "" );
            BulbMolluscs.Add( "Lindigoticum Bulb Mollusc", 50000, "", "" );
            BulbMolluscs.Add( "Viride Bulb Mollusc", 50000, "", "" );
            ParasolMolluscs.Add( "Luteolum Parasol Mollusc", 50000, "", "" );
            ParasolMolluscs.Add( "Lindigoticum Parasol Mollusc", 50000, "", "" );
            ParasolMolluscs.Add( "Viride Parasol Mollusc", 50000, "", "" );
            SquidMolluscs.Add( "Albulum Squid Mollusc", 50000, "", "" );
            SquidMolluscs.Add( "Caeruleum Squid Mollusc", 50000, "", "" );
            SquidMolluscs.Add( "Puniceum Squid Mollusc", 50000, "", "" );
            SquidMolluscs.Add( "Rubeum Squid Mollusc", 50000, "", "" );
            SquidMolluscs.Add( "Roseum Squid Mollusc", 50000, "", "" );
            BulletMolluscs.Add( "Cereum Bullet Mollusc", 50000, "", "" );
            BulletMolluscs.Add( "Lividum Bullet Mollusc", 50000, "", "" );
            BulletMolluscs.Add( "Viride Bullet Mollusc", 50000, "", "" );
            BulletMolluscs.Add( "Rubeum Bullet Mollusc", 50000, "", "" );
            BulletMolluscs.Add( "Flavum Bullet Mollusc", 50000, "", "" );
            CapsuleMolluscs.Add( "Luteolum Capsule Mollusc", 50000, "", "" );
            CapsuleMolluscs.Add( "Lindigoticum Capsule Mollusc", 50000, "", "" );
            CollaredPod.Add( "Albidum Collared Pod", 50000, "", "" );
            CollaredPod.Add( "Lividum Collared Pod", 50000, "", "" );
            CollaredPod.Add( "Blatteum Collared Pod", 50000, "", "" );
            CollaredPod.Add( "Rubicundum Collared Pod", 50000, "", "" );
            StolonPod.Add( "Stolon Pod", 50000, "", "" );
            StolonTree.Add( "Stolon Tree", 50000, "", "" );
            AsterPods.Add( "Cereum Aster Pod", 50000, "", "" );
            AsterPods.Add( "Lindigoticum Aster Pod", 50000, "", "" );
            AsterPods.Add( "Prasinum Aster Pod", 50000, "", "" );
            AsterPods.Add( "Puniceum Aster Pod", 50000, "", "" );
            AsterPods.Add( "Rubellum Aster Pod", 50000, "", "" );
            ChalicePods.Add( "Albidum Chalice Pod", 50000, "", "" );
            ChalicePods.Add( "Ostrinum Chalice Pod", 50000, "", "" );
            PedunclePods.Add( "Candidum peduncle Pod", 50000, "", "" );
            PedunclePods.Add( "Caeruleum peduncle Pod", 50000, "", "" );
            PedunclePods.Add( "Gypseeum peduncle Pod", 50000, "", "" );
            PedunclePods.Add( "Purpureum peduncle Pod", 50000, "", "" );
            PedunclePods.Add( "Rufum peduncle Pod", 50000, "", "" );
            RhizomePods.Add( "Candidum Rhizome Pod", 50000, "", "" );
            RhizomePods.Add( "Cobalteum Rhizome Pod", 50000, "", "" );
            RhizomePods.Add( "Gypseeum Rhizome Pod", 50000, "", "" );
            RhizomePods.Add( "Purpureum Rhizome Pod", 50000, "", "" );
            RhizomePods.Add( "Rubeum Rhizome Pod", 50000, "", "" );
            QuadripartitePods.Add( "Albidum Quadripartite Pod", 50000, "", "" );
            QuadripartitePods.Add( "Caeruleum Quadripartite Pod", 50000, "", "" );
            QuadripartitePods.Add( "Viride Quadripartite Pod", 50000, "", "" );
            QuadripartitePods.Add( "Blatteum Quadripartite Pod", 50000, "", "" );
            OctahedralPods.Add( "Niveus Octahedral Pod", 50000, "", "" );
            OctahedralPods.Add( "Caeruleum Octahedral Pod", 50000, "", "" );
            OctahedralPods.Add( "Viride Octahedral Pod", 50000, "", "" );
            OctahedralPods.Add( "Rubeum Octahedral Pod", 50000, "", "" );
            AsterTrees.Add( "Cereum Aster Tree", 50000, "", "" );
            AsterTrees.Add( "Prasinum Aster Tree", 50000, "", "" );
            AsterTrees.Add( "Rubellum Aster Tree", 50000, "", "" );
            PeduncleTrees.Add( "Albidum Peduncle Tree", 50000, "", "" );
            PeduncleTrees.Add( "Caeruleum Peduncle Tree", 50000, "", "" );
            PeduncleTrees.Add( "Viride Peduncle Tree", 50000, "", "" );
            PeduncleTrees.Add( "Ostrinum Peduncle Tree", 50000, "", "" );
            PeduncleTrees.Add( "Rubellum Peduncle Tree", 50000, "", "" );
            GyreTrees.Add( "Viridis Gyre Tree", 50000, "", "" );
            GyrePods.Add( "", 50000, "", "" );
            VoidHearts.Add( "Chryseum Void Heart", 50000, "", "" );
            CalcitePlates.Add( "Luteolum Calcite Plates", 50000, "", "" );
            CalcitePlates.Add( "Lindigoticum Calcite Plates", 50000, "", "" );
            CalcitePlates.Add( "Viride Calcite Plates", 50000, "", "" );
            ThargoidBarnacles.Add( "Common Thargoid Barnacle", 50000, "These biological structures extract resources from a planet and convert them into meta-alloys, a key component in the creation of Thargoid ships and technologies.", "" );
            ThargoidBarnacles.Add( "Large Thargoid Barnacle", 50000, "These biological structures extract resources from a planet and convert them into meta-alloys, a key component in the creation of Thargoid ships and technologies.", "" );
            ThargoidBarnacles.Add( "Thargoid Barnacle Barbs", 50000, "These biological structures are typically found near Thargoid barnacles. Smaller ones contain rare elements, while larger ones contain meta-alloys.", "" );


            // Exobiology - Reverse Lookup
            Species.Add( "Aleoida Arcus", "Aleoida" );
            Species.Add( "Aleoida Coronamus", "Aleoida" );
            Species.Add( "Aleoida Gravis", "Aleoida" );
            Species.Add( "Aleoida Laminiae", "Aleoida" );
            Species.Add( "Aleoida Spica", "Aleoida" );
            Species.Add( "Amphora Plant", "Amphora" );
            Species.Add( "Blatteum Bioluminescent Anemone", "Anemone" );
            Species.Add( "Croceum Anemone", "Anemone" );
            Species.Add( "Luteolum Anemone", "Anemone" );
            Species.Add( "Prasinum Bioluminescent Anemone", "Anemone" );
            Species.Add( "Puniceum Anemone", "Anemone" );
            Species.Add( "Roseum Anemone", "Anemone" );
            Species.Add( "Roseum Bioluminescent Anemone", "Anemone" );
            Species.Add( "Rubeum Bioluminescent Anemone", "Anemone" );
            Species.Add( "Bacterium Acies", "Bacterium" );
            Species.Add( "Bacterium Alcyoneum", "Bacterium" );
            Species.Add( "Bacterium Aurasus", "Bacterium" );
            Species.Add( "Bacterium Bullaris", "Bacterium" );
            Species.Add( "Bacterium Cerbrus", "Bacterium" );
            Species.Add( "Bacterium Informem", "Bacterium" );
            Species.Add( "Bacterium Nebulus", "Bacterium" );
            Species.Add( "Bacterium Omentum", "Bacterium" );
            Species.Add( "Bacterium Scopulum", "Bacterium" );
            Species.Add( "Bacterium Tela", "Bacterium" );
            Species.Add( "Bacterium Verrata", "Bacterium" );
            Species.Add( "Bacterium Vesicula", "Bacterium" );
            Species.Add( "Bacterium Volu", "Bacterium" );
            Species.Add( "Bark Mounds", "Bark Mounds" );
            Species.Add( "Aureum Brain Tree", "Brain Tree" );
            Species.Add( "Gypseeum Brain Tree", "Brain Tree" );
            Species.Add( "Lindigoticum Brain Tree", "Brain Tree" );
            Species.Add( "Lividum Brain Tree", "Brain Tree" );
            Species.Add( "Ostrinum Brain Tree", "Brain Tree" );
            Species.Add( "Puniceum Brain Tree", "Brain Tree" );
            Species.Add( "Roseum Brain Tree", "Brain Tree" );
            Species.Add( "Viride Brain Tree", "Brain Tree" );
            Species.Add( "Cactoida Cortexum", "Cactoida" );
            Species.Add( "Cactoida Lapis", "Cactoida" );
            Species.Add( "Cactoida Peperatis", "Cactoida" );
            Species.Add( "Cactoida Pullulanta", "Cactoida" );
            Species.Add( "Cactoida Vermis", "Cactoida" );
            Species.Add( "Clypeus Lacrimam", "Clypeus" );
            Species.Add( "Clypeus Margaritus", "Clypeus" );
            Species.Add( "Clypeus Speculumi", "Clypeus" );
            Species.Add( "Concha Aureolas", "Concha" );
            Species.Add( "Concha Biconcavis", "Concha" );
            Species.Add( "Concha Labiata", "Concha" );
            Species.Add( "Concha Renibus", "Concha" );
            Species.Add( "Crystalline Shards", "Crystalline Shards" );
            Species.Add( "Electricae Pluma", "Electricae" );
            Species.Add( "Electricae Radialem", "Electricae" );
            Species.Add( "Fonticulua Campestris", "Fonticulua" );
            Species.Add( "Fonticulua Digitos", "Fonticulua" );
            Species.Add( "Fonticulua Fluctus", "Fonticulua" );
            Species.Add( "Fonticulua Lapida", "Fonticulua" );
            Species.Add( "Fonticulua Segmentatus", "Fonticulua" );
            Species.Add( "Fonticulua Upupam", "Fonticulua" );
            Species.Add( "Frutexa Acus", "Frutexa" );
            Species.Add( "Frutexa Collum", "Frutexa" );
            Species.Add( "Frutexa Fera", "Frutexa" );
            Species.Add( "Frutexa Flabellum", "Frutexa" );
            Species.Add( "Frutexa Flammasis", "Frutexa" );
            Species.Add( "Frutexa Metallicum", "Frutexa" );
            Species.Add( "Frutexa Sponsae", "Frutexa" );
            Species.Add( "Fumerola Aquatis", "Fumerola" );
            Species.Add( "Fumerola Carbosis", "Fumerola" );
            Species.Add( "Fumerola Extremus", "Fumerola" );
            Species.Add( "Fumerola Nitris", "Fumerola" );
            Species.Add( "Fungoida Bullarum", "Fungoida" );
            Species.Add( "Fungoida Gelata", "Fungoida" );
            Species.Add( "Fungoida Setisis", "Fungoida" );
            Species.Add( "Fungoida Stabitis", "Fungoida" );
            Species.Add( "Osseus Cornibus", "Osseus" );
            Species.Add( "Osseus Discus", "Osseus" );
            Species.Add( "Osseus Fractus", "Osseus" );
            Species.Add( "Osseus Pellebantus", "Osseus" );
            Species.Add( "Osseus Pumice", "Osseus" );
            Species.Add( "Osseus Spiralis", "Osseus" );
            Species.Add( "Recepta Conditivus", "Recepta" );
            Species.Add( "Recepta Deltahedronix", "Recepta" );
            Species.Add( "Recepta Umbrux", "Recepta" );
            Species.Add( "Albidum Sinuous Tubers", "Sinuous Tubers" );
            Species.Add( "Blatteum Sinuous Tubers", "Sinuous Tubers" );
            Species.Add( "Caeruleum Sinuous Tubers", "Sinuous Tubers" );
            Species.Add( "Lindigoticum Sinuous Tubers", "Sinuous Tubers" );
            Species.Add( "Prasinum Sinuous Tubers", "Sinuous Tubers" );
            Species.Add( "Roseum Sinuous Tubers", "Sinuous Tubers" );
            Species.Add( "Violaceum Sinuous Tubers", "Sinuous Tubers" );
            Species.Add( "Viride Sinuous Tubers", "Sinuous Tubers" );
            Species.Add( "Stratum Araneamus", "Stratum" );
            Species.Add( "Stratum Cucumisis", "Stratum" );
            Species.Add( "Stratum Excutitus", "Stratum" );
            Species.Add( "Stratum Frigus", "Stratum" );
            Species.Add( "Stratum Laminamus", "Stratum" );
            Species.Add( "Stratum Limaxus", "Stratum" );
            Species.Add( "Stratum Paleas", "Stratum" );
            Species.Add( "Stratum Tectonicas", "Stratum" );
            Species.Add( "Tubus Cavas", "Tubus" );
            Species.Add( "Tubus Compagibus", "Tubus" );
            Species.Add( "Tubus Conifer", "Tubus" );
            Species.Add( "Tubus Rosarium", "Tubus" );
            Species.Add( "Tubus Sororibus", "Tubus" );
            Species.Add( "Tussock Albata", "Tussock" );
            Species.Add( "Tussock Capillum", "Tussock" );
            Species.Add( "Tussock Caputus", "Tussock" );
            Species.Add( "Tussock Catena", "Tussock" );
            Species.Add( "Tussock Cultro", "Tussock" );
            Species.Add( "Tussock Divisa", "Tussock" );
            Species.Add( "Tussock Ignis", "Tussock" );
            Species.Add( "Tussock Pennata", "Tussock" );
            Species.Add( "Tussock Pennatis", "Tussock" );
            Species.Add( "Tussock Propagito", "Tussock" );
            Species.Add( "Tussock Serrati", "Tussock" );
            Species.Add( "Tussock Stigmasis", "Tussock" );
            Species.Add( "Tussock Triticum", "Tussock" );
            Species.Add( "Tussock Ventusa", "Tussock" );
            Species.Add( "Tussock Virgam", "Tussock" );

            // Other Organics - Reverse Lookup
            Species.Add( "Solid Mineral Spheres", "MineralSpheres" );
            Species.Add( "Lattice Mineral Spheres", "MineralSpheres" );
            Species.Add( "Prasinum Metallic Crystals", "MetallicCrystals" );
            Species.Add( "Purpureum Metallic Crystals", "MetallicCrystals" );
            Species.Add( "Rubeum Metallic Crystals", "MetallicCrystals" );
            Species.Add( "Flavum Metallic Crystals", "MetallicCrystals" );
            Species.Add( "Lindigoticum Silicate Crystals", "SilicateCrystals" );
            Species.Add( "Prasinum Silicate Crystals", "SilicateCrystals" );
            Species.Add( "Roseum Silicate Crystals", "SilicateCrystals" );
            Species.Add( "Purpureum Silicate Crystals", "SilicateCrystals" );
            Species.Add( "Albidium Silicate Crystals", "SilicateCrystals" );
            Species.Add( "Rubeum Silicate Crystals", "SilicateCrystals" );
            Species.Add( "Flavum Silicate Crystals", "SilicateCrystals" );
            Species.Add( "Lindigoticum Ice Crystals", "IceCrystals" );
            Species.Add( "Prasinum Ice Crystals", "IceCrystals" );
            Species.Add( "Roseum Ice Crystals", "IceCrystals" );
            Species.Add( "Purpureum Ice Crystals", "IceCrystals" );
            Species.Add( "Rubeum Ice Crystals", "IceCrystals" );
            Species.Add( "Albidium Ice Crystals", "IceCrystals" );
            Species.Add( "Flavum Ice Crystals", "IceCrystals" );
            Species.Add( "Luteolum Reel Mollusc", "ReelMolluscs" );
            Species.Add( "Lindigoticum Reel Mollusc", "ReelMolluscs" );
            Species.Add( "Viride Reel Mollusc", "ReelMolluscs" );
            Species.Add( "Niveum Globe Molluscs", "GlobeMolluscs" );
            Species.Add( "Albens Bell Mollusc", "BellMolluscs" );
            Species.Add( "Blatteum Bell Mollusc", "BellMolluscs" );
            Species.Add( "Lindigoticum Bell Mollusc", "BellMolluscs" );
            Species.Add( "Luteolum Umbrella Mollusc", "UmbrellaMolluscs" );
            Species.Add( "Lindigoticum Umbrella Mollusc", "UmbrellaMolluscs" );
            Species.Add( "Viride Umbrella Mollusc", "UmbrellaMolluscs" );
            Species.Add( "Albulum Gourd Mollusc", "GourdMollusc" );
            Species.Add( "Caeruleum Gourd Mollusc", "GourdMollusc" );
            Species.Add( "Viride Gourd Mollusc", "GourdMollusc" );
            Species.Add( "Phoeniceum Gourd Mollusc", "GourdMollusc" );
            Species.Add( "Purpureum Gourd Mollusc", "GourdMollusc" );
            Species.Add( "Rufum Gourd Mollusc", "GourdMollusc" );
            Species.Add( "Croceum Gourd Mollusc", "GourdMollusc" );
            Species.Add( "Caeruleum Torus Mollusc", "TorusMolluscs" );
            Species.Add( "Rubellum Torus Mollusc", "TorusMolluscs" );
            Species.Add( "Luteolum Bulb Mollusc", "BulbMolluscs" );
            Species.Add( "Lindigoticum Bulb Mollusc", "BulbMolluscs" );
            Species.Add( "Viride Bulb Mollusc", "BulbMolluscs" );
            Species.Add( "Luteolum Parasol Mollusc", "ParasolMolluscs" );
            Species.Add( "Lindigoticum Parasol Mollusc", "ParasolMolluscs" );
            Species.Add( "Viride Parasol Mollusc", "ParasolMolluscs" );
            Species.Add( "Albulum Squid Mollusc", "SquidMolluscs" );
            Species.Add( "Caeruleum Squid Mollusc", "SquidMolluscs" );
            Species.Add( "Puniceum Squid Mollusc", "SquidMolluscs" );
            Species.Add( "Rubeum Squid Mollusc", "SquidMolluscs" );
            Species.Add( "Roseum Squid Mollusc", "SquidMolluscs" );
            Species.Add( "Cereum Bullet Mollusc", "BulletMolluscs" );
            Species.Add( "Lividum Bullet Mollusc", "BulletMolluscs" );
            Species.Add( "Viride Bullet Mollusc", "BulletMolluscs" );
            Species.Add( "Rubeum Bullet Mollusc", "BulletMolluscs" );
            Species.Add( "Flavum Bullet Mollusc", "BulletMolluscs" );
            Species.Add( "Luteolum Capsule Mollusc", "CapsuleMolluscs" );
            Species.Add( "Lindigoticum Capsule Mollusc", "CapsuleMolluscs" );
            Species.Add( "Albidum Collared Pod", "CollaredPod" );
            Species.Add( "Lividum Collared Pod", "CollaredPod" );
            Species.Add( "Blatteum Collared Pod", "CollaredPod" );
            Species.Add( "Rubicundum Collared Pod", "CollaredPod" );
            Species.Add( "Stolon Pod", "StolonPod" );
            Species.Add( "Stolon Tree", "StolonTree" );
            Species.Add( "Cereum Aster Pod", "AsterPods" );
            Species.Add( "Lindigoticum Aster Pod", "AsterPods" );
            Species.Add( "Prasinum Aster Pod", "AsterPods" );
            Species.Add( "Puniceum Aster Pod", "AsterPods" );
            Species.Add( "Rubellum Aster Pod", "AsterPods" );
            Species.Add( "Albidum Chalice Pod", "ChalicePods" );
            Species.Add( "Ostrinum Chalice Pod", "ChalicePods" );
            Species.Add( "Candidum peduncle Pod", "PedunclePods" );
            Species.Add( "Caeruleum peduncle Pod", "PedunclePods" );
            Species.Add( "Gypseeum peduncle Pod", "PedunclePods" );
            Species.Add( "Purpureum peduncle Pod", "PedunclePods" );
            Species.Add( "Rufum peduncle Pod", "PedunclePods" );
            Species.Add( "Candidum Rhizome Pod", "RhizomePods" );
            Species.Add( "Cobalteum Rhizome Pod", "RhizomePods" );
            Species.Add( "Gypseeum Rhizome Pod", "RhizomePods" );
            Species.Add( "Purpureum Rhizome Pod", "RhizomePods" );
            Species.Add( "Rubeum Rhizome Pod", "RhizomePods" );
            Species.Add( "Albidum Quadripartite Pod", "QuadripartitePods" );
            Species.Add( "Caeruleum Quadripartite Pod", "QuadripartitePods" );
            Species.Add( "Viride Quadripartite Pod", "QuadripartitePods" );
            Species.Add( "Blatteum Quadripartite Pod", "QuadripartitePods" );
            Species.Add( "Niveus Octahedral Pod", "OctahedralPods" );
            Species.Add( "Caeruleum Octahedral Pod", "OctahedralPods" );
            Species.Add( "Viride Octahedral Pod", "OctahedralPods" );
            Species.Add( "Rubeum Octahedral Pod", "OctahedralPods" );
            Species.Add( "Cereum Aster Tree", "AsterTrees" );
            Species.Add( "Prasinum Aster Tree", "AsterTrees" );
            Species.Add( "Rubellum Aster Tree", "AsterTrees" );
            Species.Add( "Albidum Peduncle Tree", "PeduncleTrees" );
            Species.Add( "Caeruleum Peduncle Tree", "PeduncleTrees" );
            Species.Add( "Viride Peduncle Tree", "PeduncleTrees" );
            Species.Add( "Ostrinum Peduncle Tree", "PeduncleTrees" );
            Species.Add( "Rubellum Peduncle Tree", "PeduncleTrees" );
            Species.Add( "Viridis Gyre Tree", "GyreTrees" );
            Species.Add( "", "GyrePods" );
            Species.Add( "Chryseum Void Heart", "VoidHearts" );
            Species.Add( "Luteolum Calcite Plates", "CalcitePlates" );
            Species.Add( "Lindigoticum Calcite Plates", "CalcitePlates" );
            Species.Add( "Viride Calcite Plates", "CalcitePlates" );
            Species.Add( "Common Thargoid Barnacle", "ThargoidBarnacles" );
            Species.Add( "Large Thargoid Barnacle", "ThargoidBarnacles" );
            Species.Add( "Thargoid Barnacle Barbs", "ThargoidBarnacles" );

        }

        public static OrganicData LookupByVariant ( string localisedVariant )
        {
            bool found = false;
            string genus = "";
            string species = "";

            string[] variantSplit = localisedVariant.Split( '-' );
            if (variantSplit != null)
            {
                species = variantSplit[ 0 ];
                species = species.Trim();
            }

            found = Species.TryGetValue( species, out genus );

            if (found)
            {
                return GetData( genus, species );
            }

            return null;
        }

        public static OrganicData GetData ( string localisedGenus, string localisedSpecies )
        {
            OrganicData myData = new OrganicData();
            OrganicSpecies val = new OrganicSpecies();

            if ( localisedGenus == "Aleoida" )
            {
                myData.genus = Aleoida;
                Aleoida.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Amphora" )
            {
                myData.genus = Amphora;
                Amphora.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Anemone" )
            {
                myData.genus = Anemone;
                Anemone.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Bacterium" )
            {
                myData.genus = Bacterium;
                Bacterium.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Bark Mounds" )
            {
                myData.genus = BarkMounds;
                BarkMounds.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Brain Tree" )
            {
                myData.genus = BrainTree;
                BrainTree.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Cactoida" )
            {
                myData.genus = Cactoida;
                Cactoida.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Clypeus" )
            {
                myData.genus = Clypeus;
                Clypeus.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Concha" )
            {
                myData.genus = Concha;
                Concha.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Crystalline Shards" )
            {
                myData.genus = CrystallineShards;
                CrystallineShards.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Electricae" )
            {
                myData.genus = Electricae;
                Electricae.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Fonticulua" )
            {
                myData.genus = Fonticulua;
                Fonticulua.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Frutexa" )
            {
                myData.genus = Frutexa;
                Frutexa.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Fumerola" )
            {
                myData.genus = Fumerola;
                Fumerola.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Fungoida" )
            {
                myData.genus = Fungoida;
                Fungoida.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Osseus" )
            {
                myData.genus = Osseus;
                Osseus.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Recepta" )
            {
                myData.genus = Recepta;
                Recepta.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Sinuous Tuber" )
            {
                myData.genus = SinuousTuber;
                SinuousTuber.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Stratum" )
            {
                myData.genus = Stratum;
                Stratum.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Tubus" )
            {
                myData.genus = Tubus;
                Tubus.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Tussock" )
            {
                myData.genus = Tussock;
                Tussock.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Mineral Spheres")
            {
                myData.genus = MineralSpheres;
                MineralSpheres.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Metallic Crystals" )
            {
                myData.genus = MetallicCrystals;
                MetallicCrystals.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Silicate Crystals" )
            {
                myData.genus = SilicateCrystals;
                SilicateCrystals.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Ice Crystals" )
            {
                myData.genus = IceCrystals;
                IceCrystals.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Reel Molluscs" )
            {
                myData.genus = ReelMolluscs;
                ReelMolluscs.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Globe Molluscs" )
            {
                myData.genus = GlobeMolluscs;
                GlobeMolluscs.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Bell Molluscs" )
            {
                myData.genus = BellMolluscs;
                BellMolluscs.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Umbrella Molluscs" )
            {
                myData.genus = UmbrellaMolluscs;
                UmbrellaMolluscs.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Gourd Mollusc" )
            {
                myData.genus = GourdMollusc;
                GourdMollusc.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Torus Molluscs" )
            {
                myData.genus = TorusMolluscs;
                TorusMolluscs.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Bulb Molluscs" )
            {
                myData.genus = BulbMolluscs;
                BulbMolluscs.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Parasol Molluscs" )
            {
                myData.genus = ParasolMolluscs;
                ParasolMolluscs.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Squid Molluscs" )
            {
                myData.genus = SquidMolluscs;
                SquidMolluscs.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Bullet Molluscs" )
            {
                myData.genus = BulletMolluscs;
                BulletMolluscs.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Capsule Molluscs" )
            {
                myData.genus = CapsuleMolluscs;
                CapsuleMolluscs.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Collared Pod" )
            {
                myData.genus = CollaredPod;
                CollaredPod.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Stolon Pod" )
            {
                myData.genus = StolonPod;
                StolonPod.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Stolon Tree" )
            {
                myData.genus = StolonTree;
                StolonTree.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Aster Pods" )
            {
                myData.genus = AsterPods;
                AsterPods.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Chalice Pods" )
            {
                myData.genus = ChalicePods;
                ChalicePods.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Peduncle Pods" )
            {
                myData.genus = PedunclePods;
                PedunclePods.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Rhizome Pods" )
            {
                myData.genus = RhizomePods;
                RhizomePods.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Quadripartite Pods" )
            {
                myData.genus = QuadripartitePods;
                QuadripartitePods.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Octahedral Pods" )
            {
                myData.genus = OctahedralPods;
                OctahedralPods.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Aster Trees" )
            {
                myData.genus = AsterTrees;
                AsterTrees.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Peduncle Trees" )
            {
                myData.genus = PeduncleTrees;
                PeduncleTrees.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Gyre Trees" )
            {
                myData.genus = GyreTrees;
                GyreTrees.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Gyre Pods" )
            {
                myData.genus = GyrePods;
                GyrePods.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Void Hearts" )
            {
                myData.genus = VoidHearts;
                VoidHearts.species.TryGetValue( localisedSpecies, out val );
            }
            else if (localisedGenus == "Calcite Plates" )
            {
                myData.genus = CalcitePlates;
                CalcitePlates.species.TryGetValue( localisedSpecies, out val );
            }
            else if ( localisedGenus == "Thargoid Barnacles" )
            {
                myData.genus = ThargoidBarnacles;
                ThargoidBarnacles.species.TryGetValue( localisedSpecies, out val );
            }

            if ( val != null ) { myData.species = val; }
            else
            {
                myData.species.name = "Invalid";
                myData.species.value = 0;
                myData.species.description = "Invalid, species not found.";
            }

            return myData;
        }
    }
}

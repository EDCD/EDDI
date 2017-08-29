using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Material definitions
    /// </summary>
    public class Material
    {
        public static readonly List<Material> MATERIALS = new List<Material>();

        public string category { get; set; }

        public string EDName { get; private set; }

        public Rarity rarity { get; private set; }

        public string name { get; private set; }

        // Only for elements
        public string symbol { get; private set; }

        public decimal? goodpctbody { get; private set; }

        public decimal? greatpctbody { get; private set; }

        // Blueprints for the material; 
        public List<Blueprint> blueprints { get; set; }

        // Location of the material
        public string location { get; set; }

        private Material(string EDName, string category, string name, Rarity rarity, string symbol = null, decimal? goodpctbody = null, decimal? greatpctbody = null)
        {
            this.EDName = EDName;
            this.category = category;
            this.symbol = symbol;
            this.name = name;
            this.rarity = rarity;
            this.goodpctbody = goodpctbody;
            this.greatpctbody = greatpctbody;

            MATERIALS.Add(this);
        }

        public static readonly Material Carbon = new Material("carbon", "Element", "Carbonne", Rarity.VeryCommon, "C", 20.4M, 24.6M);
        public static readonly Material Iron = new Material("iron", "Element", "Fer", Rarity.VeryCommon, "Fe", 36.3M, 48.4M);
        public static readonly Material Nickel = new Material("nickel", "Element", "Nickel", Rarity.VeryCommon, "Ni", 26.6M, 31.9M);
        public static readonly Material Phosphorus = new Material("phosphorus", "Element", "Phosphore", Rarity.VeryCommon, "P", 15.1M, 18.1M);
        public static readonly Material Sulphur = new Material("sulphur", "Element", "Soufre", Rarity.VeryCommon, "S", 27.9M, 33.5M);

        public static readonly Material Chromium = new Material("chromium", "Element", "Chrome", Rarity.Common, "Cr", 13.8M, 16.6M);
        public static readonly Material Germanium = new Material("germanium", "Element", "Germanium", Rarity.Common, "Ge", 5.6M, 6.8M);
        public static readonly Material Manganese = new Material("manganese", "Element", "Manganèse", Rarity.Common, "Mn", 12.9M, 15.5M);
        public static readonly Material Vanadium = new Material("vanadium", "Element", "Vanadium", Rarity.Common, "V", 8.3M, 9.9M);
        public static readonly Material Zinc = new Material("zinc", "Element", "Zinc", Rarity.Common, "Zn", 9.1M, 10.9M);

        public static readonly Material Arsenic = new Material("arsenic", "Element", "Arsenic", Rarity.Standard, "As", 2.3M, 2.7M);
        public static readonly Material Niobium = new Material("niobium", "Element", "Niobium", Rarity.Standard, "Nb", 2.4M, 2.9M);
        public static readonly Material Selenium = new Material("selenium", "Element", "Sélénium", Rarity.Standard, "Se", 3.7M, 4.4M);
        public static readonly Material Tungsten = new Material("tungsten", "Element", "Tungstène", Rarity.Standard, "W", 2.0M, 2.3M);
        public static readonly Material Zirconium = new Material("zirconium", "Element", "Zirconium", Rarity.Standard, "Zr", 4.1M, 5.0M);

        public static readonly Material Cadmium = new Material("cadmium", "Element", "Cadmium", Rarity.Rare, "Cd", 2.8M, 3.3M);
        public static readonly Material Mercury = new Material("mercury", "Element", "Mercure", Rarity.Rare, "Hg", 1.6M, 1.9M);
        public static readonly Material Molybdenum = new Material("molybdenum", "Element", "Molybdène", Rarity.Rare, "Mo", 2.3M, 2.8M);
        public static readonly Material Tin = new Material("tin", "Element", "Étain", Rarity.Rare, "Sn", 2.4M, 2.9M);
        public static readonly Material Yttrium = new Material("yttrium", "Element", "Yttrium", Rarity.Rare, "Y", 2.1M, 2.5M);

        public static readonly Material Antimony = new Material("antimony", "Element", "Antimoine", Rarity.VeryRare, "Sb", 1.4M, 1.6M);
        public static readonly Material Polonium = new Material("polonium", "Element", "Polonium", Rarity.VeryRare, "Po", 1.1M, 1.3M);
        public static readonly Material Ruthenium = new Material("ruthenium", "Element", "Ruthénium", Rarity.VeryRare, "Ru", 2.2M, 2.6M);
        public static readonly Material Technetium = new Material("technetium", "Element", "Technétium", Rarity.VeryRare, "Tc", 1.3M, 1.5M);
        public static readonly Material Tellurium = new Material("tellurium", "Element", "Tellure", Rarity.VeryRare, "Te", 1.3M, 1.5M);

        public static readonly Material AnomalousBulkScanData = new Material("bulkscandata", "Data", "Fichier volumineux d'analyse anormal", Rarity.VeryCommon);
        public static readonly Material AtypicalDisruptedWakeEchoes = new Material("disruptedwakeechoes", "Data", "Échos de sillages perturbés atypiques", Rarity.VeryCommon);
        public static readonly Material DistortedShieldCycleRecordings = new Material("shieldcyclerecordings", "Data", "Enregistrements de cycles de bouclier déformés", Rarity.VeryCommon);
        public static readonly Material ExceptionalScrambledEmissionData = new Material("scrambledemissiondata", "Data", "Données d'émissions brouillées exceptionnelles", Rarity.VeryCommon);
        public static readonly Material SpecialisedLegacyFirmware = new Material("legacyfirmware", "Data", "Micrologiciel spécialisé périmé", Rarity.VeryCommon);
        public static readonly Material UnusualEncryptedFiles = new Material("encryptedfiles", "Data", "Fichiers cryptés inhabituels", Rarity.VeryCommon);

        public static readonly Material AnomalousFSDTelemetry = new Material("fsdtelemetry", "Data", "Télémétrie FSD anormale", Rarity.Common);
        public static readonly Material InconsistentShieldSoakAnalysis = new Material("shieldsoakanalysis", "Data", "Analyse d'absorption de bouclier incohérente", Rarity.Common);
        public static readonly Material IrregularEmissionData = new Material("archivedemissiondata", "Data", "Données d'émissions aberrantes", Rarity.Common);
        public static readonly Material ModifiedConsumerFirmware = new Material("consumerfirmware", "Data", "Micrologiciel consommateur modifié", Rarity.Common);
        public static readonly Material TaggedEncryptionCodes = new Material("encryptioncodes", "Data", "Clés de cryptage balisées", Rarity.Common);
        public static readonly Material UnidentifiedScanArchives = new Material("scanarchives", "Data", "Données d'analyse archivées non identifiées", Rarity.Common);

        public static readonly Material ClassifiedScanDatabanks = new Material("scandatabanks", "Data", "Banques de données d'analyse classifiées", Rarity.Standard);
        public static readonly Material CrackedIndustrialFirmware = new Material("industrialfirmware", "Data", "Micrologiciel industriel piraté", Rarity.Standard);
        public static readonly Material OpenSymmetricKeys = new Material("symmetrickeys", "Data", "clés symétriques ouvertes", Rarity.Standard);
        public static readonly Material StrangeWakeSolutions = new Material("wakesolutions", "Data", "Solutions de sillages anormales", Rarity.Standard);
        public static readonly Material UnexpectedEmissionData = new Material("emissiondata", "Data", "Données d'émissions inattendues", Rarity.Standard);
        public static readonly Material UntypicalShieldScans = new Material("shielddensityreports", "Data", "Analyses de bouclier atypique", Rarity.Standard);
        public static readonly Material UnknownShipSignature = new Material("unknownshipsignature", "Data", "Signature de vaisseau inconu", Rarity.Standard);
        public static readonly Material UnknownWakeScan = new Material("unknownwakedata", "Data", "Données de sillage inconu", Rarity.Standard);

        public static readonly Material AberrantShieldPatternAnalysis = new Material("shieldpatternanalysis", "Data", "Analyse de modèle de bouclier aberrante", Rarity.Rare);
        public static readonly Material AtypicalEncryptionArchives = new Material("encryptionarchives", "Data", "Archives cryptées atypiques", Rarity.Rare);
        public static readonly Material DecodedEmissionData = new Material("decodedemissiondata", "Data", "Données d'émissions décodées", Rarity.Rare);
        public static readonly Material DivergentScanData = new Material("encodedscandata", "Data", "Données d'analyse divergentes", Rarity.Rare);
        public static readonly Material EccentricHyperspaceTrajectories = new Material("hyperspacetrajectories", "Data", "Trajectoires hyperespace exentriques", Rarity.Rare);
        public static readonly Material SecurityFirmwarePatch = new Material("securityfirmware", "Data", "Mise à jour de micrologiciel de sécurité", Rarity.Rare);

        public static readonly Material AbnormalCompactEmissionData = new Material("compactemissionsdata", "Data", "Données d'émissions compactes anormales", Rarity.VeryRare);
        public static readonly Material AdaptiveEncryptorsCapture = new Material("adaptiveencryptors", "Data", "Capture de cryptage évolutif", Rarity.VeryRare);
        public static readonly Material ClassifiedScanFragment = new Material("classifiedscandata", "Data", "Données d'analyse classifiées parcellaires", Rarity.VeryRare);
        public static readonly Material DataminedWakeExceptions = new Material("dataminedwake", "Data", "Exploration de données de sillage anormales", Rarity.VeryRare);
        public static readonly Material ModifiedEmbeddedFirmware = new Material("embeddedfirmware", "Data", "logiciel intégré modifié", Rarity.VeryRare);
        public static readonly Material PeculiarShieldFrequencyData = new Material("shieldfrequencydata", "Data", "Données de Fréquences de bouclier Singulières", Rarity.VeryRare);

        public static readonly Material BasicConductors = new Material("basicconductors", "Manufacturé", "Conducteurs simples", Rarity.VeryCommon);
        public static readonly Material ChemicalStorageUnits = new Material("", "Manufacturé", "Unité de stockage chimique", Rarity.VeryCommon);
        public static readonly Material CompactComposites = new Material("", "Manufacturé", "Composites compacts", Rarity.VeryCommon);
        public static readonly Material CrystalShards = new Material("crystalshards", "Manufacturé", "Eclat de cristal", Rarity.VeryCommon);
        public static readonly Material GridResistors = new Material("gridresistors", "Manufacturé", "Résistances à grille", Rarity.VeryCommon);
        public static readonly Material HeatConductionWiring = new Material("heatconductionwiring", "Manufacturé", "Cablage de conduction thermique", Rarity.VeryCommon);
        public static readonly Material MechanicalScrap = new Material("mechanicalscrap", "Manufacturé", "Ferraille mécanique", Rarity.VeryCommon);
        public static readonly Material SalvagedAlloys = new Material("salvagedalloys", "Manufacturé", "Alliages récupérés", Rarity.VeryCommon);
        public static readonly Material TemperedAlloys = new Material("temperedalloys", "Manufacturé", "Alliages tempérés", Rarity.VeryCommon);
        public static readonly Material WornShieldEmitters = new Material("wornshieldemitters", "Manufacturé", "Émetteurs de bouclier usé", Rarity.VeryCommon);

        public static readonly Material ChemicalProcessors = new Material("chemicalprocessors", "Manufacturé", "Processeurs chimique", Rarity.Common);
        public static readonly Material ConductiveComponents = new Material("conductivecomponents", "Manufacturé", "Composants conducteurs", Rarity.Common);
        public static readonly Material FilamentComposites = new Material("", "Manufacturé", "Filament composite", Rarity.Common);
        public static readonly Material FlawedFocusCrystals = new Material("uncutfocuscrystals", "Manufacturé", "Cristaux de focalisation imparfaits", Rarity.Common);
        public static readonly Material GalvanisingAlloys = new Material("galvanisingalloys", "Manufacturé", "Alliages Galvaniques", Rarity.Common);
        public static readonly Material HeatDispersionPlate = new Material("heatdispersionplate", "Manufacturé", "Plaque de dissipation thermique", Rarity.Common);
        public static readonly Material HeatResistantCeramics = new Material("heatresistantceramics", "Manufacturé", "Céramiques résistantes à la chaleur", Rarity.Common);
        public static readonly Material HybridCapacitors = new Material("hybridcapacitors", "Manufacturé", "Condensateurs hybrides", Rarity.Common);
        public static readonly Material MechanicalEquipment = new Material("mechanicalequipment", "Manufacturé", "Équipements mécanique", Rarity.Common);
        public static readonly Material ShieldEmitters = new Material("shieldemitters", "Manufacturé", "Éméteurs de bouclier", Rarity.Common);

        public static readonly Material ChemicalDistillery = new Material("chemicaldistillery", "Manufacturé", "Distillerie chimique", Rarity.Standard);
        public static readonly Material ConductiveCeramics = new Material("conductiveceramics", "Manufacturé", "Conducteurs en céramiques", Rarity.Standard);
        public static readonly Material ElectrochemicalArrays = new Material("electrochemicalarrays", "Manufacturé", "Réseaux électrochimique", Rarity.Standard);
        public static readonly Material FocusCrystals = new Material("focuscrystals", "Manufacturé", "Cristaux de focalisation", Rarity.Standard);
        public static readonly Material HeatExchangers = new Material("heatexchangers", "Manufacturé", "Échangeurs de chaleur", Rarity.Standard);
        public static readonly Material HighDensityComposites = new Material("highdensitycomposites", "Manufacturé", "Composites à haute densité", Rarity.Standard);
        public static readonly Material MechanicalComponents = new Material("mechanicalcomponents", "Manufacturé", "Composants mécaniques", Rarity.Standard);
        public static readonly Material PhaseAlloys = new Material("phasealloys", "Manufacturé", "Alliage de phase", Rarity.Standard);
        public static readonly Material PrecipitatedAlloys = new Material("precipitatedalloys", "Manufacturé", "Alliage précipité", Rarity.Standard);
        public static readonly Material ShieldingSensors = new Material("shieldingsensors", "Manufacturé", "Capteurs de bouclier", Rarity.Standard);

        public static readonly Material ChemicalManipulators = new Material("chemicalmanipulators", "Manufacturé", "Manipulateur chimique", Rarity.Rare);
        public static readonly Material CompoundShielding = new Material("compoundshielding", "Manufacturé", "Protection composite", Rarity.Rare);
        public static readonly Material ConductivePolymers = new Material("conductivepolymers", "Manufacturé", "Conducteurs en polymères", Rarity.Rare);
        public static readonly Material ConfigurableComponents = new Material("configurablecomponents", "Manufacturé", "Composant paramétrables", Rarity.Rare);
        public static readonly Material HeatVanes = new Material("heatvanes", "Manufacturé", "Vannes thermiques", Rarity.Rare);
        public static readonly Material PolymerCapacitors = new Material("polymercapacitors", "Manufacturé", "Condensateurs en polymères", Rarity.Rare);
        public static readonly Material ProprietaryComposites = new Material("fedproprietarycomposites", "Manufacturé", "Composites brevetés", Rarity.Rare);
        public static readonly Material ProtoLightAlloys = new Material("protolightalloys", "Manufacturé", "Proto-alliages légers", Rarity.Rare);
        public static readonly Material RefinedFocusCrystals = new Material("refinedfocuscrystals", "Manufacturé", "Cristaux de focalisation raffinés", Rarity.Rare);
        public static readonly Material ThermicAlloys = new Material("thermicalloys", "Manufacturé", "Alliages thermiques", Rarity.Rare);

        public static readonly Material BiotechConductors = new Material("biotechconductors", "Manufacturé", "Conducteurs biotechniques", Rarity.VeryRare);
        public static readonly Material CoreDynamicsComposites = new Material("fedcorecomposites", "Manufacturé", "Composites Core Dynamics", Rarity.VeryRare);
        public static readonly Material ExquisiteFocusCrystals = new Material("exquisitefocuscrystals", "Manufacturé", "Cristaux de focalisation sans défaut", Rarity.VeryRare);
        public static readonly Material ImperialShielding = new Material("imperialshielding", "Manufacturé", "Protection impériale", Rarity.VeryRare);
        public static readonly Material ImprovisedComponents = new Material("improvisedcomponents", "Manufacturé", "Composants improvisés", Rarity.VeryRare);
        public static readonly Material MilitaryGradeAlloys = new Material("militarygradealloys", "Manufacturé", "Alliages militaires", Rarity.VeryRare);
        public static readonly Material MilitarySupercapacitors = new Material("militarysupercapacitors", "Manufacturé", "Supercondensateurs militaires", Rarity.VeryRare);
        public static readonly Material PharmaceuticalIsolators = new Material("pharmaceuticalisolators", "Manufacturé", "Isolants pharmaceutiques", Rarity.VeryRare);
        public static readonly Material ProtoHeatRadiators = new Material("protoheatradiators", "Manufacturé", "Proto-radiateurs", Rarity.VeryRare);
        public static readonly Material ProtoRadiolicAlloys = new Material("protoradiolicalloys", "Manufacturé", "Proto-alliages radiologiques", Rarity.VeryRare);
        public static readonly Material UnknownFragment = new Material("unknownenergysource", "Manufacturé", "Fragment inconu", Rarity.VeryRare);

        public static readonly Material AncientBiologicalData = new Material("ancientbiologicaldata", "Data", "Donnée biologique Antique", Rarity.Common);
        public static readonly Material AncientCulturalData = new Material("ancientculturaldata", "Data", "Données culturelles antique", Rarity.Common);
        public static readonly Material AncientHistoricalData = new Material("ancienthistoricaldata", "Data", "Données historiques antiques", Rarity.Common);
        public static readonly Material AncientLanguageData = new Material("ancientlanguagedata", "Data", "Données linguistiques antiques", Rarity.Common);
        public static readonly Material AncientTechnologicalData = new Material("ancienttechnologicaldata", "Data", "Données technologiques antiques", Rarity.Common);

        public static Material FromName(string from)
        {
            if (string.IsNullOrEmpty(from))
            {
                return null;
            }

            string tidiedFrom = from.ToLowerInvariant().Replace(" ", "").Replace("cadmiun", "cadmium");
            Material result = MATERIALS.FirstOrDefault(v => v.name.ToLowerInvariant().Replace(" ", "") == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown material name " + from);
                result = new Material(tidiedFrom, "Unknown", from, Rarity.Unknown);
            }
            return result;
        }

        public static Material FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }

            string tidiedFrom = from.ToLowerInvariant().Replace("cadmiun", "cadmium");
            Material result = MATERIALS.FirstOrDefault(v => v.EDName.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown material ED name " + from);
                result = new Material(from, "Unknown", tidiedFrom, Rarity.Unknown);
            }
            return result;
        }

        public static Material FromSymbol(string from)
        {
            Material result = MATERIALS.FirstOrDefault(v => v.symbol == from);
            if (result == null)
            {
                Logging.Report("Unknown material symbol " + from);
            }
            return result;
        }
    }
}

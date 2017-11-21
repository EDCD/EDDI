using System.Collections.Generic;
using System.Linq;
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

        public static readonly Material Carbon = new Material("carbon", "Element", "Carbon", Rarity.VeryCommon, "C", 20.4M, 24.6M);
        public static readonly Material Iron = new Material("iron", "Element", "Iron", Rarity.VeryCommon, "Fe", 36.3M, 48.4M);
        public static readonly Material Nickel = new Material("nickel", "Element", "Nickel", Rarity.VeryCommon, "Ni", 26.6M, 31.9M);
        public static readonly Material Phosphorus = new Material("phosphorus", "Element", "Phosphorus", Rarity.VeryCommon, "P", 15.1M, 18.1M);
        public static readonly Material Sulphur = new Material("sulphur", "Element", "Sulphur", Rarity.VeryCommon, "S", 27.9M, 33.5M);

        public static readonly Material Chromium = new Material("chromium", "Element", "Chromium", Rarity.Common, "Cr", 13.8M, 16.6M);
        public static readonly Material Germanium = new Material("germanium", "Element", "Germanium", Rarity.Common, "Ge", 5.6M, 6.8M);
        public static readonly Material Manganese = new Material("manganese", "Element", "Manganese", Rarity.Common, "Mn", 12.9M, 15.5M);
        public static readonly Material Vanadium = new Material("vanadium", "Element", "Vanadium", Rarity.Common, "V", 8.3M, 9.9M);
        public static readonly Material Zinc = new Material("zinc", "Element", "Zinc", Rarity.Common, "Zn", 9.1M, 10.9M);

        public static readonly Material Arsenic = new Material("arsenic", "Element", "Arsenic", Rarity.Standard, "As", 2.3M, 2.7M);
        public static readonly Material Niobium = new Material("niobium", "Element", "Niobium", Rarity.Standard, "Nb", 2.4M, 2.9M);
        public static readonly Material Selenium = new Material("selenium", "Element", "Selenium", Rarity.Standard, "Se", 3.7M, 4.4M);
        public static readonly Material Tungsten = new Material("tungsten", "Element", "Tungsten", Rarity.Standard, "W", 2.0M, 2.3M);
        public static readonly Material Zirconium = new Material("zirconium", "Element", "Zirconium", Rarity.Standard, "Zr", 4.1M, 5.0M);

        public static readonly Material Cadmium = new Material("cadmium", "Element", "Cadmium", Rarity.Rare, "Cd", 2.8M, 3.3M);
        public static readonly Material Mercury = new Material("mercury", "Element", "Mercury", Rarity.Rare, "Hg", 1.6M, 1.9M);
        public static readonly Material Molybdenum = new Material("molybdenum", "Element", "Molybdenum", Rarity.Rare, "Mo", 2.3M, 2.8M);
        public static readonly Material Tin = new Material("tin", "Element", "Tin", Rarity.Rare, "Sn", 2.4M, 2.9M);
        public static readonly Material Yttrium = new Material("yttrium", "Element", "Yttrium", Rarity.Rare, "Y", 2.1M, 2.5M);

        public static readonly Material Antimony = new Material("antimony", "Element", "Antimony", Rarity.VeryRare, "Sb", 1.4M, 1.6M);
        public static readonly Material Polonium = new Material("polonium", "Element", "Polonium", Rarity.VeryRare, "Po", 1.1M, 1.3M);
        public static readonly Material Ruthenium = new Material("ruthenium", "Element", "Ruthenium", Rarity.VeryRare, "Ru", 2.2M, 2.6M);
        public static readonly Material Technetium = new Material("technetium", "Element", "Technetium", Rarity.VeryRare, "Tc", 1.3M, 1.5M);
        public static readonly Material Tellurium = new Material("tellurium", "Element", "Tellurium", Rarity.VeryRare, "Te", 1.3M, 1.5M);

        public static readonly Material AnomalousBulkScanData = new Material("bulkscandata", "Data", "Anomalous Bulk Scan Data", Rarity.VeryCommon);
        public static readonly Material AtypicalDisruptedWakeEchoes = new Material("disruptedwakeechoes", "Data", "Atypical Disrupted Wake Echoes", Rarity.VeryCommon);
        public static readonly Material DistortedShieldCycleRecordings = new Material("shieldcyclerecordings", "Data", "Distorted Shield Cycle Recordings", Rarity.VeryCommon);
        public static readonly Material ExceptionalScrambledEmissionData = new Material("scrambledemissiondata", "Data", "Exceptional Scrambled Emission Data", Rarity.VeryCommon);
        public static readonly Material SpecialisedLegacyFirmware = new Material("legacyfirmware", "Data", "Specialised Legacy Firmware", Rarity.VeryCommon);
        public static readonly Material UnusualEncryptedFiles = new Material("encryptedfiles", "Data", "Unusual Encrypted Files", Rarity.VeryCommon);

        public static readonly Material AnomalousFSDTelemetry = new Material("fsdtelemetry", "Data", "Anomalous FSD Telemetry", Rarity.Common);
        public static readonly Material InconsistentShieldSoakAnalysis = new Material("shieldsoakanalysis", "Data", "Inconsistent Shield Soak Analysis", Rarity.Common);
        public static readonly Material IrregularEmissionData = new Material("archivedemissiondata", "Data", "Irregular Emission Data", Rarity.Common);
        public static readonly Material ModifiedConsumerFirmware = new Material("consumerfirmware", "Data", "Modified Consumer Firmware", Rarity.Common);
        public static readonly Material TaggedEncryptionCodes = new Material("encryptioncodes", "Data", "Tagged Encryption Codes", Rarity.Common);
        public static readonly Material UnidentifiedScanArchives = new Material("scanarchives", "Data", "Unidentified Scan Archives", Rarity.Common);

        public static readonly Material ClassifiedScanDatabanks = new Material("scandatabanks", "Data", "Classified Scan Databanks", Rarity.Standard);
        public static readonly Material CrackedIndustrialFirmware = new Material("industrialfirmware", "Data", "Cracked Industrial Firmware", Rarity.Standard);
        public static readonly Material OpenSymmetricKeys = new Material("symmetrickeys", "Data", "Open Symmetric Keys", Rarity.Standard);
        public static readonly Material StrangeWakeSolutions = new Material("wakesolutions", "Data", "Strange Wake Solutions", Rarity.Standard);
        public static readonly Material UnexpectedEmissionData = new Material("emissiondata", "Data", "Unexpected Emission Data", Rarity.Standard);
        public static readonly Material UntypicalShieldScans = new Material("shielddensityreports", "Data", "Untypical Shield Scans", Rarity.Standard);
        public static readonly Material UnknownShipSignature = new Material("unknownshipsignature", "Data", "Thargoid Ship Signature", Rarity.Standard);
        public static readonly Material UnknownWakeScan = new Material("unknownwakedata", "Data", "Thargoid Wake Data", Rarity.Standard);

        public static readonly Material AberrantShieldPatternAnalysis = new Material("shieldpatternanalysis", "Data", "Aberrant Shield Pattern Analysis", Rarity.Rare);
        public static readonly Material AtypicalEncryptionArchives = new Material("encryptionarchives", "Data", "Atypical Encryption Archives", Rarity.Rare);
        public static readonly Material DecodedEmissionData = new Material("decodedemissiondata", "Data", "Decoded Emission Data", Rarity.Rare);
        public static readonly Material DivergentScanData = new Material("encodedscandata", "Data", "Divergent Scan Data", Rarity.Rare);
        public static readonly Material EccentricHyperspaceTrajectories = new Material("hyperspacetrajectories", "Data", "Eccentric Hyperspace Trajectories", Rarity.Rare);
        public static readonly Material SecurityFirmwarePatch = new Material("securityfirmware", "Data", "Security Firmware Patch", Rarity.Rare);

        public static readonly Material AbnormalCompactEmissionData = new Material("compactemissionsdata", "Data", "Abnormal Compact Emission Data", Rarity.VeryRare);
        public static readonly Material AdaptiveEncryptorsCapture = new Material("adaptiveencryptors", "Data", "Adaptive Encryptors Capture", Rarity.VeryRare);
        public static readonly Material ClassifiedScanFragment = new Material("classifiedscandata", "Data", "Classified Scan Fragment", Rarity.VeryRare);
        public static readonly Material DataminedWakeExceptions = new Material("dataminedwake", "Data", "Datamined Wake Exceptions", Rarity.VeryRare);
        public static readonly Material ModifiedEmbeddedFirmware = new Material("embeddedfirmware", "Data", "Modified Embedded Firmware", Rarity.VeryRare);
        public static readonly Material PeculiarShieldFrequencyData = new Material("shieldfrequencydata", "Data", "Peculiar Shield Frequency Data", Rarity.VeryRare);

        public static readonly Material BasicConductors = new Material("basicconductors", "Manufactured", "Basic Conductors", Rarity.VeryCommon);
        public static readonly Material ChemicalStorageUnits = new Material("chemicalstorageunits", "Manufactured", "Chemical Storage Units", Rarity.VeryCommon);
        public static readonly Material CompactComposites = new Material("compactcomposites", "Manufactured", "Compact Composites", Rarity.VeryCommon);
        public static readonly Material CrystalShards = new Material("crystalshards", "Manufactured", "Crystal Shards", Rarity.VeryCommon);
        public static readonly Material GridResistors = new Material("gridresistors", "Manufactured", "Grid Resistors", Rarity.VeryCommon);
        public static readonly Material HeatConductionWiring = new Material("heatconductionwiring", "Manufactured", "Heat Conduction Wiring", Rarity.VeryCommon);
        public static readonly Material MechanicalScrap = new Material("mechanicalscrap", "Manufactured", "Mechanical Scrap", Rarity.VeryCommon);
        public static readonly Material SalvagedAlloys = new Material("salvagedalloys", "Manufactured", "Salvaged Alloys", Rarity.VeryCommon);
        public static readonly Material TemperedAlloys = new Material("temperedalloys", "Manufactured", "Tempered Alloys", Rarity.VeryCommon);
        public static readonly Material WornShieldEmitters = new Material("wornshieldemitters", "Manufactured", "Worn Shield Emitters", Rarity.VeryCommon);

        public static readonly Material ChemicalProcessors = new Material("chemicalprocessors", "Manufactured", "Chemical Processors", Rarity.Common);
        public static readonly Material ConductiveComponents = new Material("conductivecomponents", "Manufactured", "Conductive Components", Rarity.Common);
        public static readonly Material FilamentComposites = new Material("filamentcomposites", "Manufactured", "Filament Composites", Rarity.Common);
        public static readonly Material FlawedFocusCrystals = new Material("uncutfocuscrystals", "Manufactured", "Flawed Focus Crystals", Rarity.Common);
        public static readonly Material GalvanisingAlloys = new Material("galvanisingalloys", "Manufactured", "Galvanising Alloys", Rarity.Common);
        public static readonly Material HeatDispersionPlate = new Material("heatdispersionplate", "Manufactured", "Heat Dispersion Plate", Rarity.Common);
        public static readonly Material HeatResistantCeramics = new Material("heatresistantceramics", "Manufactured", "Heat Resistant Ceramics", Rarity.Common);
        public static readonly Material HybridCapacitors = new Material("hybridcapacitors", "Manufactured", "Hybrid Capacitors", Rarity.Common);
        public static readonly Material MechanicalEquipment = new Material("mechanicalequipment", "Manufactured", "Mechanical Equipment", Rarity.Common);
        public static readonly Material ShieldEmitters = new Material("shieldemitters", "Manufactured", "Shield Emitters", Rarity.Common);

        public static readonly Material ChemicalDistillery = new Material("chemicaldistillery", "Manufactured", "Chemical Distillery", Rarity.Standard);
        public static readonly Material ConductiveCeramics = new Material("conductiveceramics", "Manufactured", "Conductive Ceramics", Rarity.Standard);
        public static readonly Material ElectrochemicalArrays = new Material("electrochemicalarrays", "Manufactured", "Electrochemical Arrays", Rarity.Standard);
        public static readonly Material FocusCrystals = new Material("focuscrystals", "Manufactured", "Focus Crystals", Rarity.Standard);
        public static readonly Material HeatExchangers = new Material("heatexchangers", "Manufactured", "Heat Exchangers", Rarity.Standard);
        public static readonly Material HighDensityComposites = new Material("highdensitycomposites", "Manufactured", "High Density Composites", Rarity.Standard);
        public static readonly Material MechanicalComponents = new Material("mechanicalcomponents", "Manufactured", "Mechanical Components", Rarity.Standard);
        public static readonly Material PhaseAlloys = new Material("phasealloys", "Manufactured", "Phase Alloys", Rarity.Standard);
        public static readonly Material PrecipitatedAlloys = new Material("precipitatedalloys", "Manufactured", "Precipitated Alloys", Rarity.Standard);
        public static readonly Material ShieldingSensors = new Material("shieldingsensors", "Manufactured", "Shielding Sensors", Rarity.Standard);

        public static readonly Material ChemicalManipulators = new Material("chemicalmanipulators", "Manufactured", "Chemical Manipulators", Rarity.Rare);
        public static readonly Material CompoundShielding = new Material("compoundshielding", "Manufactured", "Compound Shielding", Rarity.Rare);
        public static readonly Material ConductivePolymers = new Material("conductivepolymers", "Manufactured", "Conductive Polymers", Rarity.Rare);
        public static readonly Material ConfigurableComponents = new Material("configurablecomponents", "Manufactured", "Configurable Components", Rarity.Rare);
        public static readonly Material HeatVanes = new Material("heatvanes", "Manufactured", "Heat Vanes", Rarity.Rare);
        public static readonly Material PolymerCapacitors = new Material("polymercapacitors", "Manufactured", "Polymer Capacitors", Rarity.Rare);
        public static readonly Material ProprietaryComposites = new Material("fedproprietarycomposites", "Manufactured", "Proprietary Composites", Rarity.Rare);
        public static readonly Material ProtoLightAlloys = new Material("protolightalloys", "Manufactured", "Proto Light Alloys", Rarity.Rare);
        public static readonly Material RefinedFocusCrystals = new Material("refinedfocuscrystals", "Manufactured", "Refined Focus Crystals", Rarity.Rare);
        public static readonly Material ThermicAlloys = new Material("thermicalloys", "Manufactured", "Thermic Alloys", Rarity.Rare);

        public static readonly Material BiotechConductors = new Material("biotechconductors", "Manufactured", "Biotech Conductors", Rarity.VeryRare);
        public static readonly Material CoreDynamicsComposites = new Material("fedcorecomposites", "Manufactured", "Core Dynamics Composites", Rarity.VeryRare);
        public static readonly Material ExquisiteFocusCrystals = new Material("exquisitefocuscrystals", "Manufactured", "Exquisite Focus Crystals", Rarity.VeryRare);
        public static readonly Material ImperialShielding = new Material("imperialshielding", "Manufactured", "Imperial Shielding", Rarity.VeryRare);
        public static readonly Material ImprovisedComponents = new Material("improvisedcomponents", "Manufactured", "Improvised Components", Rarity.VeryRare);
        public static readonly Material MilitaryGradeAlloys = new Material("militarygradealloys", "Manufactured", "Military Grade Alloys", Rarity.VeryRare);
        public static readonly Material MilitarySupercapacitors = new Material("militarysupercapacitors", "Manufactured", "Military Supercapacitors", Rarity.VeryRare);
        public static readonly Material PharmaceuticalIsolators = new Material("pharmaceuticalisolators", "Manufactured", "Pharmaceutical Isolators", Rarity.VeryRare);
        public static readonly Material ProtoHeatRadiators = new Material("protoheatradiators", "Manufactured", "Proto Heat Radiators", Rarity.VeryRare);
        public static readonly Material ProtoRadiolicAlloys = new Material("protoradiolicalloys", "Manufactured", "Proto Radiolic Alloys", Rarity.VeryRare);

        public static readonly Material AncientBiologicalData = new Material("ancientbiologicaldata", "Data", "Ancient Biological Data", Rarity.Common);
        public static readonly Material AncientCulturalData = new Material("ancientculturaldata", "Data", "Ancient Cultural Data", Rarity.Common);
        public static readonly Material AncientHistoricalData = new Material("ancienthistoricaldata", "Data", "Ancient Historical Data", Rarity.Common);
        public static readonly Material AncientLanguageData = new Material("ancientlanguagedata", "Data", "Ancient Language Data", Rarity.Common);
        public static readonly Material AncientTechnologicalData = new Material("ancienttechnologicaldata", "Data", "Ancient Technological Data", Rarity.Common);

        public static readonly Material UnknownCarapace = new Material("unknowncarapace", "Manufactured", "Thargoid Carapace", Rarity.Common);
        public static readonly Material UnknownEnergyCell = new Material("unknownenergycell", "Manufactured", "Thargoid Energy Cell", Rarity.Standard);
        public static readonly Material UnknownTechnologyComponents = new Material("unknowntechnologycomponents", "Manufactured", "Thargoid Technology Components", Rarity.Rare);
        public static readonly Material UnknownEnergySource = new Material("unknownenergysource", "Manufactured", "Sensor Fragment", Rarity.VeryRare);
        public static readonly Material UnknownOrganicCircuitry = new Material("unknownorganiccircuitry", "Manufactured", "Thargoid Organic Circuitry", Rarity.VeryRare);

        public static readonly Material Tg_StructuralData = new Material("tg_structuraldata", "Data", "Thargoid Structural Data", Rarity.Common);
        public static readonly Material Tg_CompositionData = new Material("tg_compositiondata", "Data", "Thargoid Material Composition Data", Rarity.Standard);
        public static readonly Material Tg_ResidueData = new Material("tg_residuedata", "Data", "Thargoid Residue Data", Rarity.Rare);
        public static readonly Material Tg_ShipFlightData = new Material("tg_shipflightdata", "Data", "Ship Flight Data", Rarity.Rare);
        public static readonly Material Tg_ShipSystemData = new Material("tg_shipsystemdata", "Data", "Ship System Data", Rarity.Rare);

        public static readonly Material Tg_WreckageComponents = new Material("tg_wreckagecomponents", "Manufactured", "Wreckage Components", Rarity.Standard);
        public static readonly Material Tg_BiomechanicalConduits = new Material("tg_biomechanicalconduits", "Manufactured", "Bio-Mechanical Conduits", Rarity.Standard);
        public static readonly Material Tg_WeaponParts = new Material("tg_weaponparts", "Manufactured", "Weapon Parts", Rarity.Rare);
        public static readonly Material Tg_PropulsionElement = new Material("tg_propulsionelement", "Manufactured", "Propulsion Elements", Rarity.VeryRare);

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
                // Fall back to the edname if searching by name fails.
                result = MATERIALS.FirstOrDefault(v => v.EDName.ToLowerInvariant() == tidiedFrom);
            }
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

        public static bool DeprecatedMaterials(string name)
        {
            // These material names have been replaced / are no longer in use. Listed for reference so that they won't be retained by the material monitor.
            if (name == null)
            {
                return false;
            }
            List<string> deprecatedMaterialsList = new List<string>
            {
                "Thargoid Residue Data Analysis",
                "Unknown Ship Signature",
                "Unknown Wake Data",
                "Unknown Fragment",
            };

            return deprecatedMaterialsList.Contains(name.Trim());
        }
    }
}

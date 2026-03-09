using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    using static MaterialCategory;
    using static Rarity;

    public class Material : ResourceBasedLocalizedEDName<Material>
    {
        static Material()
        {
            resourceManager = Properties.Materials.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new Material(edname, MaterialCategory.Unknown, Rarity.Unknown);

            surfaceElements = AllOfThem
                .Where(m => m.Category == Element)
                .Where(m => m.greatpctbody != null)
                .ToList().ToImmutableHashSet();

            jumponiumElements = new[] { Carbon, Germanium, Vanadium, Cadmium, Niobium, Arsenic, Yttrium, Polonium }
                .ToImmutableHashSet();
        }

        #region Elements

        // The below percentages are calculated by finding the top percentage ever found on a body for a given material, 
        // then taking 90% of that value as a definition for a `great` percentage and by taking 75% of that value as a `good` percentage.
        // The data used to generate the current values are located at https://docs.google.com/spreadsheets/d/1UcgHvnDF-lYYPD7PNkT_g7B1dr9lBBkVPPlL51ITrz4

        // Grade 1, Very Common
        public static readonly Material Carbon = new("carbon", Element, VeryCommon, "C", 24.6M, 29.5M);
        public static readonly Material Iron = new("iron", Element, VeryCommon, "Fe", 36.4M, 43.6M);
        public static readonly Material Lead = new("lead", Element, VeryCommon, "Pb", null, null);
        public static readonly Material Nickel = new("nickel", Element, VeryCommon, "Ni", 27.6M, 33.2M);
        public static readonly Material Phosphorus = new("phosphorus", Element, VeryCommon, "P", 15.7M, 18.9M);
        public static readonly Material Rhenium = new("rhenium", Element, VeryCommon, "Re", null, null);
        public static readonly Material Sulphur = new("sulphur", Element, VeryCommon, "S", 29.2M, 35.1M);

        // Grade 2, Common
        public static readonly Material Arsenic = new("arsenic", Element, Common, "As", 2.3M, 2.7M);
        public static readonly Material Chromium = new("chromium", Element, Common, "Cr", 14.0M, 16.8M);
        public static readonly Material Germanium = new("germanium", Element, Common, "Ge", 4.9M, 5.9M);
        public static readonly Material Manganese = new("manganese", Element, Common, "Mn", 13.0M, 15.6M);
        public static readonly Material Vanadium = new("vanadium", Element, Common, "V", 8.3M, 10M);
        public static readonly Material Zinc = new("zinc", Element, Common, "Zn", 9.2M, 11.1M);
        public static readonly Material Zirconium = new("zirconium", Element, Common, "Zr", 4.2M, 5.0M);

        // Grade 3, Standard
        public static readonly Material Boron = new("boron", Element, Standard, "B", null, null);
        public static readonly Material Cadmium = new("cadmium", Element, Standard, "Cd", 2.8M, 3.4M);
        public static readonly Material Mercury = new("mercury", Element, Standard, "Hg", 1.6M, 1.9M);
        public static readonly Material Molybdenum = new("molybdenum", Element, Standard, "Mo", 2.4M, 2.9M);
        public static readonly Material Niobium = new("niobium", Element, Standard, "Nb", 2.5M, 3.0M);
        public static readonly Material Tin = new("tin", Element, Standard, "Sn", 2.4M, 2.9M);
        public static readonly Material Tungsten = new("tungsten", Element, Standard, "W", 2.0M, 2.4M);

        // Grade 4, Rare
        public static readonly Material Ruthenium = new("ruthenium", Element, Rare, "Ru", 2.2M, 2.7M);
        public static readonly Material Selenium = new("selenium", Element, Rare, "Se", 4.5M, 5.4M);
        public static readonly Material Technetium = new("technetium", Element, Rare, "Tc", 1.3M, 1.6M);
        public static readonly Material Tellurium = new("tellurium", Element, Rare, "Te", 1.3M, 1.6M);
        public static readonly Material Yttrium = new("yttrium", Element, Rare, "Y", 2.2M, 2.6M);
        public static readonly Material Antimony = new("antimony", Element, Rare, "Sb", 1.4M, 1.6M); // Rare per Material Trader UI and FDev spreadsheet but very rare per in-game right panel description.
        public static readonly Material Polonium = new("polonium", Element, Rare, "Po", 1.1M, 1.3M); // Rare per Material Trader UI and FDev spreadsheet but very rare per in-game right panel description.

        #endregion

        #region Data

        // Grade 1, Very Common
        public static readonly Material AnomalousBulkScanData = new("bulkscandata", Data, VeryCommon);
        public static readonly Material AtypicalDisruptedWakeEchoes = new("disruptedwakeechoes", Data, VeryCommon);
        public static readonly Material DistortedShieldCycleRecordings = new("shieldcyclerecordings", Data, VeryCommon);
        public static readonly Material ExceptionalScrambledEmissionData = new("scrambledemissiondata", Data, VeryCommon);
        public static readonly Material SpecialisedLegacyFirmware = new("legacyfirmware", Data, VeryCommon);
        public static readonly Material UnusualEncryptedFiles = new("encryptedfiles", Data, VeryCommon);

        // Grade 2, Common
        public static readonly Material AnomalousFSDTelemetry = new("fsdtelemetry", Data, Common);
        public static readonly Material InconsistentShieldSoakAnalysis = new("shieldsoakanalysis", Data, Common);
        public static readonly Material IrregularEmissionData = new("archivedemissiondata", Data, Common);
        public static readonly Material ModifiedConsumerFirmware = new("consumerfirmware", Data, Common);
        public static readonly Material TaggedEncryptionCodes = new("encryptioncodes", Data, Common);
        public static readonly Material UnidentifiedScanArchives = new("scanarchives", Data, Common);
        // Grade 2 Xeno
        public static readonly Material Tg_StructuralData = new("tg_structuraldata", Data, Common);

        // Grade 3, Standard
        public static readonly Material ClassifiedScanDatabanks = new("scandatabanks", Data, Standard);
        public static readonly Material CrackedIndustrialFirmware = new("industrialfirmware", Data, Standard);
        public static readonly Material OpenSymmetricKeys = new("symmetrickeys", Data, Standard);
        public static readonly Material StrangeWakeSolutions = new("wakesolutions", Data, Standard);
        public static readonly Material UnexpectedEmissionData = new("emissiondata", Data, Standard);
        public static readonly Material UntypicalShieldScans = new("shielddensityreports", Data, Standard);
        // Grade 3 Xeno
        public static readonly Material Tg_CompositionData = new("tg_compositiondata", Data, Standard);
        public static readonly Material Tg_InterdictionData = new("tg_interdictiondata", Encoded, Standard);
        public static readonly Material UnknownShipSignature = new("unknownshipsignature", Data, Standard);

        // Grade 4, Rare
        public static readonly Material AberrantShieldPatternAnalysis = new("shieldpatternanalysis", Data, Rare);
        public static readonly Material AtypicalEncryptionArchives = new("encryptionarchives", Data, Rare);
        public static readonly Material DecodedEmissionData = new("decodedemissiondata", Data, Rare);
        public static readonly Material DivergentScanData = new("encodedscandata", Data, Rare);
        public static readonly Material EccentricHyperspaceTrajectories = new("hyperspacetrajectories", Data, Rare);
        public static readonly Material SecurityFirmwarePatch = new("securityfirmware", Data, Rare);
        // Grade 4 Xeno
        public static readonly Material AncientHistoricalData = new("ancienthistoricaldata", Data, Rare); // Rare per FDev spreadsheet but very common per in-game right panel description.
        public static readonly Material AncientCulturalData = new("ancientculturaldata", Data, Rare); // Rare per FDev spreadsheet but common per in-game right panel description.
        public static readonly Material AncientBiologicalData = new("ancientbiologicaldata", Data, Rare); // Rare per FDev spreadsheet but standard per in-game right panel description.
        public static readonly Material AncientLanguageData = new("ancientlanguagedata", Data, Rare);
        public static readonly Material AncientTechnologicalData = new("ancienttechnologicaldata", Data, Rare);
        public static readonly Material Tg_ShipFlightData = new("tg_shipflightdata", Data, Rare);
        public static readonly Material Tg_ShipSystemData = new("tg_shipsystemsdata", Data, Rare);
        public static readonly Material Tg_ResidueData = new("tg_residuedata", Data, Rare);
        public static readonly Material UnknownWakeScan = new("unknownwakedata", Data, Rare);
        public static readonly Material Guardian_WeaponBlueprint = new("guardian_weaponblueprint", Data, Rare);
        public static readonly Material Guardian_ModuleBlueprint = new("guardian_moduleblueprint", Data, Rare);

        // Grade 5, Very Rare
        public static readonly Material AbnormalCompactEmissionData = new("compactemissionsdata", Data, VeryRare);
        public static readonly Material AdaptiveEncryptorsCapture = new("adaptiveencryptors", Data, VeryRare);
        public static readonly Material ClassifiedScanFragment = new("classifiedscandata", Data, VeryRare);
        public static readonly Material DataminedWakeExceptions = new("dataminedwake", Data, VeryRare);
        public static readonly Material ModifiedEmbeddedFirmware = new("embeddedfirmware", Data, VeryRare);
        public static readonly Material PeculiarShieldFrequencyData = new("shieldfrequencydata", Data, VeryRare);
        // Grade 5 Xeno
        public static readonly Material Guardian_VesselBlueprint = new("guardian_vesselblueprint", Data, VeryRare);
        public static readonly Material Tg_StructuralData02 = new( "tg_structuraldata02", Data, VeryRare );

        // Unknown rarity
        public static readonly Material SearchRescueVoucher = new( "searchrescuevoucher", Data, Rarity.Unknown );

        #endregion

        #region Manufactured

        // Grade 1, Very Common
        public static readonly Material BasicConductors = new("basicconductors", Manufactured, VeryCommon);
        public static readonly Material ChemicalStorageUnits = new("chemicalstorageunits", Manufactured, VeryCommon);
        public static readonly Material CompactComposites = new("compactcomposites", Manufactured, VeryCommon);
        public static readonly Material CrystalShards = new("crystalshards", Manufactured, VeryCommon);
        public static readonly Material GridResistors = new("gridresistors", Manufactured, VeryCommon);
        public static readonly Material HeatConductionWiring = new("heatconductionwiring", Manufactured, VeryCommon);
        public static readonly Material MechanicalScrap = new("mechanicalscrap", Manufactured, VeryCommon);
        public static readonly Material SalvagedAlloys = new("salvagedalloys", Manufactured, VeryCommon);
        public static readonly Material TemperedAlloys = new("temperedalloys", Manufactured, VeryCommon);
        public static readonly Material WornShieldEmitters = new("wornshieldemitters", Manufactured, VeryCommon);
        // Grade 1 Xeno
        public static readonly Material GuardianPowerCell = new("guardian_powercell", Manufactured, VeryCommon);
        public static readonly Material GuardianSentinelWreckageComponents = new("guardian_sentinel_wreckagecomponents", Manufactured, VeryCommon);
        public static readonly Material HardenedSurfaceFragments = new( "tg_abrasion03", Manufactured, VeryCommon );

        // Grade 2, Common
        public static readonly Material ChemicalProcessors = new("chemicalprocessors", Manufactured, Common);
        public static readonly Material ConductiveComponents = new("conductivecomponents", Manufactured, Common);
        public static readonly Material FilamentComposites = new("filamentcomposites", Manufactured, Common);
        public static readonly Material FlawedFocusCrystals = new("uncutfocuscrystals", Manufactured, Common);
        public static readonly Material GalvanisingAlloys = new("galvanisingalloys", Manufactured, Common);
        public static readonly Material HeatDispersionPlate = new("heatdispersionplate", Manufactured, Common);
        public static readonly Material HeatResistantCeramics = new("heatresistantceramics", Manufactured, Common);
        public static readonly Material HybridCapacitors = new("hybridcapacitors", Manufactured, Common);
        public static readonly Material MechanicalEquipment = new("mechanicalequipment", Manufactured, Common);
        public static readonly Material ShieldEmitters = new("shieldemitters", Manufactured, Common);
        // Grade 2 Xeno
        public static readonly Material CausticShard = new("tg_causticshard", Manufactured, Common);
        public static readonly Material GuardianPowerConduit = new("guardian_powerconduit", Manufactured, Common);
        public static readonly Material TacticalCoreChip = new("unknowncorechip", Manufactured, Common);
        public static readonly Material ThargoidCarapace = new("unknowncarapace", Manufactured, Common);

        // Grade 3, Standard
        public static readonly Material ChemicalDistillery = new("chemicaldistillery", Manufactured, Standard);
        public static readonly Material ConductiveCeramics = new("conductiveceramics", Manufactured, Standard);
        public static readonly Material ElectrochemicalArrays = new("electrochemicalarrays", Manufactured, Standard);
        public static readonly Material FocusCrystals = new("focuscrystals", Manufactured, Standard);
        public static readonly Material HeatExchangers = new("heatexchangers", Manufactured, Standard);
        public static readonly Material HighDensityComposites = new("highdensitycomposites", Manufactured, Standard);
        public static readonly Material MechanicalComponents = new("mechanicalcomponents", Manufactured, Standard);
        public static readonly Material PhaseAlloys = new("phasealloys", Manufactured, Standard);
        public static readonly Material PrecipitatedAlloys = new("precipitatedalloys", Manufactured, Standard);
        public static readonly Material ShieldingSensors = new("shieldingsensors", Manufactured, Standard);
        // Grade 3 Xeno
        public static readonly Material CorrosiveMechanisms = new("tg_causticgeneratorparts", Manufactured, Standard);
        public static readonly Material GuardianSentinelWeaponParts = new("guardian_sentinel_weaponparts", Manufactured, Standard);
        public static readonly Material GuardianTechnologyComponent = new("guardian_techcomponent", Manufactured, Standard);
        public static readonly Material MassiveEnergySurgeAnalytics = new("tg_shutdowndata", Manufactured, Standard);
        public static readonly Material PhasingMembraneResidue = new( "tg_abrasion02", Manufactured, Standard );
        public static readonly Material ThargoidBiomechanicalConduits = new("tg_biomechanicalconduits", Manufactured, Standard);
        public static readonly Material ThargoidEnergyCell = new("unknownenergycell", Manufactured, Standard);
        public static readonly Material ThargoidWreckageComponents = new("tg_wreckagecomponents", Manufactured, Standard);

        // Grade 4, Rare
        public static readonly Material ChemicalManipulators = new("chemicalmanipulators", Manufactured, Rare);
        public static readonly Material CompoundShielding = new("compoundshielding", Manufactured, Rare);
        public static readonly Material ConductivePolymers = new("conductivepolymers", Manufactured, Rare);
        public static readonly Material ConfigurableComponents = new("configurablecomponents", Manufactured, Rare);
        public static readonly Material HeatVanes = new("heatvanes", Manufactured, Rare);
        public static readonly Material PolymerCapacitors = new("polymercapacitors", Manufactured, Rare);
        public static readonly Material ProprietaryComposites = new("fedproprietarycomposites", Manufactured, Rare);
        public static readonly Material ProtoLightAlloys = new("protolightalloys", Manufactured, Rare);
        public static readonly Material RefinedFocusCrystals = new("refinedfocuscrystals", Manufactured, Rare);
        public static readonly Material ThermicAlloys = new("thermicalloys", Manufactured, Rare);
        // Grade 4 Xeno
        public static readonly Material CausticCrystal = new("tg_causticcrystal", Manufactured, Rare);
        public static readonly Material ThargoidTechnologyComponents = new("unknowntechnologycomponents", Manufactured, Rare);
        public static readonly Material ThargoidWeaponParts = new("tg_weaponparts", Manufactured, Rare);

        // Grade 5, Very Rare
        public static readonly Material BiotechConductors = new("biotechconductors", Manufactured, VeryRare);
        public static readonly Material CoreDynamicsComposites = new("fedcorecomposites", Manufactured, VeryRare);
        public static readonly Material ExquisiteFocusCrystals = new("exquisitefocuscrystals", Manufactured, VeryRare);
        public static readonly Material ImperialShielding = new("imperialshielding", Manufactured, VeryRare);
        public static readonly Material ImprovisedComponents = new("improvisedcomponents", Manufactured, VeryRare);
        public static readonly Material MilitaryGradeAlloys = new("militarygradealloys", Manufactured, VeryRare);
        public static readonly Material MilitarySupercapacitors = new("militarysupercapacitors", Manufactured, VeryRare);
        public static readonly Material PharmaceuticalIsolators = new("pharmaceuticalisolators", Manufactured, VeryRare);
        public static readonly Material ProtoHeatRadiators = new("protoheatradiators", Manufactured, VeryRare);
        public static readonly Material ProtoRadiolicAlloys = new("protoradiolicalloys", Manufactured, VeryRare);
        // Grade 5 Xeno
        public static readonly Material HeatExposureSpecimen = new( "tg_abrasion01", Manufactured, VeryRare );
        public static readonly Material PropulsionElement = new("tg_propulsionelement", Manufactured, VeryRare);
        public static readonly Material SensorFragment = new("unknownenergysource", Manufactured, VeryRare);
        public static readonly Material ThargoidOrganicCircuitry = new("unknownorganiccircuitry", Manufactured, VeryRare);

        // Unknown Rarity

        #endregion

        [PublicAPI]
        public string category => Category.localizedName;

        [JsonProperty("category")]
        public MaterialCategory Category { get; }

        [PublicAPI]
        public string rarity => Rarity.localizedName;

        [JsonProperty("rarity")]
        public Rarity Rarity { get; }

        // Only for elements

        [PublicAPI]
        public string symbol { get; }

        [PublicAPI]
        public decimal? goodpctbody { get; }

        [PublicAPI]
        public decimal? greatpctbody { get; }

        // The body with the greatest percent concentration (for the MaterialDetails() function).

        [PublicAPI]
        public string bodyname { get; set; }

        [PublicAPI]
        public string bodyshortname { get; set; }

        public static ImmutableHashSet<Material> surfaceElements { get; } // Elements which are available at a planetary surface and not just in space
        public static ImmutableHashSet<Material> jumponiumElements { get; } // Elements which are used for FSD injection

        // Blueprints for the material; 
        [PublicAPI]
        public List<Blueprint> blueprints => Blueprint.AllOfThem.ToList().Distinct()
            .Where(bp => bp.materials?.Any(ma => ma.edname == this.edname) ?? false)
            .ToList();

        // Location of the material (localized)

        [PublicAPI]
        public string location => Properties.MaterialLocations.ResourceManager.GetString(edname);

        // dummy used to ensure that the static constructor has run
        public Material() : this("", MaterialCategory.Unknown, VeryCommon)
        { }

        private Material(string edname, MaterialCategory category, Rarity rarity, string symbol = null, decimal? goodpctbody = null, decimal? greatpctbody = null) : base(edname, edname)
        {
            this.Category = category;
            this.symbol = symbol;
            this.Rarity = rarity;
            this.goodpctbody = goodpctbody; // top 25% from top value ever recorded
            this.greatpctbody = greatpctbody; // top 10% from top value ever recorded
        }

        public static Material FromSymbol(string from)
        {
            Material result = AllOfThem.FirstOrDefault(v => v.symbol == from);
            if (result == null)
            {
                Logging.Info("Unknown material symbol " + from);
            }
            return result;
        }

        public static Body highestPercentBody(string materialEdName, IList<Body> bodies)
        {
            Body bestBody = null;
            decimal percentage = 0;
            foreach (Body body in bodies.ToList().OrderBy(b => b.distance))
            {
                foreach (MaterialPresence materialPresence in body.materials)
                {
                    if (materialPresence?.definition?.edname == materialEdName)
                    {
                        if (materialPresence?.percentage > percentage)
                        {
                            percentage = materialPresence.percentage;
                            bestBody = body;
                        }
                    }
                }
            }
            return bestBody;
        }

        public static bool EDNameExists(string edName)
        {
            if (edName == null) { return false; }
            return AllOfThem.Any(v => string.Equals(v.edname, titiedEDName(edName), StringComparison.InvariantCultureIgnoreCase));
        }

        private static string titiedEDName(string edName)
        {
            return edName?.ToLowerInvariant().Replace("$", "").Replace(";", "").Replace("_name", "");
        }
    }
}

﻿using Newtonsoft.Json;
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

            // The below percentages are calculated by finding the top percentage ever found on a body for a given material, 
            // then taking 90% of that value as a definition for a `great` percentage and by taking 75% of that value as a `good` percentage.
            // The data used to generate the current values are located at https://docs.google.com/spreadsheets/d/1UcgHvnDF-lYYPD7PNkT_g7B1dr9lBBkVPPlL51ITrz4

            // Grade 1, Very Common
            var Carbon = new Material("carbon", Element, VeryCommon, "C", 24.6M, 29.5M);
            var Iron = new Material("iron", Element, VeryCommon, "Fe", 36.4M, 43.6M);
            var Lead = new Material("lead", Element, VeryCommon, "Pb", null, null);
            var Nickel = new Material("nickel", Element, VeryCommon, "Ni", 27.6M, 33.2M);
            var Phosphorus = new Material("phosphorus", Element, VeryCommon, "P", 15.7M, 18.9M);
            var Rhenium = new Material("rhenium", Element, VeryCommon, "Re", null, null);
            var Sulphur = new Material("sulphur", Element, VeryCommon, "S", 29.2M, 35.1M);

            // Grade 2, Common
            var Arsenic = new Material("arsenic", Element, Common, "As", 2.3M, 2.7M);
            var Chromium = new Material("chromium", Element, Common, "Cr", 14.0M, 16.8M);
            var Germanium = new Material("germanium", Element, Common, "Ge", 4.9M, 5.9M);
            var Manganese = new Material("manganese", Element, Common, "Mn", 13.0M, 15.6M);
            var Vanadium = new Material("vanadium", Element, Common, "V", 8.3M, 10M);
            var Zinc = new Material("zinc", Element, Common, "Zn", 9.2M, 11.1M);
            var Zirconium = new Material("zirconium", Element, Common, "Zr", 4.2M, 5.0M);

            // Grade 3, Standard
            var Boron = new Material("boron", Element, Standard, "B", null, null);
            var Cadmium = new Material("cadmium", Element, Standard, "Cd", 2.8M, 3.4M);
            var Mercury = new Material("mercury", Element, Standard, "Hg", 1.6M, 1.9M);
            var Molybdenum = new Material("molybdenum", Element, Standard, "Mo", 2.4M, 2.9M);
            var Niobium = new Material("niobium", Element, Standard, "Nb", 2.5M, 3.0M);
            var Tin = new Material("tin", Element, Standard, "Sn", 2.4M, 2.9M);
            var Tungsten = new Material("tungsten", Element, Standard, "W", 2.0M, 2.4M);

            // Grade 4, Rare
            var Ruthenium = new Material("ruthenium", Element, Rare, "Ru", 2.2M, 2.7M);
            var Selenium = new Material("selenium", Element, Rare, "Se", 4.5M, 5.4M);
            var Technetium = new Material("technetium", Element, Rare, "Tc", 1.3M, 1.6M);
            var Tellurium = new Material("tellurium", Element, Rare, "Te", 1.3M, 1.6M);
            var Yttrium = new Material("yttrium", Element, Rare, "Y", 2.2M, 2.6M);
            var Antimony = new Material("antimony", Element, Rare, "Sb", 1.4M, 1.6M); // Rare per Material Trader UI and FDev spreadsheet but very rare per in-game right panel description.
            var Polonium = new Material("polonium", Element, Rare, "Po", 1.1M, 1.3M); // Rare per Material Trader UI and FDev spreadsheet but very rare per in-game right panel description.

            ///<summary>Data</summary>

            // Grade 1, Very Common
            var AnomalousBulkScanData = new Material("bulkscandata", Data, VeryCommon);
            var AtypicalDisruptedWakeEchoes = new Material("disruptedwakeechoes", Data, VeryCommon);
            var DistortedShieldCycleRecordings = new Material("shieldcyclerecordings", Data, VeryCommon);
            var ExceptionalScrambledEmissionData = new Material("scrambledemissiondata", Data, VeryCommon);
            var SpecialisedLegacyFirmware = new Material("legacyfirmware", Data, VeryCommon);
            var UnusualEncryptedFiles = new Material("encryptedfiles", Data, VeryCommon);

            // Grade 2, Common
            var AnomalousFSDTelemetry = new Material("fsdtelemetry", Data, Common);
            var InconsistentShieldSoakAnalysis = new Material("shieldsoakanalysis", Data, Common);
            var IrregularEmissionData = new Material("archivedemissiondata", Data, Common);
            var ModifiedConsumerFirmware = new Material("consumerfirmware", Data, Common);
            var TaggedEncryptionCodes = new Material("encryptioncodes", Data, Common);
            var UnidentifiedScanArchives = new Material("scanarchives", Data, Common);
            // Grade 2 Xeno
            var Tg_StructuralData = new Material("tg_structuraldata", Data, Common);

            // Grade 3, Standard
            var ClassifiedScanDatabanks = new Material("scandatabanks", Data, Standard);
            var CrackedIndustrialFirmware = new Material("industrialfirmware", Data, Standard);
            var OpenSymmetricKeys = new Material("symmetrickeys", Data, Standard);
            var StrangeWakeSolutions = new Material("wakesolutions", Data, Standard);
            var UnexpectedEmissionData = new Material("emissiondata", Data, Standard);
            var UntypicalShieldScans = new Material("shielddensityreports", Data, Standard);
            // Grade 3 Xeno
            var Tg_CompositionData = new Material("tg_compositiondata", Data, Standard);
            var UnknownShipSignature = new Material("unknownshipsignature", Data, Standard);

            // Grade 4, Rare
            var AberrantShieldPatternAnalysis = new Material("shieldpatternanalysis", Data, Rare);
            var AtypicalEncryptionArchives = new Material("encryptionarchives", Data, Rare);
            var DecodedEmissionData = new Material("decodedemissiondata", Data, Rare);
            var DivergentScanData = new Material("encodedscandata", Data, Rare);
            var EccentricHyperspaceTrajectories = new Material("hyperspacetrajectories", Data, Rare);
            var SecurityFirmwarePatch = new Material("securityfirmware", Data, Rare);
            // Grade 4 Xeno
            var AncientHistoricalData = new Material("ancienthistoricaldata", Data, Rare); // Rare per FDev spreadsheet but very common per in-game right panel description.
            var AncientCulturalData = new Material("ancientculturaldata", Data, Common); // Rare per FDev spreadsheet but common per in-game right panel description.
            var AncientBiologicalData = new Material("ancientbiologicaldata", Data, Standard); // Rare per FDev spreadsheet but standard per in-game right panel description.
            var AncientLanguageData = new Material("ancientlanguagedata", Data, Rare);
            var AncientTechnologicalData = new Material("ancienttechnologicaldata", Data, Rare);
            var Tg_ShipFlightData = new Material("tg_shipflightdata", Data, Rare);
            var Tg_ShipSystemData = new Material("tg_shipsystemsdata", Data, Rare);
            var Tg_ResidueData = new Material("tg_residuedata", Data, Rare);
            var UnknownWakeScan = new Material("unknownwakedata", Data, Rare);
            var Guardian_WeaponBlueprint = new Material("guardian_weaponblueprint", Data, Rare);
            var Guardian_ModuleBlueprint = new Material("guardian_moduleblueprint", Data, Rare);

            // Grade 5, Very Rare
            var AbnormalCompactEmissionData = new Material("compactemissionsdata", Data, VeryRare);
            var AdaptiveEncryptorsCapture = new Material("adaptiveencryptors", Data, VeryRare);
            var ClassifiedScanFragment = new Material("classifiedscandata", Data, VeryRare);
            var DataminedWakeExceptions = new Material("dataminedwake", Data, VeryRare);
            var ModifiedEmbeddedFirmware = new Material("embeddedfirmware", Data, VeryRare);
            var PeculiarShieldFrequencyData = new Material("shieldfrequencydata", Data, VeryRare);
            // Grade 5 Xeno
            var Guardian_VesselBlueprint = new Material("guardian_vesselblueprint", Data, VeryRare);

            ///<summary>Manufactured</summary>

            // Grade 1, Very Common
            var BasicConductors = new Material("basicconductors", Manufactured, VeryCommon);
            var ChemicalStorageUnits = new Material("chemicalstorageunits", Manufactured, VeryCommon);
            var CompactComposites = new Material("compactcomposites", Manufactured, VeryCommon);
            var CrystalShards = new Material("crystalshards", Manufactured, VeryCommon);
            var GridResistors = new Material("gridresistors", Manufactured, VeryCommon);
            var HeatConductionWiring = new Material("heatconductionwiring", Manufactured, VeryCommon);
            var MechanicalScrap = new Material("mechanicalscrap", Manufactured, VeryCommon);
            var SalvagedAlloys = new Material("salvagedalloys", Manufactured, VeryCommon);
            var TemperedAlloys = new Material("temperedalloys", Manufactured, VeryCommon);
            var WornShieldEmitters = new Material("wornshieldemitters", Manufactured, VeryCommon);
            // Grade 1 Xeno
            var Guardian_PowerCell = new Material("guardian_powercell", Manufactured, VeryCommon);
            var Guardian_Sentinel_WreckageComponents = new Material("guardian_sentinel_wreckagecomponents", Manufactured, VeryCommon);

            // Grade 2, Common
            var ChemicalProcessors = new Material("chemicalprocessors", Manufactured, Common);
            var ConductiveComponents = new Material("conductivecomponents", Manufactured, Common);
            var FilamentComposites = new Material("filamentcomposites", Manufactured, Common);
            var FlawedFocusCrystals = new Material("uncutfocuscrystals", Manufactured, Common);
            var GalvanisingAlloys = new Material("galvanisingalloys", Manufactured, Common);
            var HeatDispersionPlate = new Material("heatdispersionplate", Manufactured, Common);
            var HeatResistantCeramics = new Material("heatresistantceramics", Manufactured, Common);
            var HybridCapacitors = new Material("hybridcapacitors", Manufactured, Common);
            var MechanicalEquipment = new Material("mechanicalequipment", Manufactured, Common);
            var ShieldEmitters = new Material("shieldemitters", Manufactured, Common);
            // Grade 2 Xeno
            var Guardian_PowerConduit = new Material("guardian_powerconduit", Manufactured, Common);
            var UnknownCarapace = new Material("unknowncarapace", Manufactured, Common);

            // Grade 3, Standard
            var ChemicalDistillery = new Material("chemicaldistillery", Manufactured, Standard);
            var ConductiveCeramics = new Material("conductiveceramics", Manufactured, Standard);
            var ElectrochemicalArrays = new Material("electrochemicalarrays", Manufactured, Standard);
            var FocusCrystals = new Material("focuscrystals", Manufactured, Standard);
            var HeatExchangers = new Material("heatexchangers", Manufactured, Standard);
            var HighDensityComposites = new Material("highdensitycomposites", Manufactured, Standard);
            var MechanicalComponents = new Material("mechanicalcomponents", Manufactured, Standard);
            var PhaseAlloys = new Material("phasealloys", Manufactured, Standard);
            var PrecipitatedAlloys = new Material("precipitatedalloys", Manufactured, Standard);
            var ShieldingSensors = new Material("shieldingsensors", Manufactured, Standard);
            // Grade 3 Xeno
            var Tg_BiomechanicalConduits = new Material("tg_biomechanicalconduits", Manufactured, Standard);
            var Guardian_Sentinel_WeaponParts = new Material("guardian_sentinel_weaponparts", Manufactured, Standard);
            var Guardian_TechComponent = new Material("guardian_techcomponent", Manufactured, Standard);
            var UnknownEnergyCell = new Material("unknownenergycell", Manufactured, Standard);
            var Tg_WreckageComponents = new Material("tg_wreckagecomponents", Manufactured, Standard);

            // Grade 4, Rare
            var ChemicalManipulators = new Material("chemicalmanipulators", Manufactured, Rare);
            var CompoundShielding = new Material("compoundshielding", Manufactured, Rare);
            var ConductivePolymers = new Material("conductivepolymers", Manufactured, Rare);
            var ConfigurableComponents = new Material("configurablecomponents", Manufactured, Rare);
            var HeatVanes = new Material("heatvanes", Manufactured, Rare);
            var PolymerCapacitors = new Material("polymercapacitors", Manufactured, Rare);
            var ProprietaryComposites = new Material("fedproprietarycomposites", Manufactured, Rare);
            var ProtoLightAlloys = new Material("protolightalloys", Manufactured, Rare);
            var RefinedFocusCrystals = new Material("refinedfocuscrystals", Manufactured, Rare);
            var ThermicAlloys = new Material("thermicalloys", Manufactured, Rare);
            // Grade 4 Xeno
            var UnknownTechnologyComponents = new Material("unknowntechnologycomponents", Manufactured, Rare);
            var Tg_WeaponParts = new Material("tg_weaponparts", Manufactured, Rare);

            // Grade 5, Very Rare
            var BiotechConductors = new Material("biotechconductors", Manufactured, VeryRare);
            var CoreDynamicsComposites = new Material("fedcorecomposites", Manufactured, VeryRare);
            var ExquisiteFocusCrystals = new Material("exquisitefocuscrystals", Manufactured, VeryRare);
            var ImperialShielding = new Material("imperialshielding", Manufactured, VeryRare);
            var ImprovisedComponents = new Material("improvisedcomponents", Manufactured, VeryRare);
            var MilitaryGradeAlloys = new Material("militarygradealloys", Manufactured, VeryRare);
            var MilitarySupercapacitors = new Material("militarysupercapacitors", Manufactured, VeryRare);
            var PharmaceuticalIsolators = new Material("pharmaceuticalisolators", Manufactured, VeryRare);
            var ProtoHeatRadiators = new Material("protoheatradiators", Manufactured, VeryRare);
            var ProtoRadiolicAlloys = new Material("protoradiolicalloys", Manufactured, VeryRare);
            // Grade 5 Xeno
            var UnknownEnergySource = new Material("unknownenergysource", Manufactured, VeryRare);
            var UnknownOrganicCircuitry = new Material("unknownorganiccircuitry", Manufactured, VeryRare);
            var Tg_PropulsionElement = new Material("tg_propulsionelement", Manufactured, VeryRare);

            surfaceElements = ImmutableHashSet.ToImmutableHashSet(AllOfThem
                .Where(m => m.Category == Element)
                .Where(m => m.greatpctbody != null).ToList());

            jumponiumElements = ImmutableHashSet.Create(
                Carbon,
                Germanium,
                Vanadium,
                Cadmium,
                Niobium,
                Arsenic,
                Yttrium,
                Polonium
            );
        }

        public string category => Category.localizedName;

        [JsonProperty("category")]
        public MaterialCategory Category { get; }
        public Rarity rarity { get; }

        // Only for elements
        public string symbol { get; }

        public decimal? goodpctbody { get; }
        public decimal? greatpctbody { get; }

        // The body with the greatest percent concentration (for the MaterialDetails() function).
        public string bodyname { get; set; }
        public string bodyshortname { get; set; }

        public static ImmutableHashSet<Material> surfaceElements { get; } // Elements which are available at a planetary surface and not just in space
        public static ImmutableHashSet<Material> jumponiumElements { get; } // Elements which are used for FSD injection

        // Blueprints for the material; 
        public List<Blueprint> blueprints => Blueprint.AllOfThem
            .Where(bp => bp.materials?.Where(ma => ma.edname == this.edname).Count() > 0)
            .ToList();

        // Location of the material (localized)
        public string location => Properties.MaterialLocations.ResourceManager.GetString(edname);

        // dummy used to ensure that the static constructor has run
        public Material() : this("", MaterialCategory.Unknown, VeryCommon)
        { }

        private Material(string edname, MaterialCategory category, Rarity rarity, string symbol = null, decimal? goodpctbody = null, decimal? greatpctbody = null) : base(edname, edname)
        {
            this.Category = category;
            this.symbol = symbol;
            this.rarity = rarity;
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
                        if (materialPresence.percentage > percentage)
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

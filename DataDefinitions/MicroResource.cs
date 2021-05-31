using Newtonsoft.Json;
using System;
using System.Linq;

namespace EddiDataDefinitions
{
    /// <summary> Atmosphere Class </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class MicroResource : ResourceBasedLocalizedEDName<MicroResource>
    {
        static MicroResource()
        {
            resourceManager = Properties.MicroResources.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new MicroResource(edname, MicroResourceCategory.Unknown);
        }

        // Components
        public static readonly MicroResource Aerogel = new MicroResource("Aerogel", MicroResourceCategory.Components, 128961524);
        public static readonly MicroResource CarbonFibrePlating = new MicroResource("CarbonFibrePlating", MicroResourceCategory.Components, 128961526);
        public static readonly MicroResource ChemicalCatalyst = new MicroResource("ChemicalCatalyst", MicroResourceCategory.Components, 128961527);
        public static readonly MicroResource ChemicalSuperbase = new MicroResource("ChemicalSuperbase", MicroResourceCategory.Components, 128961528);
        public static readonly MicroResource CircuitBoard = new MicroResource("CircuitBoard", MicroResourceCategory.Components, 128961529);
        public static readonly MicroResource CircuitSwitch = new MicroResource("CircuitSwitch", MicroResourceCategory.Components, 128961530);
        public static readonly MicroResource ElectricalFuse = new MicroResource("ElectricalFuse", MicroResourceCategory.Components, 128961531);
        public static readonly MicroResource ElectricalWiring = new MicroResource("ElectricalWiring", MicroResourceCategory.Components, 128961532);
        public static readonly MicroResource Electromagnet = new MicroResource("Electromagnet", MicroResourceCategory.Components, 128962573);
        public static readonly MicroResource EncryptedMemoryChip = new MicroResource("EncryptedMemoryChip", MicroResourceCategory.Components, 128961533);
        public static readonly MicroResource Epinephrine = new MicroResource("Epinephrine", MicroResourceCategory.Components, 128962575);
        public static readonly MicroResource EpoxyAdhesive = new MicroResource("EpoxyAdhesive", MicroResourceCategory.Components, 128961534);
        public static readonly MicroResource Graphene = new MicroResource("Graphene", MicroResourceCategory.Components, 128064021);
        public static readonly MicroResource IonBattery = new MicroResource("IonBattery", MicroResourceCategory.Components, 128965844);
        public static readonly MicroResource MemoryChip = new MicroResource("MemoryChip", MicroResourceCategory.Components, 128961537);
        public static readonly MicroResource MetalCoil = new MicroResource("MetalCoil", MicroResourceCategory.Components, 128961538);
        public static readonly MicroResource MicroElectrode = new MicroResource("MicroElectrode", MicroResourceCategory.Components, 128964025);
        public static readonly MicroResource MicroHydraulics = new MicroResource("MicroHydraulics", MicroResourceCategory.Components, 128961539);
        public static readonly MicroResource MicroSuperCapacitor = new MicroResource("MicroSuperCapacitor", MicroResourceCategory.Components, 128961540);
        public static readonly MicroResource MicroThrusters = new MicroResource("MicroThrusters", MicroResourceCategory.Components, 128961541);
        public static readonly MicroResource MicroTransformer = new MicroResource("MicroTransformer", MicroResourceCategory.Components, 128961542);
        public static readonly MicroResource Motor = new MicroResource("Motor", MicroResourceCategory.Components, 128961543);
        public static readonly MicroResource OpticalFibre = new MicroResource("OpticalFibre", MicroResourceCategory.Components, 128961544);
        public static readonly MicroResource OpticalLens = new MicroResource("OpticalLens", MicroResourceCategory.Components, 128961545);
        public static readonly MicroResource OxygenicBacteria = new MicroResource("OxygenicBacteria", MicroResourceCategory.Components, 128962574);
        public static readonly MicroResource pHNeutraliser = new MicroResource("pHNeutraliser", MicroResourceCategory.Components, 128962576);
        public static readonly MicroResource RDX = new MicroResource("RDX", MicroResourceCategory.Components, 128962572);
        public static readonly MicroResource Scrambler = new MicroResource("Scrambler", MicroResourceCategory.Components, 128961547);
        public static readonly MicroResource TitaniumPlating = new MicroResource("TitaniumPlating", MicroResourceCategory.Components, 128961549);
        public static readonly MicroResource Transmitter = new MicroResource("Transmitter", MicroResourceCategory.Components, 128961550);
        public static readonly MicroResource TungstenCarbide = new MicroResource("TungstenCarbide", MicroResourceCategory.Components, 128961551);
        public static readonly MicroResource ViscoelasticPolymer = new MicroResource("ViscoelasticPolymer", MicroResourceCategory.Components, 128961552);
        public static readonly MicroResource WeaponComponent = new MicroResource("WeaponComponent", MicroResourceCategory.Components, 128965845);

        // Consumables
        public static readonly MicroResource Amm_Grenade_Frag = new MicroResource("Amm_Grenade_Frag", MicroResourceCategory.Consumables, 128951162);
        public static readonly MicroResource Amm_Grenade_EMP = new MicroResource("Amm_Grenade_EMP", MicroResourceCategory.Consumables, 128951161);
        public static readonly MicroResource Amm_Grenade_Shield = new MicroResource("Amm_Grenade_Shield", MicroResourceCategory.Consumables, 128951163);
        public static readonly MicroResource Bypass = new MicroResource("Bypass", MicroResourceCategory.Consumables, 128961251);
        public static readonly MicroResource EnergyCell = new MicroResource("EnergyCell", MicroResourceCategory.Consumables, 128941245);
        public static readonly MicroResource HealthPack = new MicroResource("HealthPack", MicroResourceCategory.Consumables, 128932270);

        // Data
        public static readonly MicroResource AccidentLogs = new MicroResource("AccidentLogs", MicroResourceCategory.Data, 128972282);
        public static readonly MicroResource AirQualityReports = new MicroResource("AirQualityReports", MicroResourceCategory.Data, 128972283);
        public static readonly MicroResource AtmosphericData = new MicroResource("AtmosphericData", MicroResourceCategory.Data, 128972284);
        public static readonly MicroResource AudioLogs = new MicroResource("AudioLogs", MicroResourceCategory.Data, 128972285);
        public static readonly MicroResource AXCombatLogs = new MicroResource("AXCombatLogs", MicroResourceCategory.Data, 128972281);
        public static readonly MicroResource BallisticsData = new MicroResource("BallisticsData", MicroResourceCategory.Data, 128972286);
        public static readonly MicroResource BiologicalWeaponData = new MicroResource("BiologicalWeaponData", MicroResourceCategory.Data, 128972287);
        public static readonly MicroResource BiometricData = new MicroResource("BiometricData", MicroResourceCategory.Data, 128964034);
        public static readonly MicroResource BlacklistData = new MicroResource("BlacklistData", MicroResourceCategory.Data, 128972288);
        public static readonly MicroResource BloodTestResults = new MicroResource("BloodTestResults", MicroResourceCategory.Data, 128972289);
        public static readonly MicroResource CampaignPlans = new MicroResource("CampaignPlans", MicroResourceCategory.Data, 128972290);
        public static readonly MicroResource CatMedia = new MicroResource("CatMedia", MicroResourceCategory.Data, 128972291);
        public static readonly MicroResource CensusData = new MicroResource("CensusData", MicroResourceCategory.Data, 128972292);
        public static readonly MicroResource ChemicalExperimentData = new MicroResource("ChemicalExperimentData", MicroResourceCategory.Data, 128972293);
        public static readonly MicroResource ChemicalFormulae = new MicroResource("ChemicalFormulae", MicroResourceCategory.Data, 128972294);
        public static readonly MicroResource ChemicalInventory = new MicroResource("ChemicalInventory", MicroResourceCategory.Data, 128972295);
        public static readonly MicroResource ChemicalPatents = new MicroResource("ChemicalPatents", MicroResourceCategory.Data, 128972296);
        public static readonly MicroResource ChemicalWeaponData = new MicroResource("ChemicalWeaponData", MicroResourceCategory.Data, 128972297);
        public static readonly MicroResource ClassicEntertainment = new MicroResource("ClassicEntertainment", MicroResourceCategory.Data, 128972298);
        public static readonly MicroResource CocktailRecipes = new MicroResource("CocktailRecipes", MicroResourceCategory.Data, 128972299);
        public static readonly MicroResource CombatTrainingMaterial = new MicroResource("CombatTrainingMaterial", MicroResourceCategory.Data, 128972300);
        public static readonly MicroResource CombatantPerformance = new MicroResource("CombatantPerformance", MicroResourceCategory.Data, 128972301);
        public static readonly MicroResource ConflictHistory = new MicroResource("ConflictHistory", MicroResourceCategory.Data, 128972302);
        public static readonly MicroResource CriminalRecords = new MicroResource("CriminalRecords", MicroResourceCategory.Data, 128972388);
        public static readonly MicroResource CropYieldAnalysis = new MicroResource("CropYieldAnalysis", MicroResourceCategory.Data, 128972303);
        public static readonly MicroResource CulinaryRecipes = new MicroResource("CulinaryRecipes", MicroResourceCategory.Data, 128972304);
        public static readonly MicroResource DigitalDesigns = new MicroResource("DigitalDesigns", MicroResourceCategory.Data, 128972305);
        public static readonly MicroResource DutyRota = new MicroResource("DutyRota", MicroResourceCategory.Data, 128972306);
        public static readonly MicroResource EmployeeDirectory = new MicroResource("EmployeeDirectory", MicroResourceCategory.Data, 128972307);
        public static readonly MicroResource EmployeeExpenses = new MicroResource("EmployeeExpenses", MicroResourceCategory.Data, 128972308);
        public static readonly MicroResource EmployeeGeneticData = new MicroResource("EmployeeGeneticData", MicroResourceCategory.Data, 128972309);
        public static readonly MicroResource EmploymentHistory = new MicroResource("EmploymentHistory", MicroResourceCategory.Data, 128972310);
        public static readonly MicroResource EnhancedInterrogationRecordings = new MicroResource("EnhancedInterrogationRecordings", MicroResourceCategory.Data, 128972311);
        public static readonly MicroResource EspionageMaterial = new MicroResource("EspionageMaterial", MicroResourceCategory.Data, 128972312);
        public static readonly MicroResource EvacuationProtocols = new MicroResource("EvacuationProtocols", MicroResourceCategory.Data, 128972313);
        public static readonly MicroResource ExplorationJournals = new MicroResource("ExplorationJournals", MicroResourceCategory.Data, 128972314);
        public static readonly MicroResource ExtractionYieldData = new MicroResource("ExtractionYieldData", MicroResourceCategory.Data, 128972315);
        public static readonly MicroResource FactionAssociates = new MicroResource("FactionAssociates", MicroResourceCategory.Data, 128972316);
        public static readonly MicroResource FactionDonatorList = new MicroResource("FactionDonatorList", MicroResourceCategory.Data, 128972389);
        public static readonly MicroResource FactionNews = new MicroResource("FactionNews", MicroResourceCategory.Data, 128972318);
        public static readonly MicroResource FinancialProjections = new MicroResource("FinancialProjections", MicroResourceCategory.Data, 128972317);
        public static readonly MicroResource FleetRegistry = new MicroResource("FleetRegistry", MicroResourceCategory.Data, 128972319);
        public static readonly MicroResource GeneSequencingData = new MicroResource("GeneSequencingData", MicroResourceCategory.Data, 128972320);
        public static readonly MicroResource GeneticResearch = new MicroResource("GeneticResearch", MicroResourceCategory.Data, 128972321);
        public static readonly MicroResource GeologicalData = new MicroResource("GeologicalData", MicroResourceCategory.Data, 128972387);
        public static readonly MicroResource HydroponicData = new MicroResource("HydroponicData", MicroResourceCategory.Data, 128972322);
        public static readonly MicroResource IncidentLogs = new MicroResource("IncidentLogs", MicroResourceCategory.Data, 128972323);
        public static readonly MicroResource InfluenceProjections = new MicroResource("InfluenceProjections", MicroResourceCategory.Data, 128972324);
        public static readonly MicroResource InternalCorrespondence = new MicroResource("InternalCorrespondence", MicroResourceCategory.Data, 128672130);
        public static readonly MicroResource InterrogationRecordings = new MicroResource("InterrogationRecordings", MicroResourceCategory.Data, 128972325);
        public static readonly MicroResource InterviewRecordings = new MicroResource("InterviewRecordings", MicroResourceCategory.Data, 128972326);
        public static readonly MicroResource JobApplications = new MicroResource("JobApplications", MicroResourceCategory.Data, 128972327);
        public static readonly MicroResource Kompromat = new MicroResource("Kompromat", MicroResourceCategory.Data, 128972328);
        public static readonly MicroResource LiteraryFiction = new MicroResource("LiteraryFiction", MicroResourceCategory.Data, 128972329);
        public static readonly MicroResource MaintenanceLogs = new MicroResource("MaintenanceLogs", MicroResourceCategory.Data, 128972330);
        public static readonly MicroResource ManufacturingInstructions = new MicroResource("ManufacturingInstructions", MicroResourceCategory.Data, 128972331);
        public static readonly MicroResource MedicalRecords = new MicroResource("MedicalRecords", MicroResourceCategory.Data, 128972332);
        public static readonly MicroResource MedicalTrialRecords = new MicroResource("MedicalTrialRecords", MicroResourceCategory.Data, 128972333);
        public static readonly MicroResource MeetingMinutes = new MicroResource("MeetingMinutes", MicroResourceCategory.Data, 128972334);
        public static readonly MicroResource MineralSurvey = new MicroResource("MineralSurvey", MicroResourceCategory.Data, 128972335);
        public static readonly MicroResource MiningAnalytics = new MicroResource("MiningAnalytics", MicroResourceCategory.Data, 128972336);
        public static readonly MicroResource MultimediaEntertainment = new MicroResource("MultimediaEntertainment", MicroResourceCategory.Data, 128972337);
        public static readonly MicroResource NetworkAccessHistory = new MicroResource("NetworkAccessHistory", MicroResourceCategory.Data, 128972338);
        public static readonly MicroResource NetworkSecurityProtocols = new MicroResource("NetworkSecurityProtocols", MicroResourceCategory.Data, 128972339);
        public static readonly MicroResource NextofKinRecords = new MicroResource("NextofKinRecords", MicroResourceCategory.Data, 128972340);
        public static readonly MicroResource NOCData = new MicroResource("NOCData", MicroResourceCategory.Data, 128972280);
        public static readonly MicroResource OperationalManual = new MicroResource("OperationalManual", MicroResourceCategory.Data, 128972341);
        public static readonly MicroResource OpinionPolls = new MicroResource("OpinionPolls", MicroResourceCategory.Data, 128972342);
        public static readonly MicroResource PatientHistory = new MicroResource("PatientHistory", MicroResourceCategory.Data, 128972343);
        public static readonly MicroResource PatrolRoutes = new MicroResource("PatrolRoutes", MicroResourceCategory.Data, 128972344);
        public static readonly MicroResource PayrollInformation = new MicroResource("PayrollInformation", MicroResourceCategory.Data, 128972386);
        public static readonly MicroResource PersonalLogs = new MicroResource("PersonalLogs", MicroResourceCategory.Data, 128972345);
        public static readonly MicroResource PharmaceuticalPatents = new MicroResource("PharmaceuticalPatents", MicroResourceCategory.Data, 128972390);
        public static readonly MicroResource PhotoAlbums = new MicroResource("PhotoAlbums", MicroResourceCategory.Data, 128972346);
        public static readonly MicroResource PlantGrowthCharts = new MicroResource("PlantGrowthCharts", MicroResourceCategory.Data, 128972347);
        public static readonly MicroResource PoliticalAffiliations = new MicroResource("PoliticalAffiliations", MicroResourceCategory.Data, 128972348);
        public static readonly MicroResource PrisonerLogs = new MicroResource("PrisonerLogs", MicroResourceCategory.Data, 128972349);
        public static readonly MicroResource ProductionReports = new MicroResource("ProductionReports", MicroResourceCategory.Data, 128972350);
        public static readonly MicroResource ProductionSchedule = new MicroResource("ProductionSchedule", MicroResourceCategory.Data, 128972351);
        public static readonly MicroResource Propaganda = new MicroResource("Propaganda", MicroResourceCategory.Data, 128972352);
        public static readonly MicroResource PurchaseRecords = new MicroResource("PurchaseRecords", MicroResourceCategory.Data, 128972353);
        public static readonly MicroResource PurchaseRequests = new MicroResource("PurchaseRequests", MicroResourceCategory.Data, 128972354);
        public static readonly MicroResource RadioactivityData = new MicroResource("RadioactivityData", MicroResourceCategory.Data, 128972355);
        public static readonly MicroResource ReactorOutputReview = new MicroResource("ReactorOutputReview", MicroResourceCategory.Data, 128972356);
        public static readonly MicroResource RecyclingLogs = new MicroResource("RecyclingLogs", MicroResourceCategory.Data, 128972357);
        public static readonly MicroResource ResidentialDirectory = new MicroResource("ResidentialDirectory", MicroResourceCategory.Data, 128972358);
        public static readonly MicroResource RiskAssessments = new MicroResource("RiskAssessments", MicroResourceCategory.Data, 128972359);
        public static readonly MicroResource SalesRecords = new MicroResource("SalesRecords", MicroResourceCategory.Data, 128972360);
        public static readonly MicroResource SecurityExpenses = new MicroResource("SecurityExpenses", MicroResourceCategory.Data, 128972361);
        public static readonly MicroResource SeedGeneaology = new MicroResource("SeedGeneaology", MicroResourceCategory.Data, 128972362);
        public static readonly MicroResource SettlementAssaultPlans = new MicroResource("SettlementAssaultPlans", MicroResourceCategory.Data, 128972363);
        public static readonly MicroResource SettlementDefencePlans = new MicroResource("SettlementDefencePlans", MicroResourceCategory.Data, 128972364);
        public static readonly MicroResource ShareholderInformation = new MicroResource("ShareholderInformation", MicroResourceCategory.Data, 128972365);
        public static readonly MicroResource SlushFundLogs = new MicroResource("SlushFundLogs", MicroResourceCategory.Data, 128972366);
        public static readonly MicroResource SmearCampaignPlans = new MicroResource("SmearCampaignPlans", MicroResourceCategory.Data, 128972367);
        public static readonly MicroResource SpectralAnalysisData = new MicroResource("SpectralAnalysisData", MicroResourceCategory.Data, 128972368);
        public static readonly MicroResource Spyware = new MicroResource("Spyware", MicroResourceCategory.Data, 128961514);
        public static readonly MicroResource StellarActivityLogs = new MicroResource("StellarActivityLogs", MicroResourceCategory.Data, 128972369);
        public static readonly MicroResource SurveilleanceLogs = new MicroResource("SurveilleanceLogs", MicroResourceCategory.Data, 128972370);
        public static readonly MicroResource TacticalPlans = new MicroResource("TacticalPlans", MicroResourceCategory.Data, 128972371);
        public static readonly MicroResource TaxRecords = new MicroResource("TaxRecords", MicroResourceCategory.Data, 128972372);
        public static readonly MicroResource TopographicalSurveys = new MicroResource("TopographicalSurveys", MicroResourceCategory.Data, 128972373);
        public static readonly MicroResource TravelPermits = new MicroResource("TravelPermits", MicroResourceCategory.Data, 128972374);
        public static readonly MicroResource TroopDeploymentRecords = new MicroResource("TroopDeploymentRecords", MicroResourceCategory.Data, 128972375);
        public static readonly MicroResource UnionMembership = new MicroResource("UnionMembership", MicroResourceCategory.Data, 128972376);
        public static readonly MicroResource VaccinationRecords = new MicroResource("VaccinationRecords", MicroResourceCategory.Data, 128972377);
        public static readonly MicroResource VaccineResearch = new MicroResource("VaccineResearch", MicroResourceCategory.Data, 128972378);
        public static readonly MicroResource VIPSecurityDetail = new MicroResource("VIPSecurityDetail", MicroResourceCategory.Data, 128972379);
        public static readonly MicroResource VirologyData = new MicroResource("VirologyData", MicroResourceCategory.Data, 128972380);
        public static readonly MicroResource Virus = new MicroResource("Virus", MicroResourceCategory.Data, 128972381);
        public static readonly MicroResource VisitorRegister = new MicroResource("VisitorRegister", MicroResourceCategory.Data, 128972382);
        public static readonly MicroResource WeaponInventory = new MicroResource("WeaponInventory", MicroResourceCategory.Data, 128972383);
        public static readonly MicroResource WeaponTestData = new MicroResource("WeaponTestData", MicroResourceCategory.Data, 128972384);
        public static readonly MicroResource XenoDefenceProtocols = new MicroResource("XenoDefenceProtocols", MicroResourceCategory.Data, 128972385);

        // Items
        public static readonly MicroResource AgriculturalProcessSample = new MicroResource("AgriculturalProcessSample", MicroResourceCategory.Items, 128965837);
        public static readonly MicroResource BiochemicalAgent = new MicroResource("BiochemicalAgent", MicroResourceCategory.Items, 128961554);
        public static readonly MicroResource BuildingSchematic = new MicroResource("BuildingSchematic", MicroResourceCategory.Items, 128962597);
        public static readonly MicroResource Californium = new MicroResource("Californium", MicroResourceCategory.Items, 128961556);
        public static readonly MicroResource CastFossil = new MicroResource("CastFossil", MicroResourceCategory.Items, 128961557);
        public static readonly MicroResource ChemicalProcessSample = new MicroResource("ChemicalProcessSample", MicroResourceCategory.Items, 128965838);
        public static readonly MicroResource ChemicalSample = new MicroResource("ChemicalSample", MicroResourceCategory.Items, 128959449);
        public static readonly MicroResource CompactLibrary = new MicroResource("CompactLibrary", MicroResourceCategory.Items, 128962598);
        public static readonly MicroResource CompressionLiquefiedGas = new MicroResource("CompressionLiquefiedGas", MicroResourceCategory.Items, 128965840);
        public static readonly MicroResource DeepMantleSample = new MicroResource("DeepMantleSample", MicroResourceCategory.Items, 128962599);
        public static readonly MicroResource DegradedPowerRegulator = new MicroResource("DegradedPowerRegulator", MicroResourceCategory.Items, 128965841);
        public static readonly MicroResource GeneticRepairMeds = new MicroResource("GeneticRepairMeds", MicroResourceCategory.Items, 128962315);
        public static readonly MicroResource GeneticSample = new MicroResource("GeneticSample", MicroResourceCategory.Items, 128961564);
        public static readonly MicroResource GMeds = new MicroResource("GMeds", MicroResourceCategory.Items, 128961565);
        public static readonly MicroResource HealthMonitor = new MicroResource("HealthMonitor", MicroResourceCategory.Items, 128961566);
        public static readonly MicroResource Hush = new MicroResource("Hush", MicroResourceCategory.Items, 128962600);
        public static readonly MicroResource InertiaCanister = new MicroResource("InertiaCanister", MicroResourceCategory.Items, 128961567);
        public static readonly MicroResource Infinity = new MicroResource("Infinity", MicroResourceCategory.Items, 128962601);
        public static readonly MicroResource InorganicContaminant = new MicroResource("InorganicContaminant", MicroResourceCategory.Items, 128965836);
        public static readonly MicroResource Insight = new MicroResource("Insight", MicroResourceCategory.Items, 128961568);
        public static readonly MicroResource InsightDataBank = new MicroResource("InsightDataBank", MicroResourceCategory.Items, 128961569);
        public static readonly MicroResource InsightEntertainmentSuite = new MicroResource("InsightEntertainmentSuite", MicroResourceCategory.Items, 128962602);
        public static readonly MicroResource IonisedGas = new MicroResource("IonisedGas", MicroResourceCategory.Items, 128961570);
        public static readonly MicroResource LargeCapacityPowerRegulator = new MicroResource("LargeCapacityPowerRegulator", MicroResourceCategory.Items, 128965842);
        public static readonly MicroResource Lazarus = new MicroResource("Lazarus", MicroResourceCategory.Items, 128962603);
        public static readonly MicroResource MicrobialInhibitor = new MicroResource("MicrobialInhibitor", MicroResourceCategory.Items, 128962604);
        public static readonly MicroResource MutagenicCatalyst = new MicroResource("MutagenicCatalyst", MicroResourceCategory.Items, 128961571);
        public static readonly MicroResource NutritionalConcentrate = new MicroResource("NutritionalConcentrate", MicroResourceCategory.Items, 128962605);
        public static readonly MicroResource PersonalComputer = new MicroResource("PersonalComputer", MicroResourceCategory.Items, 128961584);
        public static readonly MicroResource PersonalDocuments = new MicroResource("PersonalDocuments", MicroResourceCategory.Items, 128962606);
        public static readonly MicroResource PetrifiedFossil = new MicroResource("PetrifiedFossil", MicroResourceCategory.Items, 128961586);
        public static readonly MicroResource Push = new MicroResource("Push", MicroResourceCategory.Items, 128962607);
        public static readonly MicroResource PyrolyticCatalyst = new MicroResource("PyrolyticCatalyst", MicroResourceCategory.Items, 128965835);
        public static readonly MicroResource RefinementProcessSample = new MicroResource("RefinementProcessSample", MicroResourceCategory.Items, 128965839);
        public static readonly MicroResource ShipSchematic = new MicroResource("ShipSchematic", MicroResourceCategory.Items, 128962608);
        public static readonly MicroResource SuitSchematic = new MicroResource("SuitSchematic", MicroResourceCategory.Items, 128962609);
        public static readonly MicroResource SurveillanceEquipment = new MicroResource("SurveillanceEquipment", MicroResourceCategory.Items, 128962610);
        public static readonly MicroResource SyntheticGenome = new MicroResource("SyntheticGenome", MicroResourceCategory.Items, 128961590);
        public static readonly MicroResource SyntheticPathogen = new MicroResource("SyntheticPathogen", MicroResourceCategory.Items, 128962611);
        public static readonly MicroResource TrueFormFossil = new MicroResource("TrueFormFossil", MicroResourceCategory.Items, 128961591);
        public static readonly MicroResource UniversalTranslator = new MicroResource("UniversalTranslator", MicroResourceCategory.Items, 128962612);
        public static readonly MicroResource VehicleSchematic = new MicroResource("VehicleSchematic", MicroResourceCategory.Items, 128962613);
        public static readonly MicroResource WeaponSchematic = new MicroResource("WeaponSchematic", MicroResourceCategory.Items, 128962614);
        
        // Unknown / Miscellaneous
        public static readonly MicroResource None = new MicroResource("None", MicroResourceCategory.Unknown);

        public string category => Category?.localizedName;

        public long? EDID { get; private set; }
        public MicroResourceCategory Category { get; private set; }

        // dummy used to ensure that the static constructor has run
        public MicroResource() : this("", MicroResourceCategory.Unknown)
        { }

        private MicroResource(string edname, MicroResourceCategory category, long? EDID = null) : base(edname, edname)
        {
            this.Category = category;
            this.EDID = EDID;
        }

        public static MicroResource FromEDName(string edname, string fallbackName = null, string categoryEdName = null)
        {
            if (edname == null) { return None; }
            string normalizedEDName = edname
                .ToLowerInvariant()
                .Replace("$", "")
                .Replace("_name;", "");
            var result = ResourceBasedLocalizedEDName<MicroResource>.FromEDName(normalizedEDName);
            result.fallbackLocalizedName = fallbackName;
            if (!string.IsNullOrEmpty(categoryEdName)) { result.Category = MicroResourceCategory.FromEDName(categoryEdName); }
            return result;
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

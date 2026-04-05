using Newtonsoft.Json;
using System;
using System.Linq;
using Utilities;

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
        }

        // Components
        public static readonly MicroResource Aerogel = new("Aerogel", MicroResourceCategory.Components, 128961524);
        public static readonly MicroResource CarbonFibrePlating = new("CarbonFibrePlating", MicroResourceCategory.Components, 128961526);
        public static readonly MicroResource ChemicalCatalyst = new("ChemicalCatalyst", MicroResourceCategory.Components, 128961527);
        public static readonly MicroResource ChemicalSuperbase = new("ChemicalSuperbase", MicroResourceCategory.Components, 128961528);
        public static readonly MicroResource CircuitBoard = new("CircuitBoard", MicroResourceCategory.Components, 128961529);
        public static readonly MicroResource CircuitSwitch = new("CircuitSwitch", MicroResourceCategory.Components, 128961530);
        public static readonly MicroResource ElectricalFuse = new("ElectricalFuse", MicroResourceCategory.Components, 128961531);
        public static readonly MicroResource ElectricalWiring = new("ElectricalWiring", MicroResourceCategory.Components, 128961532);
        public static readonly MicroResource Electromagnet = new("Electromagnet", MicroResourceCategory.Components, 128962573);
        public static readonly MicroResource EncryptedMemoryChip = new("EncryptedMemoryChip", MicroResourceCategory.Components, 128961533);
        public static readonly MicroResource Epinephrine = new("Epinephrine", MicroResourceCategory.Components, 128962575);
        public static readonly MicroResource EpoxyAdhesive = new("EpoxyAdhesive", MicroResourceCategory.Components, 128961534);
        public static readonly MicroResource Graphene = new("Graphene", MicroResourceCategory.Components, 128064021);
        public static readonly MicroResource IonBattery = new("IonBattery", MicroResourceCategory.Components, 128965844);
        public static readonly MicroResource MemoryChip = new("MemoryChip", MicroResourceCategory.Components, 128961537);
        public static readonly MicroResource MetalCoil = new("MetalCoil", MicroResourceCategory.Components, 128961538);
        public static readonly MicroResource MicroElectrode = new("MicroElectrode", MicroResourceCategory.Components, 128964025);
        public static readonly MicroResource MicroHydraulics = new("MicroHydraulics", MicroResourceCategory.Components, 128961539);
        public static readonly MicroResource MicroSuperCapacitor = new("MicroSuperCapacitor", MicroResourceCategory.Components, 128961540);
        public static readonly MicroResource MicroThrusters = new("MicroThrusters", MicroResourceCategory.Components, 128961541);
        public static readonly MicroResource MicroTransformer = new("MicroTransformer", MicroResourceCategory.Components, 128961542);
        public static readonly MicroResource Motor = new("Motor", MicroResourceCategory.Components, 128961543);
        public static readonly MicroResource OpticalFibre = new("OpticalFibre", MicroResourceCategory.Components, 128961544);
        public static readonly MicroResource OpticalLens = new("OpticalLens", MicroResourceCategory.Components, 128961545);
        public static readonly MicroResource OxygenicBacteria = new("OxygenicBacteria", MicroResourceCategory.Components, 128962574);
        public static readonly MicroResource pHNeutraliser = new("pHNeutraliser", MicroResourceCategory.Components, 128962576);
        public static readonly MicroResource RDX = new("RDX", MicroResourceCategory.Components, 128962572);
        public static readonly MicroResource Scrambler = new("Scrambler", MicroResourceCategory.Components, 128961547);
        public static readonly MicroResource TitaniumPlating = new("TitaniumPlating", MicroResourceCategory.Components, 128961549);
        public static readonly MicroResource Transmitter = new("Transmitter", MicroResourceCategory.Components, 128961550);
        public static readonly MicroResource TungstenCarbide = new("TungstenCarbide", MicroResourceCategory.Components, 128961551);
        public static readonly MicroResource ViscoelasticPolymer = new("ViscoelasticPolymer", MicroResourceCategory.Components, 128961552);
        public static readonly MicroResource WeaponComponent = new("WeaponComponent", MicroResourceCategory.Components, 128965845);

        // Consumables
        public static readonly MicroResource Amm_Grenade_Frag = new("Amm_Grenade_Frag", MicroResourceCategory.Consumables, 128951162);
        public static readonly MicroResource Amm_Grenade_EMP = new("Amm_Grenade_EMP", MicroResourceCategory.Consumables, 128951161);
        public static readonly MicroResource Amm_Grenade_Shield = new("Amm_Grenade_Shield", MicroResourceCategory.Consumables, 128951163);
        public static readonly MicroResource Bypass = new("Bypass", MicroResourceCategory.Consumables, 128961251);
        public static readonly MicroResource EnergyCell = new("EnergyCell", MicroResourceCategory.Consumables, 128941245);
        public static readonly MicroResource HealthPack = new("HealthPack", MicroResourceCategory.Consumables, 128932270);

        // Data
        public static readonly MicroResource AccidentLogs = new("AccidentLogs", MicroResourceCategory.Data, 128972282);
        public static readonly MicroResource AirQualityReports = new("AirQualityReports", MicroResourceCategory.Data, 128972283);
        public static readonly MicroResource AtmosphericData = new("AtmosphericData", MicroResourceCategory.Data, 128972284);
        public static readonly MicroResource AudioLogs = new("AudioLogs", MicroResourceCategory.Data, 128972285);
        public static readonly MicroResource AXCombatLogs = new("AXCombatLogs", MicroResourceCategory.Data, 128972281);
        public static readonly MicroResource BallisticsData = new("BallisticsData", MicroResourceCategory.Data, 128972286);
        public static readonly MicroResource BiologicalWeaponData = new("BiologicalWeaponData", MicroResourceCategory.Data, 128972287);
        public static readonly MicroResource BiometricData = new("BiometricData", MicroResourceCategory.Data, 128964034);
        public static readonly MicroResource BlacklistData = new("BlacklistData", MicroResourceCategory.Data, 128972288);
        public static readonly MicroResource BloodTestResults = new("BloodTestResults", MicroResourceCategory.Data, 128972289);
        public static readonly MicroResource CampaignPlans = new("CampaignPlans", MicroResourceCategory.Data, 128972290);
        public static readonly MicroResource CatMedia = new("CatMedia", MicroResourceCategory.Data, 128972291);
        public static readonly MicroResource CensusData = new("CensusData", MicroResourceCategory.Data, 128972292);
        public static readonly MicroResource ChemicalExperimentData = new("ChemicalExperimentData", MicroResourceCategory.Data, 128972293);
        public static readonly MicroResource ChemicalFormulae = new("ChemicalFormulae", MicroResourceCategory.Data, 128972294);
        public static readonly MicroResource ChemicalInventory = new("ChemicalInventory", MicroResourceCategory.Data, 128972295);
        public static readonly MicroResource ChemicalPatents = new("ChemicalPatents", MicroResourceCategory.Data, 128972296);
        public static readonly MicroResource ChemicalWeaponData = new("ChemicalWeaponData", MicroResourceCategory.Data, 128972297);
        public static readonly MicroResource ClassicEntertainment = new("ClassicEntertainment", MicroResourceCategory.Data, 128972298);
        public static readonly MicroResource CocktailRecipes = new("CocktailRecipes", MicroResourceCategory.Data, 128972299);
        public static readonly MicroResource CombatTrainingMaterial = new("CombatTrainingMaterial", MicroResourceCategory.Data, 128972300);
        public static readonly MicroResource CombatantPerformance = new("CombatantPerformance", MicroResourceCategory.Data, 128972301);
        public static readonly MicroResource ConflictHistory = new("ConflictHistory", MicroResourceCategory.Data, 128972302);
        public static readonly MicroResource CriminalRecords = new("CriminalRecords", MicroResourceCategory.Data, 128972388);
        public static readonly MicroResource CropYieldAnalysis = new("CropYieldAnalysis", MicroResourceCategory.Data, 128972303);
        public static readonly MicroResource CulinaryRecipes = new("CulinaryRecipes", MicroResourceCategory.Data, 128972304);
        public static readonly MicroResource DigitalDesigns = new("DigitalDesigns", MicroResourceCategory.Data, 128972305);
        public static readonly MicroResource DutyRota = new("DutyRota", MicroResourceCategory.Data, 128972306);
        public static readonly MicroResource EmployeeDirectory = new("EmployeeDirectory", MicroResourceCategory.Data, 128972307);
        public static readonly MicroResource EmployeeExpenses = new("EmployeeExpenses", MicroResourceCategory.Data, 128972308);
        public static readonly MicroResource EmployeeGeneticData = new("EmployeeGeneticData", MicroResourceCategory.Data, 128972309);
        public static readonly MicroResource EmploymentHistory = new("EmploymentHistory", MicroResourceCategory.Data, 128972310);
        public static readonly MicroResource EnhancedInterrogationRecordings = new("EnhancedInterrogationRecordings", MicroResourceCategory.Data, 128972311);
        public static readonly MicroResource EspionageMaterial = new("EspionageMaterial", MicroResourceCategory.Data, 128972312);
        public static readonly MicroResource EvacuationProtocols = new("EvacuationProtocols", MicroResourceCategory.Data, 128972313);
        public static readonly MicroResource ExplorationJournals = new("ExplorationJournals", MicroResourceCategory.Data, 128972314);
        public static readonly MicroResource ExtractionYieldData = new("ExtractionYieldData", MicroResourceCategory.Data, 128972315);
        public static readonly MicroResource FactionAssociates = new("FactionAssociates", MicroResourceCategory.Data, 128972316);
        public static readonly MicroResource FactionDonatorList = new("FactionDonatorList", MicroResourceCategory.Data, 128972389);
        public static readonly MicroResource FactionNews = new("FactionNews", MicroResourceCategory.Data, 128972318);
        public static readonly MicroResource FinancialProjections = new("FinancialProjections", MicroResourceCategory.Data, 128972317);
        public static readonly MicroResource FleetRegistry = new("FleetRegistry", MicroResourceCategory.Data, 128972319);
        public static readonly MicroResource GeneSequencingData = new("GeneSequencingData", MicroResourceCategory.Data, 128972320);
        public static readonly MicroResource GeneticResearch = new("GeneticResearch", MicroResourceCategory.Data, 128972321);
        public static readonly MicroResource GeologicalData = new("GeologicalData", MicroResourceCategory.Data, 128972387);
        public static readonly MicroResource HydroponicData = new("HydroponicData", MicroResourceCategory.Data, 128972322);
        public static readonly MicroResource IncidentLogs = new("IncidentLogs", MicroResourceCategory.Data, 128972323);
        public static readonly MicroResource InfluenceProjections = new("InfluenceProjections", MicroResourceCategory.Data, 128972324);
        public static readonly MicroResource InternalCorrespondence = new("InternalCorrespondence", MicroResourceCategory.Data, 128672130);
        public static readonly MicroResource InterrogationRecordings = new("InterrogationRecordings", MicroResourceCategory.Data, 128972325);
        public static readonly MicroResource InterviewRecordings = new("InterviewRecordings", MicroResourceCategory.Data, 128972326);
        public static readonly MicroResource JobApplications = new("JobApplications", MicroResourceCategory.Data, 128972327);
        public static readonly MicroResource Kompromat = new("Kompromat", MicroResourceCategory.Data, 128972328);
        public static readonly MicroResource LiteraryFiction = new("LiteraryFiction", MicroResourceCategory.Data, 128972329);
        public static readonly MicroResource MaintenanceLogs = new("MaintenanceLogs", MicroResourceCategory.Data, 128972330);
        public static readonly MicroResource ManufacturingInstructions = new("ManufacturingInstructions", MicroResourceCategory.Data, 128972331);
        public static readonly MicroResource MedicalRecords = new("MedicalRecords", MicroResourceCategory.Data, 128972332);
        public static readonly MicroResource MedicalTrialRecords = new("MedicalTrialRecords", MicroResourceCategory.Data, 128972333);
        public static readonly MicroResource MeetingMinutes = new("MeetingMinutes", MicroResourceCategory.Data, 128972334);
        public static readonly MicroResource MineralSurvey = new("MineralSurvey", MicroResourceCategory.Data, 128972335);
        public static readonly MicroResource MiningAnalytics = new("MiningAnalytics", MicroResourceCategory.Data, 128972336);
        public static readonly MicroResource MultimediaEntertainment = new("MultimediaEntertainment", MicroResourceCategory.Data, 128972337);
        public static readonly MicroResource NetworkAccessHistory = new("NetworkAccessHistory", MicroResourceCategory.Data, 128972338);
        public static readonly MicroResource NetworkSecurityProtocols = new("NetworkSecurityProtocols", MicroResourceCategory.Data, 128972339);
        public static readonly MicroResource NextofKinRecords = new("NextofKinRecords", MicroResourceCategory.Data, 128972340);
        public static readonly MicroResource NOCData = new("NOCData", MicroResourceCategory.Data, 128972280);
        public static readonly MicroResource OperationalManual = new("OperationalManual", MicroResourceCategory.Data, 128972341);
        public static readonly MicroResource OpinionPolls = new("OpinionPolls", MicroResourceCategory.Data, 128972342);
        public static readonly MicroResource PatientHistory = new("PatientHistory", MicroResourceCategory.Data, 128972343);
        public static readonly MicroResource PatrolRoutes = new("PatrolRoutes", MicroResourceCategory.Data, 128972344);
        public static readonly MicroResource PayrollInformation = new("PayrollInformation", MicroResourceCategory.Data, 128972386);
        public static readonly MicroResource PersonalLogs = new("PersonalLogs", MicroResourceCategory.Data, 128972345);
        public static readonly MicroResource PharmaceuticalPatents = new("PharmaceuticalPatents", MicroResourceCategory.Data, 128972390);
        public static readonly MicroResource PhotoAlbums = new("PhotoAlbums", MicroResourceCategory.Data, 128972346);
        public static readonly MicroResource PlantGrowthCharts = new("PlantGrowthCharts", MicroResourceCategory.Data, 128972347);
        public static readonly MicroResource PoliticalAffiliations = new("PoliticalAffiliations", MicroResourceCategory.Data, 128972348);
        public static readonly MicroResource PrisonerLogs = new("PrisonerLogs", MicroResourceCategory.Data, 128972349);
        public static readonly MicroResource ProductionReports = new("ProductionReports", MicroResourceCategory.Data, 128972350);
        public static readonly MicroResource ProductionSchedule = new("ProductionSchedule", MicroResourceCategory.Data, 128972351);
        public static readonly MicroResource Propaganda = new("Propaganda", MicroResourceCategory.Data, 128972352);
        public static readonly MicroResource PurchaseRecords = new("PurchaseRecords", MicroResourceCategory.Data, 128972353);
        public static readonly MicroResource PurchaseRequests = new("PurchaseRequests", MicroResourceCategory.Data, 128972354);
        public static readonly MicroResource RadioactivityData = new("RadioactivityData", MicroResourceCategory.Data, 128972355);
        public static readonly MicroResource ReactorOutputReview = new("ReactorOutputReview", MicroResourceCategory.Data, 128972356);
        public static readonly MicroResource RecyclingLogs = new("RecyclingLogs", MicroResourceCategory.Data, 128972357);
        public static readonly MicroResource ResidentialDirectory = new("ResidentialDirectory", MicroResourceCategory.Data, 128972358);
        public static readonly MicroResource RiskAssessments = new("RiskAssessments", MicroResourceCategory.Data, 128972359);
        public static readonly MicroResource SalesRecords = new("SalesRecords", MicroResourceCategory.Data, 128972360);
        public static readonly MicroResource SecurityExpenses = new("SecurityExpenses", MicroResourceCategory.Data, 128972361);
        public static readonly MicroResource SeedGeneaology = new("SeedGeneaology", MicroResourceCategory.Data, 128972362);
        public static readonly MicroResource SettlementAssaultPlans = new("SettlementAssaultPlans", MicroResourceCategory.Data, 128972363);
        public static readonly MicroResource SettlementDefencePlans = new("SettlementDefencePlans", MicroResourceCategory.Data, 128972364);
        public static readonly MicroResource ShareholderInformation = new("ShareholderInformation", MicroResourceCategory.Data, 128972365);
        public static readonly MicroResource SlushFundLogs = new("SlushFundLogs", MicroResourceCategory.Data, 128972366);
        public static readonly MicroResource SmearCampaignPlans = new("SmearCampaignPlans", MicroResourceCategory.Data, 128972367);
        public static readonly MicroResource SpectralAnalysisData = new("SpectralAnalysisData", MicroResourceCategory.Data, 128972368);
        public static readonly MicroResource Spyware = new("Spyware", MicroResourceCategory.Data, 128961514);
        public static readonly MicroResource StellarActivityLogs = new("StellarActivityLogs", MicroResourceCategory.Data, 128972369);
        public static readonly MicroResource SurveilleanceLogs = new("SurveilleanceLogs", MicroResourceCategory.Data, 128972370);
        public static readonly MicroResource TacticalPlans = new("TacticalPlans", MicroResourceCategory.Data, 128972371);
        public static readonly MicroResource TaxRecords = new("TaxRecords", MicroResourceCategory.Data, 128972372);
        public static readonly MicroResource TopographicalSurveys = new("TopographicalSurveys", MicroResourceCategory.Data, 128972373);
        public static readonly MicroResource TravelPermits = new("TravelPermits", MicroResourceCategory.Data, 128972374);
        public static readonly MicroResource TroopDeploymentRecords = new("TroopDeploymentRecords", MicroResourceCategory.Data, 128972375);
        public static readonly MicroResource UnionMembership = new("UnionMembership", MicroResourceCategory.Data, 128972376);
        public static readonly MicroResource VaccinationRecords = new("VaccinationRecords", MicroResourceCategory.Data, 128972377);
        public static readonly MicroResource VaccineResearch = new("VaccineResearch", MicroResourceCategory.Data, 128972378);
        public static readonly MicroResource VIPSecurityDetail = new("VIPSecurityDetail", MicroResourceCategory.Data, 128972379);
        public static readonly MicroResource VirologyData = new("VirologyData", MicroResourceCategory.Data, 128972380);
        public static readonly MicroResource Virus = new("Virus", MicroResourceCategory.Data, 128972381);
        public static readonly MicroResource VisitorRegister = new("VisitorRegister", MicroResourceCategory.Data, 128972382);
        public static readonly MicroResource WeaponInventory = new("WeaponInventory", MicroResourceCategory.Data, 128972383);
        public static readonly MicroResource WeaponTestData = new("WeaponTestData", MicroResourceCategory.Data, 128972384);
        public static readonly MicroResource XenoDefenceProtocols = new("XenoDefenceProtocols", MicroResourceCategory.Data, 128972385);

        // PowerPlay 2.0 Data

        public static readonly MicroResource PowerEmployeeData = new( "PowerEmployeeData", MicroResourceCategory.Data, null, true ); // Power Association Data
        public static readonly MicroResource PowerClassifiedData = new( "PowerClassifiedData", MicroResourceCategory.Data, null, true ); // Power Classified Data
        public static readonly MicroResource PowerFinancialRecords = new( "PowerFinancialRecords", MicroResourceCategory.Data, null, true ); // Power Industrial Data
        public static readonly MicroResource PowerPreparationSpyware = new( "PowerPreparationSpyware", MicroResourceCategory.Data, null, true ); // Power Injection Malware (uploaded data)
        public static readonly MicroResource PowerPropagandaData = new( "PowerPropagandaData", MicroResourceCategory.Data, null, true ); // Power Political Data
        public static readonly MicroResource PowerResearchData = new( "PowerResearchData", MicroResourceCategory.Data, null, true ); // Power Research Data
        public static readonly MicroResource PowerSpyware = new( "PowerSpyware", MicroResourceCategory.Data, null, true ); // Power Tracker Malware (uploaded data)

        // Items
        public static readonly MicroResource AgriculturalProcessSample = new("AgriculturalProcessSample", MicroResourceCategory.Items, 128965837);
        public static readonly MicroResource BiochemicalAgent = new("BiochemicalAgent", MicroResourceCategory.Items, 128961554);
        public static readonly MicroResource BiomechanicalComponent = new("BiomechanicalComponent", MicroResourceCategory.Items, null); // EDID not yet identified
        public static readonly MicroResource BuildingSchematic = new("BuildingSchematic", MicroResourceCategory.Items, 128962597);
        public static readonly MicroResource Californium = new("Californium", MicroResourceCategory.Items, 128961556);
        public static readonly MicroResource CastFossil = new("CastFossil", MicroResourceCategory.Items, 128961557);
        public static readonly MicroResource ChemicalProcessSample = new("ChemicalProcessSample", MicroResourceCategory.Items, 128965838);
        public static readonly MicroResource ChemicalSample = new("ChemicalSample", MicroResourceCategory.Items, 128959449);
        public static readonly MicroResource CompactLibrary = new("CompactLibrary", MicroResourceCategory.Items, 128962598);
        public static readonly MicroResource CompressionLiquefiedGas = new("CompressionLiquefiedGas", MicroResourceCategory.Items, 128965840);
        public static readonly MicroResource DeepMantleSample = new("DeepMantleSample", MicroResourceCategory.Items, 128962599);
        public static readonly MicroResource DegradedPowerRegulator = new("DegradedPowerRegulator", MicroResourceCategory.Items, 128965841);
        public static readonly MicroResource GeneticRepairMeds = new("GeneticRepairMeds", MicroResourceCategory.Items, 128962315);
        public static readonly MicroResource GeneticSample = new("GeneticSample", MicroResourceCategory.Items, 128961564);
        public static readonly MicroResource GMeds = new("GMeds", MicroResourceCategory.Items, 128961565);
        public static readonly MicroResource HealthMonitor = new("HealthMonitor", MicroResourceCategory.Items, 128961566);
        public static readonly MicroResource Hush = new("Hush", MicroResourceCategory.Items, 128962600);
        public static readonly MicroResource InertiaCanister = new("InertiaCanister", MicroResourceCategory.Items, 128961567);
        public static readonly MicroResource Infinity = new("Infinity", MicroResourceCategory.Items, 128962601);
        public static readonly MicroResource InorganicContaminant = new("InorganicContaminant", MicroResourceCategory.Items, 128965836);
        public static readonly MicroResource Insight = new("Insight", MicroResourceCategory.Items, 128961568);
        public static readonly MicroResource InsightDataBank = new("InsightDataBank", MicroResourceCategory.Items, 128961569);
        public static readonly MicroResource InsightEntertainmentSuite = new("InsightEntertainmentSuite", MicroResourceCategory.Items, 128962602);
        public static readonly MicroResource IonisedGas = new("IonisedGas", MicroResourceCategory.Items, 128961570);
        public static readonly MicroResource LargeCapacityPowerRegulator = new("LargeCapacityPowerRegulator", MicroResourceCategory.Items, 128965842);
        public static readonly MicroResource Lazarus = new("Lazarus", MicroResourceCategory.Items, 128962603);
        public static readonly MicroResource MicrobialInhibitor = new("MicrobialInhibitor", MicroResourceCategory.Items, 128962604);
        public static readonly MicroResource MutagenicCatalyst = new("MutagenicCatalyst", MicroResourceCategory.Items, 128961571);
        public static readonly MicroResource NutritionalConcentrate = new("NutritionalConcentrate", MicroResourceCategory.Items, 128962605);
        public static readonly MicroResource PersonalComputer = new("PersonalComputer", MicroResourceCategory.Items, 128961584);
        public static readonly MicroResource PersonalDocuments = new("PersonalDocuments", MicroResourceCategory.Items, 128962606);
        public static readonly MicroResource PetrifiedFossil = new("PetrifiedFossil", MicroResourceCategory.Items, 128961586);
        public static readonly MicroResource Push = new("Push", MicroResourceCategory.Items, 128962607);
        public static readonly MicroResource PyrolyticCatalyst = new("PyrolyticCatalyst", MicroResourceCategory.Items, 128965835);
        public static readonly MicroResource RefinementProcessSample = new("RefinementProcessSample", MicroResourceCategory.Items, 128965839);
        public static readonly MicroResource SabotagedComponent = new( "SabotagedComponent", MicroResourceCategory.Items, null );
        public static readonly MicroResource ShipSchematic = new("ShipSchematic", MicroResourceCategory.Items, 128962608);
        public static readonly MicroResource SuitSchematic = new("SuitSchematic", MicroResourceCategory.Items, 128962609);
        public static readonly MicroResource SurveillanceEquipment = new("SurveillanceEquipment", MicroResourceCategory.Items, 128962610);
        public static readonly MicroResource SyntheticGenome = new("SyntheticGenome", MicroResourceCategory.Items, 128961590);
        public static readonly MicroResource SyntheticPathogen = new("SyntheticPathogen", MicroResourceCategory.Items, 128962611);
        public static readonly MicroResource TrueFormFossil = new("TrueFormFossil", MicroResourceCategory.Items, 128961591);
        public static readonly MicroResource UniversalTranslator = new("UniversalTranslator", MicroResourceCategory.Items, 128962612);
        public static readonly MicroResource VehicleSchematic = new("VehicleSchematic", MicroResourceCategory.Items, 128962613);
        public static readonly MicroResource WeaponSchematic = new("WeaponSchematic", MicroResourceCategory.Items, 128962614);
        public static readonly MicroResource UnicaSeed = new ("nm_seed", MicroResourceCategory.Items, null);

        // PowerPlay 2.0 Items
        public static readonly MicroResource PowerAgriculture = new( "PowerAgriculture", MicroResourceCategory.Items, null, true ); // Agricultural Sample
        public static readonly MicroResource PowerComputer = new( "PowerComputer", MicroResourceCategory.Items, null, true ); // Computer Parts
        public static readonly MicroResource PowerElectronics = new( "PowerElectronics", MicroResourceCategory.Items, null, true ); // Electronics Package
        public static readonly MicroResource PowerEquipment = new( "PowerEquipment", MicroResourceCategory.Items, null, true ); // Personal protective Equipment
        public static readonly MicroResource PowerExperiment = new( "PowerExperiment", MicroResourceCategory.Items, null, true ); // Experiment Prototype
        public static readonly MicroResource PowerExtraction = new( "PowerExtraction", MicroResourceCategory.Items, null, true ); // Extraction Sample
        public static readonly MicroResource PowerIndustrial = new( "PowerIndustrial", MicroResourceCategory.Items, null, true ); // Industrial Component
        public static readonly MicroResource PowerInventory = new( "PowerInventory", MicroResourceCategory.Items, null, true ); // Inventory Record
        public static readonly MicroResource PowerMedical = new( "PowerMedical", MicroResourceCategory.Items, null, true ); // Medical Sample
        public static readonly MicroResource PowerMiscComputer = new( "PowerMiscComputer", MicroResourceCategory.Items, null, true ); // Data Storage Device
        public static readonly MicroResource PowerMiscIndust = new( "PowerMiscIndust", MicroResourceCategory.Items, null, true ); // Industrial Machinery
        public static readonly MicroResource PowerplayMilitary = new( "PowerplayMilitary", MicroResourceCategory.Items, null, true ); // Military Schematic
        public static readonly MicroResource PowerPower = new( "PowerPower", MicroResourceCategory.Items, null, true ); // Energy Regulator
        public static readonly MicroResource PowerResearch = new( "PowerResearch", MicroResourceCategory.Items, null, true ); // Research Notes
        public static readonly MicroResource PowerSecurity = new( "PowerSecurity", MicroResourceCategory.Items, null, true ); // Security Logs
        
        // Unknown / Miscellaneous
        public static readonly MicroResource None = new("None", MicroResourceCategory.Unknown);

        // Powerplay 2.0 Unconfirmed
//        public static readonly MicroResource PowerMegashipData = new MicroResource( "PowerMegashipData", MicroResourceCategory.Data, null, true ); // Power Megaship Data

        [PublicAPI("The localized category name")]
        public string category => Category?.localizedName;

        public long? EDID { get; private set; }

        public MicroResourceCategory Category { get; set; }

        public bool powerplayItem { get; set; }

        // dummy used to ensure that the static constructor has run
        public MicroResource() : this("", MicroResourceCategory.Unknown)
        { }

        private MicroResource(string edname, MicroResourceCategory category, long? EDID = null, bool powerplayItem = false ) : base(edname, edname)
        {
            this.Category = category;
            this.EDID = EDID;
            this.powerplayItem = powerplayItem;
        }

        public static MicroResource FromEDName(string edname, string fallbackName = null, string categoryEdName = null)
        {
            if (edname == null) { return None; }
            var normalizedEDName = edname
                .ToLowerInvariant()
                .Replace("$", "")
                .Replace("_name;", "");
            var result = ResourceBasedLocalizedEDName<MicroResource>.FromEDName(normalizedEDName);
            if ( result is null )
            {
                Logging.Error( $"Unknown micro-resource: '{edname}'" +
                               ( !string.IsNullOrEmpty( fallbackName ) ? $" - Localized Name: '{fallbackName}'" : "" ) +
                               ( !string.IsNullOrEmpty( categoryEdName ) ? $" - Category: '{categoryEdName}'" : "" ) );
                result = new MicroResource( normalizedEDName, MicroResourceCategory.Unknown );
            }
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

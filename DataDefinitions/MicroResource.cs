using Newtonsoft.Json;

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
        public static readonly MicroResource Aerogel = new MicroResource("Aerogel", MicroResourceCategory.Component);
        public static readonly MicroResource CarbonFibrePlating = new MicroResource("CarbonFibrePlating", MicroResourceCategory.Component);
        public static readonly MicroResource CircuitBoard = new MicroResource("CircuitBoard", MicroResourceCategory.Component);
        public static readonly MicroResource CircuitSwitch = new MicroResource("CircuitSwitch", MicroResourceCategory.Component);
        public static readonly MicroResource ElectricalFuse = new MicroResource("ElectricalFuse", MicroResourceCategory.Component);
        public static readonly MicroResource ElectricalWiring = new MicroResource("ElectricalWiring", MicroResourceCategory.Component);
        public static readonly MicroResource Electromagnet = new MicroResource("Electromagnet", MicroResourceCategory.Component);
        public static readonly MicroResource EncryptedMemoryChip = new MicroResource("EncryptedMemoryChip", MicroResourceCategory.Component);
        public static readonly MicroResource EpoxyAdhesive = new MicroResource("EpoxyAdhesive", MicroResourceCategory.Component);
        public static readonly MicroResource Graphene = new MicroResource("Graphene", MicroResourceCategory.Component);
        public static readonly MicroResource MemoryChip = new MicroResource("MemoryChip", MicroResourceCategory.Component);
        public static readonly MicroResource MetalCoil = new MicroResource("MetalCoil", MicroResourceCategory.Component);
        public static readonly MicroResource MicroElectrode = new MicroResource("MicroElectrode", MicroResourceCategory.Component);
        public static readonly MicroResource MicroHydraulics = new MicroResource("MicroHydraulics", MicroResourceCategory.Component);
        public static readonly MicroResource MicroSuperCapacitor = new MicroResource("MicroSuperCapacitor", MicroResourceCategory.Component);
        public static readonly MicroResource MicroTransformer = new MicroResource("MicroTransformer", MicroResourceCategory.Component);
        public static readonly MicroResource Motor = new MicroResource("Motor", MicroResourceCategory.Component);
        public static readonly MicroResource OpticalFibre = new MicroResource("OpticalFibre", MicroResourceCategory.Component);
        public static readonly MicroResource Scrambler = new MicroResource("Scrambler", MicroResourceCategory.Component);
        public static readonly MicroResource TitaniumPlating = new MicroResource("TitaniumPlating", MicroResourceCategory.Component);
        public static readonly MicroResource Transmitter = new MicroResource("Transmitter", MicroResourceCategory.Component);
        public static readonly MicroResource TungstenCarbide = new MicroResource("TungstenCarbide", MicroResourceCategory.Component);

        // Consumables
        public static readonly MicroResource Amm_Grenade_Frag = new MicroResource("Amm_Grenade_Frag", MicroResourceCategory.Consumable);
        public static readonly MicroResource Amm_Grenade_EMP = new MicroResource("Amm_Grenade_EMP", MicroResourceCategory.Consumable);
        public static readonly MicroResource Amm_Grenade_Shield = new MicroResource("Amm_Grenade_Shield", MicroResourceCategory.Consumable);
        public static readonly MicroResource Bypass = new MicroResource("Bypass", MicroResourceCategory.Consumable);
        public static readonly MicroResource EnergyCell = new MicroResource("EnergyCell", MicroResourceCategory.Consumable);
        public static readonly MicroResource HealthPack = new MicroResource("HealthPack", MicroResourceCategory.Consumable);

        // Data
        public static readonly MicroResource AccidentLogs = new MicroResource("AccidentLogs", MicroResourceCategory.Data);
        public static readonly MicroResource AirQualityReports = new MicroResource("AirQualityReports", MicroResourceCategory.Data);
        public static readonly MicroResource AtmosphericData = new MicroResource("AtmosphericData", MicroResourceCategory.Data);
        public static readonly MicroResource BlacklistData = new MicroResource("BlacklistData", MicroResourceCategory.Data);
        public static readonly MicroResource BloodTestResults = new MicroResource("BloodTestResults", MicroResourceCategory.Data);
        public static readonly MicroResource CampaignPlans = new MicroResource("CampaignPlans", MicroResourceCategory.Data);
        public static readonly MicroResource CatMedia = new MicroResource("CatMedia", MicroResourceCategory.Data);
        public static readonly MicroResource CensusData = new MicroResource("CensusData", MicroResourceCategory.Data);
        public static readonly MicroResource ChemicalExperimentData = new MicroResource("ChemicalExperimentData", MicroResourceCategory.Data);
        public static readonly MicroResource ChemicalFormulae = new MicroResource("ChemicalFormulae", MicroResourceCategory.Data);
        public static readonly MicroResource ChemicalInventory = new MicroResource("ChemicalInventory", MicroResourceCategory.Data);
        public static readonly MicroResource ChemicalPatents = new MicroResource("ChemicalPatents", MicroResourceCategory.Data);
        public static readonly MicroResource CombatantPerformance = new MicroResource("CombatantPerformance", MicroResourceCategory.Data);
        public static readonly MicroResource CombatTrainingMaterial = new MicroResource("CombatTrainingMaterial", MicroResourceCategory.Data);
        public static readonly MicroResource DutyRota = new MicroResource("DutyRota", MicroResourceCategory.Data);
        public static readonly MicroResource EmployeeDirectory = new MicroResource("EmployeeDirectory", MicroResourceCategory.Data);
        public static readonly MicroResource EmployeeGeneticData = new MicroResource("EmployeeGeneticData", MicroResourceCategory.Data);
        public static readonly MicroResource EmploymentHistory = new MicroResource("EmploymentHistory", MicroResourceCategory.Data);
        public static readonly MicroResource EvacuationProtocols = new MicroResource("EvacuationProtocols", MicroResourceCategory.Data);
        public static readonly MicroResource ExplorationJournals = new MicroResource("ExplorationJournals", MicroResourceCategory.Data);
        public static readonly MicroResource FactionAssociates = new MicroResource("FactionAssociates", MicroResourceCategory.Data);
        public static readonly MicroResource FactionDonatorList = new MicroResource("FactionDonatorList", MicroResourceCategory.Data);
        public static readonly MicroResource FactionNews = new MicroResource("FactionNews", MicroResourceCategory.Data);
        public static readonly MicroResource FinancialProjections = new MicroResource("FinancialProjections", MicroResourceCategory.Data);
        public static readonly MicroResource GeographicalData = new MicroResource("GeographicalData", MicroResourceCategory.Data);
        public static readonly MicroResource InternalCorrespondence = new MicroResource("InternalCorrespondence", MicroResourceCategory.Data);
        public static readonly MicroResource LiteraryFiction = new MicroResource("LiteraryFiction", MicroResourceCategory.Data);
        public static readonly MicroResource MaintenanceLogs = new MicroResource("MaintenanceLogs", MicroResourceCategory.Data);
        public static readonly MicroResource ManufacturingInstructions = new MicroResource("ManufacturingInstructions", MicroResourceCategory.Data);
        public static readonly MicroResource MeetingMinutes = new MicroResource("MeetingMinutes", MicroResourceCategory.Data);
        public static readonly MicroResource MineralSurvey = new MicroResource("MineralSurvey", MicroResourceCategory.Data);
        public static readonly MicroResource MultimediaEntertainment = new MicroResource("MultimediaEntertainment", MicroResourceCategory.Data);
        public static readonly MicroResource NetworkAccessHistory = new MicroResource("NetworkAccessHistory", MicroResourceCategory.Data);
        public static readonly MicroResource NextOfKinRecords = new MicroResource("NextOfKinRecords", MicroResourceCategory.Data);
        public static readonly MicroResource NOCData = new MicroResource("NOCData", MicroResourceCategory.Data);
        public static readonly MicroResource OperationalManual = new MicroResource("OperationalManual", MicroResourceCategory.Data);
        public static readonly MicroResource PatrolRoutes = new MicroResource("PatrolRoutes", MicroResourceCategory.Data);
        public static readonly MicroResource PayrollInformation = new MicroResource("PayrollInformation", MicroResourceCategory.Data);
        public static readonly MicroResource PersonalLogs = new MicroResource("PersonalLogs", MicroResourceCategory.Data);
        public static readonly MicroResource PharmaceuticalPatents = new MicroResource("PharmaceuticalPatents", MicroResourceCategory.Data);
        public static readonly MicroResource ProductionReports = new MicroResource("ProductionReports", MicroResourceCategory.Data);
        public static readonly MicroResource ProductionSchedule = new MicroResource("ProductionSchedule", MicroResourceCategory.Data);
        public static readonly MicroResource PurchaseRecords = new MicroResource("PurchaseRecords", MicroResourceCategory.Data);
        public static readonly MicroResource PurchaseRequests = new MicroResource("PurchaseRequests", MicroResourceCategory.Data);
        public static readonly MicroResource RadioactivityData = new MicroResource("RadioactivityData", MicroResourceCategory.Data);
        public static readonly MicroResource ReactorOutputReview = new MicroResource("ReactorOutputReview", MicroResourceCategory.Data);
        public static readonly MicroResource ResidentialDirectory = new MicroResource("ResidentialDirectory", MicroResourceCategory.Data);
        public static readonly MicroResource SalesRecords = new MicroResource("SalesRecords", MicroResourceCategory.Data);
        public static readonly MicroResource SettlementDefencePlans = new MicroResource("SettlementDefencePlans", MicroResourceCategory.Data);
        public static readonly MicroResource ShareholderInformation = new MicroResource("ShareholderInformation", MicroResourceCategory.Data);
        public static readonly MicroResource Spyware = new MicroResource("Spyware", MicroResourceCategory.Data);
        public static readonly MicroResource StellarActivityLogs = new MicroResource("StellarActivityLogs", MicroResourceCategory.Data);
        public static readonly MicroResource SurveilleanceLogs = new MicroResource("SurveilleanceLogs", MicroResourceCategory.Data);
        public static readonly MicroResource TacticalPlans = new MicroResource("TacticalPlans", MicroResourceCategory.Data);
        public static readonly MicroResource TaxRecords = new MicroResource("TaxRecords", MicroResourceCategory.Data);
        public static readonly MicroResource TopographicalSurveys = new MicroResource("TopographicalSurveys", MicroResourceCategory.Data);
        public static readonly MicroResource TravelPermits = new MicroResource("TravelPermits", MicroResourceCategory.Data);
        public static readonly MicroResource TroopDeploymentRecords = new MicroResource("TroopDeploymentRecords", MicroResourceCategory.Data);
        public static readonly MicroResource UnionMembership = new MicroResource("UnionMembership", MicroResourceCategory.Data);
        public static readonly MicroResource VaccinationRecords = new MicroResource("VaccinationRecords", MicroResourceCategory.Data);
        public static readonly MicroResource VaccineResearch = new MicroResource("VaccineResearch", MicroResourceCategory.Data);
        public static readonly MicroResource VirologyData = new MicroResource("VirologyData", MicroResourceCategory.Data);
        public static readonly MicroResource Virus = new MicroResource("Virus", MicroResourceCategory.Data);
        public static readonly MicroResource VisitorRegister = new MicroResource("VisitorRegister", MicroResourceCategory.Data);
        public static readonly MicroResource WeaponInventory = new MicroResource("WeaponInventory", MicroResourceCategory.Data);

        // Items
        public static readonly MicroResource AgriculturalProcessSample = new MicroResource("AgriculturalProcessSample", MicroResourceCategory.Item);
        public static readonly MicroResource BiochemicalAgent = new MicroResource("BiochemicalAgent", MicroResourceCategory.Item);
        public static readonly MicroResource BuildingSchematic = new MicroResource("BuildingSchematic", MicroResourceCategory.Item);
        public static readonly MicroResource Californium = new MicroResource("Californium", MicroResourceCategory.Item);
        public static readonly MicroResource ChemicalProcessSample = new MicroResource("ChemicalProcessSample", MicroResourceCategory.Item);
        public static readonly MicroResource ChemicalSample = new MicroResource("ChemicalSample", MicroResourceCategory.Item);
        public static readonly MicroResource CompactLibrary = new MicroResource("CompactLibrary", MicroResourceCategory.Item);
        public static readonly MicroResource DegradedPowerRegulator = new MicroResource("DegradedPowerRegulator", MicroResourceCategory.Item);
        public static readonly MicroResource Hush = new MicroResource("Hush", MicroResourceCategory.Item);
        public static readonly MicroResource Infinity = new MicroResource("Infinity", MicroResourceCategory.Item);
        public static readonly MicroResource Insight = new MicroResource("Insight", MicroResourceCategory.Item);
        public static readonly MicroResource InsightDataBank = new MicroResource("InsightDataBank", MicroResourceCategory.Item);
        public static readonly MicroResource InsightEntertainmentSuite = new MicroResource("InsightEntertainmentSuite", MicroResourceCategory.Item);
        public static readonly MicroResource IonisedGas = new MicroResource("IonisedGas", MicroResourceCategory.Item);
        public static readonly MicroResource LargeCapacityPowerRegulator = new MicroResource("LargeCapacityPowerRegulator", MicroResourceCategory.Item);
        public static readonly MicroResource Lazarus = new MicroResource("Lazarus", MicroResourceCategory.Item);
        public static readonly MicroResource PersonalDocuments = new MicroResource("PersonalDocuments", MicroResourceCategory.Item);
        public static readonly MicroResource Push = new MicroResource("Push", MicroResourceCategory.Item);
        public static readonly MicroResource PyrolyticCatalyst = new MicroResource("PyrolyticCatalyst", MicroResourceCategory.Item);
        public static readonly MicroResource ShipSchematic = new MicroResource("ShipSchematic", MicroResourceCategory.Item);
        public static readonly MicroResource SuitSchematic = new MicroResource("SuitSchematic", MicroResourceCategory.Item);
        public static readonly MicroResource SurveillanceEquipment = new MicroResource("SurveillanceEquipment", MicroResourceCategory.Item);
        public static readonly MicroResource SyntheticPathogen = new MicroResource("SyntheticPathogen", MicroResourceCategory.Item);
        public static readonly MicroResource TrueFormFossil = new MicroResource("TrueFormFossil", MicroResourceCategory.Item);
        public static readonly MicroResource UniversalTranslator = new MicroResource("UniversalTranslator", MicroResourceCategory.Item);
        public static readonly MicroResource VehicleSchematic = new MicroResource("VehicleSchematic", MicroResourceCategory.Item);
        public static readonly MicroResource WeaponSchematic = new MicroResource("WeaponSchematic", MicroResourceCategory.Item);

        // Unknown / Miscellaneous
        public static readonly MicroResource None = new MicroResource("None", MicroResourceCategory.Unknown);

        public string category => Category?.localizedName;

        public MicroResourceCategory Category { get; private set; }

        // dummy used to ensure that the static constructor has run
        public MicroResource() : this("", MicroResourceCategory.Unknown)
        { }

        private MicroResource(string edname, MicroResourceCategory category) : base(edname, edname)
        {
            this.Category = category;
        }

        public new static MicroResource FromEDName(string edname)
        {
            if (edname == null) { return None; }
            string normalizedEDName = edname
                .ToLowerInvariant()
                .Replace("$", "")
                .Replace("_name;", "");
            return ResourceBasedLocalizedEDName<MicroResource>.FromEDName(normalizedEDName);
        }
    }
}

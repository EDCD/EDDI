using System.Collections.Generic;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Materials for a blueprint
    /// </summary>
    public class BlueprintTemplate : ResourceBasedLocalizedEDName<BlueprintTemplate>
    {
        static BlueprintTemplate()
        {
            resourceManager = Properties.Modifications.ResourceManager;
            resourceManager.IgnoreCase = true;

            None = new BlueprintTemplate("None", null);
            _ = new BlueprintTemplate("ArmourAdvanced", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1), new MaterialAmount(Material.ConductiveComponents, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1), new MaterialAmount(Material.ConductiveComponents, 1), new MaterialAmount(Material.HighDensityComposites, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Germanium, 1), new MaterialAmount(Material.ConductiveCeramics, 1), new MaterialAmount(Material.ProprietaryComposites, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Tin, 1), new MaterialAmount(Material.ConductiveCeramics, 1), new MaterialAmount(Material.MilitaryGradeAlloys, 1) } },
            });
            _ = new BlueprintTemplate("ArmourExplosive", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1), new MaterialAmount(Material.Zinc, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Vanadium, 1), new MaterialAmount(Material.Zirconium, 1), new MaterialAmount(Material.SalvagedAlloys, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Tungsten, 1), new MaterialAmount(Material.Mercury, 1), new MaterialAmount(Material.GalvanisingAlloys, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Molybdenum, 1), new MaterialAmount(Material.Ruthenium, 1), new MaterialAmount(Material.PhaseAlloys, 1) } },
            });
            _ = new BlueprintTemplate("ArmourHeavyDuty", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1), new MaterialAmount(Material.ShieldEmitters, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1), new MaterialAmount(Material.ShieldEmitters, 1), new MaterialAmount(Material.HighDensityComposites, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Vanadium, 1), new MaterialAmount(Material.ShieldingSensors, 1), new MaterialAmount(Material.ProprietaryComposites, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Tungsten, 1), new MaterialAmount(Material.CompoundShielding, 1), new MaterialAmount(Material.CoreDynamicsComposites, 1) } },
            });
            _ = new BlueprintTemplate("ArmourKinetic", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1), new MaterialAmount(Material.Vanadium, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Vanadium, 1), new MaterialAmount(Material.SalvagedAlloys, 1), new MaterialAmount(Material.HighDensityComposites, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Tungsten, 1), new MaterialAmount(Material.GalvanisingAlloys, 1), new MaterialAmount(Material.ProprietaryComposites, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Molybdenum, 1), new MaterialAmount(Material.PhaseAlloys, 1), new MaterialAmount(Material.CoreDynamicsComposites, 1) } },
            });
            _ = new BlueprintTemplate("ArmourThermic", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.HeatConductionWiring, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1), new MaterialAmount(Material.HeatDispersionPlate, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Vanadium, 1), new MaterialAmount(Material.SalvagedAlloys, 1), new MaterialAmount(Material.HeatExchangers, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Tungsten, 1), new MaterialAmount(Material.GalvanisingAlloys, 1), new MaterialAmount(Material.HeatVanes, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Molybdenum, 1), new MaterialAmount(Material.PhaseAlloys, 1), new MaterialAmount(Material.ProtoHeatRadiators, 1) } },
            });
            _ = new BlueprintTemplate("EngineDirty", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.SpecialisedLegacyFirmware, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.SpecialisedLegacyFirmware, 1), new MaterialAmount(Material.MechanicalEquipment, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.SpecialisedLegacyFirmware, 1), new MaterialAmount(Material.Chromium, 1), new MaterialAmount(Material.MechanicalComponents, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.ModifiedConsumerFirmware, 1), new MaterialAmount(Material.Selenium, 1), new MaterialAmount(Material.ConfigurableComponents, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.CrackedIndustrialFirmware, 1), new MaterialAmount(Material.Cadmium, 1), new MaterialAmount(Material.PharmaceuticalIsolators, 1) } },
            });
            _ = new BlueprintTemplate("EngineReinforced", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.HeatConductionWiring, 1), new MaterialAmount(Material.Vanadium, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.HeatConductionWiring, 1), new MaterialAmount(Material.Vanadium, 1), new MaterialAmount(Material.ShieldingSensors, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.HeatDispersionPlate, 1), new MaterialAmount(Material.HighDensityComposites, 1), new MaterialAmount(Material.CompoundShielding, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.HeatExchangers, 1), new MaterialAmount(Material.ProprietaryComposites, 1), new MaterialAmount(Material.ImperialShielding, 1) } },
            });
            _ = new BlueprintTemplate("EngineTuned", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Sulphur, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.SpecialisedLegacyFirmware, 1), new MaterialAmount(Material.ConductiveComponents, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.SpecialisedLegacyFirmware, 1), new MaterialAmount(Material.ConductiveComponents, 1), new MaterialAmount(Material.UnexpectedEmissionData, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.ModifiedConsumerFirmware, 1), new MaterialAmount(Material.ConductiveCeramics, 1), new MaterialAmount(Material.DecodedEmissionData, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.ConductiveCeramics, 1), new MaterialAmount(Material.Tin, 1), new MaterialAmount(Material.AbnormalCompactEmissionData, 1) } },
            });
            _ = new BlueprintTemplate("FSDFastBoot", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.GridResistors, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.GridResistors, 1), new MaterialAmount(Material.Chromium, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.GridResistors, 1), new MaterialAmount(Material.HeatDispersionPlate, 1), new MaterialAmount(Material.Selenium, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.HybridCapacitors, 1), new MaterialAmount(Material.HeatExchangers, 1), new MaterialAmount(Material.Cadmium, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.ElectrochemicalArrays, 1), new MaterialAmount(Material.HeatVanes, 1), new MaterialAmount(Material.Tellurium, 1) } },
            });
            _ = new BlueprintTemplate("FSDLongRange", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.AtypicalDisruptedWakeEchoes, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.AtypicalDisruptedWakeEchoes, 1), new MaterialAmount(Material.ChemicalProcessors, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Phosphorus, 1), new MaterialAmount(Material.ChemicalProcessors, 1), new MaterialAmount(Material.StrangeWakeSolutions, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Manganese, 1), new MaterialAmount(Material.ChemicalDistillery, 1), new MaterialAmount(Material.EccentricHyperspaceTrajectories, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Arsenic, 1), new MaterialAmount(Material.ChemicalManipulators, 1), new MaterialAmount(Material.DataminedWakeExceptions, 1) } },
            });
            _ = new BlueprintTemplate("FSDShielded", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1), new MaterialAmount(Material.ShieldEmitters, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1), new MaterialAmount(Material.Zinc, 1), new MaterialAmount(Material.ShieldingSensors, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Vanadium, 1), new MaterialAmount(Material.HighDensityComposites, 1), new MaterialAmount(Material.CompoundShielding, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Tungsten, 1), new MaterialAmount(Material.ProprietaryComposites, 1), new MaterialAmount(Material.ImperialShielding, 1) } },
            });
            _ = new BlueprintTemplate("FSDinterdictorExpanded", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalScrap, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.UnusualEncryptedFiles, 1), new MaterialAmount(Material.MechanicalEquipment, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.GridResistors, 1), new MaterialAmount(Material.TaggedEncryptionCodes, 1), new MaterialAmount(Material.MechanicalComponents, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalEquipment, 1), new MaterialAmount(Material.StrangeWakeSolutions, 1), new MaterialAmount(Material.DivergentScanData, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalComponents, 1), new MaterialAmount(Material.EccentricHyperspaceTrajectories, 1), new MaterialAmount(Material.ClassifiedScanFragment, 1) } },
            });
            _ = new BlueprintTemplate("FSDinterdictorLongRange", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.UnusualEncryptedFiles, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.AtypicalDisruptedWakeEchoes, 1), new MaterialAmount(Material.TaggedEncryptionCodes, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.AnomalousBulkScanData, 1), new MaterialAmount(Material.AnomalousFSDTelemetry, 1), new MaterialAmount(Material.OpenSymmetricKeys, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.UnidentifiedScanArchives, 1), new MaterialAmount(Material.StrangeWakeSolutions, 1), new MaterialAmount(Material.AtypicalEncryptionArchives, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.ClassifiedScanDatabanks, 1), new MaterialAmount(Material.EccentricHyperspaceTrajectories, 1), new MaterialAmount(Material.AdaptiveEncryptorsCapture, 1) } },
            });
            _ = new BlueprintTemplate("HullReinforcementAdvanced", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1), new MaterialAmount(Material.ConductiveComponents, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1), new MaterialAmount(Material.ConductiveComponents, 1), new MaterialAmount(Material.HighDensityComposites, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Germanium, 1), new MaterialAmount(Material.ConductiveCeramics, 1), new MaterialAmount(Material.ProprietaryComposites, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.ConductiveCeramics, 1), new MaterialAmount(Material.Tin, 1), new MaterialAmount(Material.MilitaryGradeAlloys, 1) } },
            });
            _ = new BlueprintTemplate("HullReinforcementExplosive", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1), new MaterialAmount(Material.Zinc, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.SalvagedAlloys, 1), new MaterialAmount(Material.Vanadium, 1), new MaterialAmount(Material.Zirconium, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.GalvanisingAlloys, 1), new MaterialAmount(Material.Tungsten, 1), new MaterialAmount(Material.Mercury, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.PhaseAlloys, 1), new MaterialAmount(Material.Molybdenum, 1), new MaterialAmount(Material.Ruthenium, 1) } },
            });
            _ = new BlueprintTemplate("HullReinforcementHeavyDuty", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1), new MaterialAmount(Material.ShieldEmitters, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1), new MaterialAmount(Material.ShieldEmitters, 1), new MaterialAmount(Material.HighDensityComposites, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Vanadium, 1), new MaterialAmount(Material.ShieldingSensors, 1), new MaterialAmount(Material.ProprietaryComposites, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Tungsten, 1), new MaterialAmount(Material.CompoundShielding, 1), new MaterialAmount(Material.CoreDynamicsComposites, 1) } },
            });
            _ = new BlueprintTemplate("HullReinforcementKinetic", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1), new MaterialAmount(Material.Vanadium, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.SalvagedAlloys, 1), new MaterialAmount(Material.Vanadium, 1), new MaterialAmount(Material.HighDensityComposites, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.GalvanisingAlloys, 1), new MaterialAmount(Material.Tungsten, 1), new MaterialAmount(Material.ProprietaryComposites, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.PhaseAlloys, 1), new MaterialAmount(Material.Molybdenum, 1), new MaterialAmount(Material.CoreDynamicsComposites, 1) } },
            });
            _ = new BlueprintTemplate("HullReinforcementThermic", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.HeatConductionWiring, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1), new MaterialAmount(Material.HeatDispersionPlate, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.SalvagedAlloys, 1), new MaterialAmount(Material.Vanadium, 1), new MaterialAmount(Material.HeatExchangers, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.GalvanisingAlloys, 1), new MaterialAmount(Material.Tungsten, 1), new MaterialAmount(Material.HeatVanes, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.PhaseAlloys, 1), new MaterialAmount(Material.Molybdenum, 1), new MaterialAmount(Material.ProtoHeatRadiators, 1) } },
            });
            _ = new BlueprintTemplate("MiscChaffCapacity", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalScrap, 1) } },
            });
            _ = new BlueprintTemplate("MiscHeatSinkCapacity", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalScrap, 1), new MaterialAmount(Material.Vanadium, 1), new MaterialAmount(Material.Niobium, 1) } },
            });
            _ = new BlueprintTemplate("MiscLightWeight", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Phosphorus, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.SalvagedAlloys, 1), new MaterialAmount(Material.Manganese, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.SalvagedAlloys, 1), new MaterialAmount(Material.Manganese, 1), new MaterialAmount(Material.ConductiveCeramics, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.ConductiveComponents, 1), new MaterialAmount(Material.PhaseAlloys, 1), new MaterialAmount(Material.ProtoLightAlloys, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.ConductiveCeramics, 1), new MaterialAmount(Material.ProtoLightAlloys, 1), new MaterialAmount(Material.ProtoRadiolicAlloys, 1) } },
            });
            _ = new BlueprintTemplate("MiscPointDefenseCapacity", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalScrap, 1), new MaterialAmount(Material.Vanadium, 1), new MaterialAmount(Material.Niobium, 1) } },
            });
            _ = new BlueprintTemplate("MiscReinforced", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1), new MaterialAmount(Material.ShieldEmitters, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1), new MaterialAmount(Material.ShieldEmitters, 1), new MaterialAmount(Material.Tungsten, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Zinc, 1), new MaterialAmount(Material.Tungsten, 1), new MaterialAmount(Material.Molybdenum, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.HighDensityComposites, 1), new MaterialAmount(Material.Molybdenum, 1), new MaterialAmount(Material.Technetium, 1) } },
            });
            _ = new BlueprintTemplate("MiscShielded", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.WornShieldEmitters, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1), new MaterialAmount(Material.ShieldEmitters, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1), new MaterialAmount(Material.ShieldEmitters, 1), new MaterialAmount(Material.HighDensityComposites, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Vanadium, 1), new MaterialAmount(Material.ShieldingSensors, 1), new MaterialAmount(Material.ProprietaryComposites, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Tungsten, 1), new MaterialAmount(Material.CompoundShielding, 1), new MaterialAmount(Material.CoreDynamicsComposites, 1) } },
            });
            _ = new BlueprintTemplate("PowerDistributorHighCapacity", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Sulphur, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.SpecialisedLegacyFirmware, 1), new MaterialAmount(Material.Chromium, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.SpecialisedLegacyFirmware, 1), new MaterialAmount(Material.Chromium, 1), new MaterialAmount(Material.HighDensityComposites, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.ModifiedConsumerFirmware, 1), new MaterialAmount(Material.Selenium, 1), new MaterialAmount(Material.ProprietaryComposites, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.CrackedIndustrialFirmware, 1), new MaterialAmount(Material.ProprietaryComposites, 1), new MaterialAmount(Material.MilitarySupercapacitors, 1) } },
            });
            _ = new BlueprintTemplate("PowerDistributorHighFrequency", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.SpecialisedLegacyFirmware, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.SpecialisedLegacyFirmware, 1), new MaterialAmount(Material.ChemicalProcessors, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.GridResistors, 1), new MaterialAmount(Material.ModifiedConsumerFirmware, 1), new MaterialAmount(Material.ChemicalDistillery, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.HybridCapacitors, 1), new MaterialAmount(Material.CrackedIndustrialFirmware, 1), new MaterialAmount(Material.ChemicalManipulators, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.CrackedIndustrialFirmware, 1), new MaterialAmount(Material.ChemicalManipulators, 1), new MaterialAmount(Material.ExquisiteFocusCrystals, 1) } },
            });
            _ = new BlueprintTemplate("PowerDistributorPriorityEngines", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Sulphur, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Sulphur, 1), new MaterialAmount(Material.ConductiveComponents, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.AnomalousBulkScanData, 1), new MaterialAmount(Material.Chromium, 1), new MaterialAmount(Material.ElectrochemicalArrays, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.UnidentifiedScanArchives, 1), new MaterialAmount(Material.Selenium, 1), new MaterialAmount(Material.PolymerCapacitors, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.ClassifiedScanDatabanks, 1), new MaterialAmount(Material.Cadmium, 1), new MaterialAmount(Material.MilitarySupercapacitors, 1) } },
            });
            _ = new BlueprintTemplate("PowerDistributorPrioritySystems", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Sulphur, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Sulphur, 1), new MaterialAmount(Material.ConductiveComponents, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.AnomalousBulkScanData, 1), new MaterialAmount(Material.Chromium, 1), new MaterialAmount(Material.ElectrochemicalArrays, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.UnidentifiedScanArchives, 1), new MaterialAmount(Material.Selenium, 1), new MaterialAmount(Material.PolymerCapacitors, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.ClassifiedScanDatabanks, 1), new MaterialAmount(Material.Cadmium, 1), new MaterialAmount(Material.MilitarySupercapacitors, 1) } },
            });
            _ = new BlueprintTemplate("PowerDistributorPriorityWeapons", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Sulphur, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Sulphur, 1), new MaterialAmount(Material.ConductiveComponents, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.AnomalousBulkScanData, 1), new MaterialAmount(Material.HybridCapacitors, 1), new MaterialAmount(Material.Selenium, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.UnidentifiedScanArchives, 1), new MaterialAmount(Material.ElectrochemicalArrays, 1), new MaterialAmount(Material.Cadmium, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.ClassifiedScanDatabanks, 1), new MaterialAmount(Material.PolymerCapacitors, 1), new MaterialAmount(Material.Tellurium, 1) } },
            });
            _ = new BlueprintTemplate("PowerDistributorShielded", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.WornShieldEmitters, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1), new MaterialAmount(Material.ShieldEmitters, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1), new MaterialAmount(Material.ShieldEmitters, 1), new MaterialAmount(Material.HighDensityComposites, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Vanadium, 1), new MaterialAmount(Material.ShieldingSensors, 1), new MaterialAmount(Material.ProprietaryComposites, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Tungsten, 1), new MaterialAmount(Material.CompoundShielding, 1), new MaterialAmount(Material.CoreDynamicsComposites, 1) } },
            });
            _ = new BlueprintTemplate("PowerPlantArmoured", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.WornShieldEmitters, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1), new MaterialAmount(Material.ShieldEmitters, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1), new MaterialAmount(Material.ShieldEmitters, 1), new MaterialAmount(Material.HighDensityComposites, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Vanadium, 1), new MaterialAmount(Material.ShieldingSensors, 1), new MaterialAmount(Material.ProprietaryComposites, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Tungsten, 1), new MaterialAmount(Material.CompoundShielding, 1), new MaterialAmount(Material.CoreDynamicsComposites, 1) } },
            });
            _ = new BlueprintTemplate("PowerPlantBoosted", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Sulphur, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.HeatConductionWiring, 1), new MaterialAmount(Material.ConductiveComponents, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.HeatConductionWiring, 1), new MaterialAmount(Material.ConductiveComponents, 1), new MaterialAmount(Material.Selenium, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.HeatDispersionPlate, 1), new MaterialAmount(Material.ConductiveCeramics, 1), new MaterialAmount(Material.Cadmium, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.ConductiveCeramics, 1), new MaterialAmount(Material.ChemicalManipulators, 1), new MaterialAmount(Material.Tellurium, 1) } },
            });
            _ = new BlueprintTemplate("PowerPlantStealth", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1), new MaterialAmount(Material.IrregularEmissionData, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1), new MaterialAmount(Material.IrregularEmissionData, 1), new MaterialAmount(Material.HeatExchangers, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Germanium, 1), new MaterialAmount(Material.UnexpectedEmissionData, 1), new MaterialAmount(Material.HeatVanes, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Niobium, 1), new MaterialAmount(Material.DecodedEmissionData, 1), new MaterialAmount(Material.ProtoHeatRadiators, 1) } },
            });
            _ = new BlueprintTemplate("SensorExpanded", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalScrap, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalScrap, 1), new MaterialAmount(Material.Germanium, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalScrap, 1), new MaterialAmount(Material.Germanium, 1), new MaterialAmount(Material.PhaseAlloys, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalEquipment, 1), new MaterialAmount(Material.Niobium, 1), new MaterialAmount(Material.ProtoLightAlloys, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalComponents, 1), new MaterialAmount(Material.Tin, 1), new MaterialAmount(Material.ProtoRadiolicAlloys, 1) } },
            });
            _ = new BlueprintTemplate("SensorFastScan", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Phosphorus, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Phosphorus, 1), new MaterialAmount(Material.FlawedFocusCrystals, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Phosphorus, 1), new MaterialAmount(Material.FlawedFocusCrystals, 1), new MaterialAmount(Material.OpenSymmetricKeys, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Manganese, 1), new MaterialAmount(Material.FocusCrystals, 1), new MaterialAmount(Material.AtypicalEncryptionArchives, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Arsenic, 1), new MaterialAmount(Material.RefinedFocusCrystals, 1), new MaterialAmount(Material.AdaptiveEncryptorsCapture, 1) } },
            });
            _ = new BlueprintTemplate("SensorLightWeight", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Phosphorus, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.SalvagedAlloys, 1), new MaterialAmount(Material.Manganese, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.SalvagedAlloys, 1), new MaterialAmount(Material.Manganese, 1), new MaterialAmount(Material.ConductiveCeramics, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.ConductiveComponents, 1), new MaterialAmount(Material.PhaseAlloys, 1), new MaterialAmount(Material.ProtoLightAlloys, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.ConductiveCeramics, 1), new MaterialAmount(Material.ProtoLightAlloys, 1), new MaterialAmount(Material.ProtoRadiolicAlloys, 1) } },
            });
            _ = new BlueprintTemplate("SensorLongRange", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1), new MaterialAmount(Material.HybridCapacitors, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1), new MaterialAmount(Material.HybridCapacitors, 1), new MaterialAmount(Material.UnexpectedEmissionData, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Germanium, 1), new MaterialAmount(Material.ElectrochemicalArrays, 1), new MaterialAmount(Material.DecodedEmissionData, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Niobium, 1), new MaterialAmount(Material.PolymerCapacitors, 1), new MaterialAmount(Material.AbnormalCompactEmissionData, 1) } },
            });
            _ = new BlueprintTemplate("SensorWideAngle", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalScrap, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalScrap, 1), new MaterialAmount(Material.Germanium, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalScrap, 1), new MaterialAmount(Material.Germanium, 1), new MaterialAmount(Material.ClassifiedScanDatabanks, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalEquipment, 1), new MaterialAmount(Material.Niobium, 1), new MaterialAmount(Material.DivergentScanData, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalComponents, 1), new MaterialAmount(Material.Tin, 1), new MaterialAmount(Material.ClassifiedScanFragment, 1) } },
            });
            _ = new BlueprintTemplate("ShieldBoosterExplosive", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1), new MaterialAmount(Material.ConductiveComponents, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1), new MaterialAmount(Material.ConductiveComponents, 1), new MaterialAmount(Material.FocusCrystals, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Germanium, 1), new MaterialAmount(Material.UntypicalShieldScans, 1), new MaterialAmount(Material.RefinedFocusCrystals, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Niobium, 1), new MaterialAmount(Material.AberrantShieldPatternAnalysis, 1), new MaterialAmount(Material.ExquisiteFocusCrystals, 1) } },
            });
            _ = new BlueprintTemplate("ShieldBoosterHeavyDuty", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.GridResistors, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.DistortedShieldCycleRecordings, 1), new MaterialAmount(Material.HybridCapacitors, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.DistortedShieldCycleRecordings, 1), new MaterialAmount(Material.HybridCapacitors, 1), new MaterialAmount(Material.Niobium, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.InconsistentShieldSoakAnalysis, 1), new MaterialAmount(Material.ElectrochemicalArrays, 1), new MaterialAmount(Material.Tin, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.UntypicalShieldScans, 1), new MaterialAmount(Material.PolymerCapacitors, 1), new MaterialAmount(Material.Antimony, 1) } },
            });
            _ = new BlueprintTemplate("ShieldBoosterKinetic", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.GridResistors, 1), new MaterialAmount(Material.Germanium, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.SalvagedAlloys, 1), new MaterialAmount(Material.HybridCapacitors, 1), new MaterialAmount(Material.FocusCrystals, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.GalvanisingAlloys, 1), new MaterialAmount(Material.UntypicalShieldScans, 1), new MaterialAmount(Material.RefinedFocusCrystals, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.PhaseAlloys, 1), new MaterialAmount(Material.AberrantShieldPatternAnalysis, 1), new MaterialAmount(Material.ExquisiteFocusCrystals, 1) } },
            });
            _ = new BlueprintTemplate("ShieldBoosterResistive", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Phosphorus, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Phosphorus, 1), new MaterialAmount(Material.ConductiveComponents, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Phosphorus, 1), new MaterialAmount(Material.ConductiveComponents, 1), new MaterialAmount(Material.FocusCrystals, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Manganese, 1), new MaterialAmount(Material.ConductiveCeramics, 1), new MaterialAmount(Material.RefinedFocusCrystals, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.ConductiveCeramics, 1), new MaterialAmount(Material.RefinedFocusCrystals, 1), new MaterialAmount(Material.ImperialShielding, 1) } },
            });
            _ = new BlueprintTemplate("ShieldBoosterThermic", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.HeatConductionWiring, 1), new MaterialAmount(Material.Germanium, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.HeatConductionWiring, 1), new MaterialAmount(Material.HeatDispersionPlate, 1), new MaterialAmount(Material.FocusCrystals, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.HeatDispersionPlate, 1), new MaterialAmount(Material.UntypicalShieldScans, 1), new MaterialAmount(Material.RefinedFocusCrystals, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.HeatExchangers, 1), new MaterialAmount(Material.AberrantShieldPatternAnalysis, 1), new MaterialAmount(Material.ExquisiteFocusCrystals, 1) } },
            });
            _ = new BlueprintTemplate("ShieldCellBankRapid", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Sulphur, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.GridResistors, 1), new MaterialAmount(Material.Chromium, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Sulphur, 1), new MaterialAmount(Material.HybridCapacitors, 1), new MaterialAmount(Material.PrecipitatedAlloys, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Chromium, 1), new MaterialAmount(Material.ElectrochemicalArrays, 1), new MaterialAmount(Material.ThermicAlloys, 1) } },
            });
            _ = new BlueprintTemplate("ShieldCellBankSpecialised", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.SpecialisedLegacyFirmware, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.SpecialisedLegacyFirmware, 1), new MaterialAmount(Material.ConductiveComponents, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.ExceptionalScrambledEmissionData, 1), new MaterialAmount(Material.ConductiveComponents, 1), new MaterialAmount(Material.CrackedIndustrialFirmware, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.ConductiveComponents, 1), new MaterialAmount(Material.CrackedIndustrialFirmware, 1), new MaterialAmount(Material.Yttrium, 1) } },
            });
            _ = new BlueprintTemplate("ShieldGeneratorKinetic", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.DistortedShieldCycleRecordings, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.DistortedShieldCycleRecordings, 1), new MaterialAmount(Material.ModifiedConsumerFirmware, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.DistortedShieldCycleRecordings, 1), new MaterialAmount(Material.ModifiedConsumerFirmware, 1), new MaterialAmount(Material.Selenium, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.InconsistentShieldSoakAnalysis, 1), new MaterialAmount(Material.FocusCrystals, 1), new MaterialAmount(Material.Mercury, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.UntypicalShieldScans, 1), new MaterialAmount(Material.RefinedFocusCrystals, 1), new MaterialAmount(Material.Ruthenium, 1) } },
            });
            _ = new BlueprintTemplate("ShieldGeneratorOptimised", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.DistortedShieldCycleRecordings, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.DistortedShieldCycleRecordings, 1), new MaterialAmount(Material.Germanium, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.DistortedShieldCycleRecordings, 1), new MaterialAmount(Material.Germanium, 1), new MaterialAmount(Material.PrecipitatedAlloys, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.InconsistentShieldSoakAnalysis, 1), new MaterialAmount(Material.Niobium, 1), new MaterialAmount(Material.ThermicAlloys, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.UntypicalShieldScans, 1), new MaterialAmount(Material.Tin, 1), new MaterialAmount(Material.MilitaryGradeAlloys, 1) } },
            });
            _ = new BlueprintTemplate("ShieldGeneratorReinforced", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Phosphorus, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Phosphorus, 1), new MaterialAmount(Material.ConductiveComponents, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Phosphorus, 1), new MaterialAmount(Material.ConductiveComponents, 1), new MaterialAmount(Material.MechanicalComponents, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Manganese, 1), new MaterialAmount(Material.ConductiveCeramics, 1), new MaterialAmount(Material.ConfigurableComponents, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Arsenic, 1), new MaterialAmount(Material.ConductivePolymers, 1), new MaterialAmount(Material.ImprovisedComponents, 1) } },
            });
            _ = new BlueprintTemplate("ShieldGeneratorThermic", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.DistortedShieldCycleRecordings, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.DistortedShieldCycleRecordings, 1), new MaterialAmount(Material.Germanium, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.DistortedShieldCycleRecordings, 1), new MaterialAmount(Material.Germanium, 1), new MaterialAmount(Material.Selenium, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.InconsistentShieldSoakAnalysis, 1), new MaterialAmount(Material.FocusCrystals, 1), new MaterialAmount(Material.Mercury, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.UntypicalShieldScans, 1), new MaterialAmount(Material.RefinedFocusCrystals, 1), new MaterialAmount(Material.Ruthenium, 1) } },
            });
            _ = new BlueprintTemplate("WeaponDoubleShot", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1), new MaterialAmount(Material.MechanicalEquipment, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Carbon, 1), new MaterialAmount(Material.MechanicalEquipment, 1), new MaterialAmount(Material.CrackedIndustrialFirmware, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Vanadium, 1), new MaterialAmount(Material.MechanicalComponents, 1), new MaterialAmount(Material.SecurityFirmwarePatch, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.HighDensityComposites, 1), new MaterialAmount(Material.ConfigurableComponents, 1), new MaterialAmount(Material.ModifiedEmbeddedFirmware, 1) } },
            });
            _ = new BlueprintTemplate("WeaponEfficient", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Sulphur, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Sulphur, 1), new MaterialAmount(Material.HeatDispersionPlate, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Chromium, 1), new MaterialAmount(Material.ExceptionalScrambledEmissionData, 1), new MaterialAmount(Material.HeatExchangers, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Selenium, 1), new MaterialAmount(Material.IrregularEmissionData, 1), new MaterialAmount(Material.HeatVanes, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Cadmium, 1), new MaterialAmount(Material.UnexpectedEmissionData, 1), new MaterialAmount(Material.ProtoHeatRadiators, 1) } },
            });
            _ = new BlueprintTemplate("WeaponFocused", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1), new MaterialAmount(Material.ConductiveComponents, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Iron, 1), new MaterialAmount(Material.Chromium, 1), new MaterialAmount(Material.ConductiveCeramics, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Germanium, 1), new MaterialAmount(Material.FocusCrystals, 1), new MaterialAmount(Material.PolymerCapacitors, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Niobium, 1), new MaterialAmount(Material.RefinedFocusCrystals, 1), new MaterialAmount(Material.MilitarySupercapacitors, 1) } },
            });
            _ = new BlueprintTemplate("WeaponHighCapacity", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalScrap, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalScrap, 1), new MaterialAmount(Material.Vanadium, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalScrap, 1), new MaterialAmount(Material.Vanadium, 1), new MaterialAmount(Material.Niobium, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalEquipment, 1), new MaterialAmount(Material.HighDensityComposites, 1), new MaterialAmount(Material.Tin, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalComponents, 1), new MaterialAmount(Material.ProprietaryComposites, 1), new MaterialAmount(Material.MilitarySupercapacitors, 1) } },
            });
            _ = new BlueprintTemplate("WeaponLightWeight", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Phosphorus, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Manganese, 1),  new MaterialAmount(Material.SalvagedAlloys, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Manganese, 1),  new MaterialAmount(Material.SalvagedAlloys, 1),   new MaterialAmount(Material.ConductiveCeramics, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.PhaseAlloys, 1),  new MaterialAmount(Material.ConductiveComponents, 1),   new MaterialAmount(Material.ProtoLightAlloys, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.ConductiveCeramics, 1),  new MaterialAmount(Material.ProtoLightAlloys, 1),   new MaterialAmount(Material.ProtoRadiolicAlloys, 1) } },
            });
            _ = new BlueprintTemplate("WeaponLongRange", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Sulphur, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Sulphur, 1), new MaterialAmount(Material.ModifiedConsumerFirmware, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Sulphur, 1), new MaterialAmount(Material.ModifiedConsumerFirmware, 1), new MaterialAmount(Material.FocusCrystals, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FocusCrystals, 1), new MaterialAmount(Material.ModifiedConsumerFirmware, 1), new MaterialAmount(Material.ConductivePolymers, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.ThermicAlloys, 1), new MaterialAmount(Material.CrackedIndustrialFirmware, 1), new MaterialAmount(Material.BiotechConductors, 1) } },
            });
            _ = new BlueprintTemplate("WeaponOvercharged", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1), new MaterialAmount(Material.ConductiveComponents, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1), new MaterialAmount(Material.ConductiveComponents, 1), new MaterialAmount(Material.ElectrochemicalArrays, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Zinc, 1), new MaterialAmount(Material.ConductiveCeramics, 1), new MaterialAmount(Material.PolymerCapacitors, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Zirconium, 1), new MaterialAmount(Material.ConductivePolymers, 1), new MaterialAmount(Material.ModifiedEmbeddedFirmware, 1) } },
            });
            _ = new BlueprintTemplate("WeaponRapidFire", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalScrap, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.MechanicalScrap, 1), new MaterialAmount(Material.HeatDispersionPlate, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.SpecialisedLegacyFirmware, 1), new MaterialAmount(Material.MechanicalEquipment, 1), new MaterialAmount(Material.PrecipitatedAlloys, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.ModifiedConsumerFirmware, 1), new MaterialAmount(Material.MechanicalComponents, 1), new MaterialAmount(Material.ThermicAlloys, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.PrecipitatedAlloys, 1), new MaterialAmount(Material.ConfigurableComponents, 1), new MaterialAmount(Material.Technetium, 1) } },
            });
            _ = new BlueprintTemplate("WeaponShortRange", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1), new MaterialAmount(Material.ModifiedConsumerFirmware, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1), new MaterialAmount(Material.ModifiedConsumerFirmware, 1), new MaterialAmount(Material.ElectrochemicalArrays, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.ElectrochemicalArrays, 1), new MaterialAmount(Material.ModifiedConsumerFirmware, 1), new MaterialAmount(Material.ConductivePolymers, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.CrackedIndustrialFirmware, 1), new MaterialAmount(Material.ConfigurableComponents, 1), new MaterialAmount(Material.BiotechConductors, 1) } },
            });
            _ = new BlueprintTemplate("WeaponSturdy", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1), new MaterialAmount(Material.ShieldEmitters, 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.Nickel, 1), new MaterialAmount(Material.ShieldEmitters, 1), new MaterialAmount(Material.Tungsten, 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.Zinc, 1), new MaterialAmount(Material.Molybdenum, 1), new MaterialAmount(Material.Tungsten, 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.Molybdenum, 1), new MaterialAmount(Material.Technetium, 1), new MaterialAmount(Material.HighDensityComposites, 1) } },
            });
            _ = new BlueprintTemplate("DecorativeGreen", new Dictionary<int, List<MaterialAmount>>()
            {
                { 5, new List<MaterialAmount>() }
            });
            _ = new BlueprintTemplate("DecorativeRed", new Dictionary<int, List<MaterialAmount>>()
            {
                { 5, new List<MaterialAmount>() }
            });
            _ = new BlueprintTemplate("DecorativeYellow", new Dictionary<int, List<MaterialAmount>>()
            {
                { 5, new List<MaterialAmount>() }
            });
        }
        public static readonly BlueprintTemplate None;

        // dummy used to ensure that the static constructor has run
        public BlueprintTemplate() : this("", null)
        { }

        // Not intended to be user facing
        public Dictionary<int, List<MaterialAmount>> byGrade { get; private set; }

        public BlueprintTemplate(string edname, Dictionary<int, List<MaterialAmount>> materialsByGrade) : base(edname, normalizeEDName(edname))
        {
            this.byGrade = materialsByGrade;
        }

        private static string normalizeEDName(string edname)
        {
            string normalizedName = edname
                .Replace("_", "")
                .Replace("CargoScanner", "Misc") // CargoScanner uses the `Misc` template family.
                .Replace("ChaffLauncher", "Misc") // ChaffLauncher uses the `Misc` template family.
                .Replace("FuelTransferLimpet", "Misc") // FuelTransferLimpet uses the `Misc` template family.
                .Replace("HatchBreakerLimpet", "Misc") // HatchBreakerLimpet uses the `Misc` template family.
                .Replace("HeatSinkLauncher", "Misc") // HeatSinkLauncher uses the `Misc` template family.
                .Replace("KillWarrantScanner", "Sensor") // KillWarrantScanner uses the `Sensor` template family
                .Replace("LifeSupport", "Misc") // LifeSupport uses the `Misc` template family
                .Replace("PointDefence", "Misc") // PointDefence uses the `Misc` template family
                .Replace("SensorMisc", "Sensor") // SensorMisc uses the `Sensor` template family
                .Replace("SensorSensor", "Sensor") // SensorSensor uses the `Sensor` template family.
                .Replace("SensorWakeScanner", "Sensor") // SensorWakeScanner uses the `Sensor` template family.
                ;
            return normalizedName;
        }

        new public static BlueprintTemplate FromEDName(string edname)
        {
            if (string.IsNullOrEmpty(edname)) { return null; }
            string normalizedEDName = normalizeEDName(edname);
            BlueprintTemplate result = ResourceBasedLocalizedEDName<BlueprintTemplate>.FromEDName(normalizedEDName);
            return result;
        }
    }
}

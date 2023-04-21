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
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1), new MaterialAmount(Material.FromEDName("highdensitycomposites"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("germanium"), 1), new MaterialAmount(Material.FromEDName("conductiveceramics"), 1), new MaterialAmount(Material.FromEDName("fedproprietarycomposites"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("tin"), 1), new MaterialAmount(Material.FromEDName("conductiveceramics"), 1), new MaterialAmount(Material.FromEDName("militarygradealloys"), 1) } },
            });
            _ = new BlueprintTemplate("ArmourExplosive", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1), new MaterialAmount(Material.FromEDName("zinc"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("vanadium"), 1), new MaterialAmount(Material.FromEDName("zirconium"), 1), new MaterialAmount(Material.FromEDName("salvagedalloys"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("tungsten"), 1), new MaterialAmount(Material.FromEDName("mercury"), 1), new MaterialAmount(Material.FromEDName("galvanisingalloys"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("molybdenum"), 1), new MaterialAmount(Material.FromEDName("ruthenium"), 1), new MaterialAmount(Material.FromEDName("phasealloys"), 1) } },
            });
            _ = new BlueprintTemplate("ArmourHeavyDuty", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1), new MaterialAmount(Material.FromEDName("shieldemitters"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1), new MaterialAmount(Material.FromEDName("shieldemitters"), 1), new MaterialAmount(Material.FromEDName("highdensitycomposites"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("vanadium"), 1), new MaterialAmount(Material.FromEDName("shieldingsensors"), 1), new MaterialAmount(Material.FromEDName("fedproprietarycomposites"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("tungsten"), 1), new MaterialAmount(Material.FromEDName("compoundshielding"), 1), new MaterialAmount(Material.FromEDName("fedcorecomposites"), 1) } },
            });
            _ = new BlueprintTemplate("ArmourKinetic", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1), new MaterialAmount(Material.FromEDName("vanadium"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("vanadium"), 1), new MaterialAmount(Material.FromEDName("salvagedalloys"), 1), new MaterialAmount(Material.FromEDName("highdensitycomposites"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("tungsten"), 1), new MaterialAmount(Material.FromEDName("galvanisingalloys"), 1), new MaterialAmount(Material.FromEDName("fedproprietarycomposites"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("molybdenum"), 1), new MaterialAmount(Material.FromEDName("phasealloys"), 1), new MaterialAmount(Material.FromEDName("fedcorecomposites"), 1) } },
            });
            _ = new BlueprintTemplate("ArmourThermic", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("heatconductionwiring"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1), new MaterialAmount(Material.FromEDName("heatdispersionplate"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("vanadium"), 1), new MaterialAmount(Material.FromEDName("salvagedalloys"), 1), new MaterialAmount(Material.FromEDName("heatexchangers"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("tungsten"), 1), new MaterialAmount(Material.FromEDName("galvanisingalloys"), 1), new MaterialAmount(Material.FromEDName("heatvanes"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("molybdenum"), 1), new MaterialAmount(Material.FromEDName("phasealloys"), 1), new MaterialAmount(Material.FromEDName("protoheatradiators"), 1) } },
            });
            _ = new BlueprintTemplate("EngineDirty", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("legacyfirmware"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("legacyfirmware"), 1), new MaterialAmount(Material.FromEDName("mechanicalequipment"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("legacyfirmware"), 1), new MaterialAmount(Material.FromEDName("chromium"), 1), new MaterialAmount(Material.FromEDName("mechanicalcomponents"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("consumerfirmware"), 1), new MaterialAmount(Material.FromEDName("selenium"), 1), new MaterialAmount(Material.FromEDName("configurablecomponents"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("industrialfirmware"), 1), new MaterialAmount(Material.FromEDName("cadmium"), 1), new MaterialAmount(Material.FromEDName("pharmaceuticalisolators"), 1) } },
            });
            _ = new BlueprintTemplate("EngineReinforced", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("heatconductionwiring"), 1), new MaterialAmount(Material.FromEDName("vanadium"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("heatconductionwiring"), 1), new MaterialAmount(Material.FromEDName("vanadium"), 1), new MaterialAmount(Material.FromEDName("shieldingsensors"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("heatdispersionplate"), 1), new MaterialAmount(Material.FromEDName("highdensitycomposites"), 1), new MaterialAmount(Material.FromEDName("compoundshielding"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("heatexchangers"), 1), new MaterialAmount(Material.FromEDName("fedproprietarycomposites"), 1), new MaterialAmount(Material.FromEDName("imperialshielding"), 1) } },
            });
            _ = new BlueprintTemplate("EngineTuned", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("sulphur"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("legacyfirmware"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("legacyfirmware"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1), new MaterialAmount(Material.FromEDName("emissiondata"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("consumerfirmware"), 1), new MaterialAmount(Material.FromEDName("conductiveceramics"), 1), new MaterialAmount(Material.FromEDName("decodedemissiondata"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("conductiveceramics"), 1), new MaterialAmount(Material.FromEDName("tin"), 1), new MaterialAmount(Material.FromEDName("compactemissionsdata"), 1) } },
            });
            _ = new BlueprintTemplate("FSDFastBoot", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("gridresistors"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("gridresistors"), 1), new MaterialAmount(Material.FromEDName("chromium"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("gridresistors"), 1), new MaterialAmount(Material.FromEDName("heatdispersionplate"), 1), new MaterialAmount(Material.FromEDName("selenium"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("hybridcapacitors"), 1), new MaterialAmount(Material.FromEDName("heatexchangers"), 1), new MaterialAmount(Material.FromEDName("cadmium"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("electrochemicalarrays"), 1), new MaterialAmount(Material.FromEDName("heatvanes"), 1), new MaterialAmount(Material.FromEDName("tellurium"), 1) } },
            });
            _ = new BlueprintTemplate("FSDLongRange", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("disruptedwakeechoes"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("disruptedwakeechoes"), 1), new MaterialAmount(Material.FromEDName("chemicalprocessors"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phosphorus"), 1), new MaterialAmount(Material.FromEDName("chemicalprocessors"), 1), new MaterialAmount(Material.FromEDName("wakesolutions"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("manganese"), 1), new MaterialAmount(Material.FromEDName("chemicaldistillery"), 1), new MaterialAmount(Material.FromEDName("hyperspacetrajectories"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("arsenic"), 1), new MaterialAmount(Material.FromEDName("chemicalmanipulators"), 1), new MaterialAmount(Material.FromEDName("dataminedwake"), 1) } },
            });
            _ = new BlueprintTemplate("FSDShielded", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1), new MaterialAmount(Material.FromEDName("shieldemitters"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1), new MaterialAmount(Material.FromEDName("zinc"), 1), new MaterialAmount(Material.FromEDName("shieldingsensors"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("vanadium"), 1), new MaterialAmount(Material.FromEDName("highdensitycomposites"), 1), new MaterialAmount(Material.FromEDName("compoundshielding"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("tungsten"), 1), new MaterialAmount(Material.FromEDName("fedproprietarycomposites"), 1), new MaterialAmount(Material.FromEDName("imperialshielding"), 1) } },
            });
            _ = new BlueprintTemplate("FSDinterdictorExpanded", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalscrap"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("encryptedfiles"), 1), new MaterialAmount(Material.FromEDName("mechanicalequipment"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("gridresistors"), 1), new MaterialAmount(Material.FromEDName("encryptioncodes"), 1), new MaterialAmount(Material.FromEDName("mechanicalcomponents"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalequipment"), 1), new MaterialAmount(Material.FromEDName("wakesolutions"), 1), new MaterialAmount(Material.FromEDName("encodedscandata"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalcomponents"), 1), new MaterialAmount(Material.FromEDName("hyperspacetrajectories"), 1), new MaterialAmount(Material.FromEDName("classifiedscandata"), 1) } },
            });
            _ = new BlueprintTemplate("FSDinterdictorLongRange", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("encryptedfiles"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("disruptedwakeechoes"), 1), new MaterialAmount(Material.FromEDName("encryptioncodes"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("bulkscandata"), 1), new MaterialAmount(Material.FromEDName("fsdtelemetry"), 1), new MaterialAmount(Material.FromEDName("symmetrickeys"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("scanarchives"), 1), new MaterialAmount(Material.FromEDName("wakesolutions"), 1), new MaterialAmount(Material.FromEDName("encryptionarchives"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("scandatabanks"), 1), new MaterialAmount(Material.FromEDName("hyperspacetrajectories"), 1), new MaterialAmount(Material.FromEDName("adaptiveencryptors"), 1) } },
            });
            _ = new BlueprintTemplate("HullReinforcementAdvanced", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1), new MaterialAmount(Material.FromEDName("highdensitycomposites"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("germanium"), 1), new MaterialAmount(Material.FromEDName("conductiveceramics"), 1), new MaterialAmount(Material.FromEDName("fedproprietarycomposites"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("conductiveceramics"), 1), new MaterialAmount(Material.FromEDName("tin"), 1), new MaterialAmount(Material.FromEDName("militarygradealloys"), 1) } },
            });
            _ = new BlueprintTemplate("HullReinforcementExplosive", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1), new MaterialAmount(Material.FromEDName("zinc"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("salvagedalloys"), 1), new MaterialAmount(Material.FromEDName("vanadium"), 1), new MaterialAmount(Material.FromEDName("zirconium"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("galvanisingalloys"), 1), new MaterialAmount(Material.FromEDName("tungsten"), 1), new MaterialAmount(Material.FromEDName("mercury"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phasealloys"), 1), new MaterialAmount(Material.FromEDName("molybdenum"), 1), new MaterialAmount(Material.FromEDName("ruthenium"), 1) } },
            });
            _ = new BlueprintTemplate("HullReinforcementHeavyDuty", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1), new MaterialAmount(Material.FromEDName("shieldemitters"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1), new MaterialAmount(Material.FromEDName("shieldemitters"), 1), new MaterialAmount(Material.FromEDName("highdensitycomposites"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("vanadium"), 1), new MaterialAmount(Material.FromEDName("shieldingsensors"), 1), new MaterialAmount(Material.FromEDName("fedproprietarycomposites"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("tungsten"), 1), new MaterialAmount(Material.FromEDName("compoundshielding"), 1), new MaterialAmount(Material.FromEDName("fedcorecomposites"), 1) } },
            });
            _ = new BlueprintTemplate("HullReinforcementKinetic", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1), new MaterialAmount(Material.FromEDName("vanadium"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("salvagedalloys"), 1), new MaterialAmount(Material.FromEDName("vanadium"), 1), new MaterialAmount(Material.FromEDName("highdensitycomposites"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("galvanisingalloys"), 1), new MaterialAmount(Material.FromEDName("tungsten"), 1), new MaterialAmount(Material.FromEDName("fedproprietarycomposites"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phasealloys"), 1), new MaterialAmount(Material.FromEDName("molybdenum"), 1), new MaterialAmount(Material.FromEDName("fedcorecomposites"), 1) } },
            });
            _ = new BlueprintTemplate("HullReinforcementThermic", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("heatconductionwiring"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1), new MaterialAmount(Material.FromEDName("heatdispersionplate"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("salvagedalloys"), 1), new MaterialAmount(Material.FromEDName("vanadium"), 1), new MaterialAmount(Material.FromEDName("heatexchangers"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("galvanisingalloys"), 1), new MaterialAmount(Material.FromEDName("tungsten"), 1), new MaterialAmount(Material.FromEDName("heatvanes"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phasealloys"), 1), new MaterialAmount(Material.FromEDName("molybdenum"), 1), new MaterialAmount(Material.FromEDName("protoheatradiators"), 1) } },
            });
            _ = new BlueprintTemplate("MiscChaffCapacity", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalscrap"), 1) } },
            });
            _ = new BlueprintTemplate("MiscHeatSinkCapacity", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalscrap"), 1), new MaterialAmount(Material.FromEDName("vanadium"), 1), new MaterialAmount(Material.FromEDName("niobium"), 1) } },
            });
            _ = new BlueprintTemplate("MiscLightWeight", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phosphorus"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("salvagedalloys"), 1), new MaterialAmount(Material.FromEDName("manganese"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("salvagedalloys"), 1), new MaterialAmount(Material.FromEDName("manganese"), 1), new MaterialAmount(Material.FromEDName("conductiveceramics"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("conductivecomponents"), 1), new MaterialAmount(Material.FromEDName("phasealloys"), 1), new MaterialAmount(Material.FromEDName("protolightalloys"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("conductiveceramics"), 1), new MaterialAmount(Material.FromEDName("protolightalloys"), 1), new MaterialAmount(Material.FromEDName("protoradiolicalloys"), 1) } },
            });
            _ = new BlueprintTemplate("MiscPointDefenseCapacity", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalscrap"), 1), new MaterialAmount(Material.FromEDName("vanadium"), 1), new MaterialAmount(Material.FromEDName("niobium"), 1) } },
            });
            _ = new BlueprintTemplate("MiscReinforced", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1), new MaterialAmount(Material.FromEDName("shieldemitters"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1), new MaterialAmount(Material.FromEDName("shieldemitters"), 1), new MaterialAmount(Material.FromEDName("tungsten"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("zinc"), 1), new MaterialAmount(Material.FromEDName("tungsten"), 1), new MaterialAmount(Material.FromEDName("molybdenum"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("highdensitycomposites"), 1), new MaterialAmount(Material.FromEDName("molybdenum"), 1), new MaterialAmount(Material.FromEDName("technetium"), 1) } },
            });
            _ = new BlueprintTemplate("MiscShielded", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("wornshieldemitters"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1), new MaterialAmount(Material.FromEDName("shieldemitters"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1), new MaterialAmount(Material.FromEDName("shieldemitters"), 1), new MaterialAmount(Material.FromEDName("highdensitycomposites"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("vanadium"), 1), new MaterialAmount(Material.FromEDName("shieldingsensors"), 1), new MaterialAmount(Material.FromEDName("fedproprietarycomposites"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("tungsten"), 1), new MaterialAmount(Material.FromEDName("compoundshielding"), 1), new MaterialAmount(Material.FromEDName("fedcorecomposites"), 1) } },
            });
            _ = new BlueprintTemplate("PowerDistributorHighCapacity", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("sulphur"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("legacyfirmware"), 1), new MaterialAmount(Material.FromEDName("chromium"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("legacyfirmware"), 1), new MaterialAmount(Material.FromEDName("chromium"), 1), new MaterialAmount(Material.FromEDName("highdensitycomposites"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("consumerfirmware"), 1), new MaterialAmount(Material.FromEDName("selenium"), 1), new MaterialAmount(Material.FromEDName("fedproprietarycomposites"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("industrialfirmware"), 1), new MaterialAmount(Material.FromEDName("fedproprietarycomposites"), 1), new MaterialAmount(Material.FromEDName("militarysupercapacitors"), 1) } },
            });
            _ = new BlueprintTemplate("PowerDistributorHighFrequency", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("legacyfirmware"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("legacyfirmware"), 1), new MaterialAmount(Material.FromEDName("chemicalprocessors"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("gridresistors"), 1), new MaterialAmount(Material.FromEDName("consumerfirmware"), 1), new MaterialAmount(Material.FromEDName("chemicaldistillery"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("hybridcapacitors"), 1), new MaterialAmount(Material.FromEDName("industrialfirmware"), 1), new MaterialAmount(Material.FromEDName("chemicalmanipulators"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("industrialfirmware"), 1), new MaterialAmount(Material.FromEDName("chemicalmanipulators"), 1), new MaterialAmount(Material.FromEDName("exquisitefocuscrystals"), 1) } },
            });
            _ = new BlueprintTemplate("PowerDistributorPriorityEngines", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("sulphur"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("sulphur"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("bulkscandata"), 1), new MaterialAmount(Material.FromEDName("chromium"), 1), new MaterialAmount(Material.FromEDName("electrochemicalarrays"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("scanarchives"), 1), new MaterialAmount(Material.FromEDName("selenium"), 1), new MaterialAmount(Material.FromEDName("polymercapacitors"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("scandatabanks"), 1), new MaterialAmount(Material.FromEDName("cadmium"), 1), new MaterialAmount(Material.FromEDName("militarysupercapacitors"), 1) } },
            });
            _ = new BlueprintTemplate("PowerDistributorPrioritySystems", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("sulphur"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("sulphur"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("bulkscandata"), 1), new MaterialAmount(Material.FromEDName("chromium"), 1), new MaterialAmount(Material.FromEDName("electrochemicalarrays"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("scanarchives"), 1), new MaterialAmount(Material.FromEDName("selenium"), 1), new MaterialAmount(Material.FromEDName("polymercapacitors"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("scandatabanks"), 1), new MaterialAmount(Material.FromEDName("cadmium"), 1), new MaterialAmount(Material.FromEDName("militarysupercapacitors"), 1) } },
            });
            _ = new BlueprintTemplate("PowerDistributorPriorityWeapons", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("sulphur"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("sulphur"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("bulkscandata"), 1), new MaterialAmount(Material.FromEDName("hybridcapacitors"), 1), new MaterialAmount(Material.FromEDName("selenium"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("scanarchives"), 1), new MaterialAmount(Material.FromEDName("electrochemicalarrays"), 1), new MaterialAmount(Material.FromEDName("cadmium"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("scandatabanks"), 1), new MaterialAmount(Material.FromEDName("polymercapacitors"), 1), new MaterialAmount(Material.FromEDName("tellurium"), 1) } },
            });
            _ = new BlueprintTemplate("PowerDistributorShielded", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("wornshieldemitters"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1), new MaterialAmount(Material.FromEDName("shieldemitters"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1), new MaterialAmount(Material.FromEDName("shieldemitters"), 1), new MaterialAmount(Material.FromEDName("highdensitycomposites"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("vanadium"), 1), new MaterialAmount(Material.FromEDName("shieldingsensors"), 1), new MaterialAmount(Material.FromEDName("fedproprietarycomposites"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("tungsten"), 1), new MaterialAmount(Material.FromEDName("compoundshielding"), 1), new MaterialAmount(Material.FromEDName("fedcorecomposites"), 1) } },
            });
            _ = new BlueprintTemplate("PowerPlantArmoured", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("wornshieldemitters"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1), new MaterialAmount(Material.FromEDName("shieldemitters"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1), new MaterialAmount(Material.FromEDName("shieldemitters"), 1), new MaterialAmount(Material.FromEDName("highdensitycomposites"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("vanadium"), 1), new MaterialAmount(Material.FromEDName("shieldingsensors"), 1), new MaterialAmount(Material.FromEDName("fedproprietarycomposites"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("tungsten"), 1), new MaterialAmount(Material.FromEDName("compoundshielding"), 1), new MaterialAmount(Material.FromEDName("fedcorecomposites"), 1) } },
            });
            _ = new BlueprintTemplate("PowerPlantBoosted", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("sulphur"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("heatconductionwiring"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("heatconductionwiring"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1), new MaterialAmount(Material.FromEDName("selenium"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("heatdispersionplate"), 1), new MaterialAmount(Material.FromEDName("conductiveceramics"), 1), new MaterialAmount(Material.FromEDName("cadmium"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("conductiveceramics"), 1), new MaterialAmount(Material.FromEDName("chemicalmanipulators"), 1), new MaterialAmount(Material.FromEDName("tellurium"), 1) } },
            });
            _ = new BlueprintTemplate("PowerPlantStealth", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1), new MaterialAmount(Material.FromEDName("archivedemissiondata"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1), new MaterialAmount(Material.FromEDName("archivedemissiondata"), 1), new MaterialAmount(Material.FromEDName("heatexchangers"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("germanium"), 1), new MaterialAmount(Material.FromEDName("emissiondata"), 1), new MaterialAmount(Material.FromEDName("heatvanes"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("niobium"), 1), new MaterialAmount(Material.FromEDName("decodedemissiondata"), 1), new MaterialAmount(Material.FromEDName("protoheatradiators"), 1) } },
            });
            _ = new BlueprintTemplate("SensorExpanded", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalscrap"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalscrap"), 1), new MaterialAmount(Material.FromEDName("germanium"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalscrap"), 1), new MaterialAmount(Material.FromEDName("germanium"), 1), new MaterialAmount(Material.FromEDName("phasealloys"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalequipment"), 1), new MaterialAmount(Material.FromEDName("niobium"), 1), new MaterialAmount(Material.FromEDName("protolightalloys"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalcomponents"), 1), new MaterialAmount(Material.FromEDName("tin"), 1), new MaterialAmount(Material.FromEDName("protoradiolicalloys"), 1) } },
            });
            _ = new BlueprintTemplate("SensorFastScan", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phosphorus"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phosphorus"), 1), new MaterialAmount(Material.FromEDName("uncutfocuscrystals"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phosphorus"), 1), new MaterialAmount(Material.FromEDName("uncutfocuscrystals"), 1), new MaterialAmount(Material.FromEDName("symmetrickeys"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("manganese"), 1), new MaterialAmount(Material.FromEDName("focuscrystals"), 1), new MaterialAmount(Material.FromEDName("encryptionarchives"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("arsenic"), 1), new MaterialAmount(Material.FromEDName("refinedfocuscrystals"), 1), new MaterialAmount(Material.FromEDName("adaptiveencryptors"), 1) } },
            });
            _ = new BlueprintTemplate("SensorLightWeight", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phosphorus"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("salvagedalloys"), 1), new MaterialAmount(Material.FromEDName("manganese"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("salvagedalloys"), 1), new MaterialAmount(Material.FromEDName("manganese"), 1), new MaterialAmount(Material.FromEDName("conductiveceramics"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("conductivecomponents"), 1), new MaterialAmount(Material.FromEDName("phasealloys"), 1), new MaterialAmount(Material.FromEDName("protolightalloys"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("conductiveceramics"), 1), new MaterialAmount(Material.FromEDName("protolightalloys"), 1), new MaterialAmount(Material.FromEDName("protoradiolicalloys"), 1) } },
            });
            _ = new BlueprintTemplate("SensorLongRange", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1), new MaterialAmount(Material.FromEDName("hybridcapacitors"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1), new MaterialAmount(Material.FromEDName("hybridcapacitors"), 1), new MaterialAmount(Material.FromEDName("emissiondata"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("germanium"), 1), new MaterialAmount(Material.FromEDName("electrochemicalarrays"), 1), new MaterialAmount(Material.FromEDName("decodedemissiondata"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("niobium"), 1), new MaterialAmount(Material.FromEDName("polymercapacitors"), 1), new MaterialAmount(Material.FromEDName("compactemissionsdata"), 1) } },
            });
            _ = new BlueprintTemplate("SensorWideAngle", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalscrap"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalscrap"), 1), new MaterialAmount(Material.FromEDName("germanium"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalscrap"), 1), new MaterialAmount(Material.FromEDName("germanium"), 1), new MaterialAmount(Material.FromEDName("scandatabanks"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalequipment"), 1), new MaterialAmount(Material.FromEDName("niobium"), 1), new MaterialAmount(Material.FromEDName("encodedscandata"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalcomponents"), 1), new MaterialAmount(Material.FromEDName("tin"), 1), new MaterialAmount(Material.FromEDName("classifiedscandata"), 1) } },
            });
            _ = new BlueprintTemplate("ShieldBoosterExplosive", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1), new MaterialAmount(Material.FromEDName("focuscrystals"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("germanium"), 1), new MaterialAmount(Material.FromEDName("shielddensityreports"), 1), new MaterialAmount(Material.FromEDName("refinedfocuscrystals"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("niobium"), 1), new MaterialAmount(Material.FromEDName("shieldpatternanalysis"), 1), new MaterialAmount(Material.FromEDName("exquisitefocuscrystals"), 1) } },
            });
            _ = new BlueprintTemplate("ShieldBoosterHeavyDuty", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("gridresistors"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shieldcyclerecordings"), 1), new MaterialAmount(Material.FromEDName("hybridcapacitors"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shieldcyclerecordings"), 1), new MaterialAmount(Material.FromEDName("hybridcapacitors"), 1), new MaterialAmount(Material.FromEDName("niobium"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shieldsoakanalysis"), 1), new MaterialAmount(Material.FromEDName("electrochemicalarrays"), 1), new MaterialAmount(Material.FromEDName("tin"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shielddensityreports"), 1), new MaterialAmount(Material.FromEDName("polymercapacitors"), 1), new MaterialAmount(Material.FromEDName("antimony"), 1) } },
            });
            _ = new BlueprintTemplate("ShieldBoosterKinetic", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("gridresistors"), 1), new MaterialAmount(Material.FromEDName("germanium"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("salvagedalloys"), 1), new MaterialAmount(Material.FromEDName("hybridcapacitors"), 1), new MaterialAmount(Material.FromEDName("focuscrystals"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("galvanisingalloys"), 1), new MaterialAmount(Material.FromEDName("shielddensityreports"), 1), new MaterialAmount(Material.FromEDName("refinedfocuscrystals"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phasealloys"), 1), new MaterialAmount(Material.FromEDName("shieldpatternanalysis"), 1), new MaterialAmount(Material.FromEDName("exquisitefocuscrystals"), 1) } },
            });
            _ = new BlueprintTemplate("ShieldBoosterResistive", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phosphorus"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phosphorus"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phosphorus"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1), new MaterialAmount(Material.FromEDName("focuscrystals"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("manganese"), 1), new MaterialAmount(Material.FromEDName("conductiveceramics"), 1), new MaterialAmount(Material.FromEDName("refinedfocuscrystals"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("conductiveceramics"), 1), new MaterialAmount(Material.FromEDName("refinedfocuscrystals"), 1), new MaterialAmount(Material.FromEDName("imperialshielding"), 1) } },
            });
            _ = new BlueprintTemplate("ShieldBoosterThermic", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("heatconductionwiring"), 1), new MaterialAmount(Material.FromEDName("germanium"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("heatconductionwiring"), 1), new MaterialAmount(Material.FromEDName("heatdispersionplate"), 1), new MaterialAmount(Material.FromEDName("focuscrystals"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("heatdispersionplate"), 1), new MaterialAmount(Material.FromEDName("shielddensityreports"), 1), new MaterialAmount(Material.FromEDName("refinedfocuscrystals"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("heatexchangers"), 1), new MaterialAmount(Material.FromEDName("shieldpatternanalysis"), 1), new MaterialAmount(Material.FromEDName("exquisitefocuscrystals"), 1) } },
            });
            _ = new BlueprintTemplate("ShieldCellBankRapid", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("sulphur"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("gridresistors"), 1), new MaterialAmount(Material.FromEDName("chromium"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("sulphur"), 1), new MaterialAmount(Material.FromEDName("hybridcapacitors"), 1), new MaterialAmount(Material.FromEDName("precipitatedalloys"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("chromium"), 1), new MaterialAmount(Material.FromEDName("electrochemicalarrays"), 1), new MaterialAmount(Material.FromEDName("thermicalloys"), 1) } },
            });
            _ = new BlueprintTemplate("ShieldCellBankSpecialised", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("legacyfirmware"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("legacyfirmware"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("scrambledemissiondata"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1), new MaterialAmount(Material.FromEDName("industrialfirmware"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("conductivecomponents"), 1), new MaterialAmount(Material.FromEDName("industrialfirmware"), 1), new MaterialAmount(Material.FromEDName("yttrium"), 1) } },
            });
            _ = new BlueprintTemplate("ShieldGeneratorKinetic", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shieldcyclerecordings"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shieldcyclerecordings"), 1), new MaterialAmount(Material.FromEDName("consumerfirmware"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shieldcyclerecordings"), 1), new MaterialAmount(Material.FromEDName("consumerfirmware"), 1), new MaterialAmount(Material.FromEDName("selenium"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shieldsoakanalysis"), 1), new MaterialAmount(Material.FromEDName("focuscrystals"), 1), new MaterialAmount(Material.FromEDName("mercury"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shielddensityreports"), 1), new MaterialAmount(Material.FromEDName("refinedfocuscrystals"), 1), new MaterialAmount(Material.FromEDName("ruthenium"), 1) } },
            });
            _ = new BlueprintTemplate("ShieldGeneratorOptimised", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shieldcyclerecordings"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shieldcyclerecordings"), 1), new MaterialAmount(Material.FromEDName("germanium"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shieldcyclerecordings"), 1), new MaterialAmount(Material.FromEDName("germanium"), 1), new MaterialAmount(Material.FromEDName("precipitatedalloys"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shieldsoakanalysis"), 1), new MaterialAmount(Material.FromEDName("niobium"), 1), new MaterialAmount(Material.FromEDName("thermicalloys"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shielddensityreports"), 1), new MaterialAmount(Material.FromEDName("tin"), 1), new MaterialAmount(Material.FromEDName("militarygradealloys"), 1) } },
            });
            _ = new BlueprintTemplate("ShieldGeneratorReinforced", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phosphorus"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phosphorus"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phosphorus"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1), new MaterialAmount(Material.FromEDName("mechanicalcomponents"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("manganese"), 1), new MaterialAmount(Material.FromEDName("conductiveceramics"), 1), new MaterialAmount(Material.FromEDName("configurablecomponents"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("arsenic"), 1), new MaterialAmount(Material.FromEDName("conductivepolymers"), 1), new MaterialAmount(Material.FromEDName("improvisedcomponents"), 1) } },
            });
            _ = new BlueprintTemplate("ShieldGeneratorThermic", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shieldcyclerecordings"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shieldcyclerecordings"), 1), new MaterialAmount(Material.FromEDName("germanium"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shieldcyclerecordings"), 1), new MaterialAmount(Material.FromEDName("germanium"), 1), new MaterialAmount(Material.FromEDName("selenium"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shieldsoakanalysis"), 1), new MaterialAmount(Material.FromEDName("focuscrystals"), 1), new MaterialAmount(Material.FromEDName("mercury"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("shielddensityreports"), 1), new MaterialAmount(Material.FromEDName("refinedfocuscrystals"), 1), new MaterialAmount(Material.FromEDName("ruthenium"), 1) } },
            });
            _ = new BlueprintTemplate("WeaponDoubleShot", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1), new MaterialAmount(Material.FromEDName("mechanicalequipment"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("carbon"), 1), new MaterialAmount(Material.FromEDName("mechanicalequipment"), 1), new MaterialAmount(Material.FromEDName("industrialfirmware"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("vanadium"), 1), new MaterialAmount(Material.FromEDName("mechanicalcomponents"), 1), new MaterialAmount(Material.FromEDName("securityfirmware"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("highdensitycomposites"), 1), new MaterialAmount(Material.FromEDName("configurablecomponents"), 1), new MaterialAmount(Material.FromEDName("embeddedfirmware"), 1) } },
            });
            _ = new BlueprintTemplate("WeaponEfficient", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("sulphur"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("sulphur"), 1), new MaterialAmount(Material.FromEDName("heatdispersionplate"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("chromium"), 1), new MaterialAmount(Material.FromEDName("scrambledemissiondata"), 1), new MaterialAmount(Material.FromEDName("heatexchangers"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("selenium"), 1), new MaterialAmount(Material.FromEDName("archivedemissiondata"), 1), new MaterialAmount(Material.FromEDName("heatvanes"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("cadmium"), 1), new MaterialAmount(Material.FromEDName("emissiondata"), 1), new MaterialAmount(Material.FromEDName("protoheatradiators"), 1) } },
            });
            _ = new BlueprintTemplate("WeaponFocused", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("iron"), 1), new MaterialAmount(Material.FromEDName("chromium"), 1), new MaterialAmount(Material.FromEDName("conductiveceramics"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("germanium"), 1), new MaterialAmount(Material.FromEDName("focuscrystals"), 1), new MaterialAmount(Material.FromEDName("polymercapacitors"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("niobium"), 1), new MaterialAmount(Material.FromEDName("refinedfocuscrystals"), 1), new MaterialAmount(Material.FromEDName("militarysupercapacitors"), 1) } },
            });
            _ = new BlueprintTemplate("WeaponHighCapacity", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalscrap"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalscrap"), 1), new MaterialAmount(Material.FromEDName("vanadium"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalscrap"), 1), new MaterialAmount(Material.FromEDName("vanadium"), 1), new MaterialAmount(Material.FromEDName("niobium"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalequipment"), 1), new MaterialAmount(Material.FromEDName("highdensitycomposites"), 1), new MaterialAmount(Material.FromEDName("tin"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalcomponents"), 1), new MaterialAmount(Material.FromEDName("fedproprietarycomposites"), 1), new MaterialAmount(Material.FromEDName("militarysupercapacitors"), 1) } },
            });
            _ = new BlueprintTemplate("WeaponLightWeight", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phosphorus"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("manganese"), 1),  new MaterialAmount(Material.FromEDName("salvagedalloys"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("manganese"), 1),  new MaterialAmount(Material.FromEDName("salvagedalloys"), 1),   new MaterialAmount(Material.FromEDName("conductiveceramics"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("phasealloys"), 1),  new MaterialAmount(Material.FromEDName("conductivecomponents"), 1),   new MaterialAmount(Material.FromEDName("protolightalloys"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("conductiveceramics"), 1),  new MaterialAmount(Material.FromEDName("protolightalloys"), 1),   new MaterialAmount(Material.FromEDName("protoradiolicalloys"), 1) } },
            });
            _ = new BlueprintTemplate("WeaponLongRange", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("sulphur"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("sulphur"), 1), new MaterialAmount(Material.FromEDName("consumerfirmware"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("sulphur"), 1), new MaterialAmount(Material.FromEDName("consumerfirmware"), 1), new MaterialAmount(Material.FromEDName("focuscrystals"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("focuscrystals"), 1), new MaterialAmount(Material.FromEDName("consumerfirmware"), 1), new MaterialAmount(Material.FromEDName("conductivepolymers"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("thermicalloys"), 1), new MaterialAmount(Material.FromEDName("industrialfirmware"), 1), new MaterialAmount(Material.FromEDName("biotechconductors"), 1) } },
            });
            _ = new BlueprintTemplate("WeaponOvercharged", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1), new MaterialAmount(Material.FromEDName("conductivecomponents"), 1), new MaterialAmount(Material.FromEDName("electrochemicalarrays"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("zinc"), 1), new MaterialAmount(Material.FromEDName("conductiveceramics"), 1), new MaterialAmount(Material.FromEDName("polymercapacitors"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("zirconium"), 1), new MaterialAmount(Material.FromEDName("conductivepolymers"), 1), new MaterialAmount(Material.FromEDName("embeddedfirmware"), 1) } },
            });
            _ = new BlueprintTemplate("WeaponRapidFire", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalscrap"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("mechanicalscrap"), 1), new MaterialAmount(Material.FromEDName("heatdispersionplate"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("legacyfirmware"), 1), new MaterialAmount(Material.FromEDName("mechanicalequipment"), 1), new MaterialAmount(Material.FromEDName("precipitatedalloys"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("consumerfirmware"), 1), new MaterialAmount(Material.FromEDName("mechanicalcomponents"), 1), new MaterialAmount(Material.FromEDName("thermicalloys"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("precipitatedalloys"), 1), new MaterialAmount(Material.FromEDName("configurablecomponents"), 1), new MaterialAmount(Material.FromEDName("technetium"), 1) } },
            });
            _ = new BlueprintTemplate("WeaponShortRange", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1), new MaterialAmount(Material.FromEDName("consumerfirmware"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1), new MaterialAmount(Material.FromEDName("consumerfirmware"), 1), new MaterialAmount(Material.FromEDName("electrochemicalarrays"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("electrochemicalarrays"), 1), new MaterialAmount(Material.FromEDName("consumerfirmware"), 1), new MaterialAmount(Material.FromEDName("conductivepolymers"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("industrialfirmware"), 1), new MaterialAmount(Material.FromEDName("configurablecomponents"), 1), new MaterialAmount(Material.FromEDName("biotechconductors"), 1) } },
            });
            _ = new BlueprintTemplate("WeaponSturdy", new Dictionary<int, List<MaterialAmount>>()
            {
                { 1, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1) } },
                { 2, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1), new MaterialAmount(Material.FromEDName("shieldemitters"), 1) } },
                { 3, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("nickel"), 1), new MaterialAmount(Material.FromEDName("shieldemitters"), 1), new MaterialAmount(Material.FromEDName("tungsten"), 1) } },
                { 4, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("zinc"), 1), new MaterialAmount(Material.FromEDName("molybdenum"), 1), new MaterialAmount(Material.FromEDName("tungsten"), 1) } },
                { 5, new List<MaterialAmount>() { new MaterialAmount(Material.FromEDName("molybdenum"), 1), new MaterialAmount(Material.FromEDName("technetium"), 1), new MaterialAmount(Material.FromEDName("highdensitycomposites"), 1) } },
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

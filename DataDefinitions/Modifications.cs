namespace EddiDataDefinitions
{
    /// <summary>
    /// Modification types
    /// </summary>
    public class Modifications : ResourceBasedLocalizedEDName<Modifications>
    {
        static Modifications()
        {
            resourceManager = Properties.Modifications.ResourceManager;
            resourceManager.IgnoreCase = true;

            var ArmourAdvanced = new Modifications("Armour_Advanced");
            var ArmourExplosive = new Modifications("Armour_Explosive");
            var ArmourHeavyDuty = new Modifications("Armour_HeavyDuty");
            var ArmourKinetic = new Modifications("Armour_Kinetic");
            var ArmourThermic = new Modifications("Armour_Thermic");
            var EngineDirty = new Modifications("Engine_Dirty");
            var EngineReinforced = new Modifications("Engine_Reinforced");
            var EngineTuned = new Modifications("Engine_Tuned");
            var FSDFastBoot = new Modifications("FSD_FastBoot");
            var FSDinterdictorExpanded = new Modifications("FSDinterdictor_Expanded");
            var FSDinterdictorLongRange = new Modifications("FSDinterdictor_LongRange");
            var FSDLongRange = new Modifications("FSD_LongRange");
            var FSDShielded = new Modifications("FSD_Shielded");
            var HullReinforcementAdvanced = new Modifications("HullReinforcement_Advanced");
            var HullReinforcementExplosive = new Modifications("HullReinforcement_Explosive");
            var HullReinforcementHeavyDuty = new Modifications("HullReinforcement_HeavyDuty");
            var HullReinforcementKinetic = new Modifications("HullReinforcement_Kinetic");
            var HullReinforcementThermic = new Modifications("HullReinforcement_Thermic");
            var MiscChaffCapacity = new Modifications("Misc_ChaffCapacity");
            var MiscHeatSinkCapacity = new Modifications("Misc_HeatSinkCapacity");
            var MiscLightWeight = new Modifications("Misc_LightWeight");
            var MiscPointDefenseCapacity = new Modifications("Misc_PointDefenseCapacity");
            var MiscReinforced = new Modifications("Misc_Reinforced");
            var MiscShielded = new Modifications("Misc_Shielded");
            var PowerDistributorHighCapacity = new Modifications("PowerDistributor_HighCapacity");
            var PowerDistributorHighFrequency = new Modifications("PowerDistributor_HighFrequency");
            var PowerDistributorPriorityEngines = new Modifications("PowerDistributor_PriorityEngines");
            var PowerDistributorPrioritySystems = new Modifications("PowerDistributor_PrioritySystems");
            var PowerDistributorPriorityWeapons = new Modifications("PowerDistributor_PriorityWeapons");
            var PowerDistributorShielded = new Modifications("PowerDistributor_Shielded");
            var PowerPlantArmoured = new Modifications("PowerPlant_Armoured");
            var PowerPlantBoosted = new Modifications("PowerPlant_Boosted");
            var PowerPlantStealth = new Modifications("PowerPlant_Stealth");
            var SensorExpanded = new Modifications("Sensor_Expanded");
            var SensorFastScan = new Modifications("Sensor_FastScan");
            var SensorLightWeight = new Modifications("Sensor_LightWeight");
            var SensorLongRange = new Modifications("Sensor_LongRange");
            var SensorWideAngle = new Modifications("Sensor_WideAngle");
            var ShieldBoosterExplosive = new Modifications("ShieldBooster_Explosive");
            var ShieldBoosterHeavyDuty = new Modifications("ShieldBooster_HeavyDuty");
            var ShieldBoosterKinetic = new Modifications("ShieldBooster_Kinetic");
            var ShieldBoosterResistive = new Modifications("ShieldBooster_Resistive");
            var ShieldBoosterThermic = new Modifications("ShieldBooster_Thermic");
            var ShieldCellBankRapid = new Modifications("ShieldCellBank_Rapid");
            var ShieldCellBankSpecialised = new Modifications("ShieldCellBank_Specialised");
            var ShieldGeneratorKinetic = new Modifications("ShieldGenerator_Kinetic");
            var ShieldGeneratorOptimised = new Modifications("ShieldGenerator_Optimised");
            var ShieldGeneratorReinforced = new Modifications("ShieldGenerator_Reinforced");
            var ShieldGeneratorThermic = new Modifications("ShieldGenerator_Thermic");
            var WakeScannerFast = new Modifications("WakeScanner_Fast");
            var WakeScannerLightWeight = new Modifications("WakeScanner_LightWeight");
            var WakeScannerLongRange = new Modifications("WakeScanner_LongRange");
            var WakeScannerReinforced = new Modifications("WakeScanner_Reinforced");
            var WakeScannerShielded = new Modifications("WakeScanner_Shielded");
            var WakeScannerWideAngle = new Modifications("WakeScanner_WideAngle");
            var WeaponDoubleShot = new Modifications("Weapon_DoubleShot");
            var WeaponEfficient = new Modifications("Weapon_Efficient");
            var WeaponFocused = new Modifications("Weapon_Focused");
            var WeaponHighCapacity = new Modifications("Weapon_HighCapacity");
            var WeaponLightWeight = new Modifications("Weapon_LightWeight");
            var WeaponLongRange = new Modifications("Weapon_LongRange");
            var WeaponOvercharged = new Modifications("Weapon_Overcharged");
            var WeaponRapidFire = new Modifications("Weapon_RapidFire");
            var WeaponShortRange = new Modifications("Weapon_ShortRange");
            var WeaponSturdy = new Modifications("Weapon_Sturdy");
        }

        public static readonly Modifications None = new Modifications("None");

        // dummy used to ensure that the static constructor has run
        public Modifications() : this("")
        { }

        private Modifications(string edname) : base(edname, edname.Replace("_", ""))
        { }
    }
}
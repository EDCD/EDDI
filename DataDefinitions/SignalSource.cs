using System;
using System.Collections.Generic;

namespace EddiDataDefinitions
{
    public class SignalSource : ResourceBasedLocalizedEDName<SignalSource>
    {
        static SignalSource()
        {
            resourceManager = Properties.SignalSource.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = edname => new SignalSource(edname);

            UnidentifiedSignalSource = new SignalSource("USS");

            var NavBeacon = new SignalSource("MULTIPLAYER_SCENARIO42_TITLE");
            var CompromisedNavBeacon = new SignalSource("MULTIPLAYER_SCENARIO80_TITLE");

            var ResourceExtraction = new SignalSource("MULTIPLAYER_SCENARIO14_TITLE");
            var ResourceExtractionLow = new SignalSource("MULTIPLAYER_SCENARIO77_TITLE");
            var ResourceExtractionHigh = new SignalSource("MULTIPLAYER_SCENARIO78_TITLE");
            var ResourceExtractionHazardous = new SignalSource("MULTIPLAYER_SCENARIO79_TITLE");
            var SalvageableWreckage = new SignalSource("MULTIPLAYER_SCENARIO81_TITLE");

            var CombatZoneHigh = new SignalSource("Warzone_PointRace_High");
            var CombatZoneMedium = new SignalSource("Warzone_PointRace_Med");
            var CombatZoneLow = new SignalSource("Warzone_PointRace_Low");
            var CombatZoneThargoid = new SignalSource("Warzone_TG");

            var Aftermath = new SignalSource("USS_Type_Aftermath");
            var Anomaly = new SignalSource("USS_Type_Anomaly");
            var Ceremonial = new SignalSource("USS_Type_Ceremonial");
            var Convoy = new SignalSource("USS_Type_Convoy");
            var DegradedEmissions = new SignalSource("USS_Type_Salvage");
            var Distress = new SignalSource("USS_Type_DistressSignal");
            var EncodedEmissions = new SignalSource("USS_Type_ValuableSalvage");
            var HighGradeEmissions = new SignalSource("USS_Type_VeryValuableSalvage");
            var MissionTarget = new SignalSource("USS_Type_MissionTarget");
            var NonHuman = new SignalSource("USS_Type_NonHuman");
            var TradingBeacon = new SignalSource("USS_Type_TradingBeacon");
            var WeaponsFire = new SignalSource("USS_Type_WeaponsFire");

            var UnregisteredCommsBeacon = new SignalSource("NumberStation");
            var ListeningPost = new SignalSource("ListeningPost");

            var CapShip = new SignalSource("FIXED_EVENT_CAPSHIP");
            var Checkpoint = new SignalSource("FIXED_EVENT_CHECKPOINT");
            var ConvoyBeacon = new SignalSource("FIXED_EVENT_CONVOY");
            var DebrisField = new SignalSource("FIXED_EVENT_DEBRIS");
            var DistributionCenter = new SignalSource("FIXED_EVENT_DISTRIBUTIONCENTRE");
            var PirateAttackT5 = new SignalSource("FIXED_EVENT_HIGHTHREATSCENARIO_T5");
            var PirateAttackT6 = new SignalSource("FIXED_EVENT_HIGHTHREATSCENARIO_T6");
            var PirateAttackT7 = new SignalSource("FIXED_EVENT_HIGHTHREATSCENARIO_T7");
            var NotableStellarPhenomenaCloud = new SignalSource("Fixed_Event_Life_Cloud");
            var NotableStellarPhenomenaRing = new SignalSource("Fixed_Event_Life_Ring");

            var AttackAftermath = new SignalSource("AttackAftermath");
            var AftermathLarge = new SignalSource("Aftermath_Large");

            var Biological = new SignalSource("SAA_SignalType_Biological");
            var Geological = new SignalSource("SAA_SignalType_Geological");
            var Guardian = new SignalSource("SAA_SignalType_Guardian");
            var Human = new SignalSource("SAA_SignalType_Human");
            var Thargoid = new SignalSource("SAA_SignalType_Thargoid");
            var Other = new SignalSource("SAA_SignalType_Other");

            var AncientGuardianRuins = new SignalSource("Ancient");
            var GuardianStructureTiny = new SignalSource("Ancient_Tiny");
            var GuardianStructureSmall = new SignalSource("Ancient_Small");
            var GuardianStructureMedium = new SignalSource("Ancient_Medium");
            var ThargoidBarnacle = new SignalSource("Settlement_Unflattened_Unknown");
            var ThargoidCrashSite = new SignalSource("Settlement_Unflattened_WreckedUnknown");

            var AbandonedBuggyEasy = new SignalSource("Abandoned_Buggy_Easy");
            var AbandonedBuggyMedium = new SignalSource("Abandoned_Buggy_Medium");
            var AbandonedBuggyHard = new SignalSource("Abandoned_Buggy_Hard");
            var DamagedEagleAssassinationEasy = new SignalSource("Damaged_Eagle_Assassination_Easy");
            var DamagedEagleAssassinationMedium = new SignalSource("Damaged_Eagle_Assassination_Medium");
            var DamagedEagleAssassinationHard = new SignalSource("Damaged_Eagle_Assassination_Hard");
            var DamagedSidewinderAssassinationEasy = new SignalSource("Damaged_Sidewinder_Assassination_Easy");
            var DamagedSidewinderAssassinationMedium = new SignalSource("Damaged_Sidewinder_Assassination_Medium");
            var DamagedSidewinderAssassinationHard = new SignalSource("Damaged_Sidewinder_Assassination_Hard");
            var DamagedEagleEasy = new SignalSource("Damaged_Eagle_Easy");
            var DamagedEagleMedium = new SignalSource("Damaged_Eagle_Medium");
            var DamagedEagleHard = new SignalSource("Damaged_Eagle_Hard");
            var DamagedSidewinderEasy = new SignalSource("Damaged_Sidewinder_Easy");
            var DamagedSidewinderMedium = new SignalSource("Damaged_Sidewinder_Medium");
            var DamagedSidewinderHard = new SignalSource("Damaged_Sidewinder_Hard");
            var SmugglersCacheEasy = new SignalSource("Smugglers_Cache_Easy");
            var SmugglersCacheMedium = new SignalSource("Smugglers_Cache_Medium");
            var SmugglersCacheHard = new SignalSource("Smugglers_Cache_Hard");
            var TrapCargo = new SignalSource("Trap_Cargo");
            var WreckageBuggyEasy = new SignalSource("Wreckage_Buggy_Easy");
            var WreckageBuggyMedium = new SignalSource("Wreckage_Buggy_Medium");
            var WreckageBuggyHard = new SignalSource("Wreckage_Buggy_Hard");
            var WreckageCargo = new SignalSource("Wreckage_Cargo");
            var WreckageProbeEasy = new SignalSource("Wreckage_Probe_Easy");
            var WreckageProbeMedium = new SignalSource("Wreckage_Probe_Medium");
            var WreckageProbeHard = new SignalSource("Wreckage_Probe_Hard");
            var WreckageSatelliteEasy = new SignalSource("Wreckage_Satellite_Easy");
            var WreckageSatelliteMedium = new SignalSource("Wreckage_Satellite_Medium");
            var WreckageSatelliteHard = new SignalSource("Wreckage_Satellite_Hard");
            var WrecksEagleEasy = new SignalSource("Wrecks_Eagle_Easy");
            var WrecksEagleMedium = new SignalSource("Wrecks_Eagle_Medium");
            var WrecksEagleHard = new SignalSource("Wrecks_Eagle_Hard");
            var WrecksSidewinderEasy = new SignalSource("Wrecks_Sidewinder_Easy");
            var WrecksSidewinderMedium = new SignalSource("Wrecks_Sidewinder_Medium");
            var WrecksSidewinderHard = new SignalSource("Wrecks_Sidewinder_Hard");
        }

        public static readonly SignalSource UnidentifiedSignalSource;

        public int index;
        public string spawningFaction { get; set; }
        public DateTime? expiry { get; set; }
        public int? threatLevel { get; set; }
        public bool? isStation { get; set; }
        public FactionState spawningState { get; set; }
        public long? systemAddress { get; set; }

        // dummy used to ensure that the static constructor has run
        public SignalSource() : this("")
        { }

        private SignalSource(string edname) : base(edname, edname)
        { }

        public new static SignalSource FromEDName(string from)
        {
            if (from != null)
            {
                if (from.Contains("$"))
                {
                    string tidiedFrom = from
                        .Replace("$", "")
                        .Replace("POIScenario_", "")
                        .Replace("POIScene_", "")
                        .Replace("Watson_", "")
                        .Replace("_Heist", "")
                        .Replace("_Salvage", "")
                        .Replace(";", "");

                    // Extract any sub-type from the name (e.g. $SAA_Unknown_Signal:#type=$SAA_SignalType_Geological;:#index=3; )
                    if (tidiedFrom.Contains(":#type="))
                    {
                        string[] fromArray = tidiedFrom.Split(new[] { ":#type=" }, System.StringSplitOptions.None);
                        tidiedFrom = fromArray[1];
                    }

                    // Extract any threat value which might be present and then strip the index value
                    int? threatLvl = null;
                    if (tidiedFrom.Contains("USS_ThreatLevel:#threatLevel="))
                    {
                        string[] fromArray = tidiedFrom.Split(new[] { "USS_ThreatLevel:#threatLevel=" }, System.StringSplitOptions.None);
                        if (int.TryParse(fromArray[1], out var threat)) { threatLvl = threat; }
                        tidiedFrom = fromArray[0];
                    }

                    // Extract any index value which might be present and then strip the index value
                    int indexResult = 0;
                    if (tidiedFrom.Contains(":#index="))
                    {
                        string[] fromArray = tidiedFrom.Split(new[] { ":#index=" }, System.StringSplitOptions.None);
                        if (int.TryParse(fromArray[1], out indexResult)) { }
                        tidiedFrom = fromArray[0];
                    }

                    // Extract any pure number parts (e.g. '_01_')
                    var parts = new List<string>();
                    foreach (var part in tidiedFrom.Split(new[] { "_" }, StringSplitOptions.None))
                    {
                        if (int.TryParse(part, out _)) { }
                        else { parts.Add(part); }
                    }
                    tidiedFrom = string.Join("_", parts);

                    // Find our signal source
                    SignalSource result = ResourceBasedLocalizedEDName<SignalSource>.FromEDName(tidiedFrom.Trim());

                    // Include our index value with our result
                    result.index = indexResult;
                    result.threatLevel = threatLvl;

                    return result;
                }
            }
            return null;
        }
    }
}

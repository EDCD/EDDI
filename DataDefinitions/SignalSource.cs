using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text.RegularExpressions;

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
            GenericSignalSource = new SignalSource("GenericSignalSource");

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
            var CombatZoneThargoidHigh = new SignalSource("Warzone_TG_High");
            var CombatZoneThargoidMedium = new SignalSource("Warzone_TG_Med");
            var CombatZoneThargoidLow = new SignalSource("Warzone_TG_Low");

            var Aftermath = new SignalSource("USS_Type_Aftermath", "USS_SalvageHaulageWreckage");
            var Anomaly = new SignalSource("USS_Type_Anomaly");
            var Ceremonial = new SignalSource("USS_Type_Ceremonial", "USS_CeremonialComms");
            var Convoy = new SignalSource("USS_Type_Convoy", "USS_ConvoyDispersalPattern");
            var DegradedEmissions = new SignalSource("USS_Type_Salvage", "USS_DegradedEmissions");
            var Distress = new SignalSource("USS_Type_DistressSignal", "USS_DistressCall");
            var EncodedEmissions = new SignalSource("USS_Type_ValuableSalvage");
            var HighGradeEmissions = new SignalSource("USS_Type_VeryValuableSalvage", "USS_HighGradeEmissions");
            var MissionTarget = new SignalSource("USS_Type_MissionTarget");
            var NonHuman = new SignalSource("USS_Type_NonHuman", "USS_NonHumanSignalSource");
            var TradingBeacon = new SignalSource("USS_Type_TradingBeacon", "USS_TradingBeacon");
            var WeaponsFire = new SignalSource("USS_Type_WeaponsFire", "USS_WeaponsFire");

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
            var PlanetAnomaly = new SignalSource("SAA_SignalType_PlanetAnomaly");
            var Other = new SignalSource("SAA_SignalType_Other");

            var AncientGuardianRuins = new SignalSource("Ancient");
            var GuardianStructureTiny = new SignalSource("Ancient_Tiny");
            var GuardianStructureSmall = new SignalSource("Ancient_Small");
            var GuardianStructureMedium = new SignalSource("Ancient_Medium");
            var ThargoidBarnacle = new SignalSource("Settlement_Unflattened_Unknown");
            var ThargoidCrashSite = new SignalSource("Settlement_Unflattened_WreckedUnknown");

            var AbandonedBuggy = new SignalSource("Abandoned_Buggy");
            var ActivePowerSource = new SignalSource("Perimeter");
            var CrashedShip = new SignalSource("CrashedShip");
            var DamagedEagleAssassination = new SignalSource("Damaged_Eagle_Assassination");
            var DamagedSidewinderAssassination = new SignalSource("Damaged_Sidewinder_Assassination");
            var DamagedEagle = new SignalSource("Damaged_Eagle");
            var DamagedSidewinder = new SignalSource("Damaged_Sidewinder");
            var SmugglersCache = new SignalSource("Smugglers_Cache");
            var Cargo = new SignalSource("Cargo");
            var TrapCargo = new SignalSource("Trap_Cargo");
            var TrapData = new SignalSource("Trap_Data");
            var WreckageBuggy = new SignalSource("Wreckage_Buggy");
            var WreckageCargo = new SignalSource("Wreckage_Cargo");
            var WreckageProbe = new SignalSource("Wreckage_Probe");
            var WreckageSatellite = new SignalSource("Wreckage_Satellite");
            var WrecksEagle = new SignalSource("Wrecks_Eagle");
            var WrecksSidewinder = new SignalSource("Wrecks_Sidewinder");

            var ArmedRevolt = new SignalSource("Gro_controlScenarioTitle");
        }

        public static readonly SignalSource UnidentifiedSignalSource;
        public static readonly SignalSource GenericSignalSource;

        public int index;
        public string spawningFaction { get; set; }
        public DateTime? expiry { get; set; }
        public int? threatLevel { get; set; }
        public bool? isStation { get; set; }
        public FactionState spawningState { get; set; }
        public long? systemAddress { get; set; }

        // Not intended to be user facing
        public string altEdName { get; private set; }

        // dummy used to ensure that the static constructor has run
        public SignalSource() : this("")
        { }

        private SignalSource(string edname, string altEdName = null) : base(edname, edname)
        {
            this.altEdName = altEdName;
        }

        public new static SignalSource FromEDName(string from)
        {
            if (from == null) return null;

            if (!from.Contains("$"))
            {
                // Appears to be a simple proper name
                return new SignalSource(from) { fallbackInvariantName = from, fallbackLocalizedName = from };
            }
            
            // Signal names can mix symbolic and proper names, e.g. "INV Audacious Dream $Warzone_TG_Med;",
            // so use regex to separate any symbolic names from proper names.
            var regex = new Regex("\\$.*;");
            var match = regex.Match(from);
            var symbolicFrom = match.Value;

            string tidiedFrom = symbolicFrom
                .Replace("$", "")
                .Replace(";", "");

            // Remove various prefix and suffix tags from non-USS sources
            if (!tidiedFrom.StartsWith("USS_"))
            {
                tidiedFrom = tidiedFrom
                    .Replace("POI_", "")
                    .Replace("POIScenario_", "")
                    .Replace("POIScene_", "")
                    .Replace("Watson_", "")
                    .Replace("_Heist", "")
                    .Replace("_Salvage", "");
            }

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
                tidiedFrom = fromArray[0]
                    .Replace("_Easy", "")
                    .Replace("_Medium", "")
                    .Replace("_Hard", "");
            }
            else
            {
                // Derive threat levels for Odyssey content from "Easy", "Medium", and "Hard" suffix tags
                if (tidiedFrom.Contains("_Easy"))
                {
                    threatLvl = 1;
                    tidiedFrom = tidiedFrom.Replace("_Easy", "");
                }
                else if (tidiedFrom.Contains("_Medium") && !tidiedFrom.StartsWith("Ancient_"))
                {
                    // We need to use size to distinguish between guardian structures so preserve the "Medium" tag
                    // when it represents the size of an ancient guardian structures. Remove it when it describes the difficulty of the encounter.
                    threatLvl = 2;
                    tidiedFrom = tidiedFrom.Replace("_Medium", "");
                }
                else if (tidiedFrom.Contains("_Hard"))
                {
                    threatLvl = 3;
                    tidiedFrom = tidiedFrom.Replace("_Hard", "");
                }
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

            // Use the USS Type for USS signals (since those are always unique)
            // There is an FDev bug where both Encoded Emissions and High Grade Emissions use the `USS_HighGradeEmissions` symbol.
            if (tidiedFrom.StartsWith("USS_") && !tidiedFrom.Contains("Type_"))
            {
                tidiedFrom = AllOfThem.FirstOrDefault(s => s.altEdName == tidiedFrom)?.edname ?? tidiedFrom;
            }

            // Find our signal source
            SignalSource result = ResourceBasedLocalizedEDName<SignalSource>.FromEDName(tidiedFrom.Trim());

            // Prepend and append any proper names that were previously set aside
            // (though these will only apply if a signal definition was not found for the symbolic name)
            if (from.Length > symbolicFrom.Length)
            {
                var localizedName = result.localizedName;
                var invariantName = result.invariantName;
                result = new SignalSource(from.Replace(symbolicFrom, invariantName).Trim())
                 {
                     fallbackInvariantName = from.Replace(symbolicFrom, invariantName).Trim(),
                     fallbackLocalizedName = from.Replace(symbolicFrom, localizedName).Trim()
                 };
            }

            // Include our index value with our result
            result.index = indexResult;
            result.threatLevel = threatLvl;

            return result;
        }

        public static SignalSource FromStationEDName(string from)
        {
            if (string.IsNullOrEmpty(from)) { return null; }

            // Signal might be a fleet carrier with name and carrier id in a single string. If so, we break them apart
            var fleetCarrierRegex = new Regex("^(.+)(?> )([A-Za-z0-9]{3}-[A-Za-z0-9]{3})$");
            if (fleetCarrierRegex.IsMatch(from))
            {
                // Fleet carrier names include both the carrier name and carrier ID, we need to separate them
                var fleetCarrierParts = fleetCarrierRegex.Matches(from)[0].Groups;
                if (fleetCarrierParts.Count == 3)
                {
                    var fleetCarrierName = fleetCarrierParts[2].Value;
                    var fleetCarrierLocalizedName = fleetCarrierParts[1].Value;
                    return new SignalSource(fleetCarrierName) { fallbackLocalizedName = fleetCarrierLocalizedName, isStation = true};
                }
            }
            return new SignalSource(from) {isStation = true};
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiDataDefinitions
{
    public class SignalSource : ResourceBasedLocalizedEDName<SignalSource>
    {
        static SignalSource()
        {
            resourceManager = Properties.SignalSource.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = edname => new SignalSource(edname);
        }

        public static readonly SignalSource UnidentifiedSignalSource = new SignalSource ("USS");
        public static readonly SignalSource GenericSignalSource = new SignalSource ("GenericSignalSource");

        public static readonly SignalSource NavBeacon = new SignalSource("MULTIPLAYER_SCENARIO42_TITLE");
        public static readonly SignalSource CompromisedNavBeacon = new SignalSource("MULTIPLAYER_SCENARIO80_TITLE");

        public static readonly SignalSource ResourceExtraction = new SignalSource("MULTIPLAYER_SCENARIO14_TITLE");
        public static readonly SignalSource ResourceExtractionLow = new SignalSource("MULTIPLAYER_SCENARIO77_TITLE");
        public static readonly SignalSource ResourceExtractionHigh = new SignalSource("MULTIPLAYER_SCENARIO78_TITLE");
        public static readonly SignalSource ResourceExtractionHazardous = new SignalSource("MULTIPLAYER_SCENARIO79_TITLE");
        public static readonly SignalSource SalvageableWreckage = new SignalSource("MULTIPLAYER_SCENARIO81_TITLE");

        public static readonly SignalSource CombatZoneHigh = new SignalSource("Warzone_PointRace_High");
        public static readonly SignalSource CombatZoneMedium = new SignalSource("Warzone_PointRace_Med");
        public static readonly SignalSource CombatZoneLow = new SignalSource("Warzone_PointRace_Low");
        public static readonly SignalSource CombatZoneThargoid = new SignalSource("Warzone_TG");
        public static readonly SignalSource CombatZoneThargoidHigh = new SignalSource("Warzone_TG_High");
        public static readonly SignalSource CombatZoneThargoidMedium = new SignalSource("Warzone_TG_Med");
        public static readonly SignalSource CombatZoneThargoidLow = new SignalSource("Warzone_TG_Low");
        public static readonly SignalSource CombatZoneThargoidVeryHigh = new SignalSource("Warzone_TG_VeryHigh");

        public static readonly SignalSource Aftermath = new SignalSource("USS_Type_Aftermath", "USS_SalvageHaulageWreckage");
        public static readonly SignalSource Anomaly = new SignalSource("USS_Type_Anomaly");
        public static readonly SignalSource Ceremonial = new SignalSource("USS_Type_Ceremonial", "USS_CeremonialComms");
        public static readonly SignalSource Convoy = new SignalSource("USS_Type_Convoy", "USS_ConvoyDispersalPattern");
        public static readonly SignalSource DegradedEmissions = new SignalSource("USS_Type_Salvage", "USS_DegradedEmissions");
        public static readonly SignalSource Distress = new SignalSource("USS_Type_DistressSignal", "USS_DistressCall");
        public static readonly SignalSource EncodedEmissions = new SignalSource("USS_Type_ValuableSalvage");
        public static readonly SignalSource HighGradeEmissions = new SignalSource("USS_Type_VeryValuableSalvage", "USS_HighGradeEmissions");
        public static readonly SignalSource MissionTarget = new SignalSource("USS_Type_MissionTarget");
        public static readonly SignalSource NonHuman = new SignalSource("USS_Type_NonHuman", "USS_NonHumanSignalSource");
        public static readonly SignalSource TradingBeacon = new SignalSource("USS_Type_TradingBeacon", "USS_TradingBeacon");
        public static readonly SignalSource WeaponsFire = new SignalSource("USS_Type_WeaponsFire", "USS_WeaponsFire");

        public static readonly SignalSource UnregisteredCommsBeacon = new SignalSource("NumberStation");
        public static readonly SignalSource ListeningPost = new SignalSource("ListeningPost");

        public static readonly SignalSource CapShip = new SignalSource("FIXED_EVENT_CAPSHIP");
        public static readonly SignalSource Checkpoint = new SignalSource("FIXED_EVENT_CHECKPOINT");
        public static readonly SignalSource ConvoyBeacon = new SignalSource("FIXED_EVENT_CONVOY");
        public static readonly SignalSource DebrisField = new SignalSource("FIXED_EVENT_DEBRIS");
        public static readonly SignalSource DistributionCenter = new SignalSource("FIXED_EVENT_DISTRIBUTIONCENTRE");
        public static readonly SignalSource PirateAttackT5 = new SignalSource("FIXED_EVENT_HIGHTHREATSCENARIO_T5");
        public static readonly SignalSource PirateAttackT6 = new SignalSource("FIXED_EVENT_HIGHTHREATSCENARIO_T6");
        public static readonly SignalSource PirateAttackT7 = new SignalSource("FIXED_EVENT_HIGHTHREATSCENARIO_T7");
        public static readonly SignalSource NotableStellarPhenomenaCloud = new SignalSource("Fixed_Event_Life_Cloud");
        public static readonly SignalSource NotableStellarPhenomenaRing = new SignalSource("Fixed_Event_Life_Ring");

        public static readonly SignalSource AttackAftermath = new SignalSource("AttackAftermath");
        public static readonly SignalSource AftermathLarge = new SignalSource("Aftermath_Large");

        public static readonly SignalSource Biological = new SignalSource("SAA_SignalType_Biological");
        public static readonly SignalSource Geological = new SignalSource("SAA_SignalType_Geological");
        public static readonly SignalSource Guardian = new SignalSource("SAA_SignalType_Guardian");
        public static readonly SignalSource Human = new SignalSource("SAA_SignalType_Human");
        public static readonly SignalSource Thargoid = new SignalSource("SAA_SignalType_Thargoid");
        public static readonly SignalSource PlanetAnomaly = new SignalSource("SAA_SignalType_PlanetAnomaly");
        public static readonly SignalSource OtherSAA = new SignalSource("SAA_SignalType_Other");

        public static readonly SignalSource AncientGuardianRuins = new SignalSource("Ancient");
        public static readonly SignalSource GuardianStructureTiny = new SignalSource("Ancient_Tiny");
        public static readonly SignalSource GuardianStructureSmall = new SignalSource("Ancient_Small");
        public static readonly SignalSource GuardianStructureMedium = new SignalSource("Ancient_Medium");
        public static readonly SignalSource ThargoidBarnacle = new SignalSource("Settlement_Unflattened_Unknown");
        public static readonly SignalSource ThargoidCrashSite = new SignalSource("Settlement_Unflattened_WreckedUnknown");

        public static readonly SignalSource AbandonedBuggy = new SignalSource("Abandoned_Buggy");
        public static readonly SignalSource ActivePowerSource = new SignalSource("Perimeter");
        public static readonly SignalSource CrashedShip = new SignalSource("CrashedShip");
        public static readonly SignalSource DamagedEagleAssassination = new SignalSource("Damaged_Eagle_Assassination");
        public static readonly SignalSource DamagedSidewinderAssassination = new SignalSource("Damaged_Sidewinder_Assassination");
        public static readonly SignalSource DamagedEagle = new SignalSource("Damaged_Eagle");
        public static readonly SignalSource DamagedSidewinder = new SignalSource("Damaged_Sidewinder");
        public static readonly SignalSource SmugglersCache = new SignalSource("Smugglers_Cache");
        public static readonly SignalSource Cargo = new SignalSource("Cargo");
        public static readonly SignalSource TrapCargo = new SignalSource("Trap_Cargo");
        public static readonly SignalSource TrapData = new SignalSource("Trap_Data");
        public static readonly SignalSource WreckageAncientProbe = new SignalSource("Wreckage_AncientProbe");
        public static readonly SignalSource WreckageBuggy = new SignalSource("Wreckage_Buggy");
        public static readonly SignalSource WreckageCargo = new SignalSource("Wreckage_Cargo");
        public static readonly SignalSource WreckageProbe = new SignalSource("Wreckage_Probe");
        public static readonly SignalSource WreckageSatellite = new SignalSource("Wreckage_Satellite");
        public static readonly SignalSource WrecksEagle = new SignalSource("Wrecks_Eagle");
        public static readonly SignalSource WrecksSidewinder = new SignalSource("Wrecks_Sidewinder");

        public static readonly SignalSource ArmedRevolt = new SignalSource("Gro_controlScenarioTitle");

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

        public static new SignalSource FromEDName(string from)
        {
            if (from == null) return null;

            if (!from.Contains("$"))
            {
                // Appears to be a simple proper name
                return new SignalSource(from) { fallbackInvariantName = from, fallbackLocalizedName = from };
            }

            SignalSource result = null;
            int? threatLvl = null;
            int indexResult = 0;

            // Signal names can mix symbolic and proper names, e.g. "INV Audacious Dream $Warzone_TG_Med;",
            // so use regex to separate any symbolic names from proper names.
            var regex = new Regex("\\$.*;");
            var match = regex.Match(from);

            if (match.Success && from.Length > match.Value.Length)
            {
                // This appears to be a mixed name, look for the symbolic portion only then
                // prepend and append any proper names that were previously set aside
                var symbolicFrom = match.Value;
                var symbolicResult = FromEDName(symbolicFrom.Trim());
                result = new SignalSource(from.Replace(symbolicFrom, symbolicResult.invariantName).Trim())
                {
                    fallbackInvariantName = from.Replace(symbolicFrom, symbolicResult.invariantName).Trim(),
                    fallbackLocalizedName = from.Replace(symbolicFrom, symbolicResult.localizedName).Trim()
                };
            }
            else
            {
                string tidiedFrom = from
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
                        .Replace("_Salvage", "")
                        .Replace("_Skimmers", "");
                }

                // Extract any sub-type from the name (e.g. $SAA_Unknown_Signal:#type=$SAA_SignalType_Geological;:#index=3; )
                if (tidiedFrom.Contains(":#type="))
                {
                    string[] fromArray = tidiedFrom.Split(new[] { ":#type=" }, System.StringSplitOptions.None);
                    tidiedFrom = fromArray[1];
                }

                // Extract any threat value which might be present and then strip the index value
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
                if (AllOfThem.Any(s => s.edname.Equals(tidiedFrom.Trim(), StringComparison.InvariantCultureIgnoreCase)))
                {
                    result = ResourceBasedLocalizedEDName<SignalSource>.FromEDName(tidiedFrom.Trim());
                }
                else
                {
                    // There is no match, return a generic result
                    result = GenericSignalSource;
                    Logging.Warn($"Unknown ED name {from} in resource {resourceManager.BaseName}.");
                }
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

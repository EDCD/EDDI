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
            var PirateAttackT5 = new SignalSource("FIXED_EVENT_HIGHTHREATSCENARIO_T5");
            var PirateAttackT6 = new SignalSource("FIXED_EVENT_HIGHTHREATSCENARIO_T6");
            var PirateAttackT7 = new SignalSource("FIXED_EVENT_HIGHTHREATSCENARIO_T7");
            var NotableStellarPhenomenaCloud = new SignalSource("Fixed_Event_Life_Cloud");
            var NotableStellarPhenomenaRing = new SignalSource("Fixed_Event_Life_Ring");

            var Biological = new SignalSource("SAA_SignalType_Biological"); //Speculative
            var Geological = new SignalSource("SAA_SignalType_Geological");
            var Guardian = new SignalSource("SAA_SignalType_Guardian"); // Speculative
            var Human = new SignalSource("SAA_SignalType_Human");
            var Thargoid = new SignalSource("SAA_SignalType_Thargoid");

            var ThargoidBarnacle = new SignalSource("Settlement_Unflattened_Unknown");
        }

        public static readonly SignalSource UnidentifiedSignalSource;

        public int index;

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
                        .Replace(";", "");

                    // Extract any index value which might be present and then strip the index value
                    int indexResult = 0;
                    if (tidiedFrom.Contains(":#index="))
                    {
                        string[] fromArray = tidiedFrom.Split(new string[] { ":#index=" }, System.StringSplitOptions.None);
                        if (int.TryParse(fromArray[1], out indexResult)) { }
                        tidiedFrom = fromArray[0];
                    }

                    // Find our signal source
                    SignalSource result = ResourceBasedLocalizedEDName<SignalSource>.FromEDName(tidiedFrom);

                    // Include our index value with our result
                    result.index = indexResult;

                    return result;
                }
            }
            return null;
        }
    }
}

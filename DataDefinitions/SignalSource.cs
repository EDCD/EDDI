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
            var ResourceExtraction = new SignalSource("MULTIPLAYER_SCENARIO14_TITLE");
            var ResourceExtractionLow = new SignalSource("MULTIPLAYER_SCENARIO77_TITLE");
            var ResourceExtractionHigh = new SignalSource("MULTIPLAYER_SCENARIO78_TITLE");
            var ResourceExtractionHazardous = new SignalSource("MULTIPLAYER_SCENARIO79_TITLE");
            var CompromisedNavBeacon = new SignalSource("MULTIPLAYER_SCENARIO80_TITLE");

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
            var ListeningPostIndex1 = new SignalSource("ListeningPost_index1");

            var NotableStellarPhenomenaCloud = new SignalSource("Fixed_Event_Life_Cloud");
            var NotableStellarPhenomenaRing = new SignalSource("Fixed_Event_Life_Ring");

            var CapShip = new SignalSource("FIXED_EVENT_CAPSHIP");
            var PirateAttack = new SignalSource("FIXED_EVENT_HIGHTHREATSCENARIO_T5");

            var Biological = new SignalSource("SAA_SignalType_Biological"); //Speculative
            var Geological = new SignalSource("SAA_SignalType_Geological");
            var Human = new SignalSource("SAA_SignalType_Human");

        }

        public static readonly SignalSource UnidentifiedSignalSource;

        // dummy used to ensure that the static constructor has run
        public SignalSource() : this("")
        { }

        private SignalSource(string edname) : base(edname, edname)
        { }

        new public static SignalSource FromEDName(string from)
        {
            if (from != null)
            {
                if (from.Contains("$"))
                {
                    return ResourceBasedLocalizedEDName<SignalSource>.FromEDName(from
                        .Replace("#", "_")
                        .Replace("$", "")
                        .Replace("=", "")
                        .Replace(":", "")
                        .Replace(";", ""));
                }
            }
            return null;
        }
    }
}

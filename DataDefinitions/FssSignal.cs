namespace EddiDataDefinitions
{
    public class FssSignal : ResourceBasedLocalizedEDName<FssSignal>
    {
        static FssSignal()
        {
            resourceManager = Properties.FssSignal.ResourceManager;
            resourceManager.IgnoreCase = true;

            var NavBeacon = new FssSignal("MULTIPLAYER_SCENARIO42_TITLE");
            var CompromisedNavBeacon = new FssSignal("MULTIPLAYER_SCENARIO80_TITLE");
            var CombatZoneHigh = new FssSignal("Warzone_PointRace_High");
            var CombatZoneMedium = new FssSignal("Warzone_PointRace_Med");
            var CombatZoneLow = new FssSignal("Warzone_PointRace_Low");
        }

        public static readonly FssSignal UnidentifiedSignalSource = new FssSignal("USS");

        // dummy used to ensure that the static constructor has run
        public FssSignal() : this("")
        { }

        private FssSignal (string edname) : base(edname, edname.Replace("$", ""))
        { }

        new public static FssSignal FromEDName(string edname)
        {
            if (edname.Contains("$"))
            {
                return ResourceBasedLocalizedEDName<FssSignal>.FromEDName(edname.Replace("$", ""));
            }

            return new FssSignal(edname);
        }
    }
}

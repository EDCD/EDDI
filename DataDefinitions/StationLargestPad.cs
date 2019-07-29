using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    /// <summary> Station's largest landing pad size </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class StationLargestPad : ResourceBasedLocalizedEDName<StationLargestPad>
    {
        static StationLargestPad()
        {
            resourceManager = Properties.StationLargestPad.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new StationLargestPad(edname);

            None = new StationLargestPad("None");
            Large = new StationLargestPad("Large");
            Medium = new StationLargestPad("Medium");
            Small = new StationLargestPad("Small");
        }

        public static readonly StationLargestPad None;
        public static readonly StationLargestPad Large;
        public static readonly StationLargestPad Medium;
        public static readonly StationLargestPad Small;

        // dummy used to ensure that the static constructor has run
        public StationLargestPad() : this("")
        { }

        private StationLargestPad(string edname) : base(edname, edname)
        { }
    }
}

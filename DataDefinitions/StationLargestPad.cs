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

            var Large = new StationLargestPad("Large");
            var Medium = new StationLargestPad("Medium");
            var Small = new StationLargestPad("Small");
        }

        public static readonly StationLargestPad None = new StationLargestPad("None");

        // dummy used to ensure that the static constructor has run
        public StationLargestPad() : this("")
        { }

        private StationLargestPad(string edname) : base(edname, edname)
        { }

        public static StationLargestPad FromSize(string value)
        {
            // Map old values from when we had an enum and map abbreviated sizes
            string size = string.Empty;
            if (value == "0" || value == null) { size = "None"; }
            value = value?.ToLowerInvariant();
            if (value == "1" || value == "s") { size = "Small"; }
            if (value == "2" || value == "m") { size = "Medium"; }
            if (value == "3" || value == "l") { size = "Large"; }

            return FromName(size);
        }
    }
}

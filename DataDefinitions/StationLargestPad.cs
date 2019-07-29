using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    /// <summary> Station's largest landing pad size </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class LandingPadSize : ResourceBasedLocalizedEDName<LandingPadSize>
    {
        static LandingPadSize()
        {
            resourceManager = Properties.StationLargestPad.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new LandingPadSize(edname);

            None = new LandingPadSize("None");
            Large = new LandingPadSize("Large");
            Medium = new LandingPadSize("Medium");
            Small = new LandingPadSize("Small");
        }

        public static readonly LandingPadSize None;
        public static readonly LandingPadSize Large;
        public static readonly LandingPadSize Medium;
        public static readonly LandingPadSize Small;

        // dummy used to ensure that the static constructor has run
        public LandingPadSize() : this("")
        { }

        private LandingPadSize(string edname) : base(edname, edname)
        { }
    }
}

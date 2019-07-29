using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    /// <summary> Size of ships, landing pads, etc. </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Size : ResourceBasedLocalizedEDName<Size>
    {
        static Size()
        {
            resourceManager = Properties.Size.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new Size(edname);

            None = new Size("None");
            Huge = new Size("Huge");
            Large = new Size("Large");
            Medium = new Size("Medium");
            Small = new Size("Small");
            Tiny = new Size("Tiny");
        }

        public static readonly Size None;
        public static readonly Size Huge;
        public static readonly Size Large;
        public static readonly Size Medium;
        public static readonly Size Small;
        public static readonly Size Tiny;

        // dummy used to ensure that the static constructor has run
        public Size() : this("")
        { }

        private Size(string edname) : base(edname, edname)
        { }
    }
}

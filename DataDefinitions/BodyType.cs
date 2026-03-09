namespace EddiDataDefinitions
{
    public class BodyType : ResourceBasedLocalizedEDName<BodyType>
    {
        static BodyType()
        {
            resourceManager = Properties.Body.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new BodyType(edname);
        }

        public static readonly BodyType None = new("None");
        public static readonly BodyType Barycenter = new("Null"); // The journal includes references to a "Null" string when we are near a barycenter
        public static readonly BodyType Belt = new("Belt");
        public static readonly BodyType HyperbolicOrbiter = new("HyperbolicOrbiter");
        public static readonly BodyType Moon = new("Moon");
        public static readonly BodyType Planet = new("Planet");
        public static readonly BodyType PlanetaryRing = new("PlanetaryRing");
        public static readonly BodyType Star = new("Star");
        public static readonly BodyType Station = new("Station");
        public static readonly BodyType StellarRing = new("StellarRing");

        // dummy used to ensure that the static constructor has run
        public BodyType() : this("")
        { }

        private BodyType(string edname) : base(edname, edname)
        { }
    }
}

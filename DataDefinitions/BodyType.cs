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

        public static readonly BodyType None = new BodyType("None");
        public static readonly BodyType Barycenter = new BodyType("Null"); // The journal includes references to a "Null" string when we are near a barycenter
        public static readonly BodyType Belt = new BodyType("Belt");
        public static readonly BodyType HyperbolicOrbiter = new BodyType("HyperbolicOrbiter");
        public static readonly BodyType Moon = new BodyType("Moon");
        public static readonly BodyType Planet = new BodyType("Planet");
        public static readonly BodyType PlanetaryRing = new BodyType("PlanetaryRing");
        public static readonly BodyType Star = new BodyType("Star");
        public static readonly BodyType Station = new BodyType("Station");
        public static readonly BodyType StellarRing = new BodyType("StellarRing");

        // dummy used to ensure that the static constructor has run
        public BodyType() : this("")
        { }

        private BodyType(string edname) : base(edname, edname)
        { }
    }
}

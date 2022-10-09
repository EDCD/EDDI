namespace EddiDataDefinitions
{
    public class BodyType : ResourceBasedLocalizedEDName<BodyType>
    {
        static BodyType()
        {
            resourceManager = Properties.Body.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new BodyType(edname);

            None = new BodyType("None");
            var Barycenter = new BodyType("Null"); // The journal includes references to a "Null" string when we are near a barycenter
            var Belt = new BodyType("Belt");
            var HyperbolicOrbiter = new BodyType("HyperbolicOrbiter");
            var Moon = new BodyType("Moon");
            var Planet = new BodyType("Planet");
            var PlanetaryRing = new BodyType("PlanetaryRing");
            var Star = new BodyType("Star");
            var Station = new BodyType("Station");
            var StellarRing = new BodyType("StellarRing");
        }

        public static readonly BodyType None;

        // dummy used to ensure that the static constructor has run
        public BodyType() : this("")
        { }

        private BodyType(string edname) : base(edname, edname)
        { }
    }
}

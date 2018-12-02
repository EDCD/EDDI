namespace EddiDataDefinitions
{
    public class BodyType : ResourceBasedLocalizedEDName<BodyType>
    {
        static BodyType()
        {
            resourceManager = Properties.Body.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new BodyType(edname);

            var Null = new BodyType("Null"); // The journal includes references to a "Null" string
            var Planet = new BodyType("Planet");
            var Star = new BodyType("Star");
            var Station = new BodyType("Station");
            var Belt = new BodyType("Belt");
            var PlanetaryRing = new BodyType("PlanetaryRing");
        }

        public static readonly BodyType None = new BodyType("None");

        // dummy used to ensure that the static constructor has run
        public BodyType() : this("")
        { }

        private BodyType(string edname) : base(edname, edname)
        { }
    }
}

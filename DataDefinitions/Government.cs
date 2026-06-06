
namespace EddiDataDefinitions
{
    /// <summary>
    /// Government types
    /// </summary>
    public class Government : ResourceBasedLocalizedEDName<Government>
    {
        static Government()
        {
            resourceManager = Properties.Governments.ResourceManager;
            resourceManager.IgnoreCase = true;
            missingEDNameHandler = (edname) => new Government(edname);

            None = new Government("None");
            Anarchy = new Government("Anarchy");
            Carrier = new Government( "Carrier" );
            Colony = new Government("Colony");
            Communism = new Government("Communism");
            Confederacy = new Government("Confederacy");
            Construction = new Government("Megaconstruction");
            Cooperative = new Government("Cooperative");
            Corporate = new Government("Corporate");
            Democracy = new Government("Democracy");
            Dictatorship = new Government("Dictatorship");
            Feudal = new Government("Feudal");
            Imperial = new Government("Imperial"); // Might not exist? I've never encountered this one.
            Patronage = new Government("Patronage");
            Prison = new Government("Prison");
            PrisonColony = new Government("PrisonColony");
            Theocracy = new Government("Theocracy");
            Workshop = new Government("Workshop");
            Engineer = new Government("engineer");
        }

        public static readonly Government None;
        public static readonly Government Anarchy;
        public static readonly Government Colony;
        public static readonly Government Communism;
        public static readonly Government Confederacy;
        public static readonly Government Construction;
        public static readonly Government Cooperative;
        public static readonly Government Corporate;
        public static readonly Government Democracy;
        public static readonly Government Dictatorship;
        public static readonly Government Feudal;
        public static readonly Government Imperial;
        public static readonly Government Patronage;
        public static readonly Government Prison;
        public static readonly Government PrisonColony;
        public static readonly Government Theocracy;
        public static readonly Government Workshop;
        public static readonly Government Engineer;
        public static readonly Government Carrier;

        // dummy used to ensure that the static constructor has run
        public Government () : this("")
        { }

        private Government(string edname) : base(edname, edname)
        { }

        new public static Government FromEDName ( string from )
        {
            if ( from is null ) { return None; }

            var tidiedFrom = from.Replace("$government_", "").Replace(";", "");
            return ResourceBasedLocalizedEDName<Government>.FromEDName( tidiedFrom );
        }

        new public static Government FromName(string from)
        {
            if (from is null) { return None; }

            // EDSM uses a special string to describe engineering workshops, standardize here.
            from = from.Replace("Workshop (Engineer)", "engineer");
            return ResourceBasedLocalizedEDName<Government>.FromName( from );
        }
    }
}

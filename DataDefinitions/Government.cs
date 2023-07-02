
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

            None = new Government("$government_None;");
            Anarchy = new Government("$government_Anarchy;");
            Carrier = new Government( "$government_Carrier;" );
            Colony = new Government("$government_Colony;");
            Communism = new Government("$government_Communism;");
            Confederacy = new Government("$government_Confederacy;");
            Cooperative = new Government("$government_Cooperative;");
            Corporate = new Government("$government_Corporate;");
            Democracy = new Government("$government_Democracy;");
            Dictatorship = new Government("$government_Dictatorship;");
            Feudal = new Government("$government_Feudal;");
            Imperial = new Government("$government_Imperial;");
            Patronage = new Government("$government_Patronage;");
            Prison = new Government("$government_Prison;");
            PrisonColony = new Government("$government_PrisonColony;");
            Theocracy = new Government("$government_Theocracy;");
            Workshop = new Government("$government_Workshop;");
            Engineer = new Government("$government_engineer;");
        }

        public static readonly Government None;
        public static readonly Government Anarchy;
        public static readonly Government Colony;
        public static readonly Government Communism;
        public static readonly Government Confederacy;
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

        private Government(string edname) : base(edname, edname.Replace("$government_", "").Replace(";", ""))
        { }

        new public static Government FromName(string from)
        {
            if (from is null)
            {
                return None;
            }

            // EDSM uses a special string to describe engineering workshops, standardize here.
            from = from.Replace("Workshop (Engineer)", "engineer");
            return ResourceBasedLocalizedEDName<Government>.FromName(from);
        }
    }
}

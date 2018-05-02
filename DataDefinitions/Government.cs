
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
            var Anarchy = new Government("$government_Anarchy;");
            var Colony = new Government("$government_Colony;");
            var Communism = new Government("$government_Communism;");
            var Confederacy = new Government("$government_Confederacy;");
            var Cooperative = new Government("$government_Cooperative;");
            var Corporate = new Government("$government_Corporate;");
            var Democracy = new Government("$government_Democracy;");
            var Dictatorship = new Government("$government_Dictatorship;");
            var Feudal = new Government("$government_Feudal;");
            var Imperial = new Government("$government_Imperial;");
            var Patronage = new Government("$government_Patronage;");
            var Prison = new Government("$government_Prison;");
            var PrisonColony = new Government("$government_PrisonColony;");
            var Theocracy = new Government("$government_Theocracy;");
            var Workshop = new Government("$government_Workshop;");
            var Engineer = new Government("$government_engineer;");
        }

        public static readonly Government None;

        // dummy used to ensure that the static constructor has run
        public Government() : this("")
        {}

        private Government(string edname) : base(edname, edname.Replace("$government_", "").Replace(";", ""))
        {}
    }
}

using System.Linq;

namespace EddiDataDefinitions
{
    class LoadoutDescription : ResourceBasedLocalizedEDName<LoadoutDescription>
    {
        static LoadoutDescription()
        {
            resourceManager = Properties.LoadoutDescription.ResourceManager;
            resourceManager.IgnoreCase = false;

            var EmpireZero = new LoadoutDescription("EmpireZero");
            var EmpireOne = new LoadoutDescription("EmpireOne");
            var EmpireTwo = new LoadoutDescription("EmpireTwo");
            var EmpireThree = new LoadoutDescription("EmpireThree");
            var EmpireFour = new LoadoutDescription("EmpireFour");
            var FederationZero = new LoadoutDescription("FederationZero");
            var FederationOne = new LoadoutDescription("FederationOne");
            var FederationTwo = new LoadoutDescription("FederationTwo");
            var FederationThree = new LoadoutDescription("FederationThree");
            var FederationFour = new LoadoutDescription("FederationFour");
            var GdnHybridV1 = new LoadoutDescription("GdnHybridV1");
            var GdnHybridV2 = new LoadoutDescription("GdnHybridV2");
            var GdnHybridV3 = new LoadoutDescription("GdnHybridV3");
            var IndependentZero = new LoadoutDescription("IndependentZero");
            var IndependentOne = new LoadoutDescription("IndependentOne");
            var IndependentTwo = new LoadoutDescription("IndependentTwo");
            var IndependentThree = new LoadoutDescription("IndependentThree");
            var IndependentFour = new LoadoutDescription("IndependentFour");
            var IndependentAT = new LoadoutDescription("IndependentAT");
            var Starter = new LoadoutDescription("Starter");
        }

        // dummy used to ensure that the static constructor has run
        public LoadoutDescription() : this("")
        { }

        private LoadoutDescription(string edname) : base(edname, edname)
        { }

        public static LoadoutDescription FromLoadoutName(string loadoutName)
        {
            if (loadoutName == null)
            {
                return null;
            }

            string tidiedLoadout = loadoutName.ToLowerInvariant()
                .Replace("_", "")
                .Replace("fighterloadout", "")
                .Replace("name", "");
            return AllOfThem.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedLoadout);
        }
    }
}

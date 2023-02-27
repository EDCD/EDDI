using System.Linq;

namespace EddiDataDefinitions
{
    class LoadoutDescription : ResourceBasedLocalizedEDName<LoadoutDescription>
    {
        static LoadoutDescription()
        {
            resourceManager = Properties.LoadoutDescription.ResourceManager;
            resourceManager.IgnoreCase = true;

            EmpireZero = new LoadoutDescription("EmpireZero");
            EmpireOne = new LoadoutDescription("EmpireOne");
            EmpireTwo = new LoadoutDescription("EmpireTwo");
            EmpireThree = new LoadoutDescription("EmpireThree");
            EmpireFour = new LoadoutDescription("EmpireFour");
            FederationZero = new LoadoutDescription("FederationZero");
            FederationOne = new LoadoutDescription("FederationOne");
            FederationTwo = new LoadoutDescription("FederationTwo");
            FederationThree = new LoadoutDescription("FederationThree");
            FederationFour = new LoadoutDescription("FederationFour");
            GdnHybridV1 = new LoadoutDescription("GdnHybridV1");
            GdnHybridV2 = new LoadoutDescription("GdnHybridV2");
            GdnHybridV3 = new LoadoutDescription("GdnHybridV3");
            IndependentZero = new LoadoutDescription("IndependentZero");
            IndependentOne = new LoadoutDescription("IndependentOne");
            IndependentTwo = new LoadoutDescription("IndependentTwo");
            IndependentThree = new LoadoutDescription("IndependentThree");
            IndependentFour = new LoadoutDescription("IndependentFour");
            IndependentAT = new LoadoutDescription("IndependentAT");
            Starter = new LoadoutDescription("Starter"); // Default loadout for Scarab SRV
            Default = new LoadoutDescription("Default"); // Default loadout for Scorpion Combat SRV
        }

        public static readonly LoadoutDescription EmpireZero;
        public static readonly LoadoutDescription EmpireOne;
        public static readonly LoadoutDescription EmpireTwo;
        public static readonly LoadoutDescription EmpireThree;
        public static readonly LoadoutDescription EmpireFour;
        public static readonly LoadoutDescription FederationZero;
        public static readonly LoadoutDescription FederationOne;
        public static readonly LoadoutDescription FederationTwo;
        public static readonly LoadoutDescription FederationThree;
        public static readonly LoadoutDescription FederationFour;
        public static readonly LoadoutDescription GdnHybridV1;
        public static readonly LoadoutDescription GdnHybridV2;
        public static readonly LoadoutDescription GdnHybridV3;
        public static readonly LoadoutDescription IndependentZero;
        public static readonly LoadoutDescription IndependentOne;
        public static readonly LoadoutDescription IndependentTwo;
        public static readonly LoadoutDescription IndependentThree;
        public static readonly LoadoutDescription IndependentFour;
        public static readonly LoadoutDescription IndependentAT;
        public static readonly LoadoutDescription Starter;
        public static readonly LoadoutDescription Default;
        
        // dummy used to ensure that the static constructor has run
        public LoadoutDescription () : this("")
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

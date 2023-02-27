using System;
using System.Linq;

namespace EddiDataDefinitions
{
    public class VehicleDefinition : ResourceBasedLocalizedEDName<VehicleDefinition>
    {
        static VehicleDefinition()
        {
            resourceManager = Properties.Vehicle.ResourceManager;
            resourceManager.IgnoreCase = true;

            Empire = new VehicleDefinition("Empire");
            Federation = new VehicleDefinition("Federation");
            GdnHybridV1 = new VehicleDefinition("GdnHybridV1");
            GdnHybridV2 = new VehicleDefinition("GdnHybridV2");
            GdnHybridV3 = new VehicleDefinition("GdnHybridV3");
            Independent = new VehicleDefinition("Independent");
            TestBuggy = new VehicleDefinition("TestBuggy"); // Scarab SRV
            CombatSRV = new VehicleDefinition("CombatMulticrewSRV01"); // Scorpion SRV
        }

        public static readonly VehicleDefinition Empire;
        public static readonly VehicleDefinition Federation;
        public static readonly VehicleDefinition GdnHybridV1;
        public static readonly VehicleDefinition GdnHybridV2;
        public static readonly VehicleDefinition GdnHybridV3;
        public static readonly VehicleDefinition Independent;
        public static readonly VehicleDefinition TestBuggy;
        public static readonly VehicleDefinition CombatSRV;
        
        // dummy used to ensure that the static constructor has run
        public VehicleDefinition () : this("")
        { }

        private VehicleDefinition(string edname) : base(edname, edname)
        { }

        public static new VehicleDefinition FromEDName(string edName)
        {
            if (edName == null) { return null; }

            return AllOfThem.FirstOrDefault(v => v.edname.ToLowerInvariant() == titiedEDName(edName));
        }

        public static bool EDNameExists(string edName)
        {
            if (edName == null) { return false; }
            return AllOfThem.Any(v => string.Equals(v.edname, titiedEDName(edName), StringComparison.InvariantCultureIgnoreCase));
        }

        private static string titiedEDName(string edName)
        {
            return edName?.ToLowerInvariant().Replace("_fighter", "").Replace("_", "");
        }
    }
}

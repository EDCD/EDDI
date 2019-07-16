using System.Linq;

namespace EddiDataDefinitions
{
    class VehicleDefinition : ResourceBasedLocalizedEDName<VehicleDefinition>
    {
        static VehicleDefinition()
        {
            resourceManager = Properties.Vehicle.ResourceManager;
            resourceManager.IgnoreCase = false;

            var Empire = new VehicleDefinition("Empire");
            var Federation = new VehicleDefinition("Federation");
            var GdnHybridV1 = new VehicleDefinition("GdnHybridV1");
            var GdnHybridV2 = new VehicleDefinition("GdnHybridV2");
            var GdnHybridV3 = new VehicleDefinition("GdnHybridV3");
            var Independent = new VehicleDefinition("Independent");
            var TestBuggy = new VehicleDefinition("TestBuggy");
        }

        // dummy used to ensure that the static constructor has run
        public VehicleDefinition() : this("")
        { }

        private VehicleDefinition(string edname) : base(edname, edname)
        { }

        public new static VehicleDefinition FromEDName(string edName)
        {
            if (edName == null)
            {
                return null;
            }

            string tidiedName = edName.ToLowerInvariant().Replace("_fighter", "").Replace("_", "");
            return AllOfThem.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedName);
        }
    }
}

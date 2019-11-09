
namespace EddiDataDefinitions
{
    /// <summary>
    /// Economy types
    /// </summary>
    public class Economy : ResourceBasedLocalizedEDName<Economy>
    {
        static Economy()
        {
            resourceManager = Properties.Economies.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new Economy(edname);

            None = new Economy("None");
            var Agriculture = new Economy("Agri");
            var Colony = new Economy("Colony");
            var Damaged = new Economy("Damaged");
            var Extraction = new Economy("Extraction");
            var Refinery = new Economy("Refinery");
            var Repair = new Economy("Repair");
            var Rescue = new Economy("Rescue");
            var Industrial = new Economy("Industrial");
            var Terraforming = new Economy("Terraforming");
            var HighTech = new Economy("HighTech");
            var Service = new Economy("Service");
            var Tourism = new Economy("Tourism");
            var Military = new Economy("Military");
            var Prison = new Economy("Prison");
        }

        public static readonly Economy None;

        // dummy used to ensure that the static constructor has run
        public Economy() : this("")
        { }

        private Economy(string edname) : base(edname, edname)
        { }

        public new static Economy FromEDName(string edname)
        {
            // Economy names from the journal are prefixed with "$economy_" and sufficed with ";" while economy names from the Frontier API are not.
            string tidiedName = edname.Replace("$economy_", "").Replace(";", "");
            return ResourceBasedLocalizedEDName<Economy>.FromEDName(tidiedName);
        }
    }
}


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

            None = new Economy("$economy_None");
            var Agriculture = new Economy("$economy_Agri");
            var Colony = new Economy("$economy_Colony");
            var Damaged = new Economy("$economy_Damaged");
            var Extraction = new Economy("$economy_Extraction");
            var Refinery = new Economy("$economy_Refinery");
            var Repair = new Economy("$economy_Repair");
            var Rescue = new Economy("$economy_Rescue");
            var Industrial = new Economy("$economy_Industrial");
            var Terraforming = new Economy("$economy_Terraforming");
            var HighTech = new Economy("$economy_HighTech");
            var Service = new Economy("$economy_Service");
            var Tourism = new Economy("$economy_Tourism");
            var Military = new Economy("$economy_Military");
            var Prison = new Economy("$economy_Prison");
        }

        public static readonly Economy None;

        // dummy used to ensure that the static constructor has run
        public Economy() : this("")
        {}

        private Economy(string edname) : base(edname, edname.Replace("$economy_", "").Replace(";", ""))
        {}
    }
}

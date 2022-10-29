
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
            Agriculture = new Economy("Agri");
            Colony = new Economy("Colony");
            Damaged = new Economy("Damaged");
            Engineer = new Economy("Engineer");
            Extraction = new Economy("Extraction");
            Refinery = new Economy("Refinery");
            Repair = new Economy("Repair");
            Rescue = new Economy("Rescue");
            Industrial = new Economy("Industrial");
            Terraforming = new Economy("Terraforming");
            HighTech = new Economy("HighTech");
            Service = new Economy("Service");
            Tourism = new Economy("Tourism");
            Military = new Economy("Military");
            Prison = new Economy("Prison");
            Carrier = new Economy("Carrier");
        }

        public static readonly Economy None;
        public static readonly Economy Agriculture;
        public static readonly Economy Colony;
        public static readonly Economy Damaged;
        public static readonly Economy Engineer;
        public static readonly Economy Extraction;
        public static readonly Economy Refinery;
        public static readonly Economy Repair;
        public static readonly Economy Rescue;
        public static readonly Economy Industrial;
        public static readonly Economy Terraforming;
        public static readonly Economy HighTech;
        public static readonly Economy Service;
        public static readonly Economy Tourism;
        public static readonly Economy Military;
        public static readonly Economy Prison;
        public static readonly Economy Carrier;

        // dummy used to ensure that the static constructor has run
        public Economy() : this("")
        { }

        private Economy(string edname) : base(edname, edname)
        { }

        public new static Economy FromEDName(string edname)
        {
            // Economy names from the journal are prefixed with "$economy_" and sufficed with ";" while economy names from the Frontier API are not.
            // We occasionally see undefined economies appear in the journal. Treat these as null / not set.
            if (string.IsNullOrEmpty(edname)) { return None; }
            string tidiedName = edname.Replace("$economy_", "").Replace(";", "");
            return tidiedName == "Undefined" ? null : ResourceBasedLocalizedEDName<Economy>.FromEDName(tidiedName);
        }
    }
}

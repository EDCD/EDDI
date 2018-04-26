
namespace EddiDataDefinitions
{
    /// <summary>
    /// Vouchers
    /// </summary>
    public class Voucher : ResourceBasedLocalizedEDName<Voucher>
    {
        static Voucher()
        {
            resourceManager = Properties.Vouchers.ResourceManager;
            resourceManager.IgnoreCase = false;
            missingEDNameHandler = (edname) => new Voucher(edname);

            var Bounty = new Voucher("bounty");
            var Bond = new Voucher("CombatBond");
            var Scannable = new Voucher("scannable");
            var Settlement = new Voucher("settlement");
            var Trade = new Voucher("trade");
        }


        // dummy used to ensure that the static constructor has run
        public Voucher() : this("")
        { }

        private Voucher(string edname) : base(edname, edname)
        {}
    }
}

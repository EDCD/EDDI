using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Vouchers
    /// </summary>
    public class Voucher
    {
        private static readonly List<Voucher> VOUCHERS = new List<Voucher>();

        public string name { get; private set; }

        public string edname { get; private set; }

        private Voucher(string edname, string name)
        {
            this.edname = edname;
            this.name = name;

            VOUCHERS.Add(this);
        }

        public static readonly Voucher Bounty = new Voucher("bounty", "Bounty");
        public static readonly Voucher Bond = new Voucher("CombatBond", "Combat Bond");
        public static readonly Voucher Scannable = new Voucher("scannable", "Scannable");
        public static readonly Voucher Settlement = new Voucher("settlement", "Settlement");
        public static readonly Voucher Trade = new Voucher("trade", "Trade");

        public static Voucher FromName(string from)
        {
            if (from == null)
            {
                return null;
            }

            Voucher result = VOUCHERS.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown Voucher name " + from);
            }
            return result;
        }

        public static Voucher FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }

            string tidiedFrom = from.ToLowerInvariant();
            Voucher result = VOUCHERS.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown Voucher ED name " + from);
                result = new Voucher(from, tidiedFrom);
            }
            return result;
        }
    }
}

using Utilities;

namespace EddiDataDefinitions
{
    public class Traffic
    {
        [PublicAPI]
        public decimal total { get; set; }

        [PublicAPI]
        public decimal week { get; set; }

        [PublicAPI]
        public decimal day { get; set; }

        public Traffic() { }

        public Traffic(decimal total, decimal week, decimal day)
        {
            this.total = total;
            this.week = week;
            this.day = day;
        }
    }
}
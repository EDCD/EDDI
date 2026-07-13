using Utilities;

namespace EddiDataDefinitions
{
    public class Traffic
    {
        [PublicAPI( "traffic over all time" )]
        public decimal total { get; set; }

        [PublicAPI( "traffic over the past week" )]
        public decimal week { get; set; }

        [PublicAPI( "traffic over the past day" )]
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

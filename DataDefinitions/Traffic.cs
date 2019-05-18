using Newtonsoft.Json;
using System;
using System.Linq;

namespace EddiDataDefinitions
{
    public class Traffic
    {
        public decimal total { get; set; }
        public decimal week { get; set; }
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
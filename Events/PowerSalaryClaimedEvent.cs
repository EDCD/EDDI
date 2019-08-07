using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class PowerSalaryClaimedEvent : Event
    {
        public const string NAME = "Power salary claimed";
        public const string DESCRIPTION = "Triggered when a commander claims salary from a power";
        public const string SAMPLE = @"{ ""timestamp"":""2016-11-16T09:28:19Z"", ""event"":""PowerplaySalary"", ""Power"":""Zachary Hudson"", ""Amount"":3000 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static PowerSalaryClaimedEvent()
        {
            VARIABLES.Add("power", "The name of the power for which the commander is claiming salary");
            VARIABLES.Add("amount", "The salary claimed");
        }

        public string power => (Power ?? Power.None).localizedName;

        public int amount { get; private set; }

        // Not intended to be user facing

        public Power Power { get; private set; }

        public PowerSalaryClaimedEvent(DateTime timestamp, Power Power, int amount) : base(timestamp, NAME)
        {
            this.Power = Power;
            this.amount = amount;
        }
    }
}

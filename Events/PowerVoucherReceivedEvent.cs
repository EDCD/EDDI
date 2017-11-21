using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class PowerVoucherReceivedEvent : Event
    {
        public const string NAME = "Power voucher received";
        public const string DESCRIPTION = "Triggered when a commander turns in combat vouchers against an opposing power";
        public const string SAMPLE = @"{ ""timestamp"":""2016-12-09T16:17:22Z"", ""event"":""PowerplayVoucher"", ""Power"":""Edmund Mahon"", ""Systems"":[ ""FAUST 3566"", ""Bielonti"" ] }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static PowerVoucherReceivedEvent()
        {
            VARIABLES.Add("power", "The name of the power against which this commander is turning in the vouchers");
            VARIABLES.Add("systems", "The systems for which the commander is turning in the vouchers");
        }

        public string power { get; private set; }

        public List<string> systems { get; private set; }

        public PowerVoucherReceivedEvent(DateTime timestamp, string power, List<string> systems) : base(timestamp, NAME)
        {
            this.power = power;
            this.systems = systems;
        }
    }
}

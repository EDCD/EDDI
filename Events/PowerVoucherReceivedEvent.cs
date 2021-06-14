using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class PowerVoucherReceivedEvent : Event
    {
        public const string NAME = "Power voucher received";
        public const string DESCRIPTION = "Triggered when a commander turns in combat vouchers against an opposing power";
        public const string SAMPLE = @"{ ""timestamp"":""2016-12-09T16:17:22Z"", ""event"":""PowerplayVoucher"", ""Power"":""Edmund Mahon"", ""Systems"":[ ""FAUST 3566"", ""Bielonti"" ] }";

        [PublicAPI("The name of the power against which this commander is turning in the vouchers")]
        public string power => (Power ?? Power.None).localizedName;

        [PublicAPI("The systems for which the commander is turning in the vouchers")]
        public List<string> systems { get; private set; }

        // Not intended to be user facing

        public Power Power { get; private set; }

        public PowerVoucherReceivedEvent(DateTime timestamp, Power Power, List<string> systems) : base(timestamp, NAME)
        {
            this.Power = Power;
            this.systems = systems;
        }
    }
}

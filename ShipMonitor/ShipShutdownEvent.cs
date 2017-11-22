using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiShipMonitor
{
    public class ShipShutdownEvent : Event
    {
        public const string NAME = "Ship shutdown";
        public const string DESCRIPTION = "Triggered when your ship's system are shutdown";
        public const string SAMPLE = @"{ ""timestamp"":""2017-01-05T23:15:06Z"", ""event"":""SystemsShutdown"" }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipShutdownEvent()
        {
        }

        public ShipShutdownEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}

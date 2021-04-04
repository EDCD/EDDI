using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiShipMonitor
{
    public class ShipRepurchasedEvent : Event
    {
        public const string NAME = "Ship repurchased";
        public const string DESCRIPTION = "Triggered when you repurchase your ship";
        public const string SAMPLE = @"{ ""timestamp"":""2016-09-20T12:27:59Z"", ""event"":""Resurrect"", ""Option"":""rebuy"", ""Cost"":36479, ""Bankrupt"":false }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipRepurchasedEvent()
        {
            VARIABLES.Add("price", "The price of repurchasing your ship");
        }

        [PublicAPI]
        public long price { get; private set; }

        public ShipRepurchasedEvent(DateTime timestamp, long price) : base(timestamp, NAME)
        {
            this.price = price;
        }
    }
}

using EddiEvents;
using System;
using Utilities;

namespace EddiShipMonitor
{
    [PublicAPI]
    public class ShipRepurchasedEvent : Event
    {
        public const string NAME = "Ship repurchased";
        public const string DESCRIPTION = "Triggered when you repurchase your ship";
        public const string SAMPLE = @"{ ""timestamp"":""2016-09-20T12:27:59Z"", ""event"":""Resurrect"", ""Option"":""rebuy"", ""Cost"":36479, ""Bankrupt"":false }";

        [PublicAPI("The price of repurchasing your ship")]
        public long price { get; private set; }

        public ShipRepurchasedEvent(DateTime timestamp, long price) : base(timestamp, NAME)
        {
            this.price = price;
        }
    }
}

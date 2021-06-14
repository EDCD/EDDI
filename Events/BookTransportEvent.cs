using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class BookTransportEvent : Event
    {
        public const string NAME = "Book transport";
        public const string DESCRIPTION = "Triggered when booking a taxi or deployment for on foot combat";
        public const string SAMPLE = "{ \"timestamp\":\"2020-10-05T11:17:50Z\", \"event\":\"BookTaxi\", \"Cost\":23200, \"DestinationSystem\":\"Opala\", \"DestinationLocation\":\"Onizuka's Hold\" }";

        [PublicAPI(@"The type of transport being booked (e.g. ""Taxi"", ""Dropship"")")]
        public string transporttype { get; }

        [PublicAPI("The credits that you paid for the transport)")]
        public int? price { get; }

        [PublicAPI("The destination star system")]
        public string starsystem { get; }

        [PublicAPI("The destination location name")]
        public string destination { get; }

        public BookTransportEvent(DateTime timestamp, string transporttype, int? price, string starsystem, string destination) : base(timestamp, NAME)
        {
            this.transporttype = transporttype;
            this.price = price;
            this.starsystem = starsystem;
            this.destination = destination;
        }
    }
}
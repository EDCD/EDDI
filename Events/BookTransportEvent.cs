using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class BookTransportEvent (
        DateTime timestamp,
        string transporttype,
        int? price,
        string starsystem,
        string destination )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Book transport";
        public const string DESCRIPTION = "Triggered when booking a taxi or deployment for on foot combat";
        public const string SAMPLE = "{ \"timestamp\":\"2020-10-05T11:17:50Z\", \"event\":\"BookTaxi\", \"Cost\":23200, \"DestinationSystem\":\"Opala\", \"DestinationLocation\":\"Onizuka's Hold\" }";

        [PublicAPI(@"The type of transport being booked (e.g. ""Taxi"", ""Dropship"")")]
        public string transporttype { get; } = transporttype;

        [PublicAPI("The credits that you paid for the transport)")]
        public int? price { get; } = price;

        [PublicAPI("The destination star system")]
        public string starsystem { get; } = starsystem;

        [PublicAPI("The destination location name")]
        public string destination { get; } = destination;
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class BookTransportEvent : Event
    {
        public const string NAME = "Book transport";
        public const string DESCRIPTION = "Triggered when booking a taxi or deployment for on foot combat";
        public const string SAMPLE = "{ \"timestamp\":\"2020-10-05T11:17:50Z\", \"event\":\"BookTaxi\", \"Cost\":23200, \"DestinationSystem\":\"Opala\", \"DestinationLocation\":\"Onizuka's Hold\" }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BookTransportEvent()
        {
            VARIABLES.Add("transporttype", @"The type of transport being booked (e.g. ""Taxi"", ""Dropship"")");
            VARIABLES.Add("price", "The credits that you paid for the transport)");
            VARIABLES.Add("starsystem", "The destination star system");
            VARIABLES.Add("destination", "The destination location name");
        }

        [JsonProperty("transporttype")]
        public string transporttype { get; }

        [JsonProperty("price")]
        public int? price { get; }

        [JsonProperty("starsystem")]
        public string starsystem { get; }

        [JsonProperty("destination")]
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
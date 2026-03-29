using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CancelTransportEvent ( DateTime timestamp, string transporttype, int? refund )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Cancel transport";
        public const string DESCRIPTION = "Triggered when canceling a taxi or deployment for on foot combat";
        public const string SAMPLE = "{ \"timestamp\":\"2021-03-30T22:30:18Z\", \"event\":\"CancelTaxi\", \"Refund\":100 }";

        [PublicAPI(@"The type of transport being cancelled (e.g. ""Taxi"", ""Dropship"")")]
        public string transporttype { get; } = transporttype;

        [PublicAPI("The credits that you were refunded for the cancelled transport)")]
        public int? refund { get; } = refund;
    }
}
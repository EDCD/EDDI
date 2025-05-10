using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class DestinationArrivedEvent : Event
    {
        public const string NAME = "Destination arrived";
        public const string DESCRIPTION = "Triggered when you drop into normal space at your selected destination";
        public static readonly object[] SAMPLES =
            {
                new DestinationArrivedEvent(Dates.FromString("2024-05-05T23:39:27Z") ?? DateTime.UtcNow, SignalSource.FromEDName("$USS_Type_Salvage;")?.invariantName, SignalSource.FromEDName("$USS_Type_Salvage;")?.localizedName, 4 ) { isSignalSource = true },
                @"{ ""timestamp"":""2024-04-21T00:35:05Z"", ""event"":""SupercruiseDestinationDrop"", ""Type"":""$FIXED_EVENT_PROBE;"", ""Type_Localised"":""Ancient probe"", ""Threat"":0 }",
                @"{ ""timestamp"":""2023-08-13T04:49:05Z"", ""event"":""SupercruiseDestinationDrop"", ""Type"":""ANDROMEDA GALAXY Q1Y-T0H"", ""Threat"":0, ""MarketID"":3703720704 }",
                @"{ ""timestamp"":""2023-07-24T05:54:13Z"", ""event"":""SupercruiseDestinationDrop"", ""Type"":""Rorschach Hub"", ""Threat"":0, ""MarketID"":3224110080 }"
            };

        [PublicAPI("The name of the destination location, localized when applicable")]
        public string name { get; private set; }

        [PublicAPI( "The invariant name of the destination location" )]
        public string invariantName { get; private set; }

        [PublicAPI( "The threat level at the destination location (0 is lowest) (typically only used for unidentified signal sources)" )]
        public int threat { get; private set; }
        
        [PublicAPI( "True if the destination is a signal source" )]
        public bool isSignalSource { get; set; }

        // Not intended to be user facing

        public long? marketID { get; private set; }

        public DestinationArrivedEvent ( DateTime timestamp, string invariantName, string localizedName = null, int? threat = null, long? marketID = null) : base(timestamp, NAME)
        {
            this.invariantName = invariantName;
            this.name = string.IsNullOrEmpty( localizedName ) ? invariantName : localizedName;
            this.threat = threat ?? 0;
            this.marketID = marketID;
        }
    }
}
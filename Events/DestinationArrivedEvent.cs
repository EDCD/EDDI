using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class DestinationArrivedEvent (
        DateTime timestamp,
        string invariantName,
        string localizedName = null,
        int? threat = null,
        long? marketId = null )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Destination arrived";
        public const string DESCRIPTION = "Triggered when you drop into normal space at your selected destination";
        public static readonly object[] SAMPLES =
        [
            new DestinationArrivedEvent(Dates.FromString("2024-05-05T23:39:27Z") ?? DateTime.UtcNow, SignalSource.FromEDName("$USS_Type_Salvage;")?.invariantName, SignalSource.FromEDName("$USS_Type_Salvage;")?.localizedName, 4 ) { isSignalSource = true },
                @"{ ""timestamp"":""2024-04-21T00:35:05Z"", ""event"":""SupercruiseDestinationDrop"", ""Type"":""$FIXED_EVENT_PROBE;"", ""Type_Localised"":""Ancient probe"", ""Threat"":0 }",
                @"{ ""timestamp"":""2023-08-13T04:49:05Z"", ""event"":""SupercruiseDestinationDrop"", ""Type"":""ANDROMEDA GALAXY Q1Y-T0H"", ""Threat"":0, ""MarketID"":3703720704 }",
                @"{ ""timestamp"":""2023-07-24T05:54:13Z"", ""event"":""SupercruiseDestinationDrop"", ""Type"":""Rorschach Hub"", ""Threat"":0, ""MarketID"":3224110080 }"
        ];

        [PublicAPI("The name of the destination location, localized when applicable")]
        public string name { get; private set; } = string.IsNullOrEmpty( localizedName ) ? invariantName : localizedName;

        [PublicAPI( "The invariant name of the destination location" )]
        public string invariantName { get; private set; } = invariantName;

        [PublicAPI( "The threat level at the destination location (0 is lowest) (typically only used for unidentified signal sources)" )]
        public int threat { get; private set; } = threat ?? 0;

        [PublicAPI( "True if the destination is a signal source" )]
        public bool isSignalSource { get; set; }

        // Not intended to be user facing

        public long? marketID { get; private set; } = marketId;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            // "Type" may be either a signal source or proper name.
            // If proper name, it might be a fleet carrier with both name and ID.
            var type = JsonParsing.getString( data, "Type" );
            var typeLocalized = JsonParsing.getString( data, "Type_Localised" );
            var threat = JsonParsing.getOptionalInt( data, "Threat" ) ?? 0; // Typically 0 except for USS drops.
            var marketID = JsonParsing.getOptionalLong( data, "MarketID" );

            if ( type.StartsWith( '$' ) )
            {
                // Symbolic signal source name. Prefer our own localization and fallback using the provided localization string if needed.
                var signalSource = SignalSource.FromEDName( type );
                if ( signalSource != null )
                {
                    signalSource.fallbackLocalizedName = typeLocalized;
                    type = signalSource.invariantName;
                    typeLocalized = signalSource.localizedName;
                }
            }
            else
            {
                // Destination might be a fleet carrier with name and carrier ID in a single string.
                // Check and break apart if needed.
                if ( string.IsNullOrEmpty( typeLocalized ) && GeneratedRegex.FleetCarrierNameAndIdRegex().IsMatch( type ) )
                {
                    // Fleet carrier names include both the carrier name and carrier ID, we need to separate them
                    var fleetCarrierParts = GeneratedRegex.FleetCarrierNameAndIdRegex().Matches( type )[ 0 ].Groups;
                    if ( fleetCarrierParts.Count == 3 )
                    {
                        type = fleetCarrierParts[ 2 ].Value;
                        typeLocalized = fleetCarrierParts[ 1 ].Value;
                    }
                }
            }

            events.Add( new DestinationArrivedEvent( timestamp, type, typeLocalized, threat, marketID ) { raw = line, fromLoad = false } );
            return true;
        }
    }
}
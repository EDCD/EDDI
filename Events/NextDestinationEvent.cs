using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class NextDestinationEvent (
        DateTime timestamp,
        ulong? systemAddress,
        int? bodyId,
        string name,
        string localizedName = null,
        Body body = null,
        Station station = null,
        SignalSource signalSource = null )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Next destination";
        public const string DESCRIPTION = "Triggered when selecting an in-system destination";
        public static readonly NextDestinationEvent SAMPLE = new(DateTime.UtcNow, 8879744226018, 59, "$MULTIPLAYER_SCENARIO14_TITLE;", "Resource Extraction Site");

        [PublicAPI("The name of the next in-system destination")]
        public string name { get; private set; } = name;

        [PublicAPI("The localized name of the next in-system destination, if known")]
        public string localizedName { get; private set; } = localizedName;

        [PublicAPI( "The numeric ID of the destination body (if the destination is a body)" )]
        public int? bodyId { get; private set; } = bodyId;

        [PublicAPI("If the destination is a body")] 
        public bool isBody => body != null;

        [PublicAPI("If the destination is a station (including megaship or fleet carrier)")]
        public bool isStation => station != null;

        [PublicAPI("If the destination is a signal source")]
        public bool isSignalSource => signalSource != null;

        [PublicAPI("If the destination is a Point of Interest / miscellaneous location")]
        public bool isPOI => body == null && station == null && signalSource == null;

        [PublicAPI( "The numeric system address of the star system" )]
        public ulong? systemAddress { get; private set; } = systemAddress;

        // Not intended to be user facing

        public Body body { get; private set; } = body;

        public Station station { get; private set; } = station;

        public SignalSource signalSource { get; private set; } = signalSource;
    }
}
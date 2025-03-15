using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class NextDestinationEvent : Event
    {
        public const string NAME = "Next destination";
        public const string DESCRIPTION = "Triggered when selecting an in-system destination";
        public static NextDestinationEvent SAMPLE = new NextDestinationEvent(DateTime.UtcNow, 8879744226018, 59, "$MULTIPLAYER_SCENARIO14_TITLE;", "Resource Extraction Site");

        [PublicAPI("The name of the next in-system destination")]
        public string name { get; private set; }

        [PublicAPI("The localized name of the next in-system destination, if known")]
        public string localizedName { get; private set; }

        [PublicAPI( "The numeric ID of the destination body (if the destination is a body)" )]
        public int? bodyId { get; private set; }

        [PublicAPI("If the destination is a body")] 
        public bool isBody => body != null;

        [PublicAPI("If the destination is a station (including megaship or fleet carrier)")]
        public bool isStation => station != null;

        [PublicAPI("If the destination is a signal source")]
        public bool isSignalSource => signalSource != null;

        [PublicAPI("If the destination is a Point of Interest / miscellaneous location")]
        public bool isPOI => body == null && station == null && signalSource == null;

        [PublicAPI( "The numeric system address of the star system" )]
        public ulong? systemAddress { get; private set; }

        // Not intended to be user facing

        public Body body { get; private set; }

        public Station station { get; private set; }

        public SignalSource signalSource { get; private set; }

        public NextDestinationEvent(DateTime timestamp, ulong? systemAddress, int? bodyId, string name, string localizedName = null, Body body = null, Station station = null, SignalSource signalSource = null) : base(timestamp, NAME)
        {
            this.systemAddress = systemAddress;
            this.bodyId = bodyId;
            this.name = name;
            this.localizedName = localizedName;
            this.body = body;
            this.station = station;
            this.signalSource = signalSource;
        }
    }
}
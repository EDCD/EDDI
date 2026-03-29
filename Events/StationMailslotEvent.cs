using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class StationMailslotEvent ( DateTime timestamp ) : Event( timestamp, NAME )
    {
        public const string NAME = "Station mailslot";
        public const string DESCRIPTION = "Triggered when your ship enters through a station's mailslot without the aid of a docking computer";
        public static readonly StationMailslotEvent SAMPLE = new( DateTime.UtcNow );
    }
}

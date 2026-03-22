using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class EnteredSupercruiseEvent (
        DateTime timestamp,
        string system,
        ulong systemAddress,
        bool? taxi,
        bool? multicrew )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Entered supercruise";
        public const string DESCRIPTION = "Triggered when your ship enters supercruise";
        public const string SAMPLE = "{ \"timestamp\":\"2021-07-19T05:28:08Z\", \"event\":\"SupercruiseEntry\", \"Taxi\":false, \"Multicrew\":false, \"StarSystem\":\"Azaladshu\", \"SystemAddress\":9467852826025 }";

        [PublicAPI("The system at which the commander has entered supercruise")]
        public string system { get; private set; } = system;

        [PublicAPI( "The numeric system address of the star system at which the commander has entered supercruise" )]
        public ulong systemAddress { get; private set; } = systemAddress;

        [PublicAPI("True if the ship is a transport (e.g. taxi or dropship)")]
        public bool? taxi { get; private set; } = taxi;

        [PublicAPI("True if the ship is belongs to another player")]
        public bool? multicrew { get; private set; } = multicrew;

        // Not intended to be user facing

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var system = JsonParsing.getString(data, "StarySystem");
            var systemAddress = JsonParsing.getULong(data, "SystemAddress");
            var taxi = JsonParsing.getOptionalBool(data, "Taxi");
            var multicrew = JsonParsing.getOptionalBool(data, "Multicrew");
            events.Add( new EnteredSupercruiseEvent( timestamp, system, systemAddress, taxi, multicrew ) { raw = line, fromLoad = false } );
            return true;
        }
    }
}

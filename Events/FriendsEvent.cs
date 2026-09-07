using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class FriendsEvent ( DateTime timestamp, string name, string status ) : Event( timestamp, NAME )
    {
        public const string NAME = "Friends status";
        public const string DESCRIPTION = "Triggered when a friendly commander changes status";
        public const string SAMPLE = "{ \"timestamp\":\"2017-08-24T17:22:03Z\", \"event\":\"Friends\", \"Status\":\"Online\", \"Name\":\"Ipsum\" }";

        [PublicAPI("the friend's commander name")]
        public string name { get; private set; } = name;

        [PublicAPI("one of the following: Requested, Declined, Added, Lost, Offline, Online")]
        public string status { get; private set; } = status;

        // Not intended to be user facing

        [Obsolete("Use 'name' instead")]
        public string friend => name; // Deprecated but preserved for backwards compatibility

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var status = JsonParsing.getString(data, "Status");
            var name = JsonParsing.getString(data, "Name");
            name = name.Replace( "$cmdr_decorate:#name=", "Commander " ).Replace( ";", "" ).Replace( "&", "Commander " );
            var @event = new FriendsEvent( timestamp, name, status ) { raw = line, fromLoad = fromLogLoad };

            events.Add( @event );
            return true;
        }
    }
}

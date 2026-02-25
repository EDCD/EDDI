using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MessageSentEvent : Event
    {
        public const string NAME = "Message sent";
        public const string DESCRIPTION = "Triggered when you send a message";
        public static string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"SendText\",\"To\":\"HRC1\",\"Message\":\"Hello\"}";

        [PublicAPI("The name of the player to which the message was sent")]
        public string to { get; private set; }

        [PublicAPI("The message")]
        public string message { get; private set; }

        public MessageSentEvent(DateTime timestamp, string to, string message) : base(timestamp, NAME)
        {
            this.to = to;
            this.message = message;
        }

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var to = JsonParsing.getString(data, "To");
            var message = JsonParsing.getString(data, "Message");
            events.Add( new MessageSentEvent( timestamp, to, message ) { raw = line, fromLoad = false } );
            return true;
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class MessageSentEvent : Event
    {
        public const string NAME = "Message sent";
        public const string DESCRIPTION = "Triggered when you send a message";
        public static string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"SendText\",\"To\":\"HRC1\",\"Message\":\"Hello\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MessageSentEvent()
        {
            VARIABLES.Add("to", "The name of the player to which the message was sent");
            VARIABLES.Add("message", "The message");
        }

        [JsonProperty("to")]
        public string to { get; private set; }

        [JsonProperty("message")]
        public string message { get; private set; }

        public MessageSentEvent(DateTime timestamp, string to, string message) : base(timestamp, NAME)
        {
            this.to = to;
            this.message = message;
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class MessageReceivedEvent : Event
    {
        public const string NAME = "Message received";
        public const string DESCRIPTION = "Triggered when you receive a message";
        public static MessageReceivedEvent SAMPLE = new MessageReceivedEvent(DateTime.Now, "HRC1", "Hello");
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MessageReceivedEvent()
        {
            SAMPLE.raw = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"ReceiveText\",\"From\":\"HRC1\",\"Message\":\"Hello\"}";

            VARIABLES.Add("from", "The name of the player who sent the message");
            VARIABLES.Add("message", "The message");
        }

        [JsonProperty("from")]
        public string from { get; private set; }

        [JsonProperty("message")]
        public string message { get; private set; }

        public MessageReceivedEvent(DateTime timestamp, string from, string message) : base(timestamp, NAME)
        {
            this.from = from;
            this.message = message;
        }
    }
}

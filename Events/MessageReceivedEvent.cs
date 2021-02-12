using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using EddiDataDefinitions;

namespace EddiEvents
{
    public class MessageReceivedEvent : Event
    {
        public const string NAME = "Message received";
        public const string DESCRIPTION = "Triggered when you receive a message";
        public const string SAMPLE = "{\"timestamp\":\"2017-10-01T18:51:52Z\", \"event\":\"ReceiveText\", \"From\":\"HRC1\", \"Message\":\"Hello\", \"Channel\":\"player\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MessageReceivedEvent()
        {
            VARIABLES.Add("from", "The name of the source who sent the message");
            VARIABLES.Add("player", "True if the sender is a player");
            VARIABLES.Add("source", "The source of the transmission");
            VARIABLES.Add("channel", "The channel in which the message came (e.g. direct, local, multicrew, wing, squadron, starsystem, npc)");
            VARIABLES.Add("message", "The message");
        }

        [JsonProperty("from")]
        public string from { get; private set; }

        [JsonProperty("source")]
        public string source => Source.localizedName;

        [JsonProperty("player")]
        public bool player { get; private set; }

        [JsonProperty("channel")]
        public string channel { get; private set; }

        [JsonProperty("message")]
        public string message { get; private set; }

        // Not intended to be user facing
        public MessageSource Source { get; }

        public MessageReceivedEvent(DateTime timestamp, string from, MessageSource source, bool player, string channel, string message) : base(timestamp, NAME)
        {
            this.from = from;
            Source = source;
            this.player = player;
            this.channel = channel;
            this.message = message;
        }
    }
}

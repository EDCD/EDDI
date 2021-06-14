using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MessageReceivedEvent : Event
    {
        public const string NAME = "Message received";
        public const string DESCRIPTION = "Triggered when you receive a message";
        public const string SAMPLE = "{\"timestamp\":\"2017-10-01T18:51:52Z\", \"event\":\"ReceiveText\", \"From\":\"HRC1\", \"Message\":\"Hello\", \"Channel\":\"player\"}";

        [PublicAPI("The name of the source who sent the message")]
        public string from { get; private set; }

        [PublicAPI("The localized source of the transmission")]
        public string source => Source.localizedName;

        [PublicAPI("The invariant source of the transmission")]
        public string source_invariant => Source.invariantName;

        [PublicAPI("True if the sender is a player")]
        public bool player { get; private set; }

        [PublicAPI("The localized channel in which the message came (i.e. friend, local, multicrew, npc, player, squadron, starsystem, voicechat, wing)")]
        public string channel => Channel.localizedName;

        [PublicAPI("The invariant channel in which the message came (i.e. friend, local, multicrew, npc, player, squadron, starsystem, voicechat, wing)")]
        public string channel_invariant => Channel.invariantName;

        [PublicAPI("The message")]
        public string message { get; private set; }

        // Not intended to be user facing

        public MessageChannel Channel { get; }

        public MessageSource Source { get; }

        public MessageReceivedEvent(DateTime timestamp, string from, MessageSource source, bool player, MessageChannel channel, string message) : base(timestamp, NAME)
        {
            this.from = from;
            Source = source;
            this.player = player;
            Channel = channel;
            this.message = message;
        }
    }
}

using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class RespawnedEvent : Event
    {
        public const string NAME = "Respawned";
        public const string DESCRIPTION = "Triggered when you respawn (either after injury or after handing yourself in to local authorities)";
        public const string SAMPLE = @"{ ""timestamp"":""2016-09-20T12:27:59Z"", ""event"":""Resurrect"", ""Option"":""rebuy"", ""Cost"":36479, ""Bankrupt"":false }";

        [PublicAPI("The action triggering the respawn. One of 'rebuy', 'recover', 'rejoin', or 'handin'")]
        public string trigger { get; private set; }

        [PublicAPI("The price paid to complete the respawn, if any")]
        public long price { get; private set; }

        public RespawnedEvent(DateTime timestamp, string option, long price) : base(timestamp, NAME)
        {
            this.trigger = option;
            this.price = price;
        }
    }
}
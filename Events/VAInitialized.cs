using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class VAInitializedEvent : Event
    {
        public const string NAME = "VA initialized";
        public const string DESCRIPTION = "Triggered when the VoiceAttack plugin is first initialized";
        public const string SAMPLE = @"{""timestamp"":""2017-09-04T00:20:48Z"", ""event"":""VAInitialized"" }";

        public VAInitializedEvent(DateTime timestamp) : base(timestamp, NAME)
        { }
    }
}

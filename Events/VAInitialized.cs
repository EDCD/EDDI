using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class VAInitializedEvent : Event
    {
        public const string NAME = "VA initialized";
        public const string DESCRIPTION = "Triggered when the VoiceAttack plugin is first initialized";
        public const string SAMPLE = @"{""timestamp"":""2017-09-04T00:20:48Z"", ""event"":""VAInitialized"" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static VAInitializedEvent()
        {
        }

        public VAInitializedEvent(DateTime timestamp) : base(timestamp, NAME)
        {
        }
    }
}

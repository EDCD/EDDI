using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class SRVDockedEvent : Event
    {
        public const string NAME = "SRV docked";
        public const string DESCRIPTION = "Triggered when you dock an SRV with your ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"DockSRV\"}";

        [PublicAPI("The srv's id")]
        public int? id { get; private set; }

        public SRVDockedEvent(DateTime timestamp, int? id) : base(timestamp, NAME)
        {
            this.id = id;
        }
    }
}

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EddiInaraService
{
    public class InaraAPIEvent
    {
        public string eventName { get; private set; }

        public string eventTimestamp => eventTimeStamp.ToString("s") + "Z";

        public object eventData { get; private set; }

        public int? eventCustomID { get; set; } // Optional index. May be set while processing.

        // Helper properties

        [JsonIgnore] public DateTime eventTimeStamp { get; set; }

        public InaraAPIEvent(DateTime eventTimeStamp, string eventName, Dictionary<string, object> eventData,
            int? eventCustomID = null)
        {
            this.eventTimeStamp = eventTimeStamp;
            this.eventName = eventName;
            this.eventData = eventData;
            this.eventCustomID = eventCustomID;
        }

        public InaraAPIEvent(DateTime eventTimeStamp, string eventName, List<Dictionary<string, object>> eventData,
            int? eventCustomID = null)
        {
            this.eventTimeStamp = eventTimeStamp;
            this.eventName = eventName;
            this.eventData = eventData;
            this.eventCustomID = eventCustomID;
        }
    }
}

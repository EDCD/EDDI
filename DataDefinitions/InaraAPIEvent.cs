using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class InaraAPIEvent
    {
        public string eventName { get; set; }

        public string eventTimestamp => eventTimeStamp.ToString("s") + "Z";

        public object eventData { get; set; }

        public int? eventCustomID { get; set; } // Optional index

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

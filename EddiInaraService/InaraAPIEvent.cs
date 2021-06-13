using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiInaraService
{
    public class InaraAPIEvent
    {
        public string eventName { get; private set; }

        public object eventData { get; private set; }

        public int? eventCustomID { get; set; } // Optional index. May be set while processing.

        public DateTime eventTimestamp { get; set; }

        public InaraAPIEvent(DateTime eventTimestamp, string eventName, Dictionary<string, object> eventData,
            int? eventCustomID = null)
        {
            this.eventTimestamp = eventTimestamp;
            this.eventName = eventName;
            this.eventData = eventData;
            this.eventCustomID = eventCustomID;
        }

        public InaraAPIEvent(DateTime eventTimestamp, string eventName, List<Dictionary<string, object>> eventData,
            int? eventCustomID = null)
        {
            this.eventTimestamp = eventTimestamp;
            this.eventName = eventName;
            this.eventData = eventData;
            this.eventCustomID = eventCustomID;
        }
    }
}

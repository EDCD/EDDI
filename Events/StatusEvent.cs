using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class StatusEvent : Event
    {
        public const string NAME = "Status";
        public const string DESCRIPTION = "Triggered when your real-time status is updated";
        public const string SAMPLE = null;
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static StatusEvent()
        {
        }

        public Status status { get; private set; }

        public StatusEvent(DateTime timestamp, Status status) : base(timestamp, NAME)
        {
            this.status = status;
        }
    }
}

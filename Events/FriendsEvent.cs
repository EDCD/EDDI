using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class FriendsEvent : Event
    {
        public const string NAME = "Friends status";
        public const string DESCRIPTION = "Triggered when a friendly commander changes status";
        public const string SAMPLE = "{ \"timestamp\":\"2017-08-24T17:22:03Z\", \"event\":\"Friends\", \"Status\":\"Online\", \"Name\":\"Ipsum\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FriendsEvent()
        {
            VARIABLES.Add("name", "the friend's commander name");
            VARIABLES.Add("status", "Status: one of the following: Requested, Declined, Added, Lost, Offline, Online");
        }

        public string name { get; private set; }
        public string status { get; private set; }
        public string friend { get; private set; } // Deprecated but preserved for backwards compatibility

        public FriendsEvent(DateTime timestamp, string name, string status) : base(timestamp, NAME)
        {
            this.name = name;
            this.status = status;
            this.friend = name;
        }
    }
}

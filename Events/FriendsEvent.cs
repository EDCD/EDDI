using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class FriendsEvent : Event
    {
        public const string NAME = "Friends status";
        public const string DESCRIPTION = "Triggered when a friendly commander changes status";
        public const string SAMPLE = "{ \"timestamp\":\"2017-08-24T17:22:03Z\", \"event\":\"Friends\", \"Status\":\"Online\", \"Name\":\"Ipsum\" }";

        [PublicAPI("the friend's commander name")]
        public string name { get; private set; }

        [PublicAPI("one of the following: Requested, Declined, Added, Lost, Offline, Online")]
        public string status { get; private set; }

        // Not intended to be user facing

        [Obsolete("Use 'name' instead")]
        public string friend => name; // Deprecated but preserved for backwards compatibility

        public FriendsEvent(DateTime timestamp, string name, string status) : base(timestamp, NAME)
        {
            this.name = name;
            this.status = status;
        }
    }
}

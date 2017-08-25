using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            VARIABLES.Add("status", "Status: one of the following: Requested, Declined, Added, Lost, Offline, Online");
            VARIABLES.Add("friend", "the friend's commander name");          
        }

        public string status { get; private set; }
        public string friend { get; private set; }        

        public FriendsEvent(DateTime timestamp, string status, string friend) : base(timestamp, NAME)
        {
            this.status = status;
            this.friend = friend;            
        }
    }
}

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
        public const string SAMPLE = "";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FriendsEvent()
        {
            VARIABLES.Add("status", "Status: one of the following: Requested, Declined, Added, Lost, Offline, Online");
            VARIABLES.Add("name", "the friend's commander name");          
        }

        public string status { get; private set; }
        public string name { get; private set; }        

        public FriendsEvent(DateTime timestamp, string status, string name) : base(timestamp, NAME)
        {
            this.status = status;
            this.name = name;            
        }
    }
}

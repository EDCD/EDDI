using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousEvents
{
    public class EnteredNormalSpaceEvent : Event
    {
        public const string NAME = "Entered normal space";
        public static EnteredNormalSpaceEvent SAMPLE = new EnteredNormalSpaceEvent(DateTime.Now, "Jameson Memorial");

        [JsonProperty("body")]
        public string body{ get; private set; }

        public EnteredNormalSpaceEvent(DateTime timestamp, string body) : base(timestamp, NAME)
        {
            this.body = body;
        }
    }
}

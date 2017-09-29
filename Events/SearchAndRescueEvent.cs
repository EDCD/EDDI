using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class SearchAndRescueEvent: Event
    {
        public const string NAME = "Search and rescue";
        public const string DESCRIPTION = "Triggered when delivering items to a Search and Rescue contact";
        public const string SAMPLE = "{ \"timestamp\":\"2017-08-26T01:58:24Z\", \"event\":\"SearchAndRescue\", \"Name\":\"occupiedcryopod\", \"Count\":2, \"Reward\":5310 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SearchAndRescueEvent()
        {
            VARIABLES.Add("commodity", "The item recovered (this is an object)");
            VARIABLES.Add("amount", "The amount of the item recovered");
            VARIABLES.Add("reward", "The monetary reward for completing the search and rescue");
        }

        public Commodity commodity { get; private set; }

        public int? amount { get; private set; }

        public long reward { get; private set; }

        public SearchAndRescueEvent(DateTime timestamp, Commodity commodity, int? amount, long reward) : base(timestamp, NAME)
        {
            this.commodity = commodity;
            this.amount = amount;
            this.reward = reward;
        }
    }
}

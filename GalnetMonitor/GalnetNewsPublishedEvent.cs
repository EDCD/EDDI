using EddiEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalnetMonitor
{
    class GalnetNewsPublishedEvent : Event
    {
        public const string NAME = "Galnet news published";
        public const string DESCRIPTION = "Triggered when news is published on Galnet";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static GalnetNewsPublishedEvent()
        {
            VARIABLES.Add("items", "The published news items");
        }

        public List<News> items { get; private set; }

        public GalnetNewsPublishedEvent(DateTime timestamp, List<News> items) : base(timestamp, NAME)
        {
            this.items = items;
        }
    }
}

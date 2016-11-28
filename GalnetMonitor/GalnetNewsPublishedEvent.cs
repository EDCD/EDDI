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
        public static GalnetNewsPublishedEvent SAMPLE = new GalnetNewsPublishedEvent(DateTime.Now, new List<News>() { new News(DateTime.Now, "Galactic News: Ram Tah Releases Statement", @"Earlier this month, engineer Ram Tah announced a research programme designed to uncover the secrets of the Synuefe ruins. The engineer asked pilots to deliver data from the site to his laboratory in the hope that he would be able to decipher it.  But Ran Tah's silence has prompted some to speculate that the engineer might not be up to the task of decoding the data, and that his attempt was nothing more than the naive dream of a backwater mechanic.  When approached with these criticisms, Ram Tah said: ""These things take time. I believe I have made a breakthrough, but I will not be pressured into offering untested claims. As soon as I have results, the galaxy will know."""),
        new News(DateTime.Now, "Galactic News: Weekly Conflict Report", @"This report presents the latest data on conflict among the galaxy's minor factions.  Here are the latest factions to experience a civil war: Left Party of HIP 43845 HIP 5712 State Corp. Green Party of HIP 103138 Uru Federal Interstellar Fusang Fortune Commodities Chamunda Crimson Gang Jamatia Central Corp. V886 Centauri Corporation HIP 32350 Industries Purungurawn Purple Society Civil wars occur when minor factions compete for control of major assets such as starports. When a faction is involved in a civil war, the standard of living, development level and security level in the system it controls are temporarily reduced. Combat activities can bring a civil war to an end.") });

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

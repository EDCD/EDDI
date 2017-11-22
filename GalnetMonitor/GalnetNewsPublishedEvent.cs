using EddiEvents;
using System;
using System.Collections.Generic;

namespace GalnetMonitor
{
    class GalnetNewsPublishedEvent : Event
    {
        public const string NAME = "Galnet news published";
        public const string DESCRIPTION = "Triggered when news is published on Galnet";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();
        public static GalnetNewsPublishedEvent SAMPLE = new GalnetNewsPublishedEvent(DateTime.Now, new List<News>() { new News("testuuid1", "Article", "Galactic News: Ram Tah Releases Statement", @"Earlier this month, engineer Ram Tah announced a research programme designed to uncover the secrets of the Synuefe ruins.", DateTime.Now, false),
        new News("testuuid2", "Conflict Report", "Galactic News: Weekly Conflict Report", @"This report presents the latest data on conflict among the galaxy's minor factions.  Here are the latest factions to experience a civil war: Left Party of HIP 43845 HIP 5712 State Corp. Green Party of HIP 103138 Uru Federal Interstellar Fusang Fortune Commodities Chamunda Crimson Gang Jamatia Central Corp. V886 Centauri Corporation HIP 32350 Industries Purungurawn Purple Society Civil wars occur when minor factions compete for control of major assets such as starports. When a faction is involved in a civil war, the standard of living, development level and security level in the system it controls are temporarily reduced. Combat activities can bring a civil war to an end.", DateTime.Now, false) });

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

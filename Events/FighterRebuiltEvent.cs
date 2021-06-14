using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class FighterRebuiltEvent : Event
    {
        public const string NAME = "Fighter rebuilt";
        public const string DESCRIPTION = "Triggered when a ship's fighter is rebuilt in the hangar";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"FighterRebuilt\",\"Loadout\":\"four\", \"ID\":134}";
        
        [PublicAPI("The loadout of the fighter")]
        public string loadout { get; private set; }

        [PublicAPI("The fighter's id")]
        public int id { get; private set; }

        public FighterRebuiltEvent(DateTime timestamp, string loadout, int id) : base(timestamp, NAME)
        {
            this.loadout = loadout;
            this.id = id;
        }
    }
}

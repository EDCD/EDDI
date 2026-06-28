using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class FighterRebuiltEvent ( DateTime timestamp, string loadout, int id ) : Event( timestamp, NAME )
    {
        public const string NAME = "Fighter rebuilt";
        public const string DESCRIPTION = "Triggered when a ship's fighter is rebuilt in the hangar";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"FighterRebuilt\",\"Loadout\":\"four\", \"ID\":134}";
        
        [PublicAPI("The loadout of the fighter")]
        public string loadout { get; private set; } = loadout;

        [PublicAPI("The fighter's id")]
        public int id { get; private set; } = id;

        public static bool Handle ( DateTime timestamp, string edType, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var loadout = JsonParsing.getString(data, "Loadout");
            var fighterId = JsonParsing.getInt(data, "ID");
            events.Add( new FighterRebuiltEvent( timestamp, loadout, fighterId ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}

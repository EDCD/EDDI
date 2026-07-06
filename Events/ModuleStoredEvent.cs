using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ModuleStoredEvent (
        DateTime timestamp,
        string ship,
        int shipid,
        string slot,
        Module module,
        long? cost,
        string engineermodifications,
        Module replacementmodule,
        long marketId )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Module stored";
        public const string DESCRIPTION = "Triggered when you store a module";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleStore\", \"MarketID\": 128666762, \"Slot\":\"MediumHardpoint2\", \"Ship\":\"cobramkiii\", \"ShipID\":1, \"Hot\":true, \"StoredItem\":\"hpt_pulselaser_fixed_medium\", \"EngineerModifications\":\"\", \"Cost\":500  }";

        [PublicAPI("The ship from which the module was stored")]
        public string ship => shipDefinition?.model;

        [PublicAPI("The ID of the ship from which the module was stored")]
        public int shipid { get; private set; } = shipid;

        [PublicAPI("The outfitting slot")]
        public string slot { get; private set; } = slot;

        [PublicAPI("The module (object) being stored")]
        public Module module { get; private set; } = module;

        [PublicAPI("The cost of storage (if any)")]
        public long? cost { get; private set; } = cost;

        [PublicAPI("The name of the modification blueprint")]
        public string engineermodifications { get; private set; } = engineermodifications;

        [PublicAPI("The module (object) replacement (if a core module)")]
        public Module replacementmodule { get; private set; } = replacementmodule;

        // Not intended to be user facing

        public long marketId { get; private set; } = marketId;

        public Ship shipDefinition { get; private set; } = ShipDefinitions.FromEDModel(ship);

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var marketId = JsonParsing.getLong(data, "MarketID");
            data.TryGetValue( "ShipID", out var val );
            var shipId = (int)(long)val;
            var ship = JsonParsing.getString(data, "Ship");

            var slot = JsonParsing.getString(data, "Slot");
            var module = Module.FromEDName(JsonParsing.getString(data, "StoredItem"));
            module.hot = JsonParsing.getBool( data, "Hot" );
            var engineerModifications = JsonParsing.getString(data, "EngineerModifications");
            module.modified = engineerModifications != null;
            module.engineerlevel = JsonParsing.getOptionalInt( data, "Level" ) ?? 0;
            module.engineermodification = Blueprint.FromEDNameAndGrade( engineerModifications, module.engineerlevel ) ?? Blueprint.None;
            module.engineerquality = JsonParsing.getOptionalDecimal( data, "Quality" ) ?? 0;

            var cost = JsonParsing.getOptionalLong(data, "Cost");

            var replacementModule = Module.FromEDName(JsonParsing.getString(data, "ReplacementItem"));
            if ( replacementModule != null )
            {
                replacementModule.price = replacementModule.value;
                replacementModule.enabled = true;
                replacementModule.priority = 1;
                replacementModule.health = 100;
                replacementModule.modified = false;
            }

            events.Add( new ModuleStoredEvent( timestamp, ship, shipId, slot, module, cost, engineerModifications, replacementModule, marketId ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}

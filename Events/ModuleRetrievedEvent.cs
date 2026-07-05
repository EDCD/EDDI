using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ModuleRetrievedEvent (
        DateTime timestamp,
        string ship,
        int shipid,
        string slot,
        Module module,
        long? cost,
        string engineermodifications,
        Module swapoutmodule,
        long marketId )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Module retrieved";
        public const string DESCRIPTION = "Triggered when you fetch a previously stored module";
        public const string SAMPLE = "{ \"timestamp\":\"2016-06-10T14:32:03Z\", \"event\":\"ModuleRetrieve\", \"MarketID\": 128666762, \"Slot\":\"MediumHardpoint2\", \"Ship\":\"cobramkiii\", \"ShipID\":1, \"Hot\":true, \"RetrievedItem\":\"hpt_pulselaser_fixed_medium\", \"EngineerModifications\":\"\", \"SwapOutItem\":\"hpt_multicannon_gimbal_medium\", \"Cost\":500  }";

        [PublicAPI("The ship for which the module was retrieved")]
        public string ship => shipDefinition?.model;

        [PublicAPI("The ID of the ship for which the module was retrieved")]
        public int shipid { get; private set; } = shipid;

        [PublicAPI("The outfitting slot")]
        public string slot { get; private set; } = slot;

        [PublicAPI("The module (object) retrieved from storage")]
        public Module module { get; private set; } = module;

        [PublicAPI("The cost of retrieval")]
        public long? cost { get; private set; } = cost;

        [PublicAPI("The name of the modification blueprint")]
        public string engineermodifications { get; private set; } = engineermodifications;

        [PublicAPI("The module (object) swapped out (if the slot was not empty)")]
        public Module swapoutmodule { get; set; } = swapoutmodule;

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
            var module = Module.FromEDName(JsonParsing.getString(data, "RetrievedItem"));
            module.hot = JsonParsing.getBool( data, "Hot" );
            var engineerModifications = JsonParsing.getString(data, "EngineerModifications");
            module.modified = engineerModifications != null;
            module.engineerlevel = JsonParsing.getOptionalInt( data, "Level" ) ?? 0;
            module.engineermodification = Blueprint.FromEDNameAndGrade( engineerModifications, module.engineerlevel ) ?? Blueprint.None;
            module.engineerquality = JsonParsing.getOptionalDecimal( data, "Quality" ) ?? 0;

            // Set retrieved module defaults
            module.price = module.value;
            module.enabled = true;
            module.priority = 1;
            module.health = 100;

            // Set module cost
            var cost = JsonParsing.getOptionalLong(data, "Cost");

            var swapoutModule = Module.FromEDName(JsonParsing.getString(data, "SwapOutItem"));

            events.Add( new ModuleRetrievedEvent( timestamp, ship, shipId, slot, module, cost, engineerModifications, swapoutModule, marketId ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}

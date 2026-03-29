using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CargoEvent (
        DateTime timestamp,
        bool update,
        string vessel,
        List<CargoInfoItem> inventory,
        int cargocarried )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Cargo";
        public const string DESCRIPTION = "Triggered when a vehicle cargo inventory is updated";
        public const string SAMPLE = null;

        // Not intended to be user facing

        public bool update { get; private set; } = update;

        public string vessel { get; private set; } = vessel;

        public List<CargoInfoItem> inventory { get; private set; } = inventory;

        public int cargocarried { get; private set; } = cargocarried;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, string vehicle, ref List<Event> events, bool fromLogLoad )
        {
            var inventory = new List<CargoInfoItem>();

            var vessel = JsonParsing.getString(data, "Vessel") ?? vehicle;
            var cargocarried = JsonParsing.getOptionalInt(data, "Count") ?? 0;
            data.TryGetValue( "Inventory", out var val );
            if ( val != null )
            {
                var inventoryJson = (List<object>)val;
                foreach ( var cargoJson in inventoryJson.Cast<IDictionary<string, object>>() )
                {
                    var name = JsonParsing.getString(cargoJson, "Name");
                    var missionid = JsonParsing.getOptionalULong(cargoJson, "MissionID");
                    var count = JsonParsing.getInt(cargoJson, "Count");
                    var stolen = JsonParsing.getInt(cargoJson, "Stolen");
                    var info = new CargoInfoItem(name, missionid, count, stolen);
                    inventory.Add( info );
                }
                events.Add( new CargoEvent( timestamp, false, vessel, inventory, cargocarried ) { raw = line, fromLoad = fromLogLoad } );
                return true;
            }
            else if ( CargoInfo.TryFromFile( timestamp, vessel, cargocarried, out var info, out line ) && info != null )
            {
                events.Add( new CargoEvent( timestamp, true, vessel, info.Inventory, cargocarried ) { raw = line, fromLoad = fromLogLoad } );
                return true;
            }

            return false;
        }
    }
}

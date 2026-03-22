using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class VehicleDestroyedEvent (
        DateTime timestamp,
        string vehicle,
        VehicleDefinition vehicleDefinition,
        int? id )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Vehicle destroyed";
        public const string DESCRIPTION = "Triggered when your vehicle (fighter or SRV) is destroyed";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-22T10:53:19Z\",\"event\":\"FighterDestroyed\", \"ID\":13}";

        [PublicAPI("The vehicle that was destroyed (e.g. 'fighter' or 'srv')")]
        public string vehicle { get; private set; } = vehicle;

        [PublicAPI("The vehicle's id")]
        public int? id { get; private set; } = id;

        [PublicAPI("The localized SRV type (if the vehicle was an SRV)")]
        public string srvType => vehicle == "srv" ? vehicleDefinition?.localizedName : null;

        [PublicAPI("The invariant SRV type (if the vehicle was an SRV)")]
        public string srvTypeInvariant => vehicle == "srv" ? vehicleDefinition?.invariantName : null;

        // Not intended to be public facing at this time
        public VehicleDefinition vehicleDefinition { get; private set; } = vehicleDefinition;

        public static bool Handle ( DateTime timestamp, string edType, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading
            var vehicle = edType.Replace("Destroyed", "").ToLowerInvariant(); // e.g. FighterDestroyed or SRVDestroyed
            if ( vehicle == "fighter" )
            {
                var fighterId = JsonParsing.getInt(data, "ID");
                events.Add( new VehicleDestroyedEvent( timestamp, vehicle, null, fighterId ) { raw = line, fromLoad = false } );
            }

            if ( vehicle == "srv" )
            {
                var srvId = JsonParsing.getOptionalInt(data, "ID");
                var vehicleDefinition = VehicleDefinition.FromEDName(JsonParsing.getString(data, "SRVType"));
                vehicleDefinition.fallbackLocalizedName = JsonParsing.getString( data, "SRVType_Localised" );
                events.Add( new VehicleDestroyedEvent( timestamp, vehicle, vehicleDefinition, srvId ) { raw = line, fromLoad = false } );
            }
            return true;
        }
    }
}

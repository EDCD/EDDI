using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class StoredShipsEvent (
        DateTime timestamp,
        long marketId,
        string station,
        string system,
        List<Ship> shipyard )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Stored ships";
        public const string DESCRIPTION = "Triggered when the `Shipyard` screen is opened, providing a list of all stored ships";
        public const string SAMPLE = null;

        // Not intended to be user facing

        public string station { get; private set; } = station;

        public string system { get; private set; } = system;

        public List<Ship> shipyard { get; set; } = shipyard;

        public long marketId { get; private set; } = marketId;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var marketId = JsonParsing.getLong(data, "MarketID");
            var system = JsonParsing.getString(data, "StarSystem");
            var station = JsonParsing.getString(data, "StationName");

            var shipyard = new List<Ship>();
            foreach ( var type in new string[] { "ShipsHere", "ShipsRemote" } )
            {
                data.TryGetValue( type, out var val );
                var shipsData = (List<object>)val;
                if ( shipsData != null )
                {
                    foreach ( var shipData in shipsData.Cast<IDictionary<string, object>>() )
                    {
                        var shipType = JsonParsing.getString(shipData, "ShipType");
                        var ship = ShipDefinitions.FromEDModel(shipType);
                        if ( ship != null )
                        {
                            ship.LocalId = JsonParsing.getInt( shipData, "ShipID" );
                            ship.name = JsonParsing.getString( shipData, "Name" );
                            ship.value = JsonParsing.getLong( shipData, "Value" );
                            ship.hot = JsonParsing.getOptionalBool( shipData, "Hot" ) ?? false;
                            ship.intransit = JsonParsing.getOptionalBool( shipData, "InTransit" ) ?? false;
                            ship.transferprice = JsonParsing.getOptionalLong( shipData, "TransferPrice" );
                            ship.transfertime = JsonParsing.getOptionalLong( shipData, "TransferTime" );

                            var shipSystemName = JsonParsing.getString(shipData, "StarSystem");
                            var shipMarketID = JsonParsing.getOptionalLong( shipData, "ShipMarketID" );
                            ship.StoredLocation = new Ship.Location(
                                string.IsNullOrEmpty( shipSystemName ) ? system : shipSystemName,
                                0,
                                null,
                                null,
                                null,
                                type == "ShipsHere" ? station : null,
                                shipMarketID ?? marketId );
                            shipyard.Add( ship );
                        }
                    }
                }
            }
            events.Add( new StoredShipsEvent( timestamp, marketId, station, system, shipyard ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
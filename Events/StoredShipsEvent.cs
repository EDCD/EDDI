using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class StoredShipsEvent : Event
    {
        public const string NAME = "Stored ships";
        public const string DESCRIPTION = "Triggered when the `Shipyard` screen is opened, providing a list of all stored ships";
        public const string SAMPLE = @"{ ""timestamp"":""2017-10-04T10:07:21Z"", ""event"":""StoredShips"", ""StationName"":""Jameson Memorial"", ""StarSystem"":""Shinrarta Dezhra"", ""ShipsHere"":[ { ""ShipID"":64, ""ShipType"":""sidewinder"", ""Value"":567962 }, { ""ShipID"":20, ""ShipType"":""empire_eagle"", ""Value"":6373956 } ], ""ShipsRemote"":[ { ""ShipID"":0, ""ShipType"":""CobraMkIII"", ""StarSystem"":""Beta-1 Tucanae"", ""TransferPrice"":3777, ""TransferTime"":1590, ""Value"":9464239 } ] } ";


        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static StoredShipsEvent()
        {
        }

        public long marketId { get; private set; }
        public string station { get; private set; }
        public string system { get; private set; }
        public List<Ship> shipsHere { get; set; }
        public List<Ship> shipsRemote { get; set; }

        public StoredShipsEvent(DateTime timestamp, long marketId, string station, string system, List<Ship> shipsHere, List<Ship> shipsRemote) : base(timestamp, NAME)
        {
            this.marketId = marketId;
            this.station = station;
            this.system = system;
            this.shipsHere = shipsHere;
            this.shipsRemote = shipsRemote;
        }
    }
}
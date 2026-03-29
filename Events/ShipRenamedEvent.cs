using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipRenamedEvent ( DateTime timestamp, string ship, int shipid, string name, string ident )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Ship renamed";
        public const string DESCRIPTION = "Triggered when you rename a ship";
        public const string SAMPLE = @"{""timestamp"":""2016-09-20T18:14:26Z"",""event"":""SetUserShipName"",""Ship"":""federation_corvette"",""ShipID"":1,""UserShipName"":""Shieldless wonder"",""UserShipId"":""NCC-1701""}";

        [PublicAPI("The model of the ship that was renamed")]
        public string ship => shipDefinition?.model;

        [PublicAPI("The ID of the ship that was renamed")]
        public int? shipid { get; private set; } = shipid;

        [PublicAPI("The new name of the ship")]
        public string name { get; private set; } = name;

        [PublicAPI("The new ident of the ship")]
        public string ident { get; private set; } = ident;

        // Not intended to be user facing

        public Ship shipDefinition => ShipDefinitions.FromEDModel(edModel);

        public string edModel { get; private set; } = ship;
    }
}

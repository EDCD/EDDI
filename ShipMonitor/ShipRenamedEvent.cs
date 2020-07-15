using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiShipMonitor
{
    public class ShipRenamedEvent : Event
    {
        public const string NAME = "Ship renamed";
        public const string DESCRIPTION = "Triggered when you rename a ship";
        public const string SAMPLE = @"{""timestamp"":""2016-09-20T18:14:26Z"",""event"":""SetUserShipName"",""Ship"":""federation_corvette"",""ShipID"":1,""UserShipName"":""Shieldless wonder"",""UserShipId"":""NCC-1701""}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipRenamedEvent()
        {
            VARIABLES.Add("ship", "The model of the ship that was renamed");
            VARIABLES.Add("shipid", "The ID of the ship that was renamed");
            VARIABLES.Add("name", "The new name of the ship");
            VARIABLES.Add("ident", "The new ident of the ship");
        }

        public string ship => shipDefinition?.model;

        public int? shipid { get; private set; }

        public string name { get; private set; }

        public string ident { get; private set; }

        // Not intended to be user facing
        [VoiceAttackIgnore]
        public Ship shipDefinition => ShipDefinitions.FromEDModel(edModel);

        [VoiceAttackIgnore]
        public string edModel { get; private set; }

        public ShipRenamedEvent(DateTime timestamp, string ship, int shipid, string name, string ident) : base(timestamp, NAME)
        {
            this.edModel = ship;
            this.shipid = shipid;
            this.name = name;
            this.ident = ident;
        }
    }
}

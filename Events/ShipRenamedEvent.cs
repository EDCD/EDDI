using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class ShipRenamedEvent : Event
    {
        public const string NAME = "Ship renamed";
        public const string DESCRIPTION = "Triggered when you rename a ship";
        public const string SAMPLE = @"{""timestamp"":""2016-09-20T18:14:26Z"",""event"":""SetUserShipName"",""Ship"":""federation_corvette"",""ShipID"":1,""UserShipName"":""Shieldless wonder"",""UserShipId"":""NCC 1701""}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipRenamedEvent()
        {
            VARIABLES.Add("ship", "The ship that was renamed");
            VARIABLES.Add("name", "The new name of the ship");
            VARIABLES.Add("ident", "The new ident of the ship");
        }

        [JsonProperty("ship")]
        public string ship { get; private set; }

        [JsonIgnore]
        public Ship Ship { get; private set; }

        [JsonProperty("name")]
        public string name { get; private set; }

        [JsonProperty("ident")]
        public string ident { get; private set; }

        public ShipRenamedEvent(DateTime timestamp, Ship ship) : base(timestamp, NAME)
        {
            this.Ship = ship;
            this.ship = (ship == null ? null : ship.model);
            this.name = (ship == null ? null : ship.name);
            this.ident = (ship == null ? null : ship.ident);
        }
    }
}

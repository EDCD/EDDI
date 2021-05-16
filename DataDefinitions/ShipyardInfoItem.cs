using Newtonsoft.Json;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary> This class is designed to be deserialized from either shipyard.json or the Frontier API. </summary>
    public class ShipyardInfoItem
    {
        [JsonProperty("id")]
        public long EliteID { get; set; }

        [JsonProperty("ShipType")]
        public string edModel { get; set; }

        // The Frontier API uses `name` rather than `ShipType`, we normalize that here.
        [JsonProperty] // As a private property, it shall not be serialized.
        protected string name { set => edModel = value; }

        // Station prices
        [JsonProperty("ShipPrice")]
        public long shipPrice { get; set; }

        // The Frontier API uses `basevalue` rather than `ShipPrice`, we normalize that here.
        [JsonProperty] // As a private property, it shall not be serialized.
        private protected long basevalue { set => shipPrice = value; }

        public ShipyardInfoItem()
        { }

        public ShipyardInfoItem(long eliteId, string ShipType, long ShipPrice)
        {
            this.EliteID = eliteId;
            this.edModel = ShipType;
            this.shipPrice = ShipPrice;
        }

        public Ship ToShip()
        {
            Ship ship = ShipDefinitions.FromEliteID(EliteID) ?? ShipDefinitions.FromEDModel(edModel, false);
            if (ship == null)
            {
                // Unknown ship; report the full object so that we can update the definitions 
                Logging.Info("Ship definition error: " + edModel, JsonConvert.SerializeObject(this));

                // Create a basic ship definition & supplement from the info available 
                ship = new Ship
                {
                    EDName = edModel
                };
            }
            ship.value = shipPrice;
            return ship;
        }
    }
}
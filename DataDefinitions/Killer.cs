using Newtonsoft.Json;
using Utilities;

namespace EddiDataDefinitions
{
    // For use with the `Died` event
    public class Killer
    {
        [PublicAPI]
        public string name { get; }

        [PublicAPI]
        public string rating => combatRating?.localizedName;

        [PublicAPI]
        public string equipment => killerShip?.SpokenModel()
            ?? killerCmdrSuit?.localizedName
            ?? killerVehicle?.localizedName
            ?? killerNpcSuitLoadout?.localizedName
            ;

        // Not intended to be user facing

        [JsonIgnore]
        private CombatRating combatRating { get; }

        [JsonIgnore]
        private Ship killerShip { get; }

        [JsonIgnore]
        private VehicleDefinition killerVehicle { get; }

        [JsonIgnore]
        private NpcSuitLoadout killerNpcSuitLoadout { get; }

        [JsonIgnore]
        private Suit killerCmdrSuit { get; }

        public Killer(string edName, string edModel, CombatRating rating)
        {
            this.name = edName;
            this.combatRating = rating;

            // Might be a ship
            killerShip = ShipDefinitions.FromEDModel(edModel, false);

            // Might be a SRV or Fighter
            killerVehicle = VehicleDefinition.EDNameExists(edModel) ? VehicleDefinition.FromEDName(edModel) : null;

            // Might be an on foot commander
            killerCmdrSuit = Suit.EDNameExists(edModel) ? Suit.FromEDName(edModel) : null;

            // Might be an on foot NPC
            killerNpcSuitLoadout = NpcSuitLoadout.EDNameExists(edModel) ? NpcSuitLoadout.FromEDName(edModel) : null;
        }
    }
}

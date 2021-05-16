using System;
using System.Linq;
using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    // For use with the `Died` event
    public class Killer
    {
        public string name { get; }

        public string rating => combatRating.localizedName;

        public string equipment => killerShip?.SpokenModel()
            ?? killerCmdrSuit?.localizedName
            ?? killerVehicle?.localizedName
            ?? killerNpcSuitLoadout?.localizedName
            ;

        // Not intended to be user facing

        [JsonIgnore]
        public CombatRating combatRating { get; }

        [JsonIgnore]
        public Ship killerShip { get; }

        [JsonIgnore]
        public VehicleDefinition killerVehicle { get; }

        [JsonIgnore]
        public NpcSuitLoadout killerNpcSuitLoadout { get; }

        [JsonIgnore]
        public Suit killerCmdrSuit { get; }

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

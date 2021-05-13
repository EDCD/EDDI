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
            if (VehicleDefinition.AllOfThem.Any(v => string.Equals(v.edname, edModel, StringComparison.InvariantCultureIgnoreCase)))
            {
                killerVehicle = VehicleDefinition.FromEDName(edModel);
            }

            // Might be an on foot commander
            if (Suit.AllOfThem.Any(s => string.Equals(s.edname, edModel, StringComparison.InvariantCultureIgnoreCase)))
            {
                killerCmdrSuit = Suit.FromEDName(edModel);
            }

            // Might be an on foot NPC
            if (NpcSuitLoadout.AllOfThem.Any(s => string.Equals(s.edname, edModel, StringComparison.InvariantCultureIgnoreCase)))
            {
                killerNpcSuitLoadout = NpcSuitLoadout.FromEDName(edModel);
            }
        }
    }
}

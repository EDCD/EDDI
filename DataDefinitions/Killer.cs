using Newtonsoft.Json;
using Utilities;

namespace EddiDataDefinitions
{
    // For use with the `Died` event
    public class Killer
    {
        [PublicAPI("the killer's name")]
        public string name { get; }

        [PublicAPI("the killer's localized combat rating")]
        public string rating => combatRating?.localizedName;

        [PublicAPI("the killer's equipment")]
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
        private VesselDefinition killerVehicle { get; }

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
            if ( killerShip != null ) { return; }

            // Might be a SRV or Fighter
            killerVehicle = VesselDefinition.isVessel(edModel) ? VesselDefinition.FromEDName(edModel) : null;
            if ( killerVehicle != null ) { return; }

            // Might be an on foot commander
            killerCmdrSuit = Suit.EDNameExists(edModel) ? Suit.FromEDName(edModel) : null;
            if ( killerCmdrSuit != null ) { return; }

            // Might be an on foot NPC
            killerNpcSuitLoadout = NpcSuitLoadout.EDNameExists(edModel) ? NpcSuitLoadout.FromEDName(edModel) : null;
            if ( killerNpcSuitLoadout != null ) { return; }
        }
    }
}

using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipInterdictionEvent : Event
    {
        public const string NAME = "Ship interdiction";
        public const string DESCRIPTION = "Triggered when you interdict another ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-09-21T07:00:17Z\",\"event\":\"Interdiction\",\"Success\":true,\"Interdicted\":\"Torval's Shield\",\"IsPlayer\":false,\"Faction\":\"Zemina Torval\",\"Power\":\"Empire\"}";

        [PublicAPI("If the interdiction attempt was successful")]
        public bool succeeded { get; private set; }

        [PublicAPI("If the player being interdicted is a commander (as opposed to an NPC)")]
        public bool iscommander { get; private set; }

        [PublicAPI("The name of the commander being interdicted")]
        public string interdictee { get; private set; }

        [PublicAPI("The combat rating of the commander being interdicted")]
        public string rating { get; private set; }

        [PublicAPI("The faction of the commander being interdicted")]
        public string faction { get; private set; }

        [PublicAPI("The power of the commander being interdicted")]
        public string power { get; private set; }
        
        public ShipInterdictionEvent(DateTime timestamp, bool succeeded, bool iscommander, string interdictee, CombatRating rating, string faction, string power) : base(timestamp, NAME)
        {
            this.succeeded = succeeded;
            this.iscommander = iscommander;
            this.interdictee = interdictee;
            this.rating = rating?.localizedName;
            this.faction = faction;
            this.power = power;
        }
    }
}

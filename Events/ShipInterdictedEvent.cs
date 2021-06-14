using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipInterdictedEvent : Event
    {
        public const string NAME = "Ship interdicted";
        public const string DESCRIPTION = "Triggered when your ship is interdicted by another ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-09-21T07:00:17Z\",\"event\":\"Interdicted\",\"Submitted\":true,\"Interdictor\":\"Torval's Shield\",\"IsPlayer\":false,\"Faction\":\"Zemina Torval\",\"Power\":\"Empire\"}";

        [PublicAPI("If the interdiction attempt was successful")]
        public bool succeeded { get; private set; }

        [PublicAPI("If the commander submitted to the interdiction")]
        public bool submitted { get; private set; }

        [PublicAPI("If the player carrying out the interdiction is a commander (as opposed to an NPC)")]
        public bool iscommander { get; private set; }

        [PublicAPI("The name of the commander or NPC carrying out the interdiction")]
        public string interdictor { get; private set; }

        [PublicAPI("The combat rating of the commander or NPC carrying out the interdiction")]
        public string rating { get; private set; }

        [PublicAPI("The faction of the NPC carrying out the interdiction")]
        public string faction { get; private set; }

        [PublicAPI("The power of the NPC carrying out the interdiction")]
        public string power { get; private set; }

        public ShipInterdictedEvent(DateTime timestamp, bool succeeded, bool submitted, bool iscommander, string interdictor, CombatRating rating, string faction, string power) : base(timestamp, NAME)
        {
            this.succeeded = succeeded;
            this.submitted = submitted;
            this.iscommander = iscommander;
            this.interdictor = interdictor;
            this.rating = rating?.localizedName;
            this.faction = faction;
            this.power = power;
        }
    }
}

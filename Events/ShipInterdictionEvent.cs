using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class ShipInterdictionEvent : Event
    {
        public const string NAME = "Ship interdiction";
        public const string DESCRIPTION = "Triggered when you interdict another ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-09-21T07:00:17Z\",\"event\":\"Interdiction\",\"Success\":true,\"Interdicted\":\"Torval's Shield\",\"IsPlayer\":false,\"Faction\":\"Zemina Torval\",\"Power\":\"Empire\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipInterdictionEvent()
        {
            VARIABLES.Add("succeeded", "If the interdiction attempt was successful");
            VARIABLES.Add("iscommander", "If the player being interdicted is a commander (as opposed to an NPC)");
            VARIABLES.Add("interdictee", "The name of the commander being interdicted");
            VARIABLES.Add("rating", "The combat rating of the commander being interdicted");
            VARIABLES.Add("faction", "The faction of the commander being interdicted");
            VARIABLES.Add("power", "The power of the commander being interdicted");
        }

        [PublicAPI]
        public bool succeeded { get; private set; }

        [PublicAPI]
        public bool iscommander { get; private set; }

        [PublicAPI]
        public string interdictee { get; private set; }

        [PublicAPI]
        public string rating { get; private set; }

        [PublicAPI]
        public string faction { get; private set; }

        [PublicAPI]
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

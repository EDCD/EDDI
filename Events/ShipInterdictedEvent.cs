﻿using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class ShipInterdictedEvent : Event
    {
        public const string NAME = "Ship interdicted";
        public const string DESCRIPTION = "Triggered when your ship is interdicted by another ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-09-21T07:00:17Z\",\"event\":\"Interdicted\",\"Submitted\":true,\"Interdictor\":\"Torval's Shield\",\"IsPlayer\":false,\"Faction\":\"Zemina Torval\",\"Power\":\"Empire\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipInterdictedEvent()
        {
            VARIABLES.Add("succeeded", "If the interdiction attempt was successful");
            VARIABLES.Add("submitted", "If the commander submitted to the interdiction");
            VARIABLES.Add("iscommander", "If the player carrying out the interdiction is a commander (as opposed to an NPC)");
            VARIABLES.Add("interdictor", "The name of the commander or NPC carrying out the interdiction");
            VARIABLES.Add("rating", "The combat rating of the commander or NPC carrying out the interdiction");
            VARIABLES.Add("faction", "The faction of the NPC carrying out the interdiction");
            VARIABLES.Add("power", "The power of the NPC carrying out the interdiction");
        }

        [PublicAPI]
        public bool succeeded { get; private set; }

        [PublicAPI]
        public bool submitted { get; private set; }

        [PublicAPI]
        public bool iscommander { get; private set; }

        [PublicAPI]
        public string interdictor { get; private set; }

        [PublicAPI]
        public string rating { get; private set; }

        [PublicAPI]
        public string faction { get; private set; }

        [PublicAPI]
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
